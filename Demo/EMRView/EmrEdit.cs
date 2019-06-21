using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HC.View;
using System.Drawing;
using HC.Win32;

namespace EMRView
{
    public class emrEdit : HCEdit
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

        public emrEdit()
        {
            HCTextItem.HCDefaultTextItemClass = typeof(DeItem);
            HCDomainItem.HCDefaultDomainItemClass = typeof(DeGroup);
            this.Width = 100;
            this.Height = 100;
            FDeDoneColor = HC.View.HC.clBtnFace;
            FDeUnDoneColor = Color.FromArgb(0xFF, 0xDD, 0x80);
        }

        public bool DesignMode
        {
            get { return DesignMode; }
            set { DesignMode = value; }
        }
    }
}
