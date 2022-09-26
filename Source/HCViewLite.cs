using HC.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;

namespace HC.View
{
    public class HCViewLite : Object
    {
        private HCStyle FStyle;
        private List<HCSection> FSections;
        private int FActiveSectionIndex;

        private HCSection NewDefaultSection()
        {
            HCSection Result = new HCSection(FStyle);
            Result.OnPaintFooterAfter = DoSectionPaintFooterAfter;
            Result.OnCreateItemByStyle = DoSectionCreateStyleItem;
            return Result;
        }

        private void DoLoadFromStream(Stream stream, HCStyle style, LoadSectionProcHandler loadSectionProc)
        {
            stream.Position = 0;
            string vFileExt = "";
            ushort vFileVersion = 0;
            byte vLang = 0;
            HC._LoadFileFormatAndVersion(stream, ref vFileExt, ref vFileVersion, ref vLang);
            if (vFileExt != HC.HC_EXT)
                throw new Exception("加载失败，不是" + HC.HC_EXT + "文件！");

            if (vFileVersion > HC.HC_FileVersionInt)
                throw new Exception("加载失败，当前编辑器最高支持版本为"
                    + HC.HC_FileVersionInt.ToString() + "的文件，无法打开版本为" + vFileVersion.ToString() + "的文件！");

            if (vFileVersion > 59)
            {
                if ((byte)stream.ReadByte() != HC.HC_STREAM_VIEW)
                    return;
            }

            DoLoadStreamBefor(stream, vFileVersion);  // 触发加载前事件
            style.LoadFromStream(stream, vFileVersion);  // 加载样式表
            DoLoadMutMargin(stream, style, vFileVersion);

            if (vFileVersion > 55)
            {
                vLang = (byte)stream.ReadByte();
                vLang = (byte)stream.ReadByte();  // 布局方式
                uint vSize = 0;
                byte[] vBuffer = BitConverter.GetBytes(vSize);
                stream.Read(vBuffer, 0, vBuffer.Length);
                vSize = BitConverter.ToUInt32(vBuffer, 0);
            }

            loadSectionProc(vFileVersion);  // 加载节数量、节数据
            DoLoadStreamAfter(stream, vFileVersion);
        }

        private HCSection GetActiveSection()
        {
            return FSections[FActiveSectionIndex];
        }

        protected virtual void Create()
        {

        }

        protected void DoSectionPaintFooterAfter(object sender, int pageIndex, RECT rect, HCCanvas canvas, SectionPaintInfo paintInfo)
        {
            HCSection vSection = sender as HCSection;
            if (vSection.PageNoVisible)
            {
                int vSectionIndex = FSections.IndexOf(vSection);
                int vSectionStartPageIndex = 0;
                int vAllPageCount = 0;
                for (int i = 0; i <= FSections.Count - 1; i++)
                {
                    if (i == vSectionIndex)
                        vSectionStartPageIndex = vAllPageCount;

                    vAllPageCount = vAllPageCount + FSections[i].PageCount;
                }

                string vS = string.Format(vSection.PageNoFormat, vSectionStartPageIndex + vSection.PageNoFrom + pageIndex, vAllPageCount);
                canvas.Brush.Style = HCBrushStyle.bsClear;

                canvas.Font.BeginUpdate();
                try
                {
                    canvas.Font.Size = 10;
                    canvas.Font.Family = "宋体";
                }
                finally
                {
                    canvas.Font.EndUpdate();
                }

                canvas.TextOut(rect.Left + (rect.Width - canvas.TextWidth(vS)) / 2, rect.Top + vSection.Footer.Height, vS);
            }
        }

        protected virtual HCCustomItem DoSectionCreateStyleItem(HCCustomData data, int styleNo)
        {
            return null;
        }

        protected virtual void DoSaveMutMargin(Stream stream)
        {
            stream.WriteByte(0);
        }

        protected virtual void DoLoadMutMargin(Stream stream, HCStyle style, ushort fileVersion)
        {
            if (fileVersion > 61)
                stream.ReadByte();
        }

        protected virtual void DoSaveStreamBefor(Stream stream)
        {

        }

        protected virtual void DoSaveStreamAfter(Stream stream)
        {

        }

        protected virtual void DoLoadStreamBefor(Stream stream, ushort fileVersion)
        {

        }

