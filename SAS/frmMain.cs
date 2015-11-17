using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using SAS.Forms;
using ICSharpCode.SharpZipLib.Zip;
using SAS.ClassSet.FunctionTools;
using SAS.ClassSet.Common;
using SAS.ClassSet.MemberInfo;
using SAS.ClassSet;
using System.Threading;
namespace SAS
{
    public partial class frmMain : Form
    {
        public static frmMain fm;
        public static List<string> HostIp = new List<string>();//主机Ip地址集合
        public static Dictionary<string, string> IpAndRec = new Dictionary<string, string>();//用于存储对于Ip所接受到的信息
        public static Dictionary<string, string> IpAndName = new Dictionary<string, string>();//用于存储Ip对应的机房名称
        public frmMain()
        {
            InitializeComponent();
            fm = this;
            FormList = this.listView1;
            ListView.CheckForIllegalCrossThreadCalls = false;
        }
        SqlHelper helper = new SqlHelper();
        public static ListView FormList;
        ParameterizedThreadStart pts;
        Thread thradRecMsg;
        Client client = new Client();
        List<ClientInfo> List = new List<ClientInfo>();
        Insert2DataBase insert = new Insert2DataBase();//增加日志对象
        DataTable dt;
    //-----------------------------------------------------
        /// <summary>
        /// 设置Listviewd的属性
        /// </summary>
        private void SetListviewBorder()
        {
            ImageList il = new ImageList();
           
                //设置高度
            il.ImageSize = new Size(1, 30);
            //绑定listView控件
            listView1.SmallImageList = il;
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            // 开启双缓冲
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            SetListviewBorder();
            // Enable the OnNotifyMessage event so we get a chance to filter out 
            // Windows messages before they get to the form's WndProc
            this.SetStyle(ControlStyles.EnableNotifyMessage, true);
           
            if (Common.load())
            {
                ListViewShow();
                startconnect();
                timer1.Start();
            }
            else
            {
                MessageBox.Show("找不到数据库");
            }
         
          
        }

        private void tsbSet_Click(object sender, EventArgs e)
        {

            frmSetting f = new frmSetting();
            f.Show();
        }


        /// <summary>
        /// 开始向所有服务发送连接请求，即判断是否在线
        /// </summary>
        private void startconnect()
        {
            DataTable dt = helper.getDs("select * from MedicineInfo", "MedicineInfo").Tables[0];
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                ClientInfo info = new ClientInfo(dt.Rows[i][2].ToString(), dt.Rows[i][3].ToString());
                List.Add(info);
            }


            pts = new ParameterizedThreadStart(client.connect);
            thradRecMsg = new Thread(pts);
            thradRecMsg.IsBackground = true;
            thradRecMsg.Start(List);
           
        }
        private void groupBox3_Enter(object sender, EventArgs e)
        {
           
        }

        private void 查询ToolStripMenuItem_Click(object sender, EventArgs e)
        {
          if (listView1.CheckedItems.Count==1)
          {
              if (listView1.CheckedItems[0].SubItems[2].Text=="在线")
              {

                    if (!backgroundWorker1.IsBusy)
                    {
                        backgroundWorker1.RunWorkerAsync(backgroundWorker1);
                    } 
                    else
                    {
                        MessageBox.Show("正在等待上一次的返回结果，请稍后");
                    }
                        
                  
                
              } 
              else
              {
                  MessageBox.Show("请选择在线设备");
              }
          } 
          else
          {
          }
        }
      
