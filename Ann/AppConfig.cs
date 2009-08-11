﻿// 2009-08-11
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using Encoding = System.Text.Encoding;
using Sgry.Azuki;

namespace Sgry.Ann
{
	class AppConfig
	{
		static string _IniFilePath;
		
		public static Font Font = new Font( "Courier New", 11, FontStyle.Regular );
		public static Size WindowSize = new Size( 360, 400 );
		public static bool DrawsEolCode = true;
		public static bool DrawsFullWidthSpace = true;
		public static bool DrawsSpace = true;
		public static bool DrawsTab = true;
		public static bool HighlightsCurrentLine = true;
		public static bool ShowsLineNumber = true;
		public static int TabWidth = 8;
		public static ViewType ViewType = ViewType.Proportional;
		public static Ini Ini = new Ini();

		/// <summary>
		/// Loads application config file.
		/// </summary>
		public static void Load()
		{
			string str;
			int width, height;
			
			try
			{
				Ini.Load( IniFilePath, Encoding.UTF8 );

				int fontSize = Ini.GetInt( "Default", "FontSize", 8, Int32.MaxValue, 11 );
				str = Ini.Get( "Default", "Font", null );
				width = Ini.GetInt( "Default", "WindowWidth", 100, Int32.MaxValue, 300 );
				height = Ini.GetInt( "Default", "WindowHeight", 100, Int32.MaxValue, 480 );

				AppConfig.Font					= new Font( str, fontSize, FontStyle.Regular );
				AppConfig.WindowSize			= new Size( width, height );
				AppConfig.DrawsEolCode			= Ini.Get( "Default", "DrawsEolCode", true );
				AppConfig.DrawsFullWidthSpace	= Ini.Get( "Default", "DrawsFullWidthSpace", true );
				AppConfig.DrawsSpace			= Ini.Get( "Default", "DrawsSpace", true );
				AppConfig.DrawsTab				= Ini.Get( "Default", "DrawsTab", true );
				AppConfig.HighlightsCurrentLine	= Ini.Get( "Default", "HighlightsCurrentLine", true );
				AppConfig.ShowsLineNumber		= Ini.Get( "Default", "ShowsLineNumber", true );
				AppConfig.TabWidth				= Ini.GetInt( "Default", "TabWidth", 0, 100, 8 );
				AppConfig.ViewType				= Ini.Get( "Default", "ViewType", ViewType.Proportional );
			}
			catch
			{}
		}

		/// <summary>
		/// Saves application configuration.
		/// </summary>
		public static void Save()
		{
			try
			{
				Ini.Set( "Default", "FontSize",				AppConfig.Font.Size );
				Ini.Set( "Default", "Font",					AppConfig.Font.Name );
				Ini.Set( "Default", "WindowWidth",			AppConfig.WindowSize.Width );
				Ini.Set( "Default", "WindowHeight",			AppConfig.WindowSize.Height );
				Ini.Set( "Default", "DrawsEolCode",			AppConfig.DrawsEolCode );
				Ini.Set( "Default", "DrawsFullWidthSpace",	AppConfig.DrawsFullWidthSpace );
				Ini.Set( "Default", "DrawsSpace",			AppConfig.DrawsSpace );
				Ini.Set( "Default", "DrawsTab",				AppConfig.DrawsTab );
				Ini.Set( "Default", "HighlightsCurrentLine",AppConfig.HighlightsCurrentLine );
				Ini.Set( "Default", "ShowsLineNumber",		AppConfig.ShowsLineNumber );
				Ini.Set( "Default", "TabWidth",				AppConfig.TabWidth );
				Ini.Set( "Default", "ViewType",				AppConfig.ViewType );

				Ini.Save( IniFilePath, Encoding.UTF8, "\r\n" );
			}
			catch
			{}
		}

		#region Utilities
		/// <summary>
		/// Gets INI file path.
		/// </summary>
		static string IniFilePath
		{
			get
			{
				if( _IniFilePath == null )
				{
					Assembly exe = Assembly.GetExecutingAssembly();
					string exePath = exe.GetModules()[0].FullyQualifiedName;
					string exeDirPath = Path.GetDirectoryName( exePath );
					_IniFilePath = Path.Combine( exeDirPath, "Ann.ini" );
				}
				return _IniFilePath;
			}
		}
		#endregion
	}
}
