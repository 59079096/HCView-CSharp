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
    public delegate void OnTextStyle(int ACurStyleNo, ref HCTextStyle AWillStyle);

    public class HCStyleMatch  // 文本样式匹配类
    {
        private bool FAppend;  // True添加对应样式
        private OnTextStyle FOnTextStyle;

        public virtual int GetMatchStyleNo(HCStyle AStyle, int ACurStyleNo)
        {
            return HCStyle.Null;
        }

        public virtual bool StyleHasMatch(HCStyle AStyle, int ACurStyleNo)
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

        public override int GetMatchStyleNo(HCStyle AStyle, int ACurStyleNo)
        {
            int Result = HCStyle.Null;
            HCTextStyle vTextStyle = new HCTextStyle();
            try
            {
                vTextStyle.AssignEx(AStyle.TextStyles[ACurStyleNo]);  // item当前的样式
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
                        return ACurStyleNo;
                }
                else  // 减去
                {
                    if (vTextStyle.FontStyles.Contains((byte)FFontStyle))
                        vTextStyle.FontStyles.ExClude((byte)FFontStyle);
                    else
                        return ACurStyleNo;
                }

                if (this.OnTextStyle != null)
                    this.OnTextStyle(ACurStyleNo, ref vTextStyle);
                Result = AStyle.GetStyleNo(vTextStyle, true);  // 新样式编号
            }
            finally
            {
                vTextStyle.Dispose();
            }

            return Result;
        }

        public override bool StyleHasMatch(HCStyle AStyle, int ACurStyleNo)
        {
            bool Result = false;
            HCTextStyle vTextStyle = new HCTextStyle();
            try
            {
                vTextStyle.AssignEx(AStyle.TextStyles[ACurStyleNo]);  // item当前的样式
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

        public override int GetMatchStyleNo(HCStyle AStyle, int ACurStyleNo)
        {
            int Result = HCStyle.Null;
            if (AStyle.TextStyles[ACurStyleNo].Family == FFontName)
            {
                Result = ACurStyleNo;
                return Result;
            }
            HCTextStyle vTextStyle = new HCTextStyle();
            try
            {
                vTextStyle.AssignEx(AStyle.TextStyles[ACurStyleNo]);  // item当前的样式
                vTextStyle.Family = FFontName;
                if (this.OnTextStyle != null)
                    this.OnTextStyle(ACurStyleNo, ref vTextStyle);
            
                Result = AStyle.GetStyleNo(vTextStyle, true);  // 新样式编号
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

        public override int GetMatchStyleNo(HCStyle AStyle, int ACurStyleNo)
        {
            int Result = HCStyle.Null;
            if (AStyle.TextStyles[ACurStyleNo].Size == FFontSize)
            {
                Result = ACurStyleNo;
                return Result;
            }
            HCTextStyle vTextStyle = new HCTextStyle();
            try
            {
                vTextStyle.AssignEx(AStyle.TextStyles[ACurStyleNo]);  // item当前的样式
                vTextStyle.Size = FFontSize;
                if (this.OnTextStyle != null)
                    this.OnTextStyle(ACurStyleNo, ref vTextStyle);
                Result = AStyle.GetStyleNo(vTextStyle, true);  // 新样式编号
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

        public override int GetMatchStyleNo(HCStyle AStyle, int ACurStyleNo)
        {
            int Result = HCStyle.Null;
            if (AStyle.TextStyles[ACurStyleNo].Color == FColor)
            {
                Result = ACurStyleNo;
                return Result;
            }
            HCTextStyle vTextStyle = new HCTextStyle();
            try
            {
                vTextStyle.AssignEx(AStyle.TextStyles[ACurStyleNo]);  // item当前的样式
                vTextStyle.Color = FColor;
                if (this.OnTextStyle != null)
                    this.OnTextStyle(ACurStyleNo, ref vTextStyle);
                Result = AStyle.GetStyleNo(vTextStyle, true);  // 新样式编号
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

        public override int GetMatchStyleNo(HCStyle AStyle, int ACurStyleNo)
        {
            int Result = HCStyle.Null;
            if (AStyle.TextStyles[ACurStyleNo].BackColor == FColor)
            {
                Result = ACurStyleNo;
                return Result;
            }
            HCTextStyle vTextStyle = new HCTextStyle();
            try
            {
                vTextStyle.AssignEx(AStyle.TextStyles[ACurStyleNo]);  // item当前的样式
                vTextStyle.BackColor = FColor;
                if (this.OnTextStyle != null)
                    this.OnTextStyle(ACurStyleNo, ref vTextStyle);
                Result = AStyle.GetStyleNo(vTextStyle, true);  // 新样式编号
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

        public virtual int GetMatchParaNo(HCStyle AStyle, int ACurParaNo)
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

        public override int GetMatchParaNo(HCStyle AStyle, int ACurParaNo)
        {
            int Result = HCStyle.Null;
            if (AStyle.ParaStyles[ACurParaNo].AlignHorz == FAlign)
            {
                Result = ACurParaNo;
                return Result;
            }

            HCParaStyle vParaStyle = new HCParaStyle();
            try
            {
                vParaStyle.AssignEx(AStyle.ParaStyles[ACurParaNo]);
                vParaStyle.AlignHorz = FAlign;
                Result = AStyle.GetParaNo(vParaStyle, true);  // 新段样式
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

        public override int GetMatchParaNo(HCStyle AStyle, int ACurParaNo)
        {
            int Result = HCStyle.Null;
            if (AStyle.ParaStyles[ACurParaNo].AlignVert == FAlign)
            {
                Result = ACurParaNo;
                return Result;
            }

            HCParaStyle vParaStyle = new HCParaStyle();
            try
            {
                vParaStyle.AssignEx(AStyle.ParaStyles[ACurParaNo]);
                vParaStyle.AlignVert = FAlign;
                Result = AStyle.GetParaNo(vParaStyle, true);  // 新段样式
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

        public override int GetMatchParaNo(HCStyle AStyle, int ACurParaNo)
        {
            int Result = HCStyle.Null;
            if (AStyle.ParaStyles[ACurParaNo].LineSpaceMode == FSpaceMode)
            {
                Result = ACurParaNo;
                return Result;
            }

            HCParaStyle vParaStyle = new HCParaStyle();
            try
            {
                vParaStyle.AssignEx(AStyle.ParaStyles[ACurParaNo]);
                vParaStyle.LineSpaceMode = FSpaceMode;
                Result = AStyle.GetParaNo(vParaStyle, true);  // 新段样式
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

        public override int GetMatchParaNo(HCStyle AStyle, int ACurParaNo)
        {
            int Result = HCStyle.Null;
            if (AStyle.ParaStyles[ACurParaNo].BackColor == FBackColor)
            {
                Result = ACurParaNo;
                return Result;
            }

            HCParaStyle vParaStyle = new HCParaStyle();
            try
            {
                vParaStyle.AssignEx(AStyle.ParaStyles[ACurParaNo]);
                vParaStyle.BackColor = FBackColor;
                Result = AStyle.GetParaNo(vParaStyle, true);  // 新段样式
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
