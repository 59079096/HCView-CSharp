/*******************************************************}
{                                                       }
{         基于HCView的电子病历程序  作者：荆通          }
{                                                       }
{ 此代码仅做学习交流使用，不可用于商业目的，由此引发的  }
{ 后果请使用者承担，加入QQ群 649023932 来获取更多的技术 }
{ 交流。                                                }
{                                                       }
{*******************************************************/
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.IO;

namespace EMRView
{
    public class emrAccessDB
    {
        public const string Sql_GetAllPatient = "Select * from patients";
        public const string Sql_GetPatient = "Select * from patients WHERE brandid = {0}";
        
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
        public emrAccessDB()
        {
            FDBConn = new OleDbConnection(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=emrDB.mdb");
        }

        //定义一个析构函数用于释放被占用的系统资源（只能在类中使用析构函数，不能在结构中使用）
        ~emrAccessDB()
        {
            //FDBConn.Close();
        }
    }

    /// <summary> 访问sqlserver </summary>
    public class emrMSDB
    {
        private string FDBConnectstring;
        private string FErrMsg;
        private DataTable FDataElementDT, FDataSetElementDT, FDataSetDT;
        private static emrMSDB FDB = new emrMSDB();

        public const string Sql_GetTemplate = "SELECT id, pid, Name, Class, Type, UseRang, InOrOut, od FROM Comm_DataElementSet";
        public const string Sql_GetTemplateList = "SELECT id, desid, tname, owner, ownerid FROM Comm_TemplateInfo WHERE desid = {0}";
        public const string Sql_GetTemplateContent = "SELECT content FROM Comm_TemplateContent WHERE tid = {0}";
        public const string Sql_SaveTemplateConent = "UPDATE Comm_TemplateContent SET content=@content WHERE tid=@tid";
        public const string Sql_GetDomainItem = "SELECT DE.ID, DE.Code, DE.devalue, DE.PY, DC.Content FROM Comm_DataElementDomain DE LEFT JOIN Comm_DomainContent DC ON DE.ID = DC.DItemID WHERE DE.domainid = {0}";
        public const string Sql_GetInchRecordList = "SELECT rec.ID, cdes.pid AS despid, cdes2.name AS despname, rec.desID, cdes.od AS desorder,   cdes.Name AS desName, rec.Name, rec.DT, rec.CreateUserID, rec.CreateDT, rec.LastUserID, rec.LastDT   FROM Inch_RecordInfo rec LEFT JOIN Comm_DataElementSet cdes ON rec.desID = cdes.id    LEFT JOIN (SELECT id, name, od FROM Comm_DataElementSet WHERE pid = 0) AS cdes2 ON cdes2.id = cdes.pid     WHERE PatID = {0} AND VisitID = {1} ORDER BY cdes2.od";
        public const string Sql_GetDataSet = "SELECT id, pid, Name, Class, Type, UseRang, InOrOut, od FROM Comm_DataElementSet";
        public const string Sql_SaveRecordContent = "EXEC UpdateInchRecord @rid, @LastUserID, @Content";
        public const string Sql_NewInchRecord = "DECLARE @Result int EXEC @Result = CreateInchRecord @PatID, @VisitID, @desID, @Name, @DT, @DeptID, @CreateUserID, @Content  SELECT @Result AS RecordID";
        public const string Sql_GetDestRecordContent = "SELECT rec.ID, rec.Name, cnt.content FROM Inch_RecordInfo rec LEFT JOIN Comm_DataElementSet cdes ON rec.desID = cdes.id LEFT JOIN Inch_RecordContent cnt ON rec.ID = cnt.rid WHERE PatID = {0} AND VisitID = {1} AND cdes.pid = {2}";
        public const string Sql_SaveDomainItemContent = "EXEC SaveDomainContent @DItemID, @Content";
        public const string Sql_GetDomainItemContent = "SELECT Content FROM Comm_DomainContent WHERE DItemID = {0}";
        public const string Sql_GetDataSetRecord = "SELECT rec.ID, rec.Name, cnt.content FROM Inch_RecordInfo rec LEFT JOIN Comm_DataElementSet cdes ON rec.desID = cdes.id LEFT JOIN Inch_RecordContent cnt ON rec.ID = cnt.rid WHERE PatID = {0} AND VisitID = {1} AND cdes.pid = {2}";

        #region 数据库操作
        /// <summary>
        /// 通过SQL查询数据
        /// </summary>
        /// <param name="sql">查询脚本</param>
        /// <returns>DataTable</returns>
        public DataTable GetData(string sql)
        {
            DataTable dt = null;
            SqlConnection conn = new SqlConnection(FDBConnectstring);

            try
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = sql;
                //SqlDataReader dr = cmd.ExecuteReader();

                SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                DataSet ds = new DataSet();
                da.Fill(ds);
                if (ds != null && ds.Tables.Count > 0)
                {
                    dt = ds.Tables[0];
                }
            }
            catch (Exception exp)
            {
                FErrMsg = "执行查询失败：" + exp.Message;
            }
            finally
            {
                conn.Close();
            }

