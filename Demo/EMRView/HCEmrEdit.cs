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
using HC.View;
using System.Drawing;
using HC.Win32;
using System.IO;

namespace EMRView
{
    public class HCEmrEdit : HCEdit
    {
        private Color FDeDoneColor, FDeUnDoneColor;
        private bool FDesignMode;

        private void DoDeItemPaintBKG(object sender, HCCanvas aCanvas, RECT aDrawRect, PaintInfo aPaintInfo)
        {
            if (!aPaintInfo.Print)
            {
                DeItem vDeItem = sender as DeItem;
                if (vDeItem.IsElement)
                {
                    if ((vDeItem.MouseIn) || (vDeItem.Active))
                    {
                        if ((vDeItem.IsSelectPart) || (vDeItem.IsSelectComplate))
                        {

                        }
                        else
                        {
                            if (vDeItem[DeProp.Name] != vDeItem.Text)
                                aCanvas.Brush.Color = FDeDoneColor;
                            else
                                aCanvas.Brush.Color = FDeUnDoneColor;

                            aCanvas.FillRect(aDrawRect);
                        }
                    }
                }
                else
                {
                    if (FDesignMode && vDeItem.EditProtect)
                    {
                        aCanvas.Brush.Color = HC.View.HC.clBtnFace;
                        aCanvas.FillRect(aDrawRect);
                    }
                }
            }
        }

        protected override HCCustomItem DoDataCreateStyleItem(HCCustomData aData, int aStyleNo)
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

        protected override void DoDataInsertItem(HCCustomData aData, HCCustomItem aItem)
        {
            if (aItem is DeItem)
            {
                (aItem as DeItem).OnPaintBKG = DoDeItemPaintBKG;
            }

            base.DoDataInsertItem(aData, aItem);
        }

        public HCEmrEdit()
        {
            HCTextItem.HCDefaultTextItemClass = typeof(DeItem);
            HCDomainItem.HCDefaultDomainItemClass = typeof(DeGroup);
            this.Width = 100;
            this.Height = 100;
            FDeDoneColor = HC.View.HC.clBtnFace;
            FDeUnDoneColor = Color.FromArgb(0xFF, 0xDD, 0x80);
        }

        public DeItem NewDeItem(string atext)
        {
            DeItem vDeItem = new DeItem(atext);
            if (this.CurStyleNo > HCStyle.Null)
                vDeItem.StyleNo = this.CurStyleNo;
            else
                vDeItem.StyleNo = 0;

            vDeItem.ParaNo = this.CurParaNo;
            return vDeItem;
        }

        public bool InsertDeGroup(DeGroup aDeGroup)
        {
            return this.InsertDomain(aDeGroup);
        }

        public bool InsertDeItem(DeItem aDeItem)
        {
            return this.InsertItem(aDeItem);
        }

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
                        this.Data.DeleteActiveDataItems(this.Data.SelectInfo.StartItemNo,
                            this.Data.SelectInfo.StartItemNo, true);

                        this.Data.InsertStream(aStream, vStyle, vFileVersion);
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

        public bool DesignModeEx
        {
            get { return FDesignMode; }
            set { FDesignMode = value; }
        }
    }
}
