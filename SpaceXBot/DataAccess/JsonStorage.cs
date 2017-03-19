using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using SpaceXBot.Core;

namespace SpaceXBot.DataAccess
{
    class JsonStorage
    {
        public static async Task SerializeObjectToFile<T>(T obj, string filename)
        {
            while (true)
            {
                try
                {
                    using (var stream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None))
                    using (var writer = new StreamWriter(stream))
                        await writer.WriteAsync(JsonConvert.SerializeObject(obj, Formatting.Indented));
                    break;
                }
                catch(IOException)
                {
                    await Task.Delay(1000);
                }
            }
        }

        public static T DeserializeObjectFromFile<T>(string fileName)
        {
            return JsonConvert.DeserializeObject<T>(File.ReadAllText(fileName));
        }

        public static RootObject DeserializeObject(string json)
        {
            return JsonConvert.DeserializeObject<RootObject>(json);
        }

    }
}
