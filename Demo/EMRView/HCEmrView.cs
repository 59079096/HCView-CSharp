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
using System.Windows.Forms;
using HC.View;
using System.IO;
using HC.Win32;
using System.Drawing;

namespace EMRView
{
    public delegate void SyncDeItemEventHandle(object sender, HCCustomData aData, DeItem aItem);
    public class HCEmrView : HCView
    {
        private bool FLoading;
        private bool FDesignMode;
        private bool FTrace;  // 是否处于留痕迹状态
        private int FTraceCount;  // 当前文档痕迹数量
        private Color FDeDoneColor, FDeUnDoneColor;
        private EventHandler FOnCanNotEdit;
        private SyncDeItemEventHandle FOnSyncDeItem;

        private void DoSyncDeItem(object sender, HCCustomData aData, DeItem aItem)
        {
            if (FOnSyncDeItem != null)
                FOnSyncDeItem(sender, aData, aItem);
        }

        private void DoDeItemPaintBKG(object sender, HCCanvas aCanvas, RECT aDrawRect, PaintInfo aPaintInfo)
        {
            if (!aPaintInfo.Print)
            {
                DeItem vDeItem = sender as DeItem;
                if (vDeItem.IsElement)
                {
                    if (vDeItem.MouseIn || vDeItem.Active)
                    {
                        if (vDeItem.IsSelectPart || vDeItem.IsSelectComplate)
                        {

                        }
                        else
                        {
                            if (vDeItem[DeProp.Name] != vDeItem.Text)
                                aCanvas.Brush.Color = FDeDoneColor;
                            else  // 没填写过
                                aCanvas.Brush.Color = FDeUnDoneColor;
                            
                            aCanvas.FillRect(aDrawRect);
                        }
                    }
                }
                else  // 不是数据元
                if (FDesignMode && vDeItem.EditProtect)
                {
                    aCanvas.Brush.Color = HC.View.HC.clBtnFace;
                    aCanvas.FillRect(aDrawRect);
                }
            }
        }

        private void InsertEmrTraceItem(string aText)
        {
            DeItem vEmrTraceItem = new DeItem(aText);

            if (this.CurStyleNo < HCStyle.Null)
                vEmrTraceItem.StyleNo = 0;
            else
                vEmrTraceItem.StyleNo = this.CurStyleNo;

            vEmrTraceItem.ParaNo = this.CurParaNo;
            vEmrTraceItem.StyleEx = StyleExtra.cseAdd;

            this.InsertItem(vEmrTraceItem);
        }

        private bool CanNotEdit()
        {
            bool Result = (!this.ActiveSection.ActiveData.CanEdit()) 
                || (!(this.ActiveSectionTopLevelData() as HCRichData).CanEdit());

            if (Result && (FOnCanNotEdit != null))
                FOnCanNotEdit(this, null);

            return Result;
        }

        protected override void DoSectionCreateItem(object sender, EventArgs e)
        {
            if ((!FLoading) && FTrace)
                (sender as DeItem).StyleEx = StyleExtra.cseAdd;

            base.DoSectionCreateItem(sender, e);
        }

        protected override HCCustomItem DoSectionCreateStyleItem(HCCustomData aData, int aStyleNo)
        {
            return HCEmrView.CreateEmrStyleItem(aData, aStyleNo);
        }

        protected override void DoSectionInsertItem(object sender, HCCustomData aData, HCCustomItem aItem)
        {
            if (aItem is DeItem)
            {
                DeItem vDeItem = aItem as DeItem;
                vDeItem.OnPaintBKG = DoDeItemPaintBKG;

                if (vDeItem.StyleEx != StyleExtra.cseNone)
                {
                    FTraceCount++;

                    if (!this.AnnotatePre.Visible)
                        this.AnnotatePre.Visible = true;
                }

                DoSyncDeItem(sender, aData, vDeItem);
            }

            base.DoSectionInsertItem(sender, aData, aItem);
        }

