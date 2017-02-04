using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace RedChess.WebTests
{
    class IisExpressStarter
    {
        private Process m_IisProcess;
        private readonly ProcessStartInfo m_ProcessStartInfo;

        public IisExpressStarter(int port = 60898)
        {
            m_ProcessStartInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Normal,
                ErrorDialog = true,
                LoadUserProfile = true,
                CreateNoWindow = false,
                UseShellExecute = false,
                Arguments = $"/path:\"{PathToWebsite()}\" /port:{port}"
            };

            var programfiles = string.IsNullOrEmpty(m_ProcessStartInfo.EnvironmentVariables["programfiles"])
                ? m_ProcessStartInfo.EnvironmentVariables["programfiles(x86)"]
                : m_ProcessStartInfo.EnvironmentVariables["programfiles"];

            m_ProcessStartInfo.FileName = Path.Combine(programfiles, @"IIS Express\iisexpress.exe");
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
                    m_IisProcess = new Process {StartInfo = m_ProcessStartInfo};
                    m_IisProcess.Start();
                    m_IisProcess.WaitForExit();
                });
            }
            catch
            {
                m_IisProcess.CloseMainWindow();
                m_IisProcess.Dispose();
            }
        }

        public void Stop()
        {
            try
            {
                m_IisProcess.CloseMainWindow();
                bool hasExited = m_IisProcess.WaitForExit(1000);
                if(!hasExited)
                    m_IisProcess.Kill();
            }
            catch
            {

            }
        }
    }
}
