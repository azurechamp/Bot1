using System;

namespace Bot.Models
{

    [Serializable]
    public class ChatModel
    {

        public static string CarType { get; set; } = "";
        public static string CustomerId { get; set; }= "";
        public static string URL { get; set; } = "";
        public static decimal LoanAmout { get; set; } = 0;
        public static bool Answer1 { get; set; } = false;
        public static bool Answer2 { get; set; } = false;
        public static string YearOfVehicle { get; set; } = "";
        public static string LoanTermYear { get; set; } = "";


    }
}
