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
    }
}
