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
using System.Linq;
using System.Text;
using System.Data;
using System.IO;
using System.Windows.Forms;

namespace EMRView
{
    public static class EMR
    {
        public const int EMRSTYLE_TOOTH = -1001;  // 简单牙齿公式
        public const int EMRSTYLE_FANGJIAO = -1002;  // 房角公式
        public const int EMRSTYLE_YUEJING = -1003;  // 月经公式
        
        public static bool IsPY(Char aChar)
        {
            return ((aChar >= 'a') && (aChar <= 'z'))
                || ((aChar >= 'A') && (aChar <= 'Z'));
        }

        public static string GetValueAsString(object value)
        {
            if (value is DateTime)
                return string.Format("{0:yyyy-MM-dd HH:mm}", value);
            else
                if (value != null)
                    return value.ToString();
                else
                    return "";
        }

        public static bool TreeNodeIsTemplate(TreeNode aNode)
        {
            return (aNode != null) && (aNode.Tag is TemplateInfo);
        }

        public static bool TreeNodeIsRecordDataSet(TreeNode aNode)
        {
            return (aNode != null) && (aNode.Tag is RecordDataSetInfo);
        }

        public static bool TreeNodeIsRecord(TreeNode aNode)
        {
            return (aNode != null) && (aNode.Tag is RecordInfo);
        }

        public static void GetNodeRecordInfo(TreeNode aNode, ref int aDesPID, ref int aDesID, ref int aRecordID)
        {
            aDesPID = -1;
            aDesID = -1;
            aRecordID = -1;

            if (EMR.TreeNodeIsRecord(aNode))
            {
                aDesID = (aNode.Tag as RecordInfo).DesID;
                aRecordID = (aNode.Tag as RecordInfo).ID;

                aDesPID = -1;
                TreeNode vNode = aNode;
                while (vNode.Parent != null)
                {
                    vNode = vNode.Parent;
                    if (EMR.TreeNodeIsRecordDataSet(vNode))
                    {
                        aDesPID = (vNode.Tag as RecordDataSetInfo).DesPID;
                        break;
                    }
                }
            }
        }

        private static ServerParam serverParam = null;
        public static ServerParam ServerParam
        {
            get { return serverParam; }
            set { serverParam = value; }
        }
    }

    public class ServerParam : Object
    {
        public string Hospital = "";
        public bool PasteDifferent = true;  // 是否允许不同患者之间粘贴复制的内容
        public bool PasteOutSide = true;  // 是否允许病历内容粘贴到其他程序中
    }

    public class CustomUserInfo : Object
    {
        private string 
            FID,  // 用户ID
            FName,  // 用户名
            FDeptID,  // 用户所属科室ID
            FDeptName;  // 用户所属科室名称

        protected virtual void Clear()
        {
            FID = "";
            FName = "";
            FDeptID = "";
            FDeptName = "";
        }

        protected virtual void SetUserID(string value)
        {
            if (FID != value)
                FID = value;
        }

        public virtual object FieldByName(string aFieldName)
        {
            Type type = this.GetType(); //获取类型
            System.Reflection.PropertyInfo propertyInfo = type.GetProperty(aFieldName); //获取指定名称的属性
            return propertyInfo.GetValue(this, null); //获取属性值
        }

        public string ID
        {
            get { return FID; }
            set { SetUserID(value); }
        }

        public string Name
        {
            get { return FName; }
            set { FName = value; }
        }

        public string DeptID
        {
            get { return FDeptID; }
            set { FDeptID = value; }
        }

        public string DeptName
        {
            get { return FDeptName; }
            set { FDeptName = value; }
        }

    }

    public class UserInfo : CustomUserInfo
    {

    }

    public class PatientInfo
    {
        private string FPatID, FInpNo, FBedNo, FName, FSex, FAge, FDeptName;
        private int FDeptID;
        private DateTime FInDateTime, FInDeptDateTime;
        private byte FCareLevel;  // 护理级别
        private byte FVisitID;  // 住院次数

        public void Assign(PatientInfo aSource)
        {
            FInpNo = aSource.InpNo;
            FBedNo = aSource.BedNo;
            FName = aSource.Name;
            FSex = aSource.Sex;
            FAge = aSource.Age;
            FDeptID = aSource.DeptID;
            FDeptName = aSource.DeptName;
            FPatID = aSource.PatID;
            FInDateTime = aSource.InDateTime;
            FInDeptDateTime = aSource.InDeptDateTime;
            FCareLevel = aSource.CareLevel;
            FVisitID = aSource.VisitID;
        }

