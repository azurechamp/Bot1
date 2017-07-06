using System;

namespace Bot.Models
{
    [Serializable]
    public class Question1
    {
        public string question { get; set; }
        public string answer { get; set; }
    }

    [Serializable]
    public class Question2
    {
        public string question { get; set; }
        public string answer { get; set; }
    }

    [Serializable]
    public class License
    {
        public string Verified { get; set; }
        public string Name { get; set; }
        public Question1 Question1 { get; set; }
        public Question2 Question2 { get; set; }
    }

    [Serializable]
    public class VerificationObject
    {
        public License License { get; set; }
    }
}
