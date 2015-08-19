/**
 * rat.cs
 * Application logic for packrat.
 * Copyright (c) 2015 | gruebait
 * License: MIT
 */

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using rat;

namespace rat.con
{
  class MainClass
  {
    static bool _silent = false;
    static string _last;

    public static void Main (string[] args) {
      try {
        go( args );
      }
      catch( Exception ex ) {
        log( str.error + ex.Message );
      }
      #if DEBUG
			Console.WriteLine( str.cont );
      Console.ReadKey();
      #endif
    }

    static void go( string[] args ) {
      packopts opts = new packopts();
      int argidx = 0;
      args.Select( a => arg(a, opts, args, argidx++) ).ToList();
      log(String.Format( str.header, str.title, str.byline ));
      var r = new packrat( opts );
      r.Loaded += (s, e) => log( str.loaded, e.Index, fmt(e.File) );
      r.Processed += (s, e) => log( str.proc, e.Index, fmt(e.File) );
      r.Packed += (s, e) => log( str.packed , e.Index, fmt(e.File) );
      r.pack();
    }

    static string arg( string a, packopts opts, string[] args, int idx ) {
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

    static string fmt( string file ) {
      int idx = file.LastIndexOfAny( new char[] { '/', '\\' } );
      return idx > -1 ? file.Substring( idx + 1 ) : file;
    }

    static void log( string msg, params object[] parms ) {
      if( !_silent ) {
        Console.WriteLine(msg, parms);
      }
    }

    class str {
      public static string error  = rat.con.Resources.msgError;
	    public static string header = rat.con.Resources.msgHeader;
	    public static string title  = rat.con.Resources.msgTitle;
	    public static string cont   = rat.con.Resources.msgContinue;
	    public static string byline = rat.con.Resources.msgByline;
	    public static string loaded = rat.con.Resources.msgLoaded;
	    public static string proc   = rat.con.Resources.msgProcessed;
	    public static string packed = rat.con.Resources.msgPacked;
    }

  }
}
