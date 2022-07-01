//===============
// Dictionary.cs
//===============

// Windows.NET-implementation of a numeric sorted Dictionary

// Copyright 2022, Sven Bieg (svenbieg@web.de)
// http://github.com/svenbieg/Clusters.NET


//===========
// Namespace
//===========

namespace Clusters {


//============
// Dictionary
//============

public class Dictionary<T>: Map<string, T>
{
// Con-/Destructors
public Dictionary() {}
public Dictionary(Dictionary<T> Dictionary): base(Dictionary) {}
}

}
