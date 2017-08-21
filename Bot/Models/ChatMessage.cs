using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Models
{
    [Serializable]
    public class ChatMessage
    {
        public string Message { get; set; }
        public string From { get; set; }      
        public DateTime TimeStamp { get; set; }
    }
}
