﻿/*******************************************************}
{                                                       }
{         基于HCView的电子病历程序  作者：荆通          }
{                                                       }
{ 此代码仅做学习交流使用，不可用于商业目的，由此引发的  }
{ 后果请使用者承担，加入QQ群 649023932 来获取更多的技术 }
{ 交流。                                                }
{                                                       }
{*******************************************************/
using HC.View;
using HC.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace EMRView
{
    public enum DeTraceStyle : byte  // 痕迹样式
    {
        /// <summary> 删除痕迹 </summary>
        cseDel = 1,
        /// <summary> 新增痕迹 </summary>
        cseAdd = 2,
        /// <summary> 修改痕迹 </summary>
        cesMod = 4
    }

    /// <summary> 数据元属性 </summary>
    public static class DeProp : Object
    {
        public const string Index = "Index";
        public const string Code = "Code";
        public const string Name = "Name";

        /// <summary> 类别 单选、多选、数值、日期时间等 </summary>
        public const string Frmtp = "Frmtp";
        public const string Unit = "Unit";
        public const string HideUnit = "HdUnit";

        /// <summary> 表示格式 </summary>
        public const string PreFormat = "PRFMT";

        /// <summary> 原始数据 </summary>
        public const string Raw = "Raw";

        /// <summary> 受控词汇表(值域代码) </summary>
        public const string CMV = "CMV";

        /// <summary> 受控词汇编码(值编码) </summary>
        public const string CMVVCode = "CMVVCode";

        /// <summary> 痕迹信息(为兼容历史) </summary>
        public const string Trace = "Trace";

        /// <summary> 删除痕迹信息 </summary>
        public const string TraceDel = "TcDel";
        /// <summary> 添加痕迹信息 </summary>
        public const string TraceAdd = "TcAdd";

        /// <summary> 删除痕迹的级别 </summary>
        public const string TraceDelLevel = "TcDelL";
        /// <summary> 添加痕迹的级别 </summary>
        public const string TraceAddLevel = "TcAddL";

        /// <summary> 隐私信息 </summary>
        public const string Secret = "Secret";

        /// <summary> 元素填写后背景色 </summary>
        public static Color DE_CHECKCOLOR = Color.FromArgb(0xF0, 0xF0, 0xF0);

        /// <summary> 元素未填写时背景色 </summary>
        public static Color DE_NOCHECKCOLOR = Color.FromArgb(0xFF, 0xDD, 0x80);
    }

    public static class DeTraceLevel : Object
    {
        /// <summary> 无医师级别 </summary>
        public const string None = "";
        /// <summary> 住院医师 </summary>
        public const string One = "1";
        /// <summary> 主治医师 </summary>
        public const string Two = "2";
        /// <summary> 主任医师 </summary>
        public const string Three = "3";
    }

    /// <summary> 数据元类型 </summary>
    public static class DeFrmtp : Object
    {
        /// <summary> 单选 </summary>
        public const string Radio = "RS";
        /// <summary> 多选 </summary>
        public const string Multiselect = "MS";
        /// <summary> 数值 </summary>
        public const string Number = "N";
        /// <summary> 文本 </summary>
        public const string String = "S";
        /// <summary> 日期 </summary>
        public const string Date = "D";
        /// <summary> 时间 </summary>
        public const string Time = "T";
        /// <summary> 日期时间 </summary>
        public const string DateTime = "DT";
    }

    public enum EmrSyntaxProblem : byte
    {
        espContradiction, espWrong
    }

    /// <summary> 电子病历文本语法信息对象 </summary>
    public class EmrSyntax
    {
        public EmrSyntaxProblem Problem;
        public int Offset, Length;
    }

    public delegate void SyntaxPaintEventHandler(HCCustomData aData, int aItemNo, string aDrawText, EmrSyntax aSyntax, RECT aRect, HCCanvas aCanvas);


    /// <summary> 电子病历文本对象 </summary>
    public class EmrTextItem : HCTextItem 
    {
        private List<EmrSyntax> FSyntaxs;

        public EmrTextItem() : base()
        {

        }

        public EmrTextItem(string aText) : base(aText)
        {

        }

        public void SyntaxAdd(int aOffset, int aLength, EmrSyntaxProblem aProblem)
        {
            EmrSyntax vSyntax = new EmrSyntax();
            vSyntax.Offset = aOffset;
            vSyntax.Length = aLength;
            vSyntax.Problem = aProblem;
            if (FSyntaxs == null)
                FSyntaxs = new List<EmrSyntax>();

            FSyntaxs.Add(vSyntax);
        }

        public void SyntaxClear()
        {
            if (FSyntaxs != null)
                FSyntaxs.Clear();
        }

        public int SyntaxCount()
        {
            if (FSyntaxs != null)
                return FSyntaxs.Count;
            else
                return 0;
        }

        public List<EmrSyntax> Syntaxs
        {
            get { return FSyntaxs; }
        }
    }

    public class DeTraceStyles : HCSet { }

    /// <summary> 电子病历数据元对象 </summary>
    public sealed class DeItem : EmrTextItem  // 不可继承
    {
        private bool
            FMouseIn,
            FOutOfRang,  // 值不在正常范围内
            FIsElement,
            FEditProtect,  // 编辑保护，不允许删除、手动录入
            FCopyProtect,  // 复制保护，不允许复制
            FDeleteAllow,  // 是否允许删除
            FAllocOnly,
            FAllocValue;  // 是否分配过值

        private DeTraceStyles FTraceStyles;
        private Dictionary<string, string> FPropertys;
        private Dictionary<string, string> FScripts;

        private string GetValue(string key)
        {
            if (FPropertys.Keys.Contains(key))
                return FPropertys[key];
            else
                return "";
        }

        private void SetValue(string key, string value)
        {
            if (key == "Text")
            {

            }
            else
            if (key == "EditProtect")
                FEditProtect = bool.Parse(value);
            else
            if (key == "DeleteAllow")
                FDeleteAllow = bool.Parse(value);
            else
            if (key == "CopyProtect")
                FCopyProtect = bool.Parse(value);
            else
            if (key == "AllocValue")
                FAllocValue = bool.Parse(value);
            else
            {
                HC.View.HC.HCSetProperty(FPropertys, key, value);
                PropertyChange();
            }
        }

        private void PropertyChange()
        {
            FIsElement = FPropertys.Keys.Contains(DeProp.Index);
        }

        protected override void SetText(string value)
        {
            //FAllocValue = true;
            if (value != "")
                base.SetText(value);
            else
            {
                if (IsElement && FEditProtect)
                    Text = FPropertys[DeProp.Name];
                else
                    base.SetText("");
            }
        }

        public DeItem() : base()
        {
            FPropertys = new Dictionary<string,string>();
            FScripts = new Dictionary<string, string>();
            FEditProtect = false;
            FDeleteAllow = true;
            FAllocOnly = false;
            FCopyProtect = false;
            FAllocValue = false;
            FOutOfRang = false;
            FMouseIn = false;
            FTraceStyles = new DeTraceStyles();
        }

        public DeItem(string aText) : base(aText)
        {
            FPropertys = new Dictionary<string, string>();
            FScripts = new Dictionary<string, string>();
            FEditProtect = false;
            FDeleteAllow = true;
            FMouseIn = false;
            FTraceStyles = new DeTraceStyles();
        }

        ~DeItem()
        {

        }

        public static void GetSecretRange(string secret, ref int low, ref int hi)
        {
            low = -1;
            hi = -1;

            if (secret == "")
                return;

            int vPos = secret.IndexOf("-");
            if (vPos < 0)  // 2
                low = int.Parse(secret);
            else
            if (vPos == 0)  // -8  -
            {
                low = 1;
                string vS = secret.Substring(vPos + 1, secret.Length - vPos - 1);
                if (vS != "")  // -
                    hi = int.Parse(vS);
            }
            else  // 2-7  3-
            {
                string vS = secret.Substring(0, vPos);
                low = int.Parse(vS);
                vS = secret.Substring(vPos + 1, secret.Length - vPos - 1);
                if (vS != "")
                    hi = int.Parse(vS);
            }
        }

        public override void MouseEnter()
        {
            base.MouseEnter();
            FMouseIn = true;
        }

        public override void MouseLeave()
        {
            base.MouseLeave();
            FMouseIn = false;
        }

        protected override void SetActive(bool value)
        {
            if (!value)
                FMouseIn = false;

            base.SetActive(value);
        }

        public override void Assign(HCCustomItem source)
        {
            base.Assign(source);
            FTraceStyles.Value = (source as DeItem).TraceStyles.Value;
            FEditProtect = (source as DeItem).EditProtect;
            FDeleteAllow = (source as DeItem).DeleteAllow;
            FCopyProtect = (source as DeItem).CopyProtect;
            FAllocOnly = (source as DeItem).AllocOnly;
            FAllocValue = (source as DeItem).AllocValue;
            FOutOfRang = (source as DeItem).OutOfRang;
            HC.View.HC.AssignProperty((source as DeItem).Propertys, ref FPropertys);
            PropertyChange();
            HC.View.HC.AssignProperty((source as DeItem).FScripts, ref FScripts);
        }

        public override bool CanConcatItems(HCCustomItem aItem)
        {
            bool Result = base.CanConcatItems(aItem);
            if (Result)
            {
                DeItem vDeItem = aItem as DeItem;
                Result = ((this[DeProp.Index] == vDeItem[DeProp.Index])
                    && (this.FTraceStyles.Value == vDeItem.TraceStyles.Value)
                    && (FEditProtect == vDeItem.FEditProtect)
                    && (FDeleteAllow == vDeItem.DeleteAllow)
                    && (FCopyProtect == vDeItem.CopyProtect)
                    //&& (FAllocValue == vDeItem.AllocValue)
                    && (this[DeProp.TraceDel] == vDeItem[DeProp.TraceDel])
                    && (this[DeProp.TraceAdd] == vDeItem[DeProp.TraceAdd])
                    && (this[DeProp.TraceDelLevel] == vDeItem[DeProp.TraceDelLevel])
                    && (this[DeProp.TraceAddLevel] == vDeItem[DeProp.TraceAddLevel]));
            }

            return Result;
        }

        public override string GetHint()
        {
            if (FTraceStyles.Value == 0)
                return this[DeProp.Name];
            else
            {
                string vsAdd = this[DeProp.TraceAdd];
                string vsDel = this[DeProp.TraceDel];
                if (vsAdd != "")
                {
                    if (vsDel != "")
                        return vsAdd + "\r\n" + vsDel;
                    else
                        return vsAdd;
                }
                else
                    return vsDel;
            }
        }

        public void DeleteProperty(string propName)
        {
            if (FPropertys.Keys.Contains(propName))
                FPropertys.Remove(propName);
        }

        public override bool AcceptAction(int aOffset, bool aRestrain, HCAction aAction)
        {
            bool vResult = base.AcceptAction(aOffset, aRestrain, aAction);

            if (vResult)
            {
                switch (aAction)
                {
                    case HCAction.actInsertText:
                        if (FEditProtect || FAllocOnly)  // 两头允许输入，触发actConcatText时返回供Data层处理新TextItem还是连接
                            vResult = (aOffset == 0) || (aOffset == this.Length);

                        break;

                    case HCAction.actConcatText:
                        if (FEditProtect)
                            vResult = false;
                        else
                        if (IsElement)
                            vResult = false;

                        break;

                    case HCAction.actBackDeleteText:
                        if (FEditProtect || FAllocOnly)
                            vResult = aOffset == 0;

                        break;

                    case HCAction.actDeleteText:
                        if (FEditProtect || FAllocOnly)
                            vResult = aOffset == this.Length;
                        break;

                    case HCAction.actDeleteItem:
                        return !FEditProtect;
                }
            }

            return vResult;
        }

        public override void SaveToStreamRange(Stream aStream, int aStart, int aEnd)
        {
            base.SaveToStreamRange(aStream, aStart, aEnd);

            byte vByte = 0;
            if (FEditProtect)
                vByte = (byte)(vByte | (1 << 7));

            if (FOutOfRang)
                vByte = (byte)(vByte | (1 << 6));

            if (FCopyProtect)
                vByte = (byte)(vByte | (1 << 5));

            if (FAllocValue)
                vByte = (byte)(vByte | (1 << 4));

            if (FDeleteAllow)
                vByte = (byte)(vByte | (1 << 3));

            if (FAllocOnly)
                vByte = (byte)(vByte | (1 << 2));

            aStream.WriteByte(vByte);

            vByte = FTraceStyles.Value;
            aStream.WriteByte(vByte);

            HC.View.HC.HCSaveTextToStream(aStream, HC.View.HC.GetPropertyString(FPropertys));
            HC.View.HC.HCSaveTextToStream(aStream, HC.View.HC.GetPropertyString(FScripts, HC.View.HC.RecordSeparator, HC.View.HC.UnitSeparator));
        }

        public override void LoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            base.LoadFromStream(aStream, aStyle, aFileVersion);

            byte vByte = (byte)aStream.ReadByte();
            FEditProtect = HC.View.HC.IsOdd(vByte >> 7);
            FOutOfRang = HC.View.HC.IsOdd(vByte >> 6);
            FCopyProtect = HC.View.HC.IsOdd(vByte >> 5);
            FAllocValue = HC.View.HC.IsOdd(vByte >> 4);
            if (aFileVersion > 34)
                FDeleteAllow = HC.View.HC.IsOdd(vByte >> 3);
            else
                FDeleteAllow = true;

            FAllocOnly = HC.View.HC.IsOdd(vByte >> 2);

            vByte = (byte)aStream.ReadByte();
            if (aFileVersion > 46)
                FTraceStyles.Value = vByte;
            else
            {
                if (vByte == 0)
                    FTraceStyles.Value = 0;
                else
                if (vByte == 1)
                    FTraceStyles.Value = (byte)DeTraceStyle.cseDel;
                else
                    FTraceStyles.Value = (byte)DeTraceStyle.cseAdd;
            }

            string vS = "";
            HC.View.HC.HCLoadTextFromStream(aStream, ref vS, aFileVersion);
            HC.View.HC.SetPropertyString(vS, FPropertys);

            if (aFileVersion <= 46)
            {
                if (this[DeProp.Trace] != "")
                {
                    if (vByte == 0)
                    {

                    }
                    else
                    if (vByte == 1)
                        this[DeProp.TraceDel] = this[DeProp.Trace];
                    else
                        this[DeProp.TraceAdd] = this[DeProp.Trace];

                    this[DeProp.Trace] = "";
                }
            }

            PropertyChange();
            if (aFileVersion > 57)
            {
                HC.View.HC.HCLoadTextFromStream(aStream, ref vS, aFileVersion);
                HC.View.HC.SetPropertyString(vS, FScripts, HC.View.HC.RecordSeparator, HC.View.HC.UnitSeparator);
            }
        }

        public override void ToXml(XmlElement aNode)
        {
            base.ToXml(aNode);
            if (FEditProtect)
                aNode.SetAttribute("editprotect", "1");

            if (FOutOfRang)
                aNode.SetAttribute("outofrang", "1");

            if (FCopyProtect)
                aNode.SetAttribute("copyprotect", "1");

            if (FAllocValue)
                aNode.SetAttribute("allocvalue", "1");

            if (FDeleteAllow)
                aNode.SetAttribute("deleteallow", "1");

            if (FAllocOnly)
                aNode.SetAttribute("alloconly", "1");

            aNode.SetAttribute("tracestyles", FTraceStyles.Value.ToString());
            string vS = HC.View.HC.GetPropertyString(FPropertys);
            if (vS != "")
                aNode.SetAttribute("property", vS);
        }

        public override void ParseXml(XmlElement aNode)
        {
            base.ParseXml(aNode);
            FEditProtect = aNode.GetAttribute("editprotect") == "1";
            FOutOfRang = aNode.GetAttribute("outofrang") == "1";
            FCopyProtect = aNode.GetAttribute("copyprotect") == "1";
            FAllocValue = aNode.GetAttribute("allocvalue") == "1";

            if (aNode.HasAttribute("deleteallow"))
                FDeleteAllow = aNode.GetAttribute("deleteallow") == "1";
            else
                FDeleteAllow = true;

            if (aNode.HasAttribute("alloconly"))
                FAllocOnly = aNode.GetAttribute("alloconly") == "1";
            else
                FAllocOnly = false;

            byte vByte = 0;

            if (aNode.HasAttribute("styleex"))
                byte.TryParse(aNode.GetAttribute("styleex"), out vByte);
            else
            if (aNode.HasAttribute("tracestyle"))
            {
                byte.TryParse(aNode.GetAttribute("tracestyle"), out vByte);
                if (vByte == 0)
                    FTraceStyles.Value = 0;
                else
                if (vByte == 1)
                    FTraceStyles.Value = (byte)DeTraceStyle.cseDel;
                else
                    FTraceStyles.Value = (byte)DeTraceStyle.cseAdd;
            }
            else
            if (aNode.HasAttribute("tracestyles"))
            {
                byte.TryParse(aNode.GetAttribute("tracestyles"), out vByte);
                FTraceStyles.Value = vByte;
            }

            if (aNode.HasAttribute("property"))
            {
                string vProp = HC.View.HC.GetXmlRN(aNode.GetAttribute("property"));
                HC.View.HC.SetPropertyString(vProp, FPropertys);
            }
        }

        public void ToJson(string aJsonObj)
        {

        }

        public void ParseJson(string aJsonObj)
        {

        }

        public bool IsElement
        {
            get { return FIsElement; }
        }

        public bool MouseIn
        {
            get { return FMouseIn; }
        }

        public DeTraceStyles TraceStyles
        {
            get { return FTraceStyles; }
            set { FTraceStyles = value; }
        }

        public bool EditProtect
        {
            get { return FEditProtect; }
            set { FEditProtect = value; }
        }

        public bool DeleteAllow
        {
            get { return FDeleteAllow; }
            set { FDeleteAllow = value; }
        }

        public bool CopyProtect
        {
            get { return FCopyProtect; }
            set { FCopyProtect = value; }
        }

        public bool AllocOnly
        {
            get { return FAllocOnly; }
            set { FAllocOnly = value; }
        }

        public bool AllocValue
        {
            get { return FAllocValue; }
            set { FAllocValue = value; }
        }

        public bool OutOfRang
        {
            get { return FOutOfRang; }
            set { FOutOfRang = value; }
        }

        public Dictionary<string, string> Propertys
        {
            get { return FPropertys; }
        }

        public string this[string aKey]
        {
            get { return GetValue(aKey); }
            set { SetValue(aKey, value); }
        }
    }

    public class DeTableRow : HCTableRow
    {
        private bool FEditProtect, FDeleteAllow;
        private Dictionary<string, string> FPropertys;

        private string GetValue(string key)
        {
            if (FPropertys.Keys.Contains(key))
                return FPropertys[key];
            else
                return "";
        }

        private void SetValue(string key, string value)
        {
            HC.View.HC.HCSetProperty(FPropertys, key, value);
        }

        protected override HCTableCell DoCreateCell(HCStyle style)
        {
            return new DeTableCell(style);
        }

        public DeTableRow(HCStyle style, int colCount)
            : base(style, colCount)
        {
            FEditProtect = false;
            FDeleteAllow = true;
            FPropertys = new Dictionary<string, string>();
        }

        ~DeTableRow()
        {

        }

        public override void SaveToStream(Stream stream)
        {
            base.SaveToStream(stream);
            byte vByte = 0;
            if (FEditProtect)
                vByte = (byte)(vByte | (1 << 7));

            if (FDeleteAllow)
                vByte = (byte)(vByte | (1 << 5));

            stream.WriteByte(vByte);
            HC.View.HC.HCSaveTextToStream(stream, HC.View.HC.GetPropertyString(FPropertys));
        }

        public override void LoadFromStream(Stream stream, ushort fileVersion)
        {
            base.LoadFromStream(stream, fileVersion);
            if (fileVersion > 53)
            {
                if (fileVersion > 59)
                {
                    byte vByte = (byte)stream.ReadByte();
                    FEditProtect = HC.View.HC.IsOdd(vByte >> 7);
                    if (fileVersion > 61)
                        FDeleteAllow = HC.View.HC.IsOdd(vByte >> 5);
                }

                string vS = "";
                HC.View.HC.HCLoadTextFromStream(stream, ref vS, fileVersion);
                HC.View.HC.SetPropertyString(vS, FPropertys);
            }
        }

        public override void ToXml(XmlElement aNode)
        {
            base.ToXml(aNode);
            aNode.SetAttribute("property", HC.View.HC.GetPropertyString(FPropertys));
            if (FEditProtect)
                aNode.SetAttribute("editprotect", "1");
        }

        public override void ParseXml(XmlElement aNode)
        {
            base.ParseXml(aNode);
            string vProp = HC.View.HC.GetXmlRN(aNode.Attributes["property"].Value);
            HC.View.HC.SetPropertyString(vProp, FPropertys);
            if (aNode.HasAttribute("editprotect"))
                FEditProtect = aNode.GetAttribute("editprotect") == "1";
            else
                FEditProtect = false;
        }

        public Dictionary<string, string> Propertys
        {
            get { return FPropertys; }
        }

        public string this[string aKey]
        {
            get { return GetValue(aKey); }
            set { SetValue(aKey, value); }
        }

        public bool DeleteAllow
        {
            get { return FDeleteAllow; }
            set { FDeleteAllow = value; }
        }
    }

    public class DeTableCell : HCTableCell
    {
        private bool FEditProtect, FDeleteAllow;
        private Dictionary<string, string> FPropertys;

        private string GetValue(string key)
        {
            if (FPropertys.Keys.Contains(key))
                return FPropertys[key];
            else
                return "";
        }

        private void SetValue(string key, string value)
        {
            HC.View.HC.HCSetProperty(FPropertys, key, value);
        }

        private void SetEditProtect(bool value)
        {
            FEditProtect = value;
            if (this.CellData != null)
                this.CellData.ReadOnly = FEditProtect;
        }

        public DeTableCell(HCStyle style)
            : base(style)
        {
            FEditProtect = false;
            FDeleteAllow = true;
            FPropertys = new Dictionary<string, string>();
        }

        ~DeTableCell()
        {

        }

        public override void SaveToStream(Stream stream)
        {
            base.SaveToStream(stream);
            byte vByte = 0;
            if (FEditProtect)
                vByte = (byte)(vByte | (1 << 7));

            if (FDeleteAllow)
                vByte = (byte)(vByte | (1 << 6));

            stream.WriteByte(vByte);
            HC.View.HC.HCSaveTextToStream(stream, HC.View.HC.GetPropertyString(FPropertys));
        }

        public override void LoadFromStream(Stream stream, HCStyle style, ushort fileVersion)
        {
            base.LoadFromStream(stream, style, fileVersion);
            if (fileVersion > 53)
            {
                if (fileVersion > 56)
                {
                    byte vByte = (byte)stream.ReadByte();
                    FEditProtect = HC.View.HC.IsOdd(vByte >> 7);

                    if (fileVersion > 61)
                        FDeleteAllow = HC.View.HC.IsOdd(vByte >> 6);
                }

                string vS = "";
                HC.View.HC.HCLoadTextFromStream(stream, ref vS, fileVersion);
                HC.View.HC.SetPropertyString(vS, FPropertys);
            }
        }

        public override void ToXml(XmlElement aNode)
        {
            base.ToXml(aNode);
            aNode.SetAttribute("property", HC.View.HC.GetPropertyString(FPropertys));
            if (FEditProtect)
                aNode.SetAttribute("editprotect", "1");
        }

        public override void ParseXml(XmlElement aNode)
        {
            base.ParseXml(aNode);
            string vProp = HC.View.HC.GetXmlRN(aNode.Attributes["property"].Value);
            HC.View.HC.SetPropertyString(vProp, FPropertys);
            if (aNode.HasAttribute("editprotect"))
                FEditProtect = aNode.GetAttribute("editprotect") == "1";
            else
                FEditProtect = false;
        }

        public Dictionary<string, string> Propertys
        {
            get { return FPropertys; }
        }

        public string this[string aKey]
        {
            get { return GetValue(aKey); }
            set { SetValue(aKey, value); }
        }

        public bool EditProtect
        {
            get { return FEditProtect; }
            set { SetEditProtect(value); }
        }

        public bool DeleteAllow
        {
            get { return FDeleteAllow; }
            set { FDeleteAllow = value; }
        }
    }

    public class DeTable : HCTableItem
    {
        private bool FEditProtect, FDeleteAllow;
        private Dictionary<string, string> FPropertys;
        private Dictionary<string, string> FScripts;

        private string GetValue(string key)
        {
            if (FPropertys.Keys.Contains(key))
                return FPropertys[key];
            else
                return "";
        }

        private void SetValue(string key, string  value)
        {
            HC.View.HC.HCSetProperty(FPropertys, key, value);
        }

        protected override void CellChangeByAction(int aRow, int aCol, HCProcedure aProcedure)
        {
            base.CellChangeByAction(aRow, aCol, aProcedure);
        }

        protected override HCTableRow DoCreateRow(HCStyle style, int colCount)
        {
            return new DeTableRow(style, colCount);
        }

        public DeTable(HCCustomData aOwnerData, int aRowCount, int aColCount, int aWidth) 
            : base(aOwnerData, aRowCount, aColCount, aWidth)
        {
            FDeleteAllow = true;
            FPropertys = new Dictionary<string, string>();
            FScripts = new Dictionary<string, string>();
        }

        ~DeTable()
        {

        }

        public override void Assign(HCCustomItem source)
        {
            base.Assign(source);
            FEditProtect = (source as DeTable).EditProtect;
            FDeleteAllow = (source as DeTable).DeleteAllow;
            HC.View.HC.AssignProperty((source as DeTable).Propertys, ref FPropertys);
            HC.View.HC.AssignProperty((source as DeTable).FScripts, ref FScripts);
        }

        public override void SaveToStreamRange(Stream aStream, int aStart, int aEnd)
        {
            base.SaveToStreamRange(aStream, aStart, aEnd);

            byte vByte = 0;
            if (FEditProtect)
                vByte = (byte)(vByte | (1 << 7));

            if (FDeleteAllow)
                vByte = (byte)(vByte | (1 << 6));

            aStream.WriteByte(vByte);

            HC.View.HC.HCSaveTextToStream(aStream, HC.View.HC.GetPropertyString(FPropertys));
            HC.View.HC.HCSaveTextToStream(aStream, HC.View.HC.GetPropertyString(FScripts, HC.View.HC.RecordSeparator, HC.View.HC.UnitSeparator));
        }

        public override void LoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            base.LoadFromStream(aStream, aStyle, aFileVersion);

            byte vByte = (byte)aStream.ReadByte();
            FEditProtect = HC.View.HC.IsOdd(vByte >> 7);

            if (aFileVersion > 34)
                FDeleteAllow = HC.View.HC.IsOdd(vByte >> 6);
            else
                FDeleteAllow = true;

            string vS = "";
            HC.View.HC.HCLoadTextFromStream(aStream, ref vS, aFileVersion);
            HC.View.HC.SetPropertyString(vS, FPropertys);
            if (aFileVersion > 57)
            {
                HC.View.HC.HCLoadTextFromStream(aStream, ref vS, aFileVersion);
                HC.View.HC.SetPropertyString(vS, FScripts, HC.View.HC.RecordSeparator, HC.View.HC.UnitSeparator);
            }
        }

        public override void ToXml(XmlElement aNode)
        {
            base.ToXml(aNode);
            if (FEditProtect)
                aNode.SetAttribute("editprotect", "1");

            if (FDeleteAllow)
                aNode.SetAttribute("deleteallow", "1");

            aNode.SetAttribute("property", HC.View.HC.GetPropertyString(FPropertys));
        }

        public override void ParseXml(XmlElement aNode)
        {
            base.ParseXml(aNode);
            if (aNode.HasAttribute("editprotect"))
                FEditProtect = aNode.GetAttribute("editprotect") == "1";
            else
                FEditProtect = false;

            if (aNode.HasAttribute("deleteallow"))
                FDeleteAllow = aNode.GetAttribute("deleteallow") == "1";
            else
                FDeleteAllow = true;

            string vProp = HC.View.HC.GetXmlRN(aNode.Attributes["property"].Value);
            HC.View.HC.SetPropertyString(vProp, FPropertys);
        }

        public void ToJson(string aJsonObj)
        {

        }

        public void ParseJson(string aJsonObj)
        {

        }

        public bool EditProtect
        {
            get { return FEditProtect; }
            set { FEditProtect = value; }
        }

        public bool DeleteAllow
        {
            get { return FDeleteAllow; }
            set { FDeleteAllow = value; }
        }

        public Dictionary<string, string> Propertys
        {
            get { return FPropertys; }
        }

        public string this[string aKey]
        {
            get { return GetValue(aKey); }
            set { SetValue(aKey, value); }
        }
    }

    public class DeCheckBox : HCCheckBoxItem
    {

        private bool FEditProtect, FDeleteAllow;

        private Dictionary<string, string> FPropertys;
        private Dictionary<string, string> FScripts;

        private string GetValue(string key)
        {
            if (FPropertys.Keys.Contains(key))
                return FPropertys[key];
            else
                return "";
        }

        private void SetValue(string key, string value)
        {
            HC.View.HC.HCSetProperty(FPropertys, key, value);
        }

        protected override void DoSetChecked(bool value)
        {
            if (!EditProtect)
                base.DoSetChecked(value);
        }

        public DeCheckBox(HCCustomData aOwnerData, string aText, bool aChecked) : base(aOwnerData, aText, aChecked)
        {
            FDeleteAllow = true;
            FPropertys = new Dictionary<string, string>();
            FScripts = new Dictionary<string, string>();
        }

        ~DeCheckBox()
        {

        }

        public override void Assign(HCCustomItem source)
        {
            base.Assign(source);
            FEditProtect = (source as DeCheckBox).EditProtect;
            FDeleteAllow = (source as DeCheckBox).DeleteAllow;
            HC.View.HC.AssignProperty((source as DeCheckBox).Propertys, ref FPropertys);
            HC.View.HC.AssignProperty((source as DeCheckBox).FScripts, ref FScripts);
        }

        public override void SaveToStreamRange(Stream aStream, int aStart, int aEnd)
        {
            base.SaveToStreamRange(aStream, aStart, aEnd);

            byte vByte = 0;
            if (FEditProtect)
                vByte = (byte)(vByte | (1 << 7));

            if (FDeleteAllow)
                vByte = (byte)(vByte | (1 << 6));

            aStream.WriteByte(vByte);

            HC.View.HC.HCSaveTextToStream(aStream, HC.View.HC.GetPropertyString(FPropertys));
            HC.View.HC.HCSaveTextToStream(aStream, HC.View.HC.GetPropertyString(FScripts, HC.View.HC.RecordSeparator, HC.View.HC.UnitSeparator));
        }

        public override void LoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            base.LoadFromStream(aStream, aStyle, aFileVersion);

            byte vByte = (byte)aStream.ReadByte();
            FEditProtect = HC.View.HC.IsOdd(vByte >> 7);

            if (aFileVersion > 34)
                FDeleteAllow = HC.View.HC.IsOdd(vByte >> 6);
            else
                FDeleteAllow = true;

            string vS = "";
            HC.View.HC.HCLoadTextFromStream(aStream, ref vS, aFileVersion);
            HC.View.HC.SetPropertyString(vS, FPropertys);
            if (aFileVersion > 57)
            {
                HC.View.HC.HCLoadTextFromStream(aStream, ref vS, aFileVersion);
                HC.View.HC.SetPropertyString(vS, FScripts, HC.View.HC.RecordSeparator, HC.View.HC.UnitSeparator);
            }
        }

        public override void ToXml(XmlElement aNode)
        {
            base.ToXml(aNode);
            if (FEditProtect)
                aNode.SetAttribute("editprotect", "1");

            if (FDeleteAllow)
                aNode.SetAttribute("deleteallow", "1");

            aNode.SetAttribute("property", HC.View.HC.GetPropertyString(FPropertys));
        }

        public override void ParseXml(XmlElement aNode)
        {
            base.ParseXml(aNode);
            FEditProtect = aNode.GetAttribute("editprotect") == "1";

            if (aNode.HasAttribute("deleteallow"))
                FDeleteAllow = aNode.GetAttribute("deleteallow") == "1";
            else
                FDeleteAllow = true;

            string vProp = HC.View.HC.GetXmlRN(aNode.Attributes["property"].Value);
            HC.View.HC.SetPropertyString(vProp, FPropertys);
        }

        public void ToJson(string aJsonObj)
        {

        }

        public void ParseJson(string aJsonObj)
        {

        }

        public bool EditProtect
        {
            get { return FEditProtect; }
            set { FEditProtect = value; }
        }

        public bool DeleteAllow
        {
            get { return FDeleteAllow; }
            set { FDeleteAllow = value; }
        }

        public Dictionary<string, string> Propertys
        {
            get { return FPropertys; }
        }

        public string this[string aKey]
        {
            get { return GetValue(aKey); }
            set { SetValue(aKey, value); }
        }
    }

    public class DeButton : HCButtonItem
    {
        private bool FEditProtect, FDeleteAllow;
        private Dictionary<string, string> FPropertys;
        private Dictionary<string, string> FScripts;

        private string GetValue(string key)
        {
            if (FPropertys.Keys.Contains(key))
                return FPropertys[key];
            else
                return "";
        }

        private void SetValue(string key, string value)
        {
            HC.View.HC.HCSetProperty(FPropertys, key, value);
        }

        public DeButton(HCCustomData aOwnerData, string aText) : base(aOwnerData, aText)
        {
            FDeleteAllow = true;
            FPropertys = new Dictionary<string, string>();
            FScripts = new Dictionary<string, string>();
        }

        ~DeButton()
        {

        }

        public override void Assign(HCCustomItem source)
        {
            base.Assign(source);
            FEditProtect = (source as DeEdit).EditProtect;
            FDeleteAllow = (source as DeEdit).DeleteAllow;
            HC.View.HC.AssignProperty((source as DeButton).Propertys, ref FPropertys);
            HC.View.HC.AssignProperty((source as DeButton).FScripts, ref FScripts);
        }

        public override void SaveToStreamRange(Stream aStream, int aStart, int aEnd)
        {
            base.SaveToStreamRange(aStream, aStart, aEnd);

            byte vByte = 0;
            if (FEditProtect)
                vByte = (byte)(vByte | (1 << 7));

            if (FDeleteAllow)
                vByte = (byte)(vByte | (1 << 6));

            aStream.WriteByte(vByte);

            HC.View.HC.HCSaveTextToStream(aStream, HC.View.HC.GetPropertyString(FPropertys));
            HC.View.HC.HCSaveTextToStream(aStream, HC.View.HC.GetPropertyString(FScripts, HC.View.HC.RecordSeparator, HC.View.HC.UnitSeparator));
        }

        public override void LoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            base.LoadFromStream(aStream, aStyle, aFileVersion);

            byte vByte = (byte)aStream.ReadByte();
            FEditProtect = HC.View.HC.IsOdd(vByte >> 7);

            if (aFileVersion > 34)
                FDeleteAllow = HC.View.HC.IsOdd(vByte >> 6);
            else
                FDeleteAllow = true;

            string vS = "";
            HC.View.HC.HCLoadTextFromStream(aStream, ref vS, aFileVersion);
            HC.View.HC.SetPropertyString(vS, FPropertys);
            if (aFileVersion > 57)
            {
                HC.View.HC.HCLoadTextFromStream(aStream, ref vS, aFileVersion);
                HC.View.HC.SetPropertyString(vS, FScripts, HC.View.HC.RecordSeparator, HC.View.HC.UnitSeparator);
            }
        }

        public override void ToXml(XmlElement aNode)
        {
            base.ToXml(aNode);
            if (FEditProtect)
                aNode.SetAttribute("editprotect", "1");

            if (FDeleteAllow)
                aNode.SetAttribute("deleteallow", "1");

            aNode.SetAttribute("property", HC.View.HC.GetPropertyString(FPropertys));
        }

        public override void ParseXml(XmlElement aNode)
        {
            base.ParseXml(aNode);
            FEditProtect = aNode.GetAttribute("editprotect") == "1";

            if (aNode.HasAttribute("deleteallow"))
                FDeleteAllow = aNode.GetAttribute("deleteallow") == "1";
            else
                FDeleteAllow = true;

            string vProp = HC.View.HC.GetXmlRN(aNode.Attributes["property"].Value);
            HC.View.HC.SetPropertyString(vProp, FPropertys);
        }

        public void ToJson(string aJsonObj)
        {

        }

        public void ParseJson(string aJsonObj)
        {

        }

        public bool EditProtect
        {
            get { return FEditProtect; }
            set { FEditProtect = value; }
        }

        public bool DeleteAllow
        {
            get { return FDeleteAllow; }
            set { FDeleteAllow = value; }
        }

        public Dictionary<string, string> Propertys
        {
            get { return FPropertys; }
        }

        public string this[string aKey]
        {
            get { return GetValue(aKey); }
            set { SetValue(aKey, value); }
        }
    }

    public class DeEdit : HCEditItem
    {
        private bool FEditProtect, FDeleteAllow;
        private Dictionary<string, string> FPropertys;
        private Dictionary<string, string> FScripts;

        private string GetValue(string key)
        {
            if (FPropertys.Keys.Contains(key))
                return FPropertys[key];
            else
                return "";
        }

        private void SetValue(string key, string value)
        {
            HC.View.HC.HCSetProperty(FPropertys, key, value);
        }

        public override void KeyDown(KeyEventArgs e)
        {
            if (!FEditProtect)
                base.KeyDown(e);
        }

        public override void KeyPress(ref char key)
        {
            if (!FEditProtect)
                base.KeyPress(ref key);
        }

        public DeEdit(HCCustomData aOwnerData, string aText) : base(aOwnerData, aText)
        {
            FDeleteAllow = true;
            FPropertys = new Dictionary<string, string>();
            FScripts = new Dictionary<string, string>();
        }

        ~DeEdit()
        {

        }

        public override void Assign(HCCustomItem source)
        {
            base.Assign(source);
            FEditProtect = (source as DeEdit).EditProtect;
            FDeleteAllow = (source as DeEdit).DeleteAllow;
            HC.View.HC.AssignProperty((source as DeEdit).Propertys, ref FPropertys);
            HC.View.HC.AssignProperty((source as DeEdit).FScripts, ref FScripts);
        }

        public override bool InsertText(string aText)
        {
            if (!FEditProtect)
                return base.InsertText(aText);
            else
                return false;
        }

        public override void SaveToStreamRange(Stream aStream, int aStart, int aEnd)
        {
            base.SaveToStreamRange(aStream, aStart, aEnd);

            byte vByte = 0;
            if (FEditProtect)
                vByte = (byte)(vByte | (1 << 7));

            if (FDeleteAllow)
                vByte = (byte)(vByte | (1 << 6));

            aStream.WriteByte(vByte);

            HC.View.HC.HCSaveTextToStream(aStream, HC.View.HC.GetPropertyString(FPropertys));
            HC.View.HC.HCSaveTextToStream(aStream, HC.View.HC.GetPropertyString(FScripts, HC.View.HC.RecordSeparator, HC.View.HC.UnitSeparator));
        }

        public override void LoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            base.LoadFromStream(aStream, aStyle, aFileVersion);

            byte vByte = (byte)aStream.ReadByte();
            FEditProtect = HC.View.HC.IsOdd(vByte >> 7);

            if (aFileVersion > 34)
                FDeleteAllow = HC.View.HC.IsOdd(vByte >> 6);
            else
                FDeleteAllow = true;

            string vS = "";
            HC.View.HC.HCLoadTextFromStream(aStream, ref vS, aFileVersion);
            HC.View.HC.SetPropertyString(vS, FPropertys);
            if (aFileVersion > 57)
            {
                HC.View.HC.HCLoadTextFromStream(aStream, ref vS, aFileVersion);
                HC.View.HC.SetPropertyString(vS, FScripts, HC.View.HC.RecordSeparator, HC.View.HC.UnitSeparator);
            }
        }

        public override void ToXml(XmlElement aNode)
        {
            base.ToXml(aNode);
            if (FEditProtect)
                aNode.SetAttribute("editprotect", "1");

            if (FDeleteAllow)
                aNode.SetAttribute("deleteallow", "1");

            aNode.SetAttribute("property", HC.View.HC.GetPropertyString(FPropertys));
        }

        public override void ParseXml(XmlElement aNode)
        {
            base.ParseXml(aNode);
            FEditProtect = aNode.GetAttribute("editprotect") == "1";

            if (aNode.HasAttribute("deleteallow"))
                FDeleteAllow = aNode.GetAttribute("deleteallow") == "1";
            else
                FDeleteAllow = true;

            string vProp = HC.View.HC.GetXmlRN(aNode.Attributes["property"].Value);
            HC.View.HC.SetPropertyString(vProp, FPropertys);
        }

        public void ToJson(string aJsonObj)
        {

        }

        public void ParseJson(string aJsonObj)
        {

        }

        public bool EditProtect
        {
            get { return FEditProtect; }
            set { FEditProtect = value; }
        }

        public bool DeleteAllow
        {
            get { return FDeleteAllow; }
            set { FDeleteAllow = value; }
        }

        public Dictionary<string, string> Propertys
        {
            get { return FPropertys; }
        }

        public string this[string aKey]
        {
            get { return GetValue(aKey); }
            set { SetValue(aKey, value); }
        }
    }

    public class DeCombobox : HCComboboxItem
    {
        private bool FEditProtect, FDeleteAllow;
        private Dictionary<string, string> FPropertys;
        private Dictionary<string, string> FScripts;

        private string GetValue(string key)
        {
            if (FPropertys.Keys.Contains(key))
                return FPropertys[key];
            else
                return "";
        }

        private void SetValue(string key, string value)
        {
            HC.View.HC.HCSetProperty(FPropertys, key, value);
        }

        public override void KeyDown(KeyEventArgs e)
        {
            if (!FEditProtect)
                base.KeyDown(e);
        }

        public override void KeyPress(ref char key)
        {
            if (!FEditProtect)
                base.KeyPress(ref key);
        }

        protected override void DoPopup()
        {
            if (!FEditProtect)
                base.DoPopup();
        }

        public DeCombobox(HCCustomData aOwnerData, string aText) : base(aOwnerData, aText)
        {
            FDeleteAllow = true;
            FPropertys = new Dictionary<string, string>();
            FScripts = new Dictionary<string, string>();
            SaveItem = false;
        }

        ~DeCombobox()
        {

        }

        public override void Assign(HCCustomItem source)
        {
            base.Assign(source);
            FEditProtect = (source as DeCombobox).EditProtect;
            FDeleteAllow = (source as DeCombobox).DeleteAllow;
            HC.View.HC.AssignProperty((source as DeCombobox).Propertys, ref FPropertys);
            HC.View.HC.AssignProperty((source as DeCombobox).FScripts, ref FScripts);
        }

        public override bool InsertText(string aText)
        {
            if (!FEditProtect)
                return base.InsertText(aText);
            else
                return false;
        }

        public override void SaveToStreamRange(Stream aStream, int aStart, int aEnd)
        {
            base.SaveToStreamRange(aStream, aStart, aEnd);

            byte vByte = 0;
            if (FEditProtect)
                vByte = (byte)(vByte | (1 << 7));

            if (FDeleteAllow)
                vByte = (byte)(vByte | (1 << 6));

            aStream.WriteByte(vByte);

            HC.View.HC.HCSaveTextToStream(aStream, HC.View.HC.GetPropertyString(FPropertys));
            HC.View.HC.HCSaveTextToStream(aStream, HC.View.HC.GetPropertyString(FScripts, HC.View.HC.RecordSeparator, HC.View.HC.UnitSeparator));
        }

        public override void LoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            base.LoadFromStream(aStream, aStyle, aFileVersion);

            byte vByte = (byte)aStream.ReadByte();
            FEditProtect = HC.View.HC.IsOdd(vByte >> 7);

            if (aFileVersion > 34)
                FDeleteAllow = HC.View.HC.IsOdd(vByte >> 6);
            else
                FDeleteAllow = true;

            string vS = "";
            HC.View.HC.HCLoadTextFromStream(aStream, ref vS, aFileVersion);
            HC.View.HC.SetPropertyString(vS, FPropertys);
            if (aFileVersion > 57)
            {
                HC.View.HC.HCLoadTextFromStream(aStream, ref vS, aFileVersion);
                HC.View.HC.SetPropertyString(vS, FScripts, HC.View.HC.RecordSeparator, HC.View.HC.UnitSeparator);
            }
        }

        public override void ToXml(XmlElement aNode)
        {
            base.ToXml(aNode);
            if (FEditProtect)
                aNode.SetAttribute("editprotect", "1");

            if (FDeleteAllow)
                aNode.SetAttribute("deleteallow", "1");

            aNode.SetAttribute("property", HC.View.HC.GetPropertyString(FPropertys));
        }

        public override void ParseXml(XmlElement aNode)
        {
            base.ParseXml(aNode);
            FEditProtect = aNode.GetAttribute("editprotect") == "1";

            if (aNode.HasAttribute("deleteallow"))
                FDeleteAllow = aNode.GetAttribute("deleteallow") == "1";
            else
                FDeleteAllow = true;

            string vProp = HC.View.HC.GetXmlRN(aNode.Attributes["property"].Value);
            HC.View.HC.SetPropertyString(vProp, FPropertys);
        }

        public void ToJson(string aJsonObj)
        {

        }

        public void ParseJson(string aJsonObj)
        {

        }

        public bool EditProtect
        {
            get { return FEditProtect; }
            set { FEditProtect = value; }
        }

        public bool DeleteAllow
        {
            get { return FDeleteAllow; }
            set { FDeleteAllow = value; }
        }

        public Dictionary<string, string> Propertys
        {
            get { return FPropertys; }
        }

        public string this[string aKey]
        {
            get { return GetValue(aKey); }
            set { SetValue(aKey, value); }
        }
    }

    public class DeDateTimePicker : HCDateTimePicker
    {
        private bool FEditProtect, FDeleteAllow;
        private Dictionary<string, string> FPropertys;
        private Dictionary<string, string> FScripts;

        private string GetValue(string key)
        {
            if (FPropertys.Keys.Contains(key))
                return FPropertys[key];
            else
                return "";
        }

        private void SetValue(string key, string value)
        {
            HC.View.HC.HCSetProperty(FPropertys, key, value);
        }

        protected override void DoPopup()
        {
            if (!FEditProtect)
                base.DoPopup();
        }

        public override void KeyDown(KeyEventArgs e)
        {
            if (!FEditProtect)
                base.KeyDown(e);
        }

        public override void KeyPress(ref char key)
        {
            if (!FEditProtect)
                base.KeyPress(ref key);
        }

        public DeDateTimePicker(HCCustomData aOwnerData, DateTime aDateTime) : base(aOwnerData, aDateTime)
        {
            FDeleteAllow = true;
            FPropertys = new Dictionary<string, string>();
            FScripts = new Dictionary<string, string>();
        }

        ~DeDateTimePicker()
        {

        }

        public override void Assign(HCCustomItem source)
        {
            base.Assign(source);
            FEditProtect = (source as DeDateTimePicker).EditProtect;
            FDeleteAllow = (source as DeDateTimePicker).DeleteAllow;
            HC.View.HC.AssignProperty((source as DeDateTimePicker).Propertys, ref FPropertys);
            HC.View.HC.AssignProperty((source as DeDateTimePicker).FScripts, ref FScripts);
        }

        public override bool InsertText(string aText)
        {
            if (!FEditProtect)
                return base.InsertText(aText);
            else
                return false;
        }

        public override void SaveToStreamRange(Stream aStream, int aStart, int aEnd)
        {
            base.SaveToStreamRange(aStream, aStart, aEnd);

            byte vByte = 0;
            if (FEditProtect)
                vByte = (byte)(vByte | (1 << 7));

            if (FDeleteAllow)
                vByte = (byte)(vByte | (1 << 6));

            aStream.WriteByte(vByte);

            HC.View.HC.HCSaveTextToStream(aStream, HC.View.HC.GetPropertyString(FPropertys));
            HC.View.HC.HCSaveTextToStream(aStream, HC.View.HC.GetPropertyString(FScripts, HC.View.HC.RecordSeparator, HC.View.HC.UnitSeparator));
        }

        public override void LoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            base.LoadFromStream(aStream, aStyle, aFileVersion);

            byte vByte = (byte)aStream.ReadByte();
            FEditProtect = HC.View.HC.IsOdd(vByte >> 7);

            if (aFileVersion > 34)
                FDeleteAllow = HC.View.HC.IsOdd(vByte >> 6);
            else
                FDeleteAllow = true;

            string vS = "";
            HC.View.HC.HCLoadTextFromStream(aStream, ref vS, aFileVersion);
            HC.View.HC.SetPropertyString(vS, FPropertys);
            if (aFileVersion > 57)
            {
                HC.View.HC.HCLoadTextFromStream(aStream, ref vS, aFileVersion);
                HC.View.HC.SetPropertyString(vS, FScripts, HC.View.HC.RecordSeparator, HC.View.HC.UnitSeparator);
            }
        }

        public override void ToXml(XmlElement aNode)
        {
            base.ToXml(aNode);
            if (FEditProtect)
                aNode.SetAttribute("editprotect", "1");

            if (FDeleteAllow)
                aNode.SetAttribute("deleteallow", "1");

            aNode.SetAttribute("property", HC.View.HC.GetPropertyString(FPropertys));
        }

        public override void ParseXml(XmlElement aNode)
        {
            base.ParseXml(aNode);
            FEditProtect = aNode.GetAttribute("editprotect") == "1";

            if (aNode.HasAttribute("deleteallow"))
                FDeleteAllow = aNode.GetAttribute("deleteallow") == "1";
            else
                FDeleteAllow = true;

            string vProp = HC.View.HC.GetXmlRN(aNode.Attributes["property"].Value);
            HC.View.HC.SetPropertyString(vProp, FPropertys);
        }

        public  void ToJson(string aJsonObj)
        {

        }

        public  void ParseJson(string aJsonObj)
        {

        }

        public bool EditProtect
        {
            get { return FEditProtect; }
            set { FEditProtect = value; }
        }

        public bool DeleteAllow
        {
            get { return FDeleteAllow; }
            set { FDeleteAllow = value; }
        }

        public Dictionary<string, string> Propertys
        {
            get { return FPropertys; }
        }

        public string this[string aKey]
        {
            get { return GetValue(aKey); }
            set { SetValue(aKey, value); }
        }
    }

    public class DeRadioGroup : HCRadioGroup
    {
        private bool FEditProtect, FDeleteAllow;
        private Dictionary<string, string> FPropertys;
        private Dictionary<string, string> FScripts;

        private string GetValue(string key)
        {
            if (FPropertys.Keys.Contains(key))
                return FPropertys[key];
            else
                return "";
        }

        private void SetValue(string key, string value)
        {
            HC.View.HC.HCSetProperty(FPropertys, key, value);
        }

        protected override void DoSetItemChecked(int index, bool value)
        {
            if (!FEditProtect)
                base.DoSetItemChecked(index, value);
        }

        public DeRadioGroup(HCCustomData aOwnerData) : base(aOwnerData)
        {
            FDeleteAllow = true;
            FPropertys = new Dictionary<string, string>();
            FScripts = new Dictionary<string, string>();
        }

        ~DeRadioGroup()
        {

        }

        public override void Assign(HCCustomItem source)
        {
            base.Assign(source);
            FEditProtect = (source as DeRadioGroup).EditProtect;
            FDeleteAllow = (source as DeRadioGroup).DeleteAllow;
            HC.View.HC.AssignProperty((source as DeRadioGroup).Propertys, ref FPropertys);
            HC.View.HC.AssignProperty((source as DeRadioGroup).FScripts, ref FScripts);
        }

        public override void SaveToStreamRange(Stream aStream, int aStart, int aEnd)
        {
            base.SaveToStreamRange(aStream, aStart, aEnd);

            byte vByte = 0;
            if (FEditProtect)
                vByte = (byte)(vByte | (1 << 7));

            if (FDeleteAllow)
                vByte = (byte)(vByte | (1 << 6));

            aStream.WriteByte(vByte);

            HC.View.HC.HCSaveTextToStream(aStream, HC.View.HC.GetPropertyString(FPropertys));
            HC.View.HC.HCSaveTextToStream(aStream, HC.View.HC.GetPropertyString(FScripts, HC.View.HC.RecordSeparator, HC.View.HC.UnitSeparator));
        }

        public override void LoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            base.LoadFromStream(aStream, aStyle, aFileVersion);

            byte vByte = (byte)aStream.ReadByte();
            FEditProtect = HC.View.HC.IsOdd(vByte >> 7);

            if (aFileVersion > 34)
                FDeleteAllow = HC.View.HC.IsOdd(vByte >> 6);
            else
                FDeleteAllow = true;

            string vS = "";
            HC.View.HC.HCLoadTextFromStream(aStream, ref vS, aFileVersion);
            HC.View.HC.SetPropertyString(vS, FPropertys);
            if (aFileVersion > 57)
            {
                HC.View.HC.HCLoadTextFromStream(aStream, ref vS, aFileVersion);
                HC.View.HC.SetPropertyString(vS, FScripts, HC.View.HC.RecordSeparator, HC.View.HC.UnitSeparator);
            }
        }

        public override void ToXml(XmlElement aNode)
        {
            base.ToXml(aNode);
            if (FEditProtect)
                aNode.SetAttribute("editprotect", "1");

            if (FDeleteAllow)
                aNode.SetAttribute("deleteallow", "1");

            aNode.SetAttribute("property", HC.View.HC.GetPropertyString(FPropertys));
        }

        public override void ParseXml(XmlElement aNode)
        {
            base.ParseXml(aNode);
            FEditProtect = aNode.GetAttribute("editprotect") == "1";

            if (aNode.HasAttribute("deleteallow"))
                FDeleteAllow = aNode.GetAttribute("deleteallow") == "1";
            else
                FDeleteAllow = true;

            string vProp = HC.View.HC.GetXmlRN(aNode.Attributes["property"].Value);
            HC.View.HC.SetPropertyString(vProp, FPropertys);
        }

        public void ToJson(string aJsonObj)
        {

        }

        public void ParseJson(string aJsonObj)
        {

        }

        public bool EditProtect
        {
            get { return FEditProtect; }
            set { FEditProtect = value; }
        }

        public bool DeleteAllow
        {
            get { return FDeleteAllow; }
            set { FDeleteAllow = value; }
        }

        public Dictionary<string, string> Propertys
        {
            get { return FPropertys; }
        }

        public string this[string aKey]
        {
            get { return GetValue(aKey); }
            set { SetValue(aKey, value); }
        }
    }

    public class DeBarCodeItem : HCBarCodeItem
    {
        private bool FEditProtect, FDeleteAllow;
        private Dictionary<string, string> FPropertys;

        private string GetValue(string key)
        {
            if (FPropertys.Keys.Contains(key))
                return FPropertys[key];
            else
                return "";
        }

        private void SetValue(string key, string value)
        {
            HC.View.HC.HCSetProperty(FPropertys, key, value);
        }

        public DeBarCodeItem(HCCustomData aOwnerData, string text) : base(aOwnerData, text)
        {
            FDeleteAllow = true;
            FPropertys = new Dictionary<string, string>();
        }

        ~DeBarCodeItem()
        {

        }

        public override void Assign(HCCustomItem source)
        {
            base.Assign(source);
            FEditProtect = (source as DeBarCodeItem).EditProtect;
            FDeleteAllow = (source as DeBarCodeItem).DeleteAllow;
            HC.View.HC.AssignProperty((source as DeBarCodeItem).Propertys, ref FPropertys);
        }

        public override void SaveToStreamRange(Stream aStream, int aStart, int aEnd)
        {
            base.SaveToStreamRange(aStream, aStart, aEnd);

            byte vByte = 0;
            if (FEditProtect)
                vByte = (byte)(vByte | (1 << 7));

            if (FDeleteAllow)
                vByte = (byte)(vByte | (1 << 6));

            aStream.WriteByte(vByte);

            HC.View.HC.HCSaveTextToStream(aStream, HC.View.HC.GetPropertyString(FPropertys));
        }

        public override void LoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            base.LoadFromStream(aStream, aStyle, aFileVersion);
            if (aFileVersion > 60)
            {
                byte vByte = (byte)aStream.ReadByte();
                FEditProtect = HC.View.HC.IsOdd(vByte >> 7);
                FDeleteAllow = HC.View.HC.IsOdd(vByte >> 6);

                string vS = "";
                HC.View.HC.HCLoadTextFromStream(aStream, ref vS, aFileVersion);
                HC.View.HC.SetPropertyString(vS, FPropertys);
            }
        }

        public override void ToXml(XmlElement aNode)
        {
            base.ToXml(aNode);
            if (FEditProtect)
                aNode.SetAttribute("editprotect", "1");

            if (FDeleteAllow)
                aNode.SetAttribute("deleteallow", "1");

            aNode.SetAttribute("property", HC.View.HC.GetPropertyString(FPropertys));
        }

        public override void ParseXml(XmlElement aNode)
        {
            base.ParseXml(aNode);
            FEditProtect = aNode.GetAttribute("editprotect") == "1";

            if (aNode.HasAttribute("deleteallow"))
                FDeleteAllow = aNode.GetAttribute("deleteallow") == "1";
            else
                FDeleteAllow = true;

            if (aNode.HasAttribute("property"))
            {
                string vProp = HC.View.HC.GetXmlRN(aNode.Attributes["property"].Value);
                HC.View.HC.SetPropertyString(vProp, FPropertys);
            }
        }

        public void ToJson(string aJsonObj)
        {

        }

        public void ParseJson(string aJsonObj)
        {

        }

        public bool EditProtect
        {
            get { return FEditProtect; }
            set { FEditProtect = value; }
        }

        public bool DeleteAllow
        {
            get { return FDeleteAllow; }
            set { FDeleteAllow = value; }
        }

        public Dictionary<string, string> Propertys
        {
            get { return FPropertys; }
        }

        public string this[string aKey]
        {
            get { return GetValue(aKey); }
            set { SetValue(aKey, value); }
        }
    }

    public class DeQRCodeItem : HCQRCodeItem
    {
        private bool FEditProtect, FDeleteAllow;
        private Dictionary<string, string> FPropertys;

        private string GetValue(string key)
        {
            if (FPropertys.Keys.Contains(key))
                return FPropertys[key];
            else
                return "";
        }

        private void SetValue(string key, string value)
        {
            FPropertys[key] = value;
        }

        public DeQRCodeItem(HCCustomData aOwnerData, string text) : base(aOwnerData, text)
        {
            FDeleteAllow = true;
            FPropertys = new Dictionary<string, string>();
        }

        ~DeQRCodeItem()
        {

        }

        public override void Assign(HCCustomItem source)
        {
            base.Assign(source);
            FEditProtect = (source as DeQRCodeItem).EditProtect;
            FDeleteAllow = (source as DeQRCodeItem).DeleteAllow;
            HC.View.HC.AssignProperty((source as DeQRCodeItem).Propertys, ref FPropertys);
        }

        public override void SaveToStreamRange(Stream aStream, int aStart, int aEnd)
        {
            base.SaveToStreamRange(aStream, aStart, aEnd);

            byte vByte = 0;
            if (FEditProtect)
                vByte = (byte)(vByte | (1 << 7));

            if (FDeleteAllow)
                vByte = (byte)(vByte | (1 << 6));

            aStream.WriteByte(vByte);

            HC.View.HC.HCSaveTextToStream(aStream, HC.View.HC.GetPropertyString(FPropertys));
        }

        public override void LoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            base.LoadFromStream(aStream, aStyle, aFileVersion);
            if (aFileVersion > 60)
            {
                byte vByte = (byte)aStream.ReadByte();
                FEditProtect = HC.View.HC.IsOdd(vByte >> 7);
                FDeleteAllow = HC.View.HC.IsOdd(vByte >> 6);

                string vS = "";
                HC.View.HC.HCLoadTextFromStream(aStream, ref vS, aFileVersion);
                HC.View.HC.SetPropertyString(vS, FPropertys);
            }
        }

        public override void ToXml(XmlElement aNode)
        {
            base.ToXml(aNode);
            if (FEditProtect)
                aNode.SetAttribute("editprotect", "1");

            if (FDeleteAllow)
                aNode.SetAttribute("deleteallow", "1");

            aNode.SetAttribute("property", HC.View.HC.GetPropertyString(FPropertys));
        }

        public override void ParseXml(XmlElement aNode)
        {
            base.ParseXml(aNode);
            FEditProtect = aNode.GetAttribute("editprotect") == "1";

            if (aNode.HasAttribute("deleteallow"))
                FDeleteAllow = aNode.GetAttribute("deleteallow") == "1";
            else
                FDeleteAllow = true;

            if (aNode.HasAttribute("property"))
            {
                string vProp = HC.View.HC.GetXmlRN(aNode.Attributes["property"].Value);
                HC.View.HC.SetPropertyString(vProp, FPropertys);
            }
        }

        public void ToJson(string aJsonObj)
        {

        }

        public void ParseJson(string aJsonObj)
        {

        }

        public bool EditProtect
        {
            get { return FEditProtect; }
            set { FEditProtect = value; }
        }

        public bool DeleteAllow
        {
            get { return FDeleteAllow; }
            set { FDeleteAllow = value; }
        }

        public Dictionary<string, string> Propertys
        {
            get { return FPropertys; }
        }

        public string this[string aKey]
        {
            get { return GetValue(aKey); }
            set { SetValue(aKey, value); }
        }
    }

    public class DeFloatBarCodeItem : HCFloatBarCodeItem
    {
        private bool FEditProtect, FDeleteAllow;
        private Dictionary<string, string> FPropertys;

        private string GetValue(string key)
        {
            if (FPropertys.Keys.Contains(key))
                return FPropertys[key];
            else
                return "";
        }

        private void SetValue(string key, string value)
        {
            HC.View.HC.HCSetProperty(FPropertys, key, value);
        }

        public DeFloatBarCodeItem(HCCustomData aOwnerData) : base(aOwnerData)
        {
            FDeleteAllow = true;
            FPropertys = new Dictionary<string, string>();
        }

        ~DeFloatBarCodeItem()
        {

        }

        public override void Assign(HCCustomItem source)
        {
            base.Assign(source);
            FEditProtect = (source as DeFloatBarCodeItem).EditProtect;
            FDeleteAllow = (source as DeFloatBarCodeItem).DeleteAllow;
            HC.View.HC.AssignProperty((source as DeFloatBarCodeItem).Propertys, ref FPropertys);
        }

        public override void SaveToStreamRange(Stream aStream, int aStart, int aEnd)
        {
            base.SaveToStreamRange(aStream, aStart, aEnd);

            byte vByte = 0;
            if (FEditProtect)
                vByte = (byte)(vByte | (1 << 7));

            if (FDeleteAllow)
                vByte = (byte)(vByte | (1 << 6));

            aStream.WriteByte(vByte);

            HC.View.HC.HCSaveTextToStream(aStream, HC.View.HC.GetPropertyString(FPropertys));
        }

        public override void LoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            base.LoadFromStream(aStream, aStyle, aFileVersion);

            byte vByte = (byte)aStream.ReadByte();
            FEditProtect = HC.View.HC.IsOdd(vByte >> 7);

            if (aFileVersion > 34)
                FDeleteAllow = HC.View.HC.IsOdd(vByte >> 6);
            else
                FDeleteAllow = true;

            string vS = "";
            HC.View.HC.HCLoadTextFromStream(aStream, ref vS, aFileVersion);
            HC.View.HC.SetPropertyString(vS, FPropertys);
        }

        public override void ToXml(XmlElement aNode)
        {
            base.ToXml(aNode);
            if (FEditProtect)
                aNode.SetAttribute("editprotect", "1");

            if (FDeleteAllow)
                aNode.SetAttribute("deleteallow", "1");

            aNode.SetAttribute("property", HC.View.HC.GetPropertyString(FPropertys));
        }

        public override void ParseXml(XmlElement aNode)
        {
            base.ParseXml(aNode);
            FEditProtect = aNode.GetAttribute("editprotect") == "1";

            if (aNode.HasAttribute("deleteallow"))
                FDeleteAllow = aNode.GetAttribute("deleteallow") == "1";
            else
                FDeleteAllow = true;

            string vProp = HC.View.HC.GetXmlRN(aNode.Attributes["property"].Value);
            HC.View.HC.SetPropertyString(vProp, FPropertys);
        }

        public void ToJson(string aJsonObj)
        {

        }

        public void ParseJson(string aJsonObj)
        {

        }

        public bool EditProtect
        {
            get { return FEditProtect; }
            set { FEditProtect = value; }
        }

        public bool DeleteAllow
        {
            get { return FDeleteAllow; }
            set { FDeleteAllow = value; }
        }

        public Dictionary<string, string> Propertys
        {
            get { return FPropertys; }
        }

        public string this[string aKey]
        {
            get { return GetValue(aKey); }
            set { SetValue(aKey, value); }
        }
    }

    public class DeImageItem : HCImageItem
    {
        private bool FEditProtect, FDeleteAllow;
        private Dictionary<string, string> FPropertys;
        private Dictionary<string, string> FScripts;

        private string GetValue(string key)
        {
            if (FPropertys.Keys.Contains(key))
                return FPropertys[key];
            else
                return "";
        }

        private void SetValue(string key, string value)
        {
            HC.View.HC.HCSetProperty(FPropertys, key, value);
        }

        protected override void DoPaint(HCStyle aStyle, RECT aDrawRect, int aDataDrawTop, int aDataDrawBottom, int aDataScreenTop, int aDataScreenBottom, HCCanvas aCanvas, PaintInfo aPaintInfo)
        {
            base.DoPaint(aStyle, aDrawRect, aDataDrawTop, aDataDrawBottom, aDataScreenTop, aDataScreenBottom, aCanvas, aPaintInfo);
            if ((this.Empty) && this.Active && (!aPaintInfo.Print))  // 非打印状态下的空白图片
            {
                aCanvas.Font.Size = 12;
                aCanvas.Font.FontStyles.InClude((byte)HCFontStyle.tsItalic);
                aCanvas.TextOut(aDrawRect.Left + 2, aDrawRect.Top + 2, "DeIndex:" + this[DeProp.Index]);
            }
        }

        public DeImageItem(HCCustomData aOwnerData) : base(aOwnerData)
        {
            FDeleteAllow = true;
            FPropertys = new Dictionary<string, string>();
            FScripts = new Dictionary<string, string>();
        }

        ~DeImageItem()
        {

        }

        public override void Assign(HCCustomItem source)
        {
            base.Assign(source);
            FEditProtect = (source as DeImageItem).EditProtect;
            FDeleteAllow = (source as DeImageItem).DeleteAllow;
            HC.View.HC.AssignProperty((source as DeImageItem).Propertys, ref FPropertys);
            HC.View.HC.AssignProperty((source as DeImageItem).FScripts, ref FScripts);
        }

        public override void SaveToStreamRange(Stream aStream, int aStart, int aEnd)
        {
            base.SaveToStreamRange(aStream, aStart, aEnd);

            byte vByte = 0;
            if (FEditProtect)
                vByte = (byte)(vByte | (1 << 7));

            if (FDeleteAllow)
                vByte = (byte)(vByte | (1 << 6));

            aStream.WriteByte(vByte);

            HC.View.HC.HCSaveTextToStream(aStream, HC.View.HC.GetPropertyString(FPropertys));
            HC.View.HC.HCSaveTextToStream(aStream, HC.View.HC.GetPropertyString(FScripts, HC.View.HC.RecordSeparator, HC.View.HC.UnitSeparator));
        }

        public override void LoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            base.LoadFromStream(aStream, aStyle, aFileVersion);
            if (aFileVersion < 33)
                return;

            byte vByte = (byte)aStream.ReadByte();
            FEditProtect = HC.View.HC.IsOdd(vByte >> 7);

            if (aFileVersion > 34)
                FDeleteAllow = HC.View.HC.IsOdd(vByte >> 6);
            else
                FDeleteAllow = true;

            string vS = "";
            HC.View.HC.HCLoadTextFromStream(aStream, ref vS, aFileVersion);
            HC.View.HC.SetPropertyString(vS, FPropertys);
            if (aFileVersion > 57)
            {
                HC.View.HC.HCLoadTextFromStream(aStream, ref vS, aFileVersion);
                HC.View.HC.SetPropertyString(vS, FScripts, HC.View.HC.RecordSeparator, HC.View.HC.UnitSeparator);
            }
        }

        public override void ToXml(XmlElement aNode)
        {
            base.ToXml(aNode);
            if (FEditProtect)
                aNode.SetAttribute("editprotect", "1");

            if (FDeleteAllow)
                aNode.SetAttribute("deleteallow", "1");

            aNode.SetAttribute("property", HC.View.HC.GetPropertyString(FPropertys));
        }

        public override void ParseXml(XmlElement aNode)
        {
            base.ParseXml(aNode);
            FEditProtect = aNode.GetAttribute("editprotect") == "1";

            if (aNode.HasAttribute("deleteallow"))
                FDeleteAllow = aNode.GetAttribute("deleteallow") == "1";
            else
                FDeleteAllow = true;

            string vProp = HC.View.HC.GetXmlRN(aNode.Attributes["property"].Value);
            HC.View.HC.SetPropertyString(vProp, FPropertys);
        }

        public void ToJson(string aJsonObj)
        {

        }

        public void ParseJson(string aJsonObj)
        {

        }

        public bool EditProtect
        {
            get { return FEditProtect; }
            set { FEditProtect = value; }
        }

        public bool DeleteAllow
        {
            get { return FDeleteAllow; }
            set { FDeleteAllow = value; }
        }

        public Dictionary<string, string> Propertys
        {
            get { return FPropertys; }
        }

        public string this[string aKey]
        {
            get { return GetValue(aKey); }
            set { SetValue(aKey, value); }
        }
    }
}
