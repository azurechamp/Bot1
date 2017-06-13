using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Models
{

    [Serializable]
    public class ChatModel
    {

        public static string CarType { get; set; } = "";
        public static decimal LoanAmout { get; set; } = 0;
        public static bool Answer1 { get; set; } = false;
        public static bool Answer2 { get; set; } = false;
        public static string YearOfVehicle { get; set; } = "";
        public static string LoanTermYear { get; set; } = "";


    }
}
