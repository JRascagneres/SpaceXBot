using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Modules;
using Discord.Commands;
using Discord.Commands.Permissions.Levels;

namespace SpaceXBot.Modules
{
    class StandardModule : IModule
    {
        private ModuleManager _m;
        private DiscordClient _client;

        public void Install(ModuleManager manager)
        {
            _m = manager;
            _client = manager.Client;

            _m.CreateCommands("", g =>
            {
                g.CreateCommand("ping")
                .Description("Ping")
                .Do(async e =>
                {
                    await e.Channel.SendMessage("pong!");
                });

                g.CreateCommand("kill")
                .Description("Threaten The Bot!")
                .Do(async e =>
                {
                    await e.Channel.SendMessage("Sad Beep!");
                });

                g.CreateCommand("dotheharlemshake")
                .Description("Derp?")
                .Do(async e =>
                {
                    await e.Channel.SendMessage("NO!");
                });

                g.CreateCommand("echo")
                .Description("Make the bot speak to you!")
                .Parameter("text", ParameterType.Unparsed)
                .Do(async e =>
                {
                    if(e.GetArg("text") == "")
                    {
                        await e.Channel.SendMessage("Echo what?");
                        return;
                    }
                    await e.Channel.SendMessage(e.Args[0]);
                });
            });
        }
    }
}
