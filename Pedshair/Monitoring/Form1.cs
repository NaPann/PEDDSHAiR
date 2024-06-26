﻿using Dapper;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Monitoring
{
    public partial class Form1 : Form
    {
        Timer MainTimer = new Timer();
        Timer ProcessTimer = new Timer();
        public Form1()
        {
            InitializeComponent();

            string _qr = Properties.Settings.Default.URL;

            Zen.Barcode.CodeQrBarcodeDraw qrcode = Zen.Barcode.BarcodeDrawFactory.CodeQr;
            this.pmQR.Image = qrcode.Draw(_qr, 50);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            pbImage.BackColor = Color.FromArgb(85, 255, 255, 255);
            lblSocialUsername.BackColor = Color.FromArgb(85, 255, 255, 255);
            lblMessage.BackColor = Color.FromArgb(85, 255, 255, 255);
            panel1.BackColor = Color.FromArgb(85, 255, 255, 255);
            MainTimer.Interval = (5 * 1000);
            MainTimer.Tick += new EventHandler(MyTimer_Tick);
            MainTimer.Start();
        }
        private async void MyTimer_Tick(object sender, EventArgs e)
        {
            var connectionString = Properties.Settings.Default.DBConnection;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                var queryData = @"SELECT Top (1) * FROM [PEDSHAIR_DB].[dbo].[tbl_service_request] where IsApprove <> 1 and IsPublish <> 1 and ServiceType <> 'Tips' AND [IsPaid] = 1 order by CreatedDateTime asc;";

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
                    pbImage.Visible = true;
                    panel1.Visible = true;
                    this.lblSocialUsername.Visible = true;
                    this.lblMessage.Visible = true;
                }
                else
                {
                    pbImage.SizeMode = PictureBoxSizeMode.Normal;
                    pbImage.BackColor = Color.FromArgb(85, 255, 255, 255);
                    pbImage.Visible = false;
                    pbImage.Image = null;
                    lblMessage.Text = "";
                    lblSocialUsername.Text = "";
                    panel1.Visible = false;
                    this.lblSocialUsername.Visible = false;
                    this.lblMessage.Visible = false;
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
