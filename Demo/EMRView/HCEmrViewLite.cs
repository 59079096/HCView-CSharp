using HC.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EMRView
{
    public delegate void HCImportAsTextEventHandler(string aText);

    public class HCEmrViewLite : HCView
    {

        protected override void Create()
        {
            HCTextItem.HCDefaultTextItemClass = typeof(DeItem);
            HCDomainItem.HCDefaultDomainItemClass = typeof(DeGroup);
        }

        protected override HCCustomItem DoSectionCreateStyleItem(HCCustomData aData, int aStyleNo)
        {
            return CreateEmrStyleItem(aData, aStyleNo);
        }

        /// <summary> 创建指定样式的Item </summary>
        /// <param name="aData">要创建Item的Data</param>
        /// <param name="aStyleNo">要创建的Item样式</param>
        /// <returns>创建好的Item</returns>
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

        public static HCCustomFloatItem CreateEmrFloatStyleItem(HCSectionData aData, int aStyleNo)
        {
            switch (aStyleNo)
            {
                case HCStyle.FloatBarCode:
                    return new DeFloatBarCodeItem(aData);

                default:
                    return null;
            }
        }
    }
}
