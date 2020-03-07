/*******************************************************}
{                                                       }
{               HCView V1.1  作者：荆通                 }
{                                                       }
{      本代码遵循BSD协议，你可以加入QQ群 649023932      }
{            来获取更多的技术交流 2018-5-4              }
{                                                       }
{           文本类的HCItem样式匹配处理单元              }
{                                                       }
{*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace HC.View
{
    public delegate void OnTextStyle(int aCurStyleNo, HCTextStyle aWillStyle);

    public abstract class HCStyleMatch : Object  // 文本样式匹配类
    {
        private bool FAppend = false;
        private bool FLock;
        private OnTextStyle FOnTextStyle;

        protected void SetAppend(bool value)
        {
            if ((FAppend != value) && (!FLock))
                FAppend = value;
                
            FLock = true;
        }

        protected abstract bool DoMatchCur(HCTextStyle aTextStyle);
        protected abstract void DoMatchNew(HCTextStyle aTextStyle);

        public HCStyleMatch()
        {
            FLock = false;
        }

        public int GetMatchStyleNo(HCStyle aStyle, int aCurStyleNo)
        {
            if (DoMatchCur(aStyle.TextStyles[aCurStyleNo]))
                return aCurStyleNo;

            using (HCTextStyle vTextStyle = new HCTextStyle())
            {
                vTextStyle.AssignEx(aStyle.TextStyles[aCurStyleNo]);
                DoMatchNew(vTextStyle);
                if (FOnTextStyle != null)
                    FOnTextStyle(aCurStyleNo, vTextStyle);

                return aStyle.GetStyleNo(vTextStyle, true);
            }
        }

        public virtual bool StyleHasMatch(HCStyle aStyle, int aCurStyleNo)
        {
            return false;
        }

        public bool Append
        {
            get { return FAppend; }
            set { SetAppend(value); }
        }

        public OnTextStyle OnTextStyle
        {
            get { return FOnTextStyle; }
            set { FOnTextStyle = value; }
        }
    }

    public class TextStyleMatch : HCStyleMatch
    {
        private HCFontStyle FFontStyle;

        protected override bool DoMatchCur(HCTextStyle aTextStyle)
        {
            return Append && aTextStyle.FontStyles.Contains((byte)FFontStyle);
        }

        protected override void DoMatchNew(HCTextStyle aTextStyle)
        {
            if (Append)
            {
                if (FFontStyle == HCFontStyle.tsSuperscript)
                    aTextStyle.FontStyles.ExClude((byte)HCFontStyle.tsSubscript);
                else
                if (FFontStyle == HCFontStyle.tsSubscript)
                    aTextStyle.FontStyles.ExClude((byte)HCFontStyle.tsSuperscript);

                aTextStyle.FontStyles.InClude((byte)FFontStyle);
            }
            else
                aTextStyle.FontStyles.ExClude((byte)FFontStyle);
        }

        public override bool StyleHasMatch(HCStyle aStyle, int aCurStyleNo)
        {
            return aStyle.TextStyles[aCurStyleNo].FontStyles.Contains((byte)FFontStyle);
        }

        public HCFontStyle FontStyle
        {
            get { return FFontStyle; }
            set { FFontStyle = value; }
        }
    }

    public class FontNameStyleMatch : HCStyleMatch
    {
        private string FFontName;

        protected override bool DoMatchCur(HCTextStyle aTextStyle)
        {
            return aTextStyle.Family == FFontName;
        }

        protected override void DoMatchNew(HCTextStyle aTextStyle)
        {
            aTextStyle.Family = FFontName;
        }

        public string FontName
        {
            get { return FFontName; }
            set { FFontName = value; }
        }
    }

    public class FontSizeStyleMatch : HCStyleMatch
    {
        private Single FFontSize;

        protected override bool DoMatchCur(HCTextStyle aTextStyle)
        {
            return aTextStyle.Size == FFontSize;
        }

        protected override void DoMatchNew(HCTextStyle aTextStyle)
        {
            aTextStyle.Size = FFontSize;
        }

        public Single FontSize
        {
            get { return FFontSize; }
            set { FFontSize = value; }
        }
    }

    public class ColorStyleMatch : HCStyleMatch
    {
        private Color FColor;

        protected override bool DoMatchCur(HCTextStyle aTextStyle)
        {
            return aTextStyle.Color == FColor;
        }

        protected override void DoMatchNew(HCTextStyle aTextStyle)
        {
            aTextStyle.Color = FColor;
        }

        public Color Color
        {
            get { return FColor; }
            set { FColor = value; }
        }
    }

    public class BackColorStyleMatch : HCStyleMatch
    {
        private Color FColor;

        protected override bool DoMatchCur(HCTextStyle aTextStyle)
        {
            return aTextStyle.BackColor == FColor;
        }

        protected override void DoMatchNew(HCTextStyle aTextStyle)
        {
            aTextStyle.BackColor = FColor;
        }

        public Color Color
        {
            get { return FColor; }
            set { FColor = value; }
        }
    }

    public abstract class HCParaMatch : Object  // 段样式匹配类
    {
        protected abstract bool DoMatchCurPara(HCParaStyle aParaStyle);
        protected abstract void DoMatchNewPara(HCParaStyle aParaStyle);

        public virtual int GetMatchParaNo(HCStyle aStyle, int aCurParaNo)
        {
            if (DoMatchCurPara(aStyle.ParaStyles[aCurParaNo]))
                return aCurParaNo;

            using (HCParaStyle vParaStyle = new HCParaStyle())
            {
                vParaStyle.AssignEx(aStyle.ParaStyles[aCurParaNo]);
                DoMatchNewPara(vParaStyle);
                return aStyle.GetParaNo(vParaStyle, true);
            }
        }
    }

    public class ParaAlignHorzMatch : HCParaMatch
    {
        private ParaAlignHorz FAlign;

        protected override bool DoMatchCurPara(HCParaStyle aParaStyle)
        {
            return aParaStyle.AlignHorz == FAlign;
        }

        protected override void DoMatchNewPara(HCParaStyle aParaStyle)
        {
            aParaStyle.AlignHorz = FAlign;
        }

        public ParaAlignHorz Align
        {
            get { return FAlign; }
            set { FAlign = value; }
        }
    }

    public class ParaAlignVertMatch : HCParaMatch
    {
        private ParaAlignVert FAlign;

        protected override bool DoMatchCurPara(HCParaStyle aParaStyle)
        {
            return aParaStyle.AlignVert == FAlign;
        }

        protected override void DoMatchNewPara(HCParaStyle aParaStyle)
        {
            aParaStyle.AlignVert = FAlign;
        }

        public ParaAlignVert Align
        {
            get { return FAlign; }
            set { FAlign = value; }
        }
    }

    public class ParaLineSpaceMatch : HCParaMatch
    {
        private ParaLineSpaceMode FSpaceMode;
        private Single FSpace;

        protected override bool DoMatchCurPara(HCParaStyle aParaStyle)
        {
            bool vResult = FSpaceMode == aParaStyle.LineSpaceMode;
            if (vResult)
            {
                if (FSpaceMode == ParaLineSpaceMode.plsFix)
                    return FSpace == aParaStyle.LineSpace;
                else
                if (FSpaceMode == ParaLineSpaceMode.plsMult)
                    return FSpace == aParaStyle.LineSpace;
                else
                    return true;
            }

            return false;
        }

        protected override void DoMatchNewPara(HCParaStyle aParaStyle)
        {
            aParaStyle.LineSpaceMode = FSpaceMode;
            aParaStyle.LineSpace = FSpace;
        }

        public ParaLineSpaceMode SpaceMode
        {
            get { return FSpaceMode; }
            set { FSpaceMode = value; }
        }

        public Single Space
        {
            get { return FSpace; }
            set { FSpace = value; }
        }
    }

    public class ParaBackColorMatch : HCParaMatch
    {
        private Color FBackColor;

        protected override bool DoMatchCurPara(HCParaStyle aParaStyle)
        {
            return aParaStyle.BackColor == FBackColor;
        }

        protected override void DoMatchNewPara(HCParaStyle aParaStyle)
        {
            aParaStyle.BackColor = FBackColor;
        }

        public Color BackColor
        {
            get { return FBackColor; }
            set { FBackColor = value; }
        }
    }

    public class ParaBreakRoughMatch : HCParaMatch
    {
        private bool FBreakRough;

        protected override bool DoMatchCurPara(HCParaStyle aParaStyle)
        {
            return aParaStyle.BreakRough == FBreakRough;
        }

        protected override void DoMatchNewPara(HCParaStyle aParaStyle)
        {
            aParaStyle.BreakRough = FBreakRough;
        }

        public bool BreakRough
        {
            get { return FBreakRough; }
            set { FBreakRough = value; }
        }
    }

    public class ParaFirstIndentMatch : HCParaMatch  // 段首行缩进匹配类
    {
        private Single FIndent;

        protected override bool DoMatchCurPara(HCParaStyle aParaStyle)
        {
            return aParaStyle.FirstIndent == FIndent;
        }

        protected override void DoMatchNewPara(HCParaStyle aParaStyle)
        {
            aParaStyle.FirstIndent = FIndent;
        }

        public Single Indent
        {
            get { return FIndent; }
            set { FIndent = value; }
        }
    }

    public class ParaLeftIndentMatch : HCParaMatch  // 段左缩进匹配类
    {
        private Single FIndent;

        protected override bool DoMatchCurPara(HCParaStyle aParaStyle)
        {
            return aParaStyle.LeftIndent == FIndent;
        }

        protected override void DoMatchNewPara(HCParaStyle aParaStyle)
        {
            aParaStyle.LeftIndent = FIndent;
        }

        public Single Indent
        {
            get { return FIndent; }
            set { FIndent = value; }
        }
    }

    public class ParaRightIndentMatch : HCParaMatch  // 段右缩进匹配类
    {
        private Single FIndent;

        protected override bool DoMatchCurPara(HCParaStyle aParaStyle)
        {
            return aParaStyle.RightIndent == FIndent;
        }

        protected override void DoMatchNewPara(HCParaStyle aParaStyle)
        {
            aParaStyle.RightIndent = FIndent;
        }

        public Single Indent
        {
            get { return FIndent; }
            set { FIndent = value; }
        }
    }
}
