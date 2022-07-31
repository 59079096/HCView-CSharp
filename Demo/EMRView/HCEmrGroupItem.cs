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
using HC.Win32;
using HC.View;
using System.IO;
using System.Xml;
using System.Drawing;

namespace EMRView
{
    public static class GroupProp : Object
    {
        /// <summary> 数据组唯一索引 </summary>
        public const string Index = "Index";
        /// <summary> 数据组名称 </summary>
        public const string Name = "Name";
        /// <summary> 数据组类型 </summary>
        public const string SubType = "RT";
        /// <summary> 全部属性 </summary>
        public const string Propertys = "Propertys";
    }

    public static class SubType : Object
    {
        /// <summary> 病程 </summary>
        public const string Proc = "P";
    }

    public class DeGroup : HCDomainItem
    {
        private bool FChanged, FReadOnly, FDeleteAllow;
        private int FTextStyleNo = HCStyle.Domain;
        #if PROCSERIES
        private bool FIsProc;
        #endif
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

        #if PROCSERIES
        private bool GetIsProcBegin()
        {
            if (this.MarkType == MarkType.cmtBeg)
                return FIsProc;
            else
                return false;
        }

        private bool GetIsProcEnd()
        {
            if (this.MarkType == MarkType.cmtEnd)
                return FIsProc;
            else
                return false;
        }
        #endif

        protected override void DoPaint(HCStyle aStyle, RECT aDrawRect, int aDataDrawTop, int aDataDrawBottom, 
            int aDataScreenTop, int aDataScreenBottom, HCCanvas aCanvas, PaintInfo aPaintInfo)
        {
            base.DoPaint(aStyle, aDrawRect, aDataDrawTop, aDataDrawBottom, aDataScreenTop, aDataScreenBottom, aCanvas, aPaintInfo);
        }

        public DeGroup(HCCustomData aOwnerData) : base(aOwnerData)
        {
            FPropertys = new Dictionary<string, string>();
            FScripts = new Dictionary<string, string>();
            FReadOnly = false;
            FDeleteAllow = false;
#if PROCSERIES
            FIsProc = false;
#endif
        }

        ~DeGroup()
        {

        }

        public override void SaveToStreamRange(Stream aStream, int aStart, int aEnd)
        {
            base.SaveToStreamRange(aStream, aStart, aEnd);
            byte vByte = 0;
            if (FDeleteAllow)
                vByte = (byte)(vByte | (1 << 3));

            if (this.Empty)
                vByte = (byte)(vByte | (1 << 4));

            aStream.WriteByte(vByte);

            byte[] buffer = System.BitConverter.GetBytes(FTextStyleNo);
            aStream.Write(buffer, 0, buffer.Length);
            HC.View.HC.HCSaveTextToStream(aStream, HC.View.HC.GetPropertyString(FPropertys));
            HC.View.HC.HCSaveTextToStream(aStream, HC.View.HC.GetPropertyString(FScripts, HC.View.HC.RecordSeparator, HC.View.HC.UnitSeparator));
        }

        public override void LoadFromStream(Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            base.LoadFromStream(aStream, aStyle, aFileVersion);

            if (aFileVersion > 58)
            {
                byte vByte = (byte)aStream.ReadByte();
                FDeleteAllow = HC.View.HC.IsOdd(vByte >> 3);
                this.Empty = HC.View.HC.IsOdd(vByte >> 4);
            }
            else
                FDeleteAllow = false;

            if (aFileVersion > 52)
            {
                byte[] vBuffer = BitConverter.GetBytes(FTextStyleNo);
                aStream.Read(vBuffer, 0, vBuffer.Length);
                FTextStyleNo = System.BitConverter.ToInt32(vBuffer, 0);

                if (!OwnerData.Style.States.Contain(HCState.hosLoading))
                {
                    if (aStyle != null)
                        FTextStyleNo = OwnerData.Style.GetStyleNo(aStyle.TextStyles[FTextStyleNo], true);
                    else
                        FTextStyleNo = 0;
                }
            }

            string vS = "";
            HC.View.HC.HCLoadTextFromStream(aStream, ref vS, aFileVersion);
            HC.View.HC.SetPropertyString(vS, FPropertys);
            CheckPropertys();
            if (aFileVersion > 57)
            {
                HC.View.HC.HCLoadTextFromStream(aStream, ref vS, aFileVersion);
                HC.View.HC.SetPropertyString(vS, FScripts, HC.View.HC.RecordSeparator, HC.View.HC.UnitSeparator);
            }
        }

        public override void Assign(HCCustomItem source)
        {
            base.Assign(source);
            FReadOnly = (source as DeGroup).ReadOnly;
            FDeleteAllow = (source as DeGroup).DeleteAllow;
            HC.View.HC.AssignProperty((source as DeGroup).Propertys, ref FPropertys);
            CheckPropertys();
            HC.View.HC.AssignProperty((source as DeGroup).FScripts, ref FScripts);
        }

        public override void ApplySelectTextStyle(HCStyle aStyle, HCStyleMatch aMatchStyle)
        {
            FTextStyleNo = aMatchStyle.GetMatchStyleNo(aStyle, FTextStyleNo);
        }

        public override void MarkStyleUsed(bool aMark)
        {
            if (aMark)
                OwnerData.Style.TextStyles[FTextStyleNo].CheckSaveUsed = true;
            else
                FTextStyleNo = OwnerData.Style.TextStyles[FTextStyleNo].TempNo;
        }

