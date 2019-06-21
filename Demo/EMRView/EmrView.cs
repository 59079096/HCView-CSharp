using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HC.View;
using System.IO;
using HC.Win32;

namespace EMRView
{
    public class EmrView : HCView
    {
        private bool FLoading;
        private bool FTrace;

        private void DoSectionCreateItem(object sender, EventArgs e)
        {
            if ((!FLoading) && FTrace)
                (sender as DeItem).StyleEx = StyleExtra.cseAdd;
        }

        private void InsertEmrTraceItem(string aText)
        {
            DeItem vEmrTraceItem = new DeItem();
            vEmrTraceItem.Text = aText;
            vEmrTraceItem.StyleNo = CurStyleNo;
            vEmrTraceItem.ParaNo = CurParaNo;
            vEmrTraceItem.StyleEx = StyleExtra.cseAdd;

            this.InsertItem(vEmrTraceItem);
        }

        private HCCustomItem DoCreateStyleItem(HCCustomData aData, int aStyleNo)
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

                default:
                    return null;
            }
        }

        private bool DoCanEdit(Object sender)
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
                            vData.SelectInfo.StartItemNo = vData.SelectInfo.StartItemNo;
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
                                vData.InsertItem(vDeItem);
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
                                vData.InsertItem(vDeItem);
                                if (e.KeyCode == Keys.Back)  // 回删
                                    vData.SelectInfo.StartItemOffset = vData.SelectInfo.StartItemOffset - 1;
                            }
                        }
                        else  // 在Item中间
                        {
                            vData.InsertItem(vDeItem);
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

        /// <summary> 鼠标按压 </summary>
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if (FTrace)
            {
                if (HC.View.HC.IsKeyPressWant(e))
                {
                    HCRichData vData = this.ActiveSectionTopLevelData() as HCRichData;
                    if (vData.SelectInfo.StartItemNo < 0)
                        return;

                    if (vData.SelectExists())
                        this.DisSelect();
                    else
                        InsertEmrTraceItem(e.KeyChar.ToString());

                    return;
                }
            }

            base.OnKeyPress(e);
        }

        /// <summary> 插入文本 </summary>
        /// <param name="AText">要插入的字符串(支持带#13#10的回车换行)</param>
        /// <returns>True：插入成功</returns>
        protected override bool DoInsertText(string aText)
        {
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
            HCTextItem.HCDefaultTextItemClass = typeof(DeItem);
            HCDomainItem.HCDefaultDomainItemClass = typeof(DeGroup);
        }

        public EmrView() : base()
        {
            this.Width = 100;
            this.Height = 100;
            this.OnSectionCreateItem = DoSectionCreateItem;
            this.OnSectionCreateStyleItem = DoCreateStyleItem;
            this.OnSectionCanEdit = DoCanEdit; 
        }

        ~EmrView()
        {

        }

        /// <summary> 文档保存到流 </summary>
        public override void SaveToStream(Stream aStream, bool aQuick, HashSet<SectionArea> aAreas)
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

        /// <summary> 替换指定数据组的内容 </summary>
        /// <param name="AData">指定从哪个Data里获取</param>
        /// <param name="ADeGroupStartNo">被替换的数据组起始位置</param>
        /// <param name="AText">要替换的内容</param>
        public void SetDataDeGroupText(HCViewData aData, int aDeGroupStartNo, string aText)
        {
            int vEndNo = -1;
            int vIgnore = 0;

            for (int i = aDeGroupStartNo + 1; i <= aData.Items.Count - 1; i++)
            {
                if (aData.Items[i] is DeGroup)
                {
                    if ((aData.Items[i] as DeGroup).MarkType == MarkType.cmtEnd)
                    {
                        if (vIgnore == 0)
                        {
                            vEndNo = i;
                            break;
                        }
                        else
                            vIgnore--;
                    }
                    else
                        vIgnore++;
                }
            }

            if (vEndNo >= 0)  // 找到了要引用的内容
            {
                this.BeginUpdate();
                try
                {
                    aData.SetSelectBound(aDeGroupStartNo, HC.View.HC.OffsetAfter,
                        vEndNo, HC.View.HC.OffsetBefor);
                    aData.InsertText(aText);
                }
                finally
                {
                    this.EndUpdate();
                }
            }
        }

        /// <summary> 是否处于留痕状态 </summary>
        public bool Trace
        {
            get { return FTrace; }
            set { FTrace = value; }
        }
    }
}
