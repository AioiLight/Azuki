// file: LineLogic.cs
// brief: Logics to manipulate line/column in a string.
// author: YAMAMOTO Suguru
// update: 2009-10-31
//=========================================================
using System;
using System.Collections;
using System.Text;

namespace Sgry.Azuki
{
	/// <summary>
	/// Logics to handle line/column in a buffer.
	/// In this logic, "line" means characters with one EOL code at tail.
	/// </summary>
	static class LineLogic
	{
		public static readonly char[] EolChars = new char[]{ '\r', '\n' };

		#region Index Conversion
		public static int GetCharIndexFromLineColumnIndex( TextBuffer text, SplitArray<int> lhi, int lineIndex, int columnIndex )
		{
			DebugUtl.Assert( text != null && lhi != null && 0 <= lineIndex && 0 <= columnIndex, "invalid arguments were given" );
			DebugUtl.Assert( lineIndex < lhi.Count, String.Format("too large line index was given (given:{0} actual line count:{1})", lineIndex, lhi.Count) );

			int lineHeadIndex = lhi[lineIndex];

#			if DEBUG
			int lineLength = GetLineLengthByCharIndex( text, lineHeadIndex );
			if( lineLength < columnIndex )
			{
				if( lineIndex == lhi.Count-1
					&& lineLength+1 == columnIndex )
				{
					// indicates EOF. this case is valid.
				}
				else
				{
					DebugUtl.Fail( "specified column index was too large (given:"+columnIndex+" actual line length:"+lineLength+")" );
				}
			}
#			endif

			return lineHeadIndex + columnIndex;
		}

		public static int GetLineIndexFromCharIndex( SplitArray<int> lhi, int charIndex )
		{
			DebugUtl.Assert( 0 <= charIndex, "invalid args; given charIndex was "+charIndex );

			// find the first line whose line-head index exceeds given char-index
			for( int i=1; i<lhi.Count; i++ )
			{
				int nextLineHeadIndex = lhi[i];
				if( charIndex < nextLineHeadIndex )
				{
					// found the NEXT line of the target line.
					return i-1;
				}
			}

			// if given index indicates the EOF code, return its index
			// (giving char-index indicating after EOF would be the same result but only in release build.
			// in debug build, giving such index causes assertion error.)
			return lhi.Count - 1;
		}

		public static void GetLineColumnIndexFromCharIndex( TextBuffer text, SplitArray<int> lhi, int charIndex, out int lineIndex, out int columnIndex )
		{
			DebugUtl.Assert( text != null && lhi != null );
			DebugUtl.Assert( 0 <= charIndex, "invalid args; given charIndex was "+charIndex );
			DebugUtl.Assert( charIndex <= text.Count, String.Format("given charIndex was too large (given:{0} actual text count:{1})", charIndex, text.Count) );

			// find the first line whose line-head index exceeds given char-index
			for( int i=1; i<lhi.Count; i++ )
			{
				int nextLineHeadIndex = lhi[i];
				if( charIndex < nextLineHeadIndex )
				{
					// found the NEXT line of the target line.
					lineIndex = i-1;
					columnIndex = charIndex - lhi[i-1];
					return;
				}
			}

			// if given index indicates the EOF code, return its index
			// (giving char-index indicating after EOF would be the same result but only in release build.
			// in debug build, giving such index causes assertion error.)
			lineIndex = lhi.Count - 1;
			columnIndex = charIndex - lhi[lineIndex];
		}

		public static int GetLineHeadIndexFromCharIndex( TextBuffer text, SplitArray<int> lhi, int charIndex )
		{
			DebugUtl.Assert( text != null && lhi != null );
			DebugUtl.Assert( 0 <= charIndex, "invalid arguments were given ("+charIndex+")" );
			DebugUtl.Assert( charIndex <= text.Count, String.Format("too large char-index was given (given:{0} actual text count:{1})", charIndex, text.Count) );

			// find the first line whose line-head index exceeds given char-index
			for( int i=1; i<lhi.Count; i++ )
			{
				int nextLineHeadIndex = lhi[i];
				if( charIndex < nextLineHeadIndex )
				{
					// found the NEXT line of the target line.
					return lhi[i-1];
				}
			}

			// if given index indicates the EOF code, return its index
			// (giving char-index indicating after EOF would be the same result but only in release build.
			// in debug build, giving such index causes assertion error.)
			return lhi[ lhi.Count-1 ];
		}
		#endregion