        /// <summary>
        /// 向服务器发送信息
        /// </summary>
        private void MsgToServer()
        {
            List<string> ListIp = new List<string>();
            Client sender = new Client();
            string strMsg = "+查询主机\r\n";//指令
            byte[] data = Encoding.GetEncoding("GBK").GetBytes(strMsg.Trim());//转码为Byte数组（GBK）
            //获取当前在线的服务器集合，存储在ListIp中
             for (int i = 0; i < listView1.Items.Count;i++ )
            {
                if (listView1.Items[i].SubItems[2].Text=="在线")
                {
                    ListIp.Add(listView1.Items[i].SubItems[1].Text);
                }
            }
            //遍历在线集合，发送指令数组data
            foreach(string IpNode in ListIp){
                sender.send(IpNode, data);
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            if (listView1.CheckedItems.Count == 1)
            {
                if (listView1.CheckedItems[0].SubItems[2].Text == "在线")
                {
                     if (!backgroundWorker2.IsBusy)
                     {
                          backgroundWorker2.RunWorkerAsync(backgroundWorker2);
                     } 
                     else
                     {
                         MessageBox.Show("正在等待上一次操作的返回结果");

                     }
                   

                }
                else
                {
                    MessageBox.Show("请选择在线设备");
                }
            }
            else
            {
            }

        }
        /// <summary>
        /// 刷新主界面
        /// </summary>
        public void ListViewShow()
        {  
            Client.dict.Clear();
            HostIp.Clear();
            IpAndName.Clear();
            IpAndRec.Clear();
            dt = helper.getDs("select * from MedicineInfo", "MedicineInfo").Tables[0];
            listView1.Items.Clear();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string[] str = new string[]{
                    (i+1).ToString(),
                    dt.Rows[i][2].ToString(),
                     "断线",
                        dt.Rows[i][0].ToString(),
                          dt.Rows[i][1].ToString(),"","","","","","","","","","","",""
                    };
                ListViewItem lit = new ListViewItem(str);
                listView1.Items.Add(lit);
            }
            DataRow[] dr = dt.Select("Type='主机'");
            for (int i = 0; i < dr.Length;i++ )
            {
                HostIp.Add(dr[i][2].ToString());
            }
            for (int i = 0; i < listView1.Items.Count;i++ )
            {
                IpAndName.Add(listView1.Items[i].SubItems[1].Text, listView1.Items[i].SubItems[3].Text);
            }
        }

