using Dapper;
using Monitoring.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace Monitoring
{
    public partial class Form1 : Form
    {
        Timer MainTimer = new Timer();
        Timer ProcessTimer = new Timer();
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            MainTimer.Interval = (5 * 1000);
            MainTimer.Tick += new EventHandler(MyTimer_Tick);
            MainTimer.Start();
        }
        private async void MyTimer_Tick(object sender, EventArgs e)
        {
            var connectionString = "Data Source=202.44.230.90;Initial Catalog=PEDSHAIR_DB;User id=pedshair_user;Password=P@$$w0rd@321;";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                var queryData = @"SELECT Top (1) * FROM [PEDSHAIR_DB].[dbo].[tbl_service_request] where IsApprove <> 1 and IsPublish <> 1 and ServiceType <> 'Tips' order by CreatedDateTime asc;";

                var monitorDatas = conn.Query<CommonModel>(queryData).ToList();

                if (monitorDatas != null && monitorDatas.Count > 0)
                {
                    MainTimer.Stop();
                    var query = "";
                    foreach (var item in monitorDatas)
                    {
                        await MyMethodAsync(item);
                        query = @"UPDATE tbl_service_request SET IsPublish = 1, PublishedDatetime = GETDATE()  WHERE id =" + item.Id + ";";

                        if (item.PriceType == "1")
                            MainTimer.Interval = (40 * 1000);
                        else if (item.PriceType == "2")
                            MainTimer.Interval = (60 * 1000);
                        else if (item.PriceType == "6")
                            MainTimer.Interval = (80 * 1000);
                    }

                    var affectRow = await conn.ExecuteAsync(query);
                    
                    MainTimer.Start();


                }
                else
                {
                    pbImage.Image = pbImage.InitialImage;
                    pbImage.SizeMode = PictureBoxSizeMode.Normal;
                    lblMessage.Text = "";
                    lblSocialUsername.Text = "";
                    MainTimer.Interval = (5 * 1000);
                    MainTimer.Start();
                }
            }

        }
        public async Task MyMethodAsync(CommonModel data)
        {
            byte[] bytes = Convert.FromBase64String(data.Image);
            System.Drawing.Image image;
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                image = System.Drawing.Image.FromStream(ms);
            }

            pbImage.Image = image;
            pbImage.SizeMode = PictureBoxSizeMode.CenterImage;
            lblMessage.Text = data.Message;
            lblSocialUsername.Text = data.SocialUsername;

            if (lblSocialUsername.InvokeRequired)
            {
                lblSocialUsername.Invoke(new Action(() => lblSocialUsername.Text = data.SocialUsername));
            }
            else
            {
                lblSocialUsername.Text = data.SocialUsername;
            }

        }
    }
}
