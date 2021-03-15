using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using txtrconvert.Impl;
using static txtrconvert.Util.Shared.StaticMembers;

namespace txtrconvert.Util
{
    public partial class Logger : ICopyable<Logger, Logger.CopyLevel>, IDisposable
    {
        #region Tag

        private string TAG = nameof(Logger);

        public string Tag
        {
            get { return TAG; }
            set { if (!TAG.Trim().Equals("")) TAG = value; }
        }

        #endregion

        #region Level

        private Levels level = Levels.VERBOSE;

        public Levels Level
        {
            get { return level; }
            set
            {
                if (value == Levels.VERBOSE
                    || value == Levels.INFO
                    || value == Levels.WARNING
                    || value == Levels.ERROR)
                {
                    level = value;
                }
            }
        }

        #endregion

        #region Flag

        private Flags FLAG = Flags.NONE;

        public Flags Flag
        {
            get { return FLAG; }
            set
            {
                if ((value & Flags.NONE) == Flags.NONE
                    || (value & Flags.DEBUG) == Flags.DEBUG
                    || (value & Flags.DEBUGCONSOLE) == Flags.DEBUGCONSOLE
                    || (value & Flags.CLICONSOLE) == Flags.CLICONSOLE
                    || (value & Flags.CLICOLORS) == Flags.CLICOLORS
                    || (value & Flags.LOGFILE) == Flags.LOGFILE
                    || (value & Flags.ALL) == Flags.ALL)
                {
                    if ((FLAG & value) == value)
                        FLAG &= ~value;
                    else
                        FLAG |= value;
                }
            }
        }

        #endregion

        #region Indent

        private string indentSequence = "  ";

        public string IndentSequence
        {
            get { return indentSequence; }
            set { indentSequence = value; } // TODO: No special checking yet... could have length check?
        }

        private int indentLevel = 0;

        public int IndentLevel
        {
            get { return indentLevel; }
            set { if (value < 0) indentLevel = value; } // TODO: No special checking yet... could have greater than 255 check?
        }

        #endregion

        #region LogFile

        private FileStream logStream = null;

        public bool InitializeLogFile(string pLogFilePath)
        {
            if ((FLAG & Flags.LOGFILE) == Flags.LOGFILE)
            {
                if (!SetupLogFile(pLogFilePath))
                {
                    FLAG &= ~Flags.LOGFILE;
                    logStream?.Close();
                    logStream = null;
                    Trace.WriteLine("Failed to initilize Logger FileStream");
                    return false;
                }
                return true;
            }
            return false;
        }

