using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityTools.Common
{
    public class RunCommand
    {
        public delegate void Output(string message);
        public delegate void Exit(string cmd, string args, int exitCode);

        public class CommandInfo
        {
            public string exe = null;
            public string args = null;
            public string workingDirectory = null;
            public Output output = null;
            public Exit exit = null;
            public bool async = true;
            public bool enableLogging = true;
            public bool rawOutput = true;

            internal StreamWriter log = null;
            internal Process process = null;
        }

        static public void RunCommandInProcess(CommandInfo command)
        {
            UnityEngine.Debug.Assert(command != null, "Command is null");

            if (string.IsNullOrEmpty(command.workingDirectory) == false)
            {
                if (Directory.Exists(command.workingDirectory) == false)
                {
                    UnityEngine.Debug.Assert(false, "workingDirectory is invalid" + command.workingDirectory);
                }
            }

            if (command.enableLogging)
            {
                var path = command.workingDirectory + "/" + command.exe + string.Format("_{0:yyyyMMddHmmss}", DateTime.Now) +".log";
                command.log = new StreamWriter(path);
                command.log.WriteLine(string.Format("[{2}][INFO]Command {0} {1} Start Running", command.exe, command.args, DateTime.Now.ToString()));
            }            

            var proc = new Process();
            proc.StartInfo.FileName = command.exe;
            proc.StartInfo.Arguments = command.args;
            proc.StartInfo.UseShellExecute = false;//to redirect output
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.StandardOutputEncoding = Encoding.GetEncoding(932);//932 is from cmd chcp, default will output invalid Japanese words
            proc.StartInfo.StandardErrorEncoding = Encoding.GetEncoding(932);
            proc.StartInfo.WorkingDirectory = command.workingDirectory;
            
            //proc.EnableRaisingEvents = true;//for ExitDataHandler


            proc.OutputDataReceived += (s, ea) => { if (ea.Data != null) OutputDataHandler(command, ea.Data); };
            proc.ErrorDataReceived  += (s, ea) => { if (ea.Data != null) ErrorDataHandler (command, ea.Data); };

            //proc.Exited += (s, ea) => ExitDataHandler(command, proc.ExitCode);

            command.process = proc;

            if (command.async)
            {
                StartProcessAsync(command);
            }
            else
            {
                StartProcess(command);
            }
        }
        static private void OutputDataHandler(CommandInfo command, string output)
        {
			var message = command.rawOutput ? output : "[" + DateTime.Now.ToString() + "][OUTPUT]" + output;

            command.output?.Invoke(message);

            if (command.enableLogging)
            {
                command.log.WriteLine(message);
            }
        }
        static private void ErrorDataHandler(CommandInfo command, string error)
        {
			var message = command.rawOutput ? error : "[" + DateTime.Now.ToString() + "][OUTPUT]" + error;

            command.output?.Invoke(message);

            if (command.enableLogging)
            {
                command.log.WriteLine(message);
            }
        }
        static private void ExitDataHandler(CommandInfo command, int code)
        {
            var message = string.Format("[{2}][INFO]Command {0} {1} End Running with Code {3}", command.exe, command.args, DateTime.Now.ToString(), code);

            if(!command.rawOutput) command.exit?.Invoke(command.exe, command.args, code);

            if(command.enableLogging)
            {
                command.log.WriteLine(message);
                command.log.Flush();
                command.log.Close();
                command.log = null;
            }
            
            command.process.Close();
            command.process = null;
        }
        static private int StartProcess(CommandInfo command)
        {
            try
            {
                bool started = command.process.Start();
                if (!started)
                {
                    //you may allow for the process to be re-used (started = false) 
                    //but I'm not sure about the guarantees of the Exited event in such a case
                    throw new InvalidOperationException("Could not start process: " + command.process);
                }

                command.process.BeginOutputReadLine();
                command.process.BeginErrorReadLine();

                command.process.WaitForExit();
            }
            catch (Exception e)
            {
                var message = string.Format("[{2}][ERROR]Exception {0}: {1}", e.GetType(), e.Message, DateTime.Now.ToString());
                command.output?.Invoke(message);
                if(command.enableLogging)
                {
                    command.log.WriteLine(message);
                }
            }
            finally
            {
                ExitDataHandler(command, command.process.ExitCode);
            }
            return 0;
        }

        static private async void StartProcessAsync(CommandInfo command)
        {
            await Task.Run<int>(() =>
            {
                return StartProcess(command);
            });
        }
    }

}