using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Modules;
using Discord.Commands;
using Discord.Commands.Permissions.Levels;
using SpaceXBot.Core;
using SpaceXBot.DataAccess;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using System.Globalization;

namespace SpaceXBot.Modules
{
    class CountdownModule : IModule
    {
        private ModuleManager _m;
        private DiscordClient _client;
#if (DEBUG)
        private ulong[] channelIDs = SpaceXBot.Core.SpxBot.Config.CountdownChannelDebugIDs;
#else
        private ulong[] channelIDs = SpaceXBot.Core.SpxBot.Config.CountdownChannelIDs;
#endif


        public void Install(ModuleManager manager)
        {
            _m = manager;
            _client = manager.Client;

            _m.CreateCommands("", g =>
            {
                g.MinPermissions((int)PermissionLevel.User);
                g.CreateCommand("listLaunches")
                .Parameter("retNumber", ParameterType.Optional)
                .Description("Returns Future Launches {number} \n{number} is an optional number of launches to return")
                .Do(async e =>
                {
                    RootObject rootObject;
                    int launchCount;

                    if (e.GetArg("retNumber") == null)
                    {
                        launchCount = 10;
                    }
                    else
                    {
                        launchCount = Convert.ToInt32(e.GetArg("retNumber"));
                    }

                    rootObject = Utils.getLaunches(launchCount);

                    string output = string.Empty;

                    for (int i = 0; i < rootObject.launches.Count; i++)
                    {
                        Launch currentLaunch = rootObject.launches[i];
                        output += "\n **Vehicle: ** " + currentLaunch.name.Split('|')[0] + "  |  " + "**Payload: **" + currentLaunch.name.Split('|')[1] + "  |  " + "**Current NET: **" + Utils.parseDate(currentLaunch.net) + " UTC";
                    }
                    await e.Channel.SendMessage(output);
                });

            });


            Task.Run(async () =>
            {
                while(true)
                {
                    RootObject rootObject;
                    rootObject = Utils.getLaunches(10);

                    foreach (var launch in rootObject.launches) {
                        DateTime launchTime = Utils.parseDate(launch.net);
                        TimeSpan remaining = launchTime.Subtract(DateTime.UtcNow);

                        if (launchTime > DateTime.UtcNow)
                        {
                            foreach (ulong channelID in channelIDs)
                            {
                                if (remaining.Days == 1)
                                {
                                    Console.WriteLine("Tested");
                                    await _client.GetChannel(154309859238477824).SendMessage("Tested");
                                }
                            }
                        }                        
                    }
                    await Task.Delay(10000);
                }
            });

        }


    }
}
