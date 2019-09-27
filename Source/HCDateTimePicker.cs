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
        private string FNewYear = "";
        private bool FJoinKey = false;

        private void GetAreaPosition(string aChar, bool aUpper, ref int aIndex, ref int aCount)
        {
            bool vFind = false;
            string vs = "";
            aIndex = -1;
            aCount = 0;
            for (int i = 0; i < FFormat.Length; i++)
            {
                if (aUpper)
                    vs = FFormat[i].ToString().ToUpper();
                else
                    vs = FFormat[i].ToString();

                if (vs == aChar)  // 找到
                {
                    if (!vFind)
                        vFind = true;

                    if (aIndex < 0)
                        aIndex = i;

                    aCount++;
                }
                else
                if (vFind)  // 在收录中出现了不相等的了
                    return;
            }
        }

        private void AppendFormat(HCCanvas aCanvas, DateTimeArea aArea, ref RECT aRect)
        {
            aRect.SetEmpty();

            int vIndex = 0, vCount = -1;

            switch (aArea)
            {
                case DateTimeArea.dtaYear:
                    GetAreaPosition("y", false, ref vIndex, ref vCount);
                    break;

                case DateTimeArea.dtaMonth:
                    GetAreaPosition("M", true, ref vIndex, ref vCount);
                    break;

                case DateTimeArea.dtaDay:
                    GetAreaPosition("d", false, ref vIndex, ref vCount);
                    break;

                case DateTimeArea.dtaHour:
                    GetAreaPosition("h", false, ref vIndex, ref vCount);
                    break;

                case DateTimeArea.dtaMinute:
                    GetAreaPosition("m", false, ref vIndex, ref vCount);
                    break;

                case DateTimeArea.dtaSecond:
                    GetAreaPosition("s", false, ref vIndex, ref vCount);
                    break;
            }

            if (vCount > 0)
            {
                string vs = "";
                if (vIndex > 0)
                    vs = FFormat.Substring(0, vIndex);

                aRect.Left = FMargin;
                if (vs != "")
                    aRect.Left += aCanvas.TextExtent(vs).cx;

                SIZE vSize = aCanvas.TextExtent(FFormat.Substring(vIndex, vCount));

                aRect.Top = (Height - vSize.cy) / 2;
                aRect.Right = aRect.Left + vSize.cx;
                aRect.Bottom = aRect.Top + vSize.cy;
            }
        }

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
                    AppendFormat(vCanvas, aArea, ref Result);
                //else
                //    AppendFormat(ref Result, "C");  // 用短格式显示日期与时间
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
                this.Text = string.Format("{0:" + FFormat + "}", FDateTime);
                FAreaRect = GetAreaRect(FActiveArea);
            }
        }

        #region SetInputYear子方法
        private uint Power10(byte sqr)
        {
            uint Result = 10;
            for (byte i = 2; i <= sqr; i++)
                Result = Result * 10;

            return Result;
        }

        private int GetYear(string aYear)
        {
            int Result = FDateTime.Year;
            int vYear = 1999;
            if (int.TryParse(aYear, out vYear))
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
                this.DateTime = new System.DateTime(GetYear(FNewYear), FDateTime.Month, FDateTime.Day,
                    FDateTime.Hour, FDateTime.Minute, FDateTime.Second);
                FNewYear = "";
            }
        }

        private void SetFormat(string value)
        {
            if (FFormat != value)
            {
                FFormat = value;
                if (FFormat.Substring(0, 3) != "{0:")  // 兼容旧的
                    this.Text = string.Format("{0:" + FFormat + "}", FDateTime);

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

        public override bool MouseDown(System.Windows.Forms.MouseEventArgs e)
        {
            //return base.MouseDown(e);
            
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

            return true;
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
            //base.KeyPress(ref key);
            if (this.ReadOnly)
                return;

            int vNumber = 0;
            DateTime vDateTime = FDateTime;
            if (FActiveArea != DateTimeArea.dtaNone)
            {
                if ("0123456789".IndexOf(key) >= 0)
                {
                    switch (FActiveArea)
                    {
                        case DateTimeArea.dtaYear:
                            if (FNewYear.Length > 3)
                                FNewYear.Remove(0, 1);
                            FNewYear = FNewYear + key;
                            break;

                        case DateTimeArea.dtaMonth:
                            vNumber = vDateTime.Month;  // 当前月份
                            if (vNumber > 9)  // 当前月份已经是2位数了
                            {
                                if (key == '0')  // 2位月份再输入0不处理
                                    return;

                                vDateTime = new DateTime(vDateTime.Year, int.Parse(key.ToString()), vDateTime.Day, 
                                    vDateTime.Hour, vDateTime.Minute, vDateTime.Second);
                            }
                            else  // 当前月份是1位数字
                            if ((vNumber == 1) && FJoinKey)  // 当前月份是1月且是连续输入
                            {
                                if ("012".IndexOf(key) >= 0)  // 10, 11, 12
                                {
                                    vNumber = vNumber * 10 + int.Parse(key.ToString());
                                    vDateTime = new DateTime(vDateTime.Year, vNumber, vDateTime.Day, 
                                        vDateTime.Hour, vDateTime.Minute, vDateTime.Second);  // 直接修改为新键入
                                }
                            }
                            else  // // 不是连续输入，是第1次输入
                            {
                                if (key == '0')  // 月份第1位是0不处理
                                    return;

                                vDateTime = new DateTime(vDateTime.Year, int.Parse(key.ToString()), vDateTime.Day, 
                                    vDateTime.Hour, vDateTime.Minute, vDateTime.Second);  // 直接修改为新键入
                            }
                            break;

                        case DateTimeArea.dtaDay:
                            vNumber = vDateTime.Day;  // 当前日期
                            if (vNumber > 9)  // 当前日期已经是2位数了
                            {
                                if (key == '0')  // 2位日期再输入0不处理
                                    return;

                                vDateTime = new DateTime(vDateTime.Year, vDateTime.Month, int.Parse(key.ToString()), 
                                    vDateTime.Hour, vDateTime.Minute, vDateTime.Second);  // 直接修改为新键入
                            }
                            else  // 当前日期是1位数字
                            if (FJoinKey)  // 是连续输入
                            {
                                vNumber = vNumber * 10 + int.Parse(key.ToString());
                                if (vNumber > DateTime.DaysInMonth(vDateTime.Year, vDateTime.Month))
                                    vNumber = int.Parse(key.ToString());

                                vDateTime = new DateTime(vDateTime.Year, vDateTime.Month, vNumber,
                                    vDateTime.Hour, vDateTime.Minute, vDateTime.Second);  // 直接修改为新键入
                            }
                            break;

                        case DateTimeArea.dtaHour:
                            vNumber = vDateTime.Hour;  // 当前时
                            if (vNumber > 9)  // 当前时已经2位数了
                            {
                                if (key == '0')
                                    return;  // 2位时再输入0不处理

                                vDateTime = new DateTime(vDateTime.Year, vDateTime.Month, vDateTime.Day,
                                    vDateTime.Hour, vDateTime.Minute, vDateTime.Second);
                            }
                            else  // 当前时是1位数字
                            if (FJoinKey)  // 当前时是连续输入
                            {
                                vNumber = vNumber * 10 + int.Parse(key.ToString());
                                if (vNumber > 24)
                                    vNumber = int.Parse(key.ToString());

                                vDateTime = new DateTime(vDateTime.Year, vDateTime.Month, vDateTime.Day,
                                    vNumber, vDateTime.Minute, vDateTime.Second);
                            }
                            else  // 不是连续输入，是第1次输入
                            {
                                if (key == '0')
                                    return;

                                vDateTime = new DateTime(vDateTime.Year, vDateTime.Month, vDateTime.Day,
                                    int.Parse(key.ToString()), vDateTime.Minute, vDateTime.Second);
                            }
                            break;

                        case DateTimeArea.dtaMinute:
                            vNumber = vDateTime.Minute;
                            if (vNumber > 9)  // 当前分已经是2位数了
                            {
                                if (key == '0')
                                    return;  // 2位时再输入0不处理

                                vDateTime = new DateTime(vDateTime.Year, vDateTime.Month, vDateTime.Day,
                                    vDateTime.Hour, int.Parse(key.ToString()), vDateTime.Second);
                            }
                            else  // 当前分是1位数字
                            if (FJoinKey)  // 当前分是连续输入
                            {
                                vNumber = vNumber * 10 + int.Parse(key.ToString());
                                if (vNumber > 60)
                                    vNumber = int.Parse(key.ToString());

                                vDateTime = new DateTime(vDateTime.Year, vDateTime.Month, vDateTime.Day,
                                    vDateTime.Hour, vNumber, vDateTime.Second);
                            }
                            else  // 不是连续输入，是第1次输入
                            {
                                if (key == '0')
                                    return;  // 分第1位是0不处理

                                vDateTime = new DateTime(vDateTime.Year, vDateTime.Month, vDateTime.Day,
                                    vDateTime.Minute, int.Parse(key.ToString()), vDateTime.Second);
                            }
                            break;

                        case DateTimeArea.dtaSecond:
                            {
                                vNumber = vDateTime.Second;  // 当前秒
                                if (vNumber > 9)  // 当前秒已经是2位数了
                                {
                                    if (key == '0')
                                        return;  // 2位时再输入0不处理

                                    vDateTime = new DateTime(vDateTime.Year, vDateTime.Month, vDateTime.Day,
                                        vDateTime.Hour, vDateTime.Minute, int.Parse(key.ToString()));
                                }
                                else  // 当前秒是1位数字
                                if (FJoinKey)  // 当前秒是连续输入
                                {
                                    vNumber = vNumber * 10 + int.Parse(key.ToString());
                                    if (vNumber > 60)
                                        vNumber = int.Parse(key.ToString());

                                    vDateTime = new DateTime(vDateTime.Year, vDateTime.Month, vDateTime.Day,
                                        vDateTime.Hour, vDateTime.Minute, vNumber);
                                }
                                else  // 不是连续输入，是第1次输入
                                {
                                    if (key == '0')
                                        return;  // 秒第1位是0不处理

                                    vDateTime = new DateTime(vDateTime.Year, vDateTime.Month, vDateTime.Day,
                                        vDateTime.Hour, vDateTime.Minute, int.Parse(key.ToString()));
                                }
                            }
                            break;

                        case DateTimeArea.dtaMillisecond:
                            break;
                    }
                }

                if (FActiveArea != DateTimeArea.dtaYear)  // 除年外，其他的需要实时更新
                {
                    FActiveArea = GetAreaAt(FAreaRect.Left, FAreaRect.Top);
                    if (FActiveArea != DateTimeArea.dtaNone)
                        FAreaRect = GetAreaRect(FActiveArea);

                    FJoinKey = true;
                    SetDateTime(vDateTime);
                }

                this.OwnerData.Style.UpdateInfoRePaint();
            }
        }

        public override bool InsertText(string aText)
        {
            return false;
        }

        public override void GetCaretInfo(ref HCCaretInfo aCaretInfo)
        {
            aCaretInfo.Visible = false;
        }

        public HCDateTimePicker(HCCustomData aOwnerData, DateTime aDateTime)
            : base(aOwnerData, string.Format("{0:yyyy-MM-dd hh:mm:ss}", aDateTime))
        {
            FFormat = "yyyy-MM-dd hh:mm:ss";
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
            FDateTime = (source as HCDateTimePicker).DateTime;
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
            if (FFormat.Substring(0, 3) == "{0:")  // 兼容旧的
            {
                int vLength = FFormat.IndexOf('}');
                FFormat = Format.Substring(3, vLength - 3).Replace("SS", "ss").Replace("HH", "hh");
            }

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
            set { SetDateTime(value); }
        }
    }
}
