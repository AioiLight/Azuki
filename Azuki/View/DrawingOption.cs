using System;

namespace Sgry.Azuki
{
	/// <summary>
	/// Option flags to specify how Azuki draws text area.
	/// </summary>
	[Flags]
	public enum DrawingOption : int
	{
		/// <summary>Draws space character.</summary>
		DrawsSpace				= 0x0001,

		/// <summary>Draws full-width space character.</summary>
		DrawsFullWidthSpace		= 0x0002,

		/// <summary>Draws tab character.</summary>
		DrawsTab				= 0x0004,

		/// <summary>Draws EOL (End Of Line) code.</summary>
		DrawsEol				= 0x0008,

		/// <summary>Shows line number area.</summary>
		HighlightCurrentLine	= 0x0010,

		/// <summary>Shows line number area.</summary>
		ShowsLineNumber			= 0x0020,

		/// <summary>Shows horizontal ruler.</summary>
		ShowsHRuler				= 0x0040,

		/// <summary>Draws EOF (End Of File) mark.</summary>
		DrawsEof				= 0x0080,

		/// <summary>Shows 'dirt bar'.</summary>
		ShowsDirtBar			= 0x0100,

		/// <summary>Highlights matched bracket.</summary>
		HighlightsMatchedBracket= 0x0200,

		/// <summary>Whether to include wrapped screen lines for line numbering.</summary>
		UseScreenLineNumber	= 0x0400,

        /// <summary>
        /// Gets or sets c to start zero horizontal ruler or one.
        /// </summary>
        HRulerStartsFromZero = 0x800,
	}
}
