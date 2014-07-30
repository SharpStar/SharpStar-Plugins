using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace ServerManagementPlugin
{
    //Credit: http://stackoverflow.com/a/16154403

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct STARTUPINFO
    {
        public Int32 cb;
        public string lpReserved;
        public string lpDesktop;
        public string lpTitle;
        public Int32 dwX;
        public Int32 dwY;
        public Int32 dwXSize;
        public Int32 dwYSize;
        public Int32 dwXCountChars;
        public Int32 dwYCountChars;
        public Int32 dwFillAttribute;
        public Int32 dwFlags;
        public Int16 wShowWindow;
        public Int16 cbReserved2;
        public IntPtr lpReserved2;
        public IntPtr hStdInput;
        public IntPtr hStdOutput;
        public IntPtr hStdError;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct PROCESS_INFORMATION
    {
        public IntPtr hProcess;
        public IntPtr hThread;
        public int dwProcessId;
        public int dwThreadId;
    }

    //MUST COMPILE WITH UNSAFE
    [StructLayout(LayoutKind.Sequential)]
    public struct SECURITY_ATTRIBUTES
    {
        public int nLength;
        public unsafe byte* lpSecurityDescriptor; //replacing unsafe byte * with IntPtr would allow compiling without unsafe, but it doesn't make a difference
        public int bInheritHandle;
    }

    public static class ConsoleProcess
    {
        [DllImport("kernel32.dll")]
        private static extern bool CreateProcess(string lpApplicationName, string lpCommandLine, ref SECURITY_ATTRIBUTES lpProcessAttributes, ref SECURITY_ATTRIBUTES lpThreadAttributes, bool bInheritHandles, uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, ref STARTUPINFO lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GenerateConsoleCtrlEvent(ConsoleCtrlEvent sigevent, int dwProcessGroupId);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool AttachConsole(int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool FreeConsole();

        [DllImport("kernel32.dll")]
        static extern bool SetConsoleCtrlHandler(ConsoleCtrlDelegate HandlerRoutine,
           bool Add);

        // Delegate type to be used as the Handler Routine for SCCH
        delegate Boolean ConsoleCtrlDelegate(CtrlTypes CtrlType);

        // Enumerated type for the control messages sent to the handler routine
        public enum CtrlTypes : uint
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT,
            CTRL_CLOSE_EVENT,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT
        }

        public enum ConsoleCtrlEvent
        {
            CTRL_C = 0,
            CTRL_BREAK = 1,
            CTRL_CLOSE = 2,
            CTRL_LOGOFF = 5,
            CTRL_SHUTDOWN = 6
        }

        public static Process StartDetachedInNewProcessGroup(string executableFileName, string arguments)
        {
            const uint NORMAL_PRIORITY_CLASS = 0x0020;
            const uint CREATE_NEW_PROCESS_GROUP = 0x00000200;
            const uint CREATE_NO_WINDOW = 0x08000000;
            const uint DETACHED_PROCESS = 0x00000008;
            const uint CREATE_NEW_CONSOLE = 0x00000010;

            bool retValue;
            PROCESS_INFORMATION pInfo = new PROCESS_INFORMATION();
            STARTUPINFO sInfo = new STARTUPINFO();
            SECURITY_ATTRIBUTES pSec = new SECURITY_ATTRIBUTES();
            SECURITY_ATTRIBUTES tSec = new SECURITY_ATTRIBUTES();
            pSec.nLength = Marshal.SizeOf(pSec);
            tSec.nLength = Marshal.SizeOf(tSec);

            retValue = CreateProcess(null, BuildCommandLine(executableFileName, arguments).ToString(), ref pSec, ref tSec, false, NORMAL_PRIORITY_CLASS | CREATE_NEW_PROCESS_GROUP, IntPtr.Zero, null, ref sInfo, out pInfo); //trying using DETACHED_PROCESS
            //doesn't work either //retValue = CreateProcess( null, application + " " + args, ref pSec,ref tSec, false, NORMAL_PRIORITY_CLASS | CREATE_NEW_PROCESS_GROUP | CREATE_NEW_CONSOLE, IntPtr.Zero, null, ref sInfo, out pInfo ); //try using CREATE_NEW_CONSOLE (just to see if it response WITH a console window; it doesn't)
            //doesn't work either //retValue = CreateProcess( null, application + " " + args, ref pSec,ref tSec, false, NORMAL_PRIORITY_CLASS | CREATE_NEW_PROCESS_GROUP | CREATE_NO_WINDOW, IntPtr.Zero, null, ref sInfo, out pInfo ); //try using CREATE_NO_WINDOW
            if (retValue)
                return Process.GetProcessById(pInfo.dwProcessId);
            else
                return null;
        }

        public static void ExitViaCtrlEvent(this Process p)
        {
            GenerateConsoleCtrlEvent(ConsoleCtrlEvent.CTRL_BREAK, p.Id);
            //doesn't work either //GenerateConsoleCtrlEvent( ConsoleCtrlEvent.CTRL_C, p.Id );
        }

        //Extracted from the System.Diagnostics.Process class via .NET Reflector
        private static StringBuilder BuildCommandLine(string executableFileName, string arguments)
        {
            StringBuilder builder = new StringBuilder();
            string str = executableFileName.Trim();
            bool flag = str.StartsWith("\"", StringComparison.Ordinal) && str.EndsWith("\"", StringComparison.Ordinal);
            if (!flag)
            {
                builder.Append("\"");
            }
            builder.Append(str);
            if (!flag)
            {
                builder.Append("\"");
            }
            if (!string.IsNullOrEmpty(arguments))
            {
                builder.Append(" ");
                builder.Append(arguments);
            }
            return builder;
        }
    }
    }