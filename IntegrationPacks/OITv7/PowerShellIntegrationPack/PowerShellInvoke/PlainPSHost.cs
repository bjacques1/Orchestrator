namespace PowerShellInvoke
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using System.Management.Automation;
    using System.Management.Automation.Host;
    using System.Management.Automation.Runspaces;
    using System.Security;
    using System.Text;
    using System.Threading;
    using PowerShellIntegrationPack;

    /// <summary>
    ///     Logging for PSHost in a PowerShell runspace
    /// </summary>
    internal class Logger : IDisposable
    {
        private TextWriter writerOut = null;
        private TextWriter writerError = null;

        /// <summary>
        ///     Opens the logging for stdout and stderr
        /// </summary>
        /// <param name="outFilename">File name for stdout, null if no logging</param>
        /// <param name="errFilename">File name for stderr, null if no logging</param>
        public void Open(
            string outFilename,
            string errFilename)
        {
            try
            {
                if (!string.IsNullOrEmpty(outFilename))
                {
                    writerOut = new StreamWriter(outFilename, true, System.Text.Encoding.UTF8, 512);
                }

                if (!string.IsNullOrEmpty(errFilename))
                {
                    writerError = new StreamWriter(errFilename, true, Encoding.UTF8, 1024);
                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        ///     Flushes the logs to the disk
        /// </summary>
        public void Flush()
        {
            if (writerOut != null)
            {
                writerOut.Flush();
            }

            if (writerError != null)
            {
                writerError.Flush();
            }
        }

        /// <summary>
        ///     Closes the logging
        /// </summary>
        public void Close()
        {
            if (writerOut != null)
            {
                writerOut.Close();
                writerOut.Dispose();
                writerOut = null;
            }

            if (writerError != null)
            {
                writerError.Close();
                writerError.Dispose();
                writerError = null;
            }
        }

        /// <summary>
        ///     Writes a message to the output with end of line
        /// </summary>
        /// <param name="message">Message being written</param>
        /// <param name="parameters">Parameters for the format</param>
        public void Out(string message, params object[] parameters)
        {
            if (writerOut == null)
            {
                Console.Out.WriteLine(message, parameters);
            }
            else
            {
                writerOut.WriteLine(message, parameters);
            }
        }

        /// <summary>
        ///     Writes a message to the output without end of line
        /// </summary>
        /// <param name="message">Message being written</param>
        /// <param name="parameters">Parameters for the format</param>
        public void OutInline(string message, params object[] parameters)
        {
            if (writerOut == null)
            {
                Console.Out.Write(message, parameters);
            }
            else
            {
                writerOut.Write(message, parameters);
            }
        }

        /// <summary>
        ///     Writes a message to the error with end of file
        /// </summary>
        /// <param name="message">Message being written</param>
        /// <param name="parameters">Parameters for the format</param>
        public void Error(string message, params object[] parameters)
        {
            if (writerError == null)
            {
                Console.Error.WriteLine(message, parameters);
            }
            else
            {
                writerError.WriteLine(message, parameters);
            }
        }

        private bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    this.Close();
                }
            }
        }
    }

    /// <summary>
    ///     Customized dummy PSHostRawUserInterface
    /// </summary>
    internal sealed class PlainPSHostUserRawInterface : PSHostRawUserInterface
    {
        #region Properties

        public override ConsoleColor BackgroundColor { get; set; }
        public override ConsoleColor ForegroundColor { get; set; }

        public override Coordinates CursorPosition { get; set; }
        public override Coordinates WindowPosition { get; set; }
        
        public override Size MaxPhysicalWindowSize { get { return new Size(150, 75); } }
        public override Size MaxWindowSize { get { return new Size(9999, 9999); } }
        public override Size WindowSize { get; set; }
        public override Size BufferSize { get; set; }
        public override int CursorSize { get; set; }
        public override string WindowTitle { get; set; }

        public override bool KeyAvailable { get { return false; } }

        #endregion

        public PlainPSHostUserRawInterface()
        {
            BackgroundColor = ConsoleColor.Black;
            ForegroundColor = ConsoleColor.White;
            
            CursorPosition = new Coordinates { X = 0, Y = 0 };
            WindowPosition = new Coordinates { X = 0, Y = 0 };

            WindowSize = new Size { Width = 132, Height = 50 };
            BufferSize = new Size { Width = 132, Height = 99999 };
            CursorSize = 50; // cursor size as a percentage of a buffer cell
            WindowTitle = "PlainPSHostUserRawInterface";
        }
        
        public override void FlushInputBuffer()
        {
            EventTracing.TraceInfo("PlainPSHostUserRawInterface.FlushInputBuffer");
        }

        public override BufferCell[,] GetBufferContents(Rectangle rectangle)
        {
            EventTracing.TraceInfo("PlainPSHostUserRawInterface.GetBufferContents");
            return null;
        }
        
        public override KeyInfo ReadKey(ReadKeyOptions options)
        {
            return new KeyInfo();
        }

        public override void ScrollBufferContents(Rectangle source, Coordinates destination, Rectangle clip, BufferCell fill)
        {
            EventTracing.TraceInfo("PlainPSHostUserRawInterface.ScrollBufferContents");
        }

        public override void SetBufferContents(Rectangle rectangle, BufferCell fill)
        {
            EventTracing.TraceInfo("PlainPSHostUserRawInterface.SetBufferContents(rectangle, fill)");
        }

        public override void SetBufferContents(Coordinates origin, BufferCell[,] contents)
        {
            EventTracing.TraceInfo("PlainPSHostUserRawInterface.SetBufferContents(origin, contents)");
        }
    }

    /// <summary>
    ///     Customized PSHostUserInterface for capturing the log of stdout and stderr
    ///     http://msdn.microsoft.com/en-us/library/system.management.automation.host.pshostuserinterface(v=VS.85).aspx
    /// </summary>
    internal class PlainPSHostUserInterface : PSHostUserInterface
    {
        private Logger log = null;
        
        private PlainPSHostUserRawInterface rawUI = new PlainPSHostUserRawInterface();
        public override PSHostRawUserInterface RawUI { get { return rawUI; } }

        internal PlainPSHostUserInterface(Logger log)
        {
            if (log == null)
            {
                throw new ArgumentNullException("log");
            }

            this.log = log;
        }

        public override Dictionary<string, PSObject> Prompt(
            string caption,
            string message,
            Collection<FieldDescription> descriptions)
        {
            // TODO: feed input to make it interactive
            log.Out("PROMPT: {0} -- {1}", caption, message);

            return null;
        }

        public override int PromptForChoice(
            string caption,
            string message,
            Collection<ChoiceDescription> choices,
            int defaultChoice)
        {
            // TODO:
            log.Out("PROMPT FOR CHOICE: {0} -- {1}", caption, message);
            foreach (var choice in choices)
            {
                log.Out("  CHOICE {0} : {1}", choice.Label, choice.HelpMessage);
            }

            return 0;
        }

        public override PSCredential PromptForCredential(
            string caption,
            string message,
            string userName,
            string targetName,
            PSCredentialTypes allowedCredentialTypes,
            PSCredentialUIOptions options)
        {
            // TODO: feed password here.
            log.Out("PROMPT FOR CREDENTIAL: {0} -- {1}", caption, message);
            log.Out("   UserName: {0}  Target: {1}", userName, targetName);

            return null;
        }

        public override PSCredential PromptForCredential(
            string caption,
            string message,
            string userName,
            string targetName)
        {
            // TODO:
            log.Out("PROMPT FOR CREDENTIAL: {0} -- {1}", caption, message);
            log.Out("   UserName: {0}  Target: {1}", userName, targetName);

            return null;
        }
        
        public override string ReadLine()
        {
            log.Error("ReadLine() disabled");

            return string.Empty;
        }

        public override SecureString ReadLineAsSecureString()
        {
            log.Error("ReadLineAsSecureString() disabled");
            return null;
        }

        public override void Write(
            ConsoleColor foregroundColor,
            ConsoleColor backgroundColor,
            string value)
        {
            log.OutInline(value);
        }

        public override void Write(string value)
        {
            log.OutInline(value);
        }

        public override void WriteDebugLine(string message)
        {
            log.Out("DEBUG: " + message);
        }

        public override void WriteErrorLine(string value)
        {
            log.Error(value);
        }

        public override void WriteLine(string value)
        {
            log.Out(value);
        }

        public override void WriteProgress(long sourceId, ProgressRecord record)
        {
            log.Out("PROGRESS: {0}% complete for Status '{1}' {2} seconds remaining.",
                record.PercentComplete,
                record.StatusDescription,
                record.SecondsRemaining);
        }

        public override void WriteVerboseLine(string message)
        {
            log.Out(message);
        }

        public override void WriteWarningLine(string message)
        {
            log.Out("WARNING: " + message);
        }
    }

    /// <summary>
    ///     Customized PSHost for capturing the log of stdout and stderr
    ///     http://msdn.microsoft.com/en-us/library/system.management.automation.host.pshost_members(v=VS.85).aspx
    /// </summary>
    internal class PlainPSHost : PSHost, IHostSupportsInteractiveSession
    {
        private Logger log = null;

        private Guid instanceGuid = Guid.NewGuid();
        public override Guid InstanceId { get { return instanceGuid; } }
        
        private PlainPSHostUserInterface hostUI = null;
        public override PSHostUserInterface UI { get { return hostUI; } }

        private Runspace runspace = null;
        public Runspace Runspace { get { return runspace; } }
        public bool IsRunspacePushed { get { return runspace == null; } }

        public override CultureInfo CurrentCulture { get { return Thread.CurrentThread.CurrentCulture; } }
        public override CultureInfo CurrentUICulture { get { return Thread.CurrentThread.CurrentUICulture; } }       
        public override Version Version { get { return new Version(1, 0, 0, 0); } }
        public override string Name { get { return "PlainPSHost"; } }

        public PlainPSHost(Logger log)
        {
            if (log == null)
            {
                throw new ArgumentNullException("log");
            }

            this.log = log;
            hostUI = new PlainPSHostUserInterface(log);
        }

        public override void EnterNestedPrompt()
        {
            log.Out("EnterNestedPrompt");
        }

        public override void ExitNestedPrompt()
        {
            log.Out("ExitNestedPrompt");
        }

        public override void NotifyBeginApplication()
        {
            log.Out("NotifyBeginApplication");
        }

        public override void NotifyEndApplication()
        {
            log.Out("NotifyEndApplication");
        }

        public override void SetShouldExit(int exitCode)
        {
            log.Out("SetShouldExit");
        }

        public void PopRunspace()
        {
            log.Out("PopRunspace");
            runspace = null;
        }

        public void PushRunspace(Runspace runspace)
        {
            log.Out("PushRunspace {0}", runspace.InstanceId.ToString());
            this.runspace = runspace;
        }
    }
}