        protected virtual void DoLoadStreamAfter(Stream stream, ushort fileVersion)
        {

        }

        protected void DataLoadLiteStream(Stream stream, HCLoadProc proc)
        {
            string vFileFormat = "";
            ushort vFileVersion = 0;
            byte vLang = 0;
            HC._LoadFileFormatAndVersion(stream, ref vFileFormat, ref vFileVersion, ref vLang);
            if (vFileVersion > 59)
            {
                if ((byte)stream.ReadByte() != HC.HC_STREAM_LITE)
                    return;
            }

            using (HCStyle vStyle = new HCStyle())
            {
                vStyle.LoadFromStream(stream, vFileVersion);
                proc(vFileVersion, vStyle);
            }
        }

        public HCViewLite() : base()
        {
            Create();
            FStyle = new HCStyle(true, true);
            FSections = new List<HCSection>();
            FSections.Add(NewDefaultSection());
            FActiveSectionIndex = 0;
        }

        ~HCViewLite()
        {

        }

        public static void DeleteUnUsedStyle(HCStyle style, List<HCSection> sections, HashSet<SectionArea> parts)
        {
            style.TextStyles[0].CheckSaveUsed = true;
            style.TextStyles[0].TempNo = 0;
            for (int i = 1; i < style.TextStyles.Count; i++)
            {
                style.TextStyles[i].CheckSaveUsed = false;
                style.TextStyles[i].TempNo = HCStyle.Null;
            }

            for (int i = 0; i < style.ParaStyles.Count; i++)
            {
                style.ParaStyles[i].CheckSaveUsed = false;
                style.ParaStyles[i].TempNo = HCStyle.Null;
            }

            for (int i = 0; i < sections.Count; i++)
                sections[i].MarkStyleUsed(true, parts);

            int vUnCount = 0;
            for (int i = 1; i < style.TextStyles.Count; i++)
            {
                if (style.TextStyles[i].CheckSaveUsed)
                    style.TextStyles[i].TempNo = i - vUnCount;
                else
                    vUnCount++;
            }

            vUnCount = 0;
            for (int i = 0; i < style.ParaStyles.Count; i++)
            {
                if (style.ParaStyles[i].CheckSaveUsed)
                    style.ParaStyles[i].TempNo = i - vUnCount;
                else
                    vUnCount++;
            }

            HCCustomData vData = null;
            for (int i = 0; i < sections.Count; i++)
            {
                sections[i].MarkStyleUsed(false, parts);

                vData = sections[i].ActiveData.GetTopLevelData();
                if (vData.CurStyleNo > HCStyle.Null)
                    vData.CurStyleNo = style.TextStyles[vData.CurStyleNo].TempNo;

                vData.CurParaNo = style.ParaStyles[vData.CurParaNo].TempNo;
            }

            for (int i = style.TextStyles.Count - 1; i >= 1; i--)
            {
                if (!style.TextStyles[i].CheckSaveUsed)
                    style.TextStyles.RemoveAt(i);
            }

            for (int i = style.ParaStyles.Count - 1; i >= 0; i--)
            {
                if (!style.ParaStyles[i].CheckSaveUsed)
                    style.ParaStyles.RemoveAt(i);
            }
        }

        public virtual void Clear()
        {
            FStyle.Initialize();
            FSections.RemoveRange(1, FSections.Count - 1);
            FActiveSectionIndex = 0;
            FSections[0].Clear();
        }


        public int GetPageCount()
        {
            int Result = 0;
            for (int i = 0; i <= FSections.Count - 1; i++)
                Result = Result + FSections[i].PageCount;

            return Result;
        }

        public int GetSectionPageIndexByPageIndex(int pageIndex, ref int sectionPageIndex)
        {
            int Result = -1, vPageCount = 0;

            for (int i = 0; i <= FSections.Count - 1; i++)
            {
                if (vPageCount + FSections[i].PageCount > pageIndex)
                {
                    Result = i;  // 找到节序号
                    sectionPageIndex = pageIndex - vPageCount;
                    break;
                }
                else
                    vPageCount = vPageCount + FSections[i].PageCount;
            }

            return Result;
        }

