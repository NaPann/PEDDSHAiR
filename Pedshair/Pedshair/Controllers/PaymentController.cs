using Dapper;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pedshair.Models;
using System.Data.SqlClient;
using System.Globalization;
using System.Reflection;

namespace Pedshair.Controllers
{
    public class PaymentController : Controller
    {
        private IConfiguration _config; private string connectionString = "";
        public PaymentController(IConfiguration config)
        {
            _config = config;
            connectionString = _config.GetValue<string>("ConnectionStrings");
        }
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

		//Pay Solution
		public IActionResult PaySolution(decimal amt, string pay_type, long refno)
		{
            var q = new PaymentPaySolutionData()
            {
                total = amt,
                channel = pay_type,
                refno = refno,
                customeremail = "pipat@pedd.co.th",
                productdetail = "FOR-PAYMENT",
                cc = "00",
                merchantid = 77647050,
                postbackurl = "https://www.pedshair.com/Payment/PSPostback"

            };
			return View(q);
		}
        [HttpPost]
        public ActionResult PostPS(long refno, int merchantid)
        {
            return RedirectToAction("index", "warp", new { refno = refno, merchantid = merchantid });
            //try
            //{
            //    /* 
            //    Merchant ID : 77647050
            //    Password : sM1J7o8r
            //    API Key : aBPSkHCn
            //    Secret Key : tefPkE3jbdcVkch4 
            //    */
            //    string _marchantID = "77647050";
            //    string _password = "sM1J7o8r";
            //    string _apiKey = "aBPSkHCn";
            //    string _secretKey = "tefPkE3jbdcVkch4";

            //    //var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(_pbKey);
            //    //string _p = System.Convert.ToBase64String(plainTextBytes);

            //    //var plainTextBytesS = System.Text.Encoding.UTF8.GetBytes(_scKey);
            //    //string _s = System.Convert.ToBase64String(plainTextBytesS);

            //    var client = new HttpClient();
            //    var request = new HttpRequestMessage(HttpMethod.Post, "https://payment.paysolutions.asia/epaylink/payment.aspx");
            //    //request.Headers.Add("Authorization", "Basic " + _p);
            //    var collection = new List<KeyValuePair<string, string>>();

            //    string _orderno = "ord-" + DateTime.Now.ToString("yyyyMMddHHmmss");

            //    collection.Add(new("customeremail", "thannapan@pedd.co.th"));
            //    collection.Add(new("productdetail", "FOR-PAYMENT"));
            //    collection.Add(new("refno", $"{_orderno}"));
            //    collection.Add(new("merchantid", $"{_marchantID}"));
            //    collection.Add(new("total", amount.ToString("N2")));
            //    collection.Add(new("cc", "00"));
            //    collection.Add(new("lang", "TH"));
            //    collection.Add(new("channel", "promptpay"));

            //    var content = new FormUrlEncodedContent(collection);
            //    request.Content = content;
            //    var response = await client.SendAsync(request);
            //    response.EnsureSuccessStatusCode();
            //    var res = await response.Content.ReadAsStringAsync();

            //    string source_id = Newtonsoft.Json.Linq.JObject.Parse(res).GetValue("id").ToString();

            //    var _date = DateTime.Now.AddMinutes(30);
            //    client = new HttpClient();
            //    request = new HttpRequestMessage(HttpMethod.Post, "https://api.omise.co/charges");
            //    request.Headers.Add("Authorization", "Basic " + "");
            //    collection = new List<KeyValuePair<string, string>>();
            //    collection.Add(new("amount", amount + "00"));
            //    collection.Add(new("currency", "THB"));
            //    collection.Add(new("source", source_id));
            //    collection.Add(new("expires_at", String.Format("{0}T{1}Z", _date.ToString("yyyy-MM-dd", new CultureInfo("en-us")), _date.ToString("HH:mm:ss", new CultureInfo("en-us")))));
            //    content = new FormUrlEncodedContent(collection);
            //    request.Content = content;
            //    response = await client.SendAsync(request);
            //    response.EnsureSuccessStatusCode();
            //    res = await response.Content.ReadAsStringAsync();

            //    string qr_img = Newtonsoft.Json.Linq.JObject.Parse(res)["source"]["scannable_code"]["image"]["download_uri"].ToString();
            //    string _chrg_id = Newtonsoft.Json.Linq.JObject.Parse(res)["id"].ToString();

            //    return Json(new { qr = qr_img, status = "ok", chrg_id = _chrg_id });
            //}
            //catch (Exception ex)
            //{
            //    return Json(new { model = "", mess = ex.Message, status = "err" });
            //}
        }

		//Post Back
		[HttpPost]
		public ActionResult PSPostback(PaymentPaySolutionData model)

        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                //log
                string sql = "INSERT INTO [dbo].[tbl_postback] (json_text, log_date) VALUES (@x, getdate())";
                var rowsAffected = connection.Execute(sql, new { x = String.Format("{0}|{1}|{2}", model.refno.ToString(), model.total.ToString(), model.merchantid.ToString()) });

                //update
                sql = "UPDATE [dbo].[tbl_service_request] SET [IsPaid] = 1 WHERE [refno] = @refno";
                connection.Execute(sql, model);
            }
            var ret = new Dictionary<string, string>();
            ret.Add("result", "completed");
            return Ok(JsonConvert.SerializeObject(ret));
        }

        public IActionResult thankyou()
        {
            return View();
        }
    }
}
