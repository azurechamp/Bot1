using System;

namespace Bot.GlobalVars
{
    [Serializable]
    public class UrlEndpoints
    {
        public static string ValidationUrl { get; set; } = "https://raw.githubusercontent.com/muhammad92/Sport/master/test.json";
        public static string GetUrlEndpoint { get; set; } = "http://visiloanapi.azurewebsites.net/api/customer/getuploadurl";
        public static string GetBankPackages { get; set; } = "http://visiloanapi.azurewebsites.net/api/customer/getbankoptions";

    }
}
