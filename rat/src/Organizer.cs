/**
 * Organizer.cs
 * Layout packing logic for Packrat.
 * Copyright (c) 2015-16 | hacksalot <hacksalot@indevious.com>
 * License: MIT
 */

using System;
using System.Collections;
using System.Drawing;

namespace rat {
  


  /// <summary>
  /// Organize the available space using a "divide and conquer"
  /// bin-packing algorithm. Geometrically divide the space with
  /// each added element.
  /// </summary>
  public class Organizer {



    /// <summary>
    /// Construct an organizer for a given atlas size.
    /// </summary>
    public Organizer( Size dims ) {
      _dims = dims;
      _root = new Node<ImgAsset>() {
        rc = new Rectangle( 0, 0, dims.Width, dims.Height )
      };
    }



    /// <summary>
    /// Submit an array of images for packing.
    /// </summary>
    public void Pack( ImgAsset[] images ) {
      var ic = new ImageComparer();
      System.Array.Sort( images, ic );
      for( int r = 0; r < images.Length; r++ ) {
        if ( Rectangle.Empty == Pack( images[r] ) ) break;
      }
    }



    /// <summary>
    /// Pack a single image.
    /// </summary>
    public Rectangle Pack( ImgAsset img ) {
      Node<ImgAsset> n = _root.Insert( img, img.Rect );
      if (n != null)
        img.Rect = n.rc;
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
          return sub[1].Insert( thing, rcThing );
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

          if( dw > dh ) { // split horizontally
            sub[0].rc = new Rectangle( rc.Left, rc.Top, rcThing.Width, rc.Height );
            sub[1].rc = new Rectangle( rc.Left + rcThing.Width, rc.Top, rc.Width - rcThing.Width, rc.Height );
          }
          else {          // split vertically
            sub[0].rc = new Rectangle( rc.Left, rc.Top, rc.Width, rcThing.Height );
            sub[1].rc = new Rectangle( rc.Left, rc.Top + rcThing.Height, rc.Width, rc.Height - rcThing.Height );
          }

          return sub[0].Insert( thing, rcThing );
        }
      }
    }

    Size             _dims;
    Node<ImgAsset>   _root;
  }



  /// <summary>
  /// Compare two images based on size. TODO: Replace with lambda if
  /// supported by Mono.
  /// </summary>
  public class ImageComparer : IComparer {
    public int Compare( object imgA, object imgB ) {
      int aArea = ((ImgAsset)imgA).Rect.Width * ((ImgAsset)imgA).Rect.Height;
      int bArea = ((ImgAsset)imgB).Rect.Width * ((ImgAsset)imgB).Rect.Height;
      if( aArea < bArea ) return -1;
      else if( bArea < aArea ) return 1;
      else return 0;
    }
  }



}
