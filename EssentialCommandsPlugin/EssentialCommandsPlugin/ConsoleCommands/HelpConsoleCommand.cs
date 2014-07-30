using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpStar.Lib.Logging;
using SharpStar.Lib.Plugins;

namespace EssentialCommandsPlugin.ConsoleCommands
{
    public class HelpConsoleCommand
    {

        private static readonly SharpStarLogger Logger = EssentialCommands.Logger;

        [ConsoleCommand("help", "Display a list of commands")]
        public void Help(string[] args)
        {
            Logger.Info("-- List of Commands --");

            foreach (var cmd in EssentialCommands.ConsoleCommands)
            {
                Logger.Info("Command: {0} - {1}", cmd.Key, cmd.Value);
            }
        }

    }
}
