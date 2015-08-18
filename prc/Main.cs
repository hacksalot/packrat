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
    const string header =
      "*******************************************\n" +
      "* packrat v0.1.0                          *\n" +
      "* Down 'n dirty texture atlas packing.    *\n" +
      "*******************************************\n";

    public static void Main (string[] args) {
      Console.WriteLine( header );
      packrat.pack( args );
    }

  }
}
