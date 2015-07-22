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
                Arguments = string.Format("/path:\"{0}\" /port:{1}", @"H:\PersonalRepo\Chess\Chess", port)
            };

            var programfiles = string.IsNullOrEmpty(m_processStartInfo.EnvironmentVariables["programfiles"])
                ? m_processStartInfo.EnvironmentVariables["programfiles(x86)"]
                : m_processStartInfo.EnvironmentVariables["programfiles"];

            m_processStartInfo.FileName = Path.Combine(programfiles, @"IIS Express\iisexpress.exe");
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
