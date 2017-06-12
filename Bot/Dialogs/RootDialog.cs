using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Net.Http;
using Newtonsoft.Json;
using Bot.Models;
using System.Threading;

namespace Bot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        #region variables
        VerificationObject retainedObj;
        #endregion

        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
            await context.PostAsync("Hi There, Welcome to Visi Lending Service what's the amount you want to lend?");

            context.Wait(LoanAmountReceivedAsync);
        }

        private async Task LoanAmountReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
            try
            {
                int amount = Int32.Parse(activity.Text);
                await context.PostAsync("Ok Great! is that car Used/New ?");
                context.Wait(TypeOfCarReceivedAsync);
            }
            catch (FormatException exc)
            {
                await context.PostAsync("Please enter a valid amount!");
                context.Wait(LoanAmountReceivedAsync);
            }
           
        }

        private async Task TypeOfCarReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            if (activity.Text.ToLower().Equals("used"))
            {
                await context.PostAsync("So, it's a Used Car. Please click on the link below to upload picture of your driving license http://www.abc.com");
                //WebHookCall
                await context.PostAsync("When you are done, press 1 and send!");
                context.Wait(CheckForVarification);

            }
            else if (activity.Text.ToLower().Equals("new"))
            {
                await context.PostAsync("So, it's a New Car. Please click on the link below to upload picture of your driving license http://www.abc.com");
                await context.PostAsync("When you are done, press 1 and send!");
                context.Wait(CheckForVarification);
                //MessageReceivedAsync

            }
            else
            {
                await context.PostAsync("That doesn't seem to be a valid options, please enter (Used/New)");
                context.Wait(TypeOfCarReceivedAsync);
            }
        }
        
        private async Task CheckForVarification(IDialogContext context, IAwaitable<object> result)
        {

            var client = new HttpClient();
            var response = await client.GetAsync(GlobalVars.UrlEndpoints.ValidationUrl);
            var jsonString = response.Content.ReadAsStringAsync().Result;
            var res = JsonConvert.DeserializeObject<VerificationObject>(jsonString);

            retainedObj = res;
           
            Thread.Sleep(1000);

            if (retainedObj.License.Verified.Equals("Yes"))
            {
                await context.PostAsync($"Hello {retainedObj.License.Name}, Your license validity status is {retainedObj.License.Verified}. Please answer the questions below to verify your identity.");
                await context.PostAsync($"Q:{(retainedObj.License.Question1 as Question1).question}");
                context.Wait(FirstQuestionAnswer);
            }
            else if (retainedObj.License.Verified.Equals("No"))
            {
                await context.PostAsync(" Your license doesn't looks valid, Please click on the link below to upload picture of your driving license http://www.abc.com");
                await context.PostAsync("When you are done, press 1 and send!");
                context.Wait(CheckForVarification);
            }


        }

        private async Task FirstQuestionAnswer(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
            if (activity.Text.ToLower().Equals(retainedObj.License.Question1.answer.ToLower()))
            {
                await context.PostAsync("Great!");
                await context.PostAsync($"Q:{(retainedObj.License.Question2 as Question2).question}");

                context.Wait(SecondQuestionAnswer);

            }
            else if (activity.Text.ToLower().Equals("exit"))
            {
                context.Reset();
            }
            else
            {
                await context.PostAsync($"Q:{(retainedObj.License.Question1 as Question1).question}");
                context.Wait(FirstQuestionAnswer);
            }

        }

        private async Task SecondQuestionAnswer(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
            if (activity.Text.ToLower().Equals(retainedObj.License.Question2.answer.ToLower()))
            {
                await context.PostAsync("Great!");
                await context.PostAsync($"Here are your current offers. \nPNC 1) 48 Mnths 4.875% =$483.23\n2) 48 Mnths 4.875% =$483.23\n 3) 48 Mnths 4.875% =$483.23");

                context.Wait(BankOffers);
                //Move it to bank offers

            }
            else if (activity.Text.ToLower().Equals("exit"))
            {
                context.Reset();
            }
            else
            {
                await context.PostAsync($"Q:{(retainedObj.License.Question2 as Question2).question}");
                context.Wait(SecondQuestionAnswer);
            }
        }

        private async Task BankOffers(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
            await context.PostAsync("Your Bank authorization code is H4BH9. Please complete the application by following the link below \n http://ww.abc.com");

            context.Wait(MessageReceivedAsync);
        }






        }
    }