		#region Line Range
		public static void GetLineRangeWithEol( TextBuffer text, SplitArray<int> lhi, int lineIndex, out int begin, out int end )
		{
			DebugUtl.Assert( lineIndex < lhi.Count, "argument out of range; given lineIndex is "+lineIndex+" but lhi.Count is "+lhi.Count );

			// get range of the line including EOL code
			begin = lhi[lineIndex];
			if( lineIndex+1 < lhi.Count )
			{
				end = lhi[lineIndex + 1];
			}
			else
			{
				end = text.Count;
			}
		}

		public static void GetLineRange( TextBuffer text, SplitArray<int> lhi, int lineIndex, out int begin, out int end )
		{
			DebugUtl.Assert( 0 <= lineIndex && lineIndex < lhi.Count, "argument out of range; given lineIndex is "+lineIndex+" but lhi.Count is "+lhi.Count );
			int length;

			// get range of the line including EOL code
			begin = lhi[lineIndex];
			if( lineIndex+1 < lhi.Count )
			{
				end = lhi[lineIndex + 1];
			}
			else
			{
				end = text.Count;
			}

			// subtract length of the trailing EOL code
			length = end - begin;
			if( 1 <= length && text.GetAt(end-1) == '\n' )
			{
				if( 2 <= length && text.GetAt(end-2) == '\r' )
					end -= 2;
				else
					end--;
			}
			else if( 1 <= length && text.GetAt(end-1) == '\r' )
			{
				end--;
			}
		}
		#endregion

		#region Line Head Index Management
		/// <summary>
		/// Maintain line head indexes for text insertion.
		/// THIS MUST BE CALLED BEFORE ACTUAL INSERTION.
		/// </summary>
		public static void LHI_Insert(
				SplitArray<int> lhi, SplitArray<LineDirtyState> lms, TextBuffer text, string insertText, int insertIndex
			)
		{
			DebugUtl.Assert( lhi != null && 0 < lhi.Count && lhi[0] == 0, "lhi must have 0 as a first member." );
			DebugUtl.Assert( lms != null && 0 < lms.Count, "lms must have at one or more items." );
			DebugUtl.Assert( lhi.Count == lms.Count, "lhi.Count("+lhi.Count+") is not lms.Count("+lms.Count+")" );
			DebugUtl.Assert( insertText != null && 0 < insertText.Length, "insertText must not be null nor empty." );
			DebugUtl.Assert( 0 <= insertIndex && insertIndex <= text.Count, "insertIndex is out of range ("+insertIndex+")." );
			int insTargetLine, dummy;
			int lineIndex; // work variable
			int lineHeadIndex;
			int lineEndIndex;
			int insLineCount;

			// at first, find the line which contains the insertion point
			GetLineColumnIndexFromCharIndex( text, lhi, insertIndex, out insTargetLine, out dummy );
			lineIndex = insTargetLine;

			// if the inserting divides a CR+LF, insert an entry for the CR separated
			if( 0 < insertIndex && text[insertIndex-1] == '\r'
				&& insertIndex < text.Count && text[insertIndex] == '\n' )
			{
				lhi.Insert( lineIndex+1, insertIndex );
				lms.Insert( lineIndex+1, LineDirtyState.Dirty );
				lineIndex++;
			}

			// if inserted text begins with LF and is inserted just after a CR,
			// remove this CR's entry
			if( 0 < insertIndex && text[insertIndex-1] == '\r'
				&& 0 < insertText.Length && insertText[0] == '\n' )
			{
				lhi.Delete( lineIndex, lineIndex+1 );
				lms.Delete( lineIndex, lineIndex+1 );
				lineIndex--;
			}

			// insert line index entries to LHI
			insLineCount = 1;
			lineHeadIndex = 0;
			do
			{
				// get end index of this line
				lineEndIndex = NextLineHead( insertText, lineHeadIndex ) - 1;
				if( lineEndIndex == -2 ) // == "if NextLineHead returns -1"
				{
					// no more lines following to this line.
					// this is the final line. no need to insert new entry
					break;
				}
				lhi.Insert( lineIndex+insLineCount, insertIndex+lineEndIndex+1 );
				lms.Insert( lineIndex+insLineCount, LineDirtyState.Dirty );
				insLineCount++;

				// find next line head
				lineHeadIndex = NextLineHead( insertText, lineHeadIndex );
			}
			while( lineHeadIndex != -1 );

			// if inserted text is ending with CR and is inserted just before a LF,
			// remove this CR's entry
			if( 0 < insertText.Length && insertText[insertText.Length - 1] == '\r'
				&& insertIndex < text.Count && text[insertIndex] == '\n' )
			{
				int lastInsertedLine = lineIndex + insLineCount - 1;
				lhi.Delete( lastInsertedLine, lastInsertedLine+1 );
				lms.Delete( lastInsertedLine, lastInsertedLine+1 );
				lineIndex--;
			}

			// shift all followings
			for( int i=lineIndex+insLineCount; i<lhi.Count; i++ )
			{
				lhi[i] += insertText.Length;
			}

			// mark the insertion target line as 'dirty'
			if( insertText[0] == '\n'
				&& 0 < insertIndex && text[insertIndex-1] == '\r'
				&& insertIndex < text.Count && text[insertIndex] != '\n' )
			{
				// inserted text has a LF at beginning
				// and there is a CR (not CR+LF) at insertion point.
				// because in this case insertion target line should be
				// the line containing the existing CR,
				// mark not calculated insertion target line
				// but the line at one line before.
				DebugUtl.Assert( 0 < insTargetLine );
				lms[insTargetLine-1] = LineDirtyState.Dirty;
			}
			else
			{
				lms[insTargetLine] = LineDirtyState.Dirty;
			}
		}
		
