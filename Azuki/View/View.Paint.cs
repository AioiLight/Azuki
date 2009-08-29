// file: View.Paint.cs
// brief: Common painting logic
// author: YAMAMOTO Suguru
// update: 2009-08-29
//=========================================================
//DEBUG//#define DRAW_SLOWLY
using System;
using System.Collections.Generic;
using System.Drawing;
using StringBuilder = System.Text.StringBuilder;
using Debug = System.Diagnostics.Debug;

namespace Sgry.Azuki
{
	abstract partial class View
	{
		/// <summary>
		/// Paints content to a graphic device.
		/// </summary>
		/// <param name="clipRect">clipping rectangle that covers all invalidated region (in client area coordinate)</param>
		public abstract void Paint( Rectangle clipRect );

		/// <summary>
		/// Paints a token including special characters.
		/// </summary>
		protected void DrawToken( string token, CharClass klass, bool inSelection, ref Point tokenPos, ref Point tokenEndPos, ref Rectangle clipRect )
		{
			Debug.Assert( token != null, "given token is null." );
			Debug.Assert( 0 < token.Length, "given token is empty." );
#			if DRAW_SLOWLY
			if(!Windows.WinApi.IsKeyDownAsync(System.Windows.Forms.Keys.ControlKey))
			{ _Gra.BackColor=Color.Red; _Gra.FillRectangle(tokenPos.X, tokenPos.Y, 2, LineHeight); DebugUtl.Sleep(400); }
#			endif
			Color fore, back;

			// get fore/back color for the class
			Utl.ColorFromCharClass( ColorScheme, klass, inSelection, out fore, out back );
			_Gra.BackColor = back;

			//--- draw graphic ---
			// space
			if( token == " " )
			{
				// draw background
				_Gra.FillRectangle( tokenPos.X, tokenPos.Y, _SpaceWidth, LineSpacing );

				// draw foreground graphic
				if( DrawsSpace )
				{
					_Gra.ForeColor = ColorScheme.WhiteSpaceColor;
					_Gra.DrawRectangle(
							tokenPos.X + (_SpaceWidth >> 1) - 1,
							tokenPos.Y + (_LineHeight >> 1),
							1,
							1
						);
				}
				return;
			}
			// full-width space
			else if( token == "\x3000" )
			{
				int graLeft, graWidth, graTop, graBottom;

				// calc desired foreground graphic position
				graLeft = tokenPos.X + 2;
				graWidth = _FullSpaceWidth - 5;
				graTop = (tokenPos.Y + _LineHeight / 2) - (graWidth / 2);
				graBottom = (tokenPos.Y + _LineHeight / 2) + (graWidth / 2);

				// draw background
				_Gra.FillRectangle( tokenPos.X, tokenPos.Y, _FullSpaceWidth, LineSpacing );

				// draw foreground
				if( DrawsFullWidthSpace )
				{
					_Gra.ForeColor = ColorScheme.WhiteSpaceColor;
					_Gra.DrawRectangle( graLeft, graTop, graWidth, graBottom-graTop );
				}
				return;
			}
			// tab
			else if( token == "\t" )
			{
				int bgLeft, bgRight;
				int fgLeft, fgRight;
				int fgTop = tokenPos.Y + (_LineHeight * 1 / 3);
				int fgBottom = tokenPos.Y + (_LineHeight * 2 / 3);

				// calc next tab stop (calc in virtual space and convert it to screen coordinate)
				Point p = tokenPos;
				ScreenToVirtual( ref p );
				bgRight = Utl.CalcNextTabStop( p.X, TabWidthInPx );
				bgRight -= ScrollPosX - XofTextArea;
				
				// calc desired foreground graphic position
				fgLeft = tokenPos.X + 2;
				fgRight = bgRight - 2;
				bgLeft = tokenPos.X;

				// draw background
				_Gra.FillRectangle( bgLeft, tokenPos.Y, bgRight-bgLeft, LineSpacing );

				// draw foreground
				if( DrawsTab )
				{
					_Gra.ForeColor = ColorScheme.WhiteSpaceColor;
					_Gra.DrawLine( fgLeft, fgBottom, fgRight, fgBottom );
					_Gra.DrawLine( fgRight, fgBottom, fgRight, fgTop );
				}
				return;
			}
			// EOL-Code
			else if( LineLogic.IsEolChar(token, 0) )
			{
				// before to draw background,
				// change bgcolor to normal if it's not selected
				if( inSelection == false )
					_Gra.BackColor = ColorScheme.BackColor;

				// draw background
				_Gra.FillRectangle( tokenPos.X, tokenPos.Y, _LineHeight>>1, LineSpacing );

				if( DrawsEolCode == false )
					return;

				// calc metric
				int width = (_LineHeight >> 1); // _LineHeight/2
				int y_middle = tokenPos.Y + width;
				int x_middle = tokenPos.X + (width >> 1); // width/2
				int halfSpaceWidth = (_SpaceWidth >> 1); // _SpaceWidth/2
				int left = tokenPos.X + 1;
				int right = tokenPos.X + width - 2;
				int bottom = y_middle + (width >> 1);

				// draw EOL char's graphic
				_Gra.ForeColor = ColorScheme.EolColor;
				if( token == "\r" ) // CR (left arrow)
				{
					_Gra.DrawLine( left, y_middle, left+halfSpaceWidth, y_middle-halfSpaceWidth );
					_Gra.DrawLine( left, y_middle, tokenPos.X+width-2, y_middle );
					_Gra.DrawLine( left, y_middle, left+halfSpaceWidth, y_middle+halfSpaceWidth );
				}
				else if( token == "\n" ) // LF (down arrow)
				{
					_Gra.DrawLine( x_middle, bottom, x_middle-halfSpaceWidth, bottom-halfSpaceWidth );
					_Gra.DrawLine( x_middle, y_middle-(width>>1), x_middle, bottom );
					_Gra.DrawLine( x_middle, bottom, x_middle+halfSpaceWidth, bottom-halfSpaceWidth );
				}
				else // CRLF (snapped arrow)
				{
					_Gra.DrawLine( right, y_middle-(width>>1), right, y_middle+2 );

					_Gra.DrawLine( left, y_middle+2, left+halfSpaceWidth, y_middle+2-halfSpaceWidth );
					_Gra.DrawLine( right, y_middle+2, left, y_middle+2 );
					_Gra.DrawLine( left, y_middle+2, left+halfSpaceWidth, y_middle+2+halfSpaceWidth );
				}
				return;
			}

			// draw normal visible text
			_Gra.FillRectangle( tokenPos.X, tokenPos.Y, tokenEndPos.X-tokenPos.X, LineSpacing );
			_Gra.DrawText( token, ref tokenPos, fore );
		}

