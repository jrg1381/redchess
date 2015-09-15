using System;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace Redchess.AnalysisWorker
{
    internal class BidirectionalProcess : IDisposable
    {
        private const string c_exePath = @"c:\windows\system32\cmd.exe";
        private const int c_processStartTimeoutInSeconds = 10;
        private const string c_processReadyString = "reserved";

        private readonly StringBuilder m_builder;
        private readonly Process m_process;
        private readonly AutoResetEvent m_waitForEvent;
        private string m_triggerText;
        private bool m_isReady;
        private readonly object m_builderLock = new object();

        public BidirectionalProcess()
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = c_exePath,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            m_builder = new StringBuilder();
            m_waitForEvent = new AutoResetEvent(true);
            m_isReady = false;

            m_process = new Process
            {
                StartInfo = processStartInfo,
                EnableRaisingEvents = true
            };

            m_process.OutputDataReceived += ProcessOnOutputDataReceived;
            m_process.Exited += ProcessOnExited;
            m_process.Start();
            m_process.BeginOutputReadLine();
        }

        private void ProcessOnExited(object sender, EventArgs eventArgs)
        {
            m_isReady = false;
            Dispose();
        }

        /// <summary>
        /// Wait c_processStartTimeoutInSeconds seconds for the process to be ready. Readiness is indicated by
        /// the presence of c_processReadyString in the output.
        /// <exception cref="TimeoutException"></exception>
        /// </summary>
        public void WaitForReady()
        {
            if (m_isReady) return;

            var expiryTime = DateTime.UtcNow.AddSeconds(c_processStartTimeoutInSeconds);
            while (!m_builder.ToString().Contains(c_processReadyString) && DateTime.UtcNow < expiryTime)
            {
                Thread.Sleep(100);
            }

            m_isReady = DateTime.UtcNow < expiryTime;
            if (m_isReady)
            {
                m_builder.Clear();
                return;
            }

            throw new TimeoutException("Process was not ready for communication");
        }

        private void ProcessOnOutputDataReceived(object sender, DataReceivedEventArgs dataReceivedEventArgs)
        {
            lock (m_builderLock)
            {
                m_builder.AppendLine(dataReceivedEventArgs.Data);
                if (m_triggerText != null && m_builder.ToString().Contains(m_triggerText))
                {
                    m_waitForEvent.Set();
                    m_triggerText = null;
                }
            }
        }

        public string Write(string command, string completionTrigger = null)
        {
            if (!m_isReady)
            {
                throw new InvalidOperationException("Process is not ready");
            }

            m_triggerText = completionTrigger;
            m_waitForEvent.Reset();

            m_process.StandardInput.WriteLine(command);

            if (completionTrigger != null)
            {
                m_waitForEvent.WaitOne();
            }

            var result = m_builder.ToString();
            m_builder.Clear();
            return result;
        }

        public void Close()
        {
            m_isReady = false;
            m_process.Kill();
            Dispose();
        }

        public void Dispose()
        {
            if (m_process != null)
            {
                m_process.Dispose();
            }
            if (m_waitForEvent != null)
            {
                m_waitForEvent.Dispose();
            }
        }
    }
}