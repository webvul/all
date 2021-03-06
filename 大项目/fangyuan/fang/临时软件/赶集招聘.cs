﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace fang.临时软件
{
    public partial class 赶集招聘 : Form
    {
        public 赶集招聘()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false; // 设置线程之间可以操作
        }
        bool status = true;
        int page;
        #region  赶集本地服务
        public void run()
        {

            page = Convert.ToInt32(textBox3.Text);
            if (textBox2.Text == "")
            {
                MessageBox.Show("请输入二级网址！");
                return;
            }

            try
            {

                string[] URLs = textBox2.Text.Split(new string[] { "\r\n" }, StringSplitOptions.None);

                foreach (string URL in URLs)
                {



                    for (int i = 1; i < page; i++)
                    {



                        string url = URL + "o" + i + "/";

                        Match citymatch = Regex.Match(url, @"//([\s\S]*?)\.");
                        string city = citymatch.Groups[1].Value;

                        string html = method.GetUrl(url, "utf-8");

                        MatchCollection urls = Regex.Matches(html, @"post_url=""([\s\S]*?)""", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                       
                        ArrayList lists = new ArrayList();
                        
                        foreach (Match NextMatch in urls)
                        {
                            lists.Add( NextMatch.Groups[1].Value);

                        }

                        if ( lists.Count == 0)  //当前页没有网址数据跳过之后的网址采集，进行下个foreach采集

                            break;


                        foreach (string list in lists)
                        {
                            string html1 = method.GetUrl(list, "utf-8");
                            Match Url3g = Regex.Match(html1, @"format=html5; url=([\s\S]*?)""");

                            string strhtml = method.GetUrl(Url3g.Groups[1].Value, "utf-8");
                            textBox1.Text = Url3g.Groups[1].Value;
                            Match tel = Regex.Match(strhtml, @"&phone=([\s\S]*?)&");
                            Match addr = Regex.Match(strhtml, @"地点</th><td>([\s\S]*?)<");
                            Match lxr = Regex.Match(strhtml, @"联系人</th><td>([\s\S]*?)<", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                            Match company = Regex.Match(strhtml, @"content=""【([\s\S]*?)】");
                            Match infos = Regex.Match(strhtml, @"<h1 class=""title"">([\s\S]*?)</h1>");

                            if (tel.Groups[1].Value != "")
                            {
                                ListViewItem lv1 = listView1.Items.Add(listView1.Items.Count.ToString());
                                lv1.SubItems.Add(Regex.Replace(lxr.Groups[1].Value, "<[^>]*>", "").Trim());
                                lv1.SubItems.Add(tel.Groups[1].Value.Trim());
                                lv1.SubItems.Add(Regex.Replace(addr.Groups[1].Value.Trim(), "<[^>]*>", ""));
                                lv1.SubItems.Add(company.Groups[1].Value.Trim());
                                lv1.SubItems.Add(Regex.Replace(infos.Groups[1].Value.Trim(), "<[^>]*>", ""));


                                Application.DoEvents();
                                Thread.Sleep(Convert.ToInt32(100));



                                if (listView1.Items.Count > 1)
                                {
                                    listView1.EnsureVisible(listView1.Items.Count - 1);  //滚动到指定位置
                                }

                                while (this.status == false)
                                {
                                    Application.DoEvents();//如果loader是false表明正在加载,,则Application.DoEvents()意思就是处理其他消息。阻止当前的队列继续执行。
                                }
                            }

                        }

                       

                    }

                }
            }


            catch (System.Exception ex)
            {

                MessageBox.Show(ex.ToString());
            }

        }

        #endregion

        private void button1_Click(object sender, EventArgs e)
        {
            Thread thread = new Thread(new ThreadStart(run));
            Control.CheckForIllegalCrossThreadCalls = false;
            thread.Start();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.status = false;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.status = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            method.DataTableToExcel(method.listViewToDataTable(this.listView1), "Sheet1", true);
        }

        private void 赶集招聘_Load(object sender, EventArgs e)
        {

        }
    }
}