        public void SaveToFile(string fileName, bool quick = false)
        {
            FileStream vStream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            try
            {
                HashSet<SectionArea> vParts = new HashSet<SectionArea> { SectionArea.saHeader, SectionArea.saPage, SectionArea.saFooter };
                SaveToStream(vStream, quick, vParts);
            }
            finally
            {
                vStream.Close();
                vStream.Dispose();
            }
        }

        public bool LoadFromFile(string fileName)
        {
            bool vResult = false;
            FileStream vStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            try
            {
                vResult = LoadFromStream(vStream);
            }
            finally
            {
                vStream.Dispose();
            }

            return vResult;
        }

        public void LoadFromDocumentFile(string fileName, string ext)
        {
            this.Clear();
        }

        public void SaveToDocumentFile(string fileName, string ext)
        {

        }

        public void LoadFromDocumentStream(Stream stream, string ext)
        {
            this.Clear();
        }

        public void SaveToPDF(string fileName)
        {
            using (FileStream vStream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                SaveToPDFStream(vStream);
            }
        }

        [DllImport("HCExpPDF.dll", EntryPoint = "SetServiceCode")]
        public static extern object SetServiceCode(object obj);

        [DllImport("HCExpPDF.dll", EntryPoint = "SaveToPDFStream", CallingConvention = CallingConvention.StdCall)]
        public static extern void SaveToPDFStream_DLL(ref object inObj, out object outObj);
        public void SaveToPDFStream(Stream stream)
        {
            using (MemoryStream vFileStream = new MemoryStream())
            {
                HashSet<SectionArea> vParts = new HashSet<SectionArea> { SectionArea.saHeader, SectionArea.saPage, SectionArea.saFooter };
                this.SaveToStream(vFileStream, true, vParts);
                vFileStream.Position = 0;
                byte[] bytes = new byte[vFileStream.Length];
                vFileStream.Read(bytes, 0, bytes.Length);
                object vInObj = (object)bytes;

                object vOutObj = null;
                SaveToPDFStream_DLL(ref vInObj, out vOutObj);
                if (vOutObj != null)
                {
                    byte[] vOutBytes = vOutObj as byte[];
                    stream.Write(vOutBytes, 0, vOutBytes.Length);
                }
            }
        }

        public string SaveToText()
        {
            FStyle.States.Include(HCState.hosSaving);
            try
            {
                string vResult = FSections[0].SaveToText();
                for (int i = 1; i <= FSections.Count - 1; i++)
                    vResult = vResult + HC.sLineBreak + FSections[i].SaveToText();

                return vResult;
            }
            finally
            {
                FStyle.States.Exclude(HCState.hosSaving);
            }
        }

        public bool LoadFromText(string text)
        {
            Clear();
            FStyle.Initialize();

            if (text != "")
            {
                FStyle.States.Include(HCState.hosLoading);
                try
                {
                    return ActiveSection.InsertText(text);
                }
                finally
                {
                    FStyle.States.Exclude(HCState.hosLoading);
                }
            }
            else
                return false;
        }

        public void SaveToTextFile(string fileName, System.Text.Encoding encoding)
        {
            using (FileStream vStream = new FileStream(fileName, FileMode.Create))
            {
                SaveToTextStream(vStream, encoding);
            }
        }

        public bool LoadFromTextFile(string fileName, System.Text.Encoding encoding)
        {
            bool vResult = false;
            using (FileStream vStream = new FileStream(fileName, FileMode.Open))
            {
                vStream.Position = 0;
                vResult = LoadFromTextStream(vStream, encoding);
            }

            return vResult;
        }

        public void SaveToTextStream(Stream stream, System.Text.Encoding encoding)
        {
            string vText = SaveToText();
            byte[] vBuffer = encoding.GetBytes(vText);
            byte[] vPreamble = encoding.GetPreamble();
            if (vPreamble.Length > 0)
                stream.Write(vPreamble, 0, vPreamble.Length);

            stream.Write(vBuffer, 0, vBuffer.Length);
        }

        public bool LoadFromTextStream(Stream stream, System.Text.Encoding encoding)
        {
            long vSize = stream.Length - stream.Position;
            byte[] vBuffer = new byte[vSize];
            stream.Read(vBuffer, 0, (int)vSize);
            string vS = encoding.GetString(vBuffer);
            return LoadFromText(vS);
        }

