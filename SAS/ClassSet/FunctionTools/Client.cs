using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Windows.Forms;
using System.IO;
using SAS.ClassSet.MemberInfo;
using System.Diagnostics;
namespace SAS.ClassSet.FunctionTools
{
    class Client
    {

        private readonly ManualResetEvent TimeoutObject = new ManualResetEvent(false); //连接超时对象 
        private readonly ManualResetEvent SendTimeout = new ManualResetEvent(false); //发送超时对象 
        private readonly ManualResetEvent RecTimeout = new ManualResetEvent(false); //接受超时对象 
        public static Dictionary<string, Socket> dict = new Dictionary<string, Socket>();//当前在线的服务器字典<服务器Ip,对象的socket对象>
        private List<Thread> ThreadPool = new List<Thread>();//线程池
        private UIShow Show = new UIShow();//事务处理对象
        int timeoutSent = 10000;//发送超时参数
        int timeoutRec = 2000;//接收超时参数
        private HandleCommand handle = new HandleCommand();//字符处理对象
        private const string OnLine = "在线";
        private const string DisConnection = "断线";
        private string recflag = "";//发送消息等待一段时间后接收的信息
        private Insert2DataBase Database = new Insert2DataBase();//写入数据库
        /// <summary>
        /// 连接服务器方法，循环创建线程用于连接每台服务器
        /// </summary>
        /// <param name="ObjIp">传入的Ip对象集合</param>
        public void connect(object ObjIp)
        {
            List<ClientInfo> IpAndport = (List<ClientInfo>)ObjIp;

            for (int i = 0; i < IpAndport.Count; i++)
            {
                ParameterizedThreadStart pts = new ParameterizedThreadStart(AloneConnect);
                Thread thradRecMsg = new Thread(pts);
                thradRecMsg.IsBackground = true;
                thradRecMsg.Start(IpAndport[i]);
            }
        }
        /*
          TimeoutObject.Reset();
                Socket socketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint ipendpount = new IPEndPoint(IPAddress.Parse(info.Ip),int.Parse(info.Port));
                socketClient.BeginConnect(ipendpount, CallBackMethod,socketClient);
                //socketClient.Connect(IPAddress.Parse(IpAndport[i].Ip), int.Parse(IpAndport[i].Port));
                if (TimeoutObject.WaitOne(timeoutMSec, false)&&socketClient!=null)
                {
                    //MessageBox.Show("网络正常");
                    dict.Add(info.Ip, socketClient);
                    //------UI显示在线

                   
                    // MessageBox.Show(IpAndport[i].Ip + "服务器" + "在线");
                    //开通讯线程
                    ApplyThread(OnLine, socketClient);
                    ShowIpStauts(info.Ip, OnLine);
                }
                else
                {
                    //MessageBox.Show("连接超时"); 
                    throw new SocketException();
                }  
         
         
         */
        /// <summary>
        /// 连接一个服务器的方法
        /// </summary>
        /// <param name="objinfo">服务器信息对象</param>
        private void AloneConnect(object objinfo)
        {
            ClientInfo info = (ClientInfo)objinfo;
            try
            {
                Socket socketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                socketClient.Connect(IPAddress.Parse(info.Ip), int.Parse(info.Port));
                
                dict.Add(info.Ip, socketClient);

                ShowIpStauts(socketClient.RemoteEndPoint.ToString().Substring(0, socketClient.RemoteEndPoint.ToString().IndexOf(":")), OnLine);
                //开通讯线程
                ApplyThread(OnLine, socketClient);


            }
            catch (SocketException)
            {


                ShowIpStauts(info.Ip, DisConnection);
                //开重连
                ClientInfo Info = new ClientInfo(info.Ip, info.Port);
                ApplyThread(DisConnection, Info);


            }

        }

        /// <summary>
        /// 申请线程
        /// </summary>
        /// <param name="flag">申请线程类型</param>
        /// <param name="obj">委托对象</param>
        private void ApplyThread(string flag, object obj)
        {
            ParameterizedThreadStart thradstart;
            Thread thrad;
            switch (flag)
            {
                case OnLine:
                    thradstart = new ParameterizedThreadStart(RecMsg);
                    thrad = new Thread(thradstart);
                    thrad.IsBackground = true;
                    thrad.Start((Socket)obj);
                    ThreadPool.Add(thrad);
                    break;
                case DisConnection:
                    thradstart = new ParameterizedThreadStart(Retry);
                    thrad = new Thread(thradstart);
                    thrad.IsBackground = true;
                    thrad.Start((ClientInfo)obj);
                    ThreadPool.Add(thrad);
                    break;

            }
        }
        /// <summary>
        /// 关闭所有线程
        /// </summary>
        public void CloseThread()
        {


            foreach (Thread t in ThreadPool)
            {
                t.Abort();


            }
            foreach (Socket s in dict.Values)
            {
                s.Close();
            }
            //dict.Clear();
            //ThreadPool.Clear();
        }

