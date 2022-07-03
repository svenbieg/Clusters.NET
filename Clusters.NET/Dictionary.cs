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


//======
// Item
//======

public class DictionaryItem<T>: MapItem<string, T>
{
// Con-/Destructors
public DictionaryItem() {}

// Common
public override int CompareTo(string? Key)
	{
	return StringHelper.StringCompare(_Key, Key);
	}
}


//============
// Dictionary
//============

public class Dictionary<T>: Map<string, T, DictionaryItem<T>>
{
public Dictionary(): base() {}
public Dictionary(Dictionary<T> Dictionary): base(Dictionary) {}
}

}
