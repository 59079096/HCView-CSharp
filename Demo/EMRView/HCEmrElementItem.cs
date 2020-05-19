/*******************************************************}
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
using System.Xml;

namespace EMRView
{
    public enum StyleExtra : byte  // 痕迹样式
    {
        cseNone, cseDel, cseAdd
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

        /// <summary> 痕迹信息 </summary>
        public const string Trace = "Trace";

        /// <summary> 隐私信息 </summary>
        public const string Secret = "Secret";

        /// <summary> 元素填写后背景色 </summary>
        public static Color DE_CHECKCOLOR = Color.FromArgb(0xF0, 0xF0, 0xF0);

        /// <summary> 元素未填写时背景色 </summary>
        public static Color DE_NOCHECKCOLOR = Color.FromArgb(0xFF, 0xDD, 0x80);

        public static string GetPropertyString(Dictionary<string, string> aProperty)
        {
            string vS = "";
            for (int i = 0; i < aProperty.Count; i++)
            {
                var element = aProperty.ElementAt(i);
                if (element.Key != "")
                    vS = vS + element.Key + "=" + element.Value + HC.View.HC.sLineBreak;
            }

            return vS;
        }

        public static void SetPropertyString(string aStrings, Dictionary<string , string> aPropertys)
        {
            aPropertys.Clear();

            if (aStrings != "")
            {
                string[] vStrings = aStrings.Split(new string[] { HC.View.HC.sLineBreak }, StringSplitOptions.None);
                for (int i = 0; i < vStrings.Length; i++)
                {
                    if (vStrings[i] != "")
                    {
                        string[] vKv = vStrings[i].Split(new string[] { "=" }, StringSplitOptions.None);
                        if (vKv[0] != "")
                            aPropertys.Add(vKv[0], vKv[1]);
                    }
                }
            }
        }
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

    public static class GroupProp : Object
    {
        /// <summary> 数据组类型 </summary>
        public const string SubType = "RT";
    }

    public static class SubType : Object
    {
        /// <summary> 病程 </summary>
        public const string Proc = "P";
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

    /// <summary> 电子病历数据元对象 </summary>
    public sealed class DeItem : EmrTextItem  // 不可继承
    {
        private bool
            FMouseIn,
            FOutOfRang,  // 值不在正常范围内
            FEditProtect,  // 编辑保护，不允许删除、手动录入
            FCopyProtect,  // 复制保护，不允许复制
            FDeleteAllow,  // 是否允许删除
            FAllocValue;  // 是否分配过值

        private StyleExtra FStyleEx;
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
            if (value.IndexOf("=") >= 0)
                throw new Exception("属性值中不允许有=号");

            FPropertys[key] = value;
        }

        private bool GetIsElement()
        {
            return FPropertys.Keys.Contains(DeProp.Index);
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
            FEditProtect = false;
            FDeleteAllow = true;
            FCopyProtect = false;
            FAllocValue = false;
            FOutOfRang = false;
            FMouseIn = false;
        }

        public DeItem(string aText) : base(aText)
        {
            FPropertys = new Dictionary<string, string>();
            FEditProtect = false;
            FDeleteAllow = true;
            FMouseIn = false;
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
            FStyleEx = (source as DeItem).StyleEx;
            FEditProtect = (source as DeItem).EditProtect;
            FDeleteAllow = (source as DeItem).DeleteAllow;
            FCopyProtect = (source as DeItem).CopyProtect;
            FAllocValue = (source as DeItem).AllocValue;
            FOutOfRang = (source as DeItem).OutOfRang;
            string vS = DeProp.GetPropertyString((source as DeItem).Propertys);
            DeProp.SetPropertyString(vS, FPropertys);
        }

        public override bool CanConcatItems(HCCustomItem aItem)
        {
            bool Result = base.CanConcatItems(aItem);
            if (Result)
            {
                DeItem vDeItem = aItem as DeItem;
                Result = ((this[DeProp.Index] == vDeItem[DeProp.Index])
                    && (this.FStyleEx == vDeItem.StyleEx)
                    && (FEditProtect == vDeItem.FEditProtect)
                    && (FDeleteAllow == vDeItem.DeleteAllow)
                    && (FCopyProtect == vDeItem.CopyProtect)
                    && (FAllocValue == vDeItem.AllocValue)
                    && (this[DeProp.Trace] == vDeItem[DeProp.Trace])); ;
            }

            return Result;
        }

        public override string GetHint()
        {
            if (FStyleEx == StyleExtra.cseNone)
                return this[DeProp.Name];
            else
                return this[DeProp.Trace];
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
                        if (FEditProtect)  // 两头允许输入，触发actConcatText时返回供Data层处理新TextItem还是连接
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
                        if (FEditProtect)
                            vResult = aOffset == 0;

                        break;

                    case HCAction.actDeleteText:
                        if (FEditProtect)
                            vResult = aOffset == this.Length;
                        break;
                }
            }

            return vResult;
        }

        public override void SaveToStream(Stream aStream, int aStart, int aEnd)
        {
            base.SaveToStream(aStream, aStart, aEnd);

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

            aStream.WriteByte(vByte);

            vByte = (byte)FStyleEx;
            aStream.WriteByte(vByte);

            HC.View.HC.HCSaveTextToStream(aStream, DeProp.GetPropertyString(FPropertys));
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

            vByte = (byte)aStream.ReadByte();
            FStyleEx = (StyleExtra)vByte;

            string vS = "";
            HC.View.HC.HCLoadTextFromStream(aStream, ref vS, aFileVersion);
            DeProp.SetPropertyString(vS, FPropertys);
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

            aNode.SetAttribute("styleex", ((byte)FStyleEx).ToString());
            aNode.SetAttribute("property", DeProp.GetPropertyString(FPropertys));
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

            byte vByte = 0;
            bool vHasValue = byte.TryParse(aNode.GetAttribute("styleex"), out vByte);
            FStyleEx = (StyleExtra)vByte;
            string vProp = HC.View.HC.GetXmlRN(aNode.Attributes["property"].Value);
            DeProp.SetPropertyString(vProp, FPropertys);
        }

        public void ToJson(string aJsonObj)
        {

        }

        public void ParseJson(string aJsonObj)
        {

        }

        public bool IsElement
        {
            get { return GetIsElement(); }
        }

        public bool MouseIn
        {
            get { return FMouseIn; }
        }

        public StyleExtra StyleEx
        {
            get { return FStyleEx; }
            set { FStyleEx = value; }
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

    public class DeTable : HCTableItem
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

        private void SetValue(string key, string  value)
        {
            FPropertys[key] = value;
        }

        public DeTable(HCCustomData aOwnerData, int aRowCount, int aColCount, int aWidth) 
            : base(aOwnerData, aRowCount, aColCount, aWidth)
        {
            FDeleteAllow = true;
            FPropertys = new Dictionary<string, string>();
        }

        ~DeTable()
        {

        }

        public override void Assign(HCCustomItem source)
        {
            base.Assign(source);
            FEditProtect = (source as DeTable).EditProtect;
            FDeleteAllow = (source as DeTable).DeleteAllow;
            string vS = DeProp.GetPropertyString((source as DeTable).Propertys);
            DeProp.SetPropertyString(vS, FPropertys);
        }

        public override void SaveToStream(Stream aStream, int aStart, int aEnd)
        {
            base.SaveToStream(aStream, aStart, aEnd);

            byte vByte = 0;
            if (FEditProtect)
                vByte = (byte)(vByte | (1 << 7));

            if (FDeleteAllow)
                vByte = (byte)(vByte | (1 << 6));

            aStream.WriteByte(vByte);

            HC.View.HC.HCSaveTextToStream(aStream, DeProp.GetPropertyString(FPropertys));
        }

        public override void LoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            base.LoadFromStream(aStream, aStyle, aFileVersion);

            byte vByte = (byte)aStream.ReadByte();
            FEditProtect = (vByte >> 7) == 1;

            if (aFileVersion > 34)
                FDeleteAllow = (vByte >> 6) == 1;
            else
                FDeleteAllow = true;

            string vS = "";
            HC.View.HC.HCLoadTextFromStream(aStream, ref vS, aFileVersion);
            DeProp.SetPropertyString(vS, FPropertys);
        }

        public override void ToXml(XmlElement aNode)
        {
            base.ToXml(aNode);
            if (FEditProtect)
                aNode.SetAttribute("editprotect", "1");

            if (FDeleteAllow)
                aNode.SetAttribute("deleteallow", "1");

            aNode.SetAttribute("property", DeProp.GetPropertyString(FPropertys));
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
            DeProp.SetPropertyString(vProp, FPropertys);
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

        public DeCheckBox(HCCustomData aOwnerData, string aText, bool aChecked) : base(aOwnerData, aText, aChecked)
        {
            FDeleteAllow = true;
            FPropertys = new Dictionary<string, string>();
        }

        ~DeCheckBox()
        {

        }

        public override void Assign(HCCustomItem source)
        {
            base.Assign(source);
            FEditProtect = (source as DeCheckBox).EditProtect;
            FDeleteAllow = (source as DeCheckBox).DeleteAllow;
            string vS = DeProp.GetPropertyString((source as DeCheckBox).Propertys);
            DeProp.SetPropertyString(vS, FPropertys);
        }

        public override void SaveToStream(Stream aStream, int aStart, int aEnd)
        {
            base.SaveToStream(aStream, aStart, aEnd);

            byte vByte = 0;
            if (FEditProtect)
                vByte = (byte)(vByte | (1 << 7));

            if (FDeleteAllow)
                vByte = (byte)(vByte | (1 << 6));

            aStream.WriteByte(vByte);

            HC.View.HC.HCSaveTextToStream(aStream, DeProp.GetPropertyString(FPropertys));
        }

        public override void LoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            base.LoadFromStream(aStream, aStyle, aFileVersion);

            byte vByte = (byte)aStream.ReadByte();
            FEditProtect = (vByte >> 7) == 1;

            if (aFileVersion > 34)
                FDeleteAllow = (vByte >> 6) == 1;
            else
                FDeleteAllow = true;

            string vS = "";
            HC.View.HC.HCLoadTextFromStream(aStream, ref vS, aFileVersion);
            DeProp.SetPropertyString(vS, FPropertys);
        }

        public override void ToXml(XmlElement aNode)
        {
            base.ToXml(aNode);
            if (FEditProtect)
                aNode.SetAttribute("editprotect", "1");

            if (FDeleteAllow)
                aNode.SetAttribute("deleteallow", "1");

            aNode.SetAttribute("property", DeProp.GetPropertyString(FPropertys));
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
            DeProp.SetPropertyString(vProp, FPropertys);
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

        public DeEdit(HCCustomData aOwnerData, string aText) : base(aOwnerData, aText)
        {
            FDeleteAllow = true;
            FPropertys = new Dictionary<string, string>();
        }

        ~DeEdit()
        {

        }

        public override void Assign(HCCustomItem source)
        {
            base.Assign(source);
            FEditProtect = (source as DeEdit).EditProtect;
            FDeleteAllow = (source as DeEdit).DeleteAllow;
            string vS = DeProp.GetPropertyString((source as DeEdit).Propertys);
            DeProp.SetPropertyString(vS, FPropertys);
        }

        public override void SaveToStream(Stream aStream, int aStart, int aEnd)
        {
            base.SaveToStream(aStream, aStart, aEnd);

            byte vByte = 0;
            if (FEditProtect)
                vByte = (byte)(vByte | (1 << 7));

            if (FDeleteAllow)
                vByte = (byte)(vByte | (1 << 6));

            aStream.WriteByte(vByte);

            HC.View.HC.HCSaveTextToStream(aStream, DeProp.GetPropertyString(FPropertys));
        }

        public override void LoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            base.LoadFromStream(aStream, aStyle, aFileVersion);

            byte vByte = (byte)aStream.ReadByte();
            FEditProtect = (vByte >> 7) == 1;

            if (aFileVersion > 34)
                FDeleteAllow = (vByte >> 6) == 1;
            else
                FDeleteAllow = true;

            string vS = "";
            HC.View.HC.HCLoadTextFromStream(aStream, ref vS, aFileVersion);
            DeProp.SetPropertyString(vS, FPropertys);
        }

        public override void ToXml(XmlElement aNode)
        {
            base.ToXml(aNode);
            if (FEditProtect)
                aNode.SetAttribute("editprotect", "1");

            if (FDeleteAllow)
                aNode.SetAttribute("deleteallow", "1");

            aNode.SetAttribute("property", DeProp.GetPropertyString(FPropertys));
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
            DeProp.SetPropertyString(vProp, FPropertys);
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

        public DeCombobox(HCCustomData aOwnerData, string aText) : base(aOwnerData, aText)
        {
            FDeleteAllow = true;
            FPropertys = new Dictionary<string, string>();
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
            string vS = DeProp.GetPropertyString((source as DeCombobox).Propertys);
            DeProp.SetPropertyString(vS, FPropertys);
        }

        public override void SaveToStream(Stream aStream, int aStart, int aEnd)
        {
            base.SaveToStream(aStream, aStart, aEnd);

            byte vByte = 0;
            if (FEditProtect)
                vByte = (byte)(vByte | (1 << 7));

            if (FDeleteAllow)
                vByte = (byte)(vByte | (1 << 6));

            aStream.WriteByte(vByte);

            HC.View.HC.HCSaveTextToStream(aStream, DeProp.GetPropertyString(FPropertys));
        }

        public override void LoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            base.LoadFromStream(aStream, aStyle, aFileVersion);

            byte vByte = (byte)aStream.ReadByte();
            FEditProtect = (vByte >> 7) == 1;

            if (aFileVersion > 34)
                FDeleteAllow = (vByte >> 6) == 1;
            else
                FDeleteAllow = true;

            string vS = "";
            HC.View.HC.HCLoadTextFromStream(aStream, ref vS, aFileVersion);
            DeProp.SetPropertyString(vS, FPropertys);
        }

        public override void ToXml(XmlElement aNode)
        {
            base.ToXml(aNode);
            if (FEditProtect)
                aNode.SetAttribute("editprotect", "1");

            if (FDeleteAllow)
                aNode.SetAttribute("deleteallow", "1");

            aNode.SetAttribute("property", DeProp.GetPropertyString(FPropertys));
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
            DeProp.SetPropertyString(vProp, FPropertys);
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

        public DeDateTimePicker(HCCustomData aOwnerData, DateTime aDateTime) : base(aOwnerData, aDateTime)
        {
            FDeleteAllow = true;
            FPropertys = new Dictionary<string, string>();
        }

        ~DeDateTimePicker()
        {

        }

        public override void Assign(HCCustomItem source)
        {
            base.Assign(source);
            FEditProtect = (source as DeDateTimePicker).EditProtect;
            FDeleteAllow = (source as DeDateTimePicker).DeleteAllow;
            string vS = DeProp.GetPropertyString((source as DeDateTimePicker).Propertys);
            DeProp.SetPropertyString(vS, FPropertys);
        }

        public override void SaveToStream(Stream aStream, int aStart, int aEnd)
        {
            base.SaveToStream(aStream, aStart, aEnd);

            byte vByte = 0;
            if (FEditProtect)
                vByte = (byte)(vByte | (1 << 7));

            if (FDeleteAllow)
                vByte = (byte)(vByte | (1 << 6));

            aStream.WriteByte(vByte);

            HC.View.HC.HCSaveTextToStream(aStream, DeProp.GetPropertyString(FPropertys));
        }

        public override void LoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            base.LoadFromStream(aStream, aStyle, aFileVersion);

            byte vByte = (byte)aStream.ReadByte();
            FEditProtect = (vByte >> 7) == 1;

            if (aFileVersion > 34)
                FDeleteAllow = (vByte >> 6) == 1;
            else
                FDeleteAllow = true;

            string vS = "";
            HC.View.HC.HCLoadTextFromStream(aStream, ref vS, aFileVersion);
            DeProp.SetPropertyString(vS, FPropertys);
        }

        public override void ToXml(XmlElement aNode)
        {
            base.ToXml(aNode);
            if (FEditProtect)
                aNode.SetAttribute("editprotect", "1");

            if (FDeleteAllow)
                aNode.SetAttribute("deleteallow", "1");

            aNode.SetAttribute("property", DeProp.GetPropertyString(FPropertys));
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
            DeProp.SetPropertyString(vProp, FPropertys);
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

        public DeRadioGroup(HCCustomData aOwnerData) : base(aOwnerData)
        {
            FDeleteAllow = true;
            FPropertys = new Dictionary<string, string>();
        }

        ~DeRadioGroup()
        {

        }

        public override void Assign(HCCustomItem source)
        {
            base.Assign(source);
            FEditProtect = (source as DeRadioGroup).EditProtect;
            FDeleteAllow = (source as DeRadioGroup).DeleteAllow;
            string vS = DeProp.GetPropertyString((source as DeRadioGroup).Propertys);
            DeProp.SetPropertyString(vS, FPropertys);
        }

        public override void SaveToStream(Stream aStream, int aStart, int aEnd)
        {
            base.SaveToStream(aStream, aStart, aEnd);

            byte vByte = 0;
            if (FEditProtect)
                vByte = (byte)(vByte | (1 << 7));

            if (FDeleteAllow)
                vByte = (byte)(vByte | (1 << 6));

            aStream.WriteByte(vByte);

            HC.View.HC.HCSaveTextToStream(aStream, DeProp.GetPropertyString(FPropertys));
        }

        public override void LoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            base.LoadFromStream(aStream, aStyle, aFileVersion);

            byte vByte = (byte)aStream.ReadByte();
            FEditProtect = (vByte >> 7) == 1;

            if (aFileVersion > 34)
                FDeleteAllow = (vByte >> 6) == 1;
            else
                FDeleteAllow = true;

            string vS = "";
            HC.View.HC.HCLoadTextFromStream(aStream, ref vS, aFileVersion);
            DeProp.SetPropertyString(vS, FPropertys);
        }

        public override void ToXml(XmlElement aNode)
        {
            base.ToXml(aNode);
            if (FEditProtect)
                aNode.SetAttribute("editprotect", "1");

            if (FDeleteAllow)
                aNode.SetAttribute("deleteallow", "1");

            aNode.SetAttribute("property", DeProp.GetPropertyString(FPropertys));
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
            DeProp.SetPropertyString(vProp, FPropertys);
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
            FPropertys[key] = value;
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
            string vS = DeProp.GetPropertyString((source as DeFloatBarCodeItem).Propertys);
            DeProp.SetPropertyString(vS, FPropertys);
        }

        public override void SaveToStream(Stream aStream, int aStart, int aEnd)
        {
            base.SaveToStream(aStream, aStart, aEnd);

            byte vByte = 0;
            if (FEditProtect)
                vByte = (byte)(vByte | (1 << 7));

            if (FDeleteAllow)
                vByte = (byte)(vByte | (1 << 6));

            aStream.WriteByte(vByte);

            HC.View.HC.HCSaveTextToStream(aStream, DeProp.GetPropertyString(FPropertys));
        }

        public override void LoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            base.LoadFromStream(aStream, aStyle, aFileVersion);

            byte vByte = (byte)aStream.ReadByte();
            FEditProtect = (vByte >> 7) == 1;

            if (aFileVersion > 34)
                FDeleteAllow = (vByte >> 6) == 1;
            else
                FDeleteAllow = true;

            string vS = "";
            HC.View.HC.HCLoadTextFromStream(aStream, ref vS, aFileVersion);
            DeProp.SetPropertyString(vS, FPropertys);
        }

        public override void ToXml(XmlElement aNode)
        {
            base.ToXml(aNode);
            if (FEditProtect)
                aNode.SetAttribute("editprotect", "1");

            if (FDeleteAllow)
                aNode.SetAttribute("deleteallow", "1");

            aNode.SetAttribute("property", DeProp.GetPropertyString(FPropertys));
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
            DeProp.SetPropertyString(vProp, FPropertys);
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
        }

        ~DeImageItem()
        {

        }

        public override void Assign(HCCustomItem source)
        {
            base.Assign(source);
            FEditProtect = (source as DeImageItem).EditProtect;
            FDeleteAllow = (source as DeImageItem).DeleteAllow;
            string vS = DeProp.GetPropertyString((source as DeImageItem).Propertys);
            DeProp.SetPropertyString(vS, FPropertys);
        }

        public override void SaveToStream(Stream aStream, int aStart, int aEnd)
        {
            base.SaveToStream(aStream, aStart, aEnd);

            byte vByte = 0;
            if (FEditProtect)
                vByte = (byte)(vByte | (1 << 7));

            if (FDeleteAllow)
                vByte = (byte)(vByte | (1 << 6));

            aStream.WriteByte(vByte);

            HC.View.HC.HCSaveTextToStream(aStream, DeProp.GetPropertyString(FPropertys));
        }

        public override void LoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            base.LoadFromStream(aStream, aStyle, aFileVersion);
            if (aFileVersion < 33)
                return;

            byte vByte = (byte)aStream.ReadByte();
            FEditProtect = (vByte >> 7) == 1;

            if (aFileVersion > 34)
                FDeleteAllow = (vByte >> 6) == 1;
            else
                FDeleteAllow = true;

            string vS = "";
            HC.View.HC.HCLoadTextFromStream(aStream, ref vS, aFileVersion);
            DeProp.SetPropertyString(vS, FPropertys);
        }

        public override void ToXml(XmlElement aNode)
        {
            base.ToXml(aNode);
            if (FEditProtect)
                aNode.SetAttribute("editprotect", "1");

            if (FDeleteAllow)
                aNode.SetAttribute("deleteallow", "1");

            aNode.SetAttribute("property", DeProp.GetPropertyString(FPropertys));
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
            DeProp.SetPropertyString(vProp, FPropertys);
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
