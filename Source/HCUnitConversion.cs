using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HC.Win32;

namespace HC.View
{
    public static class HCUnitConversion
    {
        public static Single
            PixelsPerMMX = 3.7f,
            PixelsPerMMY = 3.7f,
            FontSizeScale = 0.75f;

        public static int
            PixelsPerInchX = 96,
            PixelsPerInchY = 96;

        public static void Initialization()
        {
            IntPtr vDC = (IntPtr)GDI.CreateCompatibleDC(IntPtr.Zero);
            try
            {
                PixelsPerInchX = GDI.GetDeviceCaps(vDC, GDI.LOGPIXELSX);  // 每英寸水平逻辑像素数，1英寸dpi数
                PixelsPerInchY = GDI.GetDeviceCaps(vDC, GDI.LOGPIXELSY);  // 每英寸水平逻辑像素数，1英寸dpi数
            }
            finally
            {
                GDI.DeleteDC(vDC);
            }

            FontSizeScale = 72.0f / PixelsPerInchX;

            // 1英寸25.4毫米   FPixelsPerInchX
            PixelsPerMMX = PixelsPerInchX / 25.4f;  // 1毫米对应像素 = 1英寸dpi数 / 1英寸对应毫米
            PixelsPerMMY = PixelsPerInchY / 25.4f;  // 1毫米对应像素 = 1英寸dpi数 / 1英寸对应毫米
        }

        /// <summary> 水平像素转为毫米 </summary>
        public static int PixXToMillimeter(int value)
        {
            return (int)Math.Round(value / PixelsPerMMX);
        }

        /// <summary> 毫米转为水平像素 </summary>
        public static int MillimeterToPixX(Single value)
        {
            return (int)Math.Round(value * PixelsPerMMX);
        }

        /// <summary> 垂直像素转为毫米 </summary>
        public static int PixYToMillimeter(int value)
        {
            return (int)Math.Round(value / PixelsPerMMY);
        }

        /// <summary> 毫米转为垂直像素 </summary>
        public static int MillimeterToPixY(Single value)
        {
            return (int)Math.Round(value * PixelsPerMMY);
        }

    }
}
