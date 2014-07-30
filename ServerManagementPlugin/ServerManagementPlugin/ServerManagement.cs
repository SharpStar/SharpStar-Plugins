using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Mono.Addins;
using ServerManagementPlugin.Commands;
using ServerManagementPlugin.Config;
using ServerManagementPlugin.ConsoleCommands;
using SharpStar.Lib;
using SharpStar.Lib.Logging;
using SharpStar.Lib.Mono;
using SharpStar.Lib.Plugins;

[assembly: Addin]
[assembly: AddinDependency("SharpStar.Lib", "1.0")]
namespace ServerManagementPlugin
{
    [Extension]
    public class ServerManagement : CSPlugin
    {
        public override string Name
        {
            get { return "Server Management Plugin"; }
        }

        public static readonly SharpStarLogger Logger = new SharpStarLogger("Management");

        public static readonly ServerManagementConfig Config = new ServerManagementConfig("server_management.json");

        private static volatile bool shutdownRequested;

        private static readonly object shutdownLocker = new object();

        private Timer connTimer;

        public override void OnLoad()
        {
            RegisterCommandObject(new ServerCommand());

            RegisterConsoleCommandObject(new ServerConsoleCommand());

            if (connTimer != null)
            {
                connTimer.Stop();
                connTimer.Dispose();
            }

            if (Config.ConfigFile.ServerCheckInterval > 0)
            {
                connTimer = new Timer();
                connTimer.Interval = TimeSpan.FromMinutes(Config.ConfigFile.ServerCheckInterval).TotalMilliseconds;
                connTimer.Elapsed += (s, e) =>
                {

                    bool serverOnline = CheckServer();

                    if (!serverOnline && Config.ConfigFile.AutoRestartOnCrash && !shutdownRequested)
                    {
                        if (GetStarboundProcess() != null)
                            RestartServer();
                        else
                            StartServer();
                    }
                    else if (serverOnline)
                    {

                        Process[] procs = Process.GetProcesses().Where(p => p.ProcessName.Contains("starbound_server")).ToArray();

                        if (procs.Length == 1)
                        {

                            string fileName = FindProcessFileName(procs[0].Id);

                            Config.ConfigFile.ServerExecutable = fileName;
                            Config.Save();

                        }

                    }

                };

                connTimer.Start();
            }

        }

        private bool CheckServer()
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                socket.Connect(IPAddress.Parse("127.0.0.1"), SharpStarMain.Instance.Server.ServerPort);
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                socket.Close();
                socket.Dispose();
            }

