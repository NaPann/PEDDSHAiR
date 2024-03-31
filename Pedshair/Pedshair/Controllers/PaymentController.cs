using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Pedshair.Models;
using System.Globalization;

namespace Pedshair.Controllers
{
    public class PaymentController : Controller
    {
        public IActionResult Omise(decimal amt, string pay_type)
        {
            var q = new PaymentData()
            {
                amount = amt,
                pay_type = pay_type
            };
            return View(q);
        }
        [HttpPost]
        public async Task<JsonResult> PostOmise(string pay_type, decimal amount)
        {
            try
            {
                //string _pbKey = "pkey_test_5xdzd2vo91asqfp9fs2"; string _scKey = "skey_test_5xdzd2wjqfsmfjc4cgc";
                string _pbKey = "pkey_test_5ybjlpec30llefhp39i"; string _scKey = "skey_test_5ybjlpf9533cyrx0f62";
                var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(_pbKey);
                string _p = System.Convert.ToBase64String(plainTextBytes);

                var plainTextBytesS = System.Text.Encoding.UTF8.GetBytes(_scKey);
                string _s = System.Convert.ToBase64String(plainTextBytesS);



                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.omise.co/sources");
                request.Headers.Add("Authorization", "Basic " + _p);
                var collection = new List<KeyValuePair<string, string>>();
                collection.Add(new("amount", amount + "00"));
                collection.Add(new("currency", "THB"));
                collection.Add(new("type", "promptpay"));

                var content = new FormUrlEncodedContent(collection);
                request.Content = content;
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var res = await response.Content.ReadAsStringAsync();

                string source_id = Newtonsoft.Json.Linq.JObject.Parse(res).GetValue("id").ToString();

                var _date = DateTime.Now.AddMinutes(30);
                client = new HttpClient();
                request = new HttpRequestMessage(HttpMethod.Post, "https://api.omise.co/charges");
                request.Headers.Add("Authorization", "Basic " + _s);
                collection = new List<KeyValuePair<string, string>>();
                collection.Add(new("amount", amount + "00"));
                collection.Add(new("currency", "THB"));
                collection.Add(new("source", source_id));
                collection.Add(new("expires_at", String.Format("{0}T{1}Z", _date.ToString("yyyy-MM-dd", new CultureInfo("en-us")), _date.ToString("HH:mm:ss", new CultureInfo("en-us")))));
                content = new FormUrlEncodedContent(collection);
                request.Content = content;
                response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                res = await response.Content.ReadAsStringAsync();

                string qr_img = Newtonsoft.Json.Linq.JObject.Parse(res)["source"]["scannable_code"]["image"]["download_uri"].ToString();
                string _chrg_id = Newtonsoft.Json.Linq.JObject.Parse(res)["id"].ToString();

                return Json(new { qr = qr_img, status = "ok", chrg_id = _chrg_id });
            }
            catch (Exception ex)
            {
                return Json(new { model = "", mess = ex.Message, status = "err" });
            }
        }

        [HttpPost]
        public async Task<JsonResult> GetCompleted(string chrg_id)
        {
            try
            {
                var q = true; //_payment.GetCompleted(chrg_id).Result;
                return Json(new { x = (q ? "y" : "n"), status = "ok" });
            }
            catch (Exception ex)
            {
                return Json(new { model = "", mess = ex.Message, status = "err" });
            }
        }
    }
}
