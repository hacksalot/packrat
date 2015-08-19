/**
 * rat.cs
 * Application logic for packrat.
 * Copyright (c) 2015 | gruebait
 * License: MIT
 */

using System;
using System.Drawing;
using System.Linq;

namespace prc
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
        log( pr.console.rat.msgError + ex.Message );
      }
      #if DEBUG
      Console.WriteLine(pr.console.rat.msgContinue);
      Console.ReadKey();
      #endif
    }

    static void go( string[] args ) {
      packopts opts = new packopts();
      int argidx = 0;
      args.Select( a => arg(a, opts, args, argidx++) ).ToList();
      log( String.Format( pr.console.rat.msgHeader, pr.console.rat.msgTitle, pr.console.rat.msgByline ) );
      var rat = new packrat( opts );
      rat.Loaded += (s, e) => log( pr.console.rat.msgLoaded, e.Index, fmt(e.File) );
      rat.Processed += (s, e) => log( pr.console.rat.msgProcessed, e.Index, fmt(e.File) );
      rat.Packed += (s, e) => log( pr.console.rat.msgPacked, e.Index, fmt(e.File) );
      rat.pack();
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

  }
}
