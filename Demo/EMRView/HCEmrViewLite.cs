using HC.View;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EMRView
{
    public delegate void HCImportAsTextEventHandler(string aText);

    public class HCEmrViewLite : HCViewLite
    {
        private Dictionary<string, string> FPropertys = new Dictionary<string, string>();

        protected override void Create()
        {
            HCTextItem.HCDefaultTextItemClass = typeof(DeItem);
            HCDomainItem.HCDefaultDomainItemClass = typeof(DeGroup);
        }

        protected override HCCustomItem DoSectionCreateStyleItem(HCCustomData aData, int aStyleNo)
        {
            return CreateEmrStyleItem(aData, aStyleNo);
        }

        protected override void DoLoadStreamBefor(Stream stream, ushort fileVersion)
        {
            byte vVersion = 0;
            if (fileVersion > 43)
                vVersion = (byte)stream.ReadByte();

            if (vVersion > 0)
            {
                string vS = "";
                HC.View.HC.HCLoadTextFromStream(stream, ref vS, fileVersion);
                if (this.Style.States.Contain(HCState.hosLoading))
                    HC.View.HC.SetPropertyString(vS, FPropertys);
            }
            else
            if (this.Style.States.Contain(HCState.hosLoading))
                FPropertys.Clear();

            base.DoLoadStreamBefor(stream, fileVersion);
        }

        protected override void DoSaveStreamBefor(Stream stream)
        {
            stream.WriteByte(EMR.EmrViewVersion);
            HC.View.HC.HCSaveTextToStream(stream, HC.View.HC.GetPropertyString(FPropertys));
            base.DoSaveStreamBefor(stream);
        }

        #region 兼容付费版本的多边距和页眉页脚首页不同
        protected override void DoSaveMutMargin(Stream stream)
        {
            int a = 0;
            byte[] vBuffer1 = BitConverter.GetBytes(a);
            stream.Write(vBuffer1, 0, vBuffer1.Length);
            stream.Write(vBuffer1, 0, vBuffer1.Length);
            stream.Write(vBuffer1, 0, vBuffer1.Length);
            stream.WriteByte(0);
        }

        protected override void DoLoadMutMargin(Stream stream, HCStyle style, ushort fileVersion)
        {
            if (fileVersion > 61)
            {
                int a = 0;
                byte[] vBuffer1 = BitConverter.GetBytes(a);
                stream.Read(vBuffer1, 0, vBuffer1.Length);
                stream.Read(vBuffer1, 0, vBuffer1.Length);
                stream.Read(vBuffer1, 0, vBuffer1.Length);
                stream.ReadByte();
            }
        }
        #endregion

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

                case HCStyle.Button:
                    return new DeButton(aData, "");

                case HCStyle.RadioGroup:
                    return new DeRadioGroup(aData);

                case EMR.EMRSTYLE_YUEJING:
                    return new EmrYueJingItem(aData, "", "", "", "");

                case EMR.EMRSTYLE_TOOTH:
                    return new EmrToothItem(aData, "", "", "", "");

                case EMR.EMRSTYLE_FANGJIAO:
                    return new EmrFangJiaoItem(aData, "", "", "", "");

                case HCStyle.BarCode:
                    return new DeBarCodeItem(aData, "");

                case HCStyle.QRCode:
                    return new DeQRCodeItem(aData, "");

                case HCStyle.FloatBarCode:
                    return new DeFloatBarCodeItem(aData);

                case HCStyle.Image:
                    return new DeImageItem(aData);

                default:
                    return null;
            }
        }

        public void TraverseItem(HCItemTraverse traverse)
        {
            if (traverse.Areas.Count == 0)
                return;

            for (int i = 0; i < this.Sections.Count; i++)
            {
                if (!traverse.Stop)
                {
                    if (traverse.Areas.Contains(SectionArea.saHeader))
                        this.Sections[i].Header.TraverseItem(traverse);

                    if ((!traverse.Stop) && (traverse.Areas.Contains(SectionArea.saPage)))
                        this.Sections[i].Page.TraverseItem(traverse);

                    if ((!traverse.Stop) && (traverse.Areas.Contains(SectionArea.saFooter)))
                        this.Sections[i].Footer.TraverseItem(traverse);
                }
            }
        }
    }
}
