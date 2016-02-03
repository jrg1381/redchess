<#
    .SYNOPSIS 
    Deploys an Azure Web / Worker role package to Azure

    .DESCRIPTION
    This is script is used to deploy a compiled .cspkg file to run on Azure.

    The package can be deployed to either the Production or Staging environments as defined by the -Slot parameter (Staging is the default).

    By specifying the -SwapVips switch, the virtual IP is switched form Production to Staging. Note: specifying both -SwapVips and -Slot Production means that the new deployment ends up running in Staging!

    .PARAMETER ServiceName
    Specifies the Windows Azure service name of the deployment.

    .PARAMETER StorageAccount
    The Azure storage account to use.

    .PARAMETER Label
    Specifies a new label for the deployment.

    .PARAMETER PublishSettings
    Specifies the full path and filename for the .publishsettings file for the Windows Azure account.

    .PARAMETER Package
    Specifies the full path to the upgrade package (.cspkg) file.

    .PARAMETER Configuration
    Specifies the path to the new configuration (.cscfg) file when performing an upgrade.

    .PARAMETER SwapVips
    If specified, after deployment swap the virtual IPs between the product and staging environments.

    .PARAMETER Slot
    Specifies the environment of the deployment to modify. Supported values are "Production" or "Staging".

    .EXAMPLE

    .\deploy.ps1 -ServiceName <name> -StorageAccount <account> -Label <label> -Package <file> -Configuration <file> -SwapVips -Slot Staging

    Deploy the package to Staging, and then swap the virtual IP to point to the new deployment.

    .EXAMPLE

    .\deploy.ps1 -ServiceName <name> -StorageAccount <account> -Label <label> -Package <file> -Configuration <file> -Slot Production

    Deploy the package to Production.

    .LINK
    Import-AzurePublishSettingsFile http://msdn.microsoft.com/en-us/library/windowsazure/jj152885.aspx
    Set-AzureSubscription http://msdn.microsoft.com/en-us/library/windowsazure/jj152906.aspx
    Set-AzureDeployment http://msdn.microsoft.com/en-us/library/windowsazure/jj152836.aspx
#>

# Validation
[CmdletBinding()]
param(
    [string]$ServiceName = $(throw "-ServiceName is required."),
    [string]$StorageAccount = $(throw "-StorageAccount is required."),
    [string]$Label = $(throw "-Label is required."),
    [string]$Package = $(throw "-Package is required."),
    [string]$Configuration = $(throw "-Configuration is required."),
    [switch]$SwapVips = $false,
    [ValidateSet("Production","Staging")]
    [string]$Slot = "Staging",
    [string]$PublishSettings = "",
    [string]$CertificateThumbprint = "",
    [string]$SubscriptionId = "",
    [string]$SubscriptionName = ""
)

Write-Host "ServiceName: $ServiceName"
Write-Host "StorageAccount: $StorageAccount"
Write-Host "Label: $Label"
Write-Host "Package: $Package"
Write-Host "Configuration: $Configuration"
Write-Host "SwapVips: $SwapVips"
Write-Host "Slot: $Slot"
Write-Host ""
Write-Host "PublishSettings: $PublishSettings"
Write-Host "CertificateThumbprint: $CertificateThumbprint"
Write-Host "SubscriptionId: $SubscriptionId"
Write-Host "SubscriptionName: $SubscriptionName"

# Check that either a publish settings file or certificate reference have been passed in.
$useCertificate = [String]::IsNullOrEmpty($PublishSettings)

if($useCertificate) {
  if(([String]::IsNullOrEmpty($SubscriptionId) -or [String]::IsNullOrEmpty($CertificateThumbprint)) -or [String]::IsNullOrEmpty($SubscriptionName)) {
    throw "Either '-PublishSettings' or ('-CertificateThumbprint' and '-SubscriptionId' and '-SubscriptionName') are required."
  }
}

try {
    $ErrorActionPreference = "Stop"
    $scriptdir = $MyInvocation.MyCommand.Path | Split-Path
    
    $SubscriptionDataFile = "$scriptdir\SubscriptionData.xml"

    # set up azure credentials
    
    if($useCertificate) {
      $CertificatePath = "cert:\\CurrentUser\My\" + $CertificateThumbprint
      $Certificate = Get-Item $CertificatePath
      Set-AzureSubscription -SubscriptionName $SubscriptionName -SubscriptionId $SubscriptionId -Certificate $Certificate -CurrentStorageAccountName $StorageAccount
      Select-AzureSubscription -SubscriptionName $SubscriptionName
    } 
    else
    {
          Write-Host "Reading Azure publish settings"
          Import-AzurePublishSettingsFile -PublishSettingsFile $PublishSettings -SubscriptionDataFile $SubscriptionDataFile
          # set the storage account to use
          $currentSub = Get-AzureSubscription -SubscriptionDataFile $SubscriptionDataFile
          Set-AzureSubscription -SubscriptionName $currentSub.SubscriptionName `
        -CurrentStorageAccount $StorageAccount `
        -SubscriptionId $currentSub.SubscriptionId `
        -Certificate $currentSub.Certificate
    }
    
    # If the deployment doesn't exist, create a new one
    try {
      $existingDeployment = Get-AzureDeployment -ServiceName $ServiceName -Slot $Slot    
      $isNewDeploymenty = $FALSE
    } catch [Exception]
    {
      $isNewDeploymenty = $TRUE
    }
    
    if($isNewDeploymenty) {
      Write-Host "No existing deployment. Creating a new one..."
      New-AzureDeployment `
          -ServiceName $ServiceName `
          -Slot $Slot `
          -Label $Label `
          -Package $Package `
          -Configuration $Configuration
    } else {
      # create the new deployment
      Write-Host "Upgrade existing deployment..."
      Set-AzureDeployment `
          -Upgrade `
          -Force `
          -ServiceName $ServiceName `
          -Slot $Slot `
          -Label $Label `
          -Package $Package `
          -Configuration $Configuration
    }
    
    $storageContext = New-AzureStorageContext -StorageAccountName $StorageAccount -StorageAccountKey $ENV:StorageAccountKey
	Set-AzureServiceDiagnosticsExtension -StorageContext $storageContext -DiagnosticsConfigurationPath .\RedchessService\AnalysisWorkerContent\diagnostics.wadcfgx -ServiceName $ServiceName -Slot $Slot

    if ($SwapVips -eq $true) {
        Write-Host "swapping VIPs..."
        # this seems to regularly fail with a 500 error. Let's try retrying.
        $done = 0
        $i = 0
        while (! $done) {
            try {
                Move-AzureDeployment -ServiceName $ServiceName
                $done = 1
            } catch [Exception] {
                $e = $error[0]
                Write-Host "VIP swap failed: " $e
                
                $ex = $e.Exception
                if($i++ -lt 10 -and
                   ($ex.GetType().FullName -eq "System.ServiceModel.CommunicationException" -or
                    ($ex.GetType().FullName -eq "System.Exception" -and 
                        $ex.Message -eq "Failed: The server encountered an internal error. Please retry the request."))) {
                    Write-Host "retrying..."
                } else {
                    throw
                }
            }
        }
    }
}
catch [Exception]
{
    Write-Host "Exception: (" $error[0].Exception.GetType().FullName "): " $error[0]
    Exit 1
}