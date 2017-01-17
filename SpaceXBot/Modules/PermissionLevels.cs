using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceXBot.Modules
{
    public enum PermissionLevel
    {
        User,
        BotManager,
        ChannelModerator,
        ChannelAdmin,
        ServerModerator,
        ServerAdmin,
        ServerOwner,
        BotOwner
    }
}