		/// <summary>
		/// Draws underline to the line specified by it's Y coordinate.
		/// </summary>
		/// <param name="lineTopY">Y-coordinate of the target line.</param>
		/// <param name="color">Color to be used for drawing the underline.</param>
		protected void DrawUnderLine( int lineTopY, Color color )
		{
			DebugUtl.Assert( (lineTopY % LineSpacing) == (YofTextArea % LineSpacing), "lineTopY:"+lineTopY+", LineSpacing:"+LineSpacing+", YofTextArea:"+YofTextArea );
			int textAreaRight = _TextAreaWidth + (XofTextArea - ScrollPosX);

			// calculate position to underline
			int right = Math.Min( _VisibleSize.Width, textAreaRight );
			int bottom = lineTopY + _LineHeight;

			// draw underline
			_Gra.ForeColor = color;
			_Gra.DrawLine( XofTextArea, bottom, right - 2, bottom );
		}

		/// <summary>
		/// Draws line number area at specified line.
		/// </summary>
		/// <param name="lineTopY">Y-coordinate of the target line.</param>
		/// <param name="lineNumber">line number to be drawn or minus value if you want to draw only background.</param>
		protected void DrawLineNumber( int lineTopY, int lineNumber )
		{
			DebugUtl.Assert( (lineTopY % LineSpacing) == (YofTextArea % LineSpacing), "lineTopY:"+lineTopY+", LineSpacing:"+LineSpacing+", YofTextArea:"+YofTextArea );
			Point pos = new Point( 0, lineTopY );
			
			// fill line number area
			_Gra.BackColor = ColorScheme.LineNumberBack;
			_Gra.FillRectangle( 0, pos.Y, _LineNumAreaWidth, LineSpacing );
			_Gra.BackColor = ColorScheme.BackColor;
			_Gra.FillRectangle( _LineNumAreaWidth, pos.Y, LeftMargin, LineSpacing );
			
			// draw line number text
			if( 0 < lineNumber )
			{
				string lineNumText = lineNumber.ToString();
				pos.X = _LineNumAreaWidth - _Gra.MeasureText( lineNumText ).Width - LineNumberAreaPadding;
				_Gra.ForeColor = ColorScheme.LineNumberFore;
				_Gra.DrawText( lineNumText, ref pos, ColorScheme.LineNumberFore );
			}

			// draw margin line between the line number area and text area
			pos.X = _LineNumAreaWidth - 1;
			_Gra.ForeColor = ColorScheme.LineNumberFore;
			_Gra.DrawLine( pos.X, pos.Y, pos.X, pos.Y+LineSpacing+1 );
		}