        //--异步连接回调方法         
        private void CallBackMethod(IAsyncResult asyncresult)
        {
            //使阻塞的线程继续          
            TimeoutObject.Set();
        }
        //--异步发送回调方法         
        private void SendCallBackMethod(IAsyncResult asyncresult)
        {
            //使阻塞的线程继续          
            SendTimeout.Set();
        }
        //异步接受回调方法
        private void RecCallBackMethod(IAsyncResult asyncresult)
        {
            SendTimeout.Set();
        }
        /// <summary>
        /// 服务器无法连接时，不断尝试连接服务器
        /// </summary>
        /// <param name="ip">无法连接的Ip</param>
        /// <param name="Port">无法连接的无法端口</param>
        private void Retry(object Info)
        {
            ClientInfo info = (ClientInfo)Info;
            while (true)
            {

                try
                {
                    Socket socketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                    socketClient.Connect(IPAddress.Parse(info.Ip), int.Parse(info.Port));
                    dict.Add(info.Ip, socketClient);

                    ShowIpStauts(socketClient.RemoteEndPoint.ToString().Substring(0, socketClient.RemoteEndPoint.ToString().IndexOf(":")), OnLine);
                    //开通讯线程
                    ApplyThread(OnLine, socketClient);
                    break;
                }
                catch (System.Exception)
                {
                    continue;
                }
            }
        }
        /// <summary>
        /// 当已连接时断开的重连方法
        /// </summary>
        /// <param name="ip">断线的Ip</param>
        /// <param name="Port">断线的端口</param>
        /// <returns></returns>
        private Socket reconnect(string ip, string port)
        {
            while (true)
            {

                try
                {
                    Socket socketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    socketClient.Connect(IPAddress.Parse(ip), int.Parse(port));
                    dict.Add(ip, socketClient);
                    ShowIpStauts(socketClient.RemoteEndPoint.ToString().Substring(0, socketClient.RemoteEndPoint.ToString().IndexOf(":")), OnLine);
                    return socketClient;
                }
                catch (System.Exception)
                {
                    continue;
                }
            }


        }
        /// <summary>
        /// 向服务器发送数据
        /// </summary>
        /// <param name="Ip">服务器Ip</param>
        /// <param name="arrMsg">发送的信息</param>
        public void send(string Ip, byte[] arrMsg)
        {
            try
            {
                if (Ip == "")
                {
                    foreach (Socket s in dict.Values)
                    {
                        s.Send(arrMsg);

                    }
                }
                else
                {
                    dict[Ip].Send(arrMsg);
                }

            }
            catch (System.Exception)
            {

            }

        }

