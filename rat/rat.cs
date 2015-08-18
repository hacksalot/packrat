/**
 * MainClass.cs
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
    const string header =
      "*******************************************\n" +
      "* packrat v0.1.0                          *\n" +
      "* Down 'n dirty texture atlas packing.    *\n" +
      "*******************************************\n";

    public static void Main (string[] args) {

      log( header );

      var rat = new packrat();
      rat.Loaded += ( s, e ) => log( "Loaded {0}: {1}", e.Index, fmt( e.File ) );
      rat.Processed += ( s, e ) => log( "Processed {0}: {1}", e.Index, fmt( e.File ) );
      rat.Packed += ( s, e ) => log( "Packed {0}: {1}", e.Index, fmt( e.File ) );
      rat.pack( args );

      #if DEBUG
      log("Press any key to continue...");
      Console.ReadKey();
      #endif
    }

    static string fmt( string file ) {
      int idx = file.LastIndexOfAny( new char[] { '/', '\\' } );
      return idx > -1 ? file.Substring( idx + 1 ) : file;
    }

    static void log(string msg, params object[] parms) {
      if( !_silent ) {
        Console.WriteLine(msg, parms);
      }
    }

  }
}