		/// <summary>
		/// Draws top margin.
		/// </summary>
		protected void DrawTopMargin()
		{
			// fill area above the line-number area [copied from DrawLineNumber]
			_Gra.BackColor = ColorScheme.LineNumberBack;
			_Gra.FillRectangle( 0, 0, _LineNumAreaWidth, YofTextArea );
			_Gra.BackColor = ColorScheme.BackColor;
			_Gra.FillRectangle( _LineNumAreaWidth, 0, LeftMargin, YofTextArea );

			// draw margin line between the line number area and text area [copied from DrawLineNumber]
			int x = _LineNumAreaWidth - 1;
			_Gra.ForeColor = ColorScheme.LineNumberFore;
			_Gra.DrawLine( x, 0, x, YofTextArea );

			// fill area above the text area
			_Gra.BackColor = ColorScheme.BackColor;
			_Gra.FillRectangle( XofTextArea, 0, VisibleSize.Width-XofTextArea, YofTextArea );
		}

		/// <summary>
		/// Calculates x-coordinate of the right end of given token drawed at specified position with specified tab-width.
		/// </summary>
		internal int MeasureTokenEndX( string token, int virX )
		{
			int dummy;
			return MeasureTokenEndX( token, virX, Int32.MaxValue, out dummy );
		}

		/// <summary>
		/// Calculates x-coordinate of the right end of given token drawed at specified position with specified tab-width.
		/// </summary>
		protected int MeasureTokenEndX( string token, int virX, int rightLimitX, out int drawableLength )
		{
			StringBuilder subToken;
			int x = virX;
			int relDLen; // relatively calculated drawable length
			int subTokenWidth;
			bool hitRightLimit;

			drawableLength = 0;
			if( token.Length == 0 )
			{
				return x;
			}

			// for each char
			subToken = new StringBuilder( token.Length );
			for( int i=0; i<token.Length; i++ )
			{
				// tab?
				if( token[i] == '\t' )
				{
					// if something is in buffer, add its length and clear buffer.
					hitRightLimit = MeasureTokenEndX_TreatSubToken( _Gra, i, subToken, rightLimitX, ref x, ref drawableLength );
					if( hitRightLimit )
					{
						return x; // hit the right limit
					}

					// calc next tab stop
					subTokenWidth = Utl.CalcNextTabStop( x, TabWidthInPx );
					if( rightLimitX <= subTokenWidth )
					{
						drawableLength = i;
						return x; // hit the right limit.
					}
					drawableLength++;
					x = subTokenWidth;
				}
				else if( LineLogic.IsEolChar(token, i) )
				{
					// detected EOL code.

					hitRightLimit = MeasureTokenEndX_TreatSubToken( _Gra, i, subToken, rightLimitX, ref x, ref drawableLength );
					if( hitRightLimit )
					{
						return x; // hit the right limit
					}

					// check whether this EOL code can be drawn or not
					x += (_LineHeight >> 1);
					if( rightLimitX <= x )
					{
						x = rightLimitX; // hit the right limit
						return x;
					}

					// treat this EOL code
					drawableLength++;
					if( token[i] == '\r'
						&& i+1 < token.Length && token[i+1] == '\n' )
					{
						drawableLength++;
					}
					return x;
				}
				else
				{
					if( rightLimitX < subToken.Length )
					{
						// because any glyph in any font has at least 1px width, break seeking.
						hitRightLimit = MeasureTokenEndX_TreatSubToken( _Gra, i, subToken, rightLimitX, ref x, ref drawableLength );
						if( hitRightLimit )
						{
							return x; // hit the right limit
						}
					}
					subToken.Append( token[i] );
				}
			}

			// calc last sub-token
			if( 0 < subToken.Length )
			{
				x += _Gra.MeasureText( subToken.ToString(), rightLimitX-x, out relDLen ).Width;
				if( relDLen < subToken.Length )
				{
					drawableLength = token.Length - (subToken.Length - relDLen);
					return x; // hit the right limit.
				}
				drawableLength += subToken.Length;
			}

			// whole part of the given token can be drawn at given width.
			return x;
		}

