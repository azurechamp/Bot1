using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.GlobalVars
{
    public class BotResponses
    {
        public static string WelcomeMessage { get; set; } =
            "Welcome to the turboLoan Car Loan Application.  For Help, enter HELP at any time. For a complete list of terms and conditions, please click: https://is.gd/soMHVR \n\nPlease enter in the amount you wish to borrow?";

        public static string InitiationPropmpt { get; set; } = "Send \"Loan\" to this number to get started!";

        public static string HelpText { get; set; } =
            "HELP – This screen TERMS – for complete terms and conditions RESET – Start over \n\nFor complete help please click the link below: Https://short.bi/HELPMENOW \n\n© 2017 Visionet Systems \n\nwww.visionetsystems.com";

        public static string TermsText { get; set; } =
            "A complete list of the terms and conditions can be found at turnLoans.com. Or clicking the link below: https://short.bi/UFHFHF \n\n© 2017 Visionet Systems \n\nwww.visionetsystems.com";

        public static string CarQuestionText { get; set; } = "Is this for a New or Used car?";

        public static string InvalidInputText { get; set; } = "Please enter a valid value !";

        public static string ImageUploadPromptText1 { get; set; } =
            "Please click the link below to upload a photo of your drivers license";

        public static string ImageUploadPromptText2 { get; set; } = "When you are done, press 1 and send!";

        public static string PreQuestionText { get; set; } =
            " first we need to verify your identity. This is for your own protection. Please answer the following questions.";

        public static string UnableToVerifyText { get; set; } =
            "Unable to validate drivers license. Please upload a new photo with minimal glare and fits to the picture area \n\nWhen you are done, press 1 and send!";


        public static string VehicleYearPromptText { get; set; } = "Please enter the year of the vehicle?";

        public static string LoanTermsPromptText { get; set; } =
            "Please enter length of loan term in year(s). For see multiple term periods please separate with commas.  (example: 3,4,5) To receive all loan offers text ALL?";

        public static string UnableToVerifyIdentity { get; set; } =
            "Sorry, we were unable to verify your identity at this time.Please try again, or contact 412-298-7108 to confirm your identity.";

        public static string trterms1 { get; set; } =
            "Obtain Quotes From Lenders \n\n(Lender selection based upon loan type, state, amount)";

        public static string termsPrompt { get; set; } =
            "Please enter length of loan term in year(s). For see multiple term periods please separate with commas.  (example: 3,4,5) To receive all loan offers text ALL?";

        public static string FinalMessage { get; set; } =
            "Your Huntington Bank Authorization code is JWQTUR.  This offer is valid for 48 hours. Please click the link below to finish your application,  or provide this number to your dealer. https://is.gd/soMHVR";
    }
}