        public virtual void SaveToStream(Stream aStream, bool aQuick = false, HashSet<SectionArea> aAreas = null)
        {
            FStyle.States.Include(HCState.hosSaving);
            try
            {
                HC._SaveFileFormatAndVersion(aStream);  // 文件格式和版本
                aStream.WriteByte(HC.HC_STREAM_VIEW);
                DoSaveStreamBefor(aStream);

                HashSet<SectionArea> vArea = aAreas;
                if (vArea == null)
                {
                    vArea = new HashSet<SectionArea>();
                    vArea.Add(SectionArea.saHeader);
                    vArea.Add(SectionArea.saFooter);
                    vArea.Add(SectionArea.saPage);
                }

                if (!aQuick)
                    DeleteUnUsedStyle(FStyle, FSections, vArea);

                FStyle.SaveToStream(aStream);
                DoSaveMutMargin(aStream);

                byte vByte = 0;
                aStream.WriteByte(vByte);
                aStream.WriteByte(vByte);
                uint vSize = 0;
                byte[] vBuffer = BitConverter.GetBytes(vSize);
                aStream.Write(vBuffer, 0, vBuffer.Length);

                // 节数量
                vByte = (byte)FSections.Count;
                aStream.WriteByte(vByte);
                // 各节数据
                for (int i = 0; i <= FSections.Count - 1; i++)
                    FSections[i].SaveToStream(aStream, vArea);

                DoSaveStreamAfter(aStream);
            }
            finally
            {
                FStyle.States.Exclude(HCState.hosSaving);
            }
        }

        public virtual bool LoadFromStream(Stream stream)
        {
            FStyle.States.Include(HCState.hosLoading);
            try
            {
                stream.Position = 0;
                LoadSectionProcHandler vEvent = delegate (ushort aFileVersion)
                {
                    byte vByte = 0;
                    vByte = (byte)stream.ReadByte();  // 节数量
                    // 各节数据
                    FSections[0].LoadFromStream(stream, FStyle, aFileVersion);
                    for (int i = 1; i <= vByte - 1; i++)
                    {
                        HCSection vSection = NewDefaultSection();
                        vSection.LoadFromStream(stream, FStyle, aFileVersion);
                        FSections.Add(vSection);
                    }
                };

                DoLoadFromStream(stream, FStyle, vEvent);
            }
            finally
            {
                FStyle.States.Exclude(HCState.hosLoading);
            }

            return true;
        }

        public bool InsertLiteStream(Stream stream)
        {
            bool vResult = false;
            DataLoadLiteStream(stream, delegate (ushort fileVersion, HCStyle style)
            {
                vResult = ActiveSection.InsertStream(stream, style, fileVersion);
            });

            return vResult;
        }

        public void SaveToXml(string fileName, System.Text.Encoding encoding)
        {
            FileStream vStream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            try
            {
                SaveToXmlStream(vStream, encoding);
            }
            finally
            {
                vStream.Close();
                vStream.Dispose();
            }
        }

        public void SaveToXmlStream(Stream stream, System.Text.Encoding encoding)
        {
            FStyle.States.Include(HCState.hosSaving);
            try
            {
                HashSet<SectionArea> vParts = new HashSet<SectionArea> { SectionArea.saHeader, SectionArea.saPage, SectionArea.saFooter };
                DeleteUnUsedStyle(FStyle, FSections, vParts);

                XmlDocument vXml = new XmlDocument();
                vXml.PreserveWhitespace = true;
                //vXml. = "1.0";
                //vXml.DocumentElement
                XmlElement vElement = vXml.CreateElement("HCView");
                vElement.SetAttribute("EXT", HC.HC_EXT);
                vElement.SetAttribute("ver", HC.HC_FileVersion);
                vElement.SetAttribute("lang", HC.HC_PROGRAMLANGUAGE.ToString());
                vXml.AppendChild(vElement);

                XmlElement vNode = vXml.CreateElement("style");
                FStyle.ToXml(vNode);  // 样式表
                vElement.AppendChild(vNode);

                vNode = vXml.CreateElement("sections");
                vNode.SetAttribute("count", FSections.Count.ToString());  // 节数量
                vElement.AppendChild(vNode);

                for (int i = 0; i <= FSections.Count - 1; i++)  // 各节数据
                {
                    XmlElement vSectionNode = vXml.CreateElement("sc");
                    FSections[i].ToXml(vSectionNode);
                    vNode.AppendChild(vSectionNode);
                }

                vXml.Save(stream);
            }
            finally
            {
                FStyle.States.Exclude(HCState.hosSaving);
            }
        }

