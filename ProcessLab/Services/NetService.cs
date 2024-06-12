using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ProcessLab.Services
{
    public class NetService
    {
        public static HttpClient Client { get; set; }
        public static void Init()
        {
            Client = new HttpClient(new HttpClientHandler() { AllowAutoRedirect = true, CookieContainer = new CookieContainer(), UseCookies = true, MaxAutomaticRedirections = 10, AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip });
        }
    }
}