        protected override void DoSectionRemoveItem(object sender, HCCustomData aData, HCCustomItem aItem)
        {
            if (aItem is DeItem)
            {
                DeItem vDeItem = aItem as DeItem;
                vDeItem.OnPaintBKG = DoDeItemPaintBKG;

                if (vDeItem.StyleEx != StyleExtra.cseNone)
                {
                    FTraceCount--;

                    if ((FTraceCount == 0) && (this.AnnotatePre.Visible) && (this.AnnotatePre.Count == 0))
                        this.AnnotatePre.Visible = false;
                }
            }

            base.DoSectionRemoveItem(sender, aData, aItem);
        }

        protected override bool DoSectionCanEdit(Object sender)
        {
            HCViewData vViewData = sender as HCViewData;
            if ((vViewData.ActiveDomain != null) && (vViewData.ActiveDomain.BeginNo >= 0))
                return !((vViewData.Items[vViewData.ActiveDomain.BeginNo] as DeGroup).ReadOnly);
            else
                return true;
        }

        /// <summary> 鼠标按下 </summary>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (FTrace)
            {
                if (HC.View.HC.IsKeyDownEdit(e.KeyValue))
                {
                    string vText = "";
                    string vCurTrace = "";
                    int vStyleNo = HCStyle.Null;
                    int vParaNo = HCStyle.Null;
                    StyleExtra vCurStyleEx = StyleExtra.cseNone;

                    HCRichData vData = this.ActiveSectionTopLevelData() as HCRichData;
                    if (vData.SelectExists())
                    {
                        this.DisSelect();
                        return;
                    }

                    if (vData.SelectInfo.StartItemNo < 0)
                        return;

                    if (vData.Items[vData.SelectInfo.StartItemNo].StyleNo < HCStyle.Null)
                    {
                        base.OnKeyDown(e);
                        return;
                    }

                    // 取光标处的文本
                    if (e.KeyCode == Keys.Back)  // 回删
                    {
                        if ((vData.SelectInfo.StartItemNo == 0)
                            && (vData.SelectInfo.StartItemOffset == 0))  // 第一个最前面则不处理
                            return;
                        else  // 不是第一个最前面
                        if (vData.SelectInfo.StartItemOffset == 0)  // 最前面，移动到前一个最后面处理
                        {
                            if (vData.Items[vData.SelectInfo.StartItemNo].Text != "")  // 当前行不是空行
                            {
                                vData.SelectInfo.StartItemNo = vData.SelectInfo.StartItemNo - 1;
                                vData.SelectInfo.StartItemOffset = vData.Items[vData.SelectInfo.StartItemNo].Length;
                                this.OnKeyDown(e);
                            }
                            else  // 空行不留痕直接默认处理
                                base.OnKeyDown(e);

                            return;
                        }
                        else  // 不是第一个Item，也不是在Item最前面
                        if (vData.Items[vData.SelectInfo.StartItemNo] is DeItem)  // 文本
                        {
                            DeItem vDeItem = vData.Items[vData.SelectInfo.StartItemNo] as DeItem;
                            vText = vDeItem.SubString(vData.SelectInfo.StartItemOffset, 1);
                            vStyleNo = vDeItem.StyleNo;
                            vParaNo = vDeItem.ParaNo;
                            vCurStyleEx = vDeItem.StyleEx;
                            vCurTrace = vDeItem[DeProp.Trace];
                        }
                    }
                    else
                    if (e.KeyCode == Keys.Delete)  // 后删
                    {
                        if ((vData.SelectInfo.StartItemNo == vData.Items.Count - 1)
                            && (vData.SelectInfo.StartItemOffset == vData.Items[vData.Items.Count - 1].Length))
                            return;  // 最后一个最后面则不处理
                        else
                        if (vData.SelectInfo.StartItemOffset == vData.Items[vData.SelectInfo.StartItemNo].Length)  // 最后面，移动到后一个最前面处理
                        {
                            vData.SelectInfo.StartItemNo = vData.SelectInfo.StartItemNo + 1;
                            vData.SelectInfo.StartItemOffset = 0;
                            this.OnKeyDown(e);

                            return;
                        }
                        else  // 不是最后一个Item，也不是在Item最后面
                        if (vData.Items[vData.SelectInfo.StartItemNo] is DeItem)  // 文本
                        {
                            DeItem vDeItem = vData.Items[vData.SelectInfo.StartItemNo] as DeItem;
                            vText = vDeItem.SubString(vData.SelectInfo.StartItemOffset + 1, 1);
                            vStyleNo = vDeItem.StyleNo;
                            vParaNo = vDeItem.ParaNo;
                            vCurStyleEx = vDeItem.StyleEx;
                            vCurTrace = vDeItem[DeProp.Trace];
                        }
                    }

                    // 删除掉的内容以痕迹的形式插入
                    this.BeginUpdate();
                    try
                    {
                        base.OnKeyDown(e);

                        if (FTrace && (vText != "")) // 有删除的内容
                        {
                            if ((vCurStyleEx == StyleExtra.cseAdd) && (vCurTrace == ""))  // 新添加未生效痕迹可以直接删除
                                return;

                            // 创建删除字符对应的Item
                            DeItem vDeItem = new DeItem();
                            vDeItem.Text = vText;
                            vDeItem.StyleNo = vStyleNo;
                            vDeItem.ParaNo = vParaNo;

                            if ((vCurStyleEx == StyleExtra.cseDel) && (vCurTrace == "")) // 原来是删除未生效痕迹
                                vDeItem.StyleEx = StyleExtra.cseNone;  // 取消删除痕迹
                            else  // 生成删除痕迹
                                vDeItem.StyleEx = StyleExtra.cseDel;

                            // 插入删除痕迹Item
                            HCCustomItem vCurItem = vData.Items[vData.SelectInfo.StartItemNo];
                            if (vData.SelectInfo.StartItemOffset == 0)  // 在Item最前面
                            {
                                if (vDeItem.CanConcatItems(vCurItem))  // 可以合并
                                {
                                    vCurItem.Text = vDeItem.Text + vCurItem.Text;

                                    if (e.KeyCode == Keys.Delete)  // 后删
                                        vData.SelectInfo.StartItemOffset = vData.SelectInfo.StartItemOffset + 1;

                                    this.ActiveSection.ReFormatActiveItem();
                                }
                                else  // 不能合并
                                {
                                    vDeItem.ParaFirst = vCurItem.ParaFirst;
                                    vCurItem.ParaFirst = false;
                                    this.InsertItem(vDeItem);
                                    if (e.KeyCode == Keys.Back)  // 回删
                                        vData.SelectInfo.StartItemOffset = vData.SelectInfo.StartItemOffset - 1;
                                }
                            }
                            else
                            if (vData.SelectInfo.StartItemOffset == vCurItem.Length)  // 在Item最后面
                            {
                                if (vCurItem.CanConcatItems(vDeItem))  // 可以合并
                                {
                                    vCurItem.Text = vCurItem.Text + vDeItem.Text;

                                    if (e.KeyCode == Keys.Delete)  // 后删
                                        vData.SelectInfo.StartItemOffset = vData.SelectInfo.StartItemOffset + 1;

                                    this.ActiveSection.ReFormatActiveItem();
                                }
                                else  // 不可以合并
                                {
                                    this.InsertItem(vDeItem);
                                    if (e.KeyCode == Keys.Back)  // 回删
                                        vData.SelectInfo.StartItemOffset = vData.SelectInfo.StartItemOffset - 1;
                                }
                            }
                            else  // 在Item中间
                            {
                                this.InsertItem(vDeItem);
                                if (e.KeyCode == Keys.Back)  // 回删
                                    vData.SelectInfo.StartItemOffset = vData.SelectInfo.StartItemOffset - 1;
                            }
                        }
                    }
                    finally
                    {
                        this.EndUpdate();
                    }
                }
                else
                    base.OnKeyDown(e);
            }
            else
                base.OnKeyDown(e);
        }

