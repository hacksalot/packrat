/**
 * packrat.cs
 * Core atlas assembly logic for the Packrat tool.
 * Copyright (c) 2015 | gruebait (gruebait@eatenbygrues.com)
 * License: MIT
 */

using System;
using System.Linq;
using System.Drawing;
using System.Drawing.Drawing2D;
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
      return atlas.mips[0];
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
      if( _genMips ) {
        if (org.mips == null)
          org.mips = new List<Image>();
        for( int mipLevel = 1, // a modest for loop
             f = (int)Math.Pow(2,mipLevel),
             w = org.mips[0].Width / f,
             h = org.mips[0].Height / f;
             w != 0 && h != 0;
             mipLevel++,
             f = (int) Math.Pow(2, mipLevel),
             w = org.mips[0].Width / f,
             h = org.mips[0].Height / f ) {
          Bitmap mipImg = new Bitmap( w, h );
          Graphics gtemp = Graphics.FromImage( mipImg );
          gtemp.InterpolationMode = _mode;
          gtemp.DrawImage( org.mips[0], new Rectangle( 0, 0, w, h ) );
          gtemp.Dispose();
          org.mips.Add( mipImg );
        }
      }
      if( Processed != null )
        Processed( this, new ImageEventArgs( org.id, org.file ));
      return org;
    }

    // Generate empty mipmaps for the image
    void prepMips( prImg img ) {
      for (int mipLevel = 1, // mega for-loop
           f = (int) Math.Pow( 2, mipLevel ),
           w = img.mips[0].Width / f,
           h = img.mips[0].Height / f;
           w != 0 && h != 0;
           mipLevel++,
           f = (int) Math.Pow( 2, mipLevel ),
           w = img.mips[0].Width / f,
           h = img.mips[0].Height / f) {
        Bitmap mipImg = new Bitmap( w, h );
        img.mips.Add( mipImg );
      }
    }

    // Prepare the atlas
    prImg prep( string file ) {
      var img = new Bitmap( Math.Min(_count, _xu) * _texSize.Width,
                            (1 + (_count / _xu)) * _texSize.Height );
      var pimg = new prImg( file, img, -1 );
      prepMips( pimg );
      return pimg;
    }

    // Copy a specific mip-level from the source image to the atlas
    Image copyMip( int mipLevel, prImg src, prImg dest ) {
      Graphics g = Graphics.FromImage( dest.mips[ mipLevel ] );
      int w = src.mips[ mipLevel ].Width, h = src.mips[ mipLevel ].Height;
      g.InterpolationMode = _mode;
      g.DrawImage( src.mips[ mipLevel ], (src.id % _xu) * w, (src.id / _xu) * h );
      g.Dispose();
      return src.mips[ mipLevel ];
    }

    // Add a processed image file to the atlas
    prImg merge( prImg acc, prImg inst ) {
      int mip = 0;
      //inst.mips.Select(bmp => { return copyMip(mip++, inst, acc); });
      foreach (Image bmp in inst.mips) { copyMip(mip++, inst, acc); }
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
    InterpolationMode _mode = InterpolationMode.Bilinear;
    bool _genMips = true;

  }


  // Internal helper class for images / textures
  class prImg {
    public prImg( string path, Image org, int idx ) {
      mips = new List<Image>();
      file = path; mips.Add( org  ); id = idx;
    }
    public void save( string basePath ) {
      for( int mip = 0; mip < mips.Count; mip++ ) {
        saveMips( basePath, mip );
      }
    }
    void saveMips( string basePath, int mipLevel ) {
      string file = string.Format("{0}{1}.png",
        basePath, mipLevel > 0 ? "-" + mipLevel.ToString() : "");
      mips[ mipLevel ].Save( file );
    }
    public string file { get; set; }
    public List<Image> mips { get; set; }
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