        public object FieldByName(string aFieldName)
        {
            Type type = this.GetType(); //获取类型
            System.Reflection.PropertyInfo propertyInfo = type.GetProperty(aFieldName); //获取指定名称的属性
            return propertyInfo.GetValue(this, null); //获取属性值
        }
        //
        public string PatID
        {
            get { return FPatID; }
            set { FPatID = value; }
        }
        public string Name
        {
            get { return FName; }
            set { FName = value; }
        }
        public string Sex
        {
            get { return FSex; }
            set { FSex = value; }
        }
        public string Age
        {
            get { return FAge; }
            set { FAge = value; }
        }
        public string BedNo
        {
            get { return FBedNo; }
            set { FBedNo = value; }
        }
        public string InpNo
        {
            get { return FInpNo; }
            set { FInpNo = value; }
        }
        public DateTime InDateTime
        {
            get { return FInDateTime; }
            set { FInDateTime = value; }
        }
        public DateTime InDeptDateTime
        {
            get { return FInDeptDateTime; }
            set { FInDeptDateTime = value; }
        }
        public byte CareLevel
        {
            get { return FCareLevel; }
            set { FCareLevel = value; }
        }
        public byte VisitID
        {
            get { return FVisitID; }
            set { FVisitID = value; }
        }
        public int DeptID
        {
            get { return FDeptID; }
            set { FDeptID = value; }
        }
        public string DeptName
        {
            get { return FDeptName; }
            set { FDeptName = value; }
        }
    }

    public class RecordInfo
    {
        private int FID;
        private int FDesID;  // 数据集ID
        private string FRecName;
        private DateTime FDT, FLastDT;

        public int ID
        {
            get { return FID; }
            set { FID = value; }
        }

        public int DesID
        {
            get { return FDesID; }
            set { FDesID = value; }
        }

        public string RecName
        {
            get { return FRecName; }
            set { FRecName = value; }
        }

        public DateTime DT
        {
            get { return FDT; }
            set { FDT = value; }
        }

        public DateTime LastDT
        {
            get { return FLastDT; }
            set { FLastDT = value; }
        }
    }

    public class ServerInfo
    {
        private DateTime FDateTime;

        public object FieldByName(string aFieldName)
        {
            Type type = this.GetType(); //获取类型
            System.Reflection.PropertyInfo propertyInfo = type.GetProperty(aFieldName); //获取指定名称的属性
            return propertyInfo.GetValue(this, null); //获取属性值
        }

        public DateTime DateTime
        {
            get { return FDateTime; }
            set { FDateTime = value; }
        }
    }

    /// <summary> 数据集信息 </summary>
    public class DataSetInfo
    {
        public const int
            // 数据集
            /// <summary> 数据集正文 </summary>
            CLASS_PAGE = 1,
            /// <summary> 数据集页眉 </summary>
            CLASS_HEADER = 2,
            /// <summary> 数据集页脚 </summary>
            CLASS_FOOTER = 3,

            // 使用范围 1临床 2护理 3临床及护理
            /// <summary> 模板使用范围 临床 </summary>
            USERANG_CLINIC = 1,
            /// <summary> 模板使用范围 护理 </summary>
            USERANG_NURSE = 2,
            /// <summary> 模板使用范围 临床及护理 </summary>
            USERANG_CLINICANDNURSE = 3,

            // 住院or门诊 1住院 2门诊 3住院及门诊
            /// <summary> 住院 </summary>
            INOROUT_IN = 1,
            /// <summary> 门诊 </summary>
            INOROUT_OUT = 2,
            /// <summary> 住院及门诊 </summary>
            INOROUT_INOUT = 3;
        public
        int
            ID, PID,
            GroupClass,  // 模板类别 1正文 2页眉 3页脚
            GroupType,  // 模板类型 1数据集模板 2数据组模板
            UseRang,  // 使用范围 1临床 2护理 3临床及护理
            InOrOut;  // 住院or门诊 1住院 2门诊 3住院及门诊

        public string GroupCode, GroupName;
    }

    /// <summary> 模板信息 </summary>
    public class TemplateInfo  
    {
        public int ID, Owner, DesID;
        public string Name, OwnerID;
    }

    public class RecordDataSetInfo
    {
        private int FDesPID;

        public int DesPID
        {
            get { return FDesPID; }
            set {FDesPID = value; }
        }
    }
}
