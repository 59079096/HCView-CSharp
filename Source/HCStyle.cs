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

    public class HCStateDictionary : HCObject
    {
        public HCState State;
        public int Count;
    }

    public class HCStates : HCObject
    {
        private List<HCStateDictionary> FStates;
        
        private void DeleteState(int aIndex)
        {
            FStates.RemoveAt(aIndex);
        }

        private int GetStateIndex(HCState aState)
        {
            for (int i = 0; i < FStates.Count; i++)
            {
                if (FStates[i].State == aState)
                    return i;
            }

            return -1;
        }

        public HCStates()
        {
            FStates = new List<HCStateDictionary>();
        }

        ~HCStates()
        {

        }

        public void Include(HCState aState)
        {
            int vIndex = GetStateIndex(aState);
            if (vIndex >= 0)
                FStates[vIndex].Count++;
            else
            {
                HCStateDictionary vStateDic = new HCStateDictionary();
                vStateDic.State = aState;
                vStateDic.Count = 1;
                FStates.Add(vStateDic);
            }
        }

        public void Exclude(HCState aState)
        {
            int vIndex = GetStateIndex(aState);
            if (vIndex >= 0)
            {
                if (FStates[vIndex].Count > 1)
                    FStates[vIndex].Count--;
                else
                    DeleteState(vIndex);
            }
        }

        public bool Contain(HCState aState)
        {
            return GetStateIndex(aState) >= 0;
        }
    }

    public delegate void InvalidateRectEventHandler(RECT aRect);

    public class HCStyle : HCObject
    {
        private HCCanvas FTempCanvas, FLineHeightCanvas;
        private int FTempStyleNo;
        private byte FLineSpaceMin;
        private Color FSelColor, FBackgroudColor;
        private List<HCTextStyle> FTextStyles;
        private List<HCParaStyle> FParaStyles;
        private UpdateInfo FUpdateInfo;
        private bool FShowParaLastMark;  // 是否显示换行符
        private int FHtmlFileTempName;
        private HCStates FStates;  // 全局操作状态

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
            FTempCanvas = CreateStyleCanvas();
            FLineHeightCanvas = CreateStyleCanvas();
            FTempStyleNo = HCStyle.Null;
            FBackgroudColor = Color.FromArgb(255, 255, 255);
            FSelColor = Color.FromArgb(0xA6, 0xCA, 0xF0);
            FLineSpaceMin = 8;
            FShowParaLastMark = true;
            FStates = new HCStates();
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
            DestroyStyleCanvas(FTempCanvas);
            DestroyStyleCanvas(FLineHeightCanvas);
            //FTextStyles.Free;
            //FParaStyles.Free;
            FUpdateInfo.Dispose();
            FStates.Dispose();
        }
        
        public void Initialize()
        {
            FTextStyles.RemoveRange(1, FTextStyles.Count - 1);
            FParaStyles.RemoveRange(1, FParaStyles.Count - 1);
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
            FUpdateInfo.ReStyle = aCaretStyle;
        }

        public int AddTextStyle(HCTextStyle aTextStyle)
        {
            FTextStyles.Add(aTextStyle);
            return FTextStyles.Count - 1;
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

        public void ApplyTempStyle(int value, Single aScale = 1)
        {
            if (FTempStyleNo != value)
            {
                FTempStyleNo = value;
                if (value > HCStyle.Null)
                    FTextStyles[value].ApplyStyle(FTempCanvas, aScale);
            }
        }

        #region SaveToStream子方法
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
        #endregion

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

        #region LoadFromStream
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
        #endregion

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
            // 注意单倍行间距，文本底色不为白色也不透明时会造成字符看起来叠加的问题
            for (int i = 0; i <= FParaStyles.Count - 1; i++)
            {
                Result = Result + HC.sLineBreak + "p.ps" + i.ToString() + " {";
                Result = Result + FParaStyles[i].ToCSS() + " }";
            }

            return Result + HC.sLineBreak + "</style>";
        }

        public void ToXml(XmlElement aNode)
        {
            aNode.SetAttribute("fscount", FTextStyles.Count.ToString());
            aNode.SetAttribute("pscount", FParaStyles.Count.ToString());

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
                    for (int j = 0; j <= vNode.ChildNodes.Count - 1; j++)
                        FTextStyles[NewDefaultTextStyle()].ParseXml(vNode.ChildNodes[j] as XmlElement);
                }
                else
                if (aNode.ChildNodes[i].Name == "parastyles")
                {
                    FParaStyles.Clear();
                    XmlElement vNode = aNode.ChildNodes[i] as XmlElement;
                    for (int j = 0; j <= vNode.ChildNodes.Count - 1; j++)
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

        public byte LineSpaceMin
        {
            get { return FLineSpaceMin; }
            set { FLineSpaceMin = value; }
        }

        public int TempStyleNo
        {
            get { return FTempStyleNo; }
        }

        public HCCanvas TempCanvas
        {
            get { return FTempCanvas; }
        }

        public HCCanvas LineHeightCanvas
        {
            get { return FLineHeightCanvas; }
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

        public HCStates States
        {
            get { return FStates; }
        }

        public InvalidateRectEventHandler OnInvalidateRect
        {
            get { return FOnInvalidateRect; }
            set { FOnInvalidateRect = value; }
        }
    }
}
