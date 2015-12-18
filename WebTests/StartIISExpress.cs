using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace WebTests
{
    class StartIisExpress
    {
        private Process m_iisProcess;
        private readonly ProcessStartInfo m_processStartInfo;

        public StartIisExpress(int port = 60898)
        {
            m_processStartInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Normal,
                ErrorDialog = true,
                LoadUserProfile = true,
                CreateNoWindow = false,
                UseShellExecute = false,
                Arguments = $"/path:\"{PathToWebsite()}\" /port:{port}"
            };

            var programfiles = string.IsNullOrEmpty(m_processStartInfo.EnvironmentVariables["programfiles"])
                ? m_processStartInfo.EnvironmentVariables["programfiles(x86)"]
                : m_processStartInfo.EnvironmentVariables["programfiles"];

            m_processStartInfo.FileName = Path.Combine(programfiles, @"IIS Express\iisexpress.exe");
        }

        private string PathToWebsite()
        {
            // This is written to disk by a pre-build action ready for us to pick up. 
            // We need to do this because the test assembly might be shadow-copied by a test runner to somewhere away from the original solution.
            var solutionDirectory = File.ReadAllText("SolutionDir.txt").TrimEnd('\r', '\n', ' ');
            return Path.Combine(solutionDirectory, "Chess");
        }

        public void Start()
        {
            try
            {
                Task.Factory.StartNew(() =>
                {
                    m_iisProcess = new Process {StartInfo = m_processStartInfo};
                    m_iisProcess.Start();
                    m_iisProcess.WaitForExit();
                });
            }
            catch
            {
                m_iisProcess.CloseMainWindow();
                m_iisProcess.Dispose();
            }
        }

        public void Stop()
        {
            try
            {
                m_iisProcess.CloseMainWindow();
                bool hasExited = m_iisProcess.WaitForExit(1000);
                if(!hasExited)
                    m_iisProcess.Kill();
            }
            catch
            {

            }
        }
    }
}