		/// <summary>
		/// Maintain line head indexes for text deletion.
		/// THIS MUST BE CALLED BEFORE ACTUAL DELETION.
		/// </summary>
		public static void LHI_Delete(
				SplitArray<int> lhi, SplitArray<LineDirtyState> lms, TextBuffer text, int delBegin, int delEnd
			)
		{
			DebugUtl.Assert( lhi != null && 0 < lhi.Count && lhi[0] == 0, "lhi must have 0 as a first member." );
			DebugUtl.Assert( lms != null && 0 < lms.Count, "lms must have one or more items." );
			DebugUtl.Assert( lhi.Count == lms.Count, "lhi.Count("+lhi.Count+") is not lms.Count("+lms.Count+")" );
			DebugUtl.Assert( 0 <= delBegin && delBegin < text.Count, "delBegin is out of range." );
			DebugUtl.Assert( delBegin <= delEnd && delEnd <= text.Count, "delEnd is out of range." );
			int delFirstLine;
			int delFromL, delToL;
			int dummy;
			int delLen = delEnd - delBegin;
			
			// calculate line indexes of both ends of the range
			GetLineColumnIndexFromCharIndex( text, lhi, delBegin, out delFromL, out dummy );
			GetLineColumnIndexFromCharIndex( text, lhi, delEnd, out delToL, out dummy );
			delFirstLine = delFromL;

			if( 0 < delBegin && text[delBegin-1] == '\r' )
			{
				if( delEnd < text.Count && text[delEnd] == '\n' )
				{
					// if the deletion creates a new CR+LF, delete an entry of the CR
					lhi.Delete( delToL, delToL+1 );
					lms.Delete( delToL, delToL+1 );
					delToL--;
				}
				else if( text[delBegin] == '\n' )
				{
					// if the deletion divides a CR+LF at left side of deletion range,
					// insert an entry for the CR
					lhi.Insert( delToL, delBegin );
					lms.Insert( delToL, LineDirtyState.Dirty );
					delFromL++;
					delToL++;
				}
			}

			// subtract line head indexes for lines after deletion point
			for( int i=delToL+1; i<lhi.Count; i++ )
			{
				lhi[i] -= delLen;
			}

			// if deletion decrease line count, delete entries
			if( delFromL < delToL )
			{
				lhi.Delete( delFromL+1, delToL+1 );
				lms.Delete( delFromL+1, delToL+1 );
			}

			// mark the deletion target line as 'dirty'
			if( 0 < delBegin && text[delBegin-1] == '\r'
				&& delEnd < text.Count && text[delEnd] == '\n'
				&& 0 < delFirstLine )
			{
				// there is a CR (not CR+LF) at left of the deletion beginning position
				// and there is a LF (not CR+LF) at right of the deletion ending position.
				// because in this case deletion target line should be
				// the line containing the existing CR,
				// mark not calculated deletion target line
				// but the line at one line before.
				lms[delFirstLine-1] = LineDirtyState.Dirty;
			}
			else
			{
				lms[delFirstLine] = LineDirtyState.Dirty;
			}
		}
		#endregion