        private void 更新ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            client.CloseThread();
            Application.Restart();
           
        }
        /// <summary>
        /// 在程序加载完毕5秒后开始向所有在线服务器发送查询命令
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            MsgToServer();
            timer1.Stop();
        }

        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {
            Point curPos = this.listView1.PointToClient(Control.MousePosition);
            ListViewItem lvwItem = this.listView1.GetItemAt(curPos.X, curPos.Y);
            foreach (ListViewItem s in listView1.Items)
            {
                s.Checked = false;
                s.Selected = false;
            }
            if (lvwItem != null)
            {
                lvwItem.Selected = true;
                if (e.X > 16) lvwItem.Checked = true;
               
            }
            else { }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Server s = new Server();
            s.BeginListening("172.27.35.1", "8555", listView1);
            MessageBox.Show("服务器已开启");

            
        }
    
        #region 将控件的状态转换为命令
        /// <summary>
        /// 将所有复选框的状态转换为0/1
        /// </summary>
        /// <returns>开始设防的数据，即各开关状态</returns>
        private string configcommand()
        {
            int[] shefang = new int[7];
            string command = "";
            foreach (Control c in groupBox1.Controls)
            {
                if (c is CheckBox)
                {
                    CheckBox ch = (CheckBox)c;
                    switch (ch.Text)
                    {
                        case "射频":
                            if (ch.Checked)
                            {
                                shefang[3] = 1;
                            }
                            else
                            {
                                shefang[3] = 0;
                            }
                            break;
                        case "烟感":
                            if (ch.Checked)
                            {
                                shefang[0] = 1;
                            }
                            else
                            {
                                shefang[0] = 0;
                            }
                            break;
                        case "市电":
                            if (ch.Checked)
                            {
                                shefang[4] = 1;
                            }
                            else
                            {
                                shefang[4] = 0;
                            }
                            break;
                        case "门禁":
                            if (ch.Checked)
                            {
                                shefang[1] = 1;
                            }
                            else
                            {
                                shefang[1] = 0;
                            }
                            break;
                        case "外烟":
                            if (ch.Checked)
                            {
                                shefang[5] = 1;
                            }
                            else
                            {
                                shefang[5] = 0;
                            }
                            break;
                        case "视频":
                            if (ch.Checked)
                            {
                                shefang[2] = 1;
                            }
                            else
                            {
                                shefang[2] = 0;
                            }
                            break;
                        case "温度":
                            if (ch.Checked)
                            {
                                shefang[6] = 1;
                            }
                            else
                            {
                                shefang[6] = 0;
                            }
                            break;
                    }

                }


            }
            foreach (int i in shefang)
            {
                command = command + i.ToString();
            }
            return command;
        }
        /// <summary>
        /// 常开/常闭
        /// </summary>
        /// <returns></returns>
        private string effvalue()
        {
            if (radioButton1.Checked)
            {
                return "常开";
            }
            else
            {
                return "常闭";
            }
        }
        /// <summary>
        /// 所有的电话，包括授权电话和报警电话
        /// </summary>
        /// <returns></returns>
        private string phone()
        {
            string tel = "";
            foreach (TextBox tb in groupBox4.Controls)
            {
                if (tb.Text != "")
                {
                    tel = tel + ";" + tb.Text.Trim();
                }

            }
            tel = tel + ";";
            return tel.Substring(1);
        }
        /// <summary>
        /// 将地址转换为Unicode码
        /// </summary>
        /// <param name="strEncode"></param>
        /// <returns></returns>
        public static string Encode(string strEncode)
        {
            string strReturn = "";//  存储转换后的编码
            foreach (short shortx in strEncode.ToCharArray())
            {
                strReturn += shortx.ToString("X4");
            }
            return strReturn;
        }
        /// <summary>
        /// 将Uincode转中文
        /// </summary>
        /// <param name="strDecode"></param>
        /// <returns></returns>
        public static string Decode(string strDecode)
        {
            string sResult = "";
            for (int i = 0; i < strDecode.Length / 4; i++)
            {
                sResult += (char)short.Parse(strDecode.Substring(i * 4, 4), System.Globalization.NumberStyles.HexNumber);
            }
            return sResult;
        }
        /// <summary>
        /// 编辑成命令
        /// </summary>
        /// <returns></returns>
        private string formatcommand()
        {
            string Mphone = phone().Substring(0, phone().IndexOf(";"));
            string Ephone = phone().Substring(phone().IndexOf(";") + 1);
            string command = string.Format("+主机设防[开始设定[{0}]授权[{1}]报警[{2}]地址[{3}][{4}]温度[{5}]延时[{6}]响声[{7}]发送[{8}]修改名称[1]70DF96FE[2]95E87981[3]89C69891[4]5C049891[5]75356E90[6]591670DF结束", configcommand(), Mphone
                , Ephone, Encode(textBox1.Text.Trim()), effvalue(), textBox2.Text.Trim(), textBox4.Text.Trim(), textBox3.Text.Trim(), textBox6.Text.Trim());
            //MessageInfo info = new MessageInfo(command, textBox4.Text, "设防", DateTime.Now.ToLongTimeString());
            return command;
        }
        #endregion

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            string strMsg = "+查询主机\r\n";//指令
            //将要发送的字符串 转成 utf8对应的字节数组

            //获得列表中 选中的KEY
            byte[] data = Encoding.GetEncoding("GBK").GetBytes(strMsg.Trim());
            
            if (client.SendAcy(listView1.CheckedItems[0].SubItems[1].Text, data))
            {
                MessageBox.Show("查询成功");
                MessageInfo info = new MessageInfo(strMsg, listView1.CheckedItems[0].SubItems[1].Text, "查询操作", DateTime.Now.ToString());
                insert.insert(info);
            }
            else
            {
                MessageBox.Show("查询失败");
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //查询操作完成后
        }

        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            string strMsg = formatcommand();//发送设防命令
            //将要发送的字符串 转成 utf8对应的字节数组

            //获得列表中 选中的KEY
            byte[] data = Encoding.GetEncoding("GBK").GetBytes(strMsg.Trim());
            if (client.SendAcy(listView1.CheckedItems[0].SubItems[1].Text, data))
            {
                MessageBox.Show("设置成功");
                MessageInfo info = new MessageInfo(strMsg, listView1.CheckedItems[0].SubItems[1].Text,"设置操作",DateTime.Now.ToString());
                insert.insert(info);
            }
            else
            {
                MessageBox.Show("设置失败");
            }
        }

        private void backgroundWorker2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //设防操作完成后
        }

        private void tsbLog_Click(object sender, EventArgs e)
        {
            Logs formlog = new Logs();
            formlog.Show();
        }

        private void frmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            //client.CloseThread();
        }
    //-------------------------------------------------------------------
    }
}
