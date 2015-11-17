using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using SAS.ClassSet.MemberInfo;
using System.Linq;
namespace SAS.ClassSet.FunctionTools
{
    class UIShow
    {
        delegate void ShwMsgforViewCallBack(ListView lvi, string[] text);
        delegate void ShwStuatsforViewCallBack(ListView lvi, SelectionInfo info, string ip);

        /// <summary>
        /// 判断listview中是否存在text参数为主键的行
        /// </summary>
        /// <param name="text">主键</param>
        /// <param name="listView1">搜索控件</param>
        /// <returns></returns>
        private int IsExistsItem(string text, ListView listView1)
        {
            for (int i = 0; i < listView1.Items.Count; i++)
            {
                if (listView1.Items[i].SubItems[1].Text == text)
                {
                    return i;
                }
            }
            return -1;
        }
        /// <summary>
        /// 用于在listview中显示对应Ip是否在线
        /// </summary>
        /// <param name="lvi">绑定的控件</param>
        /// <param name="text">在线信息</param>
        public void ShwMsgforView(ListView lvi, string[] text)
        {
            if (lvi.InvokeRequired)
            {
                ShwMsgforViewCallBack shwMsgforViewCallBack = ShwMsgforView;
                lvi.Invoke(shwMsgforViewCallBack, new object[] { lvi, text });
                int count = lvi.Items.Count;
                int rownum = IsExistsItem(text[1], lvi);
                if (rownum >= 0)
                {
                    lvi.BeginUpdate();
                    lvi.Items[rownum].SubItems[1].Text = text[1];
                    lvi.Items[rownum].SubItems[2].Text = text[0];
                    //------------------记录
                    lvi.EndUpdate();

                }
                else
                {
                    string[] str = new string[]{
                       (count+1).ToString(),
                       text[1],
                         text[0],
                         " ",
                        " ",
                        " ",
                          " ",
                        " ",
                          " ",
                        " ",
                          " ",
                          " "
                    };

                    ListViewItem lit = new ListViewItem(str);
                    lvi.BeginUpdate();
                    lvi.Items.Add(lit);
                    lvi.EndUpdate();
                }

            }
            else
            {

            }

        }
        /// <summary>
        /// 判断listview中的数据是否和原来的一样，如果一样则不改变，否则改变
        /// </summary>
        /// <param name="newstring"></param>
        /// <param name="oldstring"></param>
        /// <returns></returns>
        private string CheckIsNull(string newstring, string oldstring)
        {
            if (newstring == null)
            {
                return oldstring;
            }
            else
            {
                return newstring;
            }
        }
        /// <summary>
        /// 存储接受的数据
        /// </summary>
        /// <param name="str"></param>
        /// <param name="ip"></param>
        public void SetIpAndRec(string str, string ip)
        {
            if (frmMain.IpAndRec.Keys.Contains(ip))
            {
                frmMain.IpAndRec[ip] = str;
            }
            else
            {
                frmMain.IpAndRec.Add(ip, str);
            }

        }
        /// <summary>
        /// 用于显示查询后的信息
        /// </summary>
        /// <param name="lvi">绑定控件</param>
        /// <param name="info">返回信息</param>
        /// <param name="Ip">对于Ip</param>
        public void ShwStuatsforView(ListView lvi, SelectionInfo info, string Ip)
        {
            if (lvi.InvokeRequired)
            {
                ShwStuatsforViewCallBack shwStuatsforViewCallBack = ShwStuatsforView;
                lvi.Invoke(shwStuatsforViewCallBack, new object[] { lvi, info, Ip });
                int rownum = IsExistsItem(Ip, lvi);
                if (info != null)
                {
                    if (rownum >= 0)
                    {
                        lvi.BeginUpdate();
                        lvi.Items[rownum].SubItems[3].Text = CheckIsNull(info.Address, lvi.Items[rownum].SubItems[3].Text);
                        lvi.Items[rownum].SubItems[5].Text = CheckIsNull(info.Temperature, lvi.Items[rownum].SubItems[5].Text);
                        lvi.Items[rownum].SubItems[6].Text = CheckIsNull(info.Temperaturealarm, lvi.Items[rownum].SubItems[6].Text);
                        lvi.Items[rownum].SubItems[7].Text = CheckIsNull(info.Leaves, lvi.Items[rownum].SubItems[7].Text);
                        lvi.Items[rownum].SubItems[8].Text = CheckIsNull(info.Smoke, lvi.Items[rownum].SubItems[8].Text);
                        lvi.Items[rownum].SubItems[9].Text = CheckIsNull(info.Rf, lvi.Items[rownum].SubItems[9].Text);
                        lvi.Items[rownum].SubItems[10].Text = CheckIsNull(info.Video, lvi.Items[rownum].SubItems[10].Text);
                        lvi.Items[rownum].SubItems[11].Text = CheckIsNull(info.Electric, lvi.Items[rownum].SubItems[11].Text);
                        lvi.Items[rownum].SubItems[12].Text = CheckIsNull(info.Dooralarm, lvi.Items[rownum].SubItems[12].Text);
                        lvi.Items[rownum].SubItems[13].Text = CheckIsNull(info.Alaremtime, lvi.Items[rownum].SubItems[13].Text);
                        lvi.Items[rownum].SubItems[14].Text = CheckIsNull(info.Alarmdalay, lvi.Items[rownum].SubItems[14].Text);
                        lvi.Items[rownum].SubItems[15].Text = CheckIsNull(info.Effective, lvi.Items[rownum].SubItems[15].Text);
                        lvi.Items[rownum].SubItems[16].Text = CheckIsNull(info.Status, lvi.Items[rownum].SubItems[16].Text);
                        lvi.EndUpdate();

                    }
                }

            }
            else
            {

            }
        }
    }
}
