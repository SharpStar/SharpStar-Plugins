using System;
using System.Linq;
using System.Text;
using SharpStar.Lib;
using SharpStar.Lib.Attributes;
using SharpStar.Lib.Packets;
using SharpStar.Lib.Plugins;
using SharpStar.Lib.Server;

namespace EssentialCommandsPlugin.Commands
{
    public class BanCommand
    {

        [Command("ban", "Ban a user")]
        [CommandPermission("ban")]
        public void BanPlayer(SharpStarClient client, string[] args)
        {

            if (!EssentialCommands.CanUserAccess(client, "ban"))
                return;

            if (args.Length < 2)
            {
                client.SendChatMessage("Server", "Syntax: /ban <time m/h/d> <player name> \"<reason>\"");

                return;
            }

            string time = args[0];
            string rest = string.Join(" ", args.Skip(1));

            TimeType timeType = TimeTypeHelper.GetTimeType(time);
            TimeType maxTimeType = TimeTypeHelper.GetTimeType(EssentialCommands.Config.ConfigFile.MaxTempBanTime);

            if (timeType == TimeType.None || maxTimeType == TimeType.None)
            {
                client.SendChatMessage("Server", "Syntax: /ban <time m/h/d> <player name> \"<reason>\"");

                return;
            }

            int timeInt;
            int maxTimeInt;
            if (!TimeTypeHelper.TryGetTimeInt(time, out timeInt) || !TimeTypeHelper.TryGetTimeInt(EssentialCommands.Config.ConfigFile.MaxTempBanTime, out maxTimeInt))
            {
                client.SendChatMessage("Server", "Invalid Time!");

                return;
            }

            if (!TimeTypeHelper.IsTimeAllowable(timeType, timeInt, maxTimeType, maxTimeInt))
            {
                client.SendChatMessage("Server", "The max temporary ban time is " + EssentialCommands.Config.ConfigFile.MaxTempBanTime.ToLower());

                return;
            }

            int quoteIndex = rest.IndexOf("\"", StringComparison.OrdinalIgnoreCase);

            if (quoteIndex < 0)
            {
                client.SendChatMessage("Server", "Syntax: /ban <time m/h/d> <player name> \"<reason>\"");

                return;
            }

            string playerName = rest.Substring(0, quoteIndex - 1);
            string banReasonFull = rest.Substring(quoteIndex + 1);

            int lastQuoteIndex = banReasonFull.LastIndexOf("\"", StringComparison.InvariantCulture);

            if (lastQuoteIndex < 0)
            {
                client.SendChatMessage("Server", "Syntax: /ban <time m/h/d> <player name> \"<reason>\"");

                return;
            }

            string banReason = banReasonFull.Substring(0, lastQuoteIndex);

            var players = SharpStarMain.Instance.Server.Clients.ToList().Where(p => p.Player != null && p.Player.Name.Equals(playerName, StringComparison.OrdinalIgnoreCase)).ToList();

            if (players.Count == 0)
            {
                client.SendChatMessage("Server", "There are no players by that name!");

                return;
            }

            EssentialCommands.KickBanPlayer(client.Server, players, true, banReason, timeType.ToDateTime(timeInt));

        }

        [Command("permban", "Permanently ban a player")]
        [CommandPermission("permban")]
        public void PermBanPlayer(SharpStarClient client, string[] args)
        {
            if (!EssentialCommands.CanUserAccess(client, "permban"))
                return;

            if (args.Length < 1)
            {
                client.SendChatMessage("Server", "/permban <player name>");
            }

            string rest = string.Join(" ", args);

            int quoteIndex = rest.IndexOf("\"", StringComparison.OrdinalIgnoreCase);

            if (quoteIndex < 0)
            {
                client.SendChatMessage("Server", "Syntax: /permban <player name> \"<reason>\"");

                return;
            }

            string playerName = rest.Substring(0, quoteIndex - 1);
            string banReasonFull = rest.Substring(quoteIndex + 1);

            int lastQuoteIndex = banReasonFull.LastIndexOf("\"", StringComparison.InvariantCulture);

            if (lastQuoteIndex < 0)
            {
                client.SendChatMessage("Server", "Syntax: /permban <player name> \"<reason>\"");

                return;
            }

            string banReason = banReasonFull.Substring(0, lastQuoteIndex);

            var players = SharpStarMain.Instance.Server.Clients.ToList().Where(p => p.Player != null && p.Player.Name.Equals(playerName, StringComparison.OrdinalIgnoreCase)).ToList();

            if (players.Count == 0)
            {
                client.SendChatMessage("Server", "There are no players by that name!");

                return;
            }

            EssentialCommands.KickBanPlayer(client.Server, players, true, banReason);

        }