        private bool SetupLogFile(string pLogFilePath)
        {
            try
            {
                if (Directory.Exists(Path.GetDirectoryName(pLogFilePath))) Directory.CreateDirectory(Path.GetDirectoryName(pLogFilePath));
                logStream = new FileStream(pLogFilePath, File.Exists(pLogFilePath) ? FileMode.Append : FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
                logStream?.Write(Encoding.Unicode.GetBytes($"{NewLine}=== Logging Session started at {DateTime.Now:MMMM dd, yyy H:mm:ss zzz} ==={NewLine}"));
                logStream?.Flush();
                return true;
            }
            catch (Exception exception)
            {
                Trace.WriteLine($"Failed to setuip Logger FileStream:{NewLine}{exception}");
                return false;
            }
        }

        #endregion

        #region (constructor)

        public Logger(string pTAG) : this(pTAG, Levels.VERBOSE, Flags.NONE)
        {
        }

        public Logger(string pTAG, Levels pLevel) : this(pTAG, pLevel, Flags.NONE)
        {
        }

        public Logger(string pTAG, Levels pLevel, Flags pFlags) : this(pTAG, pLevel, Flags.NONE, "")
        {
        }

        public Logger(string pTAG, Levels pLevel, Flags pFlags, string pLogFilePath)
        {
            Tag = pTAG;
            Level = pLevel;
            Flag = pFlags;
            InitializeLogFile(pLogFilePath);
        }

        #endregion

        #region Verbose

        public void Verbose<T>(params T[] args) { VerbosePrefix("", args); }

        public void VerbosePrefix<T>(string prefix, params T[] args) { Log(Levels.VERBOSE, prefix, append: false, args); }

        public void VerboseAppend<T>(params T[] args) { VerbosePrefixAppend("", args); }

        public void VerbosePrefixAppend<T>(string prefix, params T[] args) { Log(Levels.VERBOSE, prefix, append: true, args); }

        #endregion

        #region Info

        public void Info<T>(params T[] args) { InfoPrefix("", args); }

        public void InfoPrefix<T>(string prefix, params T[] args) { Log(Levels.INFO, prefix, append: false, args); }

        public void InfoAppend<T>(params T[] args) { InfoPrefixAppend("", args); }

        public void InfoPrefixAppend<T>(string prefix, params T[] args) { Log(Levels.INFO, prefix, append: true, args); }

        #endregion

        #region Warning

        public void Warning<T>(params T[] args) { WarningPrefix("", args); }

        public void WarningPrefix<T>(string prefix, params T[] args) { Log(Levels.WARNING, prefix, append: false, args); }

        public void WarningAppend<T>(params T[] args) { WarninPrefixgAppend("", args); }

        public void WarninPrefixgAppend<T>(string prefix, params T[] args) { Log(Levels.WARNING, prefix, append: true, args); }

        #endregion

        #region Error

        public void Error<T>(params T[] args) { ErrorPrefix("", args); }

        public void ErrorPrefix<T>(string prefix, params T[] args) { Log(Levels.ERROR, prefix, append: false, args); }

        public void ErrorAppend<T>(params T[] args) { ErrorPrefixAppend("", args); }

        public void ErrorPrefixAppend<T>(string prefix, params T[] args) { Log(Levels.ERROR, prefix, append: true, args); }

        #endregion

        #region Log

        private void Log<T>(Levels pLevel, string prefix, bool append, params T[] args)
        {
            if ((FLAG & Flags.DEBUG) == Flags.DEBUG && (int)pLevel >= (int)level)
            {
                // Output format is: indentprefix[yyyymmdd|level|TAG::CallingMethodName] arg1 arg2 arg3 ...
                StringBuilder output = new StringBuilder();
                foreach (var _ in Enumerable.Range(0, indentLevel)) output.Append(indentSequence);
                // No .Trim() because prefix could have whitespace/newline
                if (!prefix.Equals("")) output.Append(prefix);
                output.Append("[");
                output.Append(DateTime.Now.ToString("yyyymmdd"));
                output.Append("|");
                output.Append(pLevel.ToString());
                output.Append("|");
                output.Append(TAG);
                output.Append("::");
                output.Append(GetCallingMethodName());
                output.Append("] ");
                // AppendJoin is found in .NET 5 only, see extension method if not .NET 5 project
                output.AppendJoin(' ', args);

                // Visual Studio Debug Console Output
                if ((FLAG & Flags.DEBUGCONSOLE) == Flags.DEBUGCONSOLE)
                {
                    if (append)
                        Debug.Write(output.ToString());
                    else
                        Debug.WriteLine(output.ToString());
                }

                // Standard Console Output
                if ((FLAG & Flags.CLICONSOLE) == Flags.CLICONSOLE)
                {
                    ConsoleColor prevClr = Console.ForegroundColor;

                    if ((FLAG & Flags.CLICOLORS) == Flags.CLICOLORS)
                    {
                        switch (pLevel)
                        {
                            case Levels.VERBOSE:
                                Console.ForegroundColor = ConsoleColor.DarkGray;
                                break;
                            case Levels.INFO:
                                Console.ForegroundColor = ConsoleColor.Blue;
                                break;
                            case Levels.WARNING:
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                break;
                            case Levels.ERROR:
                                Console.ForegroundColor = ConsoleColor.Red;
                                break;
                        }
                    }

                    if (level == Levels.WARNING || level == Levels.ERROR)
                    {
                        if (append)
                            Console.Error.Write(output.ToString());
                        else
                            Console.Error.WriteLine(output.ToString());
                    }
                    else
                    {
                        if (append)
                            Console.Out.Write(output.ToString());
                        else
                            Console.Out.WriteLine(output.ToString());
                    }

                    Console.ForegroundColor = prevClr;
                }

                if ((FLAG & Flags.LOGFILE) == Flags.LOGFILE)
                {
                    logStream?.Write(Encoding.Unicode.GetBytes($"{output}{NewLine}"));
                    logStream?.Flush();
                }
            }
        }

        private string GetCallingMethodName()
        {
            int level = 0;
            while (true)
            {
                MethodBase methodBase = new StackFrame(level)?.GetMethod();
                Type classBase = methodBase?.DeclaringType;
                if (level > 4 || methodBase == null || classBase == null) break;
                else if (classBase.Name != nameof(Logger)) return methodBase.Name;
                ++level;
            }
            return "UNKNOWN";
        }

        #endregion

        #region Indent

        public void Indent() { IncreaseIndent(); }

        public void IncreaseIndent() { ++indentLevel; }

        public void Outdent() { DecreaseIndent(); }

        public void DecreaseIndent() {
            --indentLevel;
            if (indentLevel < 0) indentLevel = 0;
        }

        public void ResetIndent() {
            indentLevel = 0;
        }

        #endregion

        #region Copy

        public void CopyTo(in Logger logger)
        {
            CopyTo(logger, CopyLevel.ALL);
        }

        public void CopyTo(in Logger logger, CopyLevel copyLevel)
        {
            Copy(logger, copyLevel, true);
        }

        public void CopyFrom(in Logger logger)
        {
            CopyFrom(logger, CopyLevel.ALL);
        }

        public void CopyFrom(in Logger logger, CopyLevel copyLevel)
        {
            Copy(logger, copyLevel, false);
        }

        public void Copy(in Logger logger, CopyLevel copyLevel, bool toOrFrom)
        {
            if ((copyLevel & CopyLevel.TAG) == CopyLevel.TAG)
            {
                if (toOrFrom)
                    logger.Tag = Tag;
                else
                    Tag = logger.Tag;
            }
            if ((copyLevel & CopyLevel.LEVEL) == CopyLevel.LEVEL)
            {
                if (toOrFrom)
                    logger.Level = Level;
                else
                    Level = logger.Level;
            }
            if ((copyLevel & CopyLevel.FLAG) == CopyLevel.FLAG)
            {
                if (toOrFrom)
                    logger.Flag = Flag;
                else
                    Flag = logger.Flag;
            }
            if ((copyLevel & CopyLevel.INDENTSEQ) == CopyLevel.INDENTSEQ)
            {
                if (toOrFrom)
                    logger.IndentSequence = IndentSequence;
                else
                    IndentSequence = logger.IndentSequence;
            }
            if ((copyLevel & CopyLevel.INDENTLVL) == CopyLevel.INDENTLVL)
            {
                if (toOrFrom)
                    logger.IndentLevel = IndentLevel;
                else
                    IndentLevel = logger.IndentLevel;
            }
            if ((copyLevel & CopyLevel.LOGFILE) == CopyLevel.LOGFILE)
            {
                if (toOrFrom)
                {
                    if (logStream != null) logger.InitializeLogFile(logStream.Name);
                }
                else
                {
                    if (logger.logStream != null) InitializeLogFile(logger.logStream.Name);
                }
            }
        }

        #endregion

        #region Clone

        public Logger Clone()
        {
            return new Logger(Tag, Level, Flag);
        }

        public void Clone(out Logger newLogger)
        {
            newLogger = Clone();
        }

        public Logger ClonePartial(CopyLevel copyLevel)
        {
            string vTag = nameof(Logger);
            Levels vLevel = Levels.VERBOSE;
            Flags vFlag = Flags.NONE;
            if ((copyLevel & CopyLevel.TAG) == CopyLevel.TAG)
                vTag = Tag;
            if ((copyLevel & CopyLevel.LEVEL) == CopyLevel.LEVEL)
                vLevel = Level;
            if ((copyLevel & CopyLevel.FLAG) == CopyLevel.FLAG)
                vFlag = Flag;
            return new Logger(vTag, vLevel, vFlag);
        }

        public void ClonePartial(out Logger newLogger, CopyLevel copyLevel)
        {
            newLogger = ClonePartial(copyLevel);
        }

        #endregion

        #region Dispose

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                TAG = null;
                level = 0;
                FLAG = 0;
                indentLevel = 0;
                indentSequence = null;
                logStream?.Close();
                logStream = null;
            }
        }

        #endregion
    }
}
