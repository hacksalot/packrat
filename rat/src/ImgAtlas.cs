/**
 * ImgAtlas.cs
 * Atlas containment and zoning logic for Packrat.
 * Copyright (c) 2015-16 | hacksalot <hacksalot@indevious.com>
 * License: MIT
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;

namespace rat
{
  class ImgAtlas : ImgAsset
  {
    public ImgAtlas( string path, Bitmap o, int idx ) 
      : base( path, o, idx )
    {
    }

    public void Add( ImgAsset asset ) {
      Children.Add( asset );
    }

    public void Save( string basePath ) {
      for( int mip = 0; mip < Mips.Count; mip++ ) {
        SaveMip( basePath, mip );
      }
      DDSFileWriter dw = new DDSFileWriter();
      dw.Write( basePath + ".dds", (uint)Rect.Width, (uint)Rect.Height, Mips );
      TAIFileWriter tw = new TAIFileWriter();
      tw.Write( basePath + ".tai", this, Children );
    }

    List<ImgAsset> Children = new List<ImgAsset>();
  }
}