        [Command("unban", "Unban a user")]
        [CommandPermission("ban")]
        public void UnbanPlayer(SharpStarClient client, string[] args)
        {

            if (!EssentialCommands.CanUserAccess(client, "unban"))
                return;

            if (args.Length == 0)
            {

                client.SendChatMessage("Server", "Syntax: /unban <player name>");

                return;

            }

            string username = args[0];

            var usr = SharpStarMain.Instance.Database.GetUser(username);

            if (usr == null)
            {
                client.SendChatMessage("Server", "There is no user by that name!");

                return;
            }

            EssentialCommands.Database.RemoveBanByUserId(usr.Id);

            client.SendChatMessage("Server", "The user is no longer banned!");

        }

        [PacketEvent(KnownPacket.ConnectionResponse)]
        public void ConnectionResponse(IPacket packet, SharpStarClient client)
        {

            if (client.Server.Player == null)
                return;

            ConnectionResponsePacket crp = (ConnectionResponsePacket)packet;

            EssentialCommandsBanUUID uuidBan = EssentialCommands.Database.GetBansUuid(client.Server.Player.UUID);

            string banReason = "You have been banned!";
            string tempTemplate = EssentialCommands.Config.ConfigFile.TempBanTemplate;
            string permTemplate = EssentialCommands.Config.ConfigFile.PermBanTemplate;

            if (uuidBan != null)
            {
                EssentialCommandsBan ban = EssentialCommands.Database.GetBan(uuidBan.BanId);

                if (ban != null)
                {
                    if (ban.ExpirationTime.HasValue && DateTime.Now > ban.ExpirationTime.Value)
                    {
                        EssentialCommands.Database.RemoveBan(ban.Id);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(ban.BanReason))
                        {
                            banReason = ban.BanReason;
                        }

                        if (!string.IsNullOrEmpty(tempTemplate) && !string.IsNullOrEmpty(permTemplate))
                        {
                            string tmp = banReason;

                            if (ban.ExpirationTime.HasValue)
                                banReason = tempTemplate;
                            else
                                banReason = permTemplate;

                            banReason = banReason.Replace("<reason>", tmp);

                            if (ban.ExpirationTime.HasValue)
                                banReason = banReason.Replace("<time>", HumanizeTimeSpan(ban.ExpirationTime.Value - DateTime.Now));

                            if (client.Server.Player.UserAccount != null)
                                banReason = banReason.Replace("<account>", client.Server.Player.UserAccount.Username);

                        }

                        crp.Success = false;
                        crp.RejectionReason = banReason;

                        EssentialCommands.Logger.Info("Banned player {0} tried to join!", client.Server.Player.Name);
                    }
                }
                else
                {
                    EssentialCommands.Database.RemoveBanUuid(uuidBan.UUID);
                }
            }
            else if (client.Server.Player.UserAccount != null)
            {
                EssentialCommandsBan ban = EssentialCommands.Database.GetBanByUserId(client.Server.Player.UserAccount.Id);

                if (ban != null)
                {

                    if (ban.ExpirationTime.HasValue && DateTime.Now > ban.ExpirationTime.Value)
                    {
                        EssentialCommands.Database.RemoveBan(ban.Id);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(ban.BanReason))
                        {
                            banReason = ban.BanReason;
                        }

                        if (!string.IsNullOrEmpty(tempTemplate) && !string.IsNullOrEmpty(permTemplate))
                        {
                            string tmp = banReason;

                            if (ban.ExpirationTime.HasValue)
                                banReason = tempTemplate;
                            else
                                banReason = permTemplate;

                            banReason = banReason.Replace("<reason>", tmp);

                            if (ban.ExpirationTime.HasValue)
                                banReason = banReason.Replace("<time>", HumanizeTimeSpan(ban.ExpirationTime.Value - DateTime.Now));

                            if (client.Server.Player.UserAccount != null)
                                banReason = banReason.Replace("<account>", client.Server.Player.UserAccount.Username);

                        }

                        crp.Success = false;
                        crp.RejectionReason = banReason;

                        EssentialCommands.Logger.Info("Banned player {0} ({1}) tried to join!", client.Server.Player.Name, client.Server.Player.UserAccount.Username);
                    }
                }

            }

        }

        private static string HumanizeTimeSpan(TimeSpan ts)
        {

            StringBuilder sb = new StringBuilder();

            if (ts.Days > 0)
            {
                sb.Append(ts.Days + " days ");

                ts = ts.Subtract(TimeSpan.FromDays(ts.Days));
            }

            if (ts.Hours > 0)
            {
                sb.Append(ts.Hours + " hours ");
                
                ts = ts.Subtract(TimeSpan.FromHours(ts.Hours));
            }

            if (ts.Minutes > 0)
            {
                sb.Append(ts.Minutes + " minutes ");

                ts = ts.Subtract(TimeSpan.FromMinutes(ts.Minutes));
            }

            if (ts.Seconds > 0)
            {
                sb.Append(ts.Seconds + " seconds");
            }

            return sb.ToString();

        }

    }

    public enum TimeType
    {
        None,
        Minute,
        Hour,
        Day
    }

    public static class TimeTypeHelper
    {

        public static bool IsTimeAllowable(TimeType type, int time, TimeType maxType, int maxTime)
        {

            TimeSpan ts;
            TimeSpan ts2;

            switch (type)
            {
                case TimeType.Minute:
                    ts = TimeSpan.FromMinutes(time);
                    break;
                case TimeType.Hour:
                    ts = TimeSpan.FromHours(time);
                    break;
                case TimeType.Day:
                    ts = TimeSpan.FromDays(time);
                    break;
                default:
                    return false;
            }

            switch (maxType)
            {
                case TimeType.Minute:
                    ts2 = TimeSpan.FromMinutes(maxTime);
                    break;
                case TimeType.Hour:
                    ts2 = TimeSpan.FromHours(maxTime);
                    break;
                case TimeType.Day:
                    ts2 = TimeSpan.FromDays(maxTime);
                    break;
                default:
                    return false;
            }

            return ts <= ts2;

        }

        public static TimeType GetTimeType(string time)
        {
            TimeType timeType = TimeType.None;

            switch (time.Last())
            {
                case 'm':
                    timeType = TimeType.Minute;
                    break;
                case 'h':
                    timeType = TimeType.Hour;
                    break;
                case 'd':
                    timeType = TimeType.Day;
                    break;
            }

            return timeType;
        }

        public static DateTime? ToDateTime(this TimeType type, int time)
        {
            DateTime? dt = DateTime.Now;

            switch (type)
            {
                case TimeType.Minute:
                    dt = dt.Value.AddMinutes(time);
                    break;
                case TimeType.Hour:
                    dt = dt.Value.AddHours(time);
                    break;
                case TimeType.Day:
                    dt = dt.Value.AddDays(time);
                    break;
                default:
                    dt = null;
                    break;
            }

            return dt;
        }

        public static bool TryGetTimeInt(string time, out int timeOut)
        {
            return int.TryParse(time.Substring(0, time.Length - 1), out timeOut);
        }

    }

}
