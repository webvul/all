﻿using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace 美团
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool InternetGetCookieEx(string pchURL, string pchCookieName, StringBuilder pchCookieData, ref System.UInt32 pcchCookieData, int dwFlags, IntPtr lpReserved);

        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]    
        static extern int InternetSetCookieEx(string lpszURL, string lpszCookieName, string lpszCookieData, int dwFlags, IntPtr dwReserved);

        #region  获取cookie
        /// <summary>
        /// 获取cookie
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private static string GetCookies(string url)
        {
            uint datasize = 256;
            StringBuilder cookieData = new StringBuilder((int)datasize);
            if (!InternetGetCookieEx(url, null, cookieData, ref datasize, 0x2000, IntPtr.Zero))
            {
                if (datasize < 0)
                    return null;


                cookieData = new StringBuilder((int)datasize);
                if (!InternetGetCookieEx(url, null, cookieData, ref datasize, 0x00002000, IntPtr.Zero))
                    return null;
            }
            return cookieData.ToString();
        }

        #endregion

        public string cookie;
        bool zanting = true;
        public static string username = "";
        private void Form1_Load(object sender, EventArgs e)
        {
            method.SetWebBrowserFeatures(method.IeVersion.IE11);
            getCityName();
            label3.Text = username;
            webBrowser1.Navigate("https://i.meituan.com/wrapapi/poiinfo?poiId=150177929");
            webBrowser1.ScriptErrorsSuppressed = true;
        }

        #region 获取数据库美团城市名称
        public void getCityName()
        {
            ArrayList list = new ArrayList();
            try
            {
                string constr = "Host =47.99.68.92;Database=citys;Username=root;Password=zhoukaige00.@*.";
                string str = "SELECT name from meituan_province_city ";
                MySqlDataAdapter da = new MySqlDataAdapter(str, constr);
                DataSet ds = new DataSet();
                da.Fill(ds);
                DataTable dt = ds.Tables[0];
                foreach (DataRow dr in dt.Rows)
                {
                    list.Add(dr[0].ToString().Trim());
                }
            }
            catch (MySqlException ee)
            {
                MessageBox.Show(ee.Message.ToString());
            }
            comboBox1.DataSource = list;

        }
        #endregion

        #region GET请求
        public static string meituan_GetUrl(string Url, string COOKIE)
        {
            try
            {


                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);  //创建一个链接

                request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.11 (KHTML, like Gecko) Chrome/23.0.1271.97 Safari/537.11";

                request.Headers.Add("Cookie", COOKIE);

                request.Referer = "https://i.meituan.com/wrapapi/poiinfo?poiId=150177929";
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;  //获取反馈

                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding("utf-8")); //reader.ReadToEnd() 表示取得网页的源码流 需要引用 using  IO

                string content = reader.ReadToEnd();
                reader.Close();
                response.Close();
                return content;

            }
            catch (System.Exception ex)
            {
                ex.ToString();



            }
            return "";
        }
        #endregion

        #region  获取数据库中城市名称对应的拼音

        public string Getpinyin(string city)
        {

            try
            {
                string constr = "Host =47.99.68.92;Database=citys;Username=root;Password=zhoukaige00.@*.";
                MySqlConnection mycon = new MySqlConnection(constr);
                mycon.Open();

                MySqlCommand cmd = new MySqlCommand("select meituan_city_pinyin from meituan_city where meituan_city_name='" + city + "'  ", mycon);         //SQL语句读取textbox的值'"+textBox1.Text+"'


                MySqlDataReader reader = cmd.ExecuteReader();  //读取数据库数据信息，这个方法不需要绑定资源

                if (reader.Read())
                {

                    string citypinyin = reader["meituan_city_pinyin"].ToString().Trim();
                    return citypinyin;
                }
                mycon.Close();
                reader.Close();
                return "";


            }

            catch (System.Exception ex)
            {
                return ex.ToString();
            }


        }

        #endregion

        #region  不包含美食分类不分区域多城市多行业
        public void Search()
        {

            try
            {
                string[] citys = textBox1.Text.Trim().Split(new string[] { "," }, StringSplitOptions.None);

                if (textBox1.Text.Trim() == "")
                {
                    MessageBox.Show("请输入城市！");
                    return;
                }

                if (textBox2.Text.Trim() == "")
                {
                    MessageBox.Show("请输入关键字!");
                    return;
                }
                string[] keywords = textBox2.Text.Trim().Split(new string[] { "," }, StringSplitOptions.None);

                foreach (string city in citys)
                {
                    foreach (string keyword in keywords)

                    {
                        
                        for (int i = 1; i <= 50; i++)

                        {


                            string Url = "http://i.meituan.com/s/" + Getpinyin(city) + "-" + keyword + "?p=" + i;
                            
                            string html = meituan_GetUrl(Url, this.cookie);  //定义的GetRul方法 返回 reader.ReadToEnd()

                          
                            MatchCollection all = Regex.Matches(html, @"data-href=""//i.meituan.com/poi/([\s\S]*?)"">");

                            ArrayList lists = new ArrayList();
                            foreach (Match NextMatch in all)
                            {

                                //lists.Add("http://i.meituan.com/poi/" + NextMatch.Groups[1].Value);
                                lists.Add("https://i.meituan.com/wrapapi/poiinfo?poiId=" + NextMatch.Groups[1].Value);
                            }

                            if (lists.Count == 0)  //当前页没有网址数据跳过之后的网址采集，进行下个foreach采集

                                break;

                            string tm1 = DateTime.Now.ToString();  //获取系统时间

                            toolStripStatusLabel1.Text = tm1 + "-->正在采集" + city + "" + keyword + "第" + i + "页";
                           
                            foreach (string list in lists)

                            {
                               
                                string strhtml1 = meituan_GetUrl(list, this.cookie);  //定义的GetRul方法 返回 reader.ReadToEnd()

                                //Match name = Regex.Match(strhtml1, @"<h1 class=""dealcard-brand"">([\s\S]*?)</h1>");
                                //Match tell = Regex.Match(strhtml1, @"data-tele=""([\s\S]*?)""");
                                //Match addr = Regex.Match(strhtml1, @"addr:([\s\S]*?)&");
                                Match name = Regex.Match(strhtml1, @"name"":""([\s\S]*?)""");
                                Match tell = Regex.Match(strhtml1, @"phone"":""([\s\S]*?)""");
                                Match addr = Regex.Match(strhtml1, @"address"":""([\s\S]*?)""");
                                if (name.Groups[1].Value != "")
                                {
                                    ListViewItem listViewItem = this.listView1.Items.Add((listView1.Items.Count + 1).ToString());
                                    listViewItem.SubItems.Add(name.Groups[1].Value);
                                    listViewItem.SubItems.Add(tell.Groups[1].Value);
                                    listViewItem.SubItems.Add(addr.Groups[1].Value);
                                    listViewItem.SubItems.Add(city);


                                    while (this.zanting == false)
                                    {
                                        Application.DoEvents();//如果loader是false表明正在加载,,则Application.DoEvents()意思就是处理其他消息。阻止当前的队列继续执行。
                                    }

                                }
                                Application.DoEvents();
                                Thread.Sleep(1000);


                            }

                        }
                    }


                }
            }





            catch (System.Exception ex)
            {
                ex.ToString();
            }
        }

        #endregion

        private void LinkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            DialogResult dr = MessageBox.Show("确定要关闭吗？", "关闭", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (dr == DialogResult.OK)
            {
                Environment.Exit(0);
            }
            else
            {

            }
        }

        private void LinkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.WindowState = FormWindowState.Minimized; //最小化
        }
        private Point mPoint = new Point();
        private void Panel1_MouseDown(object sender, MouseEventArgs e)
        {
            mPoint.X = e.X;
            mPoint.Y = e.Y;

        }

        private void Panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Point myPosittion = MousePosition;
                myPosittion.Offset(-mPoint.X, -mPoint.Y);
                Location = myPosittion;
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            SetWindowRegion();
        }

        public void SetWindowRegion()
        {
            System.Drawing.Drawing2D.GraphicsPath FormPath;
            FormPath = new System.Drawing.Drawing2D.GraphicsPath();
            Rectangle rect = new Rectangle(0, 0, this.Width, this.Height);
            FormPath = GetRoundedRectPath(rect, 10);
            this.Region = new Region(FormPath);

        }
        private GraphicsPath GetRoundedRectPath(Rectangle rect, int radius)
        {
            int diameter = radius;
            Rectangle arcRect = new Rectangle(rect.Location, new Size(diameter, diameter));
            GraphicsPath path = new GraphicsPath();

            // 左上角
            path.AddArc(arcRect, 180, 90);

            // 右上角
            arcRect.X = rect.Right - diameter;
            path.AddArc(arcRect, 270, 90);

            // 右下角
            arcRect.Y = rect.Bottom - diameter;
            path.AddArc(arcRect, 0, 90);

            // 左下角
            arcRect.X = rect.Left;
            path.AddArc(arcRect, 90, 90);
            path.CloseFigure();//闭合曲线
            return path;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
           
            this.cookie = GetCookies("https://i.meituan.com/wrapapi/poiinfo?poiId=150177929");

           
            button1.Enabled = false;
            Thread search_thread = new Thread(new ThreadStart(this.Search));
            Control.CheckForIllegalCrossThreadCalls = false;
            search_thread.Start();
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            zanting = false;
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            zanting = true;
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            method.DataTableToExcel(method.listViewToDataTable(this.listView1), "Sheet1", true);
        }

        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBox1.Text += comboBox1.SelectedItem.ToString() + ",";
        }

        private void LinkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.acaiji.com");
        }

        private void LinkLabel9_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            listView1.Items.Clear();
        }

        private void LinkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            method.ListviewToTxt(listView1);
        }

        private void LinkLabel11_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            button1.Enabled = true;
        }
    }
}
