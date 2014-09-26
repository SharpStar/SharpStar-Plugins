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
using SharpStar.Lib.Packets;
using SharpStar.Lib.Plugins;
using SharpStar.Lib.Server;

[assembly: Addin("ServerManagement", Version = "1.1.0.0")]
[assembly: AddinDescription("A plugin to manage a Starbound server")]
[assembly: AddinProperty("sharpstar", "0.2.3.1")]
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

        private static volatile bool startingOrRestarting;

        private static readonly object locker = new object();

        private Timer connTimer;

        private ManagementEventWatcher processStopEvent;

        private static int pid;


        private static readonly PacketReader _pReader = new PacketReader();

        public override void OnLoad()
        {
            startingOrRestarting = false;

            RegisterCommandObject(new ServerCommand());

            RegisterConsoleCommandObject(new ServerConsoleCommand());

            if (connTimer != null)
            {
                connTimer.Stop();
                connTimer.Dispose();
            }

            if (Config.ConfigFile.AutoRestartOnCrash && !MonoHelper.IsRunningOnMono())
            {
                processStopEvent = new ManagementEventWatcher("SELECT * FROM Win32_ProcessStopTrace");
                processStopEvent.EventArrived += processStopEvent_EventArrived;
                processStopEvent.Start();
            }

            Process initProc = GetStarboundProcess();

            if (initProc != null)
            {
                pid = initProc.Id;

                Config.ConfigFile.ServerExecutable = FindProcessFileName(pid);
                Config.Save();
            }

            if (Config.ConfigFile.ServerCheckInterval > 0)
            {
                connTimer = new Timer();
                connTimer.Interval = TimeSpan.FromMinutes(Config.ConfigFile.ServerCheckInterval).TotalMilliseconds;
                connTimer.Elapsed += async (s, e) =>
                {
                    bool serverOnline = await CheckServer() && CheckServerUDP();

                    if (serverOnline)
                    {
                        lock (locker)
                        {
                            startingOrRestarting = false;
                            shutdownRequested = false;
                        }
                    }
                    else
                    {
                        KillClients();
                    }

                    Process serverProc = GetStarboundProcess();

                    if (serverProc != null)
                    {
                        pid = serverProc.Id;
                    }

                    bool startRestart;

                    if (MonoHelper.IsRunningOnMono())
                    {
                        startRestart = (!serverOnline || serverProc == null) && Config.ConfigFile.AutoRestartOnCrash && !shutdownRequested;
                    }
                    else
                    {
                        startRestart = ((serverProc != null && !serverProc.Responding) || serverProc == null || !serverOnline) && Config.ConfigFile.AutoRestartOnCrash && !shutdownRequested;
                    }

                    if (startRestart)
                    {
                        if (serverProc != null)
                        {
                            RestartServer();

                            Logger.Error("The Starbound server did not respond after a certain period of time. Restarting the server...");
                        }
                        else
                        {
                            Logger.Error("Server instance not found. Starting it up!");

                            StartServer();
                        }
                    }
                    else if (serverOnline)
                    {
                        if (serverProc != null)
                        {
                            string fileName = FindProcessFileName(pid);

                            Config.ConfigFile.ServerExecutable = fileName;
                            Config.Save();
                        }
                    }

                };

                connTimer.Start();
            }

        }

        private void processStopEvent_EventArrived(object sender, EventArrivedEventArgs e)
        {
            int processId = int.Parse(e.NewEvent.Properties["ProcessID"].Value.ToString());

            if (processId == pid && Config.ConfigFile.AutoRestartOnCrash && !shutdownRequested && !startingOrRestarting)
            {
                StartServer();

                Logger.Error("The Starbound server has been terminated. Starting it back up!");
            }
        }

        public override void OnUnload()
        {
            if (connTimer != null)
            {
                connTimer.Stop();
                connTimer = null;
            }

            if (processStopEvent != null)
            {
                processStopEvent.Stop();
                processStopEvent.Dispose();
                processStopEvent = null;
            }
        }

        private static void KillClients()
        {
            foreach (SharpStarServerClient ssc in SharpStarMain.Instance.Server.Clients)
            {
                try
                {
                    ssc.PlayerClient.ForceDisconnect();
                }
                catch
                {
                }

                try
                {
                    ssc.ServerClient.ForceDisconnect();
                }
                catch
                {
                }
            }
        }

        private async Task<bool> CheckServer()
        {
            return await Task.Run(() =>
            {
                TcpClient client = new TcpClient();

                bool toReturn;
                try
                {
                    string bind = SharpStarMain.Instance.Config.ConfigFile.StarboundBind;

                    if (string.IsNullOrEmpty(bind))
                        client.Connect(IPAddress.Parse("127.0.0.1"), SharpStarMain.Instance.Config.ConfigFile.ServerPort);
                    else
                        client.Connect(bind, SharpStarMain.Instance.Config.ConfigFile.ServerPort);

                    var stream = client.GetStream();

                    byte[] buffer = new byte[1024];

                    bool gotPacket = false;

                    DateTime start = DateTime.Now;
                    while (!gotPacket && (DateTime.Now - start) < TimeSpan.FromSeconds(5))
                    {
                        int read = stream.Read(buffer, 0, buffer.Length);

                        _pReader.NetworkBuffer = new ArraySegment<byte>(buffer, 0, read);

                        var packets = _pReader.UpdateBuffer(true).ToList();

                        gotPacket = packets.Any();
                    }

                    client.Client.Shutdown(SocketShutdown.Both);

                    toReturn = gotPacket;
                }
                catch
                {
                    toReturn = false;
                }
                finally
                {
                    client.Close();
                }

                return toReturn;
            });

        }

        private bool CheckServerUDP()
        {
            bool toReturn;

            UdpClient udpClient = new UdpClient();

            try
            {
                string bind = SharpStarMain.Instance.Config.ConfigFile.StarboundBind;

                IPEndPoint ipe;

                if (string.IsNullOrEmpty(bind))
                    ipe = new IPEndPoint(IPAddress.Parse("127.0.0.1"), SharpStarMain.Instance.Config.ConfigFile.ServerPort);
                else
                    ipe = new IPEndPoint(IPAddress.Parse(bind), SharpStarMain.Instance.Config.ConfigFile.ServerPort);

                udpClient.Connect(ipe);

                byte[] toSend = { 0xFF, 0xFF, 0xFF, 0xFF, 0x54, 0x53, 0x6F, 0x75, 0x72, 0x63, 0x65, 0x20, 0x45, 0x6E, 0x67, 0x69, 0x6E, 0x65, 0x20, 0x51, 0x75, 0x65, 0x72, 0x79, 0x00 };

                udpClient.Send(toSend, toSend.Length);

                byte[] recv = udpClient.Receive(ref ipe);

                using (MemoryStream ms = new MemoryStream(recv))
                {
                    A2SInfoResponse.FromStream(ms);
                }

                toReturn = true;

            }
            catch
            {
                toReturn = false;
            }
            finally
            {
                udpClient.Close();
            }

            return toReturn;

        }

        public static Process GetStarboundProcess()
        {
            Process[] procs = Process.GetProcesses();

            foreach (Process proc in procs)
            {
                try
                {
                    if (!proc.HasExited && proc.ProcessName.Contains("starbound_server"))
                    {
                        return proc;
                    }
                }
                catch
                {
                }
            }

            return null;
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

            Process[] procs = Process.GetProcesses();

            foreach (Process proc in procs)
            {
                try
                {
                    if (!proc.HasExited && proc.ProcessName.Contains("starbound_server"))
                    {
                        Logger.Error("Server is already running!");

                        return;
                    }
                }
                catch
                {
                }
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

                    pid = newServerProc.Id;
                }
            }
            else
            {
                Process proc = ConsoleProcess.StartDetachedInNewProcessGroup(fileName, String.Empty);
                pid = proc.Id;
            }

            lock (locker)
            {
                shutdownRequested = false;
                startingOrRestarting = false;
            }

        }

        public static void RestartServer()
        {

            Process[] procs = Process.GetProcesses();
            Process serverProc = null;

            foreach (Process proc in procs)
            {
                try
                {
                    if (!proc.HasExited && proc.ProcessName.Contains("starbound_server"))
                    {
                        serverProc = proc;
                    }
                }
                catch
                {
                }
            }

            if (serverProc != null)
            {

                string fileName = FindProcessFileName(serverProc.Id);

                if (!string.IsNullOrEmpty(fileName))
                {
                    Config.ConfigFile.ServerExecutable = fileName;
                    Config.Save();
                }

                lock (locker)
                {
                    startingOrRestarting = true;
                }

                KillClients();

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


                            Logger.Info("New server instance started, shutting down the old one now!");

                            StopServer(serverProc);

                            pid = newServerProc.Id;

                        }
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
                        Process proc = ConsoleProcess.StartDetachedInNewProcessGroup(fileName, String.Empty);
                        pid = proc.Id;

                        Logger.Info("New server instance started, shutting down the old one now!");

                        lock (locker)
                        {
                            startingOrRestarting = true;
                        }

                        StopServer(serverProc);

                        lock (locker)
                        {
                            startingOrRestarting = false;
                        }
                    }
                    else
                    {
                        Logger.Error("Restart failed! You may need to run SharpStar as an administrator.");
                    }
                }

            }
            else
            {
                Logger.Warn("Starbound Server process not found! Starting it up...");

                StartServer();
            }

            lock (locker)
            {
                shutdownRequested = false;
            }

        }

        public static void StopServer(Process serverProc)
        {

            if (serverProc == null || serverProc.HasExited)
            {
                Logger.Error("The server is not currently online!");

                return;
            }

            lock (locker)
            {
                shutdownRequested = true;
            }

            Logger.Info("Server shutting down...");

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

                    if (!killServerProc.WaitForExit((int)TimeSpan.FromSeconds(5).TotalMilliseconds))
                    {
                        Logger.Error("Error shutting down the server gracefully. Process has been killed.");

                        killServerProc.Kill();
                        serverProc.Kill();
                    }

                    killServerProc.Close();
                    killServerProc.Dispose();

                    Logger.Info("Server shutdown completed!");
                }
            }
            else
            {
                serverProc.ExitViaCtrlEvent();

                Timer checkProcTimer = new Timer();
                checkProcTimer.AutoReset = false;
                checkProcTimer.Interval = TimeSpan.FromSeconds(10).TotalMilliseconds;

                checkProcTimer.Elapsed += (s, e) =>
                {
                    if (!serverProc.HasExited)
                    {
                        Logger.Error("Error shutting down the server gracefully. Process has been killed.");

                        serverProc.Kill();
                        serverProc.Close();
                    }
                    else
                    {
                        Logger.Info("Server shutdown completed!");
                    }
                };

                checkProcTimer.Start();

            }

            KillClients();

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
