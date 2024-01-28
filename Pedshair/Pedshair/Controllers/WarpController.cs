using Dapper;
using Microsoft.AspNetCore.Mvc;
using Pedshair.Models;
using System.Data.SqlClient;
using System.Net.Http.Headers;

namespace Pedshair.Controllers
{
    public class WarpController : Controller
    {
        public IActionResult Index()
        {
            var vm = new CommonModel();
            vm.ServiceType = "Warp";
            vm.MessageType = "ส่งข้อความ";
            vm.SocialType = "Instagram";
            return View(vm);
        }

        [HttpPost("warp/index")]
        public ActionResult Index(CommonModel model, IFormFile imageData)
        {
            model.CreatedBy = "TEST";
            var connectionString = "Data Source=202.44.230.90;Initial Catalog=PEDSHAIR_DB;User id=pedshair_user;Password=P@$$w0rd@321;";

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

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                var sql = "INSERT INTO [dbo].[tbl_service_request] ([ServiceType],[PriceType],[Price],[SocialType],[SocialUsername],[Message],[Image],[CreatedDateTime],[CreatedBy]) VALUES (@ServiceType,@PriceType,@Price,@SocialType,@SocialUsername,@Message,@Image,GETDATE(),@CreatedBy)";
                var rowsAffected = connection.Execute(sql, model);
            }

            return View(new CommonModel());
        }
    }
}
