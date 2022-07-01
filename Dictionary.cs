//===============
// Dictionary.cs
//===============

// Windows.NET-implementation of a human-readable Dictionary

// Copyright 2022, Sven Bieg (svenbieg@web.de)
// http://github.com/svenbieg/Clusters.NET


//===========
// Namespace
//===========

namespace Clusters {


//======
// Item
//======

public class DictionaryItem<T>: IndexItem<string, T>
{
// Con-/Destructors
public DictionaryItem(string Key): base(Key) {}
public DictionaryItem(string Key, T? Value): base(Key, Value) {}

// Access
public override int CompareTo(IndexItem<string, T>? Item)
	{
	if(Item==null)
		{
		if(Key==null)
			return 0;
		return 1;
		}
	return StringHelper.StringCompare(Key, Item.Key);
	}
}


//============
// Dictionary
//============

public class Dictionary<T>: Index<DictionaryItem<T>>
{
// Con-/Destructors
public Dictionary() {}
public Dictionary(Dictionary<T> Dictionary): base(Dictionary) {}

// Modification
public bool Add(string Key)
	{
	return Add(new DictionaryItem<T>(Key));
	}
public bool Add(string Key, T Value)
	{
	return Add(new DictionaryItem<T>(Key, Value));
	}
}

}
