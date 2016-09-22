/**
 * Writer.cs
 * A utility class for writing .DDS (Direct Draw Surface) files with mipmaps.
 * Copyright (c) 2015-16 | hacksalot <hacksalot@indevious.com>
 * License: MIT
 */

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Drawing;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;

namespace rat
{
  

  /// <summary>A simple .DDS file writer with mipmap support.</summary>
  /// <remarks>Creates a valid .DDS file containing a specified number
  /// of mipmaps in an uncompressed ARGB format. This DDS file can be
  /// converted to DXT1, DXT3, etc., using a third-party tool like
  /// Compressonator or Unity.</remarks>
  class Writer
  {


    /// <summary>Write a DDS file to the specified path.</summary>
    /// <param name="path">Path to the .DDS file to be created.</param>
    /// <param name="w">Image width (of the biggest mipmap).</param>
    /// <param name="h">Image height (of the biggest mipmap).</param>
    /// <param name="images">A collection containing the original image
    /// and its mipmaps, ordered from largest to smallest.</param>
    public void WriteDDS( string path, uint w, uint h, IEnumerable<Bitmap> images )
    {
      DDSHeader hdr = new DDSHeader() {
        Width = w, Height = h,
        MIPCount = (uint)images.Count(),
        PitchOrLinearSize = ( w * 32 + 7 ) / 8 //{1}
      };
      FileStream fs = new FileStream( path, FileMode.Create );
      BinaryWriter bw = new BinaryWriter( fs );
      byte[] temp = hdr.Serialize();
      bw.Write( temp );
      foreach( Bitmap bmp in images ) {
        bw.Write( GetImgData( bmp ) );
      }
      bw.Close();
      fs.Close();
    }



    /// <summary>Return a buffer containing the image's raw data.</summary>
    public byte[] GetImgData( System.Drawing.Bitmap img )
    {
      BitmapData bd = img.LockBits(
        new Rectangle(0, 0, img.Width, img.Height),
        ImageLockMode.ReadOnly, img.PixelFormat );
      int bytes = Math.Abs( bd.Stride ) * img.Height;
      byte[] rgbValues = new byte[ bytes ];
      Marshal.Copy(bd.Scan0, rgbValues, 0, bytes);
      img.UnlockBits(bd);
      return rgbValues;

      //Byte[] data = null;
      //byte[] buf = new byte[4];
      //using( MemoryStream ms = new MemoryStream() ) {
      //  img.Save( ms, ImageFormat.Bmp );//img.RawFormat );
      //  ms.Position = 10;
      //  ms.Read( buf, 0, 4 );
      //  if( !BitConverter.IsLittleEndian )
      //    Array.Reverse( buf );
      //  uint i = BitConverter.ToUInt32( buf, 0 );
      //  data = new byte[ ms.Length - i ];
      //  ms.Position = i;
      //  ms.Read( data, 0, (int) (ms.Length - i) );
      //  //data = ms.ToArray();
      //}
      //return data;
    }



    /// <summary>
    /// A representation of a .DDS file header or, equivalently, the
    /// DDS_HEADER structure from the Windows API. Originally, we
    /// used [StructLayout] and [Serializable] to serialize instances
    /// of this class to the destination .DDS file, but you end up
    /// having to write a custom serializer anyway, thanks to markers
    /// and other metadata necessary for normal serialization, where what
    /// we *really* want is regular old C-style "copy the raw bytes of a
    /// structure to this buffer in its entirety" semantics -- which are
    /// difficult, by design, to achieve here in managed C# land. So
    /// now this structure is just a dummy container for DDS header
    /// data, and we write it with a regular binary writer. Much ado
    /// about nothing.
    /// </summary>
    [StructLayout( LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1 )]
    [Serializable]
    class DDSHeader
    {
      // DDS Header data
      public const uint Magic = (uint)0x20534444;// 'DDS '
      public const uint Size  = 124U;            // Header size
      public uint       Flags = DDSD_CAPS | DDSD_HEIGHT | DDSD_WIDTH | // Flags
                              DDSD_PIXELFORMAT | DDSD_PITCH | DDSD_MIPMAPCOUNT;
      public uint       Height= 0U;              // Height
      public uint       Width = 0U;              // Width
      public uint       PitchOrLinearSize = 0U;  // Pitch or linear size
      public uint       Depth = 0U;              // Texture depth
      public uint       MIPCount = 1U;           // Mipmap count
      public readonly uint[] Reserved = { 0,0,0,0,0,0,0,0,0,0,0 }; // Reserved

      // DDS Pixel Format data
      public const uint PFSize = (uint)32;
      public uint       PFFlags = DDPF_RGB | DDPF_ALPHAPIXELS;
      public uint       FourCC = 0;
      public uint       RBGBitCount = 32;
      public uint       RMask = 0x00FF0000U;
      public uint       GMask = 0x0000FF00U;
      public uint       BMask = 0x000000FFU;
      public uint       AMask = 0xFF000000U;
      public uint       Caps = DDSCAPS_TEXTURE|DDSCAPS_MIPMAP|DDSCAPS_COMPLEX;
      public uint       Caps2 = 0U;
      public uint       Caps3 = 0U;
      public uint       Caps4 = 0U;
      public const uint Reserved2 = 0U;

      /// <summary>Serialize the DDS header to a memory stream.</summary>
      // http://stackoverflow.com/q/1446547
      public byte[] Serialize()
      {
        using( MemoryStream m = new MemoryStream() ) {
          using( BinaryWriter writer = new BinaryWriter(m) ) {
            writer.Write( Magic );
            writer.Write( Size );
            writer.Write( Flags );
            writer.Write( Height );
            writer.Write( Width );
            writer.Write( PitchOrLinearSize );
            writer.Write( Depth );
            writer.Write( MIPCount );
            for( int r = 0; r < 11; r++ ) { writer.Write(0U); }
            writer.Write( PFSize );
            writer.Write( PFFlags );
            writer.Write( FourCC );
            writer.Write( RBGBitCount );
            writer.Write( RMask );
            writer.Write( GMask );
            writer.Write( BMask );
            writer.Write( AMask );
            writer.Write( Caps );
            writer.Write( Caps2 );
            writer.Write( Caps3 );
            writer.Write( Caps4 );
            writer.Write( Reserved2 );
            writer.Flush();
          }
          return m.ToArray();
        }
      }
    }


    // DDS header and pixel format flags inferred from the native APIs.
    const uint DDSD_CAPS        = 0x1;
    const uint DDSD_HEIGHT      = 0x2;
    const uint DDSD_WIDTH       = 0x4;
    const uint DDSD_PITCH       = 0x8;
    const uint DDSD_PIXELFORMAT = 0x1000;
    const uint DDSD_MIPMAPCOUNT = 0x20000;
    const uint DDSD_LINEARSIZE  = 0x80000;
    const uint DDSD_DEPTH       = 0x800000;
    const uint DDSCAPS_COMPLEX  = 0x8;
    const uint DDSCAPS_MIPMAP   = 0x400000;
    const uint DDSCAPS_TEXTURE  = 0x1000;
    const uint DDPF_ALPHAPIXELS = 0x1;
    const uint DDPF_ALPHA       = 0x2;
    const uint DDPF_FOURCC      = 0x4;
    const uint DDPF_RGB         = 0x40;
    const uint DDPF_YUV         = 0x200;
    const uint DDPF_LUMINANCE   = 0x20000;
  }
}

// {1} https://msdn.microsoft.com/en-us/library/windows/desktop/bb943991(v=vs.85).aspx
