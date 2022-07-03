//========
// Map.cs
//========

// Windows.NET-Implementation of a sorted linked-list
// Items and can be inserted, removed and looked-up in constant low time.

// Copyright 2022, Sven Bieg (svenbieg@web.de)
// http://github.com/svenbieg/Clusters.NET


//===========
// Namespace
//===========

namespace Clusters {


//======
// Item
//======

public class MapItem<TKey, TValue>:
	IComparable<TKey>, IComparable<MapItem<TKey, TValue>>
	where TKey: IComparable<TKey>
{
// Con-/Destructors
public MapItem() {}

// Common
public virtual int CompareTo(TKey? Key)
	{
	return _Key.CompareTo(Key);
	}
public virtual int CompareTo(MapItem<TKey, TValue>? Item)
	{
	return _Key.CompareTo(Item._Key);
	}
public TKey Key { get { return _Key; }}
internal TKey? _Key;
public TValue? Value
	{
	get { return _Value; }
	set { _Value = value; }
	}
internal TValue? _Value;
};


//=====
// Map
//=====

public class Map<TKey, TValue, TItem>: Index<TKey, TItem>
	where TKey: IComparable<TKey>
	where TItem: MapItem<TKey, TValue>, new()
{
// Con-/Destructors
public Map(): base() {}
public Map(Map<TKey, TValue, TItem> Map): base(Map) {}

// Access
public TValue? this[TKey Key]
	{
	get => Get(Key);
	set => Set(Key, value);
	}
public new TValue? Get(TKey Key)
	{
	var item=base.Get(Key);
	if(item==null)
		return default;
	return item._Value;
	}

// Modification
public bool Add(TKey Key, TValue? Value)
	{
	var item=new TItem();
	item._Key=Key;
	item._Value=Value;
	return Add(Key, item);
	}
public void Set(TKey Key, TValue? Value)
	{
	var item=new TItem();
	item._Key=Key;
	item._Value=Value;
	Set(Key, item);
	}
};

public class Map<TKey, TValue>: Map<TKey, TValue, MapItem<TKey, TValue>>
	where TKey : IComparable<TKey>
{
public Map(): base() {}
public Map(Map<TKey, TValue> Map): base(Map) {}
}

} // namespace
