using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bot.Commons;
using Bot.GlobalVars;
using Bot.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Newtonsoft.Json;

namespace Bot.Dialogs
{
    /// <summary>
    /// Class: Root Dialog
    /// 
    /// </summary>
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        #region definitions
        VerificationObject _retainedObj;
        readonly Queue<ChatMessage> _chatHistory = new Queue<ChatMessage>();
        private int bankId;
        #endregion

        #region GenericMethods


        private static async Task ImageUploadTask(Stream imageStream, string CustomerId)
        {
            
            string containerName = "license";

            // Retrieve storage account information from connection string
            CloudStorageAccount storageAccount = Common.CreateStorageAccountFromConnectionString();

            // Create a blob client for interacting with the blob service.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            Console.WriteLine("1. Creating Container");
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);
            try
            {
                BlobRequestOptions requestOptions = new BlobRequestOptions() { RetryPolicy = new NoRetry() };
                await container.CreateIfNotExistsAsync(requestOptions, null);
            }
            catch (StorageException)
            {
                Console.WriteLine("If you are running with the default connection string, please make sure you have started the storage emulator. Press the Windows key and type Azure Storage to select and run it from the list of applications - then restart the sample.");
                Console.ReadLine();
                throw;
            }

            
            Console.WriteLine("2. Uploading BlockBlob");
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(CustomerId);
            blockBlob.Properties.ContentType = "image/png";
            await blockBlob.UploadFromStreamAsync(imageStream);

        }



        /// <summary>
        /// Generic Method to Parse JSON for making code less redundant
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private static async Task<string> ParseJson(string url)
        {
            var jsonString = "";
            try
            {
                var client = new HttpClient();
                var response = await client.GetAsync(url);
                jsonString = response.Content.ReadAsStringAsync().Result;
            }catch(Exception e) { Console.WriteLine(e.Message); }
            return jsonString;
        }
        /// <summary>
        /// Adds message to history which ultimately packs it into the log and send it to API
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="from"></param>
        private void AddMessagetoHistory(string msg, string from)
        {
            _chatHistory.Enqueue(new ChatMessage { Message = msg, From = from });
        }
        /// <summary>
        /// Method for Validation of Bank Terms
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        private bool TermsValidation(Activity activity)
        {
            var validation = false;
            var termSplit = activity.Text.Split(',');
            foreach (var term in termSplit)
            {
                int value;
                Int32.TryParse(term, out value);
                if (value == 1 || value == 2 || value == 3 || value == 4 || value == 5 ||
                    value == 6)
                {
                    validation = true;
                }
                else
                {
                    validation = false;
                    break;
                }
            }
            return validation;
        }

        /// <summary>
        /// Saves the Log of user chate and push it to 
        /// API Server
        /// </summary>
        private void SaveandPushLog()
        {
            try
            {
                var historyString = JsonConvert.SerializeObject(_chatHistory);
                var base64HistoryString = Base64Encode(historyString);
                PushLog(base64HistoryString);
                _chatHistory.Clear();
            }catch(Exception e) {Console.WriteLine(e.Message); }
        }
        /// <summary>
        /// Base 64 Encode Implementation
        /// </summary>
        /// <param name="plainText"></param>
        /// <returns></returns>
        private string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
         
        }
        /// <summary>
        /// Final Prompt Implementation
        /// </summary>
        /// <param name="context"></param>
        /// <param name="jsonModel"></param>
        /// <returns></returns>
        private static async Task FinalStep(IDialogContext context, VerificationModel jsonModel)
        {

            //TODO: Add Bank Name
            await context.PostAsync($"Your {jsonModel.BankInfo} Bank Authorization code is {jsonModel.Code}.  This offer is valid for 48 hours. \n\nPlease click on the link {jsonModel.URL} to finish your application,  or provide this number to your dealer.\n\n");
        }
        /// <summary>
        /// Sends selected Terms of bank back to API and get
        /// Authorization Code for respective term and URL for 
        /// Confirmation.
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        private static async Task<VerificationModel> FinalVerification(Activity activity)
        {
            VerificationModel jsonModel = null;
            try
            {
                var response =
                    await ParseJson(
                        $"http://visiloanapi.azurewebsites.net/api/customer/VerifyCodeUrl?cid={ChatModel.CustomerId}&selectedPackage={activity.Text}");
               jsonModel  = JsonConvert.DeserializeObject<VerificationModel>(response);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return jsonModel;
        }
        /// <summary>
        /// Pushes Log 
        /// </summary>
        /// <param name="base64String"></param>
        private async void PushLog(string base64String)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(UrlEndpoints.BaseUrl);
                var content = new  FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("CustomerId", ChatModel.CustomerId), new KeyValuePair<string, string>("Text",base64String), 
                });
                var result = await client.PostAsync("/api/customer/PostConversation", content);
                string resultContent = await result.Content.ReadAsStringAsync();
                Console.WriteLine(resultContent);
            }
        }
        #endregion

        /// <summary>
        /// Starts the Bot
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
           

            return Task.CompletedTask;
        }

        /// <summary>
        /// 1st Step of Bot. 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
            if (activity != null && (activity.Text.ToLower().Equals("loan")|| activity.Text.ToLower().Equals("hello")))
            {
                await context.PostAsync(BotResponses.WelcomeMessage);
                AddMessagetoHistory(BotResponses.WelcomeMessage, "Bot");
                await context.PostAsync(BotResponses.LoanPrompt);
                AddMessagetoHistory(BotResponses.LoanPrompt, "Bot");

                context.Wait(LoanAmountReceivedAsync);
            }
            else
            {
                await context.PostAsync(BotResponses.InitiationPropmpt);
                context.Wait(MessageReceivedAsync);
            }
        }

        /// <summary>
        /// Connects when Loan Amount Received.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task LoanAmountReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
            AddMessagetoHistory(activity?.Text, "User");
            
            if (activity != null && activity.Text.ToLower().Contains("help"))
            {
                await context.PostAsync(BotResponses.HelpText);
                AddMessagetoHistory(BotResponses.HelpText, "Bot");
                context.Wait(LoanAmountReceivedAsync);
            }
            if (activity != null && activity.Text.ToLower().Contains("terms"))
            {
                await context.PostAsync(BotResponses.TermsText);
                AddMessagetoHistory(BotResponses.TermsText,"Bot");
                context.Wait(LoanAmountReceivedAsync);
            }
            
            if (activity != null && (activity.Text.ToLower().Contains("help") || activity.Text.ToLower().Contains("terms")))
            {
                //implementation left
            }
            else
            {
                if (activity != null && (activity.Text.ToLower().Equals("reset") ||
                                         activity.Text.ToLower().Equals("stop")))
                {
                    await context.PostAsync(BotResponses.BotReset);
                    context.Wait(MessageReceivedAsync);
                }
                else
                {
                    try
                    {
                        if (activity != null)
                        {
                            AddMessagetoHistory(activity.Text, "User");
                            decimal amount = decimal.Parse(activity.Text, NumberStyles.Currency);
                            ChatModel.LoanAmout = amount;
                        }
                        await context.PostAsync(BotResponses.CarQuestionText);
                        context.Wait(TypeOfCarReceivedAsync);
                    }
                    catch (FormatException)
                    {
                        await context.PostAsync(BotResponses.InvalidInputText);
                        context.Wait(LoanAmountReceivedAsync);
                    }
                }
            }
        }

        /// <summary>
        /// Connects when Type of Car
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task TypeOfCarReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            //Get Url Implementation

            string jsonResponse = await ParseJson(UrlEndpoints.GetUrlEndpoint);
            var jsonModel = JsonConvert.DeserializeObject<GetUrlObject>(jsonResponse);
            var url = jsonModel.url;
            ChatModel.CustomerId = jsonModel.CustomerId;
           
            
            
            var activity = await result as Activity;
            
            if (activity != null && activity.Text.ToLower().Contains("help"))
            {
                await context.PostAsync(BotResponses.HelpText);
                AddMessagetoHistory(BotResponses.HelpText, "Bot");
                context.Wait(TypeOfCarReceivedAsync);
            }
            if (activity != null && activity.Text.ToLower().Contains("terms"))
            {
                await context.PostAsync(BotResponses.TermsText);
                AddMessagetoHistory(BotResponses.TermsText,"Bot");
                context.Wait(TypeOfCarReceivedAsync);
            }
            

            if (activity != null && activity.Text.ToLower().Equals("used"))
            {

                AddMessagetoHistory(activity.Text, "User");
                ChatModel.CarType = "used";
                await context.PostAsync($"{BotResponses.PreImageuploadUrlPrompt} {url} {BotResponses.ImageUploadPromptText1}");
              //  await context.PostAsync(BotResponses.ImageUploadPromptText2);
                AddMessagetoHistory($"{BotResponses.PreImageuploadUrlPrompt} {url} {BotResponses.ImageUploadPromptText1}", "Bot");
                context.Wait(CheckForVarification);

            }
            else if (activity != null && activity.Text.ToLower().Equals("new"))
            {
                AddMessagetoHistory(activity.Text,"User");
                ChatModel.CarType = "new";
                await context.PostAsync($"{BotResponses.PreImageuploadUrlPrompt} {url} {BotResponses.ImageUploadPromptText1}");
                //await context.PostAsync(BotResponses.ImageUploadPromptText2);
                AddMessagetoHistory($"{BotResponses.PreImageuploadUrlPrompt} {url} {BotResponses.ImageUploadPromptText1}", "Bot");
                context.Wait(CheckForVarification);
                //MessageReceivedAsync

            }
            else
            {
                if (activity != null && (activity.Text.ToLower().Equals("reset") ||
                                         activity.Text.ToLower().Equals("stop")))
                {
                    await context.PostAsync(BotResponses.BotReset);
                    context.Wait(MessageReceivedAsync);
                    return;
                }
                if (activity != null && (activity.Text.ToLower().Contains("help") || activity.Text.ToLower().Contains("terms"))) { }
                else
                {
                    await context.PostAsync($"{BotResponses.InvalidAmount}" );
                    context.Wait(TypeOfCarReceivedAsync);
                }
            }
        }
        
        /// <summary>
        /// Connects for Verification.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task CheckForVarification(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
            bool success = false;
            if (activity != null && activity.Attachments != null)
                foreach (var attachment in activity.Attachments)
                {
                    var unused = attachment;
                    var attachmentUrl =
                        unused.ContentUrl;
                    var httpClient = new HttpClient();

                    var attachmentData =
                        await httpClient.GetByteArrayAsync(attachmentUrl);
                    Stream stream = new MemoryStream(attachmentData);
                    await ImageUploadTask(stream, ChatModel.CustomerId);
                    try
                    {

                        var request = (HttpWebRequest)WebRequest.Create("http://visiloanapi.azurewebsites.net/api/customer/PostImageData");

                        var postData = $"CustomerId={ChatModel.CustomerId}";
                        postData += $"&Url={UrlEndpoints.BlobBaseUrl}";
                        var data = Encoding.ASCII.GetBytes(postData);

                        request.Method = "POST";
                        request.ContentType =  "application/x-www-form-urlencoded";
                        request.ContentLength = data.Length;

                        using (var steam = request.GetRequestStream())
                        {
                            steam.Write(data, 0, data.Length);
                        }

                        var response = (HttpWebResponse)request.GetResponse();
                        var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

                        var modelResult = JsonConvert.DeserializeObject<PostImageDataModel>(responseString);
                        if (modelResult.Code.Trim().Equals("200"))
                        {
                            success = true;
                        }

                        //using (var client = new HttpClient())
                        //{
                        //    client.BaseAddress = new Uri(UrlEndpoints.BaseUrl);
                        //    var content = new FormUrlEncodedContent(new[]
                        //    {
                        //        new KeyValuePair<string, string>("CustomerId", ChatModel.CustomerId),
                        //        new KeyValuePair<string, string>("Url",
                        //            $"{UrlEndpoints.BlobBaseUrl}{ChatModel.CustomerId}"),
                        //    });
                        //    var apiResponseMessage = await client.PostAsync("/api/customer/PostImageData", content);
                        //    string resultContent = await apiResponseMessage.Content.ReadAsStringAsync();
                        //    Console.WriteLine(resultContent);
                        //}
                    }
                    catch (Exception exception)
                    {
                        
                        Console.WriteLine(exception.Message);
                    }
                    break;
                    //TODO: Push This data to server and refactor the code.

                }
            else
            {

                if (activity != null && (activity.Text.ToLower().Equals("reset") ||
                                         activity.Text.ToLower().Equals("stop")))
                {
                    await context.PostAsync(BotResponses.BotReset);
                    context.Wait(MessageReceivedAsync);
                }

            }
            if (success.Equals(true))
            {
                string jsonString = await ParseJson(UrlEndpoints.ValidationUrl+ $"?cid={ChatModel.CustomerId}");
                var res = JsonConvert.DeserializeObject<VerificationObject>(jsonString);

                _retainedObj = res;

                Thread.Sleep(1000);

                if (_retainedObj.License.Verified.Equals("Yes"))
                {
                    await context.PostAsync($"Hello {_retainedObj.License.Name},{BotResponses.PreQuestionText} ");
                    AddMessagetoHistory($"Hello {_retainedObj.License.Name}, {BotResponses.PreQuestionText}", "Bot");
                    if (_retainedObj.License.Question1 != null)
                        await context.PostAsync($"Q: {_retainedObj.License.Question1.question}");
                    AddMessagetoHistory($"Q: {_retainedObj.License.Question1?.question}", "Bot");
                    context.Wait(FirstQuestionAnswer);
                }
                else if (_retainedObj.License.Verified.Equals("No"))
                {
                    await context.PostAsync(BotResponses.UnableToVerifyText);
                    AddMessagetoHistory(BotResponses.UnableToVerifyText, "Bot");
                    context.Wait(CheckForVarification);
                }
            }
            else
            {
                    await context.PostAsync(BotResponses.imageSuggestPrompt);
                    AddMessagetoHistory(BotResponses.imageSuggestPrompt, "Bot");
                   //Check for hook or image
               
                //display respective message and wait for response
            }
        }

        /// <summary>
        /// Connects when bot reaches First Question for Verification
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task FirstQuestionAnswer(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
            
            if (activity != null && activity.Text.ToLower().Contains("help"))
            {
                await context.PostAsync(BotResponses.HelpText);
                AddMessagetoHistory(BotResponses.HelpText, "Bot");
                context.Wait(FirstQuestionAnswer);
            }
            if (activity != null && activity.Text.ToLower().Contains("terms"))
            {
                await context.PostAsync(BotResponses.TermsText);
                AddMessagetoHistory(BotResponses.TermsText,"Bot");
                context.Wait(FirstQuestionAnswer);
            }
            if (activity != null && activity.Text.ToLower().Equals(_retainedObj.License.Question1.answer.ToLower()))
            {
                AddMessagetoHistory(activity.Text, "User");

                ChatModel.Answer1= true;
                await context.PostAsync($"Q:{_retainedObj.License.Question2.question}");
                AddMessagetoHistory($"Q:{_retainedObj.License.Question2.question}","Bot");
                context.Wait(SecondQuestionAnswer);

            }
            else if (activity != null && (activity.Text.ToLower().Equals("reset") ||
                                     activity.Text.ToLower().Equals("stop")))
            {
                await context.PostAsync(BotResponses.BotReset);
                context.Wait(MessageReceivedAsync);
            }
            else
            {
                if (activity != null && (activity.Text.ToLower().Contains("help") || activity.Text.ToLower().Contains("terms")))
                {
                    //implementation missing yet
                }
                else
                {
                    AddMessagetoHistory(activity?.Text, "User");
                    ChatModel.Answer2 = false;
                    await context.PostAsync($"Q:{_retainedObj.License.Question2.question}");
                    AddMessagetoHistory($"Q:{_retainedObj.License.Question2.question}", "Bot");
                    context.Wait(SecondQuestionAnswer);
                }
               
            }

        }

        /// <summary>
        /// Connects when bot reaches Second Question for Verification
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task SecondQuestionAnswer(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            if (activity != null && (activity.Text.ToLower().Equals("reset") ||
                                     activity.Text.ToLower().Equals("stop")))
            {
                await context.PostAsync(BotResponses.BotReset);
                context.Wait(MessageReceivedAsync);
            }
            if (activity != null && activity.Text.ToLower().Contains("help"))
            {
                await context.PostAsync(BotResponses.HelpText);
                AddMessagetoHistory(BotResponses.HelpText, "Bot");
                context.Wait(SecondQuestionAnswer);
            }
            if (activity != null && activity.Text.ToLower().Contains("terms"))
            {
                await context.PostAsync(BotResponses.TermsText);
                AddMessagetoHistory(BotResponses.TermsText,"Bot");
                context.Wait(SecondQuestionAnswer);
            }
         
            if (activity != null && activity.Text.ToLower().Equals(_retainedObj.License.Question2.answer.ToLower()))
            {
                AddMessagetoHistory(activity.Text, "User");
                ChatModel.Answer2 = true;

                if (ChatModel.Answer1 && ChatModel.Answer2)
                {
                   
                    if (ChatModel.CarType.ToLower().Equals("used"))
                    {
                        await context.PostAsync(BotResponses.VehicleYearPromptText);
                        AddMessagetoHistory(BotResponses.VehicleYearPromptText, "Bot");
                        context.Wait(YearOfVehicle);
                        //year of vehicle
                    }
                    if (ChatModel.CarType.ToLower().Equals("new"))
                    {

                        await context.PostAsync(BotResponses.LoanTermsPromptText);
                        AddMessagetoHistory(BotResponses.LoanTermsPromptText, "Bot");
                        context.Wait(LoanTerms);
                      
                    }
                }
                else
                {
                    await context.PostAsync(BotResponses.UnableToVerifyIdentity);
                    AddMessagetoHistory(BotResponses.UnableToVerifyIdentity, "Bot");
                    SaveandPushLog();
                  
                }
                
            }
            else if (activity != null && (activity.Text.ToLower().Equals("reset") ||
                                     activity.Text.ToLower().Equals("stop")))
            {
                await context.PostAsync(BotResponses.BotReset);
                context.Wait(MessageReceivedAsync);
            }
            else
            {
                if (activity != null && (activity.Text.ToLower().Contains("help") || activity.Text.ToLower().Contains("terms"))) { }
                else
                {
                    await context.PostAsync(BotResponses.UnableToVerifyIdentity);
                    context.Wait(MessageReceivedAsync);
                    
                }
            }
            
        }

        /// <summary>
        /// Connects when there is a need for Loan Terms
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task LoanTerms(IDialogContext context, IAwaitable<object> result)
        {
            
            var activity = await result as Activity;
            if (activity != null && (activity.Text.ToLower().Equals("reset") ||
                                     activity.Text.ToLower().Equals("stop")))
            {
                await context.PostAsync(BotResponses.BotReset);
                context.Wait(MessageReceivedAsync);
            }
            AddMessagetoHistory(activity?.Text,"User");
            if (activity != null && activity.Text.ToLower().Contains("help"))
            {
                await context.PostAsync(BotResponses.HelpText);
                AddMessagetoHistory(BotResponses.HelpText, "Bot");
                context.Wait(LoanTerms);
            }
            if (activity != null && activity.Text.ToLower().Contains("terms"))
            {
                await context.PostAsync(BotResponses.TermsText);
                AddMessagetoHistory(BotResponses.TermsText, "Bot");
                context.Wait(LoanTerms);
            }

            string jsonResponse="" ;
            if (activity != null)
            {
                try
                {
                    if (activity.Text.ToLower().Equals("all"))
                        jsonResponse = await ParseJson(UrlEndpoints.GetBankPackages +
                                                       $"?amount={(int) ChatModel.LoanAmout}&terms=1,2,3,4,5,6&cid={ChatModel.CustomerId}");
                    else
                    {
                        if (TermsValidation(activity))
                        {

                            jsonResponse = await ParseJson(UrlEndpoints.GetBankPackages +
                                                           $"?amount={(int) ChatModel.LoanAmout}&terms={activity.Text}&cid={ChatModel.CustomerId}");
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            var jsonModel = JsonConvert.DeserializeObject<BankPackageModel>(jsonResponse);


            if (activity != null && !String.IsNullOrEmpty(jsonResponse))
            {
                if (activity.Text.ToLower().Equals("reset") ||
                                         activity.Text.ToLower().Equals("stop"))
                {
                    await context.PostAsync(BotResponses.BotReset);
                    context.Wait(MessageReceivedAsync);
                }
                else
                {
                    await context.PostAsync(BotResponses.trterms1);
                    Thread.Sleep(200);
                    await context.PostAsync(
                        $"Congratulations {_retainedObj.License.Name}.  Here are your pre-approved offers for a new car loan in the amount of ${ChatModel.LoanAmout}");
                    Thread.Sleep(200);

                    var packages = "";
                    //packages += BotResponses.TermsHeader;
                    foreach (var bankInfo in jsonModel.BankInfo)
                    {
                        packages +=
                            $"{bankInfo.BankId}) {bankInfo.BankName} - ${bankInfo.Amount} - {bankInfo.Term} months @ {bankInfo.Rate}%  \n\n";
                    }
                    await context.PostAsync(packages);
                    AddMessagetoHistory(packages, "Bot");
                    Thread.Sleep(200);
                    await context.PostAsync(BotResponses.LoanShortLong);
                    bankId = jsonModel.BankInfo[jsonModel.BankInfo.Count - 1].BankId;
                    context.Wait(BankOffers);
                }
            }
            else
            {
                await context.PostAsync(BotResponses.InvalidInputText + $"\n\n");
                await context.PostAsync(BotResponses.LoanTermsPromptText);
                context.Wait(LoanTerms);
            }

        }
        
        /// <summary>
        /// Connects when need to get year of Vehicle for 
        /// Old car loan.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task YearOfVehicle(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
          
            if (activity != null && activity.Text.ToLower().Contains("help"))
            {
                await context.PostAsync(BotResponses.HelpText);
                AddMessagetoHistory(BotResponses.HelpText, "Bot");
                context.Wait(YearOfVehicle);
            }
            if (activity != null && activity.Text.ToLower().Contains("terms"))
            {
                await context.PostAsync(BotResponses.TermsText);
                AddMessagetoHistory(BotResponses.TermsText, "Bot");
                context.Wait(YearOfVehicle);
            }
            
            if (activity != null && (activity.Text.Equals("2012") || activity.Text.Equals("2013") || activity.Text.Equals("2014") || activity.Text.Equals("2015") || activity.Text.Equals("2016") || activity.Text.Equals("2017") || activity.Text.Equals("2018")))
            {
                AddMessagetoHistory(activity.Text,"User");
                ChatModel.YearOfVehicle = activity.Text;
                await context.PostAsync(BotResponses.termsPrompt);
                AddMessagetoHistory(BotResponses.termsPrompt, "Bot");
                context.Wait(LoanTerms);
            }
            else
            {
                if (activity != null && (activity.Text.ToLower().Contains("help") || activity.Text.ToLower().Contains("terms"))) { }
                else if (activity != null && (activity.Text.ToLower().Equals("reset") ||
                                              activity.Text.ToLower().Equals("stop")))
                {
                    await context.PostAsync(BotResponses.BotReset);
                    context.Wait(MessageReceivedAsync);
                }
                else
                {
                    await context.PostAsync($"{BotResponses.InvalidInputText}, (2012-2018)");
                    context.Wait(YearOfVehicle);
                }
            }
        }
        
        /// <summary>
        /// Connects when bank offer is selected.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task BankOffers(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
           
            if (!String.IsNullOrEmpty(activity?.Text))
            {
                int selectedItem;
                Int32.TryParse(activity.Text.Trim(), out selectedItem);
                AddMessagetoHistory(activity.Text, "User");
                if (selectedItem > 0 && selectedItem <= bankId)
                {

                    context.Wait(MessageReceivedAsync);
                    VerificationModel jsonModel = await FinalVerification(activity);
                   // Thread.Sleep(2000);
                   // SaveandPushLog();
                    Thread.Sleep(1000);
                    await FinalStep(context, jsonModel);
                }
                else
                {
                    if (activity.Text.ToLower().Equals("shorter"))
                    {
                        await context.PostAsync(BotResponses.ChangeTermsPrompt);
                        context.Wait(LoanTerms);

                    }
                    else if (activity.Text.ToLower().Equals("longer"))
                    {
                        await context.PostAsync(BotResponses.ChangeTermsPrompt);
                        context.Wait(LoanTerms);
                    }
                    else
                    {

                        await context.PostAsync(BotResponses.InvalidInputText + $" Select the value again.");
                        context.Wait(BankOffers);
                    }
                }
            }
            else
            {
                context.Wait(MessageReceivedAsync);
            }
        }

        

    }
}