/**
 * packrat.cs
 * Core atlas assembly logic for the Packrat tool.
 * Copyright (c) 2015 | gruebait (gruebait@eatenbygrues.com)
 * License: MIT
 */

using System;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;

namespace prc {

  public class packrat
  {

    // Pack 0..N images into the atlas
    public static Image pack( string[] args ) {

      if( args.Length != 2 )
        throw new ArgumentException();

      // Run the transformation(s)
      int index = 0;
      prImg atlas = (new glob( args[1] )).
        Select( f => load( f, index++ ) ).
        Concat( new [] { load( args[0], -1) } ).
        OrderBy( i => i.id ).
        Select( f => pre( f ) ).
        OrderBy( i => i.id ).
        Aggregate( (acc, inst) => merge( acc, inst ) );
      return atlas.raw;
    }

    // Load a source image file
    static prImg load( string file, int index ) {
      if( index != -1 )
        Console.WriteLine("Loading texture #{0} ({1})", index, file);
      var rep = index != 0 ? Image.FromFile( file ) : new Bitmap( file );
      return new prImg( file, rep, index );
    }

    // Preprocess a source image file
    static prImg pre( prImg org ) {
      if( org.id != -1 )
        Console.WriteLine("Processing #{0} ({1})", org.id, org.file);
      return org;
    }

    // Add a processed image file to the atlas
    static prImg merge( prImg acc, prImg inst ) {
      Console.WriteLine("Merging {0} onto {1}", inst.file, acc.file);
      return acc;
    }

  }


  // Internal helper class for images / textures
  class prImg {
    public prImg( string path, Image org, int idx ) {
      file = path; raw = org; id = idx;
    }
    public void save( string path ) { raw.Save( path ); }
    public string file { get; set; }
    public Image raw { get; set; }
    public int id { get; set; }
  }

}
