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

namespace SpaceXBot.Modules
{
    class CountdownModule : IModule
    {
        private ModuleManager _m;
        private DiscordClient _client;
        private List<Launch> _launches;
        private string fileName = "launches.json";

        private ulong[] channelIDs = SpaceXBot.Core.SpxBot.Config.CountdownChannelIDs;

        public void Install(ModuleManager manager)
        {
            _m = manager;
            _client = manager.Client;

            _launches = LoadLaunches(fileName);

            _m.CreateCommands("", g =>
            {
                g.MinPermissions((int)PermissionLevel.User);

                g.CreateCommand("addLaunch")
                .Description("Command to add launch manually")
                .MinPermissions((int)PermissionLevel.BotManager)
                .Parameter("vehicle", ParameterType.Required)
                .Parameter("payload", ParameterType.Required)
                .Parameter("time", ParameterType.Required)
                .Do(async e =>
                {
                    if (e.GetArg("vehicle") == "" || e.GetArg("payload") == "" || e.GetArg("time") == "")
                    {
                        await e.Channel.SendMessage("Please Enter: <vehicle> <payload> <\"dd/MM/yyyy HH:mm:ss \">");
                        return;
                    }

                    DateTime time = DateTime.ParseExact(e.GetArg("time"), "dd/MM/yyyy HH:mm:ss", null);
                    await e.Channel.SendMessage("Launch Of: " + e.GetArg("vehicle") + " Carrying: " + e.GetArg("payload") + " --- Countdown Set For: " + time.ToString("dd/MM/yyyy") + " " + time.ToString("HH:mm:ss") + " UTC");

                    _launches.Add(
                        new Launch
                        {
                            Vehicle = e.GetArg("vehicle"),
                            Payload = e.GetArg("payload"),
                            Time = time,
                            Id = _launches.Any() ? _launches.Max(x => x.Id) + 1 : 0
                        });

                    if (_launches.Any())
                        _launches = _launches.OrderBy(x => x.Time).ToList();

                    await JsonStorage.SerializeObjectToFile(_launches, fileName);
                });

                g.CreateCommand("listLaunches")
                .Description("Prints out saved countdowns / launches")
                .MinPermissions((int)PermissionLevel.User)
                .Do(async e =>
                {
                    if (!_launches.Where(x => x.Time > DateTime.UtcNow).Any())
                    {
                        await e.Channel.SendMessage("No upcoming launches stored!");
                        return;
                    }
                    await e.Channel.SendMessage("\n" + string.Join("\n", _launches.Where(x => x.Time > DateTime.UtcNow).Select(x =>
                        String.Concat($"**Vehicle:** {x.Vehicle} | **Payload:** {x.Payload} | ",
                        String.IsNullOrWhiteSpace(x.ScrubReason)
                            ? $"**Time:** {x.Time.ToString("dd/MM/yyyy")} {x.Time.ToString("HH:mm")} UTC"
                            : $"**Scrub Reason:** {x.ScrubReason}"))));
                });

                g.CreateCommand("pastlaunches")
                .Description("Prints out past launches")
                .MinPermissions((int)PermissionLevel.User)
                .Do(async e =>
                {
                    if (!_launches.Where(x => x.Time < DateTime.UtcNow).Any())
                    {
                        await e.Channel.SendMessage("No Passed launches");
                        return;
                    }
                    string output = (" \n" + string.Join("\n", _launches.Where(x => x.Time < DateTime.UtcNow).Select(x =>
                        String.Concat($"**Vehicle:** {x.Vehicle} | **Payload:** {x.Payload} | ",
                        String.IsNullOrWhiteSpace(x.ScrubReason)
                            ? $"**Time:** {x.Time.ToString("dd/MM/yyyy")} {x.Time.ToString("HH:mm")} UTC"
                            : $"**Scrub Reason:** {x.ScrubReason}"))));

                    output = output.Substring(Math.Max(0, output.Length - 1997));
                    if (output.Length == 1997)
                    {
                        output = "..." + output;
                    }

                    await e.Channel.SendMessage(output);
                });

                g.CreateCommand("scrubs")
                .Description("Prints out scrubbed launches")
                .MinPermissions((int)PermissionLevel.User)
                .Do(async e =>
                {
                    if (!_launches.Where(x => !String.IsNullOrWhiteSpace(x.ScrubReason)).Any())
                    {
                        await e.Channel.SendMessage("No past scrubbed launches stored!");
                        return;
                    }
                    await e.Channel.SendMessage("\n" + string.Join("\n", _launches.Where(x => !String.IsNullOrWhiteSpace(x.ScrubReason)).Select(x => $"**Vehicle:** {x.Vehicle} | **Payload:** {x.Payload} | **Scrub Reason:** {x.ScrubReason}")));
                });

                g.CreateCommand("nextLaunch")
                .Description("Displays next launch information")
                .MinPermissions((int)PermissionLevel.User)
                .Do(async e =>
                {
                    int count = 0;
                    Boolean isLaunch = false;
                    do
                    {
                        if (DateTime.UtcNow < _launches[count].Time && _launches[count].ScrubReason == null)
                        {
                            TimeSpan remaining = _launches[count].Time.Subtract(DateTime.UtcNow);
                            String outputRemaining = remaining.Days + " Days " + remaining.Hours + "h" + remaining.Minutes + "m ";
                            String output = _launches[count].Vehicle + " launching " + _launches[count].Payload + " in " + outputRemaining;
                            await e.Channel.SendMessage(output);
                            isLaunch = true;
                        }
                        count = count + 1;
                    } while (!isLaunch);
                    if (!isLaunch)
                    {
                        await e.Channel.SendMessage("No launches available!");
                    }
                });

                g.CreateCommand("scrublaunch")
                .Description("Marks a launch as scrubbed with the reason provided")
                .MinPermissions((int)PermissionLevel.BotManager)
                .Parameter("number", ParameterType.Required)
                .Parameter("reason", ParameterType.Required)
                .Do(async e =>
                {
                    int number = (Int32.Parse(e.GetArg("number")) - 1);

                    if(_launches.Where(x => x.Time > DateTime.UtcNow).Count() < number + 1 || number < 0)
                    {
                        await e.Channel.SendMessage("Invalid index!");
                        return;
                    }

                    Launch launch = _launches.Where(x => x.Time > DateTime.UtcNow).ToArray()[number];

                    launch.ScrubReason = e.GetArg("reason");

                    _launches = _launches.OrderBy(x => x.Id).ToList();

                    await JsonStorage.SerializeObjectToFile(_launches, fileName);

                    await e.Channel.SendMessage("Launch data updated!");
                });

                g.CreateCommand("deleteLaunch")
                .Description("Delete Launch")
                .MinPermissions((int)PermissionLevel.BotManager)
                .Parameter("number", ParameterType.Required)
                .Do(async e =>
                {
                    int number = (Int32.Parse(e.GetArg("number")) - 1);

                    if (_launches.Where(x => x.Time > DateTime.UtcNow).Count() < number + 1 || number < 0)
                    {
                        await e.Channel.SendMessage("Invalid index!");
                        return;
                    }

                    Launch launch = _launches.Where(x => x.Time > DateTime.UtcNow).ToArray()[number];

                    _launches.Remove(launch);
                    _launches = _launches.OrderBy(x => x.Time).ToList();

                    await JsonStorage.SerializeObjectToFile(_launches, fileName);
                    await e.Channel.SendMessage("Deleted Launch!");
                });

                g.CreateCommand("editlaunch")
                .Description("Edit launch info")
                .MinPermissions((int)PermissionLevel.BotManager)
                .Parameter("number", ParameterType.Required)
                .Parameter("vehicle", ParameterType.Required)
                .Parameter("payload", ParameterType.Required)
                .Parameter("time", ParameterType.Required)
                .Do(async e =>
                {
                    DateTime time = DateTime.ParseExact(e.GetArg("time"), "dd/MM/yyyy HH:mm:ss", null);

                    int number = (Int32.Parse(e.GetArg("number")) - 1);

                    if (_launches.Where(x => x.Time > DateTime.UtcNow).Count() < number + 1 || number < 0)
                    {
                        await e.Channel.SendMessage("Invalid index!");
                        return;
                    }

                    Launch launch = _launches.Where(x => x.Time > DateTime.UtcNow).ToArray()[number];

                    String vehicle = e.GetArg("vehicle");
                    String payload = e.GetArg("payload");

                    _launches.First(x => x.Id == launch.Id).Vehicle = vehicle;
                    _launches.First(x => x.Id == launch.Id).Payload = payload;
                    _launches.First(x => x.Id == launch.Id).Time = time;

                    _launches = _launches.OrderBy(x => x.Time).ToList();

                    await JsonStorage.SerializeObjectToFile(_launches, fileName);

                    await e.Channel.SendMessage("Launch data updated");
                });

            });

            Task.Run(async () =>
           {
               while (true)
               {
                   int launchCount = 0;
                   foreach (var launch in _launches)
                   {
                       TimeSpan remaining = _launches[launchCount].Time.Subtract(DateTime.UtcNow);
                       if(_launches[launchCount].Time > DateTime.UtcNow)
                       {
                           if ((remaining.Hours == 6 || remaining.Hours == 3 || remaining.Hours == 2 || remaining.Hours == 1) && remaining.Days == 0 && remaining.Minutes == 0 && remaining.Seconds == 0)
                           {
                               foreach (ulong channelID in channelIDs)
                               {
                                   await _client.GetChannel(channelID).SendMessage("**" + _launches[launchCount].Vehicle + " Launching in " + remaining.Hours.ToString() + "Hours **");
                               }
                           } else if ((remaining.Minutes == 30 || remaining.Minutes == 15 || remaining.Minutes == 10 || remaining.Minutes == 5 || remaining.Minutes == 2) && remaining.Hours == 0 && remaining.Days == 0 && remaining.Seconds == 0)
                           {
                               foreach (ulong channelID in channelIDs)
                               {
                                   await _client.GetChannel(channelID).SendMessage("**" + _launches[launchCount].Vehicle + " Launching in " + remaining.Minutes.ToString() + "Minutes **");
                               }
                           } else if (remaining.Minutes == 0 && remaining.Hours == 0 && remaining.Days == 0 && remaining.Seconds == 0)
                           {
                               foreach (ulong channelID in channelIDs)
                               {
                                   await _client.GetChannel(channelID).SendMessage("**" + $"T-0 for {_launches[launchCount].Vehicle} carrying {_launches[launchCount].Payload}!!**");
                               }
                           }
                       }
                       launchCount += 1;
                   }
                   await Task.Delay(1000 * 1);
               }
           });
        }



        private List<Launch> LoadLaunches(string path)
        {
            if (!File.Exists(path))
                return new List<Launch>();
            else
                return JsonStorage.DeserializeObjectFromFile<List<Launch>>(path);
        }
    }
}
