using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.GlobalVars
{
    [Serializable]
    public class UrlEndpoints
    {
        public static string ValidationUrl { get; set; } = "https://raw.githubusercontent.com/muhammad92/Sport/master/test.json";
    }
}
