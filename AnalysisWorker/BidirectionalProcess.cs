using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Redchess.AnalysisWorker
{
    /// <summary>
    /// Class representing a process which we can read and write to
    /// </summary>
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
        private string m_capturedText;

        internal BidirectionalProcess(string exePath, string processReadyText,
            int processReadyTimeoutSeconds = c_processReadyTimeoutInSeconds)
        {
            m_processReadyTimeout = TimeSpan.FromSeconds(processReadyTimeoutSeconds);
            m_triggerText = processReadyText; // initially wait for this

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
            m_waitForEvent = new AutoResetEvent(false); // Blocking on startup
            m_isReady = false;

            m_process = new Process
            {
                StartInfo = processStartInfo,
                EnableRaisingEvents = true
            };

            m_process.Exited += ProcessOnExited;
            m_process.Start();

            Trace.WriteLine("Started external process " + m_process.Id);

            Task.Run(() =>
            {
                // Not trying to be efficient here. Read characters one by one to avoid problems with buffering.
                var buffer = new char[1];
                try
                {
                    while (m_process.StandardOutput.Read(buffer, 0, 1) > -1)
                    {
                        lock (m_builderLock)
                        {
                            m_builder.Append(buffer, 0, 1);
                            var searchString = m_builder.ToString();
                            if (MatchText(searchString))
                            {
                                // Once we see what we're looking for, stash it in m_capturedText
                                m_capturedText = searchString;
                                m_builder.Clear();
                                // And tell the thread which is waiting for us
                                m_waitForEvent.Set();
                            }
                        }
                    }
                }
                catch(InvalidOperationException)
                {
                    //Process has gone away
                }
            });
        }

        private bool MatchText(string textToSearch)
        {
            // We consider it a match when we've seen a line containing m_triggerText and that line has been terminated with \r\n

            if (m_triggerText == null)
                return true;

            var indexOfSearchText = textToSearch.IndexOf(m_triggerText, StringComparison.Ordinal);

            if (indexOfSearchText == -1)
                return false;

            var indexOfNewlineAfterSearchText = textToSearch.IndexOf("\r\n", indexOfSearchText, StringComparison.Ordinal);

            return (indexOfNewlineAfterSearchText != -1);
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

            m_isReady = m_waitForEvent.WaitOne(m_processReadyTimeout);

            if (m_isReady)
            {
                Trace.TraceError("Process was ready");
                return;
            }

            Trace.TraceError("TimeoutException waiting for external process");
            Trace.TraceInformation("Data captured so far = " + m_builder);
            throw new TimeoutException("Process was not ready for communication");
        }

        internal string Write(string command, string completionTrigger = null)
        {
            if (!m_isReady)
            {
                Trace.TraceError("Attempted to write to an unready process");
                throw new InvalidOperationException("Process is not ready");
            }

            m_triggerText = completionTrigger;
            m_waitForEvent.Reset();

            lock (m_builderLock)
            {
                // Clear anything that came after the last interaction
                m_builder.Clear();
            }

            Trace.WriteLine("Writing "+ command + " to the process");
            m_process.StandardInput.WriteLine(command);
            m_process.StandardInput.Flush();

            if (completionTrigger != null)
            {
                Trace.WriteLine("Waiting for " + completionTrigger);
                var success = m_waitForEvent.WaitOne(TimeSpan.FromSeconds(30));
                if (!success)
                {
                    Trace.TraceError("Timed out waiting for response from engine");
                    Trace.TraceInformation("Data captured so far = " + m_builder);
                    throw new TimeoutException("Timed out on external process");
                }
            }

            return m_capturedText;
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