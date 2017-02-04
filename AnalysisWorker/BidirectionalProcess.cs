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
        private const int c_ProcessReadyTimeoutInSeconds = 10;

        private readonly StringBuilder m_Builder;
        private readonly Process m_Process;
        private readonly AutoResetEvent m_WaitForEvent;
        private string m_TriggerText;
        private bool m_IsReady;
        private readonly object m_BuilderLock = new object();
        private readonly TimeSpan m_ProcessReadyTimeout;
        private string m_CapturedText;

        internal BidirectionalProcess(string exePath, string processReadyText,
            int processReadyTimeoutSeconds = c_ProcessReadyTimeoutInSeconds)
        {
            m_ProcessReadyTimeout = TimeSpan.FromSeconds(processReadyTimeoutSeconds);
            m_TriggerText = processReadyText; // initially wait for this

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

            m_Builder = new StringBuilder();
            m_WaitForEvent = new AutoResetEvent(false); // Blocking on startup
            m_IsReady = false;

            m_Process = new Process
            {
                StartInfo = processStartInfo,
                EnableRaisingEvents = true
            };

            m_Process.Exited += ProcessOnExited;
            m_Process.Start();

            Trace.WriteLine("Started external process " + m_Process.Id);

            Task.Run(() =>
            {
                // Not trying to be efficient here. Read characters one by one to avoid problems with buffering.
                var buffer = new char[1];
                try
                {
                    while (m_Process.StandardOutput.Read(buffer, 0, 1) > -1)
                    {
                        lock (m_BuilderLock)
                        {
                            m_Builder.Append(buffer, 0, 1);
                            var searchString = m_Builder.ToString();
                            if (MatchText(searchString))
                            {
                                // Once we see what we're looking for, stash it in m_capturedText
                                m_CapturedText = searchString;
                                m_Builder.Clear();
                                // And tell the thread which is waiting for us
                                m_WaitForEvent.Set();
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

            if (m_TriggerText == null)
                return true;

            var indexOfSearchText = textToSearch.IndexOf(m_TriggerText, StringComparison.Ordinal);

            if (indexOfSearchText == -1)
                return false;

            var indexOfNewlineAfterSearchText = textToSearch.IndexOf("\r\n", indexOfSearchText, StringComparison.Ordinal);

            return (indexOfNewlineAfterSearchText != -1);
        }

        private void ProcessOnExited(object sender, EventArgs eventArgs)
        {
            m_IsReady = false;
            Dispose();
        }

        /// <summary>
        /// Wait m_processReadyTimeout seconds for the process to be ready. Readiness is indicated by
        /// the presence of m_processReadyText in the output.
        /// <exception cref="TimeoutException"></exception>
        /// </summary>
        internal void WaitForReady()
        {
            if (m_IsReady) return;

            m_IsReady = m_WaitForEvent.WaitOne(m_ProcessReadyTimeout);

            if (m_IsReady)
            {
                Trace.TraceError("Process was ready");
                return;
            }

            Trace.TraceError("TimeoutException waiting for external process");
            Trace.TraceInformation("Data captured so far = " + m_Builder);
            throw new TimeoutException("Process was not ready for communication");
        }

        internal string Write(string command, string completionTrigger = null)
        {
            if (!m_IsReady)
            {
                Trace.TraceError("Attempted to write to an unready process");
                throw new InvalidOperationException("Process is not ready");
            }

            m_TriggerText = completionTrigger;
            m_WaitForEvent.Reset();

            lock (m_BuilderLock)
            {
                // Clear anything that came after the last interaction
                m_Builder.Clear();
            }

            Trace.WriteLine("Writing "+ command + " to the process");
            m_Process.StandardInput.WriteLine(command);
            m_Process.StandardInput.Flush();

            if (completionTrigger != null)
            {
                Trace.WriteLine("Waiting for " + completionTrigger);
                var success = m_WaitForEvent.WaitOne(TimeSpan.FromSeconds(30));
                if (!success)
                {
                    Trace.TraceError("Timed out waiting for response from engine");
                    Trace.TraceInformation("Data captured so far = " + m_Builder);
                    throw new TimeoutException("Timed out on external process");
                }
            }

            return m_CapturedText;
        }

        internal void Close()
        {
            m_IsReady = false;
            m_Process.Kill();
            Dispose();
        }

        public void Dispose()
        {
            if (m_Process != null)
            {
                m_Process.Dispose();
            }
            if (m_WaitForEvent != null)
            {
                m_WaitForEvent.Dispose();
            }
        }
    }
}