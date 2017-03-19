using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceXBot.Core
{
    public class Launch
    {
        public int id { get; set; }
        public string name { get; set; }
        public string net { get; set; }
        public int tbdtime { get; set; }
        public int tbddate { get; set; }
    }

    public class RootObject
    {
        public int total { get; set; }
        public List<Launch> launches { get; set; }
        public int offset { get; set; }
        public int count { get; set; }
    }
}
