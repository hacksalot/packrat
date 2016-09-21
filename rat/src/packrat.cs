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



  public class Packrat {



    /// <summary>
    /// Construct a Packrat from user options.
    /// </summary>
    public Packrat( packopts opts ) {
      _opts = opts;
    }



    /// <summary>
    /// Pack 1..N images into a mipmapped texture atlas.
    /// </summary>
    public Image Pack() {
      
      // Map: from raw file glob to list of loaded images
      var coll = ( new Glob( _opts.src ) )
        .Select( f => Load( f ) )
        .OrderBy( f => f.id )
        .Select( f => Proc(f) )
        .ToList();
      
      // Reduce: the collection of images to a single atlas
      coll.Insert( 0, Prep( _opts.dest ) );
      var atlas = coll.Aggregate( (acc, inst) => Merge(acc, inst) );

      // Save & go home
      atlas.save( atlas.file );
      return atlas.mips[ 0 ];
    }



    /// <summary>
    /// Load a source image file. Create Image and ImgEx objects.
    /// Called for each un-globbed file in the input file list.
    /// </summary>
    ImgEx Load( string file ) {  //[interim]
      Image img = Image.FromFile( file );
      if( _count == 0 )
        _texSize = img.Size;
      else if( _texSize != img.Size )
        _uniform = false;
      if( Loaded != null )
        Loaded( this, new ImageEventArgs( _count, file ) );
      return new ImgEx( file, img, _count++ );
    }



    /// <summary>
    /// Process a source image file. Create mipmaps by downscaling
    /// the original image by powers of 2 using the specified
    /// interpolation mode. Called for each un-globbed file in the
    /// input file list.
    /// </summary>
    ImgEx Proc( ImgEx org ){
      if( _opts.mipCount != 0 ) {
        if( org.mips == null )
          org.mips = new List<Image>();
        for( int mipLevel = 1, // a modest for loop
             f = (int)Math.Pow(2,mipLevel),
             w = org.mips[0].Width / f,
             h = org.mips[0].Height / f;
             w != 0 && h != 0;
             mipLevel++,
             f = (int) Math.Pow(2, mipLevel),
             w = org.mips[0].Width / f,
             h = org.mips[0].Height / f ){
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



    /// <summary>
    /// Generate empty mipmaps for the destination atlas.
    /// </summary>
    void PrepMips( ImgEx img ) {
      for (int mipLevel = 1, // totally modest for-loop
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



    /// <summary>
    /// Prepare the destination atlas.
    /// </summary>
    ImgEx Prep( string file ){
      var sz = new Size( 
        Math.Min(_count, _opts.magnitude.Width) * _texSize.Width,
        (1 + (_count / _opts.magnitude.Width)) * _texSize.Height
      );
      _packer = new ImgPacker( sz );
      var img = new Bitmap( sz.Width, sz.Height );
      var pimg = new ImgEx( file, img, -1 );
      PrepMips( pimg );
      return pimg;
    }



    /// <summary>
    /// Copy a specific mip-level from a source image to the atlas.
    /// </summary>
    Image CopyMip( int mipLevel, ImgEx src, ImgEx dest, Point? destPos ) {
      Graphics g = Graphics.FromImage( dest.mips[ mipLevel ] );
      int w = src.mips[ mipLevel ].Width, h = src.mips[ mipLevel ].Height;
      g.InterpolationMode = _opts.mode;
      Point pt = destPos.HasValue ? destPos.Value :
        new Point((src.id % _opts.magnitude.Width) * w,
                  (src.id / _opts.magnitude.Width) * h);
      g.DrawImage( src.mips[ mipLevel ], pt );
      g.Dispose();
      return src.mips[ mipLevel ];
    }



    /// <summary>
    /// Add a processed image file to the atlas.
    /// </summary>
    /// <param name="acc">The accumulator. In this case, the atlas.</param>
    /// <param name="inst">The instance. A specific image.</param>
    ImgEx Merge( ImgEx acc, ImgEx inst ) {
      if( !_uniform ){
        Rectangle pos = _packer.Pack( inst );
        if( !pos.IsEmpty )
          CopyMip( 0, inst, acc, pos.Location );
      }
      else {
        int mip = 0;
        //inst.mips.Select(bmp => { return copyMip(mip++, inst, acc); });
        foreach (Image bmp in inst.mips) { CopyMip( mip++, inst, acc, null ); }
      }
      if( Packed != null )
        Packed( this, new ImageEventArgs( inst.id, inst.file ) );
      return acc;
    }



    // Event support...
    public delegate void PackratEventHandler(object sender, ImageEventArgs e);
    public event PackratEventHandler Loaded;
    public event PackratEventHandler Processed;
    public event PackratEventHandler Packed;



    // Private data...
    int       _count = 0;       // Number of images
    bool      _uniform = true;  // Textures uniformly sized?
    Size      _texSize;         // Texture size in pixels
    packopts  _opts;            // User-provided options
    ImgPacker _packer;          // Our image packing tool.
  }



  /// <summary>
  /// Image-related event argument class.
  /// </summary>
  public class ImageEventArgs : EventArgs {
    public ImageEventArgs( int index, string file ) {
      Index = index; File = file;
    }
    public int Index { get; set; }
    public string File { get; set; }
  }



  /// <summary>
  /// Packrat user options. Objectified from command line.
  /// </summary>
  public class packopts {
    public string             src = "*.*";
    public string             dest = "atlas";
    public int                mipCount = -1;
    public Size               magnitude = new Size(1, 1);
    public bool               fourTap = false;
    public int                xu = 0;
    public int                yu = 0;
    public InterpolationMode  mode = InterpolationMode.Bilinear;
  }



}
