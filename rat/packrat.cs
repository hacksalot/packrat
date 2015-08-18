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
    public Image pack( string[] args ) {

      // Handle params
      if( args.Length != 3 )
        throw new ArgumentException();
      _xu = _yu = int.Parse( args[2] );

      // Run the transformation(s)
      var coll = (new glob(args[1]))
        .Select(f => load( f ))
        .OrderBy(f => f.id)
        .Select(f => proc(f))
        .ToList();

      // Run the reduction
      coll.Insert( 0, prep( args[0] ) );
      var atlas = coll.Aggregate((acc, inst) => merge(acc, inst));

      // Save and we're done
      atlas.save( atlas.file );
      return atlas.raw;
    }

    // Load a source image file
    prImg load( string file ) {
      Image img = Image.FromFile( file );
      if( _count == 0 )
          _texSize = img.Size;
      else if( _texSize != img.Size )
          throw new NotImplementedException();
      if( Loaded != null )
        Loaded( this, new ImageEventArgs( _count, file ) );
      return new prImg( file, img, _count++ );
    }

    // Process a source image file
    prImg proc( prImg org ) {
      if( Processed != null )
        Processed( this, new ImageEventArgs( org.id, org.file ));
      return org;
    }

    // Prepare the atlas
    prImg prep( string file ) {
      var img = new Bitmap( Math.Min(_count, _xu) * _texSize.Width, 
                            (1 + (_count / _xu)) * _texSize.Height );
      _g = Graphics.FromImage( img );
      return new prImg( file, img, -1 );
    }

    // Add a processed image file to the atlas
    prImg merge( prImg acc, prImg inst ) {
      int w = inst.raw.Size.Width, h = inst.raw.Size.Height;
      _g.DrawImage( inst.raw, (inst.id % _xu) * w, (inst.id / _yu) * h,
                   new Rectangle( 0, 0, w, h ), GraphicsUnit.Pixel );
      if( Packed != null )
        Packed( this, new ImageEventArgs( inst.id, inst.file ) );
      return acc;
    }

    // Event support
    public delegate void PackratEventHandler(object sender, ImageEventArgs e);
    public event PackratEventHandler Loaded;
    public event PackratEventHandler Processed;
    public event PackratEventHandler Packed;

    int _xu = 0;    // horz tile count
    int _yu = 0;    // vert tile count
    int _count = 0; // # of tiles
    Size _texSize;  // texture size in pixels
    Graphics _g;    // Graphics object for atlas

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

  // Standard .NET event args
  public class ImageEventArgs : EventArgs {
    public ImageEventArgs( int index, string file ) {
      Index = index; File = file;
    }
    public int Index { get; set; }
    public string File { get; set; }
  }

}