        public bool SendAcy(string Ip, byte[] arrMsg)
        {
            bool IsSendSuccess = false;
            try
            {


                SendTimeout.Reset();
                dict[Ip].BeginSend(arrMsg, 0, arrMsg.Length, SocketFlags.None, new AsyncCallback(SendCallBackMethod), dict[Ip]);


                if (SendTimeout.WaitOne(timeoutSent, false))
                {
                    //System.Threading.Thread.Sleep(10000);
                    RecTimeout.Reset();
                    if (RecTimeout.WaitOne(timeoutRec, false))
                    {
                        if (recflag.IndexOf("OK") != -1)
                        {
                          
                            IsSendSuccess = true;
                            return IsSendSuccess;
                        }
                        else
                        {
                           
                            return IsSendSuccess;
                        }

                    }
                    else
                    {
                     
                        return IsSendSuccess;
                    }


                }
                else
                {
                    return IsSendSuccess;
                }



            }
            catch (System.Exception)
            {

                return IsSendSuccess;
            }

        }
        /// <summary>
        /// 用于在主界面显示Ip及其在线状态
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="stauts"></param>
        private void ShowIpStauts(string ip, string stauts)
        {
            string[] str = new string[]{
                       stauts ,
                       ip
                    };
            Show.ShwMsgforView(frmMain.FormList, str);
        }
        /// <summary>
        /// 用于判别返回结果
        /// </summary>
        /// <param name="command">返回结果的字符串</param>
        private void ContorlCommand(string command, string ip)
        {
            recflag = "";
            if (command.IndexOf("SHOW-2") != -1)
            {
                //MessageBox.Show(command);
                recflag = command;
                RecTimeout.Set();
                Show.ShwStuatsforView(frmMain.FormList, handle.QueryHandle(command), ip);
                //MessageInfo info = new MessageInfo(command, ip, "查询返回", DateTime.Now.ToString());
                //Database.insert(info);//写入数据库
            }
            else if (command.IndexOf("+主机设防") != -1)
            {
                MessageBox.Show("设防命令");
                //MessageInfo info = new MessageInfo(command, ip, "设防返回", DateTime.Now.ToString());
                //Database.insert(info);
            }
            else if (command.IndexOf("SHOW-6") != -1)
            {
                //MessageBox.Show("更新命令");
                Show.ShwStuatsforView(frmMain.FormList, handle.QueryHandle(command), ip);
                //MessageInfo info = new MessageInfo(command, ip, "更新返回", DateTime.Now.ToString());
                //Database.insert(info);
            }
            else if (command.IndexOf("SHOW-8[") != -1)
            {
                // MessageBox.Show("转发命令"
                command = "+" + command;
               string[] str= command.Split('[', ']');
               MessageInfo info = new MessageInfo(frmMain.Decode(str[1]), ip, "警报", DateTime.Now.ToString());//将转发记录转码后保存
                Database.insert(info);
                byte[] bytearray = Encoding.Unicode.GetBytes(command);
                foreach (string host in frmMain.HostIp)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        if (SendAcy(host, bytearray))
                        {
                            MessageBox.Show("转发成功");
                            break;
                        }
                        else
                        {
                           
                            continue;
                        }

                    }
                }


            }
            else if (command.IndexOf("SHOW-8-OK") != -1)
            {
                recflag = command;
            
                RecTimeout.Set();

            }
            else if (command.IndexOf("SHOW-1[") != -1)
            {
                recflag = command;
            
                RecTimeout.Set();
            }

        }
        /// <summary>
        /// 接受来自服务器的信息
        /// </summary>
        /// <param name="Socketclient">服务器的socket对象</param>
        private void RecMsg(object Socketclient)
        {
            Socket s = (Socket)Socketclient;
            while (true)
            {

                if (s.Connected)
                {
                    try
                    {
                        //msg 为显示的信息
                        byte[] buffer = new byte[1024];
                        int size = 0;
                        long len = 0;
                        string command = "";
                        
                        while ((size = s.Receive(buffer, 0, buffer.Length, SocketFlags.None)) > 0)
                        {
                            
                            command = Encoding.GetEncoding("GBK").GetString(buffer, 0, size);
                            len += size;
                            Show.SetIpAndRec(command, s.RemoteEndPoint.ToString().Substring(0, s.RemoteEndPoint.ToString().IndexOf(":")));

                            ContorlCommand(command, s.RemoteEndPoint.ToString().Substring(0, s.RemoteEndPoint.ToString().IndexOf(":")));

                        }
                        //断线检测
                        string msg = " ";
                        s.Send(Encoding.UTF8.GetBytes(msg));
                    }
                    catch (SocketException ex)
                    {
                        MessageBox.Show(ex.ToString());
                        MessageBox.Show(s.RemoteEndPoint.ToString() + "服务器" + "断线");
                        ShowIpStauts(s.RemoteEndPoint.ToString().Substring(0, s.RemoteEndPoint.ToString().IndexOf(":")), DisConnection);
                        dict.Remove(s.RemoteEndPoint.ToString().Substring(0, s.RemoteEndPoint.ToString().IndexOf(":")));
                        s = reconnect(s.RemoteEndPoint.ToString().Substring(0, s.RemoteEndPoint.ToString().IndexOf(":")), s.RemoteEndPoint.ToString().Substring(s.RemoteEndPoint.ToString().IndexOf(":") + 1));
                        MessageBox.Show(s.RemoteEndPoint.ToString() + "服务器" + "重连成功");
                        continue;

                    }catch(Exception ex){
                        MessageBox.Show(ex.ToString());
                    }
                }
                else
                {
                    MessageBox.Show(s.RemoteEndPoint.ToString() + "服务器" + "断线");
                    ShowIpStauts(s.RemoteEndPoint.ToString().Substring(0, s.RemoteEndPoint.ToString().IndexOf(":")), DisConnection);
                    dict.Remove(s.RemoteEndPoint.ToString().Substring(0, s.RemoteEndPoint.ToString().IndexOf(":")));
                    s = reconnect(s.RemoteEndPoint.ToString().Substring(0, s.RemoteEndPoint.ToString().IndexOf(":")), s.RemoteEndPoint.ToString().Substring(s.RemoteEndPoint.ToString().IndexOf(":") + 1));
                    MessageBox.Show(s.RemoteEndPoint.ToString() + "服务器" + "重连成功");
                    continue;
                }

            }
        }
    }
}