		/// <returns>true if measured right poisition hit the limit.</returns>
		static bool MeasureTokenEndX_TreatSubToken( IGraphics gra, int i, StringBuilder subToken, int rightLimitX, ref int x, ref int drawableLength )
		{
			int subTokenWidth;
			int relDLen;
			
			if( subToken.Length == 0 )
			{
				return false;
			}

			subTokenWidth = gra.MeasureText( subToken.ToString(), rightLimitX-x, out relDLen ).Width;
			if( relDLen < subToken.Length )
			{
				// given width is too narrow to draw this sub-token.
				// chop after the limit and re-calc subtoken's width
				drawableLength = i - (subToken.Length - relDLen);
				char[] buf = new char[ relDLen ];
				for( int j=0; j<relDLen; j++ )
				{
					buf[j] = subToken[j];
				}
				x += gra.MeasureText( new String(buf) ).Width;
				return true;
			}

			x += subTokenWidth;
			drawableLength += subToken.Length;
			subToken.Length = 0;

			return false;
		}

		#region Utilities
		/// <summary>
		/// Distinguishes whether specified index is in selection or not.
		/// </summary>
		protected bool IsInSelection( int index )
		{
			int begin, end;

			if( Document.RectSelectRanges != null )
			{
				// is in rectangular selection mode.
				for( int i=0; i<Document.RectSelectRanges.Length; i+=2 )
				{
					begin = Document.RectSelectRanges[i];
					end = Document.RectSelectRanges[i+1];
					if( begin <= index && index < end )
					{
						return true;
					}
				}

				return false;
			}
			else
			{
				// is not in rectangular selection mode.
				Document.GetSelection( out begin, out end );
				return (begin <= index && index < end);
			}
		}

		/// <summary>
		/// Calculates end index of the drawing token at longest case by selection state.
		/// </summary>
		int CalcTokenEndLimit( Document doc, int index, int nextLineHead, out bool inSelection )
		{
			DebugUtl.Assert( doc != null );
			DebugUtl.Assert( index < doc.Length );
			DebugUtl.Assert( index < nextLineHead && nextLineHead <= doc.Length );
			int selBegin, selEnd;

			// get selection range on the line
			doc.GetSelection( out selBegin, out selEnd );
			if( doc.RectSelectRanges != null )
			{
				//--- rectangle selection ---
				// find a selection range that is on the drawing line
				// (finding a begin-end pair whose 'end' is at middle of 'index' and 'nextLineHead')
				int i;
				for( i=0; i<doc.RectSelectRanges.Length; i+=2 )
				{
					selBegin = doc.RectSelectRanges[i];
					selEnd = doc.RectSelectRanges[i+1];
					if( index <= selEnd && selEnd < nextLineHead )
					{
						break;
					}
				}
				if( doc.RectSelectRanges.Length <= i )
				{
					// no such pair was found so this token can extend to the line end
					inSelection = false;
					return nextLineHead;
				}
			}

			if( index < selBegin )
			{
				// token begins before selection range.
				// so this token is out of selection and must stops before reaching the selection
				inSelection = false;
				return Math.Min( selBegin, nextLineHead );
			}
			else if( index < selEnd )
			{
				// token is in selection.
				// this token must stops in the selection range
				inSelection = true;
				return Math.Min( selEnd, nextLineHead );
			}
			else
			{
				inSelection = false;
				return nextLineHead;
			}
		}

