/**
 * ImgPacker.cs
 * Image packing logic for packrat. Pack, noble packing logic. Pack like the wind.
 * Copyright (c) 2015-16 | hacksalot <hacksalot@indevious.com>
 * License: MIT
 */

using System;
using System.Collections.Generic;
using System.Drawing;

namespace rat {


  // Internal helper class for images / textures
  public class ImgEx {


    public ImgEx( string path, Bitmap o, int idx ) {
      file = path;
      org = o;
      id = idx;
      rc = new Rectangle( Point.Empty, o.Size );
      mips = new List<Bitmap>();
      mips.Add(o);
    }


    public void save( string basePath ) {
      for( int mip = 0; mip < mips.Count; mip++ ) {
        saveMips( basePath, mip );
      }
      DDSWriter dw = new DDSWriter();
      dw.Write( basePath + ".dds", (uint)rc.Width, (uint)rc.Height, mips );
    }


    void saveMips( string basePath, int mipLevel ) {
      string file = string.Format("{0}{1}.png",
        basePath, mipLevel > 0 ? "-" + mipLevel.ToString() : "");
      mips[mipLevel].Save( file );
    }


    public string       file { get; set; }
    public List<Bitmap> mips { get; set; }
    public Bitmap       org { get; set; }
    public int          id { get; set; }
    public Rectangle    rc { get; set; }
  }
}
