using Dapper;
using Microsoft.AspNetCore.Mvc;
using Pedshair.Models;
using System.Data.SqlClient;
using System.Net.Http.Headers;

namespace Pedshair.Controllers
{
    public class TipsController : Controller
    {
        public IActionResult Index()
        {
            var vm = new CommonModel();
            vm.ServiceType = "Tips";
            vm.MessageType = "ส่งข้อความ";
            vm.SocialType = "Instagram";
            return View(vm);
        }

        [HttpPost("tips/index")]
        public ActionResult Index(CommonModel model)
        {
            model.CreatedBy = "TEST";
            var connectionString = "Data Source=202.44.230.90;Initial Catalog=PEDSHAIR_DB;User id=pedshair_user;Password=P@$$w0rd@321;";


            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                var sql = "INSERT INTO [dbo].[tbl_service_request] ([ServiceType],[PriceType],[Price],[SocialType],[SocialUsername],[Message],[CreatedDateTime],[CreatedBy]) VALUES (@ServiceType,@PriceType,@Price,@SocialType,@SocialUsername,@Message,GETDATE(),@CreatedBy)";
                var rowsAffected = connection.Execute(sql, model);
            }

            return View(new CommonModel());
        }
    }
}
