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
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Configuration;

namespace EMRView
{
    public delegate void SyncDeItemEventHandle(object sender, HCCustomData aData, HCCustomItem aItem);
    public delegate bool HCCopyPasteStreamEventHandler(Stream aStream);

    public class HCEmrView :
        #if VIEWINPUTHELP
        HCEmrViewIH
        #else
        HCView
        #endif
    {
        private bool FDesignMode;
        private bool FHideTrace;  // 隐藏痕迹
        private bool FTrace;  // 是否处于留痕迹状态
        private bool FSecret;
        private bool FTraceInfoAnnotate;
        private bool FIgnoreAcceptAction = false;
        private int FTraceCount;  // 当前文档痕迹数量
        private Color FDeDoneColor, FDeUnDoneColor, FDeHotColor;
        private string FPageBlankTip;
        private object FPropertyObject;

#if PROCSERIES
        private bool FShowProcSplit;
        private int FProcCount;  // 当前文档病程数量
        private ProcInfo FCaretProcInfo,  // 当前光标处的病程信息
            FEditProcInfo;  // 当前正在编辑的病程信息
        private string FEditProcIndex;  // 当前允许编辑的病程
#endif

        private EventHandler FOnCanNotEdit;
        private SyncDeItemEventHandle FOnSyncDeItem;
        private HCCopyPasteEventHandler FOnCopyRequest, FOnPasteRequest;
        private HCCopyPasteStreamEventHandler FOnCopyAsStream, FOnPasteFromStream;
        // 语法检查相关事件
        private DataDomainItemNoEventHandler FOnSyntaxCheck;
        private SyntaxPaintEventHandler FOnSyntaxPaint;

        private void SetHideTrace(bool value)
        {
            if (FHideTrace != value)
            {
                FHideTrace = value;
                DeItem vDeItem;
                HCItemTraverse vItemTraverse = new HCItemTraverse();
                vItemTraverse.Areas.Add(SectionArea.saPage);
                vItemTraverse.Process = delegate (HCCustomData data, int itemNo, int tag, Stack<HCDomainInfo> domainStack, ref bool stop)
                {
                    if (data.Items[itemNo] is DeItem)
                    {
                        vDeItem = data.Items[itemNo] as DeItem;
                        if (vDeItem.StyleEx == StyleExtra.cseDel)
                            vDeItem.Visible = !FHideTrace;
                    }
                };

                this.TraverseItem(vItemTraverse);
                this.FormatData();
                if (FHideTrace)
                {
                    if (!this.ReadOnly)
                        this.ReadOnly = true;
                }
            }
        }

        private void SetPageBlankTip(string value)
        {
            if (FPageBlankTip != value)
            {
                FPageBlankTip = value;
                this.UpdateView();
            }
        }

        private void DoSyntaxCheck(HCCustomData aData, int aItemNo, int aTag, Stack<HCDomainInfo> aDomainStack, ref bool aStop)
        {
            //if (FOnSyntaxCheck != null) 调用前已经判断了
            if (aData.Items[aItemNo].StyleNo > HCStyle.Null)
                FOnSyntaxCheck(aData, aDomainStack, aItemNo);
        }

        private void DoSyncDeItem(object sender, HCCustomData aData, HCCustomItem aItem)
        {
            if (FOnSyncDeItem != null)
                FOnSyncDeItem(sender, aData, aItem);
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

        /// <summary> 当有新Item创建完成后触发的事件 </summary>
        /// <param name="sender">Item所属的文档节</param>
        /// <param name="e"></param>
        protected override void DoSectionCreateItem(object sender, EventArgs e)
        {
            if ((!Style.States.Contain(HCState.hosLoading)) && FTrace)
                (sender as DeItem).StyleEx = StyleExtra.cseAdd;

            base.DoSectionCreateItem(sender, e);
        }

        /// <summary> 当有新Item创建时触发 </summary>
        /// <param name="aData">创建Item的Data</param>
        /// <param name="aStyleNo">要创建的Item样式</param>
        /// <returns>创建好的Item</returns>
        protected override HCCustomItem DoSectionCreateStyleItem(HCCustomData aData, int aStyleNo)
        {
            return HCEmrViewLite.CreateEmrStyleItem(aData, aStyleNo);
        }

        protected override void DoSectionCaretItemChanged(object sender, HCCustomData data, HCCustomItem item)
        {
            string vInfo = "";
            DeGroup vDeGroup = null;
            HCCustomItem vActiveItem = this.GetTopLevelItem();
            if (vActiveItem != null)
            {
#if PROCSERIES
                if ((FProcCount > 0) && (data == this.ActiveSection.Page))
                {
                    this.GetSectionCaretProcInfo(this.ActiveSection.Page, FCaretProcInfo);
                    FCaretProcInfo.SectionIndex = this.ActiveSectionIndex;
                    if (FCaretProcInfo.EndNo > 0)
                    {
                        vDeGroup = this.ActiveSection.ActiveData.Items[FCaretProcInfo.BeginNo] as DeGroup;
                        vInfo = vDeGroup[DeProp.Name];
                    }

                    if (FCaretProcInfo.Index == FEditProcIndex)
                        FEditProcInfo.Assign(FCaretProcInfo);
                }
#endif

                HCViewData vData = this.ActiveSectionTopLevelData() as HCViewData;

                if (vData.ActiveDomain.EndNo > 0)
                {
                    vDeGroup = vData.Items[vData.ActiveDomain.BeginNo] as DeGroup;
                    #if PROCSERIES
                    if (!vDeGroup.IsProc)
                    #endif
                    {
                        if (vInfo != "")
                            vInfo = vInfo + ">" + vDeGroup[DeProp.Name] + "(" + vDeGroup[DeProp.Index] + ")";
                        else
                            vInfo = vDeGroup[DeProp.Name] + "(" + vDeGroup[DeProp.Index] + ")";
                    }
                }

                if (vActiveItem is DeItem)
                {
                    DeItem vDeItem = vActiveItem as DeItem;
                    if (vDeItem.StyleEx != StyleExtra.cseNone)
                        vInfo = vInfo + "-" + vDeItem.GetHint();
                    else
                    if (vDeItem.IsElement)
                    {
                        if (vInfo != "")
                            vInfo = vInfo + " > " + vDeItem[DeProp.Name] + "(" + vDeItem[DeProp.Index] + ")";
                        else
                            vInfo = vDeItem[DeProp.Name] + "(" + vDeItem[DeProp.Index] + ")";
                    }
                }
                else
                if (vActiveItem is DeEdit)
                {
                    DeEdit vDeEdit = vActiveItem as DeEdit;
                    if (vInfo != "")
                        vInfo = vInfo + " > " + vDeEdit[DeProp.Name] + "(" + vDeEdit[DeProp.Index] + ")";
                    else
                        vInfo = vDeEdit[DeProp.Name] + "(" + vDeEdit[DeProp.Index] + ")";
                }
                else
                if (vActiveItem is DeCombobox)
                {
                    DeCombobox vDeCombobox = vActiveItem as DeCombobox;
                    if (vInfo != "")
                        vInfo = vInfo + " > " + vDeCombobox[DeProp.Name] + "(" + vDeCombobox[DeProp.Index] + ")";
                    else
                        vInfo = vDeCombobox[DeProp.Name] + "(" + vDeCombobox[DeProp.Index] + ")";
                }
                else
                if (vActiveItem is DeDateTimePicker)
                {
                    DeDateTimePicker vDeDateTimePicker = vActiveItem as DeDateTimePicker;
                    if (vInfo != "")
                        vInfo = vInfo + " > " + vDeDateTimePicker[DeProp.Name] + "(" + vDeDateTimePicker[DeProp.Index] + ")";
                    else
                        vInfo = vDeDateTimePicker[DeProp.Name] + "(" + vDeDateTimePicker[DeProp.Index] + ")";
                }
            }

            this.HScrollBar.Statuses[1].Text = vInfo;
            base.DoSectionCaretItemChanged(sender, data, item);
        }

        /// <summary> 当节某Data有Item插入后触发 </summary>
        /// <param name="sender">在哪个文档节插入</param>
        /// <param name="aData">在哪个Data插入</param>
        /// <param name="aItem">已插入的Item</param>
        protected override void DoSectionInsertItem(object sender, HCCustomData aData, HCCustomItem aItem)
        {
            if (aItem is DeItem)
            {
                DeItem vDeItem = aItem as DeItem;
                if (vDeItem.StyleEx != StyleExtra.cseNone)
                {
                    FTraceCount++;
                    if (FTraceInfoAnnotate)
                        this.AnnotatePre.InsertDataAnnotate(null);
                }
                else
                    DoSyncDeItem(sender, aData, aItem);
            }
            #if PROCSERIES
            if (aItem is DeGroup)
            {
                if ((aItem as DeGroup).IsProcBegin)
                    FProcCount++;
            }
            #endif
            else
            if (aItem is DeEdit)
                DoSyncDeItem(sender, aData, aItem);
            else
            if (aItem is DeCombobox)
                DoSyncDeItem(sender, aData, aItem);
            else
            if (aItem is DeFloatBarCodeItem)
                DoSyncDeItem(sender, aData, aItem);
            else
            if (aItem is DeImageItem)
                DoSyncDeItem(sender, aData, aItem);

            base.DoSectionInsertItem(sender, aData, aItem);
        }

        /// <summary> 当节中某Data有Item删除后触发 </summary>
        /// <param name="sender">在哪个文档节删除</param>
        /// <param name="aData">在哪个Data删除</param>
        /// <param name="aItem">已删除的Item</param>
        protected override void DoSectionRemoveItem(object sender, HCCustomData aData, HCCustomItem aItem)
        {
            if (aItem is DeItem)
            {
                DeItem vDeItem = aItem as DeItem;

                if (vDeItem.StyleEx != StyleExtra.cseNone)
                {
                    FTraceCount--;

                    //if ((FTraceCount == 0) && (this.AnnotatePre.Visible) && (this.AnnotatePre.Count == 0))
                    //    this.AnnotatePre.Visible = false;
                }
            }

            base.DoSectionRemoveItem(sender, aData, aItem);
        }

        protected override bool DoSectionSaveItem(object sender, HCCustomData aData, int aItemNo)
        {
            bool vResult = base.DoSectionSaveItem(sender, aData, aItemNo);
            if (Style.States.Contain(HCState.hosCopying))  // 非设计模式、复制保存
            {
                if ((aData.Items[aItemNo] is DeGroup) && (!FDesignMode))
                    vResult = false;
                else
                if (aData.Items[aItemNo] is DeItem)
                    vResult = !(aData.Items[aItemNo] as DeItem).CopyProtect;  // 是否禁止复制
            }

            return vResult;
        }

        protected override bool DoSectionPaintDomainRegion(object sender, HCCustomData data, int itemNo)
        {
            return (data.Items[itemNo] as DeGroup)[GroupProp.SubType] != SubType.Proc;
        }

        protected override void DoSectionItemMouseDown(object sender, HCCustomData aData, int aItemNo, int aOffset, MouseEventArgs e)
        {
            base.DoSectionItemMouseDown(sender, aData, aItemNo, aOffset, e);
            if (!(sender as HCCustomSection).SelectExists())
            {
                HCCustomItem vItem = aData.Items[aItemNo];
                if ((vItem is DeItem) && aData.SelectInfo.StartRestrain)  // 是通过约束选中的,不按激活处理,便于数据元后输入普通内容
                    vItem.Active = false;
            }
        }

        /// <summary> 指定的节当前是否可编辑 </summary>
        /// <param name="sender">文档节</param>
        /// <returns>True：可编辑，False：不可编辑</returns>
        protected override bool DoSectionCanEdit(Object sender)
        {
            if (FIgnoreAcceptAction)
                return true;

            bool vResult = base.DoSectionCanEdit(sender);
            if (vResult)
            {
                HCViewData vViewData = sender as HCViewData;
                if ((vViewData.ActiveDomain != null) && (vViewData.ActiveDomain.BeginNo >= 0))
                    return !((vViewData.Items[vViewData.ActiveDomain.BeginNo] as DeGroup).ReadOnly);
                else
                    return true;
            }

            return false;
        }

        protected override bool DoSectionAcceptAction(object sender, HCCustomData aData, int aItemNo, int aOffset, HCAction aAction)
        {
            if (FIgnoreAcceptAction)
                return true;

            #if PROCSERIES
            if ((aAction == HCAction.actDeleteSelected) && (aData == this.ActiveSection.Page) && (FEditProcInfo.EndNo > 0))
            {
                if ((aData.SelectInfo.StartItemNo < FEditProcInfo.BeginNo) || (aData.SelectInfo.EndItemNo > FEditProcInfo.EndNo))
                    return false;
            }
            #endif

            bool vResult = base.DoSectionAcceptAction(sender, aData, aItemNo, aOffset, aAction);
            if (vResult)
            {
                switch (aAction)
                {
                    case HCAction.actBackDeleteText:
                    case HCAction.actDeleteText:
                        {
                            if (!FDesignMode && (aData.Items[aItemNo] is DeItem))
                            {
                                DeItem vDeItem = aData.Items[aItemNo] as DeItem;

                                if (vDeItem.IsElement && (vDeItem.Length == 1) && vDeItem.DeleteAllow)
                                {
                                    if (vDeItem[DeProp.Name] != "")
                                        this.SetActiveItemText(vDeItem[DeProp.Name]);
                                    else
                                        this.SetActiveItemText("未填写");

                                    vDeItem.AllocValue = false;

                                    vResult = false;
                                }
                            }
                        }
                        break;

                    case HCAction.actSetItemText:
                        {
                            if (aData.Items[aItemNo] is DeItem)
                            {
                                DeItem vDeItem = aData.Items[aItemNo] as DeItem;
                                vDeItem.AllocValue = true;
                            }
                        }
                        break;

                    case HCAction.actReturnItem:
                        {
                            if (aData.Items[aItemNo] is DeItem)
                            {
                                DeItem vDeItem = aData.Items[aItemNo] as DeItem;
                                if ((aOffset > 0) && (aOffset < vDeItem.Length) && vDeItem.IsElement)
                                    vResult = false;
                            }
                        }
                        break;

                    case HCAction.actDeleteItem:
                        {
                            HCCustomItem vItem = aData.Items[aItemNo];
                            if (vItem is DeGroup)  // 非设计模式不允许删除数据组
                                vResult = false;
                            else
                            if (vItem is DeItem)
                                vResult = (vItem as DeItem).DeleteAllow;
                            else
                            if (vItem is DeTable)
                                vResult = (vItem as DeTable).DeleteAllow;
                            else
                            if (vItem is DeCheckBox)
                                vResult = (vItem as DeCheckBox).DeleteAllow;
                            else
                            if (vItem is DeEdit)
                                vResult = (vItem as DeEdit).DeleteAllow;
                            else
                            if (vItem is DeCombobox)
                                vResult = (vItem as DeCombobox).DeleteAllow;
                            else
                            if (vItem is DeDateTimePicker)
                                vResult = (vItem as DeDateTimePicker).DeleteAllow;
                            else
                            if (vItem is DeRadioGroup)
                                vResult = (vItem as DeRadioGroup).DeleteAllow;
                            else
                            if (vItem is DeFloatBarCodeItem)
                                vResult = (vItem as DeFloatBarCodeItem).DeleteAllow;
                            else
                            if (vItem is DeImageItem)
                                vResult = (vItem as DeImageItem).DeleteAllow;
                        }

                        break;
                }
            }

            return vResult;
        }

        /// <summary> 按键按下 </summary>
        /// <param name="e">按键信息</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (FTrace)
            {
                if (HC.View.HC.IsKeyDownEdit(e.KeyValue))
                {
                    if (CanNotEdit())
                        return;

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
                        if (vData.SelectInfo.StartItemOffset == HC.View.HC.OffsetBefor)  // 在最前面
                        {
                            if (e.KeyCode == Keys.Back)  // 回删
                            {
                                if (vData.SelectInfo.StartItemNo == 0)
                                    return;  // 第一个最前面则不处理
                                else  // 不是第一个最前面
                                {
                                    vData.SelectInfo.StartItemNo = vData.SelectInfo.StartItemNo - 1;
                                    vData.SelectInfo.StartItemOffset = vData.Items[vData.SelectInfo.StartItemNo].Length;
                                    this.OnKeyDown(e);
                                }
                            }
                            else
                            if (e.KeyCode == Keys.Delete)  // 后删
                            {
                                vData.SelectInfo.StartItemOffset = HC.View.HC.OffsetAfter;
                                //this.OnKeyDown(e);
                            }
                            else
                                base.OnKeyDown(e);
                        }
                        else
                        if (vData.SelectInfo.StartItemOffset == HC.View.HC.OffsetAfter)  // 在最后面
                        {
                            if (e.KeyCode == Keys.Back)
                            {
                                vData.SelectInfo.StartItemOffset = HC.View.HC.OffsetBefor;
                                //this.OnKeyDown(e);
                            }
                            else
                            if (e.KeyCode == Keys.Delete)
                            {
                                if (vData.SelectInfo.StartItemNo == vData.Items.Count - 1)
                                    return;
                                else
                                {
                                    vData.SelectInfo.StartItemNo = vData.SelectInfo.StartItemNo + 1;
                                    vData.SelectInfo.StartItemOffset = 0;
                                    this.OnKeyDown(e);
                                }
                            }
                            else
                                base.OnKeyDown(e);
                        }
                        else
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

        /// <summary> 按键按压 </summary>
        /// <param name="e">按键信息</param>
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

        /// <summary> 在当前位置插入文本 </summary>
        /// <param name="AText">要插入的字符串(支持带#13#10的回车换行)</param>
        /// <returns>True：插入成功，False：插入失败</returns>
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

        /// <summary> 复制前，便于控制是否允许复制 </summary>
        protected override bool DoCopyRequest(int aFormat)
        {
            if (FOnCopyRequest != null)
                return FOnCopyRequest(aFormat);
            else
                return base.DoCopyRequest(aFormat);
        }

        /// <summary> 粘贴前，便于控制是否允许粘贴 </summary>
        protected override bool DoPasteRequest(int aFormat)
        {
            HCCustomItem vItem = this.ActiveSectionTopLevelData().GetActiveItem();
            if ((vItem is DeItem) && (vItem as DeItem).IsElement)
            {
                if (aFormat != User.CF_TEXT)
                    return false;
            }

            if (FOnPasteRequest != null)
                return FOnPasteRequest(aFormat);
            else
                return base.DoPasteRequest(aFormat);
        }

        /// <summary> 复制前，便于订制特征数据如内容来源 </summary>
        protected override void DoCopyAsStream(Stream aStream)
        {
            if (FOnCopyAsStream != null)
                FOnCopyAsStream(aStream);
            else
                base.DoCopyAsStream(aStream);
        }

        /// <summary> 复制前，便于订制特征数据如内容来源 </summary>
        protected override bool DoPasteFormatStream(Stream aStream)
        {
            if (FOnPasteFromStream != null)
                return FOnPasteFromStream(aStream);
            else
                return base.DoPasteFormatStream(aStream);
        }

        protected override void DoSectionPaintPageBefor(object sender, int aPageIndex, RECT aRect, HCCanvas aCanvas, SectionPaintInfo aPaintInfo)
        {
            base.DoSectionPaintPageBefor(sender, aPageIndex, aRect, aCanvas, aPaintInfo);

            #if PROCSERIES
            if ((!aPaintInfo.Print) && (FEditProcInfo.EndNo > 0))
            {
                HCPageData vData = FEditProcInfo.Data as HCPageData;
                POINT vPt = vData.DrawItems[vData.Items[FEditProcInfo.BeginNo].FirstDItemNo].Rect.TopLeft();
                vPt = this.GetFormatPointToViewCoord(vPt);
                if (vPt.Y > aRect.Top)
                {
                    aCanvas.Brush.Color = HC.View.HC.clBtnFace;
                    aCanvas.FillRect(new RECT(aRect.Left, aRect.Top, aRect.Right, vPt.Y));
                }

                if (FEditProcInfo.EndNo < vData.Items.Count - 1)
                {
                    vPt = vData.DrawItems[vData.Items[FEditProcInfo.EndNo].FirstDItemNo].Rect.BottomRight();
                    vPt = this.GetFormatPointToViewCoord(vPt);
                    if (vPt.Y < aRect.Bottom)
                    {
                        aCanvas.Brush.Color = HC.View.HC.clBtnFace;
                        vPt.X = aRect.Top + ((HCSection)sender).GetPageDataHeight(aPageIndex);
                        if (vPt.X < aRect.Bottom)
                            aCanvas.FillRect(new RECT(aRect.Left, vPt.Y, aRect.Right, vPt.X));
                        else
                            aCanvas.FillRect(new RECT(aRect.Left, vPt.Y, aRect.Right, aRect.Bottom));
                    }
                }
            }
            #endif
        }

        protected override void DoSectionDrawItemPaintBefor(object sender, HCCustomData aData, int aItemNo, int aDrawItemNo, RECT aDrawRect,
            int aDataDrawLeft, int aDataDrawRight, int aDataDrawBottom, int aDataScreenTop, int aDataScreenBottom, HCCanvas aCanvas, PaintInfo aPaintInfo)
        {
            if (aPaintInfo.Print)
                return;

            #if PROCSERIES
            if (FShowProcSplit && (FProcCount > 0))
            {
                if ((aData is HCPageData) && (aData.Items[aItemNo] is DeGroup))
                {
                    DeGroup vDeGroup = aData.Items[aItemNo] as DeGroup;
                    if (vDeGroup.IsProcBegin)
                    {
                        aCanvas.Pen.Style = HCPenStyle.psDashDotDot;
                        aCanvas.Pen.Color = Color.Blue;
                        aCanvas.MoveTo(aDataDrawLeft, aDrawRect.Top - 1);
                        aCanvas.LineTo(aDataDrawRight, aDrawRect.Top - 1);
                    }
                }
            }
            #endif

            if (!(aData.Items[aItemNo] is DeItem))
                return;

            DeItem vDeItem = aData.Items[aItemNo] as DeItem;
            if (!vDeItem.Selected())
            {
                if (vDeItem.IsElement)
                {
                    if (vDeItem.MouseIn || vDeItem.Active)  // 鼠标移入或光标在其中
                    {
                        aCanvas.Brush.Color = FDeHotColor;
                        aCanvas.FillRect(aDrawRect);
                    }
                    else
                    if (FDesignMode)  // 设计模式
                    {
                        if (vDeItem.AllocValue)  // 已经填写过了
                            aCanvas.Brush.Color = FDeDoneColor;
                        else  // 没填写过
                            aCanvas.Brush.Color = FDeUnDoneColor;

                        aCanvas.FillRect(aDrawRect);
                    }
                    else  // 非设计模式
                    {
                        if (vDeItem.OutOfRang)  // 超范围
                        {
                            aCanvas.Brush.Color = Color.Red;
                            aCanvas.FillRect(aDrawRect);
                        }
                        else  // 没超范围
                        {
                            if (vDeItem.AllocValue)  // 已经填写过了
                                aCanvas.Brush.Color = FDeDoneColor;
                            else  // 没填写过
                                aCanvas.Brush.Color = FDeUnDoneColor;

                            aCanvas.FillRect(aDrawRect);
                        }
                    }
                }
                else  // 不是数据元
                if (FDesignMode || vDeItem.MouseIn || vDeItem.Active)
                {
                    if (vDeItem.EditProtect || vDeItem.CopyProtect)
                    {
                        aCanvas.Brush.Color = HC.View.HC.clBtnFace;
                        aCanvas.FillRect(aDrawRect);
                    }
                }
            }

            if (!FHideTrace)  // 显示痕迹
            {
                if (vDeItem.StyleEx == StyleExtra.cseDel)  // 痕迹
                {
                    int vTextHeight = Style.TextStyles[vDeItem.StyleNo].FontHeight;
                    int vTop = aDrawRect.Top;
                    switch (Style.ParaStyles[vDeItem.ParaNo].AlignVert)
                    {
                        case ParaAlignVert.pavTop:
                            vTop = aDrawRect.Top + vTextHeight / 2;
                            break;

                        case ParaAlignVert.pavCenter:
                            vTop = aDrawRect.Top + (aDrawRect.Bottom - aDrawRect.Top) / 2;
                            break;

                        default:
                            vTop = aDrawRect.Bottom - vTextHeight / 2;
                            break;
                    }

                    // 绘制删除线
                    aCanvas.Pen.BeginUpdate();
                    try
                    {
                        aCanvas.Pen.Style = HCPenStyle.psSolid;
                        aCanvas.Pen.Color = Color.Red;
                        aCanvas.Pen.Width = 1;
                    }
                    finally
                    {
                        aCanvas.Pen.EndUpdate();
                    }

                    aCanvas.MoveTo(aDrawRect.Left, vTop - 1);
                    aCanvas.LineTo(aDrawRect.Right, vTop - 1);
                    aCanvas.MoveTo(aDrawRect.Left, vTop + 2);
                    aCanvas.LineTo(aDrawRect.Right, vTop + 2);
                }
                else
                if (vDeItem.StyleEx == StyleExtra.cseAdd)
                {
                    int vTextHeight = Style.TextStyles[vDeItem.StyleNo].FontHeight;
                    int vTop = aDrawRect.Bottom;
                    switch (Style.ParaStyles[vDeItem.ParaNo].AlignVert)
                    {
                        case ParaAlignVert.pavTop:
                            vTop = aDrawRect.Top + vTextHeight;
                            break;

                        case ParaAlignVert.pavCenter:
                            vTop = aDrawRect.Top + (aDrawRect.Bottom - aDrawRect.Top + vTextHeight) / 2;
                            break;
                    }

                    aCanvas.Pen.BeginUpdate();
                    try
                    {
                        aCanvas.Pen.Style = HCPenStyle.psSolid;
                        aCanvas.Pen.Color = Color.Blue;
                        aCanvas.Pen.Width = 1;
                    }
                    finally
                    {
                        aCanvas.Pen.EndUpdate();
                    }

                    aCanvas.MoveTo(aDrawRect.Left, aDrawRect.Bottom);
                    aCanvas.LineTo(aDrawRect.Right, aDrawRect.Bottom);
                }
            }
        }

        protected override void DoSectionDrawItemPaintContent(HCCustomData aData, int aItemNo, int aDrawItemNo, RECT aDrawRect, RECT aClearRect, string aDrawText,
            int aDataDrawLeft, int aDataDrawRight, int aDataDrawBottom, int aDataScreenTop, int aDataScreenBottom, HCCanvas aCanvas, PaintInfo aPaintInfo)
        {
            if (aPaintInfo.Print)
                return;

            if (!(aData.Items[aItemNo] is DeItem))
                return;

            DeItem vDeItem = aData.Items[aItemNo] as DeItem;
            if ((vDeItem.SyntaxCount() > 0) && (!vDeItem.IsSelectComplate))
            {
                int vOffset = aData.DrawItems[aDrawItemNo].CharOffs;
                int vOffsetEnd = aData.DrawItems[aDrawItemNo].CharOffsetEnd();

                int vSyOffset = 0, vSyOffsetEnd = 0, vStart = 0, vLen;
                RECT vRect = new RECT();
                bool vDT = false;
                bool vDrawSyntax = false;
                for (int i = 0; i < vDeItem.Syntaxs.Count; i++)
                {
                    vSyOffset = vDeItem.Syntaxs[i].Offset;
                    if (vSyOffset > vOffsetEnd)
                        continue;

                    vSyOffsetEnd = vSyOffset + vDeItem.Syntaxs[i].Length - 1;
                    if (vSyOffsetEnd < vOffset)
                        continue;

                    vDrawSyntax = false;
                    if ((vSyOffset <= vOffset) && (vSyOffsetEnd >= vOffsetEnd))
                    {
                        vDrawSyntax = true;
                        vRect.Left = aClearRect.Left;
                        vRect.Right = aClearRect.Right;
                    }
                    else
                    if (vSyOffset >= vOffset)  // 有交集
                    {
                        vDrawSyntax = true;
                        if (vSyOffsetEnd <= vOffsetEnd)  // 问题在DrawItem中间
                        {
                            vStart = vSyOffset - vOffset;
                            vLen = vDeItem.Syntaxs[i].Length;
                            vRect.Left = aClearRect.Left
                                + aData.GetDrawItemOffsetWidth(aDrawItemNo, vStart, aCanvas);
                            vRect.Right = aClearRect.Left
                                + aData.GetDrawItemOffsetWidth(aDrawItemNo, vStart + vLen, aCanvas);
                        }
                        else  // DrawItem是问题的一部分
                        {
                            vRect.Left = aClearRect.Left
                                + aData.GetDrawItemOffsetWidth(aDrawItemNo, vSyOffset - vOffset, aCanvas);
                            vRect.Right = aClearRect.Right;
                        }
                    }
                    else  // vSyOffset < vOffset
                    if (vSyOffsetEnd <= vOffsetEnd)  // 有交集，DrawItem是问题的一部分
                    {
                        vDrawSyntax = true;
                        vRect.Left = aClearRect.Left;
                        vRect.Right = aClearRect.Left
                            + aData.GetDrawItemOffsetWidth(aDrawItemNo, vSyOffsetEnd - vOffset + 1, aCanvas);
                    }

                    if (vDrawSyntax)  // 此DrawItem中有语法问题
                    {
                        vRect.Top = aClearRect.Top;
                        vRect.Bottom = aClearRect.Bottom;

                        if (FOnSyntaxPaint != null)
                            FOnSyntaxPaint(aData, aItemNo, aDrawText, vDeItem.Syntaxs[i], vRect, aCanvas);
                        else
                        {
                            switch (vDeItem.Syntaxs[i].Problem)
                            {
                                case EmrSyntaxProblem.espContradiction:
                                    aCanvas.Pen.Color = Color.Red;
                                    break;

                                case EmrSyntaxProblem.espWrong:
                                    aCanvas.Pen.Color = Color.Orange;
                                    break;
                            }

                            vDT = false;
                            vStart = vRect.Left;
                            aCanvas.MoveTo(vStart, vRect.Bottom);
                            while (vStart < vRect.Right)
                            {
                                vStart = vStart + 2;
                                if (vStart > vRect.Right)
                                    vStart = vRect.Right;

                                if (!vDT)
                                    aCanvas.LineTo(vStart, vRect.Bottom + 2);
                                else
                                    aCanvas.LineTo(vStart, vRect.Bottom);

                                vDT = !vDT;
                            }
                        }
                    }
                }
            }
        }

#region 子方法，绘制当前页以下空白的提示
        private void DrawBlankTip_(int aLeft, int aTop, int aRight, int aDataDrawBottom, HCCanvas aCanvas)
        {
            if (aTop + 14 <= aDataDrawBottom)
            {
                aCanvas.Font.BeginUpdate();
                try
                {
                    aCanvas.Font.Size = 12;
                    aCanvas.Font.FontStyles.Value = 0;
                    aCanvas.Font.Color = Color.Black;
                }
                finally
                {
                    aCanvas.Font.EndUpdate();
                }

                aCanvas.TextOut(aLeft + ((aRight - aLeft) - aCanvas.TextWidth(FPageBlankTip)) / 2, aTop, FPageBlankTip);
            }
        }

        private void DrawTraceHint_(DeItem deItem, RECT aDrawRect, int aDataDrawRight, HCCanvas aCanvas)
        {
            aCanvas.Font.Size = 12;
            aCanvas.Font.Color = Color.Black;
            string vTrace = deItem[DeProp.Trace];
            SIZE vSize = aCanvas.TextExtent(vTrace);
            RECT vRect = HC.View.HC.Bounds(aDrawRect.Left, aDrawRect.Top - vSize.cy - 5, vSize.cx, vSize.cy);
            if (vRect.Right > aDataDrawRight)
                vRect.Offset(aDataDrawRight - vRect.Right, 0);

            if (deItem.StyleEx == StyleExtra.cseDel)
                aCanvas.Brush.Color = HC.View.HC.clBtnFace;
            else
                aCanvas.Brush.Color = HC.View.HC.clInfoBk;

            aCanvas.TextRect(vRect, vRect.Left, vRect.Top, vTrace);
            aCanvas.Pen.Color = Color.Gray;
            aCanvas.Pen.Width = 2;
            aCanvas.MoveTo(vRect.Left + 2, vRect.Bottom + 1);
            aCanvas.LineTo(vRect.Right, vRect.Bottom + 1);
            aCanvas.MoveTo(vRect.Right + 1, vRect.Top + 2);
            aCanvas.LineTo(vRect.Right + 1, vRect.Bottom + 1);
        }
#endregion

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
        protected override void DoSectionDrawItemPaintAfter(Object sender, HCCustomData aData, int aItemNo, int aDrawItemNo, RECT aDrawRect, 
            int aDataDrawLeft, int aDataDrawRight, int aDataDrawBottom, int aDataScreenTop, int aDataScreenBottom, HCCanvas aCanvas, PaintInfo aPaintInfo)
        {
            if (aPaintInfo.Print)  // 打印时没有填写过的数据元不打印
            {
                HCCustomItem vItem = aData.Items[aItemNo];
                if (vItem.StyleNo > HCStyle.Null)
                {
                    DeItem vDeItem = vItem as DeItem;
                    if (vDeItem.IsElement && !vDeItem.AllocValue)
                    {
                        aCanvas.Brush.Color = Color.White;
                        aCanvas.FillRect(aDrawRect);
                        return;
                    }
                }
            }

            if ((!FHideTrace) && (FTraceCount > 0))  // 显示痕迹且有痕迹
            {
                HCCustomItem vItem = aData.Items[aItemNo];
                if (vItem.StyleNo > HCStyle.Null)
                {
                    DeItem vDeItem = vItem as DeItem;
                    if (vDeItem.StyleEx != StyleExtra.cseNone)  // 添加批注
                    {
                        if (FTraceInfoAnnotate)
                        {
                            HCDrawAnnotateDynamic vDrawAnnotate = new HCDrawAnnotateDynamic();
                            vDrawAnnotate.DrawRect = aDrawRect;
                            vDrawAnnotate.Title = vDeItem.GetHint();
                            vDrawAnnotate.Text = aData.GetDrawItemText(aDrawItemNo);

                            this.AnnotatePre.AddDrawAnnotate(vDrawAnnotate);
                        }
                        else
                        if (aDrawItemNo == (aData as HCRichData).HotDrawItemNo)
                            DrawTraceHint_(vDeItem, aDrawRect, aDataDrawRight, aCanvas);
                    }
                }
            }

            if (FSecret)
            {
                HCCustomItem vItem = aData.Items[aItemNo];
                if ((vItem.StyleNo > HCStyle.Null) && ((vItem as DeItem)[DeProp.Secret] != ""))
                {
                    int vSecretLow = -1, vSecretHi = -1;
                    DeItem.GetSecretRange((vItem as DeItem)[DeProp.Secret], ref vSecretLow, ref vSecretHi);
                    if (vSecretLow > 0)
                    {
                        if (vSecretHi < 0)
                            vSecretHi = vItem.Length;

                        HCCustomDrawItem vDrawItem = aData.DrawItems[aDrawItemNo];
                        if (vSecretLow <= vDrawItem.CharOffsetEnd())
                        {
                            if (vSecretLow < vDrawItem.CharOffs)
                                vSecretLow = vDrawItem.CharOffs;

                            if (vSecretHi > vDrawItem.CharOffsetEnd())
                                vSecretHi = vDrawItem.CharOffsetEnd();

                            vSecretLow = vSecretLow - vDrawItem.CharOffs + 1;
                            if (vSecretLow > 0)
                                vSecretLow--;

                            vSecretHi = vSecretHi - vDrawItem.CharOffs + 1;

                            if (vSecretHi >= 0)
                            {
                                aCanvas.Brush.Style = HCBrushStyle.bsDiagCross;
                                aCanvas.FillRect(new RECT(aDrawRect.Left + aData.GetDrawItemOffsetWidth(aDrawItemNo, vSecretLow), aDrawRect.Top,
                                    aDrawRect.Left + aData.GetDrawItemOffsetWidth(aDrawItemNo, vSecretHi), aDrawRect.Bottom));
                                aCanvas.Brush.Style = HCBrushStyle.bsSolid;
                            }
                        }
                    }
                }
            }

            #if PROCSERIES
            if ((!aPaintInfo.Print) && (aData.Items[aItemNo] is DeGroup))  // 绘制病程的前后指示箭头
            {
                DeGroup vDeGroup = aData.Items[aItemNo] as DeGroup;
                if (vDeGroup.MarkType == MarkType.cmtBeg)
                {
                    if (vDeGroup[GroupProp.SubType] == SubType.Proc)
                    {
                        if ((aItemNo > 0) && (aData.Items[aItemNo - 1] is DeGroup)
                            && ((aData.Items[aItemNo - 1] as DeGroup)[GroupProp.SubType] == SubType.Proc))
                            HC.View.HC.HCDrawArrow(aCanvas, HC.View.HC.clMedGray, aDrawRect.Left - 10, aDrawRect.Top, 0);

                        if (FEditProcInfo.BeginNo == aItemNo)
                            HC.View.HC.HCDrawArrow(aCanvas, Color.Blue, aDrawRect.Left - 10, aDrawRect.Top + 12, 1);
                        else
                            HC.View.HC.HCDrawArrow(aCanvas, HC.View.HC.clMedGray, aDrawRect.Left - 10, aDrawRect.Top + 12, 1);
                    }
                }
                else
                {
                    if (vDeGroup[GroupProp.SubType] == SubType.Proc)  // 病程尾
                    {
                        if ((aItemNo < aData.Items.Count - 1) && (aData.Items[aItemNo + 1] is DeGroup)
                            && ((aData.Items[aItemNo + 1] as DeGroup)[GroupProp.SubType] == SubType.Proc))  // 下一个是病程头
                            HC.View.HC.HCDrawArrow(aCanvas, HC.View.HC.clMedGray, aDrawRect.Right + 10, aDrawRect.Top + 12, 1);  // 向下箭头

                        if (this.FEditProcInfo.EndNo == aItemNo)
                            HC.View.HC.HCDrawArrow(aCanvas, Color.Blue, aDrawRect.Right + 10, aDrawRect.Top, 0);
                        else
                            HC.View.HC.HCDrawArrow(aCanvas, HC.View.HC.clMedGray, aDrawRect.Right + 10, aDrawRect.Top, 0);  // 向上箭头
                    }
                }
            }
            #endif

            if ((FPageBlankTip != "") && (aData is HCPageData))
            {
                if (aDrawItemNo < aData.DrawItems.Count - 1)
                {
                    if (aData.Items[aData.DrawItems[aDrawItemNo + 1].ItemNo].PageBreak)
                        DrawBlankTip_(aDataDrawLeft, aDrawRect.Top + aDrawRect.Height + aData.GetLineBlankSpace(aDrawItemNo), aDataDrawRight, aDataDrawBottom, aCanvas);
                }
                else
                    DrawBlankTip_(aDataDrawLeft, aDrawRect.Top + aDrawRect.Height + aData.GetLineBlankSpace(aDrawItemNo), aDataDrawRight, aDataDrawBottom, aCanvas);
            }

            base.DoSectionDrawItemPaintAfter(sender, aData, aItemNo, aDrawItemNo, aDrawRect, aDataDrawLeft, aDataDrawRight,
                aDataDrawBottom, aDataScreenTop, aDataScreenBottom, aCanvas, aPaintInfo);
        }

        protected override void WndProc(ref Message Message)
        {
            base.WndProc(ref Message);
        }

        protected override void Create()
        {
            base.Create();
            FHideTrace = false;
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
            FSecret = false;
            FTraceInfoAnnotate = true;
            FDeDoneColor = HC.View.HC.clBtnFace;
            FDeUnDoneColor = Color.FromArgb(0xFF, 0xDD, 0x80);
            FDeHotColor = Color.FromArgb(204, 224, 244);
            FPageBlankTip = "";// "--------本页以下空白--------";
            this.Style.DefaultTextStyle.Size = HC.View.HC.GetFontSize("小四");
            this.Style.DefaultTextStyle.Family = "宋体";
            this.HScrollBar.AddStatus(200);
#if PROCSERIES
            FShowProcSplit = true;
            FProcCount = 0;
            FCaretProcInfo = new ProcInfo();
            FEditProcInfo = new ProcInfo();
            FEditProcIndex = "";
#endif
        }

        ~HCEmrView()
        {

        }

        public override void Clear()
        {
            FTraceCount = 0;
#if PROCSERIES
            FProcCount = 0;
            FCaretProcInfo.Clear();
            FEditProcInfo.Clear();
            FEditProcIndex = "";
#endif
            base.Clear();
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
                    aTraverse.SectionIndex = i;

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
        /// <returns>True：成功，False：失败</returns>
        public bool InsertDeGroup(DeGroup aDeGroup)
        {
            return this.InsertDomain(aDeGroup);
        }

        public bool DeleteDeGroup(string deIndex)
        {
            if (deIndex == "")
                return false;

            int vStartNo = 0, vEndNo = -1;
            GetDataDeGroupItemNo(this.ActiveSection.Page, deIndex, false, ref vStartNo, ref vEndNo);
            if (vEndNo > 0)
            {
                return this.ActiveSection.DataAction(this.ActiveSection.Page, delegate ()
                {
                    FIgnoreAcceptAction = true;
                    try
                    {
                        this.ActiveSection.Page.DeleteItems(vStartNo, vEndNo, false);
                    }
                    finally
                    {
                        FIgnoreAcceptAction = false;
                    }

                    return true;
                });
            }

            return false;
        }

        /// <summary> 插入数据元 </summary>
        /// <param name="ADeItem">数据元信息</param>
        /// <returns>True：成功，False：失败</returns>
        public bool InsertDeItem(DeItem aDeItem)
        {
            return this.InsertItem(aDeItem);
        }

        /// <summary> 新建数据元 </summary>
        /// <param name="aText">数据元文本</param>
        /// <returns>新建好的数据元</returns>
        public DeItem NewDeItem(string aText)
        {
            DeItem Result = new DeItem();
            Result.Text = aText;
            Result.StyleNo = this.Style.GetStyleNo(this.Style.DefaultTextStyle, true);
            Result.ParaNo = this.CurParaNo;

            return Result;
        }

        /// <summary>
        /// 获取指定数据元的文本内容
        /// </summary>
        /// <param name="aDeIndex"></param>
        /// <returns></returns>
        public bool GetDeItemText(string aDeIndex, ref string aText)
        {
            return GetDeItemProperty(aDeIndex, "Text", ref aText);
        }

        /// <summary>
        /// 获取指定数据元指定属性值
        /// </summary>
        /// <param name="aDeIndex"></param>
        /// <param name="aPropName"></param>
        /// <param name="aPropValue"></param>
        /// <returns></returns>
        public bool GetDeItemProperty(string aDeIndex, string aPropName, ref string aPropValue)
        {            
            bool vResult = false;
            HCItemTraverse vItemTraverse = new HCItemTraverse();  // 准备存放遍历信息的对象
            vItemTraverse.Areas.Add(SectionArea.saHeader);
            vItemTraverse.Areas.Add(SectionArea.saPage);  // 遍历正文中的信息
            vItemTraverse.Areas.Add(SectionArea.saFooter);

            HCCustomItem vItem;
            string vText = "";
            TraverseItemEventHandle vTraveEvent = delegate (HCCustomData aData, int aItemNo, int aTag, Stack<HCDomainInfo> aDomainStack, ref bool aStop)
            {
                vItem = aData.Items[aItemNo];
                if ((vItem is DeItem) && (vItem as DeItem)[DeProp.Index] == aDeIndex)
                {
                    if (aPropName == "Text")
                        vText = vItem.Text;
                    else
                        vText = (vItem as DeItem)[aPropName];

                    vResult = true;
                    aStop = true;
                }
            };

            vItemTraverse.Process = vTraveEvent;  // 遍历到每一个文本对象是触发的事件
            this.TraverseItem(vItemTraverse);  // 开始遍历

            if (vResult)
                aPropValue = vText;

            return vResult;            
        }

        public bool SetDeImageGraphic(string deIndex, Stream graphicStream)
        {
            FPropertyObject = graphicStream;
            return SetDeObjectProperty(deIndex, "Graphic", "");
        }

        public bool SetSignatureGraphic(string deIndex, Stream graphicStream)
        {
            int vBeginNo = -1, vEndNo = -1;
            #if PROCSERIES
            if (this.FEditProcInfo.EndNo > 0)
            {
                vBeginNo = FEditProcInfo.BeginNo;
                vEndNo = FEditProcInfo.EndNo;
            }
            else
            #endif
            {
                vBeginNo = 0;
                vEndNo = this.ActiveSection.Page.Items.Count - 1;
            }

            bool vResult = false;
            HCCustomItem vTravItem;
            HCItemTraverse vItemTraverse = new HCItemTraverse();
            vItemTraverse.Tag = 0;
            vItemTraverse.Areas.Add(SectionArea.saPage);
            vItemTraverse.Process = delegate (HCCustomData data, int itemNo, int tag, Stack<HCDomainInfo> domainStack, ref bool stop)
            {
                if (data is HCPageData)
                {
                    if (itemNo >= vBeginNo)
                    {
                        vTravItem = data.Items[itemNo];
                        if ((vTravItem is DeImageItem) && ((vTravItem as DeImageItem)[DeProp.Index] == deIndex))
                        {
                            (vTravItem as DeImageItem).LoadGraphicStream(graphicStream, false);
                            vResult = true;
                            stop = true;
                        }
                    }

                    if (itemNo == vEndNo)
                        stop = true;
                }
            };

            this.ActiveSection.Page.TraverseItem(vItemTraverse);

            if (vResult)
            {
                this.FormatData();
                return true;
            }

            return false;
        }

        public void SyncDeItemAfterRef(HCCustomData startData, DeItem refDeItem)
        {
            bool vStart = false;
            bool vFind = false;

            HCCustomItem vItem;
            DeItem vDeItem;
            HCItemTraverse vItemTraverse = new HCItemTraverse();
            vItemTraverse.Tag = 0;
            vItemTraverse.Areas.Add(SectionArea.saPage);
            vItemTraverse.Process = delegate (HCCustomData data, int itemNo, int tag, Stack<HCDomainInfo> domainStack, ref bool stop)
            {
                vItem = data.Items[itemNo];
                if (vStart)
                {
                    if (vItem.StyleNo > HCStyle.Null)
                    {
                        vDeItem = vItem as DeItem;
                        if (vDeItem[DeProp.Index] == refDeItem[DeProp.Index])
                        {
                            vDeItem.Text = refDeItem.Text;
                            vDeItem.AllocValue = true;
                            vDeItem[DeProp.CMVVCode] = refDeItem[DeProp.CMVVCode];
                            vFind = true;
                        }
                    }
                }
                else
                if (vItem == refDeItem)
                    vStart = true;
            };

            this.TraverseItem(vItemTraverse);
            if (vFind)
                this.FormatData();
        }

        public DeItem FindSameDeItem(DeItem item)
        {
            DeItem vResult = null;
            HCItemTraverse vItemTraverse = new HCItemTraverse();
            vItemTraverse.Tag = 0;
            vItemTraverse.Areas.Add(SectionArea.saPage);
            vItemTraverse.Process = delegate (HCCustomData aData, int aItemNo, int aTag, Stack<HCDomainInfo> aDomainStack, ref bool aStop)
            {
                if (aData.Items[aItemNo].StyleNo > HCStyle.Null)
                {
                    DeItem vDeItem = aData.Items[aItemNo] as DeItem;
                    if (vDeItem[DeProp.Index] == item[DeProp.Index])
                    {
                        if (vDeItem.AllocValue)
                        {
                            vResult = vDeItem;
                            aStop = true;
                        }
                    }
                }
            };

            this.TraverseItem(vItemTraverse);
            return vResult;
        }

        /// <summary>
        /// 设置指定数据元的值
        /// </summary>
        /// <param name="aDeIndex"></param>
        /// <param name="aText"></param>
        /// <returns>是否设置成功</returns>
        public bool SetDeItemText(string aDeIndex, string aText)
        {
            return SetDeObjectProperty(aDeIndex, "Text", aText);
        }

        /// <summary>
        /// 设置指定数据元指定属性的值
        /// </summary>
        /// <param name="aDeIndex"></param>
        /// <param name="aPropName"></param>
        /// <param name="aPropValue"></param>
        /// <returns>是否设置成功</returns>
        public bool SetDeObjectProperty(string aDeIndex, string aPropName, string aPropValue)
        {
            bool vResult = false;
            bool vReFormat = false;

            HCItemTraverse vItemTraverse = new HCItemTraverse();  // 准备存放遍历信息的对象
            vItemTraverse.Areas.Add(SectionArea.saHeader);
            vItemTraverse.Areas.Add(SectionArea.saPage);  // 遍历正文中的信息
            vItemTraverse.Areas.Add(SectionArea.saFooter);

            HCCustomItem vItem;
            TraverseItemEventHandle vTraveEvent = delegate (HCCustomData aData, int aItemNo, int aTag, Stack<HCDomainInfo> aDomainStack, ref bool aStop)
            {
                if (!aData.CanEdit())
                {
                    aStop = true;
                    return;
                }

#if PROCSERIES
                if (this.FEditProcIndex != "")
                {
                    if ((FEditProcInfo.SectionIndex == vItemTraverse.SectionIndex) && (aData == FEditProcInfo.Data))
                    {
                        if ((aItemNo < FEditProcInfo.BeginNo) || (aItemNo > FEditProcInfo.EndNo))
                            return;
                    }
                }
#endif
                vItem = aData.Items[aItemNo];
                if ((vItem is DeItem) && (vItem as DeItem)[DeProp.Index] == aDeIndex)
                {
                    if (aPropName == "Text")
                    {
                        if (aPropValue != "")
                        {
                            vItem.Text = aPropValue;
                            (vItem as DeItem).AllocValue = true;
                            aData.SilenceChange();
                        }

                        vReFormat = true;
                    }
                    else
                    if (aPropName == "Propertys")
                    {
                        Dictionary<string, string> vPropertys = new Dictionary<string, string>();
                        DeProp.SetPropertyString(aPropValue, vPropertys);
                        foreach (KeyValuePair<string, string> obj in vPropertys)
                        {
                            if (obj.Key == "Text")
                            {
                                if (obj.Value != "")
                                {
                                    vItem.Text = obj.Value;
                                    (vItem as DeItem).AllocValue = true;
                                    aData.SilenceChange();
                                }

                                vReFormat = true;
                            }
                            else
                                (vItem as DeItem)[obj.Key] = obj.Value;
                        }
                    }
                    else
                        (vItem as DeItem)[aPropName] = aPropValue;

                    vResult = true;
                }
                else
                if ((vItem is DeImageItem) && ((vItem as DeImageItem)[DeProp.Index] == aDeIndex))
                {
                    if (aPropName == "Graphic")
                    {
                        (vItem as DeImageItem).LoadGraphicStream((Stream)FPropertyObject, false);
                        vReFormat = true;
                    }

                    vResult = true;
                    aStop = true;
                }
            };

            vItemTraverse.Process = vTraveEvent;  // 遍历到每一个文本对象是触发的事件
            this.TraverseItem(vItemTraverse);  // 开始遍历

            if (vResult)
            {
                if (vReFormat)
                    this.FormatData();
            }

            return vResult;
        }

#if PROCSERIES
        public bool InsertProc(string procIndex, string propertys, string beforProcIndex)
        {
            if (procIndex == "")
                return false;

            bool vResult = false;
            HCSection vSection = this.ActiveSection;
            HCViewData vPageData = this.ActiveSectionTopLevelData() as HCViewData;
            if (vPageData == this.ActiveSection.Page)  // 只能在正文插入病程
            {
                DeGroup vDeGroup = new DeGroup(vPageData);
                vDeGroup[DeProp.Index] = procIndex;
                vDeGroup[GroupProp.SubType] = SubType.Proc;

                if (propertys != "")
                {
                    string[] vStrings = propertys.Split(new string[] { HC.View.HC.sLineBreak }, StringSplitOptions.None);
                    for (int i = 0; i < vStrings.Length; i++)
                    {
                        if (vStrings[i] != "")
                        {
                            string[] vKv = vStrings[i].Split(new string[] { "=" }, StringSplitOptions.None);
                            if (vKv[0] != "")
                                vDeGroup[vKv[0]] = vKv[1];
                        }
                    }
                }

                FIgnoreAcceptAction = true;
                try
                {
                    int vStartNo = -1, vEndNo = -1, vSectionIndex = -1; ;

                    if (beforProcIndex != "")  // 在指定病程前面插入
                    {
                        if (GetProcItemNo(beforProcIndex, ref vSectionIndex, ref vStartNo, ref vEndNo))  // 有效
                        {
                            if (vSectionIndex != this.ActiveSectionIndex)
                                this.ActiveSectionIndex = vSectionIndex;

                            vSection = this.Sections[vSectionIndex];
                            vPageData = vSection.Page;
                            vPageData.SetSelectBound(vEndNo, HC.View.HC.OffsetAfter, vEndNo, HC.View.HC.OffsetAfter);
                        }
                        else
                            return false;
                    }
                    else  // 在最后追加
                        vPageData.SelectLastItemAfterWithCaret();

                    if (!vPageData.IsEmptyData())
                        this.InsertBreak();

                    this.ApplyParaAlignHorz(ParaAlignHorz.pahLeft);
                    vResult = this.InsertDeGroup(vDeGroup);

                    vEndNo = vPageData.SelectInfo.StartItemNo;
                    vPageData.SetSelectBound(vEndNo, HC.View.HC.OffsetBefor, vEndNo, HC.View.HC.OffsetBefor);
                    GetSectionCaretProcInfo(this.ActiveSection.Page, FCaretProcInfo);
                }
                finally
                {
                    FIgnoreAcceptAction = false;
                }
            }

            this.UpdateView();
            return vResult;
        }

        public bool DeleteProc(string procIndex)
        {
            if (procIndex == "")
                return false;

            int vStartNo = -1, vEndNo = -1, vSectionIndex = -1;
            if (GetProcItemNo(procIndex, ref vSectionIndex, ref vStartNo, ref vEndNo))
            {
                return this.ActiveSection.DataAction(this.ActiveSection.Page, delegate ()
                {
                    FIgnoreAcceptAction = true;
                    try
                    {
                        this.ActiveSection.Page.DeleteItems(vStartNo, vEndNo, false);
                    }
                    finally
                    {
                        FIgnoreAcceptAction = false;
                    }
                    return true;
                });
            }

            return false;
        }

        public void GetProcInfoAt(HCSectionData data, int itemNo, int offset, ProcInfo procInfo)
        {
            procInfo.Clear();

            if (itemNo < 0 || offset < 0)
                return;

            int vStartNo = itemNo;
            int vEndNo = itemNo;
            byte vLevel = 0;
            if (data.Items[itemNo] is DeGroup)  // 起始位置就是Group
            {
                if ((data.Items[itemNo] as DeGroup).IsProcBegin)  // 起始位置是起始标记
                {
                    if (offset == HC.View.HC.OffsetAfter)  // 光标在后面
                    {
                        procInfo.Data = data;
                        procInfo.BeginNo = itemNo;  // 当前即为起始标识
                        vLevel = (data.Items[itemNo] as DeGroup).Level;
                        vEndNo = itemNo + 1;
                    }
                    else  // 光标在前面
                    {
                        if (itemNo > 0)  // 不是第一个
                            vStartNo = itemNo - 1;  // 从前一个往前
                        else  // 是在第一个前面
                            return;  // 不用找了
                    }
                }
                else  // 查找位置是结束标记
                if ((data.Items[itemNo] as DeGroup).IsProcEnd)
                {
                    if (offset == HC.View.HC.OffsetAfter)  // 光标在后面
                    {
                        if (itemNo < data.Items.Count - 1)  // 不是最后一个
                            vEndNo = itemNo + 1;
                        else  // 是最后一个后面
                            return;  // 不用找了
                    }
                    else  // 光标在前面
                    {
                        procInfo.EndNo = itemNo;
                        vStartNo = itemNo - 1;
                    }
                }
            }

            if (procInfo.BeginNo < 0)  // 没找到起始
            {
                if (vStartNo < data.Items.Count / 2)  // 在前半程
                {
                    for (int i = vStartNo; i >= 0; i--)  // 先往前找起始
                    {
                        if (data.Items[i] is DeGroup)
                        {
                            if ((data.Items[i] as DeGroup).IsProcBegin)  // 起始标记
                            {
                                procInfo.Data = data;
                                procInfo.BeginNo = i;
                                vLevel = (data.Items[i] as DeGroup).Level;
                                break;
                            }
                        }
                    }

                    if ((procInfo.BeginNo >= 0) && (procInfo.EndNo < 0))  // 找结束标识
                    {
                        for (int i = vEndNo; i < data.Items.Count; i++)
                        {
                            if (data.Items[i] is DeGroup)
                            {
                                if ((data.Items[i] as DeGroup).IsProcEnd)  // 是结尾
                                {
                                    if ((data.Items[i] as DeGroup).Level == vLevel)
                                    {
                                        procInfo.EndNo = i;
                                        break;
                                    }
                                }
                            }
                        }

                        if (procInfo.EndNo < 0)
                            throw new Exception("异常：获取病程结束位置出错！");
                    }
                }
                else  // 在后半程
                {
                    for (int i = vEndNo; i < data.Items.Count; i++)  // 先往后找结束
                    {
                        if (data.Items[i] is DeGroup)
                        {
                            if ((data.Items[i] as DeGroup).IsProcEnd)  // 结束标记
                            {
                                procInfo.EndNo = i;
                                vLevel = (data.Items[i] as DeGroup).Level;
                                break;
                            }
                        }
                    }

                    if (procInfo.EndNo >= 0 && procInfo.BeginNo < 0)  // 找起始标识
                    {
                        for (int i = vStartNo; i >= 0; i--)
                        {
                            if (data.Items[i] is DeGroup)
                            {
                                if ((data.Items[i] as DeGroup).IsProcBegin)  // 是起始
                                {
                                    if ((data.Items[i] as DeGroup).Level == vLevel)
                                    {
                                        procInfo.Data = data;
                                        procInfo.BeginNo = i;
                                        break;
                                    }
                                }
                            }
                        }

                        if (procInfo.BeginNo < 0)
                            throw new Exception("异常：获取病程起始位置出错！");
                    }
                }
            }
            else
            if (procInfo.EndNo < 0)  // 找到起始了，找结束
            {
                for (int i = vEndNo; i < data.Items.Count; i++)
                {
                    if (data.Items[i] is DeGroup)
                    {
                        if ((data.Items[i] as DeGroup).IsProcEnd)  // 是结尾
                        {
                            if ((data.Items[i] as DeGroup).Level == vLevel)
                            {
                                procInfo.EndNo = i;
                                break;
                            }
                        }
                    }
                }

                if (procInfo.EndNo < 0)
                    throw new Exception("异常：获取病程结束位置出错！");
            }

            if (procInfo.EndNo > 0)
                procInfo.Index = (data.Items[procInfo.EndNo] as DeGroup)[GroupProp.Index];
        }

        public string GetCaretProcProperty(string propName)
        {
            if (FCaretProcInfo.EndNo > 0)
            {
                DeGroup vBeginGroup = this.ActiveSection.Page.Items[FCaretProcInfo.BeginNo] as DeGroup;

                if (propName == GroupProp.Name)
                    return vBeginGroup[DeProp.Name];
                else
                if (propName == GroupProp.Propertys)  // 批量属性一次处理
                    return DeProp.GetPropertyString(vBeginGroup.Propertys);
                else
                    return vBeginGroup[propName];
            }

            return "";
        }

        public string GetProcProperty(string procIndex, string propName)
        {
            int vBeginNo = -1, vEndNo = -1, vSectionIndex = -1;
            if (GetProcItemNo(procIndex, ref vSectionIndex, ref vBeginNo, ref vEndNo))
            {
                DeGroup vBeginGroup = this.Sections[vSectionIndex].Page.Items[vBeginNo] as DeGroup;

                if (propName == GroupProp.Name)
                    return vBeginGroup[DeProp.Name];
                else
                if (propName == GroupProp.Propertys)  // 批量属性一次处理
                    return DeProp.GetPropertyString(vBeginGroup.Propertys);
                else
                    return vBeginGroup[propName];
            }

            return "";
        }

        public bool SetProcProperty(string procIndex, string propName, string propValue)
        {
            int vBeginNo = -1, vEndNo = -1, vSectionIndex = -1;
            if (GetProcItemNo(procIndex, ref vSectionIndex, ref vBeginNo, ref vEndNo))
            {
                DeGroup vBeginGroup = this.Sections[vSectionIndex].Page.Items[vBeginNo] as DeGroup;
                DeGroup vEndGroup = this.Sections[vSectionIndex].Page.Items[vEndNo] as DeGroup;

                if (propName == GroupProp.Name)
                {
                    if (propValue != "")
                    {
                        vBeginGroup[DeProp.Name] = propValue;
                        vEndGroup[DeProp.Name] = propValue;
                    }
                }
                else
                if (propName == GroupProp.Propertys)  // 批量属性一次处理
                {
                    Dictionary<string, string> vPropertys = new Dictionary<string, string>();
                    DeProp.SetPropertyString(propValue, vPropertys);

                    foreach (KeyValuePair<string, string> kvp in vPropertys)
                    {
                        if (kvp.Key == GroupProp.Name)
                        {
                            if (kvp.Value != "")
                            {
                                vBeginGroup[DeProp.Name] = propValue;
                                vEndGroup[DeProp.Name] = propValue;
                            }
                        }
                        else
                        {
                            vBeginGroup[kvp.Key] = kvp.Value;
                            vEndGroup[kvp.Key] = kvp.Value;
                        }
                    }
                }
                else
                {
                    vBeginGroup[propName] = propValue;
                    vEndGroup[propName] = propValue;
                }

                return true;
            }

            return false;
        }

        public bool SetProcStream(string procIndex, Stream stream)
        {
            int vStartNo = -1, vEndNo = -1, vSectionIndex = -1;
            if (GetProcItemNo(procIndex, ref vSectionIndex, ref vStartNo, ref vEndNo))
            {
                if (this.ActiveSectionIndex != vSectionIndex)
                    this.ActiveSectionIndex = vSectionIndex;

                this.ActiveSection.DataAction(this.ActiveSection.Page, delegate ()
                {
                    this.ActiveSection.Page.SetSelectBound(vStartNo, HC.View.HC.OffsetAfter, vEndNo, HC.View.HC.OffsetBefor);
                    FIgnoreAcceptAction = true;
                    try
                    {
                        this.InsertStream(stream);
                    }
                    finally
                    {
                        FIgnoreAcceptAction = false;
                    }

                    return true;
                });

                return true;
            }

            return false;
        }

        public void SetEditProcIndex(string value)
        {
            if (FEditProcIndex != value)
            {
                FEditProcIndex = value;
                FEditProcInfo.Clear();
                int vBeginNo = -1, vEndNo = -1, vSectionIndex = -1;
                GetProcItemNo(value, ref vSectionIndex, ref vBeginNo, ref vEndNo);
                if (vEndNo > 0)
                {
                    if (this.ActiveSectionIndex != vSectionIndex)
                        this.ActiveSectionIndex = vSectionIndex;

                    FEditProcInfo.SectionIndex = vSectionIndex;
                    FEditProcInfo.Data = this.ActiveSection.Page;
                    FEditProcInfo.BeginNo = vBeginNo;
                    FEditProcInfo.EndNo = vEndNo;
                    FEditProcInfo.Index = value;
                }

                this.UpdateView();
            }
        }

        public bool GetProcItemNo(string procIndex, ref int sectionIndex, ref int startNo, ref int endNo)
        {
            bool vResult = false;
            startNo = -1;
            endNo = -1;
            sectionIndex = -1;

            HCSectionData vData = null;
            for (int i = 0; i < this.Sections.Count; i++)
            {
                vData = this.Sections[i].Page;
                for (int j = 0; i < vData.Items.Count; i++)
                {
                    if ((vData.Items[j] is DeGroup) && ((vData.Items[j] as DeGroup)[DeProp.Index] == procIndex))
                    {
                        sectionIndex = i;
                        startNo = i;
                        break;
                    }
                }
            }

            if (startNo >= 0)
            {
                endNo = vData.GetDomainAnother(startNo);
                vResult = endNo >= 0;
            }

            return vResult;
        }

        public void GetSectionCaretProcInfo(HCSectionData data, ProcInfo procInfo)
        {
            GetProcInfoAt(data, data.SelectInfo.StartItemNo, data.SelectInfo.StartItemOffset, procInfo);
        }
#endif

        /// <summary> 直接设置当前数据元的值为扩展内容 </summary>
        /// <param name="aStream">扩展内容流</param>
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

        public bool CheckDeGroupStart(HCViewData aData, int aItemNo, string aDeIndex)
        {
            bool vResult = false;
            if (aData.Items[aItemNo] is DeGroup)
            {
                DeGroup vDeGroup = aData.Items[aItemNo] as DeGroup;
                vResult = (vDeGroup.MarkType == MarkType.cmtBeg) && (vDeGroup[DeProp.Index] == aDeIndex);
            }

            return vResult;
        }

        public bool CheckDeGroupEnd(HCViewData aData, int aItemNo, string aDeIndex)
        {
            bool vResult = false;
            if (aData.Items[aItemNo] is DeGroup)
            {
                DeGroup vDeGroup = aData.Items[aItemNo] as DeGroup;
                vResult = (vDeGroup.MarkType == MarkType.cmtEnd) && (vDeGroup[DeProp.Index] == aDeIndex);
            }

            return vResult;
        }

        public void GetDataDeGroupItemNo(HCViewData aData, string aDeIndex, bool aForward, ref int aStartNo, ref int aEndNo)
        {
            aEndNo = -1;
            int vBeginNo = -1;
            int vEndNo = -1;

            if (aStartNo < 0)
                aStartNo = 0;

            if (aForward)  // 从AStartNo往前找
            {
                for (int i = aStartNo; i >= 0; i--)  // 找结尾ItemNo
                {
                    if (CheckDeGroupEnd(aData, i, aDeIndex))
                    {
                        vEndNo = i;
                        break;
                    }
                }

                if (vEndNo >= 0)  // 再往前找起始ItemNo
                {
                    for (int i = vEndNo - 1; i >= 0; i--)
                    {
                        if (CheckDeGroupStart(aData, i, aDeIndex))
                        {
                            vBeginNo = i;
                            break;
                        }
                    }
                }
            }
            else  // 从AStartNo往后找
            {
                for (int i = aStartNo; i < aData.Items.Count; i++)  // 找起始ItemNo
                {
                    if (CheckDeGroupStart(aData, i, aDeIndex))
                    {
                        vBeginNo = i;
                        break;
                    }
                }

                if (vBeginNo >= 0)  // 找结尾ItemNo
                {
                    for (int i = vBeginNo + 1; i < aData.Items.Count; i++)
                    {
                        if (CheckDeGroupEnd(aData, i, aDeIndex))
                        {
                            vEndNo = i;
                            break;
                        }
                    }
                }
            }

            if ((vBeginNo >= 0) && (vEndNo >= 0))
            {
                aStartNo = vBeginNo;
                aEndNo = vEndNo;
            }
            else
                aStartNo = -1;
        }

        /// <summary> 获取指定数据组中的文本内容 </summary>
        /// <param name="AData">指定从哪个Data里获取</param>
        /// <param name="ADeGroupStartNo">指定数据组的起始ItemNo</param>
        /// <param name="ADeGroupEndNo">指定数据组的结束ItemNo</param>
        /// <returns>数据组文本内容</returns>
        public string GetDataDeGroupText(HCViewData aData, int aDeGroupStartNo, int aDeGroupEndNo)
        {
            string Result = "";
            for (int i = aDeGroupStartNo + 1; i <= aDeGroupEndNo - 1; i++)
            {
                if (aData.Items[i].ParaFirst)
                    Result = Result + HC.View.HC.sLineBreak + aData.Items[i].Text;
                else
                    Result = Result + aData.Items[i].Text;
            }

            return Result;
        }

        /// <summary> 从当前数据组起始位置往前找相同数据组的内容Index域内容 </summary>
        /// <param name="AData">指定从哪个Data里获取</param>
        /// <param name="ADeGroupStartNo">指定从哪个位置开始往前找</param>
        /// <returns>相同数据组文本形式的内容</returns>
        public string GetDataForwardDeGroupText(HCViewData aData, int aDeGroupStartNo)
        {
            string Result = "";

            int vBeginNo = aDeGroupStartNo;
            int vEndNo = -1;
            string vDeIndex = (aData.Items[aDeGroupStartNo] as DeGroup)[DeProp.Index];

            GetDataDeGroupItemNo(aData, vDeIndex, true, ref vBeginNo, ref vEndNo);
            if (vEndNo > 0)
                Result = GetDataDeGroupText(aData, vBeginNo, vEndNo);

            return Result;
        }

        /// <summary> 设置数据组的内容为指定的文本 </summary>
        /// <param name="aData">数据组所在的Data</param>
        /// <param name="aDeGroupNo">数据组的ItemNo</param>
        /// <param name="aText">文本内容</param>
        public void SetDataDeGroupText(HCViewData aData, int aDeGroupNo, string aText)
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
            FIgnoreAcceptAction = true;
            try
            {
                if (aText != "")
                    aData.InsertText(aText);
                else
                    aData.DeleteSelected();
            }
            finally
            {
                FIgnoreAcceptAction = false;
            }
        }

        public void SetActionDataDeGroupText(HCSection section, SectionArea area, string deIndex, string text, bool startLast = true)
        {
            int vStartNo = -1, vEndNo = -1;
            HCSectionData vData = null;
            switch (area)
            {
                case SectionArea.saHeader:
                    vData = section.Header;
                    break;

                case SectionArea.saPage:
                    vData = section.Page;
                    break;

                case SectionArea.saFooter:
                    vData = section.Footer;
                    break;
            }

            if (startLast)
            {
#if PROCSERIES
                if (FEditProcIndex != "")
                    vStartNo = FEditProcInfo.EndNo;
                else
#endif
                    vStartNo = vData.Items.Count - 1;
                GetDataDeGroupItemNo(vData, deIndex, true, ref vStartNo, ref vEndNo);
            }
            else
            {
#if PROCSERIES
                if (FEditProcIndex != "")
                    vStartNo = FEditProcInfo.BeginNo;
                else
#endif
                    GetDataDeGroupItemNo(vData, deIndex, false, ref vStartNo, ref vEndNo);
            }

            if (vEndNo > 0)
            {
#if PROCSERIES
                if (FEditProcIndex != "")
                {
                    if ((vStartNo < FEditProcInfo.BeginNo) || (vEndNo > FEditProcInfo.EndNo))
                        return;
                }
#endif
                section.DataAction(vData, delegate ()
                {
                    vData.SetSelectBound(vStartNo, HC.View.HC.OffsetAfter, vEndNo, HC.View.HC.OffsetBefor);
                    FIgnoreAcceptAction = true;
                    try
                    {
                        if (text != "")
                            vData.InsertText(text);
                        else
                            vData.DeleteSelected();
                    }
                    finally
                    {
                        FIgnoreAcceptAction = false;
                    }

                    return true;
                });
            }
        }
        public void SetActionDataDeGroupStream(HCSection section, SectionArea area, string deIndex, Stream stream, bool startLast = true)
        {
            int vStartNo = -1, vEndNo = -1;
            HCSectionData vData = null;
            switch (area)
            {
                case SectionArea.saHeader:
                    vData = section.Header;
                    break;

                case SectionArea.saPage:
                    vData = section.Page;
                    break;

                case SectionArea.saFooter:
                    vData = section.Footer;
                    break;
            }

            if (startLast)
            {
#if PROCSERIES
                if (FEditProcIndex != "")
                    vStartNo = FEditProcInfo.EndNo;
                else
#endif
                    vStartNo = vData.Items.Count - 1;
                GetDataDeGroupItemNo(vData, deIndex, true, ref vStartNo, ref vEndNo);
            }
            else
            {
#if PROCSERIES
                if (FEditProcIndex != "")
                    vStartNo = FEditProcInfo.BeginNo;
                else
#endif
                    GetDataDeGroupItemNo(vData, deIndex, false, ref vStartNo, ref vEndNo);
            }

            if (vEndNo > 0)
            {
#if PROCSERIES
                if (FEditProcIndex != "")
                {
                    if ((vStartNo < FEditProcInfo.BeginNo) || (vEndNo > FEditProcInfo.EndNo))
                        return;
                }
#endif
                section.DataAction(vData, delegate ()
                {
                    vData.SetSelectBound(vStartNo, HC.View.HC.OffsetAfter, vEndNo, HC.View.HC.OffsetBefor);
                    FIgnoreAcceptAction = true;
                    try
                    {
                        this.InsertStream(stream);
                        //vData.InsertStream(AStream);
                    }
                    finally
                    {
                        FIgnoreAcceptAction = false;
                    }

                    return true;
                });
            }
        }

        public void GetDataDeGroupToStream(HCViewData aData, int aDeGroupStartNo, int aDeGroupEndNo, Stream aStream)
        {
            HC.View.HC._SaveFileFormatAndVersion(aStream);  // 文件格式和版本
            this.Style.SaveToStream(aStream);
            aData.SaveItemToStream(aStream, aDeGroupStartNo + 1, 0, aDeGroupEndNo - 1, aData.Items[aDeGroupEndNo - 1].Length);
        }

        public void SetDataDeGroupFromStream(HCViewData aData, int aDeGroupStartNo, int aDeGroupEndNo, Stream aStream)
        {
            FIgnoreAcceptAction = true;
            try
            {
                this.BeginUpdate();
                try
                {
                    aData.BeginFormat();
                    try
                    {
                        if (aDeGroupEndNo - aDeGroupStartNo > 1)  // 中间有内容
                            aData.DeleteItems(aDeGroupStartNo + 1, aDeGroupEndNo - aDeGroupStartNo - 1, false);
                        else
                            aData.SetSelectBound(aDeGroupStartNo, HC.View.HC.OffsetAfter, aDeGroupStartNo, HC.View.HC.OffsetAfter);

                        aStream.Position = 0;
                        string vFileExt = "";
                        ushort viVersion = 0;
                        byte vLang = 0;
                        HC.View.HC._LoadFileFormatAndVersion(aStream, ref vFileExt, ref viVersion, ref vLang);  // 文件格式和版本
                        HCStyle vStyle = new HCStyle();
                        vStyle.LoadFromStream(aStream, viVersion);
                        aData.InsertStream(aStream, vStyle, viVersion);
                    }
                    finally
                    {
                        aData.EndFormat(false);
                    }

                    this.FormatData();
                }
                finally
                {
                    this.EndUpdate();
                }
            }
            finally
            {
                FIgnoreAcceptAction = false;
            }
        }

        public void SyntaxCheck()
        {
            if (FOnSyntaxCheck == null)
                return;

            HCItemTraverse vItemTraverse = new HCItemTraverse();
            vItemTraverse.Tag = 0;
            vItemTraverse.Areas.Add(SectionArea.saPage);
            vItemTraverse.Process = DoSyntaxCheck;
            this.TraverseItem(vItemTraverse);
            this.UpdateView();
        }

        [DllImport("HCExpPDF.dll")]
        public static extern void EmrSaveToPDFStream(ref object inobj, out object outobj);
        public override void SaveToPDFStream(Stream stream)
        {
            using (MemoryStream vFileStream = new MemoryStream())
            {
                HashSet<SectionArea> vParts = new HashSet<SectionArea> { SectionArea.saHeader, SectionArea.saPage, SectionArea.saFooter };
                this.SaveToStream(vFileStream, true, vParts);
                vFileStream.Position = 0;
                byte[] bytes = new byte[vFileStream.Length];
                vFileStream.Read(bytes, 0, bytes.Length);
                object vInObj = (object)bytes;

                object oubOjb = null;
                EmrSaveToPDFStream(ref vInObj, out oubOjb);
                if (oubOjb != null)
                {
                    byte[] vOutBytes = oubOjb as byte[];
                    stream.Write(vOutBytes, 0, vOutBytes.Length);
                }
            }
        }

        /// <summary> 文档是否处于设计模式 </summary>
        public bool DesignModeEx
        {
            get { return FDesignMode; }
            set { FDesignMode = value; }
        }

        /// <summary> 是否隐藏痕迹 </summary>
        public bool HideTrace
        {
            get { return FHideTrace; }
            set { SetHideTrace(value); }
        }

        /// <summary> 是否处于留痕状态 </summary>
        public bool Trace
        {
            get { return FTrace; }
            set { FTrace = value; }
        }

        /// <summary> 文档中有几处痕迹 </summary>
        public int TraceCount
        {
            get { return FTraceCount; }
        }

        #if PROCSERIES
        public int ProcCount
        {
            get { return FProcCount; }
        }

        public string EditProcIndex
        {
            get { return FEditProcIndex; }
            set { SetEditProcIndex(value); }
        }

        public bool ShowProcSplit
        {
            get { return FShowProcSplit; }
            set { FShowProcSplit = value; }
        }
        #endif

        public bool IgnoreAcceptAction
        {
            get { return FIgnoreAcceptAction; }
            set { IgnoreAcceptAction = value; }
        }

        public bool Secret
        {
            get { return FSecret; }
            set { FSecret = value; }
        }

        public bool TraceInfoAnnotate
        {
            get { return FTraceInfoAnnotate; }
            set { FTraceInfoAnnotate = value; }
        }

        public string PageBlankTip
        {
            get { return FPageBlankTip; }
            set { SetPageBlankTip(value); }
        }

        public Color DeDoneColor
        {
            get { return FDeDoneColor; }
            set { FDeDoneColor = value; }
        }

        public Color DeUnDoneColor
        {
            get { return FDeUnDoneColor; }
            set { FDeUnDoneColor = value; }
        }

        public Color DeHotColor
        {
            get { return FDeHotColor; }
            set { FDeHotColor = value; }
        }

        /// <summary> 当编辑只读状态的Data时触发 </summary>
        public EventHandler OnCanNotEdit
        {
            get { return FOnCanNotEdit; }
            set { FOnCanNotEdit = value; }
        }

        /// <summary> 复制内容前触发 </summary>
        public HCCopyPasteEventHandler OnCopyRequest
        {
            get { return FOnCopyRequest; }
            set { FOnCopyRequest = value; }
        }

        /// <summary> 粘贴内容前触发 </summary>
        public HCCopyPasteEventHandler OnPasteRequest
        {
            get { return FOnPasteRequest; }
            set { FOnPasteRequest = value; }
        }

        public HCCopyPasteStreamEventHandler OnCopyAsStream
        {
            get { return FOnCopyAsStream; }
            set { FOnCopyAsStream = value; }
        }

        public HCCopyPasteStreamEventHandler OnPasteFromStream
        {
            get { return FOnPasteFromStream; }
            set { FOnPasteFromStream = value; }
        }

        /// <summary> 数据元需要同步内容时触发 </summary>
        public SyncDeItemEventHandle OnSyncDeItem
        {
            get { return FOnSyncDeItem; }
            set { FOnSyncDeItem = value; }
        }

        /// <summary> 数据元需要用语法检测器来检测时触发 </summary>
        public DataDomainItemNoEventHandler OnSyntaxCheck
        {
            get { return FOnSyntaxCheck; }
            set { FOnSyntaxCheck = value; }
        }

        /// <summary> 数据元绘制语法问题时触发 </summary>
        public SyntaxPaintEventHandler OnSyntaxPaint
        {
            get { return FOnSyntaxPaint; }
            set { FOnSyntaxPaint = value; }
        }
    }
}
