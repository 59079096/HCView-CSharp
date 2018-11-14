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

    public delegate void InvalidateRectEventHandler(RECT ARect);

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
        private Single FPixelsPerMMX, FPixelsPerMMY;  // 1毫米dpi数
        private UpdateInfo FUpdateInfo;
        private bool FShowLineLastMark,  // 是否显示换行符
            FEnableUndo;

        private InvalidateRectEventHandler FOnInvalidateRect;

        protected void SetShowLineLastMark(bool Value)
        {
            if (FShowLineLastMark != Value)
            {
                FShowLineLastMark = Value;
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

            FPixelsPerMMX = GDI.GetDeviceCaps(FDefCanvas.Handle, GDI.LOGPIXELSX) / 25.4f;  // 1毫米对应像素 = 1英寸dpi数 / 1英寸对应毫米
            FPixelsPerMMY = GDI.GetDeviceCaps(FDefCanvas.Handle, GDI.LOGPIXELSY) / 25.4f;  // 1毫米对应像素 = 1英寸dpi数 / 1英寸对应毫米

            FBackgroudColor = Color.FromArgb(255, 255, 255);
            FSelColor = Color.FromArgb(0xA6, 0xCA, 0xF0);
            FShowLineLastMark = true;
            FEnableUndo = false;
            FUpdateInfo = new UpdateInfo();
            FTextStyles = new List<HCTextStyle>();
            FParaStyles = new List<HCParaStyle>();
        }

        public HCStyle(bool ADefTextStyle, bool ADefParaStyle) : this()
        {
            if (ADefTextStyle)
                NewDefaultTextStyle();

            if (ADefParaStyle)
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
        /// <param name="ACaretStyle">重新获取光标处样式</param>
        public void UpdateInfoReCaret(bool ACaretStyle = true)
        {
            FUpdateInfo.ReCaret = true;
            if (ACaretStyle)
                FUpdateInfo.ReStyle = true;
        }

        public int AddTextStyle(HCTextStyle ATextStyle)
        {
            FTextStyles.Add(ATextStyle);
            return FTextStyles.Count - 1;
        }

        public static int GetFontHeight(HCCanvas ACanvas)
        {
            return ACanvas.TextHeight("H");
        }

        public static HCCanvas CreateStyleCanvas()
        {
            
            //IntPtr vScreenDC = User.GetDC(IntPtr.Zero);
            IntPtr vDC = (IntPtr)GDI.CreateCompatibleDC(IntPtr.Zero);
            
            HCCanvas Result = new HCCanvas(vDC);
            return Result;
        }

        public static void DestroyStyleCanvas(HCCanvas ACanvas)
        {
            ACanvas.Dispose();
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

        public int GetStyleNo(HCTextStyle ATextStyle, bool ACreateIfNull)
        {
            int Result = -1;
            for (int i = 0; i <= FTextStyles.Count - 1; i++)
            {
                if (FTextStyles[i].EqualsEx(ATextStyle))
                {
                    Result = i;
                    return Result;
                }
            }

            if (ACreateIfNull && (Result < 0))
            {
                HCTextStyle vTextStyle = new HCTextStyle();
                vTextStyle.AssignEx(ATextStyle);
                FTextStyles.Add(vTextStyle);
                Result = FTextStyles.Count - 1;
            }

            return Result;
        }

        public int GetParaNo(HCParaStyle AParaStyle, bool ACreateIfNull)
        {
            int Result = -1;
            for (int i = 0; i <= FParaStyles.Count - 1; i++)
            {
                if (FParaStyles[i].EqualsEx(AParaStyle))
                {
                    Result = i;
                    return Result;
                }
            }

            if (ACreateIfNull && (Result < 0))
            {
                HCParaStyle vParaStyle = new HCParaStyle();
                vParaStyle.AssignEx(AParaStyle);
                FParaStyles.Add(vParaStyle);
                Result = FParaStyles.Count - 1;
            }

            return Result;
        }

        private void SaveParaStyles(Stream AStream)
        {
            byte[] vBuffer = BitConverter.GetBytes(FParaStyles.Count);
            AStream.Write(vBuffer, 0, vBuffer.Length);
            for (int i = 0; i <= FParaStyles.Count - 1; i++)
                FParaStyles[i].SaveToStream(AStream);
        }

        private void SaveTextStyles(Stream AStream)
        {
            byte[] vBuffer = BitConverter.GetBytes(FTextStyles.Count);
            AStream.Write(vBuffer, 0, vBuffer.Length);
            for (int i = 0; i <= FTextStyles.Count - 1; i++)
                FTextStyles[i].SaveToStream(AStream);
        }

        public void SaveToStream(Stream AStream)
        {
            Int64 vBegPos = AStream.Position;
            byte[] vBuffer = BitConverter.GetBytes(vBegPos);
            AStream.Write(vBuffer, 0, vBuffer.Length);

            SaveParaStyles(AStream);
            SaveTextStyles(AStream);

            Int64 vEndPos = AStream.Position;
            AStream.Position = vBegPos;
            vBegPos = vEndPos - vBegPos - Marshal.SizeOf(vBegPos);
            vBuffer = BitConverter.GetBytes(vBegPos);
            AStream.Write(vBuffer, 0, vBuffer.Length);
            AStream.Position = vEndPos;
        }

        private void LoadParaStyles(Stream AStream, ushort AFileVersion)
        {
            FParaStyles.Clear();
            int vStyleCount = 0;
            byte[] vBuffer = BitConverter.GetBytes(vStyleCount);
            AStream.Read(vBuffer, 0, vBuffer.Length);
            vStyleCount = BitConverter.ToInt32(vBuffer, 0);

            for (int i = 0; i <= vStyleCount - 1; i++)
                FParaStyles[NewDefaultParaStyle()].LoadFromStream(AStream, AFileVersion);
        }

        private void LoadTextStyles(Stream AStream, ushort AFileVersion)
        {
            FTextStyles.Clear();
            int vStyleCount = 0;
            byte[] vBuffer = BitConverter.GetBytes(vStyleCount);
            AStream.Read(vBuffer, 0, vBuffer.Length);
            vStyleCount = BitConverter.ToInt32(vBuffer, 0);

            for (int i = 0; i <= vStyleCount - 1; i++)
                FTextStyles[NewDefaultTextStyle()].LoadFromStream(AStream, AFileVersion);
        }

        public void LoadFromStream(Stream AStream, ushort AFileVersion)
        {
            Int64 vDataSize = 0;
            byte[] vBuffer = BitConverter.GetBytes(vDataSize);
            AStream.Read(vBuffer, 0, vBuffer.Length);
            //
            LoadParaStyles(AStream, AFileVersion);
            LoadTextStyles(AStream, AFileVersion);
        }

        public void InvalidateRect(RECT ARect)
        {
            if (FOnInvalidateRect != null)
                FOnInvalidateRect(ARect);
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

        public Single PixelsPerMMX
        {
            get { return FPixelsPerMMX; }
        }

        public Single PixelsPerMMY
        {
            get { return FPixelsPerMMY; }
        }

        public UpdateInfo UpdateInfo
        {
            get { return FUpdateInfo; }
        }

        public bool ShowLineLastMark
        {
            get { return FShowLineLastMark; }
            set { SetShowLineLastMark(value); }
        }

        public bool EnableUndo
        {
            get { return FEnableUndo; }
            set { FEnableUndo = value; }
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
            Line = 1;
    }
}
