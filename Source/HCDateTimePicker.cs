/*******************************************************}
{                                                       }
{               HCView V1.1  作者：荆通                 }
{                                                       }
{      本代码遵循BSD协议，你可以加入QQ群 649023932      }
{            来获取更多的技术交流 2018-9-12             }
{                                                       }
{      文档CDateTimePicker(日期时间)对象实现单元        }
{                                                       }
{*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HC.Win32;

namespace HC.View
{

    public enum DateTimeArea : byte
    {
        dtaNone = 1, 
        dtaYear = 1 << 1, 
        dtaMonth = 1 << 2, 
        dtaDay = 1 << 3, 
        dtaHour = 1 << 4, 
        dtaMinute = 1 << 5, 
        dtaSecond = 1 << 6, 
        dtaMillisecond = 1 << 7
    }

    public class HCDateTimePicker : HCEditItem
    {
        private DateTime FDateTime;
        private string FFormat;
        private RECT FAreaRect;
        private DateTimeArea FActiveArea;
        private string FNewYear;
        private bool FJoinKey;

        #region 未翻译完成
        private void AppendFormat(ref RECT aRect, string aFormat)
        {

        }
        #endregion

        private RECT GetAreaRect(DateTimeArea aArea)
        {
            RECT Result = new RECT(0, 0, 0, 0);
            if (aArea == DateTimeArea.dtaNone)
                return Result;

            //int vCharOffset = 0; 
            //int vAppendLevel = 0;

            HCCanvas vCanvas = HCStyle.CreateStyleCanvas();
            try
            {
                this.OwnerData.Style.TextStyles[this.TextStyleNo].ApplyStyle(vCanvas);
                if (FFormat != "")
                    AppendFormat(ref Result, FFormat);
                else
                    AppendFormat(ref Result, "C");  // 用短格式显示日期与时间
            }
            finally
            {
                HCStyle.DestroyStyleCanvas(vCanvas);
            }

            return Result;
        }

        private DateTimeArea GetAreaAt(int x, int y)
        {
            POINT vPt = new POINT(x, y);
            if (HC.PtInRect(GetAreaRect(DateTimeArea.dtaYear), vPt))
                return DateTimeArea.dtaYear;
            else
            if (HC.PtInRect(GetAreaRect(DateTimeArea.dtaMonth), vPt))
                return DateTimeArea.dtaMonth;
            else
            if (HC.PtInRect(GetAreaRect(DateTimeArea.dtaDay), vPt))
                return DateTimeArea.dtaDay;
            else
            if (HC.PtInRect(GetAreaRect(DateTimeArea.dtaHour), vPt))
                return DateTimeArea.dtaHour;
            else
            if (HC.PtInRect(GetAreaRect(DateTimeArea.dtaMinute), vPt))
                return DateTimeArea.dtaMinute;
            else
            if (HC.PtInRect(GetAreaRect(DateTimeArea.dtaSecond), vPt))
                return DateTimeArea.dtaSecond;
            else
            if (HC.PtInRect(GetAreaRect(DateTimeArea.dtaMillisecond), vPt))
                return DateTimeArea.dtaMillisecond;
            else
                return DateTimeArea.dtaNone;
        }

        private void SetDateTime(DateTime value)
        {
            if (FDateTime != value)
            {
                FDateTime = value;
                this.Text = string.Format(FFormat, DateTime);
                FAreaRect = GetAreaRect(FActiveArea);
            }
        }

        #region SetInputYear子方法
        private uint Power10(byte Sqr)
        {
            uint Result = 10;
            for (int i = 2; i <= Result; i++)
                Result = Result * 10;

            return Result;
        }

        private int GetYear(string aYear)
        {
            int Result = FDateTime.Year;
            int vYear = 1999;
            if (!int.TryParse(aYear, out vYear))
            {
                if (vYear < Result)
                {
                    uint vPie = Power10((byte)aYear.Length);
                    Result = (int)(Result / vPie);
                    Result = (int)(Result * vPie) + vYear;
                }
            }

            return Result;
        }
        #endregion

        private void SetInputYear()
        {
            if (FNewYear != "")
            {
                this.DateTime = new System.DateTime(GetYear(FNewYear), FDateTime.Month, FDateTime.Day);
                FNewYear = "";
            }
        }

        private void SetFormat(string value)
        {
            if (FFormat != value)
            {
                FFormat = value;
                this.Text = string.Format(FFormat, FDateTime);
                FAreaRect = GetAreaRect(FActiveArea);
            }
        }

        protected override void DoPaint(HCStyle aStyle, RECT aDrawRect, int aDataDrawTop, int aDataDrawBottom, int aDataScreenTop, int aDataScreenBottom, HCCanvas aCanvas, PaintInfo aPaintInfo)
        {
            RECT vAreaRect = FAreaRect;
            vAreaRect.Offset(aDrawRect.Left, aDrawRect.Top);

            if ((FActiveArea != DateTimeArea.dtaNone) && (!this.IsSelectComplate) && (!aPaintInfo.Print))
            {
                aCanvas.Brush.Color = aStyle.SelColor;
                aCanvas.FillRect(vAreaRect);
            }

 	        base.DoPaint(aStyle, aDrawRect, aDataDrawTop, aDataDrawBottom, aDataScreenTop, aDataScreenBottom, aCanvas, aPaintInfo);

            if ((FActiveArea == DateTimeArea.dtaYear) && (FNewYear != "") && (!aPaintInfo.Print))
            {
                aCanvas.Brush.Color = aStyle.SelColor;
                aCanvas.FillRect(vAreaRect);
                User.DrawText(aCanvas.Handle, FNewYear, -1, ref vAreaRect, User.DT_RIGHT | User.DT_SINGLELINE);
            }
        }

        protected override void SetActive(bool Value)
        {
            base.SetActive(Value);
            if (!this.Active)
            {
                if (FActiveArea == DateTimeArea.dtaYear)
                    SetInputYear();

                FActiveArea = DateTimeArea.dtaNone;
            }
        }

        public override void MouseDown(System.Windows.Forms.MouseEventArgs e)
        {
 	         //base.MouseDown(e);
            this.Active = HC.PtInRect(new RECT(0, 0, Width, Height), new POINT(e.X, e.Y));

            DateTimeArea vArea = GetAreaAt(e.X, e.Y);
            if (vArea != FActiveArea)
            {
                if (FActiveArea == DateTimeArea.dtaYear)
                    SetInputYear();

                FActiveArea = vArea;
                if (FActiveArea != DateTimeArea.dtaNone)
                    FAreaRect = GetAreaRect(FActiveArea);

                this.OwnerData.Style.UpdateInfoRePaint();
            }
        }

        public override bool WantKeyDown(System.Windows.Forms.KeyEventArgs e)
        {
            return true;
        }

        public override void KeyDown(System.Windows.Forms.KeyEventArgs e)
        {
 	         //base.KeyDown(e);
            switch (e.KeyValue)
            {
                case User.VK_ESCAPE:
                    if (FNewYear != "")
                    {
                        FNewYear = "";
                        this.OwnerData.Style.UpdateInfoRePaint();
                    }
                    break;

                case User.VK_RETURN:
                    if (FActiveArea == DateTimeArea.dtaYear)
                    {
                        SetInputYear();
                        this.OwnerData.Style.UpdateInfoRePaint();
                    }
                    break;

                case User.VK_LEFT:
                    if ((byte)FActiveArea > (byte)DateTimeArea.dtaNone)
                    {
                        if (FActiveArea == DateTimeArea.dtaYear)
                            SetInputYear();

                        FActiveArea = (DateTimeArea)(((byte)FActiveArea) >> 1);
                        FAreaRect = GetAreaRect(FActiveArea);
                        this.OwnerData.Style.UpdateInfoRePaint();
                    }
                    break;

                case User.VK_RIGHT:
                    if ((byte)FActiveArea < (byte)DateTimeArea.dtaMillisecond)
                    {
                        if (FActiveArea == DateTimeArea.dtaYear)
                            SetInputYear();

                        FActiveArea = (DateTimeArea)(((byte)FActiveArea) << 1);
                        FAreaRect = GetAreaRect(FActiveArea);
                        this.OwnerData.Style.UpdateInfoRePaint();
                    }
                    break;
            }
        }

        public override void KeyPress(ref char key)
        {
 	         base.KeyPress(ref key);
        }

        public override bool InsertText(string aText)
        {
            return false;// base.InsertText(aText);
        }

        public override void GetCaretInfo(ref HCCaretInfo aCaretInfo)
        {
 	         //base.GetCaretInfo(ref aCaretInfo);
            aCaretInfo.Visible = false;
        }

        public HCDateTimePicker(HCCustomData aOwnerData, DateTime aDateTime)
            : base(aOwnerData, string.Format("YYYY-MM-DD HH:mm:SS", aDateTime))
        {
            FFormat = "YYYY-MM-DD HH:mm:SS";
            FDateTime = aDateTime;
            this.StyleNo = HCStyle.DateTimePicker;
            Width = 80;
            this.FMargin = 2;
            FActiveArea = DateTimeArea.dtaNone;
        }

        public override void Assign(HCCustomItem source)
        {
 	        base.Assign(source);
            FFormat = (source as HCDateTimePicker).Format;
            DateTime = (source as HCDateTimePicker).DateTime;
        }

        public override void SaveToStream(System.IO.Stream aStream, int aStart, int aEnd)
        {
 	        base.SaveToStream(aStream, aStart, aEnd);
            HC.HCSaveTextToStream(aStream, FFormat);  // 存Format

            byte[] vBuffer = BitConverter.GetBytes(FDateTime.ToOADate());
            aStream.Write(vBuffer, 0, vBuffer.Length);
        }

        public override void LoadFromStream(System.IO.Stream aStream, HCStyle aStyle, ushort aFileVersion)
        {
            base.LoadFromStream(aStream, aStyle, aFileVersion);
            HC.HCLoadTextFromStream(aStream, ref FFormat, aFileVersion);
            double vDT = 0;
            byte[] vBuffer = BitConverter.GetBytes(vDT);
            aStream.Read(vBuffer, 0, vBuffer.Length);
            vDT = BitConverter.ToDouble(vBuffer, 0);
            FDateTime = DateTime.FromOADate(vDT);
        }

        public override void ToXml(System.Xml.XmlElement aNode)
        {
 	        base.ToXml(aNode);
            aNode.SetAttribute("format", FFormat);
            aNode.SetAttribute("datetime", FDateTime.ToString());
        }

        public override void  ParseXml(System.Xml.XmlElement aNode)
        {
            base.ParseXml(aNode);
            FFormat = aNode.Attributes["format"].Value;
            FDateTime = DateTime.Parse(aNode.Attributes["datetime"].Value);
        }

        public string Format
        {
            get { return FFormat; }
            set { SetFormat(value); }
        }

        public DateTime DateTime
        {
            get { return FDateTime; }
            set { FDateTime = value; }
        }
    }
}
