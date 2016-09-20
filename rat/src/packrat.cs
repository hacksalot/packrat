/**
 * packrat.cs
 * Core atlas assembly logic for the Packrat tool.
 * Copyright (c) 2015-16 | hacksalot <hacksalot@indevious.com>
 * License: MIT
 */

using System;
using System.Linq;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.Generic;

namespace rat {

  public class packrat
  {

    public packrat( packopts opts ) {
      _opts = opts;
    }

    // Pack 0..N images into the atlas
    public Image pack() {

      // Run the transformation(s)
      var coll = (new glob( _opts.src ))
        .Select(f => load( f ))
        .OrderBy(f => f.id)
        .Select(f => proc(f))
        .ToList();

      // Run the reduction
      coll.Insert( 0, prep( _opts.dest ) );
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
      if( _opts.mipCount != 0 ) {
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
          gtemp.InterpolationMode = _opts.mode;
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
      var img = new Bitmap( Math.Min(_count, _opts.magnitude.Width) * _texSize.Width,
                            (1 + (_count / _opts.magnitude.Width)) * _texSize.Height );
      var pimg = new prImg( file, img, -1 );
      prepMips( pimg );
      return pimg;
    }

    // Copy a specific mip-level from the source image to the atlas
    Image copyMip( int mipLevel, prImg src, prImg dest ) {
      Graphics g = Graphics.FromImage( dest.mips[ mipLevel ] );
      int w = src.mips[ mipLevel ].Width, h = src.mips[ mipLevel ].Height;
      g.InterpolationMode = _opts.mode;
      g.DrawImage( src.mips[ mipLevel ], (src.id % _opts.magnitude.Width) * w, (src.id / _opts.magnitude.Width) * h );
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

    int _count = 0; // # of tiles
    Size _texSize;  // texture size in pixels
    packopts _opts;

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

  // And some options for the packer...
  public class packopts {
    public string src = "*.*";
    public string dest = "atlas";
    public int mipCount = -1;
    public Size magnitude = new Size(1, 1);
    public bool fourTap = false;
    public int xu = 0;
    public int yu = 0;
    public InterpolationMode mode = InterpolationMode.Bilinear;
  }

}
