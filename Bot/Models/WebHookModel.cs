using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Models
{
    [Serializable]
    public class WebHookModel
    {
        public string Code { get; set; }
        public string  MessageText { get; set; }
    }
}