        /// <summary> 鼠标按压 </summary>
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if (HC.View.HC.IsKeyPressWant(e))
            {
                if (CanNotEdit())
                    return;

                if (FTrace)
                {
                    HCCustomData vData = this.ActiveSectionTopLevelData();
                    if (vData.SelectInfo.StartItemNo < 0)
                        return;

                    if (vData.SelectExists())
                        this.DisSelect();
                    else
                        InsertEmrTraceItem(e.KeyChar.ToString());

                    return;
                }

                base.OnKeyPress(e);
            }
        }

        /// <summary> 插入文本 </summary>
        /// <param name="AText">要插入的字符串(支持带#13#10的回车换行)</param>
        /// <returns>True：插入成功</returns>
        protected override bool DoInsertText(string aText)
        {
            if (CanNotEdit())
                return false;

            if (FTrace)
            {
                InsertEmrTraceItem(aText);
                return true;
            }
            else
                return base.DoInsertText(aText);
        }

        /// <summary> 文档某节的Item绘制完成 </summary>
        /// <param name="AData">当前绘制的Data</param>
        /// <param name="ADrawItemIndex">Item对应的DrawItem序号</param>
        /// <param name="ADrawRect">Item对应的绘制区域</param>
        /// <param name="ADataDrawLeft">Data绘制时的Left</param>
        /// <param name="ADataDrawBottom">Data绘制时的Bottom</param>
        /// <param name="ADataScreenTop">绘制时呈现Data的Top位置</param>
        /// <param name="ADataScreenBottom">绘制时呈现Data的Bottom位置</param>
        /// <param name="ACanvas">画布</param>
        /// <param name="APaintInfo">绘制时的其它信息</param>
        protected override void DoSectionDrawItemPaintAfter(Object sender, HCCustomData aData, int aDrawItemNo, RECT aDrawRect, 
            int aDataDrawLeft, int aDataDrawBottom, int aDataScreenTop, int aDataScreenBottom, HCCanvas aCanvas, PaintInfo aPaintInfo)
        {
            if (FTraceCount > 0)  // 显示批注
            {
                HCCustomItem vItem = aData.Items[aData.DrawItems[aDrawItemNo].ItemNo];
                if (vItem.StyleNo > HCStyle.Null)
                {
                    DeItem vDeItem = vItem as DeItem;
                    if (vDeItem.StyleEx != StyleExtra.cseNone)  // 添加批注
                    {
                        HCDrawAnnotateDynamic vDrawAnnotate = new HCDrawAnnotateDynamic();
                        vDrawAnnotate.DrawRect = aDrawRect;
                        vDrawAnnotate.Title = vDeItem.GetHint();
                        vDrawAnnotate.Text = aData.GetDrawItemText(aDrawItemNo);

                        this.AnnotatePre.AddDrawAnnotate(vDrawAnnotate);
                    }
                }
            }

            base.DoSectionDrawItemPaintAfter(sender, aData, aDrawItemNo, aDrawRect, aDataDrawLeft, aDataDrawBottom,
                aDataScreenTop, aDataScreenBottom, aCanvas, aPaintInfo);
        }

        protected override void WndProc(ref Message Message)
        {
            base.WndProc(ref Message);
        }

        protected override void Create()
        {
            base.Create();
            FLoading = false;
            FTrace = false;
            FTraceCount = 0;
            FDesignMode = false;
            HCTextItem.HCDefaultTextItemClass = typeof(DeItem);
            HCDomainItem.HCDefaultDomainItemClass = typeof(DeGroup);
        }

        public HCEmrView() : base()
        {
            this.Width = 100;
            this.Height = 100;
            FDeDoneColor = HC.View.HC.clBtnFace;
            FDeUnDoneColor = Color.FromArgb(0xFF, 0xDD, 0x80);
        }

        ~HCEmrView()
        {

        }

        public static HCCustomItem CreateEmrStyleItem(HCCustomData aData, int aStyleNo)
        {
            switch (aStyleNo)
            {
                case HCStyle.Table:
                    return new DeTable(aData, 1, 1, 1);

                case HCStyle.CheckBox:
                    return new DeCheckBox(aData, "勾选框", false);

                case HCStyle.Edit:
                    return new DeEdit(aData, "");

                case HCStyle.Combobox:
                    return new DeCombobox(aData, "");

                case HCStyle.DateTimePicker:
                    return new DeDateTimePicker(aData, DateTime.Now);

                case HCStyle.RadioGroup:
                    return new DeRadioGroup(aData);

                case EMR.EMRSTYLE_YUEJING:
                    return new EmrYueJingItem(aData, "", "", "", "");

                case EMR.EMRSTYLE_TOOTH:
                    return new EmrToothItem(aData, "", "", "", "");

                case EMR.EMRSTYLE_FANGJIAO:
                    return new EmrFangJiaoItem(aData, "", "", "", "");

                default:
                    return null;
            }
        }

        /// <summary> 文档保存到流 </summary>
        public override void SaveToStream(Stream aStream, bool aQuick = false, HashSet<SectionArea> aAreas = null)
        {
            base.SaveToStream(aStream, aQuick, aAreas);
        }

        /// <summary> 读取文件流 </summary>
        public override void LoadFromStream(Stream aStream)
        {
            FLoading = true;
            try
            {
                base.LoadFromStream(aStream);
            }
            finally
            {
                FLoading = false;
            }
        }

        /// <summary> 遍历Item </summary>
        /// <param name="ATraverse">遍历时信息</param>
        public void TraverseItem(HCItemTraverse aTraverse)
        {
            if (aTraverse.Areas.Count == 0)
                return;

            for (int i = 0; i <= this.Sections.Count - 1; i++)
            {
                if (!aTraverse.Stop)
                {
                    if (aTraverse.Areas.Contains(SectionArea.saHeader))
                        this.Sections[i].Header.TraverseItem(aTraverse);

                    if ((!aTraverse.Stop) && (aTraverse.Areas.Contains(SectionArea.saPage)))
                        this.Sections[i].Page.TraverseItem(aTraverse);

                    if ((!aTraverse.Stop) && (aTraverse.Areas.Contains(SectionArea.saFooter)))
                        this.Sections[i].Footer.TraverseItem(aTraverse);
                }
            }
        }

        /// <summary> 插入数据组 </summary>
        /// <param name="ADeGroup">数据组信息</param>
        /// <returns>True：插入成功</returns>
        public bool InsertDeGroup(DeGroup aDeGroup)
        {
            return InsertDomain(aDeGroup);
        }

        /// <summary> 插入数据元 </summary>
        /// <param name="ADeItem">数据元信息</param>
        /// <returns>True：插入成功</returns>
        public bool InsertDeItem(DeItem aDeItem)
        {
            return this.InsertItem(aDeItem);
        }

        /// <summary> 新建数据元 </summary>
        /// <param name="AText">数据元文本</param>
        /// <returns>新建好的数据元</returns>
        public DeItem NewDeItem(string aText)
        {
            DeItem Result = new DeItem();
            Result.Text = aText;

            if (this.CurStyleNo > HCStyle.Null)
                Result.StyleNo = this.CurStyleNo;
            else
                Result.StyleNo = 0;

            Result.ParaNo = this.CurParaNo;

            return Result;
        }

        /// <summary> 直接设置当前数据元的值为扩展内容 </summary>
        public void SetActiveItemExtra(Stream aStream)
        {
            string vFileFormat = "";
            ushort vFileVersion = 0;
            byte vLang = 0;
            HC.View.HC._LoadFileFormatAndVersion(aStream, ref vFileFormat, ref vFileVersion, ref vLang);
            HCStyle vStyle = new HCStyle();
            try
            {
                vStyle.LoadFromStream(aStream, vFileVersion);
                this.BeginUpdate();
                try
                {
                    this.UndoGroupBegin();
                    try
                    {
                        HCRichData vTopData = this.ActiveSectionTopLevelData() as HCRichData;
                        this.DeleteActiveDataItems(vTopData.SelectInfo.StartItemNo);
                        ActiveSection.InsertStream(aStream, vStyle, vFileVersion);
                    }
                    finally
                    {
                        this.UndoGroupEnd();
                    }
                }
                finally
                {
                    this.EndUpdate();
                }
            }
            finally
            {
                vStyle.Dispose();
            }
        }

        /// <summary> 获取指定数据组中的文本内容 </summary>
        /// <param name="AData">指定从哪个Data里获取</param>
        /// <param name="ADeGroupStartNo">指定数据组的起始ItemNo</param>
        /// <param name="ADeGroupEndNo">指定数据组的结束ItemNo</param>
        /// <returns>数据组内容</returns>
        public string GetDataDeGroupText(HCViewData aData, int aDeGroupStartNo, int aDeGroupEndNo)
        {
            string Result = "";
            for (int i = aDeGroupStartNo + 1; i <= aDeGroupEndNo - 1; i++)
                Result = Result + aData.Items[i].Text;

            return Result;
        }

        /// <summary> 从当前数据组起始位置往前找相同Index域内容 </summary>
        /// <param name="AData">指定从哪个Data里获取</param>
        /// <param name="ADeGroupStartNo">指定从哪个位置开始往前找</param>
        /// <returns>相同Index的数据组内容</returns>
        public string GetDataForwardDeGroupText(HCViewData aData, int aDeGroupStartNo)
        {
            string Result = "";

            DeGroup vDeGroup = null;
            int vBeginNo = -1;
            int vEndNo = -1;
            string vDeIndex = (aData.Items[aDeGroupStartNo] as DeGroup)[DeProp.Index];

            for (int i = 0; i <= aDeGroupStartNo - 1; i++)  // 找起始
            {
                if (aData.Items[i] is DeGroup)
                {
                    vDeGroup = aData.Items[i] as DeGroup;
                    if (vDeGroup.MarkType == MarkType.cmtBeg)  // 是域起始
                    {
                        if (vDeGroup[DeProp.Index] == vDeIndex)  // 是目标域起始
                        {
                            vBeginNo = i;
                            break;
                        }
                    }
                }
            }

            if (vBeginNo >= 0)  // 找结束
            {
                for (int i = vBeginNo + 1; i <= aDeGroupStartNo - 1; i++)
                {
                    if (aData.Items[i] is DeGroup)
                    {
                        vDeGroup = aData.Items[i] as DeGroup;
                        if (vDeGroup.MarkType == MarkType.cmtEnd)  // 是域结束
                        {
                            if (vDeGroup[DeProp.Index] == vDeIndex)
                            {
                                vEndNo = i;
                                break;
                            }
                        }
                    }
                }

                if (vEndNo > 0)
                    Result = GetDataDeGroupText(aData, vBeginNo, vEndNo);
            }

            return Result;
        }

        public void SetDeGroupText(HCViewData aData, int aDeGroupNo, string aText)
        {
            int vGroupBeg = -1;
            int vGroupEnd = aData.GetDomainAnother(aDeGroupNo);

            if (vGroupEnd > aDeGroupNo)
                vGroupBeg = aDeGroupNo;
            else
            {
                vGroupBeg = vGroupEnd;
                vGroupEnd = aDeGroupNo;
            }

            // 选中，使用插入时删除当前数据组中的内容
            aData.SetSelectBound(vGroupBeg, HC.View.HC.OffsetAfter, vGroupEnd, HC.View.HC.OffsetBefor);
            aData.InsertText(aText);
        }

        public bool DesignModeEx
        {
            get { return FDesignMode; }
            set { FDesignMode = value; }
        }

        /// <summary> 是否处于留痕状态 </summary>
        public bool Trace
        {
            get { return FTrace; }
            set { FTrace = value; }
        }

        public EventHandler OnCanNotEdit
        {
            get { return FOnCanNotEdit; }
            set { FOnCanNotEdit = value; }
        }

        public SyncDeItemEventHandle OnSyncDeItem
        {
            get { return FOnSyncDeItem; }
            set { FOnSyncDeItem = value; }
        }
    }
}
