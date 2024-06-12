using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Teru.Code.Models;
using System.Net.Http.Json;
using Newtonsoft.Json;
using System.IO;

namespace ProcessLab.Services
{
    public static class LabService
    {
        public static CommonResult<MeasureResultDto<MeasureData>> Measure(string body)
        {
            HttpClient client = NetService.Client;
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, "http://123.57.66.238/processlab/measure");
            var content = new StringContent(body);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            req.Content = content;
            var res = client.SendAsync(req).Result;

            if (!res.IsSuccessStatusCode)
            {
                return new(false, $"请求失败，服务器响应{res.StatusCode}");
            }

            var raw = res.Content.ReadAsStringAsync().Result;
            var json = JsonConvert.DeserializeObject<MeasureResultDto<MeasureData>>(raw);

            return new(true, "成功", json);
        }

        public static CommonResult<MemoryStream> NetActiveDraw(string body)
        {
            HttpClient client = NetService.Client;
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, "http://123.57.66.238/processlab/netActiveDraw");
            var content = new StringContent(body);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            req.Content = content;
            var res = client.SendAsync(req).Result;

            if (!res.IsSuccessStatusCode)
            {
                return new(false, $"请求失败，服务器响应{res.StatusCode}");
            }

            var raw = res.Content.ReadAsStream();
            MemoryStream ms = new MemoryStream();
            raw.CopyTo(ms);
            ms.Position = 0;

            return new(true, "成功", ms);
        }

        public static CommonResult<MeasureResultDto<string>> GetAuth()
        {
            HttpClient client = NetService.Client;
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, "http://123.57.66.238/processlab/getAuth");
            var res = client.SendAsync(req).Result;

            if (!res.IsSuccessStatusCode)
            {
                return new(false, $"请求失败，服务器响应{res.StatusCode}");
            }

            var raw = res.Content.ReadAsStringAsync().Result;
            var json = JsonConvert.DeserializeObject<MeasureResultDto<string>>(raw);

            return new(true, "成功", json);
        }
    }

    public partial class MeasureResultDto<T>
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("data")]
        public T Data { get; set; }

        [JsonProperty("msg")]
        public string Msg { get; set; }
    }

    public partial class MeasureData
    {
        [JsonProperty("output")]
        public string Output { get; set; }
    }
}
