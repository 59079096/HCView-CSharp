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
    public delegate void OnTextStyle(int aCurStyleNo, ref HCTextStyle aWillStyle);

    public class HCStyleMatch  // 文本样式匹配类
    {
        private bool FAppend;  // True添加对应样式
        private OnTextStyle FOnTextStyle;

        public virtual int GetMatchStyleNo(HCStyle aStyle, int aCurStyleNo)
        {
            return HCStyle.Null;
        }

        public virtual bool StyleHasMatch(HCStyle aStyle, int aCurStyleNo)
        {
            return false;
        }

        public OnTextStyle OnTextStyle
        {
            get { return FOnTextStyle; }
            set { FOnTextStyle = value; }
        }

        public bool Append
        {
            get { return FAppend; }
            set { FAppend = value; }
        }
    }

    public class TextStyleMatch : HCStyleMatch
    {
        private HCFontStyle FFontStyle;

        public override int GetMatchStyleNo(HCStyle aStyle, int aCurStyleNo)
        {
            int Result = HCStyle.Null;
            HCTextStyle vTextStyle = new HCTextStyle();
            try
            {
                vTextStyle.AssignEx(aStyle.TextStyles[aCurStyleNo]);  // item当前的样式
                if (this.Append)
                {
                    if (!vTextStyle.FontStyles.Contains((byte)FFontStyle))
                    {
                        // 不能同时为上标和下标
                        if (FFontStyle == HCFontStyle.tsSuperscript)
                            vTextStyle.FontStyles.ExClude((byte)HCFontStyle.tsSubscript);
                        else
                        if (FFontStyle == HCFontStyle.tsSubscript)
                            vTextStyle.FontStyles.ExClude((byte)HCFontStyle.tsSuperscript);

                        vTextStyle.FontStyles.InClude((byte)FFontStyle);
                    }
                    else
                        return aCurStyleNo;
                }
                else  // 减去
                {
                    if (vTextStyle.FontStyles.Contains((byte)FFontStyle))
                        vTextStyle.FontStyles.ExClude((byte)FFontStyle);
                    else
                        return aCurStyleNo;
                }

                if (this.OnTextStyle != null)
                    this.OnTextStyle(aCurStyleNo, ref vTextStyle);
                Result = aStyle.GetStyleNo(vTextStyle, true);  // 新样式编号
            }
            finally
            {
                vTextStyle.Dispose();
            }

            return Result;
        }

        public override bool StyleHasMatch(HCStyle aStyle, int aCurStyleNo)
        {
            bool Result = false;
            HCTextStyle vTextStyle = new HCTextStyle();
            try
            {
                vTextStyle.AssignEx(aStyle.TextStyles[aCurStyleNo]);  // item当前的样式
                Result = vTextStyle.FontStyles.Contains((byte)FFontStyle);
            }
            finally
            {
                vTextStyle.Dispose();
            }
            return Result;
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

        public override int GetMatchStyleNo(HCStyle aStyle, int aCurStyleNo)
        {
            int Result = HCStyle.Null;
            if (aStyle.TextStyles[aCurStyleNo].Family == FFontName)
            {
                Result = aCurStyleNo;
                return Result;
            }
            HCTextStyle vTextStyle = new HCTextStyle();
            try
            {
                vTextStyle.AssignEx(aStyle.TextStyles[aCurStyleNo]);  // item当前的样式
                vTextStyle.Family = FFontName;
                if (this.OnTextStyle != null)
                    this.OnTextStyle(aCurStyleNo, ref vTextStyle);
            
                Result = aStyle.GetStyleNo(vTextStyle, true);  // 新样式编号
            }
            finally
            {
                vTextStyle.Dispose();
            }

            return Result;
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

        public override int GetMatchStyleNo(HCStyle aStyle, int aCurStyleNo)
        {
            int Result = HCStyle.Null;
            if (aStyle.TextStyles[aCurStyleNo].Size == FFontSize)
            {
                Result = aCurStyleNo;
                return Result;
            }
            HCTextStyle vTextStyle = new HCTextStyle();
            try
            {
                vTextStyle.AssignEx(aStyle.TextStyles[aCurStyleNo]);  // item当前的样式
                vTextStyle.Size = FFontSize;
                if (this.OnTextStyle != null)
                    this.OnTextStyle(aCurStyleNo, ref vTextStyle);
                Result = aStyle.GetStyleNo(vTextStyle, true);  // 新样式编号
            }
            finally
            {
                vTextStyle.Dispose();
            }

            return Result;
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

        public override int GetMatchStyleNo(HCStyle aStyle, int aCurStyleNo)
        {
            int Result = HCStyle.Null;
            if (aStyle.TextStyles[aCurStyleNo].Color == FColor)
            {
                Result = aCurStyleNo;
                return Result;
            }
            HCTextStyle vTextStyle = new HCTextStyle();
            try
            {
                vTextStyle.AssignEx(aStyle.TextStyles[aCurStyleNo]);  // item当前的样式
                vTextStyle.Color = FColor;
                if (this.OnTextStyle != null)
                    this.OnTextStyle(aCurStyleNo, ref vTextStyle);
                Result = aStyle.GetStyleNo(vTextStyle, true);  // 新样式编号
            }
            finally
            {
                vTextStyle.Dispose();
            }

            return Result;
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

        public override int GetMatchStyleNo(HCStyle aStyle, int aCurStyleNo)
        {
            int Result = HCStyle.Null;
            if (aStyle.TextStyles[aCurStyleNo].BackColor == FColor)
            {
                Result = aCurStyleNo;
                return Result;
            }
            HCTextStyle vTextStyle = new HCTextStyle();
            try
            {
                vTextStyle.AssignEx(aStyle.TextStyles[aCurStyleNo]);  // item当前的样式
                vTextStyle.BackColor = FColor;
                if (this.OnTextStyle != null)
                    this.OnTextStyle(aCurStyleNo, ref vTextStyle);
                Result = aStyle.GetStyleNo(vTextStyle, true);  // 新样式编号
            }
            finally
            {
                vTextStyle.Dispose();
            }

            return Result;
        }

        public Color Color
        {
            get { return FColor; }
            set { FColor = value; }
        }
    }

    public class HCParaMatch  // 段样式匹配类
    {
        private bool FJoin;  // 添加对应样式

        public virtual int GetMatchParaNo(HCStyle aStyle, int aCurParaNo)
        {
            return HCStyle.Null;
        }

        public bool Join
        {
            get { return FJoin; }
            set { FJoin = value; }
        }
    }

    public class ParaAlignHorzMatch : HCParaMatch
    {
        private ParaAlignHorz FAlign;

        public override int GetMatchParaNo(HCStyle aStyle, int aCurParaNo)
        {
            int Result = HCStyle.Null;
            if (aStyle.ParaStyles[aCurParaNo].AlignHorz == FAlign)
            {
                Result = aCurParaNo;
                return Result;
            }

            HCParaStyle vParaStyle = new HCParaStyle();
            try
            {
                vParaStyle.AssignEx(aStyle.ParaStyles[aCurParaNo]);
                vParaStyle.AlignHorz = FAlign;
                Result = aStyle.GetParaNo(vParaStyle, true);  // 新段样式
            }
            finally
            {
                vParaStyle.Dispose();
            }

            return Result;
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

        public override int GetMatchParaNo(HCStyle aStyle, int aCurParaNo)
        {
            int Result = HCStyle.Null;
            if (aStyle.ParaStyles[aCurParaNo].AlignVert == FAlign)
            {
                Result = aCurParaNo;
                return Result;
            }

            HCParaStyle vParaStyle = new HCParaStyle();
            try
            {
                vParaStyle.AssignEx(aStyle.ParaStyles[aCurParaNo]);
                vParaStyle.AlignVert = FAlign;
                Result = aStyle.GetParaNo(vParaStyle, true);  // 新段样式
            }
            finally
            {
                vParaStyle.Dispose();
            }

            return Result;
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

        public override int GetMatchParaNo(HCStyle aStyle, int aCurParaNo)
        {
            int Result = HCStyle.Null;
            if (aStyle.ParaStyles[aCurParaNo].LineSpaceMode == FSpaceMode)
            {
                Result = aCurParaNo;
                return Result;
            }

            HCParaStyle vParaStyle = new HCParaStyle();
            try
            {
                vParaStyle.AssignEx(aStyle.ParaStyles[aCurParaNo]);
                vParaStyle.LineSpaceMode = FSpaceMode;
                Result = aStyle.GetParaNo(vParaStyle, true);  // 新段样式
            }
            finally
            {
                vParaStyle.Dispose();
            }

            return Result;
        }

        public ParaLineSpaceMode SpaceMode
        {
            get { return FSpaceMode; }
            set { FSpaceMode = value; }
        }
    }

    public class ParaBackColorMatch : HCParaMatch
    {
        private Color FBackColor;

        public override int GetMatchParaNo(HCStyle aStyle, int aCurParaNo)
        {
            int Result = HCStyle.Null;
            if (aStyle.ParaStyles[aCurParaNo].BackColor == FBackColor)
            {
                Result = aCurParaNo;
                return Result;
            }

            HCParaStyle vParaStyle = new HCParaStyle();
            try
            {
                vParaStyle.AssignEx(aStyle.ParaStyles[aCurParaNo]);
                vParaStyle.BackColor = FBackColor;
                Result = aStyle.GetParaNo(vParaStyle, true);  // 新段样式
            }
            finally
            {
                vParaStyle.Dispose();
            }

            return Result;
        }

        public Color BackColor
        {
            get { return FBackColor; }
            set { FBackColor = value; }
        }
    }
}
