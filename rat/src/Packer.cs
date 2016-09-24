/**
 * Packer.cs
 * Atlas and spritesheet assembly logic for Packrat.
 * Copyright (c) 2015-16 | hacksalot <hacksalot@indevious.com>
 * License: MIT
 */

using System;
using System.Linq;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.IO;

namespace rat {



  public class Packer {



    /// <summary>
    /// Construct a Packer from user options.
    /// </summary>
    public Packer( packopts opts ) {
      _opts = opts;
    }



    /// <summary>
    /// Pack 1..N images into a mipmapped texture atlas.
    /// </summary>
    public Image Pack() {
      
      // Map: from raw file glob to list of loaded images
      var coll = _opts.src
        .SelectMany( g => Expand(g) )
        .Select( f => Load( f ) )
        .Where( f => f.Status == AssetErrorState.Success )
        .OrderBy( f => f.ID )
        .Select( f => Proc(f) )
        .ToList();
      
      // Reduce: the collection of images to a single atlas
      coll.Insert( 0, Prep( _opts.dest ) );
      var atlas = coll.Aggregate( (a, i) => Merge(a, i) ) as ImgAtlas;

      // Save & go home
      atlas.Save( atlas.File );
      return atlas.Mips[ 0 ];
    }



    IEnumerable<string> Expand( string path ) {
      if( path.Contains('*') || path.Contains('?') ) {
        return new FileGlob( path );
      }
      else {
        return new string[] { path };
      }
    }



    /// <summary>
    /// Load a source image file. Create Image and ImgAsset objects.
    /// Called for each un-globbed/expanded file in the input file list.
    /// </summary>
    ImgAsset Load( string file ) {
      ImgAsset ret = null;
      // Load the file into an image + asset object
      if( File.Exists( file )) {
        Bitmap img = new Bitmap(file);
        ret = new ImgAsset( file, img, _count );
        // Infer: are all textures the same size?
        if (_count++ == 0)
          _texSize = img.Size;
        else if (_texSize != img.Size)
          _uniform = false;
      }
      else {
        ret = new ImgAsset( file, AssetErrorState.FileMissing );
      }
      // Fire the Loaded event
      if( Loaded != null )
        Loaded( this, new ImageEventArgs( ret ) );
      return ret;
    }



    /// <summary>
    /// Process a source image file. Create mipmaps by downscaling
    /// the original image by powers of 2 using the specified
    /// interpolation mode. Called for each un-globbed file in the
    /// input file list.
    /// </summary>
    ImgAsset Proc( ImgAsset org ){
      if( _opts.mipCount != 0 ) {
        if( org.Mips == null )
          org.Mips = new List<Bitmap>();
        for( int mipLevel = 1, // a modest for loop
             f = (int)Math.Pow(2,mipLevel),
             w = org.Mips[0].Width / f,
             h = org.Mips[0].Height / f;
             w != 0 && h != 0;
             mipLevel++,
             f = (int) Math.Pow(2, mipLevel),
             w = org.Mips[0].Width / f,
             h = org.Mips[0].Height / f ){
          Bitmap mipImg = new Bitmap( w, h );
          Graphics gtemp = Graphics.FromImage( mipImg );
          gtemp.InterpolationMode = _opts.mode;
          gtemp.DrawImage( org.Mips[0], new Rectangle( 0, 0, w, h ) );
          gtemp.Dispose();
          org.Mips.Add( mipImg );
        }
      }
      if( Processed != null )
        Processed( this, new ImageEventArgs( org ));
      return org;
    }



    /// <summary>
    /// Generate empty mipmaps for the destination atlas.
    /// </summary>
    void PrepMips( ImgAsset img ) {
      for (int mipLevel = 1, // totally modest for-loop
           f = (int) Math.Pow( 2, mipLevel ),
           w = img.Mips[0].Width / f,
           h = img.Mips[0].Height / f;
           w != 0 && h != 0;
           mipLevel++,
           f = (int) Math.Pow( 2, mipLevel ),
           w = img.Mips[0].Width / f,
           h = img.Mips[0].Height / f) {
        Bitmap mipImg = new Bitmap( w, h );
        img.Mips.Add( mipImg );
      }
    }



    /// <summary>
    /// Prepare the destination atlas.
    /// </summary>
    ImgAtlas Prep( string file ){
      var sz = new Size( 
        Math.Min(_count, _opts.magnitude.Width) * _texSize.Width,
        (1 + (_count / _opts.magnitude.Width)) * _texSize.Height
      );
      _packer = new Organizer( sz );
      var img = new Bitmap( sz.Width, sz.Height );
      var atlas = new ImgAtlas( file, img, -1 );
      PrepMips( atlas );
      return atlas;
    }



    /// <summary>
    /// Copy a specific mip-level from a source image to the atlas.
    /// </summary>
    Image CopyMip( int mipLevel, ImgAsset src, ImgAsset dest, Point? destPos ) {
      Graphics g = Graphics.FromImage( dest.Mips[ mipLevel ] );
      int w = src.Mips[ mipLevel ].Width, h = src.Mips[ mipLevel ].Height;
      g.InterpolationMode = _opts.mode;
      Point pt = destPos.HasValue ? destPos.Value :
        new Point((src.ID % _opts.magnitude.Width) * w,
                  (src.ID / _opts.magnitude.Width) * h);
      src.Rect = new Rectangle( pt, src.Rect.Size );
      g.DrawImage( src.Mips[ mipLevel ], pt );
      g.Dispose();
      return src.Mips[ mipLevel ];
    }



    /// <summary>
    /// Add a processed image file to the atlas.
    /// </summary>
    /// <param name="acc">The accumulator. In this case, the atlas.</param>
    /// <param name="inst">The instance. A specific image.</param>
    ImgAsset Merge( ImgAsset acc, ImgAsset inst ) {
      ImgAtlas atlas = acc as ImgAtlas;
      if( !_uniform ){
        Rectangle pos = _packer.Pack( inst );
        if( !pos.IsEmpty ){
          int mip = 0;
          foreach (Image bmp in inst.Mips) {
            int pw = (int) Math.Pow(2.0, mip);
            Point pt = new Point(pos.Location.X / pw, pos.Location.Y / pw);
            CopyMip( mip++, inst, atlas, pt );
          }
        }
      }
      else {
        int mip = 0;
        //inst.mips.Select(bmp => { return copyMip(mip++, inst, acc); });
        foreach (Image bmp in inst.Mips) { CopyMip( mip++, inst, atlas, null ); }
      }
      atlas.Add( inst );
      if( Packed != null )
        Packed( this, new ImageEventArgs( inst ) );
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
    Organizer _packer;          // Our image packing tool.
  }



  /// <summary>
  /// Image-related event argument class.
  /// </summary>
  public class ImageEventArgs : EventArgs {
    public ImageEventArgs( ImgAsset img ) {
      Image = img;
    }
    public ImgAsset Image { get; set; }
  }



  /// <summary>
  /// Packrat user options. Objectified from command line.
  /// </summary>
  public class packopts {
    public List<string>       src = new List<string>();
    public string             dest = "atlas";
    public int                mipCount = -1;
    public Size               magnitude = new Size(1, 1);
    public bool               fourTap = false;
    public int                xu = 0;
    public int                yu = 0;
    public InterpolationMode  mode = InterpolationMode.Bilinear;
  }



}
