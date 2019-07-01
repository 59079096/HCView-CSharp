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
using System.Drawing;
using HC.Win32;

namespace EMRView
{
    public class EmrFangJiaoItem : HCExpressItem
    {
        private bool FMouseIn;

        protected override void DoPaint(HCStyle aStyle, RECT aDrawRect, int aDataDrawTop, int aDataDrawBottom, int aDataScreenTop, int aDataScreenBottom, HCCanvas aCanvas, PaintInfo aPaintInfo)
        {
            if (this.Active && (!aPaintInfo.Print))
            {
                aCanvas.Brush.Color = HC.View.HC.clBtnFace;
                aCanvas.FillRect(aDrawRect);
            }

            aStyle.TextStyles[TextStyleNo].ApplyStyle(aCanvas, aPaintInfo.ScaleY / aPaintInfo.Zoom);
            aCanvas.TextOut(aDrawRect.Left + LeftRect.Left,   aDrawRect.Top + LeftRect.Top,   LeftText);
            aCanvas.TextOut(aDrawRect.Left + TopRect.Left,    aDrawRect.Top + TopRect.Top,    TopText);
            aCanvas.TextOut(aDrawRect.Left + RightRect.Left,  aDrawRect.Top + RightRect.Top,  RightText);
            aCanvas.TextOut(aDrawRect.Left + BottomRect.Left, aDrawRect.Top + BottomRect.Top, BottomText);

            aCanvas.Pen.Color = Color.Black;
            aCanvas.DrawLine(aDrawRect.Left, aDrawRect.Top, aDrawRect.Right, aDrawRect.Bottom);
            aCanvas.DrawLine(aDrawRect.Right, aDrawRect.Top, aDrawRect.Left, aDrawRect.Bottom);

            if (!aPaintInfo.Print)
            {
                RECT vFocusRect = new RECT();

                if (FMouseIn)
                {
                    aCanvas.Pen.Color = Color.Gray;

                    vFocusRect = LeftRect;
                    vFocusRect.Offset(aDrawRect.Left, aDrawRect.Top);
                    vFocusRect.Inflate(2, 2);
                    aCanvas.Rectangle(vFocusRect);

                    vFocusRect = TopRect;
                    vFocusRect.Offset(aDrawRect.Left, aDrawRect.Top);
                    vFocusRect.Inflate(2, 2);
                    aCanvas.Rectangle(vFocusRect);

                    vFocusRect = RightRect;
                    vFocusRect.Offset(aDrawRect.Left, aDrawRect.Top);
                    vFocusRect.Inflate(2, 2);
                    aCanvas.Rectangle(vFocusRect);

                    vFocusRect = BottomRect;
                    vFocusRect.Offset(aDrawRect.Left, aDrawRect.Top);
                    vFocusRect.Inflate(2, 2);
                    aCanvas.Rectangle(vFocusRect);                    
                }

                if (FActiveArea != ExpressArea.ceaNone)
                {
                    switch (FActiveArea)
                    {
                        case ExpressArea.ceaLeft:
                            vFocusRect = LeftRect;
                            break;

                        case ExpressArea.ceaTop:
                            vFocusRect = TopRect;
                            break;

                        case ExpressArea.ceaRight:
                            vFocusRect = RightRect;
                            break;

                        default:
                            vFocusRect = BottomRect;
                            break;
                    }

                    vFocusRect.Offset(aDrawRect.Left, aDrawRect.Top);
                    vFocusRect.Inflate(2, 2);
                    aCanvas.Pen.Color = Color.Blue;
                    aCanvas.Rectangle(vFocusRect);
                }
            }
        }

        public EmrFangJiaoItem(HCCustomData aOwnerData, string aLeftText, string aTopText, string aRightText, string aBottomText)
            : base(aOwnerData, aLeftText, aTopText, aRightText, aBottomText)
        {
            this.StyleNo = EMR.EMRSTYLE_FANGJIAO;
        }

        public override void MouseEnter()
        {
            base.MouseEnter();
            FMouseIn = true;
        }

        public override void MouseLeave()
        {
            base.MouseLeave();
            FMouseIn = false;
        }

        public void ToXmlEmr(XmlElement aNode)
        {
            aNode.SetAttribute("DeCode", EMR.EMRSTYLE_FANGJIAO.ToString());
            aNode.SetAttribute("toptext", TopText);
            aNode.SetAttribute("bottomtext", BottomText);
            aNode.SetAttribute("lefttext", LeftText);
            aNode.SetAttribute("righttext", RightText);
        }

        public void ParseXmlEmr(XmlElement aNode)
        {
            if (aNode.Attributes["DeCode"].Value == EMR.EMRSTYLE_FANGJIAO.ToString())
            {
                TopText = aNode.Attributes["toptext"].Value;
                BottomText = aNode.Attributes["bottomtext"].Value;
                LeftText = aNode.Attributes["lefttext"].Value;
                RightText = aNode.Attributes["righttext"].Value;
            }
        }
    }
}
