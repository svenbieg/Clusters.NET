//========
// Map.cs
//========

// .NET-Implementation of a sorted map.
// Items and can be inserted, removed and looked-up in constant low time.

// Copyright 2024, Sven Bieg (svenbieg@web.de)
// http://github.com/svenbieg/Clusters.NET


//===========
// Namespace
//===========

namespace Clusters;


//======
// Item
//======

public class MapItem<TKey, TValue>: IComparable<MapItem<TKey, TValue>> where TKey: IComparable<TKey>
	{
	#region Con-/Destructors
	internal MapItem(IComparer<MapItem<TKey, TValue>> comparer, TKey key, TValue value=default)
		{
		_Key=key;
		_Value=value;
		Hash=comparer.GetHashCode(this);
		}
	#endregion

	#region Common
	internal uint Hash;
	public TKey Key { get { return _Key; } }
	internal TKey _Key;
	public TValue Value
		{
		get { return _Value; }
		set { _Value=value; }
		}
	private TValue _Value;
	#endregion

	#region IComparable
	public int CompareTo(MapItem<TKey, TValue>? item)
		{
		throw new NotImplementedException();
		}
	#endregion
	}


//=====
// Map
//=====

public class Map<TKey, TValue>: Index<MapItem<TKey, TValue>>
	where TKey: IComparable<TKey>
	{
	#region Con-/Destructors
	public Map(): base(new MapComparer<TKey, TValue>()) {}
	public Map(Map<TKey, TValue> copy): base(copy, new MapComparer<TKey, TValue>()) {}
	#endregion

	#region Common
	public TValue this[TKey key]
		{
		get { return Get(key); }
		set { Set(key, value); }
		}
	#endregion

	#region Access
	public bool Contains(TKey key)
		{
		var item=new MapItem<TKey, TValue>(Comparer, key);
		return Contains(item);
		}
	public TValue Get(TKey key)
		{
		var item=new MapItem<TKey, TValue>(Comparer, key);
		lock(Mutex)
			{
			if(Root==null||!Root.TryGet(item, ref item, Comparer))
				throw new KeyNotFoundException();
			return item.Value;
			}
		}
	public bool TryGet(TKey key, ref TValue value)
		{
		lock(Mutex)
			{
			if(Root==null)
				return false;
			var item=new MapItem<TKey, TValue>(Comparer, key);
			if(!Root.TryGet(item, ref item, Comparer))
				return false;
			value=item.Value;
			return true;
			}
		}
	#endregion

	#region Modification
	public bool Add(TKey key, TValue value)
		{
		var item=new MapItem<TKey, TValue>(Comparer, key, value);
		return Add(item);
		}
	public bool Remove(TKey key)
		{
		var item=new MapItem<TKey, TValue>(Comparer, key);
		return Remove(item);
		}
	public void Set(TKey key, TValue value)
		{
		var item=new MapItem<TKey, TValue>(Comparer, key, value);
		Set(item);
		}
	#endregion

	#region Enumeration
	public IndexEnumerator<MapItem<TKey, TValue>> Find(TKey key, FindFunc func)
		{
		var item=new MapItem<TKey, TValue>(Comparer, key);
		return Find(item, func);
		}
	#endregion
	}


//==========
// Comparer
//==========

public class MapComparer<TKey, TValue>: IComparer<MapItem<TKey, TValue>>
	where TKey: IComparable<TKey>
	{
	#region Con-/Destructors
	internal MapComparer()
		{
		Comparer=Comparer<TKey>.Default;
		}
	#endregion

	#region Common
	internal IComparer<TKey> Comparer;
	#endregion

	#region IComparer
	public int Compare(MapItem<TKey, TValue> item1, MapItem<TKey, TValue> item2)
		{
		if(item1.Hash<item2.Hash)
			return -1;
		if(item1.Hash>item2.Hash)
			return 1;
		return Comparer.Compare(item1._Key, item2._Key);
		}
	public uint GetHashCode(MapItem<TKey, TValue> item)
		{
		return Comparer.GetHashCode(item._Key);
		}
	#endregion
	}
