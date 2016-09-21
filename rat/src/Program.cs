/**
 * rat.cs
 * Application logic for packrat.
 * Copyright (c) 2015-16 | hacksalot <hacksalot@indevious.com>
 * License: MIT
 */

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using rat;

namespace rat.con {



  class MainClass {
    


    /// <summary>
    /// Entry point for the command-line app.
    /// </summary>
    public static void Main( string[] args ) {
      try {
        Go( args );
      }
      catch( Exception ex ) {
        Log( str.error + ex.Message );
      }
      #if DEBUG
      Console.WriteLine( str.cont );
      Console.ReadKey();
      #endif
    }



    /// <summary>
    /// Perform a single pass of the application.
    /// </summary>
    static void Go( string[] args ) {
      Log( String.Format(str.header, str.title, str.byline) );
      if( args.Length == 0 ) throw new ArgumentException( str.help );
      // Parse cmd-line options
      packopts opts = new packopts();
      int argidx = 0;
      args.Select( a => Arg(a, opts, args, argidx++) ).ToList();
      // Build the atlas
      var r = new Packrat( opts );
      r.Loaded += (s, e) => Log( str.loaded, e.Index, Fmt(e.File) );
      r.Processed += (s, e) => Log( str.proc, e.Index, Fmt(e.File) );
      r.Packed += (s, e) => Log( str.packed , e.Index, Fmt(e.File) );
      r.Pack();
    }



    /// <summary>
    /// Process command-line arguments.
    /// </summary>
    static string Arg( string a, packopts opts, string[] args, int idx ) {
      switch ( idx ) {
        case 0: opts.dest = a; break;
        case 1: opts.src = a; break;
        default:
          switch( a ) {
            case "-s": _silent = true; break;
            case "-nomips": opts.mipCount = 0; break;
            case "-4tap": opts.fourTap = true; break;
            case "-tx": opts.magnitude.Width = int.Parse( args[idx + 1] ); break;
            case "-ty": opts.magnitude.Height = int.Parse( args[idx + 1] ); break;
            case "-i": opts.mode = (InterpolationMode) Enum.Parse( opts.mode.GetType(), args[idx + 1], true ); break;
            default: break;
          }
          break;
      }
      _last = a;
      return a;
    }



    /// <summary>
    /// Format a filename.
    /// </summary>
    static string Fmt( string file ) {
      int idx = file.LastIndexOfAny( new char[] { '/', '\\' } );
      return idx > -1 ? file.Substring( idx + 1 ) : file;
    }



    /// <summary>
    /// Log a message to the console, respecting the "silent" flag.
    /// </summary>
    static void Log( string msg, params object[] parms ) {
      if( !_silent ) { Console.WriteLine(msg, parms); }
    }



    /// <summary>Resource string aliases.</summary>
    class str {
      public static string error    =   rat.con.Resources.mError;
      public static string header   =   rat.con.Resources.mHeader;
      public static string title    =   rat.con.Resources.mTitle;
      public static string cont     =   rat.con.Resources.mContinue;
      public static string byline   =   rat.con.Resources.mByline;
      public static string loaded   =   rat.con.Resources.mLoaded;
      public static string proc     =   rat.con.Resources.mProcessed;
      public static string packed   =   rat.con.Resources.mPacked;
      public static string missing  =   rat.con.Resources.mMissingInput;
      public static string help     =   rat.con.Resources.mHelp;
    }



    static bool     _silent = false;
    static string   _last;
  }
}
