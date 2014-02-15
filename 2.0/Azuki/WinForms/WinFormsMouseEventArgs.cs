﻿// file: AzukiMouseEventArgs.cs
// brief: mouse event parameter object for WinForms platform.
// author: YAMAMOTO Suguru
// update: 2010-12-04
//=========================================================
using System.Windows.Forms;

namespace Sgry.Azuki.WinForms
{
	class WinFormsMouseEventArgs : MouseEventArgs, IMouseEventArgs
	{
		readonly int _ButtonIndex;
		readonly int _Index;
		readonly bool _Shift, _Control, _Alt, _Special;

		public WinFormsMouseEventArgs( MouseEventArgs e, int index, int clicks, bool shift, bool control, bool alt, bool special )
			: base( e.Button, clicks, e.X, e.Y, 0 )
		{
			_Index = index;
			_Shift = shift;
			_Control = control;
			_Alt = alt;
			_Special = special;

			switch( e.Button )
			{
				case MouseButtons.XButton2:
					_ButtonIndex = 4;
					break;
				case MouseButtons.XButton1:
					_ButtonIndex = 3;
					break;
				case MouseButtons.Middle:
					_ButtonIndex = 2;
					break;
				case MouseButtons.Right:
					_ButtonIndex = 1;
					break;
				default:
					_ButtonIndex = 0;
					break;
			}
		}

		public int ButtonIndex
		{
			get{ return _ButtonIndex; }
		}

		public int Index
		{
			get{ return _Index; }
		}

		public bool Shift
		{
			get{ return _Shift; }
		}

		public bool Control
		{
			get{ return _Control; }
		}

		public bool Alt
		{
			get{ return _Alt; }
		}

		public bool Special
		{
			get{ return _Special; }
		}

		public bool Handled { get; set; }
	}
}
