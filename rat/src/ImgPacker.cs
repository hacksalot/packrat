﻿/**
 * ImgPacker.cs
 * Definition of the ImgPacker class.
 * Copyright (c) 2015-16 | hacksalot <hacksalot@indevious.com>
 * License: MIT
 */

using System;
using System.Collections;
using System.Drawing;

namespace rat {
  


  /// <summary>
  /// A simple image/texture packer based on a common "divide and
  /// conquer" texture packing algorithm. Geometrically divide the
  /// space with each added texture.
  /// </summary>
  public class ImgPacker {



    /// <summary>
    /// Construct an image packer for a given atlas size.
    /// </summary>
    public ImgPacker( Size dims ) {
      _dims = dims;
      _root = new Node<ImgEx>() {
        rc = new Rectangle( 0, 0, dims.Width, dims.Height )
      };
    }



    /// <summary>
    /// Submit an array of images for packing.
    /// </summary>
    public void Pack( ImgEx[] images ) {
      var ic = new ImageComparer();
      System.Array.Sort( images, ic );
      for( int r = 0; r < images.Length; r++ ) {
        if ( Rectangle.Empty == Pack(images[r]) ) break;
      }
    }



    /// <summary>
    /// Pack a single image.
    /// </summary>
    public Rectangle Pack( ImgEx img ) {
      Node<ImgEx> n = _root.Insert( img, img.rc );
      return n == null ? Rectangle.Empty : n.rc;
    }


    
    /// <summary>
    /// A simple generic node class. Internal use only.
    /// http://www.blackpawn.com/texts/lightmaps/
    /// </summary>
    private class Node<T> {

      public Node<T>[] sub;
      public Rectangle rc;
      public T data = default(T);
      public bool IsLeaf { get { return sub == null; } }

      public Node<T> Insert( T thing, Rectangle rcThing ) {

        if( !IsLeaf ) {
          Node<T> newNode = sub[0].Insert( thing, rcThing );
          if( newNode != null ) return newNode;
          return sub[1].Insert(thing, rcThing);
        }
        else { 
          if ( data != null ) return null;
          if ( rc.Width < rcThing.Width || rc.Height < rcThing.Height )
            return null;
          if ( rc.Width == rcThing.Width && rc.Height == rcThing.Height ) {
            data = thing;
            return this;
          }

          sub = new Node<T>[2] { new Node<T>(), new Node<T>() };

          float dw = rc.Width - rcThing.Width;
          float dh = rc.Height - rcThing.Height;
          if( dw > dh ) {
            sub[0].rc = new Rectangle( rc.Left, rc.Top, rcThing.Width, rc.Height );
            sub[1].rc = new Rectangle( rc.Left + rcThing.Width, rc.Top, rc.Width - rcThing.Width, rc.Height );
          }
          else {
            sub[0].rc = new Rectangle( rc.Left, rc.Top, rc.Width, rcThing.Height );
            sub[1].rc = new Rectangle( rc.Left, rc.Top + rcThing.Height, rc.Width, rc.Height - rcThing.Height );
          }

          return sub[0].Insert( thing, rcThing );
        }
      }
    }

    Size          _dims;
    Node<ImgEx>   _root;
  }



  /// <summary>
  /// Compare two images based on size. TODO: Replace with lambda if
  /// supported by Mono.
  /// </summary>
  public class ImageComparer : IComparer {
    public int Compare( object imgA, object imgB ) {
      int aArea = ((ImgEx)imgA).rc.Width * ((ImgEx)imgA).rc.Height;
      int bArea = ((ImgEx)imgB).rc.Width * ((ImgEx)imgB).rc.Height;
      if( aArea < bArea ) return -1;
      else if( bArea < aArea ) return 1;
      else return 0;
    }
  }



}
