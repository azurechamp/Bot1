using System;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Bot.GlobalVars;
using Bot.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;

namespace Bot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        #region variables
        VerificationObject _retainedObj;
        #endregion


        #region GenericMethods
        private static async Task<string> ParseJson(string url)
        {
            var client = new HttpClient();
            var response = await client.GetAsync(url);
            var jsonString = response.Content.ReadAsStringAsync().Result;
            return jsonString;
        }
        #endregion

        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
            if (activity != null && activity.Text.ToLower().Equals("loan"))
            {

                await context.PostAsync(
                    "Welcome to the turboLoan Car Loan Application.  For Help, enter HELP at any time. For a complete list of terms and conditions, please click: https://short.bi/YRTRHDHD");
                await context.PostAsync("Please enter in the amount you wish to borrow?");

                context.Wait(LoanAmountReceivedAsync);
            }
            else
            {
                await context.PostAsync("Send \"Loan\" to this number to get started!");
                context.Wait(MessageReceivedAsync);
            }
        }

        private async Task LoanAmountReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
            if (activity != null && activity.Text.ToLower().Contains("help"))
            {
                await context.PostAsync("HELP – This screen TERMS – for complete terms and conditions RESET – Start over \n\nFor complete help please click the link below: Https://short.bi/HELPMENOW \n\n© 2017 Visionet Systems \n\nwww.visionetsystems.com");
                context.Wait(LoanAmountReceivedAsync);
            }
            if (activity != null && activity.Text.ToLower().Contains("terms"))
            {
                await context.PostAsync("A complete list of the terms and conditions can be found at turnLoans.com. Or clicking the link below: https://short.bi/UFHFHF \n\n© 2017 Visionet Systems \n\nwww.visionetsystems.com");
                context.Wait(LoanAmountReceivedAsync);
            }
            
            if (activity != null && (activity.Text.ToLower().Contains("help") || activity.Text.ToLower().Contains("terms")))
            {
                //implementation left
            }
            else
            { 
                try
                {
                    if (activity != null)
                    {
                        decimal amount = decimal.Parse(activity.Text, NumberStyles.Currency); 
                        ChatModel.LoanAmout = amount;
                    }
                    await context.PostAsync("Is this for a New or Used car?");
                    context.Wait(TypeOfCarReceivedAsync);
                }
                catch (FormatException)
                {
                    await context.PostAsync("Please enter a valid value !");
                    context.Wait(LoanAmountReceivedAsync);
                }
            }
        }

       

        private async Task TypeOfCarReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            string responsePostUrl = await ParseJson(UrlEndpoints.GetUrlEndpoint);
            //Needs Refactoring 

            var activity = await result as Activity;
            if (activity != null && activity.Text.ToLower().Contains("help"))
            {
                await context.PostAsync("HELP – This screen TERMS – for complete terms and conditions RESET – Start over \n\nFor complete help please click the link below: Https://short.bi/HELPMENOW \n\n© 2017 Visionet Systems \n\nwww.visionetsystems.com");
                context.Wait(TypeOfCarReceivedAsync);
            }
            if (activity != null && activity.Text.ToLower().Contains("terms"))
            {
                await context.PostAsync("A complete list of the terms and conditions can be found at turnLoans.com. Or clicking the link below: https://short.bi/UFHFHF \n\n© 2017 Visionet Systems \n\nwww.visionetsystems.com");
                context.Wait(TypeOfCarReceivedAsync);
            }
            

            if (activity != null && activity.Text.ToLower().Equals("used"))
            {
                ChatModel.CarType = "used";
                await context.PostAsync($"Please click the link below to upload a photo of your drivers license {responsePostUrl}");
                await context.PostAsync("When you are done, press 1 and send!");
                context.Wait(CheckForVarification);

            }
            else if (activity != null && activity.Text.ToLower().Equals("new"))
            {
                ChatModel.CarType = "new";
                await context.PostAsync($"Please click the link below to upload a photo of your drivers license {responsePostUrl}");
                await context.PostAsync("When you are done, press 1 and send!");
                context.Wait(CheckForVarification);
                //MessageReceivedAsync

            }
            else
            {
                if (activity != null && (activity.Text.ToLower().Contains("help") || activity.Text.ToLower().Contains("terms"))) { }
                else
                {
                    await context.PostAsync("Please enter New/Used");
                    context.Wait(TypeOfCarReceivedAsync);
                }
            }
        }
        
        private async Task CheckForVarification(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
            if (activity != null)
                foreach (var attachment in activity.Attachments)
                {
                    var unused = attachment;
                }

            string jsonString = await ParseJson(UrlEndpoints.ValidationUrl);
            var res = JsonConvert.DeserializeObject<VerificationObject>(jsonString);

            _retainedObj = res;

            Thread.Sleep(1000);

            if (_retainedObj.License.Verified.Equals("Yes"))
            {
                await context.PostAsync($"Hello {_retainedObj.License.Name},  first we need to verify your identity. This is for your own protection. Please answer the following questions.");
                if (_retainedObj.License.Question1 != null)
                    await context.PostAsync($"Q:{_retainedObj.License.Question1.question}");
                context.Wait(FirstQuestionAnswer);
            }
            else if (_retainedObj.License.Verified.Equals("No"))
            {
                await context.PostAsync("Unable to validate drivers license. Please upload a new photo with minimal glare and fits to the picture area");
                await context.PostAsync("When you are done, press 1 and send!");
                context.Wait(CheckForVarification);
            }


        }

       

        private async Task FirstQuestionAnswer(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
            if (activity != null && activity.Text.ToLower().Contains("help"))
            {
                await context.PostAsync("HELP – This screen TERMS – for complete terms and conditions RESET – Start over \n\nFor complete help please click the link below: Https://short.bi/HELPMENOW \n\n© 2017 Visionet Systems \n\nwww.visionetsystems.com");
                context.Wait(FirstQuestionAnswer);
            }
            if (activity != null && activity.Text.ToLower().Contains("terms"))
            {
                await context.PostAsync("A complete list of the terms and conditions can be found at turnLoans.com. Or clicking the link below: https://short.bi/UFHFHF \n\n© 2017 Visionet Systems \n\nwww.visionetsystems.com");
                context.Wait(FirstQuestionAnswer);
            }
            //if (activity.Text.ToLower().Contains("reset"))
            //{
            //    context.Wait(MessageReceivedAsync);
            //}
            if (activity != null && activity.Text.ToLower().Equals(_retainedObj.License.Question1.answer.ToLower()))
            {
                ChatModel.Answer1= true;
                await context.PostAsync($"Q:{_retainedObj.License.Question2.question}");
                context.Wait(SecondQuestionAnswer);

            }
            else if (activity != null && activity.Text.ToLower().Equals("exit"))
            {
                context.Reset();
            }
            else
            {
                if (activity != null && (activity.Text.ToLower().Contains("help") || activity.Text.ToLower().Contains("terms")))
                {
                    //implenationt missing yet
                }
                else
                {
                    ChatModel.Answer2 = false;
                    context.Wait(SecondQuestionAnswer);
                }
               
            }

        }

        private async Task SecondQuestionAnswer(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
            if (activity != null && activity.Text.ToLower().Contains("help"))
            {
                await context.PostAsync("HELP – This screen TERMS – for complete terms and conditions RESET – Start over \n\nFor complete help please click the link below: Https://short.bi/HELPMENOW \n\n© 2017 Visionet Systems \n\nwww.visionetsystems.com");
                context.Wait(SecondQuestionAnswer);
            }
            if (activity != null && activity.Text.ToLower().Contains("terms"))
            {
                await context.PostAsync("A complete list of the terms and conditions can be found at turnLoans.com. Or clicking the link below: https://short.bi/UFHFHF \n\n© 2017 Visionet Systems \n\nwww.visionetsystems.com");
                context.Wait(SecondQuestionAnswer);
            }
            //if (activity.Text.ToLower().Contains("reset"))
            //{
            //    context.Wait(MessageReceivedAsync);
            //}
            if (activity != null && activity.Text.ToLower().Equals(_retainedObj.License.Question2.answer.ToLower()))
            {
                ChatModel.Answer2 = true;

                if (ChatModel.Answer1 && ChatModel.Answer2)
                {
                   
                    if (ChatModel.CarType.ToLower().Equals("used"))
                    {
                        await context.PostAsync("Please enter the year of the vehicle?");
                        context.Wait(YearOfVehicle);
                        //year of vehicle
                    }
                    if (ChatModel.CarType.ToLower().Equals("new"))
                    {

                        await context.PostAsync("Please enter length of loan term in year(s). For see multiple term periods please separate with commas.  (example: 3,4,5) To receive all loan offers text ALL?");
                        context.Wait(LoanTerms);
                        //loan terms
                      
                    }
                    
                }
                else
                {
                    await context.PostAsync("Sorry, we were unable to verify your identity at this time. Please try again, or contact 412-298-7108 to confirm your identity.");
                    context.Reset();
                }
                //Move it to bank offers

            }
            else if (activity != null && activity.Text.ToLower().Equals("exit"))
            {
                context.Reset();
            }
            else
            {
                if (activity != null && (activity.Text.ToLower().Contains("help") || activity.Text.ToLower().Contains("terms"))) { }
                else
                {
                    await context.PostAsync("Sorry, we were unable to verify your identity at this time. Please try again, or contact 412-298-7108 to confirm your identity.");
                    context.Reset();
                }
            }

            
            
        }

        private async Task LoanTerms(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
            

            if (activity != null && activity.Text.ToLower().Contains("help"))
            {
                await context.PostAsync("HELP – This screen TERMS – for complete terms and conditions RESET – Start over \n\nFor complete help please click the link below: Https://short.bi/HELPMENOW \n\n© 2017 Visionet Systems \n\nwww.visionetsystems.com");
                context.Wait(LoanTerms);
            }
            if (activity != null && activity.Text.ToLower().Contains("terms"))
            {
                await context.PostAsync("A complete list of the terms and conditions can be found at turnLoans.com. Or clicking the link below: https://short.bi/UFHFHF \n\n© 2017 Visionet Systems \n\nwww.visionetsystems.com");
                context.Wait(LoanTerms);
            }
            //if (activity.Text.ToLower().Contains("reset"))
            //{
            //    context.Wait(MessageReceivedAsync);
            //}
            if (activity != null && (activity.Text.Equals("3") || activity.Text.Equals("4") || activity.Text.Equals("5")))
            {
                await context.PostAsync("Obtain Quotes From Lenders \n\n(Lender selection based upon loan type, state, amount)");
                await context.PostAsync($"Congratulations {_retainedObj.License.Name}.  Here are your pre-approved offers for a new car loan in the amount of ${ChatModel.LoanAmout}");
                await context.PostAsync("PNC Bank \n\n1) 36 Months 4.875%=  $438.12 \n\n2) 48 Months 4.875% = $412.12 \n\n3) 60 Months 4.989%  = $395.60 \n\nHuntington \n\n4) 36 Months 4.875%=  $438.12 \n\n5) 48 Months 4.875% = $412.12 \n\n6) 60 Months 4.989%  = $395.60 \n\nChase \n\n7) 36 Months 4.875%=  $438.12 \n\n8) 48 Months 4.875% = $412.12 \n\n9) 60 Months 4.989%  = $395.60");
                await context.PostAsync("To see shorter terms, enter SHORTER To see longer terms, enter LONGER To change the loan amount  please enter in a new Loan Amount.\n\nOtherwise please select one  of the offers above to receive your preapproval authorization code.");

                context.Wait(BankOffers);
            }

        }
       
        private async Task YearOfVehicle(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
            if (activity != null && activity.Text.ToLower().Contains("help"))
            {
                await context.PostAsync("HELP – This screen TERMS – for complete terms and conditions RESET – Start over \n\nFor complete help please click the link below: Https://short.bi/HELPMENOW \n\n© 2017 Visionet Systems \n\nwww.visionetsystems.com");
                context.Wait(YearOfVehicle);
            }
            if (activity != null && activity.Text.ToLower().Contains("terms"))
            {
                await context.PostAsync("A complete list of the terms and conditions can be found at turnLoans.com. Or clicking the link below: https://short.bi/UFHFHF \n\n© 2017 Visionet Systems \n\nwww.visionetsystems.com");
                context.Wait(YearOfVehicle);
            }
            //if (activity.Text.ToLower().Contains("reset"))
            //{
            //    context.Wait(MessageReceivedAsync);
            //}

            if (activity != null && (activity.Text.Equals("2012") || activity.Text.Equals("2013") || activity.Text.Equals("2014") || activity.Text.Equals("2015") || activity.Text.Equals("2016") || activity.Text.Equals("2017") || activity.Text.Equals("2018")))
            {
                ChatModel.YearOfVehicle = activity.Text;
                await context.PostAsync("Please enter length of loan term in year(s). For see multiple term periods please separate with commas.  (example: 3,4,5) To receive all loan offers text ALL?");
                context.Wait(LoanTerms);
            }
            else
            {
                if (activity != null && (activity.Text.ToLower().Contains("help") || activity.Text.ToLower().Contains("terms"))) { }
                else
                {
                    await context.PostAsync("Please Enter valid year, (2012-2018)");
                    context.Wait(YearOfVehicle);
                }
            }
        }

        
        private async Task BankOffers(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
            if (activity != null && (activity.Text.Equals("1") || activity.Text.Equals("2") || activity.Text.Equals("3") || activity.Text.Equals("4") || activity.Text.Equals("5") || activity.Text.Equals("6") || activity.Text.Equals("7") || activity.Text.Equals("8") || activity.Text.Equals("9")))
            {
                await context.PostAsync("Your Huntington Bank Authorization code is JWQTUR.  This offer is valid for 48 hours. Please click the link below to finish your application,  or provide this number to your dealer. Https://shirt.li/NORMG123");
                context.Wait(MessageReceivedAsync);
            }
            else
            {
                context.Wait(MessageReceivedAsync);
            }
        }
        

        }
    }