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

    /// <summary> 电子病历文本对象 </summary>
    public class EmrTextItem : HCTextItem 
    {
        public EmrTextItem() : base()
        {

        }

        public EmrTextItem(string aText) : base(aText)
        {

        }
    }

    public delegate void DePaintBKGHandler(object sender, HCCanvas aCanvas, RECT aDrawRect, PaintInfo aPaintInfo);

    /// <summary> 电子病历数据元对象 </summary>
    public sealed class DeItem : EmrTextItem  // 不可继承
    {
        private bool
            FMouseIn,
            FOutOfRang,  // 值不在正常范围内
            FEditProtect,  // 编辑保护，不允许删除、手动录入
            FCopyProtect,  // 复制保护，不允许复制
            FAllocValue;  // 是否分配过值

        private StyleExtra FStyleEx;
        private Dictionary<string, string> FPropertys;
        private DePaintBKGHandler FOnPaintBKG;

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

        protected override void DoPaint(HCStyle aStyle, RECT aDrawRect, int aDataDrawTop, int aDataDrawBottom, 
            int aDataScreenTop, int aDataScreenBottom, HCCanvas aCanvas, PaintInfo aPaintInfo)
        {
            base.DoPaint(aStyle, aDrawRect, aDataDrawTop, aDataDrawBottom, aDataScreenTop,
                aDataScreenBottom, aCanvas, aPaintInfo);

            if (FOnPaintBKG != null)
                FOnPaintBKG(this, aCanvas, aDrawRect, aPaintInfo);
        }

        public DeItem() : base()
        {
            FPropertys = new Dictionary<string,string>();
            FEditProtect = false;
            FCopyProtect = false;
            FAllocValue = false;
            FOutOfRang = false;
            FMouseIn = false;
        }

        public DeItem(string aText) : base(aText)
        {
            FPropertys = new Dictionary<string, string>();
            FEditProtect = false;
            FMouseIn = false;
        }

        ~DeItem()
        {

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
            FCopyProtect = (source as DeItem).CopyProtect;
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
                    && (FCopyProtect == vDeItem.CopyProtect)
                    && (FAllocValue == vDeItem.AllocValue)
                    && (this[DeProp.Trace] == vDeItem[DeProp.Trace]));
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

        public override bool CanAccept(int aOffset, HCItemAction aAction)
        {
            bool Result = base.CanAccept(aOffset, aAction);

            if (Result)
            {
                if (this.IsElement)
                {
                    if (aAction == HCItemAction.hiaInsertChar)
                        Result = false;
                    else
                    if ((aAction == HCItemAction.hiaBackDeleteChar) || (aAction == HCItemAction.hiaDeleteChar) || (aAction == HCItemAction.hiaRemove))
                        Result = !FEditProtect;
                }
                else
                    Result = !FEditProtect;
            }

            if (!Result)
                User.MessageBeep(0);

            return Result;
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

        public DePaintBKGHandler OnPaintBKG
        {
            get { return FOnPaintBKG; }
            set { FOnPaintBKG = value; }
        }
    }

    public class DeTable : HCTableItem
    {
        private bool FEditProtect;
        private Dictionary<string, string> FPropertys;

        private string GetValue(string key)
        {
            return FPropertys[key];
        }

        private void SetValue(string key, string  value)
        {
            FPropertys[key] = value;
        }

        public DeTable(HCCustomData aOwnerData, int aRowCount, int aColCount, int aWidth) 
            : base(aOwnerData, aRowCount, aColCount, aWidth)
        {
            FPropertys = new Dictionary<string, string>();
        }

        ~DeTable()
        {

        }

        public override void Assign(HCCustomItem source)
        {
            base.Assign(source);
            string vS = DeProp.GetPropertyString((source as DeEdit).Propertys);
            DeProp.SetPropertyString(vS, FPropertys);
        }

        public override void SaveToStream(Stream aStream, int aStart, int aEnd)
        {
            base.SaveToStream(aStream, aStart, aEnd);

            byte vByte = 0;
            if (FEditProtect)
                vByte = (byte)(vByte | (1 << 7));
            aStream.WriteByte(vByte);

            HC.View.HC.HCSaveTextToStream(aStream, DeProp.GetPropertyString(FPropertys));
        }

        public override void LoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            base.LoadFromStream(aStream, aStyle, aFileVersion);

            byte vByte = (byte)aStream.ReadByte();
            FEditProtect = (vByte >> 7) == 1;

            string vS = "";
            HC.View.HC.HCLoadTextFromStream(aStream, ref vS, aFileVersion);
            DeProp.SetPropertyString(vS, FPropertys);
        }

        public override void ToXml(XmlElement aNode)
        {
            base.ToXml(aNode);
            aNode.SetAttribute("property", DeProp.GetPropertyString(FPropertys));
        }

        public override void ParseXml(XmlElement aNode)
        {
            base.ParseXml(aNode);
            DeProp.SetPropertyString(aNode.Attributes["property"].Value, FPropertys);
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

        private bool FEditProtect;

        private Dictionary<string, string> FPropertys;

        private string GetValue(string key)
        {
            return FPropertys[key];
        }

        private void SetValue(string key, string value)
        {
            FPropertys[key] = value;
        }

        public DeCheckBox(HCCustomData aOwnerData, string aText, bool aChecked) : base(aOwnerData, aText, aChecked)
        {
            FPropertys = new Dictionary<string, string>();
        }

        ~DeCheckBox()
        {

        }

        public override void Assign(HCCustomItem source)
        {
            base.Assign(source);
            string vS = DeProp.GetPropertyString((source as DeEdit).Propertys);
            DeProp.SetPropertyString(vS, FPropertys);
        }

        public override void SaveToStream(Stream aStream, int aStart, int aEnd)
        {
            base.SaveToStream(aStream, aStart, aEnd);

            byte vByte = 0;
            if (FEditProtect)
                vByte = (byte)(vByte | (1 << 7));
            aStream.WriteByte(vByte);

            HC.View.HC.HCSaveTextToStream(aStream, DeProp.GetPropertyString(FPropertys));
        }

        public override void LoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            base.LoadFromStream(aStream, aStyle, aFileVersion);

            byte vByte = (byte)aStream.ReadByte();
            FEditProtect = (vByte >> 7) == 1;

            string vS = "";
            HC.View.HC.HCLoadTextFromStream(aStream, ref vS, aFileVersion);
            DeProp.SetPropertyString(vS, FPropertys);
        }

        public override void ToXml(XmlElement aNode)
        {
            base.ToXml(aNode);
            aNode.SetAttribute("property", DeProp.GetPropertyString(FPropertys));
        }

        public override void ParseXml(XmlElement aNode)
        {
            base.ParseXml(aNode);
            DeProp.SetPropertyString(aNode.Attributes["property"].Value, FPropertys);
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
        private bool FEditProtect;
        private Dictionary<string, string> FPropertys;

        private string GetValue(string key)
        {
            return FPropertys[key];
        }

        private void SetValue(string key, string value)
        {
            FPropertys[key] = value;
        }

        public DeEdit(HCCustomData aOwnerData, string aText) : base(aOwnerData, aText)
        {
            FPropertys = new Dictionary<string, string>();
        }

        ~DeEdit()
        {

        }

        public override void Assign(HCCustomItem source)
        {
            base.Assign(source);
            string vS = DeProp.GetPropertyString((source as DeEdit).Propertys);
            DeProp.SetPropertyString(vS, FPropertys);
        }

        public override void SaveToStream(Stream aStream, int aStart, int aEnd)
        {
            base.SaveToStream(aStream, aStart, aEnd);

            byte vByte = 0;
            if (FEditProtect)
                vByte = (byte)(vByte | (1 << 7));
            aStream.WriteByte(vByte);

            HC.View.HC.HCSaveTextToStream(aStream, DeProp.GetPropertyString(FPropertys));
        }

        public override void LoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            base.LoadFromStream(aStream, aStyle, aFileVersion);

            byte vByte = (byte)aStream.ReadByte();
            FEditProtect = (vByte >> 7) == 1;

            string vS = "";
            HC.View.HC.HCLoadTextFromStream(aStream, ref vS, aFileVersion);
            DeProp.SetPropertyString(vS, FPropertys);
        }

        public override void ToXml(XmlElement aNode)
        {
            base.ToXml(aNode);
            aNode.SetAttribute("property", DeProp.GetPropertyString(FPropertys));
        }

        public override void ParseXml(XmlElement aNode)
        {
            base.ParseXml(aNode);
            DeProp.SetPropertyString(aNode.Attributes["property"].Value, FPropertys);
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
        private bool FEditProtect;
        private Dictionary<string, string> FPropertys;

        private string GetValue(string key)
        {
            return FPropertys[key];
        }

        private void SetValue(string key, string value)
        {
            FPropertys[key] = value;
        }

        public DeCombobox(HCCustomData aOwnerData, string aText) : base(aOwnerData, aText)
        {
            FPropertys = new Dictionary<string, string>();
            SaveItem = false;
        }

        ~DeCombobox()
        {

        }

        public override void Assign(HCCustomItem source)
        {
            base.Assign(source);
            string vS = DeProp.GetPropertyString((source as DeEdit).Propertys);
            DeProp.SetPropertyString(vS, FPropertys);
        }

        public override void SaveToStream(Stream aStream, int aStart, int aEnd)
        {
            base.SaveToStream(aStream, aStart, aEnd);

            byte vByte = 0;
            if (FEditProtect)
                vByte = (byte)(vByte | (1 << 7));
            aStream.WriteByte(vByte);

            HC.View.HC.HCSaveTextToStream(aStream, DeProp.GetPropertyString(FPropertys));
        }

        public override void LoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            base.LoadFromStream(aStream, aStyle, aFileVersion);

            byte vByte = (byte)aStream.ReadByte();
            FEditProtect = (vByte >> 7) == 1;

            string vS = "";
            HC.View.HC.HCLoadTextFromStream(aStream, ref vS, aFileVersion);
            DeProp.SetPropertyString(vS, FPropertys);
        }

        public override void ToXml(XmlElement aNode)
        {
            base.ToXml(aNode);
            aNode.SetAttribute("property", DeProp.GetPropertyString(FPropertys));
        }

        public override void ParseXml(XmlElement aNode)
        {
            base.ParseXml(aNode);
            DeProp.SetPropertyString(aNode.Attributes["property"].Value, FPropertys);
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
        private bool FEditProtect;
        private Dictionary<string, string> FPropertys;

        private string GetValue(string key)
        {
            return FPropertys[key];
        }

        private void SetValue(string key, string value)
        {
            FPropertys[key] = value;
        }

        public DeDateTimePicker(HCCustomData aOwnerData, DateTime aDateTime) : base(aOwnerData, aDateTime)
        {
            FPropertys = new Dictionary<string, string>();
        }

        ~DeDateTimePicker()
        {

        }

        public override void Assign(HCCustomItem source)
        {
            base.Assign(source);
            string vS = DeProp.GetPropertyString((source as DeEdit).Propertys);
            DeProp.SetPropertyString(vS, FPropertys);
        }

        public override void SaveToStream(Stream aStream, int aStart, int aEnd)
        {
            base.SaveToStream(aStream, aStart, aEnd);

            byte vByte = 0;
            if (FEditProtect)
                vByte = (byte)(vByte | (1 << 7));
            aStream.WriteByte(vByte);

            HC.View.HC.HCSaveTextToStream(aStream, DeProp.GetPropertyString(FPropertys));
        }

        public override void LoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            base.LoadFromStream(aStream, aStyle, aFileVersion);

            byte vByte = (byte)aStream.ReadByte();
            FEditProtect = (vByte >> 7) == 1;

            string vS = "";
            HC.View.HC.HCLoadTextFromStream(aStream, ref vS, aFileVersion);
            DeProp.SetPropertyString(vS, FPropertys);
        }

        public override void ToXml(XmlElement aNode)
        {
            base.ToXml(aNode);
            aNode.SetAttribute("property", DeProp.GetPropertyString(FPropertys));
        }

        public override void ParseXml(XmlElement aNode)
        {
            base.ParseXml(aNode);
            DeProp.SetPropertyString(aNode.Attributes["property"].Value, FPropertys);
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
        private bool FEditProtect;
        private Dictionary<string, string> FPropertys;

        private string GetValue(string key)
        {
            return FPropertys[key];
        }

        private void SetValue(string key, string value)
        {
            FPropertys[key] = value;
        }

        public DeRadioGroup(HCCustomData aOwnerData) : base(aOwnerData)
        {
            FPropertys = new Dictionary<string, string>();
        }

        ~DeRadioGroup()
        {

        }

        public override void Assign(HCCustomItem source)
        {
            base.Assign(source);
            string vS = DeProp.GetPropertyString((source as DeEdit).Propertys);
            DeProp.SetPropertyString(vS, FPropertys);
        }

        public override void SaveToStream(Stream aStream, int aStart, int aEnd)
        {
            base.SaveToStream(aStream, aStart, aEnd);

            byte vByte = 0;
            if (FEditProtect)
                vByte = (byte)(vByte | (1 << 7));
            aStream.WriteByte(vByte);

            HC.View.HC.HCSaveTextToStream(aStream, DeProp.GetPropertyString(FPropertys));
        }

        public override void LoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            base.LoadFromStream(aStream, aStyle, aFileVersion);

            byte vByte = (byte)aStream.ReadByte();
            FEditProtect = (vByte >> 7) == 1;

            string vS = "";
            HC.View.HC.HCLoadTextFromStream(aStream, ref vS, aFileVersion);
            DeProp.SetPropertyString(vS, FPropertys);
        }

        public override void ToXml(XmlElement aNode)
        {
            base.ToXml(aNode);
            aNode.SetAttribute("property", DeProp.GetPropertyString(FPropertys));
        }

        public override void ParseXml(XmlElement aNode)
        {
            base.ParseXml(aNode);
            DeProp.SetPropertyString(aNode.Attributes["property"].Value, FPropertys);
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
        private bool FEditProtect;
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
            FPropertys = new Dictionary<string, string>();
        }

        ~DeFloatBarCodeItem()
        {

        }

        public override void Assign(HCCustomItem source)
        {
            base.Assign(source);
            string vS = DeProp.GetPropertyString((source as DeEdit).Propertys);
            DeProp.SetPropertyString(vS, FPropertys);
        }

        public override void SaveToStream(Stream aStream, int aStart, int aEnd)
        {
            base.SaveToStream(aStream, aStart, aEnd);

            byte vByte = 0;
            if (FEditProtect)
                vByte = (byte)(vByte | (1 << 7));
            aStream.WriteByte(vByte);

            HC.View.HC.HCSaveTextToStream(aStream, DeProp.GetPropertyString(FPropertys));
        }

        public override void LoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            base.LoadFromStream(aStream, aStyle, aFileVersion);

            byte vByte = (byte)aStream.ReadByte();
            FEditProtect = (vByte >> 7) == 1;

            string vS = "";
            HC.View.HC.HCLoadTextFromStream(aStream, ref vS, aFileVersion);
            DeProp.SetPropertyString(vS, FPropertys);
        }

        public override void ToXml(XmlElement aNode)
        {
            base.ToXml(aNode);
            aNode.SetAttribute("property", DeProp.GetPropertyString(FPropertys));
        }

        public override void ParseXml(XmlElement aNode)
        {
            base.ParseXml(aNode);
            DeProp.SetPropertyString(aNode.Attributes["property"].Value, FPropertys);
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
