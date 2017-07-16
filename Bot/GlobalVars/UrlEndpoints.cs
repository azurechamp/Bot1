using System;
using Microsoft.Bot.Builder.Dialogs;

namespace Bot.GlobalVars
{
    [Serializable]
    public class UrlEndpoints
    {
        public static string BaseUrl { get; set; } = "http://visiloanapi.azurewebsites.net/api";
        public static string ValidationUrl { get; set; } = "http://visiloanapi.azurewebsites.net/api/customer/getquestionsapi";//cid=157
        public static string GetUrlEndpoint { get; set; } = "http://visiloanapi.azurewebsites.net/api/customer/getuploadurl";
        public static string GetBankPackages { get; set; } = "http://visiloanapi.azurewebsites.net/api/customer/getbankoptions";
        public static string BlobBaseUrl { get; set; } = "https://dltextloan.blob.core.windows.net/license/";
    }
}
