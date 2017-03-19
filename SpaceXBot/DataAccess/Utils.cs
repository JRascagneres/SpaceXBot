using SpaceXBot.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SpaceXBot.DataAccess
{
    class Utils
    {
        public static RootObject getLaunches(int count)
        {
            using (var webClient = new WebClient())
            {
                webClient.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36");
                var response = webClient.DownloadString("https://launchlibrary.net/1.2/launch?next=" + count);
                Console.WriteLine(response);
                return JsonStorage.DeserializeObject(response);
            }
        }

        public static DateTime parseDate(string input)
        {
            DateTime rs;
            bool successParse = DateTime.TryParseExact(input, "MMMM dd, yyyy HH:mm:ss UTC", CultureInfo.InvariantCulture, DateTimeStyles.None, out rs);
            if (successParse)
            {
                return DateTime.ParseExact(input, "MMMM dd, yyyy HH:mm:ss UTC", CultureInfo.InvariantCulture);
            }
            else
            {
                return DateTime.ParseExact(input, "MMMM d, yyyy HH:mm:ss UTC", CultureInfo.InvariantCulture);
            }
        }
    }
}
