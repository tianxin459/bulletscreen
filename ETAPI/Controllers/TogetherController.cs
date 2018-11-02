using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Net.Http.Headers;

namespace ETAPI.Controllers
{
    [Produces("application/json")]
    [Route("api/Together")]
    public class TogetherController : Controller
    {
        private string _urlGetAccessToken = @"https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid=wx65c33dab465989c4&secret=056cb6850f93278dd35f930ca344714f";
        private string _urlQRCode = @"https://api.weixin.qq.com/wxa/getwxacodeunlimit?access_token={0}";

        // GET: api/Together
        [HttpGet]
        [Route("ping")]
        public IEnumerable<string> Ping()
        {
            return new string[] { "TogetherController", DateTime.Now.ToString() };
        }

        //[HttpGet]
        //[Route("data")]
        //public IActionResult GetData()
        //{
        //    var url = 
        //    using (var c = new HttpClient())
        //    {
        //        var resp = c.GetAsync(_urlGetAccessToken).Result.Content.ReadAsStringAsync().Result;
        //        dynamic obj = JObject.Parse(resp);
        //        accessToken = obj.access_token;
        //    }
        //}

        [HttpGet]
        [Route("AccessToken")]
        public string GetAccessToken (){
            var accessToken = string.Empty;
            using (var c = new HttpClient())
            {
                var resp = c.GetAsync(_urlGetAccessToken).Result.Content.ReadAsStringAsync().Result;
                dynamic obj = JObject.Parse(resp);
                accessToken = obj.access_token;
            }
            return accessToken;
        }


        [HttpGet]
        [Route("QRCode")]
        public string GetQRCode()
        {
            //return "testing";
            dynamic response;
            Image returnImage;
            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            //var tempUrl = @"https://upload.wikimedia.org/wikipedia/commons/e/e2/%E2%80%9C%E5%B1%B1%E8%A5%BF%E7%BB%B4%E5%9F%BA%E4%BA%BA%E2%80%9DQQ%E7%BE%A4%E5%8A%A0%E7%BE%A4%E4%BA%8C%E7%BB%B4%E7%A0%81.png";
            using (var c = new HttpClient())
            {
                var accessToken = GetAccessToken();
                var request = new StringContent(JsonConvert.SerializeObject(new { scene = "ab", page = "pages/index/index" }), Encoding.UTF8, "application/json");
                var bytes = c.PostAsync(string.Format(_urlQRCode, accessToken), request).Result.Content.ReadAsByteArrayAsync().Result;
                //var bytes = c.GetAsync(tempUrl).Result.Content.ReadAsByteArrayAsync().Result;
                MemoryStream ms = new MemoryStream(bytes);
                returnImage = Image.FromStream(ms);

                returnImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                string imageBase64Data = Convert.ToBase64String(bytes);
                string imageDataURL = string.Format("data:image/png;base64,{0}", imageBase64Data);
                response = imageDataURL;
                //result.Content = new ByteArrayContent(ms.ToArray());
                //result.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                //response = JObject.Parse(resp);
            }
            return response;
        }

    }
}
