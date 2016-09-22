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


  // Internal helper class for images / textures
  public class ImgAsset
  {
    public string       Moniker { get; set; }
    public string       File { get; set; }
    public List<Bitmap> Mips { get; set; }
    public Bitmap       Org  { get; set; }
    public int          ID   { get; set; }
    public Rectangle    Rect { get; set; }

    public ImgAsset( string path, Bitmap o, int idx ) {
      File = path;
      Moniker = Path.GetFileName( path );
      Org = o;
      ID = idx;
      Rect = new Rectangle( Point.Empty, o.Size );
      Mips = new List<Bitmap>();
      Mips.Add(o);
    }

    protected void SaveMip( string basePath, int mipLevel ) {
      string file = string.Format("{0}{1}.png",
        basePath, mipLevel > 0 ? "-" + mipLevel.ToString() : "");
      Mips[mipLevel].Save( file );
    }
  }
}
