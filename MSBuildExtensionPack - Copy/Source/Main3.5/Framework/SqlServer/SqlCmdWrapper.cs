﻿//-----------------------------------------------------------------------
// <copyright file="SqlCmdWrapper.cs">(c) http://www.codeplex.com/MSBuildExtensionPack. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace MSBuild.ExtensionPack.SqlServer.Extended
{
    using System;
    using System.Collections.Specialized;
    using System.Diagnostics;

    internal sealed class SqlCmdWrapper
    {
        private readonly NameValueCollection environmentVars = new NameValueCollection();
        private readonly System.Text.StringBuilder stdOut = new System.Text.StringBuilder();
        private readonly System.Text.StringBuilder stdError = new System.Text.StringBuilder();

        internal SqlCmdWrapper(string executable, string arguments, string workingDirectory)
        {
            this.Arguments = arguments;
            this.Executable = executable;
            this.WorkingDirectory = workingDirectory;
        }

        /// <summary>
        /// Gets the standard output.
        /// </summary>
        internal string StandardOutput
        {
            get { return this.stdOut.ToString(); }
        }
        
        /// <summary>
        /// Gets the standard error.
        /// </summary>
        internal string StandardError 
        {
            get { return this.stdError.ToString(); } 
        }

        /// <summary>
        /// Gets the exit code.
        /// </summary>
        internal int ExitCode { get; private set; }

        /// <summary>
        /// Sets the working directory.
        /// </summary>
        internal string WorkingDirectory { get; set; }

        /// <summary>
        /// Sets the Executable.
        /// </summary>
        internal string Executable { get; set; }

        /// <summary>
        /// Sets the arguments.
        /// </summary>
        internal string Arguments { get; set; }

        internal NameValueCollection EnvironmentVariables
        {
            get { return this.environmentVars; }
        }

        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <returns>int</returns>
        public int Execute()
        {
            Process sqlCmdProcess = new Process();
            try
            {
                var startInfo = new ProcessStartInfo(this.Executable, this.Arguments)
                {
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    WorkingDirectory = this.WorkingDirectory
                };

                foreach (string key in this.EnvironmentVariables)
                {
                    startInfo.EnvironmentVariables[key] = this.EnvironmentVariables[key];
                }

                sqlCmdProcess.StartInfo = startInfo;

                // Set our event handlers to asynchronously read the output and errors. If
                // we use synchronous calls we may deadlock when the StandardOut/Error buffer
                // gets filled (only 4k size) and the called app blocks until the buffer
                // is flushed.  This stops the buffers from getting full and blocking.
                sqlCmdProcess.OutputDataReceived += this.StandardOutHandler;
                sqlCmdProcess.ErrorDataReceived += this.StandardErrorHandler;

                sqlCmdProcess.Start();
                sqlCmdProcess.BeginOutputReadLine();
                sqlCmdProcess.BeginErrorReadLine();
                sqlCmdProcess.WaitForExit(Int32.MaxValue);
            }
            finally
            {
                // get the exit code and release the process handle
                if (!sqlCmdProcess.HasExited)
                {
                    // not exited yet within our timeout so kill the process
                    sqlCmdProcess.Kill();

                    while (!sqlCmdProcess.HasExited)
                    {
                        System.Threading.Thread.Sleep(50);
                    }
                }

                this.ExitCode = sqlCmdProcess.ExitCode;
                sqlCmdProcess.Close();
            }

            return this.ExitCode;
        }

        private void StandardErrorHandler(object sendingProcess, DataReceivedEventArgs lineReceived)
        {
            // Collect the error output.
            if (!String.IsNullOrEmpty(lineReceived.Data))
            {
                // Add the text to the collected errors.
                this.stdError.AppendLine(lineReceived.Data);
            }
        }

        private void StandardOutHandler(object sendingProcess, DataReceivedEventArgs lineReceived)
        {
            // Collect the command output.
            if (!String.IsNullOrEmpty(lineReceived.Data))
            {
                // Add the text to the collected output.
                this.stdOut.AppendLine(lineReceived.Data);
            }
        }
    }
}
