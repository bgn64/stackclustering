using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace ClusteringPlugin.StackClustering
{
    internal class PythonStringClusterer : IStringClusterer
    {
        readonly string scriptPath;
        readonly string args;

        public PythonStringClusterer(string scriptPath, string args = "")
        {
            this.scriptPath = scriptPath;
            this.args = args;
        }

        public ClusterResult ClusterStrings(string[] strings)
        {
            var inputJson = JsonSerializer.Serialize(strings);
            var tempInputFile = Path.GetTempFileName();
            File.WriteAllText(tempInputFile, inputJson);
            Process p = new Process()
            {
                StartInfo = new ProcessStartInfo("python.exe", $"{scriptPath} {tempInputFile} {args}")
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            p.Start();
            string output = p.StandardOutput.ReadToEnd();
            string error = p.StandardError.ReadToEnd();
            p.WaitForExit();

            if (p.ExitCode != 0)
            {
                throw new Exception(error);
                throw new Exception(error);
            }

            ClusterResult? result = JsonSerializer.Deserialize<ClusterResult>(output);

            if (result == null)
            {
                throw new Exception();
            }

            return result;
        }
    }
}
