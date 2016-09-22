/**
 * TAIFileWriter.cs
 * TAI (Texture Access Index) file writer for Packrat.
 * Copyright (c) 2015-16 | hacksalot <hacksalot@indevious.com>
 * License: MIT
 */

using System;
using System.Collections.Generic;
using System.IO;

namespace rat
{
  class TAIFileWriter
  {
    public void Write( string path, ImgAtlas atlas, IEnumerable<ImgAsset> assets )
    {
      int atlasIndex = 0;
      using( StreamWriter str = new StreamWriter(path) ) {
        str.WriteLine( rat.con.Resources.mTAIComment, atlas.File, Environment.CommandLine );
        foreach( ImgAsset img in assets ) {
          str.WriteLine( "{0}\t\t{1}, {2}, 2D, {3:F6}, {4:F6}, {5:F6}, {6:F6}, {7:F6}",
            img.Moniker, atlas.File, atlasIndex,
            (double)img.Rect.Left / atlas.Rect.Width,
            (double)img.Rect.Top / atlas.Rect.Height,
            0, // depth offset; always 0 in our case
            (double)img.Rect.Width / atlas.Rect.Width,
            (double)img.Rect.Height / atlas.Rect.Height);
        }
        str.WriteLine( "" );
      }
    }
  }
}
