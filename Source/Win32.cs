using System;
using System.Runtime.InteropServices;

namespace HC.Win32
{	 
	public struct RECT 
	{
		public int Left;
		public int Top;
		public int Right;
		public int Bottom;

        public RECT(int ALeft, int ATop, int ARight, int ABottom)
        {
            Left = ALeft;
            Top = ATop;
            Right = ARight;
            Bottom = ABottom;
        }

        public POINT TopLeft()
        {
            return new POINT(Left, Top);
        }

        public void SetWidth(int Value)
        {
            Right = Left + Value;
        }

        public int Width
        {
            get { return Right - Left; }
            set { SetWidth(value); }
        }

        public void SetHeight(int Value)
        {
            Bottom = Top + Value;
        }

        public int Height
        {
            get { return Bottom - Top; }
            set { SetHeight(value); }
        }

        public void Offset(int x, int y)
        {
            Left += x;
            Top += y;
            Right += x;
            Bottom += y;
        }

        public void Inflate(int x, int y)
        {
            Left -= x;
            Right += x;
            Top -= y;
            Bottom += y;
        }
	}

    [StructLayout(LayoutKind.Sequential)]//È·±£MoveToEx×Ö¶ÎË³Ðò
	public struct POINT 
	{
		public int X;
		public int Y;

        public POINT(int ax, int ay)
        {
            X = ax;
            Y = ay;
        }

        public void Offset(int x, int y)
        {
            X += x;
            Y += y;
        }
	}

	public struct SIZE 
	{
		public int cx;
		public int cy;

        public SIZE(int X, int Y)
        {
            cx = X;
            cy = Y;
        }
	}
	public struct FILETIME 
	{
		public int dwLowDateTime;
		public int dwHighDateTime;
	}
	public struct SYSTEMTIME 
	{
		public short wYear;
		public short wMonth;
		public short wDayOfWeek;
		public short wDay;
		public short wHour;
		public short wMinute;
		public short wSecond;
		public short wMilliseconds;
	}
}

