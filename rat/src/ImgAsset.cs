/**
 * ImgAsset.cs
 * Internal image tracking class for Packrat.
 * Copyright (c) 2015-16 | hacksalot <hacksalot@indevious.com>
 * License: MIT
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace rat {



  /// <summary>
  /// Packrat image asset error codes and such.
  /// </summary>
  public enum AssetErrorState {
    Success = 0,
    FileMissing = 0x1
  }



  /// <summary>
  /// Internal asset class.
  /// </summary>
  public class ImgAsset
  {
    public string       Moniker { get; set; }
    public string       File { get; set; }
    public List<Bitmap> Mips { get; set; }
    public Bitmap       Org  { get; set; }
    public int          ID   { get; set; }
    public Rectangle    Rect { get; set; }
    public AssetErrorState Status { get; set; }


    public ImgAsset(string path, AssetErrorState es)
    {
      File = path;
      Moniker = Path.GetFileName( path );
      Rect = Rectangle.Empty;
      Status = es;
    }

    public ImgAsset( string path, Bitmap o, int idx ) {
      File = path;
      Moniker = Path.GetFileName( path );
      Org = o;
      ID = idx;
      Rect = new Rectangle( Point.Empty, o != null ? o.Size : Size.Empty );
      Mips = new List<Bitmap>();
      Mips.Add(o);
    }

    // https://en.wikipedia.org/wiki/Power_of_two#Fast_algorithm_to_check_if_a_positive_number_is_a_power_of_two
    public bool IsPowerOfTwo {
      get {
        bool horz2 = ( Rect.Width & (Rect.Width - 1 )) == 0;
        bool vert2 = ( Rect.Height & (Rect.Height - 1 )) == 0;
        return horz2 && vert2;
      }
    }

    protected void SaveMip( string basePath, int mipLevel ) {
      string file = string.Format("{0}{1}.png",
        basePath, mipLevel > 0 ? "-" + mipLevel.ToString() : "");
      Mips[mipLevel].Save( file );
    }
  }
}