		#region Utilities
		public static int CountLine( string text )
		{
			int count = 0;
			int lineHead = 0;

			lineHead = NextLineHead( text, lineHead );
			while( lineHead != -1 )
			{
				count++;
				lineHead = NextLineHead( text, lineHead );
			}

			return count + 1;
		}

		public static bool IsEolChar( char ch )
		{
			return (ch == '\r' || ch == '\n');
		}

		public static bool IsEolChar( string str, int index )
		{
			if( 0 <= index && index < str.Length )
			{
				char ch = str[index];
				return (ch == '\r' || ch == '\n');
			}
			else
			{
				return false;
			}
		}

		public static int NextLineHead( TextBuffer str, int searchFromIndex )
		{
			for( int i=searchFromIndex; i<str.Count; i++ )
			{
				// found EOL code?
				if( str[i] == '\r' )
				{
					if( i+1 < str.Count
						&& str[i+1] == '\n' )
					{
						return i+2;
					}

					return i+1;
				}
				else if( str[i] == '\n' )
				{
					return i+1;
				}
			}

			return -1; // not found
		}

		public static int NextLineHead( string str, int searchFromIndex )
		{
			for( int i=searchFromIndex; i<str.Length; i++ )
			{
				// found EOL code?
				if( str[i] == '\r' )
				{
					if( i+1 < str.Length
						&& str[i+1] == '\n' )
					{
						return i+2;
					}

					return i+1;
				}
				else if( str[i] == '\n' )
				{
					return i+1;
				}
			}

			return -1; // not found
		}

		public static int PrevLineHead( TextBuffer str, int searchFromIndex )
		{
			DebugUtl.Assert( searchFromIndex <= str.Count, "invalid argument; searchFromIndex is too large ("+searchFromIndex+" but str.Count is "+str.Count+")" );

			if( str.Count <= searchFromIndex )
			{
				searchFromIndex = str.Count - 1;
			}

			for( int i=searchFromIndex-1; 0<=i; i-- )
			{
				// found EOL code?
				if( str[i] == '\n' )
				{
					return i+1;
				}
				else if( str[i] == '\r' )
				{
					if( i+1 < str.Count
						&& str[i+1] == '\n' )
					{
						continue;
					}
					return i+1;
				}
			}

			return 0;
		}

		public static int PrevLineHead( string str, int searchFromIndex )
		{
			DebugUtl.Assert( searchFromIndex <= str.Length, "invalid argument; searchFromIndex is too large ("+searchFromIndex+" but str.Length is "+str.Length+")" );

			if( str.Length <= searchFromIndex )
			{
				searchFromIndex = str.Length - 1;
			}

			for( int i=searchFromIndex-1; 0<=i; i-- )
			{
				// found EOL code?
				if( str[i] == '\n' )
				{
					return i+1;
				}
				else if( str[i] == '\r' )
				{
					if( i+1 < str.Length
						&& str[i+1] == '\n' )
					{
						continue;
					}
					return i+1;
				}
			}

			return 0;
		}

		/// <summary>
		/// Find non-EOL char from specified index.
		/// Note that the char at specified index always be skipped.
		/// </summary>
		public static int PrevNonEolChar( TextBuffer str, int searchFromIndex )
		{
			for( int i=searchFromIndex-1; 0<=i; i-- )
			{
				if( IsEolChar(str[i]) != true )
				{
					// found non-EOL code
					return i;
				}
			}

			return -1; // not found
		}

		public static int GetLineLengthByCharIndex( TextBuffer text, int charIndex )
		{
			int prevLH = PrevLineHead( text, charIndex );
			int nextLH = NextLineHead( text, charIndex );
			if( nextLH == -1 )
			{
				nextLH = text.Count - 1;
			}

			return (nextLH - prevLH);
		}

		public static int GetLineLengthByCharIndex( string text, int charIndex )
		{
			int prevLH = PrevLineHead( text, charIndex );
			int nextLH = NextLineHead( text, charIndex );
			if( nextLH == -1 )
			{
				nextLH = text.Length - 1;
			}

			return (nextLH - prevLH);
		}
		#endregion
	}
}
