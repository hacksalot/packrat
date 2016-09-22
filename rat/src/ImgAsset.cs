/**
 * ImgAsset.cs
 * Internal image tracking class for Packrat.
 * Copyright (c) 2015-16 | hacksalot <hacksalot@indevious.com>
 * License: MIT
 */

using System;
using System.Collections.Generic;
using System.Drawing;

namespace rat {


  // Internal helper class for images / textures
  public class ImgAsset
  {



    public ImgAsset( string path, Bitmap o, int idx ) {
      File = path;
      Org = o;
      ID = idx;
      Rect = new Rectangle( Point.Empty, o.Size );
      Mips = new List<Bitmap>();
      Mips.Add(o);
    }



    public void Save( string basePath ) {
      for( int mip = 0; mip < Mips.Count; mip++ ) {
        SaveMip( basePath, mip );
      }
      Writer dw = new Writer();
      dw.WriteDDS( basePath + ".dds", (uint)Rect.Width, (uint)Rect.Height, Mips );
    }
    


    void SaveMip( string basePath, int mipLevel ) {
      string file = string.Format("{0}{1}.png",
        basePath, mipLevel > 0 ? "-" + mipLevel.ToString() : "");
      Mips[mipLevel].Save( file );
    }



    public string       File { get; set; }
    public List<Bitmap> Mips { get; set; }
    public Bitmap       Org  { get; set; }
    public int          ID   { get; set; }
    public Rectangle    Rect { get; set; }
  }
}
