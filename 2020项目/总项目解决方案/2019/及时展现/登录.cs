﻿using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace 及时展现
{
    public partial class 登录 : Form
    {
        public 登录()
        {
            InitializeComponent();
        }

        private void Button1_Click(object sender, EventArgs e)
        {

                try
                {



                    string constr = "Host =139.159.218.174;Database=data;Username=root;Password=123456";
                    MySqlConnection mycon = new MySqlConnection(constr);
                    mycon.Open();

                    MySqlCommand cmd = new MySqlCommand("select * from users where user='" + textBox1.Text.Trim() + "'  ", mycon);         //SQL语句读取textbox的值'"+skinTextBox1.Text+"'


                    MySqlDataReader reader = cmd.ExecuteReader();  //读取数据库数据信息，这个方法不需要绑定资源

                label3.Text = "正在连接服务器......";

           
                System.Threading.Thread.Sleep(200);

                label3.Text = "正在验证用户名和密码......";


                if (reader.Read())
                  {

                    string username = reader["user"].ToString().Trim();
                    string password = reader["pass"].ToString().Trim();
                    string keywords = reader["keywords"].ToString().Trim();
                    string time = reader["time"].ToString().Trim();
                    string status = reader["status"].ToString().Trim();

                    DateTime dt = DateTime.Now;
                    if (dt < Convert.ToDateTime(time))
                    {
                        if (status == "0")
                        {
                            if (textBox2.Text.Trim() == password)

                            {

                                客户端 kh = new 客户端();
                                kh.Show();
                                客户端.keywords = keywords;
                                kh.label6.Text = username;
                                kh.label7.Text = time;
                                this.Hide();
                                mycon.Close();

                                
                                MySqlConnection mycon1 = new MySqlConnection(constr);
                                mycon1.Open();
                                MySqlCommand cmd1 = new MySqlCommand("UPDATE users SET status= 1 where user='" + username+ "'   ", mycon1);         //SQL语句读取textbox的值'"+skinTextBox1.Text+"'

                                cmd1.ExecuteNonQuery();  //count就是受影响的行数,如果count>0说明执行成功,如果=0说明没有成功.
                                mycon1.Close();


                                string path = AppDomain.CurrentDomain.BaseDirectory;
                                FileStream fs1 = new FileStream(path +"config\\"+ "user.txt", FileMode.Create, FileAccess.Write);//创建写入文件 
                                StreamWriter sw = new StreamWriter(fs1);
                                sw.WriteLine(textBox1.Text);
                                sw.Close();
                                fs1.Close();







                            }
                            else

                            {
                                MessageBox.Show("您的密码错误！");
                                label3.Text = "请重新输入密码！";
                                return;
                            }
                        }

                        else
                        {
                            MessageBox.Show("您的账号在其它地方已登录！");
                            label3.Text = "您的账号在其它地方已登录！";
                            return;
                        }
                      
                    }
                    else
                    {
                        MessageBox.Show("您的账号已过期！");
                        label3.Text = "您的账号已过期！";
                        return;

                    }

                }

                else
                {
                    MessageBox.Show("您输入的账号不存在！");
                    label3.Text = "您输入的账号不存在！";
                    return;

                }

                
                reader.Close();
                
            }

                catch (System.Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }

  
        }

        private void 登录_Load(object sender, EventArgs e)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;
            if (File.Exists(path + "config\\" + "user.txt"))
            {
                StreamReader sr = new StreamReader(path + "config\\" + "user.txt", Encoding.Default);
                //一次性读取完 
                string texts = sr.ReadToEnd();
                textBox1.Text = texts;
                sr.Close();
            }


        }









    }
}
