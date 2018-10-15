using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace UnityTools.Common
{
    public class RunCommand : SystemSingleton<RunCommand>
    {
        public delegate void Output(string message);
        public delegate void Exit(string cmd, string args, int exitCode);

        public void RunProcess(string exe, string args, string path, Output output, Exit exit, bool async = true)
        {
            var proc = new Process();
            proc.StartInfo.FileName = exe;
            proc.StartInfo.Arguments = args;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;
            proc.StartInfo.Environment["Path"] += ";" + path;

            proc.EnableRaisingEvents = true;

            if (output != null)
            {
                proc.OutputDataReceived += (s, ea) => output(ea.Data);
                proc.ErrorDataReceived += (s, ea) => { if (ea.Data != null) output("ERR: " + ea.Data); };
            }

            if (exit != null)
            {
                proc.Exited += (s, ea) => exit(exe, args, proc.ExitCode);

            }

            if (async)
            {
                RunProcessAsync(proc);
            }
            else
            {
                RunProcess(proc);
            }
        }
        private int RunProcess(Process process)
        {
            bool started = process.Start();
            if (!started)
            {
                //you may allow for the process to be re-used (started = false) 
                //but I'm not sure about the guarantees of the Exited event in such a case
                throw new InvalidOperationException("Could not start process: " + process);
            }

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            process.WaitForExit();
            process.Close();

            return 0;
        }

        private async void RunProcessAsync(Process process)
        {
            await Task.Run<int>(() =>
            {
                return this.RunProcess(process);
            });
        }
    }

}