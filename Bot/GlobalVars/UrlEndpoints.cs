using System;

namespace Bot.GlobalVars
{
    [Serializable]
    public class UrlEndpoints
    {
        public static string BaseUrl { get; set; } = "http://visiloanapi.azurewebsites.net/api";
        public static string ValidationUrl { get; set; } = "http://visiloanapi.azurewebsites.net/api/customer/getquestionsapi";
        public static string GetUrlEndpoint { get; set; } = "http://visiloanapi.azurewebsites.net/api/customer/getuploadurl";
        public static string GetBankPackages { get; set; } = "http://visiloanapi.azurewebsites.net/api/customer/getbankoptions";

    }
}
