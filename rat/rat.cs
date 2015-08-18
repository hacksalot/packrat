/**
 * rat.cs
 * Application logic for packrat.
 * Copyright (c) 2015 | gruebait
 * License: MIT
 */

using System;

namespace prc
{
  class MainClass
  {
    static bool _silent = false;

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
      log( String.Format( pr.console.rat.msgHeader, pr.console.rat.msgTitle, pr.console.rat.msgByline ) );
      var rat = new packrat();
      rat.Loaded += (s, e) => log( pr.console.rat.msgLoaded, e.Index, fmt(e.File) );
      rat.Processed += (s, e) => log( pr.console.rat.msgProcessed, e.Index, fmt(e.File) );
      rat.Packed += (s, e) => log( pr.console.rat.msgPacked, e.Index, fmt(e.File) );
      rat.pack( args );
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