		/// <summary>
		/// Gets next token for painting.
		/// </summary>
		protected int NextPaintToken( Document doc, int index, int nextLineHead, out CharClass out_klass, out bool out_inSelection )
		{
			DebugUtl.Assert( nextLineHead <= doc.Length, "param 'nextLineHead'("+nextLineHead+") must not be greater than 'doc.Length'("+doc.Length+")." );

			char firstCh, ch;
			CharClass firstKlass, klass;
			int tokenEndLimit;

			out_inSelection = false;

			// if given index is out of range,
			// return -1 to terminate outer loop
			if( nextLineHead <= index )
			{
				out_klass = CharClass.Normal;
				return -1;
			}

			// calculate how many chars should be drawn as one token
			tokenEndLimit = CalcTokenEndLimit( doc, index, nextLineHead, out out_inSelection );

			// get first char class and selection state
			out_inSelection = IsInSelection( index );
			firstCh = doc[ index ];
			firstKlass = doc.GetCharClass( index );
			out_klass = firstKlass;
			if( Utl.IsSpecialChar(firstCh) )
			{
				// treat 1 special char as 1 token
				if( firstCh == '\r'
					&& index+1 < doc.Length
					&& doc[index+1] == '\n' )
				{
					return index + 2;
				}
				else
				{
					return index + 1;
				}
			}
			
			// seek until token end appears
			while( index+1 < tokenEndLimit )
			{
				// get next char
				index++;
				ch = doc[ index ];
				klass = doc.GetCharClass( index );

				// if this char is a special char, stop seeking
				if( Utl.IsSpecialChar(ch) )
				{
					return index;
				}
				// or, character class changed; token ended
				else if( klass != firstKlass )
				{
					return index;
				}
			}

			// reached to the limit
			return tokenEndLimit;
		}

		/// <summary>
		/// Class containing small utilities for class View.
		/// </summary>
		protected partial class Utl
		{
			/// <summary>
			/// Gets fore/back color pair from scheme according to char class.
			/// </summary>
			public static void ColorFromCharClass( ColorScheme cs, CharClass klass, bool inSelection, out Color fore, out Color back )
			{
				if( inSelection )
				{
					fore = cs.SelectionFore;
					back = cs.SelectionBack;
				}
				else
				{
					cs.GetColor( klass, out fore, out back );
				}
			}

			/// <summary>
			/// Calculate x-coordinate of the next tab stop.
			/// </summary>
			/// <param name="x">calculates next tab stop from this (X coordinate in virtual space)</param>
			/// <param name="tabWidthInPx">tab width (in pixel)</param>
			public static int CalcNextTabStop( int x, int tabWidthInPx )
			{
				DebugUtl.Assert( 0 < tabWidthInPx );
				return ((x / tabWidthInPx) + 1) * tabWidthInPx;
			}

			/// <summary>
			/// Distinguishs whether given char is special for painting or not.
			/// </summary>
			public static bool IsSpecialChar( char ch )
			{
				if( ch == ' '
					|| ch == '\x3000' // full-width space
					|| ch == '\t'
					|| ch == '\r'
					|| ch == '\n' )
				{
					return true;
				}

				return false;
			}

			/// <summary>
			/// Gets minimum value in four integers.
			/// </summary>
			public static int Min( int a, int b, int c, int d )
			{
				return Math.Min( a, Math.Min(b, Math.Min(c,d) ) );
			}

			/// <summary>
			/// Gets maximum value in four integers.
			/// </summary>
			public static int Max( int a, int b, int c, int d )
			{
				return Math.Max( a, Math.Max(b, Math.Max(c,d) ) );
			}
		}
		#endregion
	}
}
