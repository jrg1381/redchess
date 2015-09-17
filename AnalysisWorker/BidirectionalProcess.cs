using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace Redchess.AnalysisWorker
{
    internal class BidirectionalProcess : IDisposable
    {
        private const int c_processReadyTimeoutInSeconds = 10;

        private readonly StringBuilder m_builder;
        private readonly Process m_process;
        private readonly AutoResetEvent m_waitForEvent;
        private string m_triggerText;
        private bool m_isReady;
        private readonly object m_builderLock = new object();
        private readonly TimeSpan m_processReadyTimeout;
        private readonly string m_processReadyText;

        internal BidirectionalProcess(string exePath, string processReadyText, int processReadyTimeoutSeconds = c_processReadyTimeoutInSeconds)
        {
            m_processReadyTimeout = TimeSpan.FromSeconds(processReadyTimeoutSeconds);
            m_processReadyText = processReadyText;

            if (!File.Exists(exePath))
            {
                throw new FileNotFoundException("Executable not found " + exePath, Path.GetFullPath(exePath));
            }

            var processStartInfo = new ProcessStartInfo
            {
                FileName = exePath,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
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

            Debug.WriteLine("Started external process " + m_process.Id);
        }

        private void ProcessOnExited(object sender, EventArgs eventArgs)
        {
            m_isReady = false;
            Dispose();
        }

        /// <summary>
        /// Wait m_processReadyTimeout seconds for the process to be ready. Readiness is indicated by
        /// the presence of m_processReadyText in the output.
        /// <exception cref="TimeoutException"></exception>
        /// </summary>
        internal void WaitForReady()
        {
            if (m_isReady) return;

            var expiryTime = DateTime.UtcNow + m_processReadyTimeout;
            while (!m_builder.ToString().Contains(m_processReadyText) && DateTime.UtcNow < expiryTime)
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

        internal string Write(string command, string completionTrigger = null)
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

        internal void Close()
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