        public bool LoadFromXml(string aFileName)
        {
            bool vResult = false;
            FileStream vStream = new FileStream(aFileName, FileMode.Open, FileAccess.Read);
            try
            {
                vStream.Position = 0;
                vResult = LoadFromXmlStream(vStream);
            }
            finally
            {
                vStream.Dispose();
            }

            return vResult;
        }

        public bool LoadFromXmlStream(Stream stream)
        {
            XmlDocument vXml = new XmlDocument();
            vXml.PreserveWhitespace = true;
            vXml.Load(stream);
            if (vXml.DocumentElement.Name == "HCView")
            {
                if (vXml.DocumentElement.Attributes["EXT"].Value != HC.HC_EXT)
                    return false;

                string vVersion = vXml.DocumentElement.Attributes["ver"].Value;
                byte vLang = byte.Parse(vXml.DocumentElement.Attributes["lang"].Value);

                FStyle.States.Include(HCState.hosLoading);
                try
                {
                    for (int i = 0; i <= vXml.DocumentElement.ChildNodes.Count - 1; i++)
                    {
                        XmlElement vNode = vXml.DocumentElement.ChildNodes[i] as XmlElement;
                        if (vNode.Name == "style")
                            FStyle.ParseXml(vNode);
                        else
                        if (vNode.Name == "sections")
                        {
                            FSections[0].ParseXml(vNode.ChildNodes[0] as XmlElement);
                            for (int j = 1; j < vNode.ChildNodes.Count - 1; j++)
                            {
                                HCSection vSection = NewDefaultSection();
                                vSection.ParseXml(vNode.ChildNodes[j] as XmlElement);
                                FSections.Add(vSection);
                            }
                        }
                    }
                }
                finally
                {
                    FStyle.States.Exclude(HCState.hosLoading);
                }

                return true;
            }

            return false;
        }

        public void SaveToHtml(string fileName, bool separateSrc = false)
        {
            HashSet<SectionArea> vParts = new HashSet<SectionArea> { SectionArea.saHeader, SectionArea.saPage, SectionArea.saFooter };
            DeleteUnUsedStyle(FStyle, FSections, vParts);

            FStyle.GetHtmlFileTempName(true);

            string vPath = "";
            if (separateSrc)
                vPath = Path.GetDirectoryName(fileName);

            StringBuilder vHtmlTexts = new StringBuilder();
            vHtmlTexts.Append("<!DOCTYPE HTML>");
            vHtmlTexts.Append("<html>");
            vHtmlTexts.Append("<head>");
            vHtmlTexts.Append("<title>");
            vHtmlTexts.Append("</title>");

            vHtmlTexts.Append(FStyle.ToCSS());

            vHtmlTexts.Append("</head>");

            vHtmlTexts.Append("<body>");
            for (int i = 0; i <= FSections.Count - 1; i++)
                vHtmlTexts.Append(FSections[i].ToHtml(vPath));

            vHtmlTexts.Append("</body>");
            vHtmlTexts.Append("</html>");

            System.IO.File.WriteAllText(fileName, vHtmlTexts.ToString());
        }

