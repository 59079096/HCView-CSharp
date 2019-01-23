using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HC.View;
using System.IO;
using System.Drawing;
using HC.Win32;
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

            string[] vStrings = aStrings.Split(new string[] { HC.View.HC.sLineBreak }, StringSplitOptions.None);
            for (int i = 0; i < vStrings.Length; i++)
            {
                string[] vKv = vStrings[i].Split(new string[] { "=" }, StringSplitOptions.None);
                aPropertys.Add(vKv[0], vKv[1]);
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
    public class EmrTextItem : HCTextItem { }

    /// <summary> 电子病历数据元对象 </summary>
    public sealed class DeItem : EmrTextItem  // 不可继承
    {
        private bool FMouseIn, FDeleteProtect;
        private StyleExtra FStyleEx;
        private Dictionary<string, string> FPropertys;

        protected override void SetText(string value)
        {
            if (value != "")
                base.SetText(value);
            else
            {
                if (IsElement && FDeleteProtect)
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

            if ((!aPaintInfo.Print) && (IsElement))
            {
                if (FMouseIn || Active)
                {
                    if (IsSelectPart || IsSelectComplate)
                    {

                    }
                    else
                    {
                        if (this[DeProp.Name] != this.Text)
                            aCanvas.Brush.Color = DeProp.DE_CHECKCOLOR;
                        else
                            aCanvas.Brush.Color = DeProp.DE_NOCHECKCOLOR;

                        aCanvas.FillRect(aDrawRect);
                    }
                }
            }

            if (FStyleEx == StyleExtra.cseDel)
            {
                int vTextHeight = aCanvas.TextHeight("H");
                int vAlignVert = User.DT_BOTTOM;
                switch (aStyle.ParaStyles[this.ParaNo].AlignVert)
                {
                    case ParaAlignVert.pavCenter:
                        vAlignVert = User.DT_CENTER;
                        break;

                    case ParaAlignVert.pavTop:
                        vAlignVert = User.DT_TOP;
                        break;

                    default:
                        vAlignVert = User.DT_BOTTOM;
                        break;
                }

                int vTop = aDrawRect.Top;
                switch (vAlignVert)
                {
                    case User.DT_TOP:
                        vTop = aDrawRect.Top;
                        break;
                    
                    case User.DT_CENTER:
                        vTop = aDrawRect.Top + (aDrawRect.Bottom - aDrawRect.Top - vTextHeight) / 2;
                        break;

                    default:
                        vTop = aDrawRect.Bottom - vTextHeight;
                        break;
                }

                // 绘制删除线
                aCanvas.Pen.BeginUpdate();
                try
                {
                    aCanvas.Pen.Style = HCPenStyle.psSolid;
                    aCanvas.Pen.Color = Color.Red;
                }
                finally
                {
                    aCanvas.Pen.EndUpdate();
                }

                vTop = vTop + (aDrawRect.Bottom - vTop) / 2;
                aCanvas.MoveTo(aDrawRect.Left, vTop - 1);
                aCanvas.LineTo(aDrawRect.Right, vTop - 1);
                aCanvas.MoveTo(aDrawRect.Left, vTop + 2);
                aCanvas.LineTo(aDrawRect.Right, vTop + 2);
            }
            else
                if (FStyleEx == StyleExtra.cseAdd)
                {
                    aCanvas.Pen.BeginUpdate();
                    try
                    {
                        aCanvas.Pen.Style = HCPenStyle.psSolid;
                        aCanvas.Pen.Color = Color.Blue;
                    }
                    finally
                    {
                        aCanvas.Pen.EndUpdate();
                    }

                    aCanvas.MoveTo(aDrawRect.Left, aDrawRect.Bottom);
                    aCanvas.LineTo(aDrawRect.Right, aDrawRect.Bottom);
                }
        }

        //
        protected string GetValue(string key)
        {
            if (FPropertys.Keys.Contains(key))
                return FPropertys[key];
            else
                return "";
        }

        protected void SetValue(string key, string value)
        {
            FPropertys[key] = value;
        }

        protected bool GetIsElement()
        {
            return FPropertys.Keys.Contains(DeProp.Index);
        }

        public DeItem() : base()
        {
            FPropertys = new Dictionary<string,string>();
            FDeleteProtect = false;
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

            if ((Result) && (this.IsElement))
            {
                if (aAction == HCItemAction.hiaInsertChar)
                    Result = false;
                else
                    Result = !FDeleteProtect;
            }

            if (!Result)
                User.MessageBeep(0);

            return Result;
        }

        public override void SaveToStream(Stream aStream, int aStart, int aEnd)
        {
            base.SaveToStream(aStream, aStart, aEnd);
            byte vByte = (byte)FStyleEx;
            aStream.WriteByte(vByte);
            HC.View.HC.HCSaveTextToStream(aStream, DeProp.GetPropertyString(FPropertys));
        }

        public override void LoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            base.LoadFromStream(aStream, aStyle, aFileVersion);
            byte vByte = 0;
            vByte = (byte)aStream.ReadByte();
            FStyleEx = (StyleExtra)vByte;

            string vS = "";
            HC.View.HC.HCLoadTextFromStream(aStream, ref vS);
            DeProp.SetPropertyString(vS, FPropertys);
        }

        public override void ToXml(XmlElement aNode)
        {
            base.ToXml(aNode);
            aNode.Attributes["property"].Value = DeProp.GetPropertyString(FPropertys);
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

        public bool IsElement
        {
            get { return GetIsElement(); }
        }

        public StyleExtra StyleEx
        {
            get { return FStyleEx; }
            set { FStyleEx = value; }
        }

        public bool DeleteProtect
        {
            get { return FDeleteProtect; }
            set { FDeleteProtect = value; }
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
        private bool FDeleteProtect;
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
            HC.View.HC.HCSaveTextToStream(aStream, DeProp.GetPropertyString(FPropertys));
        }

        public override void LoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            base.LoadFromStream(aStream, aStyle, aFileVersion);
            string vS = "";
            HC.View.HC.HCLoadTextFromStream(aStream, ref vS);
            DeProp.SetPropertyString(vS, FPropertys);
        }

        public override void ToXml(XmlElement aNode)
        {
            base.ToXml(aNode);
            aNode.Attributes["property"].Value = DeProp.GetPropertyString(FPropertys);
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

        public bool DeleteProtect
        {
            get { return FDeleteProtect; }
            set { FDeleteProtect = value; }
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

        private bool FDeleteProtect;

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
            HC.View.HC.HCSaveTextToStream(aStream, DeProp.GetPropertyString(FPropertys));
        }

        public override void LoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            base.LoadFromStream(aStream, aStyle, aFileVersion);
            string vS = "";
            HC.View.HC.HCLoadTextFromStream(aStream, ref vS);
            DeProp.SetPropertyString(vS, FPropertys);
        }

        public override void ToXml(XmlElement aNode)
        {
            base.ToXml(aNode);
            aNode.Attributes["property"].Value = DeProp.GetPropertyString(FPropertys);
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

        public bool DeleteProtect
        {
            get { return FDeleteProtect; }
            set { FDeleteProtect = value; }
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
        private bool FDeleteProtect;
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
            HC.View.HC.HCSaveTextToStream(aStream, DeProp.GetPropertyString(FPropertys));
        }

        public override void LoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            base.LoadFromStream(aStream, aStyle, aFileVersion);
            string vS = "";
            HC.View.HC.HCLoadTextFromStream(aStream, ref vS);
            DeProp.SetPropertyString(vS, FPropertys);
        }

        public override void ToXml(XmlElement aNode)
        {
            base.ToXml(aNode);
            aNode.Attributes["property"].Value = DeProp.GetPropertyString(FPropertys);
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

        public bool DeleteProtect
        {
            get { return FDeleteProtect; }
            set { FDeleteProtect = value; }
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
        private bool FDeleteProtect;
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
            HC.View.HC.HCSaveTextToStream(aStream, DeProp.GetPropertyString(FPropertys));
        }

        public override void LoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            base.LoadFromStream(aStream, aStyle, aFileVersion);
            string vS = "";
            HC.View.HC.HCLoadTextFromStream(aStream, ref vS);
            DeProp.SetPropertyString(vS, FPropertys);
        }

        public override void ToXml(XmlElement aNode)
        {
            base.ToXml(aNode);
            aNode.Attributes["property"].Value = DeProp.GetPropertyString(FPropertys);
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

        public bool DeleteProtect
        {
            get { return FDeleteProtect; }
            set { FDeleteProtect = value; }
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
        private bool FDeleteProtect;
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
            HC.View.HC.HCSaveTextToStream(aStream, DeProp.GetPropertyString(FPropertys));
        }

        public override void LoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            base.LoadFromStream(aStream, aStyle, aFileVersion);
            string vS = "";
            HC.View.HC.HCLoadTextFromStream(aStream, ref vS);
            DeProp.SetPropertyString(vS, FPropertys);
        }

        public override void ToXml(XmlElement aNode)
        {
            base.ToXml(aNode);
            aNode.Attributes["property"].Value = DeProp.GetPropertyString(FPropertys);
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

        public bool DeleteProtect
        {
            get { return FDeleteProtect; }
            set { FDeleteProtect = value; }
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
        private bool FDeleteProtect;
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
            HC.View.HC.HCSaveTextToStream(aStream, DeProp.GetPropertyString(FPropertys));
        }

        public override void LoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            base.LoadFromStream(aStream, aStyle, aFileVersion);
            string vS = "";
            HC.View.HC.HCLoadTextFromStream(aStream, ref vS);
            DeProp.SetPropertyString(vS, FPropertys);
        }

        public override void ToXml(XmlElement aNode)
        {
            base.ToXml(aNode);
            aNode.Attributes["property"].Value = DeProp.GetPropertyString(FPropertys);
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

        public bool DeleteProtect
        {
            get { return FDeleteProtect; }
            set { FDeleteProtect = value; }
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
