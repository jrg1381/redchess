using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;

namespace Redchess.AnalysisWorker
{
    static class EngineDownloader
    {
        private static readonly object DownloaderLock = new object();
        private static readonly byte[] ExpectedHash = {0x41,0xf3,0x8e,0xb5,0xa8,0xae,0x98,0x59,0x6a,0x63,0x9f,0x84,0x11,0x36,0x49,0x0b,0xf9,0xe4,0x4c,0xcc};
        private const string c_DownloadUrl = "https://www.dropbox.com/s/e2jxxydxbnzqf4b/stockfish-6-64.exe?raw=1";

        public static string DownloadEngine()
        {
            Trace.WriteLine("Testing whether engine needs to be downloaded");
            var targetFileName = new Uri(c_DownloadUrl).GetComponents(UriComponents.Path, UriFormat.Unescaped);
            var targetDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string target = Path.Combine(targetDirectory, Path.GetFileName(targetFileName));

            // Only one thread is allowed to download the engine at a time
            lock (DownloaderLock)
            {
                if (!File.Exists(target))
                {
                    Trace.WriteLine("Downloading engine");
                    using (var client = new WebClient())
                    {
                        client.DownloadFile(c_DownloadUrl, target);
                    }
                    Trace.WriteLine("Engine downloaded to " + target);

                    byte[] engineHash;
                    using (var stream = new FileStream(target, FileMode.Open, FileAccess.Read))
                    {
                        engineHash = SHA1.Create().ComputeHash(stream);
                    }

                    if (!engineHash.SequenceEqual(ExpectedHash))
                    {
                        File.Delete(target);
                        Trace.WriteLine("Engine checksum invalid");
                        throw new InvalidOperationException("Engine checksum invalid");
                    }

                    Trace.WriteLine("Successfully downloaded the engine");
                }
            }

            return target;
        }
    }
}