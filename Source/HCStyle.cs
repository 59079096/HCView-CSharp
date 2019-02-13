/*******************************************************}
{                                                       }
{               HCView V1.1  作者：荆通                 }
{                                                       }
{      本代码遵循BSD协议，你可以加入QQ群 649023932      }
{            来获取更多的技术交流 2018-5-4              }
{                                                       }
{               文档内对象样式管理单元                  }
{                                                       }
{*******************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using HC.Win32;
using System.Runtime.InteropServices;
using System.Xml;

namespace HC.View
{
    /// <summary> 全局状态更新控制 </summary>
    public class UpdateInfo : HCObject    // 为保证更新操作相互之间不影响
    {
        // 以下字段，仅在需要为True时赋值，不可使用 RePaint := A <> B的形式，防止将其他处修改的True覆盖
        public bool RePaint,   // 所有参数只能写赋值为True的代码，不能赋值为多个变量的与、或、非
            ReCaret,  // 重新计算光标
            ReStyle,  // 重新计算光标时获取光标处样式
            ReScroll,  // 滚动到光标位置
            Selecting,  // 全局划选标识
            Draging;  // 全局拖拽标识
        
        public UpdateInfo()
        {
            RePaint = false;
            ReCaret = false;
            ReStyle = false;
            Draging = false;
        }
    }

    public delegate void InvalidateRectEventHandler(RECT aRect);

    public class HCStyle : HCObject
    {
        private HCCanvas FDefCanvas;

        private int
        /// <summary> 外部当前的段样式 </summary>
        FCurParaNo,
        /// <summary> 外部当前的文本样式 </summary>
        FCurStyleNo;

        private Color FSelColor, FBackgroudColor;
        private List<HCTextStyle> FTextStyles;
        private List<HCParaStyle> FParaStyles;
        private int FPixelsPerInchX, FPixelsPerInchY;  // 1英寸对应的像素数
        private Single FFontSizeScale;  // 字体需要缩放的比例
        private Single FPixelsPerMMX, FPixelsPerMMY;  // 1毫米dpi数
        private UpdateInfo FUpdateInfo;
        private bool FShowParaLastMark;  // 是否显示换行符
        private int FHtmlFileTempName;

        private InvalidateRectEventHandler FOnInvalidateRect;

        protected void SetShowParaLastMark(bool value)
        {
            if (FShowParaLastMark != value)
            {
                FShowParaLastMark = value;
                UpdateInfoRePaint();
            }
        }

        public const int
            Null = -1,  // TextItem和RectItem分界线
            Image = -2,
            Table = -3,
            Tab = -4,
            Line = -5,
            Express = -6,
            Vector = -7,  // SVG
            Domain = -8,
            PageBreak = -9,
            CheckBox = -10,
            Gif = -11,
            Control = -12,
            Edit = -13,
            Combobox = -14,
            QRCode = -15,
            BarCode = -16,
            Fraction = -17,
            DateTimePicker = -18,
            RadioGroup = -19,
            SupSubScript = -20,
            Custom = -1000;  // 自定义类型分界线

        public HCStyle()
        {
            FDefCanvas = CreateStyleCanvas();

            FPixelsPerInchX = GDI.GetDeviceCaps(FDefCanvas.Handle, GDI.LOGPIXELSX);  // 每英寸水平逻辑像素数，1英寸dpi数
            FPixelsPerInchY = GDI.GetDeviceCaps(FDefCanvas.Handle, GDI.LOGPIXELSY);  // 每英寸水平逻辑像素数，1英寸dpi数

            FPixelsPerMMX = GDI.GetDeviceCaps(FDefCanvas.Handle, GDI.LOGPIXELSX) / 25.4f;  // 1毫米对应像素 = 1英寸dpi数 / 1英寸对应毫米
            FPixelsPerMMY = GDI.GetDeviceCaps(FDefCanvas.Handle, GDI.LOGPIXELSY) / 25.4f;  // 1毫米对应像素 = 1英寸dpi数 / 1英寸对应毫米
            
            FFontSizeScale = 72.0f / FPixelsPerInchX;

            FBackgroudColor = Color.FromArgb(255, 255, 255);
            FSelColor = Color.FromArgb(0xA6, 0xCA, 0xF0);
            FShowParaLastMark = true;
            FUpdateInfo = new UpdateInfo();
            FTextStyles = new List<HCTextStyle>();
            FParaStyles = new List<HCParaStyle>();
        }

        public HCStyle(bool aDefTextStyle, bool aDefParaStyle) : this()
        {
            if (aDefTextStyle)
                NewDefaultTextStyle();

            if (aDefParaStyle)
                NewDefaultParaStyle();
        }

        ~HCStyle()
        {

        }

        public override void Dispose()
        {
            base.Dispose();
            DestroyStyleCanvas(FDefCanvas);
            //FTextStyles.Free;
            //FParaStyles.Free;
            FUpdateInfo.Dispose();
        }
        
        public void Initialize()
        {
            FTextStyles.RemoveRange(1, FTextStyles.Count - 1);
            FParaStyles.RemoveRange(1, FParaStyles.Count - 1);
            FCurStyleNo = 0;
            FCurParaNo = 0;
        }

        public void UpdateInfoRePaint()
        {
            FUpdateInfo.RePaint = true;
        }

        public void UpdateInfoReStyle()
        {
            FUpdateInfo.ReStyle = true;
        }

        public void UpdateInfoReScroll()
        {
            FUpdateInfo.ReScroll = true;
        }

        /// <summary> 更新光标位置/summary>
        /// <param name="aCaretStyle">重新获取光标处样式</param>
        public void UpdateInfoReCaret(bool aCaretStyle = true)
        {
            FUpdateInfo.ReCaret = true;
            if (aCaretStyle)
                FUpdateInfo.ReStyle = true;
        }

        public int AddTextStyle(HCTextStyle aTextStyle)
        {
            FTextStyles.Add(aTextStyle);
            return FTextStyles.Count - 1;
        }

        public static int GetFontHeight(HCCanvas aCanvas)
        {
            return aCanvas.TextHeight("H");
        }

        public static HCCanvas CreateStyleCanvas()
        {
            
            //IntPtr vScreenDC = User.GetDC(IntPtr.Zero);
            IntPtr vDC = (IntPtr)GDI.CreateCompatibleDC(IntPtr.Zero);
            
            HCCanvas Result = new HCCanvas(vDC);
            return Result;
        }

        public static void DestroyStyleCanvas(HCCanvas aCanvas)
        {
            aCanvas.Dispose();
        }

        /// <summary> 创建一个新字体样式 </summary>
        /// <returns>样式编号</returns>
        public int NewDefaultTextStyle()
        {
            HCTextStyle vTextStyle = new HCTextStyle();
            FTextStyles.Add(vTextStyle);
            return FTextStyles.Count - 1;
        }

        public int NewDefaultParaStyle()
        {
            HCParaStyle vParaStyle = new HCParaStyle();
            FParaStyles.Add(vParaStyle);
            return FParaStyles.Count - 1;
        }

        public int GetStyleNo(HCTextStyle aTextStyle, bool aCreateIfNull)
        {
            int Result = -1;
            for (int i = 0; i <= FTextStyles.Count - 1; i++)
            {
                if (FTextStyles[i].EqualsEx(aTextStyle))
                {
                    Result = i;
                    return Result;
                }
            }

            if (aCreateIfNull && (Result < 0))
            {
                HCTextStyle vTextStyle = new HCTextStyle();
                vTextStyle.AssignEx(aTextStyle);
                FTextStyles.Add(vTextStyle);
                Result = FTextStyles.Count - 1;
            }

            return Result;
        }

        public int GetParaNo(HCParaStyle aParaStyle, bool aCreateIfNull)
        {
            int Result = -1;
            for (int i = 0; i <= FParaStyles.Count - 1; i++)
            {
                if (FParaStyles[i].EqualsEx(aParaStyle))
                {
                    Result = i;
                    return Result;
                }
            }

            if (aCreateIfNull && (Result < 0))
            {
                HCParaStyle vParaStyle = new HCParaStyle();
                vParaStyle.AssignEx(aParaStyle);
                FParaStyles.Add(vParaStyle);
                Result = FParaStyles.Count - 1;
            }

            return Result;
        }

        private void SaveParaStyles(Stream aStream)
        {
            byte[] vBuffer = BitConverter.GetBytes(FParaStyles.Count);
            aStream.Write(vBuffer, 0, vBuffer.Length);
            for (int i = 0; i <= FParaStyles.Count - 1; i++)
                FParaStyles[i].SaveToStream(aStream);
        }

        private void SaveTextStyles(Stream aStream)
        {
            byte[] vBuffer = BitConverter.GetBytes(FTextStyles.Count);
            aStream.Write(vBuffer, 0, vBuffer.Length);
            for (int i = 0; i <= FTextStyles.Count - 1; i++)
                FTextStyles[i].SaveToStream(aStream);
        }

        public void SaveToStream(Stream aStream)
        {
            Int64 vBegPos = aStream.Position;
            byte[] vBuffer = BitConverter.GetBytes(vBegPos);
            aStream.Write(vBuffer, 0, vBuffer.Length);

            SaveParaStyles(aStream);
            SaveTextStyles(aStream);

            Int64 vEndPos = aStream.Position;
            aStream.Position = vBegPos;
            vBegPos = vEndPos - vBegPos - Marshal.SizeOf(vBegPos);
            vBuffer = BitConverter.GetBytes(vBegPos);
            aStream.Write(vBuffer, 0, vBuffer.Length);
            aStream.Position = vEndPos;
        }

        private void LoadParaStyles(Stream aStream, ushort aFileVersion)
        {
            FParaStyles.Clear();
            int vStyleCount = 0;
            byte[] vBuffer = BitConverter.GetBytes(vStyleCount);
            aStream.Read(vBuffer, 0, vBuffer.Length);
            vStyleCount = BitConverter.ToInt32(vBuffer, 0);

            for (int i = 0; i <= vStyleCount - 1; i++)
                FParaStyles[NewDefaultParaStyle()].LoadFromStream(aStream, aFileVersion);
        }

        private void LoadTextStyles(Stream aStream, ushort aFileVersion)
        {
            FTextStyles.Clear();
            int vStyleCount = 0;
            byte[] vBuffer = BitConverter.GetBytes(vStyleCount);
            aStream.Read(vBuffer, 0, vBuffer.Length);
            vStyleCount = BitConverter.ToInt32(vBuffer, 0);

            for (int i = 0; i <= vStyleCount - 1; i++)
                FTextStyles[NewDefaultTextStyle()].LoadFromStream(aStream, aFileVersion);
        }

        public void LoadFromStream(Stream aStream, ushort aFileVersion)
        {
            Int64 vDataSize = 0;
            byte[] vBuffer = BitConverter.GetBytes(vDataSize);
            aStream.Read(vBuffer, 0, vBuffer.Length);
            //
            LoadParaStyles(aStream, aFileVersion);
            LoadTextStyles(aStream, aFileVersion);
        }

        public string GetHtmlFileTempName(bool aReset = false)
        {
            if (aReset)
                FHtmlFileTempName = 0;
            else
                FHtmlFileTempName++;

            return FHtmlFileTempName.ToString();
        }

        public string ToCSS()
        {
            string Result = "<style type=\"text/css\">";
            for (int i = 0; i <= FTextStyles.Count - 1; i++)
            {
                Result = Result + HC.sLineBreak + "a.fs" + i.ToString() + " {";
                Result = Result + FTextStyles[i].ToCSS() + " }"; 
            }

            for (int i = 0; i <= FParaStyles.Count - 1; i++)
            {
                Result = Result + HC.sLineBreak + "p.ps" + i.ToString() + " {";
                Result = Result + FParaStyles[i].ToCSS() + " }";
            }

            return Result + HC.sLineBreak + "</style>";
        }

        public void ToXml(XmlElement aNode)
        {
            aNode.Attributes["fscount"].Value = FTextStyles.Count.ToString();
            aNode.Attributes["pscount"].Value = FParaStyles.Count.ToString();

            XmlElement vNode = aNode.OwnerDocument.CreateElement("textstyles");
            for (int i = 0; i <= FTextStyles.Count - 1; i++)
            {
                XmlElement vStyleNode = vNode.OwnerDocument.CreateElement("ts");
                FTextStyles[i].ToXml(vStyleNode);
                vNode.AppendChild(vStyleNode);
            }
            aNode.AppendChild(vNode);

            vNode = aNode.OwnerDocument.CreateElement("parastyles");
            for (int i = 0; i <= FParaStyles.Count - 1; i++)
            {
                XmlElement vParaNode = vNode.OwnerDocument.CreateElement("ps");
                FParaStyles[i].ToXml(vParaNode);
                vNode.AppendChild(vParaNode);
            }
            aNode.AppendChild(vNode);
        }

        public void ParseXml(XmlElement aNode)
        {
            for (int i = 0; i <= aNode.ChildNodes.Count - 1; i++)
            {
                if (aNode.ChildNodes[i].Name == "textstyles")
                {
                    FTextStyles.Clear();
                    XmlElement vNode = aNode.ChildNodes[i] as XmlElement;
                    for (int j = 0; j < vNode.ChildNodes.Count - 1; i++)
                        FTextStyles[NewDefaultTextStyle()].ParseXml(vNode.ChildNodes[j] as XmlElement);
                }
                else
                    if (aNode.ChildNodes[i].Name == "parastyles")
                    {
                        FParaStyles.Clear();
                        XmlElement vNode = aNode.ChildNodes[i] as XmlElement;
                        for (int j = 0; j < vNode.ChildNodes.Count - 1; i++)
                            FParaStyles[NewDefaultParaStyle()].ParseXml(vNode.ChildNodes[j] as XmlElement);
                    }
            }
        }

        public void InvalidateRect(RECT aRect)
        {
            if (FOnInvalidateRect != null)
                FOnInvalidateRect(aRect);
        }

        public List<HCTextStyle> TextStyles
        {
            get { return FTextStyles; }
            set { FTextStyles = value; }
        }

        public List<HCParaStyle> ParaStyles
        { 
            get { return FParaStyles; }
            set { FParaStyles = value; }
        }

        public Color BackgroudColor
        {
            get { return FBackgroudColor; }
            set { FBackgroudColor = value; }
        }

        public Color SelColor
        {
            get { return FSelColor; }
            set { FSelColor = value; }
        }

        public int CurParaNo
        {
            get { return FCurParaNo; }
            set { FCurParaNo = value; }
        }

        public int CurStyleNo
        {
            get { return FCurStyleNo; }
            set { FCurStyleNo = value; }
        }

        public HCCanvas DefCanvas
        {
            get { return FDefCanvas; }
        }

        public int PixelsPerInchX
        {
            get { return FPixelsPerInchX; }
        }

        public int PixelsPerInchY
        {
            get { return FPixelsPerInchY; }
        }

        public Single PixelsPerMMX
        {
            get { return FPixelsPerMMX; }
        }

        public Single PixelsPerMMY
        {
            get { return FPixelsPerMMY; }
        }

        public Single FontSizeScale
        {
            get { return FFontSizeScale; }
        }

        public UpdateInfo UpdateInfo
        {
            get { return FUpdateInfo; }
        }

        public bool ShowParaLastMark
        {
            get { return FShowParaLastMark; }
            set { SetShowParaLastMark(value); }
        }

        public InvalidateRectEventHandler OnInvalidateRect
        {
            get { return FOnInvalidateRect; }
            set { FOnInvalidateRect = value; }
        }
    }

    public class HCFloatStyle
    {
        public const int
            Line = 1;  // 直线
    }
}
