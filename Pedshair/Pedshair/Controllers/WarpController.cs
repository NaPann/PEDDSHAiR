using Dapper;
using Microsoft.AspNetCore.Mvc;
using Pedshair.Models;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Diagnostics.Eventing.Reader;
namespace Pedshair.Controllers
{
    public class WarpController : Controller
    {
		private IConfiguration _config;
		public WarpController(IConfiguration config)
		{
			_config = config;
			connectionString = _config.GetValue<string>("ConnectionStrings");
			senderSMS = _config.GetValue<string>("SMS:Sender");
			userSMS = _config.GetValue<string>("SMS:User");
			passSMS = _config.GetValue<string>("SMS:Pass");
			messSMS = _config.GetValue<string>("SMS:Message");

		}
		public IActionResult Index(string mobile)
        {
            var vm = new CommonModel();
            vm.ServiceType = "Warp";
            vm.MessageType = "ส่งข้อความ";
            vm.SocialType = "Instagram";
            vm.CreatedBy = mobile;
            return View(vm);
        }
        string connectionString = "";
        string senderSMS = "";
        string userSMS = "";
        string passSMS = "";
		string messSMS = "";
		[HttpPost("warp/index")]
        public ActionResult Index(CommonModel model, IFormFile imageData)
        {
            //model.CreatedBy = "System";
            long length = imageData.Length;
            if (length < 0)
                return BadRequest();


            if (imageData.Length > 0)
            {
                using (var ms = new MemoryStream())
                {
                    imageData.CopyTo(ms);
                    var fileBytes = ms.ToArray();
                    model.Image = Convert.ToBase64String(fileBytes);
                    // act on the Base64 data
                }
            }

            model.refno = long.Parse(DateTime.Now.ToString("yyMMddHHmmss", new System.Globalization.CultureInfo("en-us")));

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                var sql = "INSERT INTO [dbo].[tbl_service_request] ([ServiceType],[PriceType],[Price],[SocialType],[SocialUsername],[MessageType],[Message],[Image],[CreatedDateTime],[CreatedBy],IsPaid,refno) VALUES (@ServiceType,@PriceType,@Price,@SocialType,@SocialUsername,@MessageType,@Message,@Image,GETDATE(),@CreatedBy,0,@refno)";
                var rowsAffected = connection.Execute(sql, model);
            }

            // return RedirectToAction("Index","Home");
            return RedirectToAction("PaySolution", "Payment", new { amt = model.Price, pay_type = "promtpay", refno = model.refno });
        }

		//Register
		public IActionResult Register()
		{
			var vm = new RegisterData();
			return View(vm);
		}
        [HttpPost]
        public IActionResult CheckMobile(string mobile)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                var _check = connection.Query<string>(@$"SELECT mobile FROM   log_otp
WHERE (mobile = N'{mobile}') AND is_done = 1").FirstOrDefault();
                if (_check != null)
                {
                    return Json(new { status = "success" });
                }
                else
                {
                    return Json(new { status = "error" });
                }
            }
        }
        public async Task<IActionResult> Otp(string mobile)
        {
            var vm = new RegisterData();
            vm.mobile = mobile;
            vm.reference = GenerateRandomAlphanumericString();
            vm.otp = GenerateRandomOTP();

            //create otp
            var sql = @"INSERT INTO [dbo].[log_otp]
           ([mobile]
           ,[otp]
           ,[reference]
           ,[logdate]
           ,[is_done])
     VALUES
           (@mobile
           ,@otp
           ,@reference
           ,GETDATE()
           ,0)";

            SqlConnection connection = new SqlConnection(connectionString);
            var rowsAffected = connection.Execute(sql, new { vm.mobile, vm.otp, vm.reference });

			//SMS
			HttpClient client = new HttpClient();
			HttpResponseMessage response = await client.GetAsync(String.Format("https://www.thsms.com/api/rest?username={0}&password={1}&method=send&from={2}&to={3}&message={4}", userSMS, passSMS, senderSMS, mobile, messSMS
                 + vm.otp + " (ref:" + vm.reference + ")"));
			response.EnsureSuccessStatusCode();

			// Deserialize the updated product from the response body.
			var ret = await response.Content.ReadAsStringAsync();
			return View(vm);
        }
		public static string GenerateRandomAlphanumericString(int length = 5)
		{
			const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

			var random = new Random();
			var randomString = new string(Enumerable.Repeat(chars, length)
													.Select(s => s[random.Next(s.Length)]).ToArray());
			return randomString;
		}
		public static string GenerateRandomOTP(int length = 5)
		{
			const string chars = "0123456789";

			var random = new Random();
			var randomString = new string(Enumerable.Repeat(chars, length)
													.Select(s => s[random.Next(s.Length)]).ToArray());
			return randomString;
		}

		public IActionResult CheckOTP(string mobile, string reference, string otp)
		{
			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				var _check = connection.Query<string>(@$"SELECT top(1) mobile  FROM   log_otp
WHERE mobile = N'{mobile}' AND is_done = 0 AND otp = '{otp}' AND reference = '{reference}'").FirstOrDefault();
				if (_check != null)
				{
					//Correct
					var sql = @$"UPDATE [dbo].[log_otp]
   SET [is_done] = 1 WHERE mobile = @mobile AND otp = @otp AND reference = @reference";
					var rowsAffected = connection.Execute(sql, new { mobile, otp, reference });
					return Json(new { status = "success" });
				}
				else
				{
					return Json(new { status = "error" });
				}
			}
		}
	}
}
