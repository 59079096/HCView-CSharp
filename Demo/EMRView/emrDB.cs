using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.IO;

namespace EMRView
{
    public class emrDB
    {
        public const string Sql_GetAllPatient = "Select * from patients";
        public const string Sql_GetPatient = "Select * from patients WHERE brandid = {0}";
        public const string Sql_GetTemplate = "SELECT * FROM Template";
        public const string Sql_GetPatInRecord = "SELECT * FROM PatInRecord WHERE pid = {0} and vid = {1}";

        public byte[] GetTemplateContent(string aTID)
        {
            if (aTID != "")
            {
                OleDbCommand com = FDBConn.CreateCommand();
                com.CommandText = string.Format("SELECT content FROM Template where id={0}", aTID);

                object vObj = com.ExecuteScalar();
                if (!(vObj is System.DBNull))
                    return (byte[])vObj;//读取之后转换成二进制字节数组
                else
                    return null;
            }
            else
                return null;
        }

        public bool SaveTemplateContent(string aTID, Stream stream)
        {
            if (aTID != "")
            {
                stream.Position = 0;
                byte[] vFile = new byte[stream.Length];//分配数组大小
                stream.Read(vFile, 0, (int)stream.Length);//将文件内容读进数组

                OleDbCommand com = FDBConn.CreateCommand();

                //其中picture字段是OLE对象数据类型
                com.CommandText = string.Format("UPDATE Template set content = @content WHERE id = {0}", aTID);
                com.Parameters.AddWithValue("@content", vFile);
                com.ExecuteNonQuery();

                return true;
            }
            else
                return false;
        }

        //新建一个数据库的连接，叫FDBconn
        private OleDbConnection FDBConn;

        //查询sql语句并返回
        public OleDbDataReader Exec(string ASql)
        { 
            //创建并返回一个与oledbconnection关联的对象
            OleDbCommand cmd = FDBConn.CreateCommand();
            //sql命令赋值给ASql这个字符串
            cmd.CommandText = ASql;
            //开启数据库
            if (FDBConn.State != ConnectionState.Open)
            {
                FDBConn.Open();
            }
            //若要创建OleDbDataReader，必须调用ExecuteReader方法的OleDbCommand对象
            //创建一个读到的dr，读取刚才取到的数据
            OleDbDataReader dr = cmd.ExecuteReader();
            cmd.Dispose();
            //返回刚才读到的数据
            return dr;
        }

        public int ExecSql(string ASql)
        {
            //创建并返回一个与oledbconnection关联的对象
            OleDbCommand cmd = FDBConn.CreateCommand();
            //sql命令赋值给ASql这个字符串
            cmd.CommandText = ASql;
            //开启数据库
            if (FDBConn.State != ConnectionState.Open)
            {
                FDBConn.Open();
            }

            return cmd.ExecuteNonQuery();
        }

        public DataTable ExecToDataTable(string ASql)
        {
            //使用上面定义的方法传入查询数语句 dr
            OleDbDataReader dr = Exec(ASql);
            //从内存中创建一个空表 dt
            DataTable dt = new DataTable();
            //如果dr语句查出来则遍历到dt表
            if (dr.HasRows)
            {
                for (int i = 0; i < dr.FieldCount; i++)
                {
                    //定义一个dc表示dt列的架构，并把dt列添加到dc
                    DataColumn dc = dt.Columns.Add(dr.GetName(i));
                }
                //关闭dt
                dt.Rows.Clear();
            }
            //循环查询到的数据
            while (dr.Read())
            {
                //在内存中创建一个row存储dt的每一行
                DataRow row = dt.NewRow();
                //遍历dr的每一行
                for (int i = 0; i < dr.FieldCount; i++)
                {
                    row[i] = dr[i];
                }
                //表dt中的每一行添加到刚刚定义好的row中
                dt.Rows.Add(row);
            }
            //返回dt
            return dt;
        }
        //定义一个连接数据库语句
        public emrDB()
        {
            FDBConn = new OleDbConnection(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=emrDB.mdb");
        }

        //定义一个析构函数用于释放被占用的系统资源（只能在类中使用析构函数，不能在结构中使用）
        ~emrDB()
        {
            //FDBConn.Close();
        }
    }

}
