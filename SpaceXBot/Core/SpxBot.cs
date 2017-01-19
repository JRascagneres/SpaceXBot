using Discord;
using Discord.Commands;
using Discord.Modules;
using Discord.Commands.Permissions.Levels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceXBot.Modules;
using System.IO;
using SpaceXBot.Config;

namespace SpaceXBot.Core
{
    public class SpxBot
    {
        private static DiscordClient client;
        public static BotConfig Config;
        private static CommandService commands;

        public SpxBot()
        {
            StreamReader sr = new StreamReader("./config.json");
            string jsonConfig = sr.ReadToEnd();
            sr.Close();

            Config = BotConfig.CreateFromJson(jsonConfig);

            client = new DiscordClient(x =>
            {
                x.AppName = Config.AppName;
                x.AppUrl = Config.AppUrl;
                x.LogLevel = LogSeverity.Info;
                x.LogHandler = OnLogMessage;
                x.UsePermissionsCache = false;
                x.AppVersion = Config.AppVersion.ToString();
            })
            .UsingModules()
            .UsingCommands(x =>
            {
                x.HelpMode = HelpMode.Public;
                x.AllowMentionPrefix = true;
                x.PrefixChar = '.';
                x.ErrorHandler = OnCommandError;
            })
            .UsingPermissionLevels(PermissionResolver);

            client.AddModule<StandardModule>("Standard");
            client.AddModule<RedditModule>("Reddit");
            client.AddModule<CountdownModule>("Countdown");

            client.ExecuteAndWait(async () =>
            {
                while (true)
                {
                    try
                    {
                        await client.Connect(Config.botID, TokenType.Bot);
                        //client.SetGame("Monitoring Chat");
                        break;
                    }
                    catch (Exception ex)
                    {
                        client.Log.Error($"Login Failed", ex);
                        await Task.Delay(client.Config.FailedReconnectDelay);
                    }
                }

            });
        }

        private static void OnLogMessage(object sender, LogMessageEventArgs e)
        {
            //Color
            ConsoleColor color;
            switch (e.Severity)
            {
                case LogSeverity.Error: color = ConsoleColor.Red; break;
                case LogSeverity.Warning: color = ConsoleColor.Yellow; break;
                case LogSeverity.Info: color = ConsoleColor.White; break;
                case LogSeverity.Verbose: color = ConsoleColor.Gray; break;
                case LogSeverity.Debug: default: color = ConsoleColor.DarkGray; break;
            }

            //Exception
            string exMessage;
            Exception ex = e.Exception;
            if (ex != null)
            {
                while (ex is AggregateException && ex.InnerException != null)
                    ex = ex.InnerException;
                exMessage = ex.Message;
            }
            else
                exMessage = null;

            //Source
            string sourceName = e.Source?.ToString();

            //Text
            string text;
            if (e.Message == null)
            {
                text = exMessage ?? "";
                exMessage = null;
            }
            else
                text = e.Message;

            //Build message
            StringBuilder builder = new StringBuilder(text.Length + (sourceName?.Length ?? 0) + (exMessage?.Length ?? 0) + 5);
            if (sourceName != null)
            {
                builder.Append('[');
                builder.Append(sourceName);
                builder.Append("] ");
            }
            for (int i = 0; i < text.Length; i++)
            {
                //Strip control chars
                char c = text[i];
                if (!char.IsControl(c))
                    builder.Append(c);
            }
            if (exMessage != null)
            {
                builder.Append(": ");
                builder.Append(exMessage);
            }

            text = builder.ToString();
            Console.ForegroundColor = color;
            Console.WriteLine(text);
        }

        private static void OnCommandError(object sender, CommandErrorEventArgs e)
        {
            string msg = e.Exception?.GetBaseException().Message;
            if (msg == null) //No exception - show a generic message
            {
                switch (e.ErrorType)
                {
                    case CommandErrorType.Exception:
                        msg = "Unknown error.";
                        break;
                    case CommandErrorType.BadPermissions:
                        msg = "You do not have permission to run this command.";
                        break;
                    case CommandErrorType.BadArgCount:
                        msg = "You provided the incorrect number of arguments for this command.";
                        break;
                    case CommandErrorType.InvalidInput:
                        msg = "Unable to parse your command, please check your input.";
                        break;
                    case CommandErrorType.UnknownCommand:
                        //msg = "Unknown command.";
                        break;
                }
            }
            if (msg != null)
            {
                e.Channel.SendMessage($"Error: {msg}");
                client.Log.Error("Command", msg);
            }
        }

        public static int PermissionResolver(User user, Channel channel)
        {
            if (user.Server != null)
            {
                if (user == channel.Server.Owner)
                    return (int)PermissionLevel.ServerOwner;

                var serverPerms = user.ServerPermissions;
                if (serverPerms.ManageRoles)
                    return (int)PermissionLevel.ServerAdmin;
                if (serverPerms.ManageMessages && serverPerms.KickMembers && serverPerms.BanMembers)
                    return (int)PermissionLevel.ServerModerator;

                if (user.Roles.Any(x => x.Name.ToLower() == "bot manager"))
                    return (int)PermissionLevel.BotManager;

                var channelPerms = user.GetPermissions(channel);
                if (channelPerms.ManagePermissions)
                    return (int)PermissionLevel.ChannelAdmin;
                if (channelPerms.ManageMessages)
                    return (int)PermissionLevel.ChannelModerator;
            }
            return (int)PermissionLevel.User;
        }
    }
}
