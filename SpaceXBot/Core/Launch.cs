﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceXBot.Core
{
    public class Launch
    {
        public String Vehicle { get; set; }
        public String Payload { get; set; }
        public DateTime Time { get; set; }
        public String ScrubReason { get; set; }
        public int Id { get; set; }
    }

    public class RootObject
    {
        public int total { get; set; }
        public List<Launch> launches { get; set; }
        public int offset { get; set; }
        public int count { get; set;}
    }
}