            return dt;
        }

        /// <summary>
        /// 执行一个sql脚本修改
        /// </summary>
        /// <param name="sql">脚本</param>
        /// <param name="ErrMsg">错误消息</param>
        /// <returns>bool</returns>
        public bool ExecSql(string sql)
        {
            SqlConnection conn = new SqlConnection(FDBConnectstring);
            conn.Open();
            try
            {
                SqlCommand sqlComm = new SqlCommand();
                sqlComm.Connection = conn;
                sqlComm.CommandText = sql;
                sqlComm.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                FErrMsg = ex.Message;
                return false;
            }
            finally
            {
                conn.Close();
            }
        }

        public delegate void ExecCommandEventHanler(SqlCommand sqlComm);

        /// <summary>
        /// 执行一个sql脚本修改
        /// </summary>
        /// <param name="sql">脚本</param>
        /// <param name="ErrMsg">错误消息</param>
        /// <returns>bool</returns>
        public bool ExecSql(string sql, ExecCommandEventHanler exec, ExecCommandEventHanler exec2 = null)
        {
            SqlConnection conn = new SqlConnection(FDBConnectstring);
            conn.Open();
            try
            {
                SqlCommand sqlComm = new SqlCommand();
                sqlComm.Connection = conn;
                sqlComm.CommandText = sql;
                if (exec != null)
                    exec(sqlComm);
                
                sqlComm.ExecuteNonQuery();

                if (exec2 != null)
                    exec2(sqlComm);

                return true;
            }
            catch (Exception ex)
            {
                FErrMsg = ex.Message;
                return false;
            }
            finally
            {
                conn.Close();
            }
        }

        public bool ExecStoredProcedure(ExecCommandEventHanler exec)
        {
            SqlConnection conn = new SqlConnection(FDBConnectstring);
            conn.Open();
            try
            {
                SqlCommand sqlComm = new SqlCommand();
                sqlComm.Connection = conn;
                if (exec != null)
                    exec(sqlComm);

                return true;
            }
            catch (Exception ex)
            {
                FErrMsg = ex.Message;
                return false;
            }
            finally
            {
                conn.Close();
            }
        }
        #endregion

        public emrMSDB()
        {
            FDBConnectstring = @"user=emr;password=emr;database=emrDB;server=(local)";
            FDataSetDT = this.GetData("SELECT id, pid, Name, Class, Type FROM Comm_DataElementSet WHERE pid = 0 ORDER BY od");
            GetDataElement();
        }

        ~emrMSDB()
        {
    
        }

        public string ErrMsg
        {
            get { return FErrMsg; }
        }

        public void GetDataElement()
        {
            FDataElementDT = this.GetData("SELECT deid, decode, dename, py, frmtp, domainid FROM Comm_DataElement");
        }

        public void GetDataSetElement(int aDesID)
        {
            FDataSetElementDT = this.GetData(string.Format("SELECT DeID, KX FROM Comm_DataSetElement WHERE DsID ={0}", aDesID));
        }

        public void GetTemplateContent(int aTempID, Stream aStream)
        {
            DataTable dt = FDB.GetData(string.Format(Sql_GetTemplateContent, (aTempID)));
            if (dt.Rows.Count > 0)
            {
                if (dt.Rows[0]["content"].GetType() != typeof(System.DBNull))
                {
                    byte[] vContent = (byte[])dt.Rows[0]["content"];
                    aStream.Write(vContent, 0, vContent.Length);
                }
            }
        }

        public void GetRecordContent(int aRecordID, Stream aStream)
        {
            DataTable dt = FDB.GetData(string.Format("SELECT content FROM Inch_RecordContent WHERE rid = {0}", (aRecordID)));
            if (dt.Rows.Count > 0)
            {
                byte[] vContent = (byte[])dt.Rows[0]["content"];
                aStream.Write(vContent, 0, vContent.Length);
            }
        }

        public bool GetInchRecordSignature(int aRecordID)
        {
            DataTable dt = FDB.GetData(string.Format("SELECT UserID FROM Inch_RecordSignature WHERE RID = {0}", (aRecordID)));
            return dt.Rows.Count > 0;
        }

        public bool SignatureInchRecord(int aRecordID, string aUserID)
        {
            return FDB.ExecSql(string.Format("INSERT INTO Inch_RecordSignature (RID, UserID, DT) VALUES ({0}, '{1}', GETDATE())", aRecordID, aUserID), null);
        }

        public bool DeletePatientRecord(int aRecordID)
        {
            return FDB.ExecSql(string.Format("EXEC DeleteInchRecord {0}", aRecordID), null);
        }

        public DataTable DataElementDT
        {
            get { return FDataElementDT; }
        }

        public DataTable DataSetElementDT
        {
            get { return FDataSetElementDT; }
        }

        public DataTable DataSetDT
        {
            get { return FDataSetDT; }
        }

        public static emrMSDB DB
        {
            get { return FDB; }
        }
    }
}
