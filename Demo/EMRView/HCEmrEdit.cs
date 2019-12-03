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

        protected override HCCustomItem DoDataCreateStyleItem(HCCustomData aData, int aStyleNo)
        {
            return HCEmrViewLite.CreateEmrStyleItem(aData, aStyleNo);
        }

        protected override void DoDrawItemPaintBefor(HCCustomData aData, int aItemNo, int aDrawItemNo,
            RECT aDrawRect, int aDataDrawLeft, int aDataDrawRight, int aDataDrawBottom, int aDataScreenTop, int aDataScreenBottom,
            HCCanvas aCanvas, PaintInfo aPaintInfo)
        {
            if ((!aPaintInfo.Print) && (aData.Items[aItemNo] is DeItem))
            {
                DeItem vDeItem = aData.Items[aItemNo] as DeItem;
                if (vDeItem.IsElement)  // 是数据元
                {
                    if ((vDeItem.MouseIn) || (vDeItem.Active))  // 鼠标移入和光标
                    {
                        if ((vDeItem.IsSelectPart) || (vDeItem.IsSelectComplate))
                        {

                        }
                        else
                        {
                            if (vDeItem[DeProp.Name] != vDeItem.Text)  // 已经填写过了
                                aCanvas.Brush.Color = FDeDoneColor;
                            else  // 没填写过
                                aCanvas.Brush.Color = FDeUnDoneColor;

                            aCanvas.FillRect(aDrawRect);
                        }
                    }
                }
                else  // 不是数据元
                {
                    if (FDesignMode && vDeItem.EditProtect)
                    {
                        aCanvas.Brush.Color = HC.View.HC.clBtnFace;
                        aCanvas.FillRect(aDrawRect);
                    }
                }
            }
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