        public void SaveToImage(string path, string prefix, string imageType, bool onePaper = true)
        {
            HCCanvas vBmpCanvas = new HCCanvas();
            SectionPaintInfo vPaintInfo = new SectionPaintInfo();
            try
            {
                vPaintInfo.ScaleX = 1;
                vPaintInfo.ScaleY = 1;
                vPaintInfo.Zoom = 1;
                vPaintInfo.Print = true;
                vPaintInfo.DPI = HCUnitConversion.PixelsPerInchX;
                vPaintInfo.ViewModel = HCViewModel.hvmFilm;

                int vWidth = 0, vHeight = 0;
                if (onePaper)
                {
                    for (int i = 0; i < FSections.Count; i++)
                    {
                        vHeight = vHeight + FSections[i].PaperHeightPix * FSections[i].PageCount;
                        if (vWidth < FSections[i].PaperWidthPix)
                            vWidth = FSections[i].PaperWidthPix;
                    }

                    vPaintInfo.WindowWidth = vWidth;
                    vPaintInfo.WindowHeight = vHeight;

                    using (Bitmap vBmp = new Bitmap(vWidth, vHeight))
                    {
                        vBmpCanvas.Graphics = Graphics.FromImage(vBmp);

                        int vSectionIndex = 0, vSectionPageIndex = 0, vTop = 0;
                        for (int i = 0; i < this.PageCount; i++)
                        {
                            vSectionIndex = GetSectionPageIndexByPageIndex(i, ref vSectionPageIndex);
                            //vWidth = FSections[vSectionIndex].PaperWidthPix;
                            vHeight = FSections[vSectionIndex].PaperHeightPix;

                            vBmpCanvas.Brush.Color = Color.White;
                            vBmpCanvas.FillRect(new RECT(0, vTop, vWidth, vTop + vHeight));

                            ScaleInfo vScaleInfo = vPaintInfo.ScaleCanvas(vBmpCanvas);
                            try
                            {
                                FSections[vSectionIndex].PaintPaper(vSectionPageIndex, 0, vTop, vBmpCanvas, vPaintInfo);
                                vTop = vTop + vHeight;
                            }
                            finally
                            {
                                vPaintInfo.RestoreCanvasScale(vBmpCanvas, vScaleInfo);
                            }
                        }

                        vBmpCanvas.Dispose();
                        if (imageType == "BMP")
                            vBmp.Save(path + prefix + ".bmp", System.Drawing.Imaging.ImageFormat.Bmp);
                        else
                        if (imageType == "JPG")
                            vBmp.Save(path + prefix + ".jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                        else
                            vBmp.Save(path + prefix + ".png", System.Drawing.Imaging.ImageFormat.Png);
                    }
                }
                else
                {
                    int vSectionIndex = 0, vSectionPageIndex = 0;
                    for (int i = 0; i < this.PageCount; i++)
                    {
                        vSectionIndex = GetSectionPageIndexByPageIndex(i, ref vSectionPageIndex);

                        using (Bitmap vBmp = new Bitmap(FSections[vSectionIndex].PaperWidthPix, FSections[vSectionIndex].PaperHeightPix))
                        {
                            vBmpCanvas.Graphics = Graphics.FromImage(vBmp);
                            vBmpCanvas.Brush.Color = Color.White;
                            vBmpCanvas.FillRect(new RECT(0, 0, vBmp.Width, vBmp.Height));

                            vPaintInfo.WindowWidth = vBmp.Width;
                            vPaintInfo.WindowHeight = vBmp.Height;
                            ScaleInfo vScaleInfo = vPaintInfo.ScaleCanvas(vBmpCanvas);
                            try
                            {
                                vBmpCanvas.Brush.Color = Color.White;
                                vBmpCanvas.FillRect(new RECT(0, 0, vBmp.Width, vBmp.Height));
                                FSections[vSectionIndex].PaintPaper(vSectionPageIndex, 0, 0, vBmpCanvas, vPaintInfo);
                            }
                            finally
                            {
                                vPaintInfo.RestoreCanvasScale(vBmpCanvas, vScaleInfo);
                            }

                            vBmpCanvas.Dispose();
                            if (imageType == "BMP")
                                vBmp.Save(path + prefix + (i + 1).ToString() + ".bmp", System.Drawing.Imaging.ImageFormat.Bmp);
                            else
                            if (imageType == "JPG")
                                vBmp.Save(path + prefix + (i + 1).ToString() + ".jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                            else
                                vBmp.Save(path + prefix + (i + 1).ToString() + ".png", System.Drawing.Imaging.ImageFormat.Png);
                        }
                    }
                }
            }
            finally
            {
                vPaintInfo.Dispose();
            }
        }

        public HCSection ActiveSection
        {
            get { return GetActiveSection(); }
        }

        public int PageCount
        {
            get { return GetPageCount(); }
        }

        public List<HCSection> Sections
        {
            get { return FSections; }
        }


        /// <summary> 当前文档样式表 </summary>
        public HCStyle Style
        {
            get { return FStyle; }
        }
    }
}
