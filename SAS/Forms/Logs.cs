using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SAS.ClassSet.FunctionTools;
namespace SAS.Forms
{
    public partial class Logs : Form
    {
        public Logs()
        {
            InitializeComponent();
        }
        private SqlHelper helper = new SqlHelper();
        private void button1_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem s in listView1.Items)
            {
                s.Checked = true;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("是否确认删除?注意!删除后将无法恢复!", "警告", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
            {
                return;
            }
            if (listView1.CheckedItems.Count > 0)
            {
                foreach (ListViewItem LVI in listView1.CheckedItems)
                {
                    string strCMD = "delete from Logs_Data where CTime = '" + LVI.SubItems[3].Text + "'";
                    helper.Oledbcommand(strCMD);
                }
            }
            listView1.Items.Clear();
            ListViewShow(comboBox1.Text);
        }

        private void Logs_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 0;
            ListViewShow(comboBox1.Text);
        }
        /// <summary>
        /// 将数据库当前的数据显示在listview
        /// </summary>
        public void ListViewShow(string type)
        {
            DataTable dt = helper.getDs("select * from Logs_Data where Type = '" + type + "'", "Logs_Data").Tables[0];
            listView1.Items.Clear();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string[] str = new string[]{
                    (i+1).ToString(),
                    dt.Rows[i][0].ToString(),
                      dt.Rows[i][3].ToString(),
                        dt.Rows[i][2].ToString(),
                          dt.Rows[i][1].ToString()
                    };
                ListViewItem lit = new ListViewItem(str);
                listView1.Items.Add(lit);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            ListViewShow(comboBox1.Text);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListViewShow(comboBox1.Text);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            if (sfd.ShowDialog()==DialogResult.OK)
            {
                string savepath = sfd.FileName;
                ExcelHelper.UWriteListViewToExcel(listView1, savepath, comboBox1.Text);
            } 
            else
            {
            }
        }
    }
}