        public override void FormatToDrawItem(HCCustomData aRichData, int aItemNo)
        {
            this.Width = 0;
            if (FTextStyleNo < HCStyle.Null)
            {
                if (this.OwnerData.Style.States.Contain(HCState.hosLoading))
                {
                    HCTextStyle vTextStyle = new HCTextStyle();
                    FTextStyleNo = aRichData.Style.GetStyleNo(vTextStyle, true);
                }
                else
                    FTextStyleNo = aRichData.Style.GetDefaultStyleNo();
            }

            aRichData.Style.ApplyTempStyle(FTextStyleNo);
            this.Height = aRichData.CalculateLineHeight(aRichData.Style.TextStyles[FTextStyleNo], aRichData.Style.ParaStyles[this.ParaNo]) - aRichData.Style.LineSpaceMin;
            this.Empty = false;
            HCCustomItem vItem = null;
            if (MarkType == MarkType.cmtBeg)
            {
                if (aItemNo < aRichData.Items.Count - 1)
                {
                    vItem = aRichData.Items[aItemNo + 1];
                    if (vItem.StyleNo == this.StyleNo && (vItem as HCDomainItem).MarkType == MarkType.cmtEnd)
                    {
                        this.Width = 10;
                        this.Empty = true;
                    }
                    else
                    if (vItem.ParaFirst)
                        this.Width = 10;
                }
                else
                    this.Width = 10;
            }
            else
            {
                vItem = aRichData.Items[aItemNo - 1];
                if (vItem.StyleNo == this.StyleNo && (vItem as HCDomainItem).MarkType == MarkType.cmtBeg)
                {
                    this.Width = 10;
                    this.Empty = true;
                }
                else
                if (this.ParaFirst)
                    this.Width = 10;
            }
        }

        public override void PaintTop(HCCanvas aCanvas)
        {
            aCanvas.Pen.Width = 1;
            //int vH = this.OwnerData.Style.LineSpaceMin / 2;
            int vH = (FDrawRect.Height - this.OwnerData.Style.TextStyles[FTextStyleNo].FontHeight) / 2;
            if (this.MarkType == HC.View.MarkType.cmtBeg)
            {
                aCanvas.Pen.BeginUpdate();
                try
                {
                    aCanvas.Pen.Width = 1;
                    aCanvas.Pen.Style = HCPenStyle.psSolid;
                    aCanvas.Pen.Color = Color.FromArgb(0, 0, 255);
                }
                finally
                {
                    aCanvas.Pen.EndUpdate();
                }
                aCanvas.MoveTo(FDrawRect.Left + 2, FDrawRect.Top + vH);
                aCanvas.LineTo(FDrawRect.Left, FDrawRect.Top + vH);
                aCanvas.LineTo(FDrawRect.Left, FDrawRect.Bottom - vH);
                aCanvas.LineTo(FDrawRect.Left + 2, FDrawRect.Bottom - vH);
            }
            else
            {
                aCanvas.Pen.BeginUpdate();
                try
                {
                    aCanvas.Pen.Width = 1;
                    aCanvas.Pen.Style = HCPenStyle.psSolid;
                    aCanvas.Pen.Color = Color.FromArgb(0, 0, 255);
                }
                finally
                {
                    aCanvas.Pen.EndUpdate();
                }

                aCanvas.MoveTo(FDrawRect.Right - 2, FDrawRect.Top + vH);
                aCanvas.LineTo(FDrawRect.Right, FDrawRect.Top + vH);
                aCanvas.LineTo(FDrawRect.Right, FDrawRect.Bottom - vH);
                aCanvas.LineTo(FDrawRect.Right - 2, FDrawRect.Bottom - vH);
            }
        }

        public override int GetOffsetAt(int x)
        {
            #if PROCSERIES
            if (GetIsProcEnd())
                return HC.View.HC.OffsetBefor;
            else
            if (GetIsProcBegin())
                return HC.View.HC.OffsetAfter;
            else
            #endif
                return base.GetOffsetAt(x);
        }

        public override void ToXml(XmlElement aNode)
        {
            base.ToXml(aNode);
            aNode.SetAttribute("property", HC.View.HC.GetPropertyString(FPropertys));
        }

        public override void ParseXml(XmlElement aNode)
        {
            base.ParseXml(aNode);
            HC.View.HC.SetPropertyString(aNode.Attributes["property"].Value, FPropertys);
            CheckPropertys();
        }

        public void ToJson(string aJsonObj)
        {

        }

        public void ParseJson(string aJsonObj)
        {

        }

        public void CheckPropertys()
        {
            #if PROCSERIES
            FIsProc = this.GetValue(GroupProp.SubType) == SubType.Proc;
            #endif
        }

        public Dictionary<string, string> Propertys
        {
            get { return FPropertys; }
        }

        public bool ReadOnly
        {
            get { return FReadOnly; }
            set { FReadOnly = value; }
        }

        public bool Changed
        {
            get { return FChanged; }
            set { FChanged = value; }
        }

        public bool DeleteAllow
        {
            get { return FDeleteAllow; }
            set { FDeleteAllow = value; }
        }

        public string Index
        {
            get { return this.GetValue(GroupProp.Index); }
        }

        #if PROCSERIES
        public bool IsProc
        {
            get { return FIsProc; }
        }

        public bool IsProcBegin
        {
            get { return GetIsProcBegin(); }
        }

        public bool IsProcEnd
        {
            get { return GetIsProcEnd(); }
        }
        #endif

        public string this[string aKey]
        {
            get { return GetValue(aKey); }
            set { SetValue(aKey, value); }
        }
    }

    public class ProcInfo : HCDomainInfo
    {
        public string Index;
        public int SectionIndex;
  
        public ProcInfo() : base()
        {
            Clear();
        }

        public override void Clear()
        {
            Index = "";
            SectionIndex = -1;
            base.Clear();
        }

        public override void Assign(HCDomainInfo source)
        {
            base.Assign(source);
            Index = (source as ProcInfo).Index;
            SectionIndex = (source as ProcInfo).SectionIndex;
        }
    }
}
