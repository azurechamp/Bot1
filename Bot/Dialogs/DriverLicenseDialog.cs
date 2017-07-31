using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
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
    [Serializable]
    public class DirverLicenseDialog : IDialog<object>
    {
        private VerificationObject _retainedObj;

        private static  bool CheckForUpload(bool success)
        {
            for (int i = 0; i < Utils.TimerMinutes; i++)
            {
                Thread.Sleep(TimeSpan.FromSeconds(30));
                var data =  ParseJson($"{UrlEndpoints.WebHookUrl}{ChatModel.CustomerId}");
                var model = JsonConvert.DeserializeObject<WebHookModel>(data);
                if (model != null)
                {
                    if (model.Code.Equals("200"))
                    {
                        success = true;
                        break;
                    }
                    if (model.Code.Equals("300"))
                    {
                        success = false;
                        break;
                    }

                }
            }

            return success;
        }

        private static  string ParseJson(string url)
        {
            var jsonString = "";
            try
            {
                var client = new HttpClient();
                var response = client.GetStringAsync(url);
                jsonString = response.Result;
            }
            catch (Exception e) { Console.WriteLine(e.Message); }
            return jsonString;
        }
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



        public Task StartAsync(IDialogContext context)
        {
            
            context.Wait(MessageReceivedAsync);
            bool success = false;
            success = CheckForUpload(false);
            if (success.Equals(true))
            {
                //Success Result
                context.Done(true);
            }
            else
            {
                context.Done(false);
            }
            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
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

                        var request =
                            (HttpWebRequest)WebRequest.Create(
                                "http://visiloanapi.azurewebsites.net/api/customer/PostImageData");

                        var postData = $"CustomerId={ChatModel.CustomerId}";
                        postData += $"&Url={UrlEndpoints.BlobBaseUrl}";
                        var data = Encoding.ASCII.GetBytes(postData);

                        request.Method = "POST";
                        request.ContentType = "application/x-www-form-urlencoded";
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
                    //reset implementation
                }

                //Hook Implementation


            }
            if (success.Equals(true))
            {
                string jsonString =  ParseJson(UrlEndpoints.ValidationUrl + $"?cid={ChatModel.CustomerId}");
                var res = JsonConvert.DeserializeObject<VerificationObject>(jsonString);

                _retainedObj = res;

                Thread.Sleep(1000);

                if (_retainedObj.License.Verified.Equals("Yes"))
                {
                    context.Done(true);
                }
                else if (_retainedObj.License.Verified.Equals("No"))
                {
                    context.Done(false);
                }
            }
            else
            {
                if (activity != null && !String.IsNullOrEmpty(activity.Text))
                {
                    if ((activity.Text.ToLower().Equals("reset") ||
                         activity.Text.ToLower().Equals("stop")))
                    {
                        await context.PostAsync(BotResponses.BotReset);
                        context.Wait(MessageReceivedAsync);
                        return;
                    }
                    else if (activity.Text.ToLower().Contains("help"))
                    {
                        await context.PostAsync(BotResponses.HelpText);
                        //AddMessagetoHistory(BotResponses.HelpText, "Bot");
                        context.Wait(MessageReceivedAsync);
                    }
                    else if (activity.Text.ToLower().Contains("terms"))
                    {
                        await context.PostAsync(BotResponses.TermsText);
                        //AddMessagetoHistory(BotResponses.TermsText, "Bot");
                        context.Wait(MessageReceivedAsync);
                    }
                    else
                    {

                        await context.PostAsync(BotResponses.imageSuggestPrompt);
                      //  AddMessagetoHistory(BotResponses.imageSuggestPrompt, "Bot");
                        //Check for hook or image
                    }
                }
                else
                {
                    context.Done(false);
                    //(BotResponses.UnableToVerifyText, "Bot");
                }
                //display respective message and wait for response


            }
        }

        
    }
}