            return true;

        }

        public static Process GetStarboundProcess()
        {

            Process[] procs = Process.GetProcesses().Where(p => p.ProcessName.Contains("starbound_server")).ToArray();

            if (procs.Length == 0)
                return null;

            return procs[0];

        }

        public static void StartServer()
        {

            string fileName = Config.ConfigFile.ServerExecutable;

            if (string.IsNullOrEmpty(fileName))
            {
                Logger.Error("Server executable is not set! Try starting the server manually first or set the 'ServerExecutable' property to the server's executable in the server_management.json file");

                return;
            }

            if (!File.Exists(fileName))
            {
                Logger.Error("Server executable file does not exist!");

                return;
            }

            Process[] procs = Process.GetProcesses().Where(p => p.ProcessName.Contains("starbound_server")).ToArray();

            if (procs.Length > 0)
            {
                Logger.Error("Server is already running!");

                return;

            }

            Logger.Info("Starting Server...");

            if (MonoHelper.IsRunningOnMono())
            {
                using (Process newServerProc = new Process())
                {

                    ProcessStartInfo newPsi = new ProcessStartInfo
                    {
                        UseShellExecute = false,
                        FileName = fileName,
                        CreateNoWindow = true
                    };

                    newServerProc.StartInfo = newPsi;
                    newServerProc.Start();

                    newServerProc.Close();
                }
            }
            else
            {
                ConsoleProcess.StartDetachedInNewProcessGroup(fileName, String.Empty);
            }

            lock (shutdownLocker)
            {
                shutdownRequested = true;
            }

        }

        public static void RestartServer()
        {
            Process[] procs = Process.GetProcesses().Where(p => p.ProcessName.Contains("starbound_server")).ToArray();

            if (procs.Length > 1)
            {
                Logger.Error("Found more than one process containing the name starbound_server!");
            }
            else if (procs.Length == 1)
            {

                Process serverProc = procs[0];

                string fileName = FindProcessFileName(serverProc.Id);

                if (!string.IsNullOrEmpty(fileName))
                {
                    Config.ConfigFile.ServerExecutable = fileName;
                    Config.Save();
                }

                if (MonoHelper.IsRunningOnMono())
                {

                    if (!string.IsNullOrEmpty(fileName))
                    {

                        using (Process newServerProc = new Process())
                        {

                            ProcessStartInfo newPsi = new ProcessStartInfo
                            {
                                UseShellExecute = false,
                                FileName = fileName,
                                CreateNoWindow = true
                            };

                            newServerProc.StartInfo = newPsi;
                            newServerProc.Start();

                            newServerProc.Close();
                        }


                        Logger.Info("New server instance started, shutting down the old one now!");

                        StopServer(serverProc);

                    }
                    else
                    {
                        Logger.Error("Restart failed!");
                    }

                }
                else
                {
                    if (!string.IsNullOrEmpty(fileName))
                    {

                        ConsoleProcess.StartDetachedInNewProcessGroup(fileName, String.Empty);

                        Logger.Info("New server instance started, shutting down the old one now!");

                        StopServer(serverProc);
                    }
                    else
                    {
                        Logger.Error("Restart failed! You may need to run SharpStar as an administrator.");
                    }
                }

            }
            else
            {
                Logger.Warn("Starbound Server process not found!");
            }

            lock (shutdownLocker)
            {
                shutdownRequested = false;
            }

        }

        public static void StopServer(Process serverProc)
        {

            if (serverProc == null || serverProc.HasExited)
                return;

            lock (shutdownLocker)
            {
                shutdownRequested = true;
            }

            if (MonoHelper.IsRunningOnMono())
            {
                using (Process killServerProc = new Process())
                {

                    ProcessStartInfo killPsi = new ProcessStartInfo
                    {
                        UseShellExecute = false,
                        FileName = "kill",
                        Arguments = String.Format("-2 {0}", serverProc.Id)
                    };

                    killServerProc.StartInfo = killPsi;
                    killServerProc.Start();

                    killServerProc.WaitForExit();
                    killServerProc.Close();
                    killServerProc.Dispose();

                }
            }
            else
            {
                serverProc.ExitViaCtrlEvent();

                Timer checkProcTimer = new Timer();
                checkProcTimer.AutoReset = false;
                checkProcTimer.Interval = TimeSpan.FromSeconds(5).TotalMilliseconds;

                checkProcTimer.Elapsed += (s, e) =>
                {
                    if (!serverProc.HasExited)
                    {
                        Logger.Error("Error shutting down the server gracefully. Process has been killed.");

                        serverProc.Kill();
                        serverProc.Close();
                    }
                };

                checkProcTimer.Start();

            }

        }

        private static string FindProcessFileName(int pid)
        {

            string fileName = String.Empty;

            if (MonoHelper.IsRunningOnMono())
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    FileName = "readlink",
                    Arguments = String.Format("/proc/{0}/exe", pid)
                };

                var tcs = new TaskCompletionSource<string>();

                Process getFileProc = new Process();
                getFileProc.StartInfo = psi;
                getFileProc.OutputDataReceived += (s, e) => tcs.SetResult(e.Data);

                getFileProc.Start();

                getFileProc.BeginOutputReadLine();

                tcs.Task.Wait();
                getFileProc.WaitForExit();
                getFileProc.Dispose();

                fileName = tcs.Task.Result;
            }
            else
            {
                const string wmiQueryString = "SELECT ProcessId, ExecutablePath, CommandLine FROM Win32_Process";
                using (var searcher = new ManagementObjectSearcher(wmiQueryString))
                using (var results = searcher.Get())
                {
                    var query = from p in Process.GetProcesses()
                                join mo in results.Cast<ManagementObject>()
                                on p.Id equals (int)(uint)mo["ProcessId"]
                                select new
                                {
                                    Process = p,
                                    Path = (string)mo["ExecutablePath"],
                                    CommandLine = (string)mo["CommandLine"],
                                };

                    var result = query.SingleOrDefault(p => p.Process.Id == pid);

                    if (result != null)
                    {
                        fileName = result.Path;
                    }
                }
            }

            return fileName;

        }

    }
}
