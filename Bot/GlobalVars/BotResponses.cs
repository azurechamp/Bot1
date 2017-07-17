namespace Bot.GlobalVars
{
    public class BotResponses
    {
        public static string BotReset { get; set; } = "BOT RESET! Please start again!";
        public static string InvalidAmount { get; set; } = "Please enter NEW is this is a new car, or  USED if this is a request for a used car loan";
        public static string WelcomeMessage { get; set; } =
            "Welcome to the turboLoan Car Loan Application.  \n\nFor Help, enter HELP at any time. For a complete list of terms and conditions, please click: https://is.gd/soMHVR and follow along. \n\n \n\n";
        public static string LoanPrompt { get; set; } = "Please enter in the amount you wish to borrow?";

        public static string UnableToVerifyLicense { get; set; } =
            "Unfortunately due to unclear image or some other reason we are unable to verify your driving license \n\nPlease contact our support or try process again.";
        public  static string TermsHeader { get; set; } = "    Bank Name | Amount | Term | Rate \n\n";
        public static string ChangeTermsPrompt { get; set; } = "Please Enter new terms starting 1 year to 6 years \n\nShorter Terms\n\n Example: 1,2,3 \n\nLonger Terms \n\n Example: 4,5,6";
        public static string LoanShortLong { get; set; } =
            "To see shorter terms, enter SHORTER To see longer terms, enter LONGER.\n\nOtherwise please select one  of the offers above to receive your preapproval authorization code.";
        public static string InitiationPropmpt { get; set; } = "Send \"Loan\" to this number to get started!";

        public static string HelpText { get; set; } =
            "HELP – This screen TERMS – for complete terms and conditions RESET – Start over \n\nFor complete help please click the link below: Https://short.bi/HELPMENOW \n\n© 2017 Visionet Systems \n\nwww.visionetsystems.com";

        public static string TermsText { get; set; } =
            "A complete list of the terms and conditions can be found at turnLoans.com. Or clicking the link below: https://short.bi/UFHFHF \n\n© 2017 Visionet Systems \n\nwww.visionetsystems.com";

        public static string CarQuestionText { get; set; } = "Is this for a \"New\" or \"Used\" car?";

        public static string InvalidInputText { get; set; } = "Please enter a valid value !";

        public static string PreImageuploadUrlPrompt { get; set; } = "Please click on this url";
        public static string ImageUploadPromptText1 { get; set; } =
            " to upload a photo of the back of your drivers license, or simply send a photo via texting\n\n";

        public static string ImageUploadPromptText2 { get; set; } = "When you are done, press 1 and send!";

        public static string PreQuestionText { get; set; } =
            " first we need to verify your identity. This is for your own protection. Please answer the following questions.";

        public static string UnableToVerifyText { get; set; } =
            "Unable to validate drivers license. Please upload a new photo with minimal glare and fits to the picture area \n\nWhen you are done, press 1 and send!";

        public static string VehicleYearPromptText { get; set; } = "Please enter the year of the vehicle?";

        public static string LoanTermsPromptText { get; set; } =
            "Please enter the desired length of loan in year(s). \n\nFor multiple quotes please separate the years with commas.  (example: 3,4,5). \n\nTo receive all loan offers text ALL?";

        public static string UnableToVerifyIdentity { get; set; } =
            "Sorry, we were unable to verify your identity at this time.Please try again, or contact 412-298-7108 to confirm your identity.";

        public static string trterms1 { get; set; } =
            "Obtain Quotes From Lenders \n\n(Lender selection based upon loan type, state, amount)";

        public static string imageSuggestPrompt { get; set; } = "We request you to upload the image on url described above. \n\nOtherwise wait for the response from our verification server. ";
        public static string termsPrompt { get; set; } =
            "Please enter length of loan term in year(s). For multiple term periods please separate with commas.  (example: 3,4,5) To receive all loan offers text ALL?";

      
    }
}
