﻿// file: WatchPattern.cs
// brief: Represents watching text pattern.
//=========================================================
using System;
using System.Collections.Generic;
using Regex = System.Text.RegularExpressions.Regex;


namespace Sgry.Azuki
{
	/// <summary>
	/// Set of WatchPattern objects.
	/// </summary>
	/// <see cref="Sgry.Azuki.WatchPattern">WatchPattern class</see>
	public class WatchPatternSet : IEnumerable<WatchPattern>
	{
		readonly Dictionary<int, WatchPattern> _Patterns = new Dictionary<int,WatchPattern>();

		/// <summary>
		/// Registers a text pattern to be watched and automatically marked.
		/// </summary>
		/// <param name="pattern">
		/// The pattern of the text to be watched and automatically marked.
		/// </param>
		/// <exception cref="System.ArgumentNullException"/>
		/// <seealso cref="Sgry.Azuki.WatchPatternSet.Unregister"/>
		public void Register( WatchPattern pattern )
		{
			if( pattern == null )
				throw new ArgumentNullException( "pattern" );

			_Patterns[ pattern.MarkingID ] = pattern;
		}

		/// <summary>
		/// Registers a text pattern to be watched and automatically marked.
		/// </summary>
		/// <param name="markingID">
		///   The marking ID to be marked for each found matching patterns.
		/// </param>
		/// <param name="pattern">
		///   The pattern of the text to be watched and automatically marked.
		/// </param>
		/// <exception cref="System.ArgumentException">
		///   Parameter '<paramref name="markingID"/>' is invalid or not registered.
		/// </exception>
		/// <seealso cref="Sgry.Azuki.WatchPatternSet.Unregister"/>
		/// <remarks>
		///   <para>
		///   If <paramref name="pattern"/> is an empty string, nothing won't be matched with it
		///   so that every character in a document will be unmarked.
		///   </para>
		/// </remarks>
		public void Register( int markingID, Regex pattern )
		{
			_Patterns[ markingID ] = new WatchPattern( markingID, pattern );
		}

		/// <summary>
		/// Unregister a watch-pattern by markingID.
		/// </summary>
		public void Unregister( int markingID )
		{
			_Patterns.Remove( markingID );
		}

		/// <summary>
		/// Gets a watch-pattern by marking ID.
		/// </summary>
		[Obsolete("Use indexer (WatchPatternSet.Items) instead.")]
		public WatchPattern Get( int markingID )
		{
			return this[ markingID ];
		}

		/// <summary>
		/// Gets a watch-pattern by marking ID.
		/// </summary>
		public WatchPattern this[ int markingID ]
		{
			get
			{
				WatchPattern pattern;
				if( _Patterns.TryGetValue( markingID, out pattern ) )
					return pattern;
				else
					return null;
			}
		}

		#region IEnumerator
		/// <summary>
		/// Gets the enumerator that iterates through the WatchPatternSet.
		/// </summary>
		public IEnumerator<WatchPattern> GetEnumerator()
		{
			return _Patterns.Values.GetEnumerator();
		}

		/// <summary>
		/// Gets the enumerator that iterates through the WatchPatternSet.
		/// </summary>
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return _Patterns.Values.GetEnumerator();
		}
		#endregion
	}

	/// <summary>
	/// Text pattern to be watched and marked automatically.
	/// </summary>
	/// <remarks>
	///   <para>
	///   This class represents a text pattern which should always be watched by Azuki.
	///   By registering these watching patterns to
	///   <see cref="Sgry.Azuki.Document.WatchPatterns">Document.WatchPatterns</see>,
	///   such patterns will be automatically marked by Azuki as soon as it is graphically drawn
	///   so that such patterns will be able to distinguished visually and logically too.
	///   </para>
	///   <para>
	///   Most typical usage of this feature is emphasizing text patterns
	///   visually which the user is currently searching for.
	///   </para>
	/// </remarks>
	/// <example>
	///   <para>
	///   Next example code illustrates how to use WatchPattern
	///   to emphasize text search results in a document.
	///   </para>
	///   <para>
	///   Firstly of all, register how the matched patterns should be
	///   decorated in initialization part.
	///   </para>
	///   <code lang="C#">
	///   // Use yellow background for the text pattern
	///   // which matched to the text search criteria
	///   // (using marking ID 30.)
	///   Marking.Register( new MarkingInfo(30, "Search result") );
	///   azukiControl.ColorScheme.SetMarkingDecoration(
	///           30, new BgColorTextDecoration( Color.Yellow )
	///       );
	///   </code>
	///   <para>
	///   Secondly, update the WatchPattern every time the search criteria
	///   was changed.
	///   </para>
	///   <code lang="C#">
	///   // Show a dialog to let user input the pattern to search
	///   Regex pattern;
	///   DialogResult result = ShowFindDialog( out pattern );
	///   if( result != DialogResult.OK )
	///       return;
	///   
	///   // Update the text patterns to be watched
	///   doc.WatchPatterns.Register(
	///           new WatchPattern( 30, pattern )
	///       );
	///   </code>
	/// </example>
	public class WatchPattern
	{
		#region Init / Dispose
		/// <summary>
		/// Creates a new instance.
		/// </summary>
		public WatchPattern()
		{}

		/// <summary>
		/// Creates a new instance.
		/// </summary>
		public WatchPattern( WatchPattern other )
			: this( other.MarkingID, other.Pattern )
		{}

		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="markingID">
		///   The marking ID to be marked for each found matching patterns.
		/// </param>
		/// <param name="patternToBeWatched">
		///   The pattern to be watched and to be marked with '<paramref name="markingID"/>.'
		/// </param>
		/// <exception cref="System.ArgumentException">
		///   Parameter '<paramref name="markingID"/>' is invalid or not registered.
		/// </exception>
		/// <exception cref="System.ArgumentNullException">
		///   Parameter '<paramref name="patternToBeWatched"/>' is null.
		/// </exception>
		/// <remarks>
		///   <para>
		///   If <paramref name="patternToBeWatched"/> is an empty string, nothing won't be matched
		///   with it so that every character in a document will be unmarked.
		///   </para>
		/// </remarks>
		public WatchPattern( int markingID, Regex patternToBeWatched )
		{
			if( Marking.GetMarkingInfo(markingID) == null )
				throw new ArgumentException( "Specified marking ID (" + markingID + ") is"
											 + " not registered.",
											 "markingID" );
			if( patternToBeWatched == null )
				throw new ArgumentNullException( "patternToBeWatched",
												 "The pattern must not be null." );

			MarkingID = markingID;
			Pattern = patternToBeWatched;
		}
		#endregion

		#region Properties

		/// <summary>
		/// The marking ID to be marked for each found matching patterns.
		/// </summary>
		public int MarkingID { get; set; }

		/// <summary>
		/// The pattern to be watched and to be marked automatically.
		/// (accepts null.)
		/// </summary>
		/// <remarks>
		///   <para>
		///   This property gets or sets the pattern to be watched.
		///   If the pattern is null or regular expression is an empty string,
		///   Azuki simply ignores the watch pattern.
		///   </para>
		/// </remarks>
		public Regex Pattern { get; set; }
		#endregion
	}
}
