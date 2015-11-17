using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SAS.ClassSet.MemberInfo;
using System.Data;
using System.Data.OleDb;
using SAS.Forms;
namespace SAS.ClassSet.FunctionTools
{
    class Insert2DataBase
    {
        SqlHelper helper = new SqlHelper();
        public void insert(MessageInfo info)
        {
            DataTable dt = helper.getDs("select * from Logs_Data", "Logs_Data").Tables[0];
            DataRow dr = dt.NewRow();
            dr[0] = frmMain.IpAndName[info.Address];
            dr[1] = info.Allmessage;
            dr[2] = info.Time;
            dr[3] = info.Type;
            dt.Rows.Add(dr);
            OleDbDataAdapter da = helper.adapter("select * from Logs_Data");
            da.Update(dt);
        }

    }
}
