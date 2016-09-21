/**
 * glob.cs
 * Simple glob support for Mono / .NET.
 * Copyright (c) 2015-16 | hacksalot <hacksalot@indevious.com>
 * License: MIT
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace rat
{
  public class Glob : IEnumerable< string >
  {


    public Glob( string path ) {
      _paths = exp( path ).ToList();
    }


    public string this[ int index ] {
      get { return _paths[index]; }
      set { _paths.Insert(index, value); }
    }


    public IEnumerator<string> GetEnumerator() {
      return _paths.GetEnumerator();
    }


    IEnumerator IEnumerable.GetEnumerator() {
      return this.GetEnumerator();
    }


    IEnumerable<string> exp( string glob ) {
      foreach( string path in exp( head( glob ) + _sep, tail( glob )))
        yield return path;
    }


    IEnumerable<string> exp( string h, string t ) {
      if ( tail( t ) == t )
        foreach( string path in Directory.GetFiles( h, t ).OrderBy( s => s ) )
          yield return path;
      else
        foreach ( string dir in Directory.GetDirectories(h, head( t )).OrderBy( s => s ))
          foreach ( string path in exp( Path.Combine(h, dir), tail( t )) )
            yield return path;
    }


    string head( string path ) {
      if( path.StartsWith( "" + _sep + _sep )) // [1]
        return path.Substring( 0, 2 ) + path.Substring( 2 ).Split( _sep )[0]
        + _sep + path.Substring( 2 ).Split( _sep )[1];
      return path.Split( _sep )[0];
    }


    string tail( string path ) {
      return path.Contains( _sep ) ?
        path.Substring( 1 + head( path ).Length ) : path;
    }


    char _sep = Path.DirectorySeparatorChar;
    List<string> _paths;
  }
}

// [1] Handle \\share\vol\foo\bar. TODO check behavior on Linux.
