using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Modules;
using RedditSharp;

namespace SpaceXBot.Modules
{
    class RedditModule : IModule
    {
        private ModuleManager _m;
        private DiscordClient _client;

        public void Install(ModuleManager manager)
        {
            var reddit = new Reddit();
            var user = reddit.LogIn("SPXBot", "ZT7v74Jtdh7rb2wUtkFtnRzuq723QS");
            var subreddit = reddit.GetSubreddit("/r/spacex");
#if DEBUG
            ulong[] channelIDs = SpaceXBot.Core.SpxBot.Config.RedditChannelDebugIDs;
#else
            ulong[] channelIDs = SpaceXBot.Core.SpxBot.Config.RedditChannelIDs;
#endif 

            int postCheck = 4;
            List<string> Ids = new List<string>() {};
            subreddit.Subscribe();

            _m = manager;
            _client = manager.Client;

            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        if (!Ids.Any())
                        {
                            foreach (var post in subreddit.New.Take(postCheck))
                            {
                                Ids.Add(post.Id);
                            }
                        }

                        foreach (var post in subreddit.New.Take(postCheck))
                        {
                            Console.WriteLine(post.Title);
                            if (Ids.Contains(post.Id) != true)
                            {
                                if (post.IsSelfPost)
                                {
                                    if (post.SelfText.Length > 1800)
                                    {
                                        foreach (ulong channelID in channelIDs)
                                        {
                                            await _client.GetChannel(channelID).SendMessage("\n **" + post.Title + "** by " + post.Author + "\n ** Text Too Long To Display! ** \n" + post.Shortlink);
                                        }
                                    }
                                    else {
                                        foreach (ulong channelID in channelIDs)
                                        {
                                            await _client.GetChannel(channelID).SendMessage("\n **" + post.Title + "** by " + post.Author + "\n" + post.SelfText + "\n" + post.Shortlink);
                                        }
                                    }
                                        Ids.Add(post.Id);
                                }
                                else
                                {
                                    foreach (ulong channelID in channelIDs)
                                    {
                                        await _client.GetChannel(channelID).SendMessage("\n **" + post.Title + "** by " + post.Author + "\n" + post.Shortlink);
                                    }
                                    Ids.Add(post.Id);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.Write(e.Message);
                        await Task.Delay(1000 * 60 * 2);
                    }
                    await Task.Delay(1000 * 60);
                }
            });
        }
    }
}
