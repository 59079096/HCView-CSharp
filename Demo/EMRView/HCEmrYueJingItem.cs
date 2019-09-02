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
using System.Xml;

namespace EMRView
{
    public class EmrYueJingItem : HCExpressItem
    {
        private string GetMenarcheAge()
        {
            return this.LeftText;
        }

        private void SetMenarcheAge(string value)
        {
            this.LeftText = value;
        }

        private string GetMenstrualDuration()
        {
            return this.TopText;
        }
        private void SetMenstrualDuration(string value)
        {
            this.TopText = value;
        }

        private string GetMenstrualCycle()
        {
            return this.BottomText;
        }

        private void SetMenstrualCycle(string value)
        {
            this.BottomText = value;
        }

        private string GetMenstrualPause()
        {
            return this.RightText;
        }

        private void SetMenstrualPause(string value)
        {
            this.RightText = value;
        }

        public EmrYueJingItem(HCCustomData aOwnerData, string aLeftText, string aTopText, string aRightText, string aBottomText)
            : base(aOwnerData, aLeftText, aTopText, aRightText, aBottomText)
        {
            this.StyleNo = EMR.EMRSTYLE_YUEJING;
        }

        public void ToXmlEmr(XmlElement aNode)
        {
            aNode.SetAttribute("DeCode", EMR.EMRSTYLE_YUEJING.ToString());
            aNode.SetAttribute("toptext", TopText);
            aNode.SetAttribute("bottomtext", BottomText);
            aNode.SetAttribute("lefttext", LeftText);
            aNode.SetAttribute("righttext", RightText);
        }

        public void ParseXmlEmr(XmlElement aNode)
        {
            if (aNode.Attributes["DeCode"].Value == EMR.EMRSTYLE_YUEJING.ToString())
            {
                TopText = aNode.Attributes["toptext"].Value;
                BottomText = aNode.Attributes["bottomtext"].Value;
                LeftText = aNode.Attributes["lefttext"].Value;
                RightText = aNode.Attributes["righttext"].Value;
            }
        }

        /// <summary> 初潮年龄 </summary>
        public string MenarcheAge
        {
            get { return GetMenarcheAge(); }
            set { SetMenarcheAge(value); }
        }

        /// <summary> 月经持续天数 </summary>
        public string MenstrualDuration
        {
            get { return GetMenstrualDuration(); }
            set { SetMenstrualDuration(value); }
        }

        /// <summary> 月经周期 </summary>
        public string MenstrualCycle
        {
            get { return GetMenstrualCycle(); }
            set { SetMenstrualCycle(value); }
        }

        /// <summary> 绝经年龄 </summary>
        public string MenstrualPause
        {
            get { return GetMenstrualPause(); }
            set { SetMenstrualPause(value); }
        }
    }
}
