//========
// Map.cs
//========

// .NET-Implementation of a sorted map.
// Items and can be inserted, removed and looked-up in constant low time.

// Copyright 2024, Sven Bieg (svenbieg@web.de)
// http://github.com/svenbieg/Clusters.NET

using System.Collections;


//===========
// Namespace
//===========

namespace Clusters;


//======
// Item
//======

public class MapItem<TKey, TValue> where TKey: IComparable<TKey>
	{
	// Con-/Destructors
	internal MapItem(IComparer<TKey> comparer, TKey key, TValue value=default)
		{
		Hash=comparer.GetHashCode(key);
		_Key=key;
		_Value=value;
		}

	// Common
	internal int Hash;
	public TKey Key { get { return _Key; } }
	internal TKey _Key;
	public TValue Value
		{
		get { return _Value; }
		set { _Value=value; }
		}
	private TValue _Value;
	}


//=======
// Group
//=======

internal interface IMapGroup<TKey, TValue>: IClusterGroup<MapItem<TKey, TValue>>
	where TKey: IComparable<TKey>
	{
	// Common
	MapItem<TKey, TValue> First { get; }
	MapItem<TKey, TValue> Last { get; }

	// Access
	bool Find(MapItem<TKey, TValue> item, FindFunc func, ref ushort pos, ref bool exists, IComparer<TKey> comparer);
	bool TryGet(MapItem<TKey, TValue> item, ref MapItem<TKey, TValue> found, IComparer<TKey> comparer);

	// Modification
	bool Add(MapItem<TKey, TValue> item, bool again, ref bool exists, IComparer<TKey> comparer);
	bool Remove(MapItem<TKey, TValue> item, ref MapItem<TKey, TValue> removed, IComparer<TKey> comparer);
	bool Set(MapItem<TKey, TValue> item, bool again, ref bool exists, IComparer<TKey> comparer);
	}


//============
// Item-Group
//============

internal class MapItemGroup<TKey, TValue>: ClusterItemGroup<MapItem<TKey, TValue>>, IMapGroup<TKey, TValue>
	where TKey: IComparable<TKey>
	{
	// Con-/Destructors
	internal MapItemGroup() {}
	internal MapItemGroup(MapItemGroup<TKey, TValue> copy): base(copy) {}

	// Common
	private int Compare(MapItem<TKey, TValue> item1, MapItem<TKey, TValue> item2, IComparer<TKey> comparer)
		{
		if(item1.Hash<item2.Hash)
			return -1;
		if(item1.Hash>item2.Hash)
			return 1;
		return comparer.Compare(item1.Key, item2.Key);
		}
	public MapItem<TKey, TValue> First { get { return Items[0]; } }
	public virtual MapItem<TKey, TValue> Last { get { return Items[_ItemCount-1]; } }

	// Access
	public bool Find(MapItem<TKey, TValue> item, FindFunc func, ref ushort pos, ref bool exists, IComparer<TKey> comparer)
		{
		pos=GetItemPos(item, ref exists, comparer);
		if(exists)
			{
			switch(func)
				{
				case FindFunc.Above:
					{
					if(pos+1>=_ItemCount)
						return false;
					pos++;
					break;
					}
				case FindFunc.Below:
					{
					if(pos==0)
						return false;
					pos--;
					break;
					}
				default:
					{
					break;
					}
				}
			return true;
			}
		switch(func)
			{
			case FindFunc.Above:
			case FindFunc.AboveOrEqual:
				{
				if(pos==_ItemCount)
					return false;
				break;
				}
			case FindFunc.Any:
				{
				if(pos>0)
					pos--;
				break;
				}
			case FindFunc.Below:
			case FindFunc.BelowOrEqual:
				{
				if(pos==0)
					return false;
				pos--;
				break;
				}
			case FindFunc.Equal:
				{
				return false;
				}
			}
		return true;
		}
	private ushort GetItemPos(MapItem<TKey, TValue> item, ref bool exists, IComparer<TKey> comparer)
		{
		if(_ItemCount==0)
			return 0;
		ushort start=0;
		ushort end=_ItemCount;
		while(start<end)
			{
			ushort pos=(ushort)(start+(end-start)/2);
			if(Compare(Items[pos], item, comparer)>0)
				{
				end=pos;
				continue;
				}
			if(Compare(Items[pos], item, comparer)<0)
				{
				start=(ushort)(pos+1);
				continue;
				}
			exists=true;
			return pos;
			}
		return start;
		}
	public bool TryGet(MapItem<TKey, TValue> item, ref MapItem<TKey, TValue> found, IComparer<TKey> comparer)
		{
		bool exists=false;
		var pos=GetItemPos(item, ref exists, comparer);
		if(exists)
			found=Items[pos];
		return exists;
		}

	// Modification
	public bool Add(MapItem<TKey, TValue> item, bool again, ref bool exists, IComparer<TKey> comparer)
		{
		var pos=GetItemPos(item, ref exists, comparer);
		if(exists)
			return false;
		return InsertItem(pos, item);
		}
	public bool Remove(MapItem<TKey, TValue> item, ref MapItem<TKey, TValue> removed, IComparer<TKey> comparer)
		{
		bool exists=false;
		var pos=GetItemPos(item, ref exists, comparer);
		if(!exists)
			return false;
		removed=RemoveAt(pos);
		return true;
		}
	public bool Set(MapItem<TKey, TValue> item, bool again, ref bool exists, IComparer<TKey> comparer)
		{
		var pos=GetItemPos(item, ref exists, comparer);
		if(exists)
			{
			Items[pos]=item;
			return true;
			}
		return InsertItem(pos, item);
		}
	}


//==============
// Parent-Group
//==============

internal class MapParentGroup<TKey, TValue>: ClusterParentGroup<MapItem<TKey, TValue>>, IMapGroup<TKey, TValue>
	where TKey: IComparable<TKey>
	{
	// Con-Destructors
	public MapParentGroup(ushort level): base(level) {}
	public MapParentGroup(IMapGroup<TKey, TValue> child): base(child)
		{
		_First=child.First;
		_Last=child.Last;
		}
	public MapParentGroup(MapParentGroup<TKey, TValue> copy): base(copy)
		{
		UpdateBounds();
		}

	// Common
	private int Compare(MapItem<TKey, TValue> item1, MapItem<TKey, TValue> item2, IComparer<TKey> comparer)
		{
		if(item1.Hash<item2.Hash)
			return -1;
		if(item1.Hash>item2.Hash)
			return 1;
		return comparer.Compare(item1.Key, item2.Key);
		}
	public MapItem<TKey, TValue> First { get { return _First; } }
	private MapItem<TKey, TValue> _First;
	public MapItem<TKey, TValue> Last { get { return _Last; } }
	private MapItem<TKey, TValue> _Last;

	// Access
	public bool Find(MapItem<TKey, TValue> item, FindFunc func, ref ushort pos, ref bool exists, IComparer<TKey> comparer)
		{
		ushort count=GetItemPos(item, ref pos, false, comparer);
		if(count==0)
			return false;
		if(count==1)
			{
			switch(func)
				{
				case FindFunc.Above:
					{
					var child=Children[pos] as IMapGroup<TKey, TValue>;
					if(Compare(child.Last, item, comparer)==0)
						{
						if(pos+1>=_ChildCount)
							return false;
						pos++;
						}
					break;
					}
				case FindFunc.Below:
					{
					var child=Children[pos] as IMapGroup<TKey, TValue>;
					if(Compare(child.First, item, comparer)==0)
						{
						if(pos==0)
							return false;
						pos--;
						}
					break;
					}
				default:
					break;
				}
			}
		else if(count==2)
			{
			switch(func)
				{
				case FindFunc.Above:
				case FindFunc.AboveOrEqual:
					{
					pos++;
					break;
					}
				case FindFunc.Equal:
					return false;
				default:
					break;
				}
			}
		return true;
		}
	private ushort GetItemPos(MapItem<TKey, TValue> item, ref ushort group, bool must_exist, IComparer<TKey> comparer)
		{
		ushort start=0;
		ushort end=_ChildCount;
		while(start<end)
			{
			ushort pos=(ushort)(start+(end-start)/2);
			var child=Children[pos] as IMapGroup<TKey, TValue>;
			if(Compare(child.First, item, comparer)>0)
				{
				end=pos;
				continue;
				}
			if(Compare(child.Last, item, comparer)<0)
				{
				start=(ushort)(pos+1);
				continue;
				}
			group=pos;
			return 1;
			}
		if(must_exist)
			return 0;
		if(start>=_ChildCount)
			start=(ushort)(_ChildCount-1);
		group=start;
		if(start>0)
			{
			var child=Children[start] as IMapGroup<TKey, TValue>;
			if(Compare(child.First, item, comparer)>0)
				{
				group=(ushort)(start-1);
				return 2;
				}
			}
		if(start+1<_ChildCount)
			{
			var child=Children[start] as IMapGroup<TKey, TValue>;
			if(Compare(child.Last, item, comparer)<0)
				return 2;
			}
		return 1;
		}
	public bool TryGet(MapItem<TKey, TValue> item, ref MapItem<TKey, TValue> found, IComparer<TKey> comparer)
		{
		ushort pos=0;
		ushort count=GetItemPos(item, ref pos, true, comparer);
		if(count!=1)
			return false;
		var child=Children[pos] as IMapGroup<TKey, TValue>;
		return child.TryGet(item, ref found, comparer);
		}

	// Modification
	public virtual bool Add(MapItem<TKey, TValue> item, bool again, ref bool exists, IComparer<TKey> comparer)
		{
		if(AddInternal(item, again, ref exists, comparer))
			{
			_ItemCount++;
			UpdateBounds();
			return true;
			}
		return false;
		}
	private bool AddInternal(MapItem<TKey, TValue> item, bool again, ref bool exists, IComparer<TKey> comparer)
		{
		ushort group=0;
		ushort count=GetItemPos(item, ref group, false, comparer);
		if(!again)
			{
			for(ushort u=0; u<count; u++)
				{
				var child=Children[group+u] as IMapGroup<TKey, TValue>;
				if(child.Add(item, false, ref exists, comparer))
					return true;
				if(exists)
					return false;
				}
			if(ShiftChildren(group, count))
				{
				count=GetItemPos(item, ref group, false, comparer);
				for(ushort u=0; u<count; u++)
					{
					var child=Children[group+u] as IMapGroup<TKey, TValue>;
					if(child.Add(item, false, ref exists, comparer))
						return true;
					}
				}
			}
		if(!SplitChild(group))
			return false;
		count=GetItemPos(item, ref group, false, comparer);
		for(ushort u=0; u<count; u++)
			{
			var child=Children[group+u] as IMapGroup<TKey, TValue>;
			if(child.Add(item, true, ref exists, comparer))
				return true;
			}
		return false;
		}
	protected override void AppendGroups(IClusterGroup<MapItem<TKey, TValue>>[] groups, int pos, ushort count)
		{
		base.AppendGroups(groups, pos, count);
		UpdateBounds();
		}
	protected override void InsertGroups(int at, IClusterGroup<MapItem<TKey, TValue>>[] groups, int pos, ushort count)
		{
		base.InsertGroups(at, groups, pos, count);
		UpdateBounds();
		}
	public bool Remove(MapItem<TKey, TValue> item, ref MapItem<TKey, TValue> removed, IComparer<TKey> comparer)
		{
		ushort pos=0;
		if(GetItemPos(item, ref pos, true, comparer)==0)
			return false;
		var child=Children[pos] as IMapGroup<TKey, TValue>;
		if(!child.Remove(item, ref removed, comparer))
			return false;
		_ItemCount--;
		CombineChildren(pos);
		UpdateBounds();
		return true;
		}
	public override MapItem<TKey, TValue> RemoveAt(uint pos)
		{
		MapItem<TKey, TValue> item=base.RemoveAt(pos);
		UpdateBounds();
		return item;
		}
	protected override void RemoveGroups(int pos, ushort count)
		{
		base.RemoveGroups(pos, count);
		UpdateBounds();
		}
	public bool Set(MapItem<TKey, TValue> item, bool again, ref bool exists, IComparer<TKey> comparer)
		{
		if(SetInternal(item, again, ref exists, comparer))
			{
			if(!exists)
				{
				_ItemCount++;
				UpdateBounds();
				}
			return true;
			}
		return false;
		}
	private bool SetInternal(MapItem<TKey, TValue> item, bool again, ref bool exists, IComparer<TKey> comparer)
		{
		ushort pos=0;
		var count=GetItemPos(item, ref pos, true, comparer);
		if(count>0)
			{
			var child=Children[pos] as IMapGroup<TKey, TValue>;
			if(child.Set(item, again, ref exists, comparer))
				return true;
			}
		return AddInternal(item, again, ref exists, comparer);
		}
	private bool SplitChild(int pos)
		{
		if(_ChildCount==GroupSize)
			return false;
		IMapGroup<TKey, TValue> group;
		if(_Level>1)
			{
			group=new MapParentGroup<TKey, TValue>((ushort)(_Level-1));
			}
		else
			{
			group=new MapItemGroup<TKey, TValue>();
			}
		InsertGroup(pos+1, group);
		MoveChildren(pos, pos+1, 1);
		return true;
		}
	private void UpdateBounds()
		{
		if(_ChildCount>0)
			{
			var first_child=Children[0] as IMapGroup<TKey, TValue>;
			var last_child=Children[_ChildCount-1] as IMapGroup<TKey, TValue>;
			_First=first_child.First;
			_Last=last_child.Last;
			}
		}
	}


//=====
// Map
//=====

public class Map<TKey, TValue>: Cluster<MapItem<TKey, TValue>>, IEnumerable<MapItem<TKey, TValue>>
	where TKey: IComparable<TKey>
	{
	// Con-/Destructors
	public Map(IComparer<TKey> comparer=null)
		{
		if(comparer==null)
			comparer=Comparer<TKey>.Default;
		Comparer=comparer;
		}
	public Map(Map<TKey, TValue> copy, IComparer<TKey> comparer=null): this(comparer)
		{
		CopyFrom(copy);
		}

	// Common
	public TValue this[TKey key]
		{
		get { return Get(key); }
		set { Set(key, value); }
		}
	internal IComparer<TKey> Comparer;
	public MapItem<TKey, TValue> First
		{
		get
			{
			lock(Mutex)
				{
				if(Root==null)
					return default;
				return Root.First;
				}
			}
		}
	public MapItem<TKey, TValue> Last
		{
		get
			{
			lock(Mutex)
				{
				if(Root==null)
					return default;
				return Root.Last;
				}
			}
		}
	internal new IMapGroup<TKey, TValue> Root
		{
		get { return base.Root as IMapGroup<TKey, TValue>; }
		set { base.Root=value; }
		}

	// Access
	public bool Contains(TKey key)
		{
		lock(Mutex)
			{
			if(Root==null)
				return false;
			var item=new MapItem<TKey, TValue>(Comparer, key);
			return Root.TryGet(item, ref item, Comparer);
			}
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

	// Modification
	public bool Add(TKey key, TValue value)
		{
		var item=new MapItem<TKey, TValue>(Comparer, key, value);
		lock(Mutex)
			{
			if(Root==null)
				Root=new MapItemGroup<TKey, TValue>();
			bool exists=false;
			if(Root.Add(item, false, ref exists, Comparer))
				return true;
			if(exists)
				return false;
			Root=new MapParentGroup<TKey, TValue>(Root);
			return Root.Add(item, true, ref exists, Comparer);
			}
		}
	public void CopyFrom(Map<TKey, TValue> copy)
		{
		lock(Mutex)
			{
			Root=null;
			var root=copy.Root;
			if(root==null)
				return;
			if(root.Level>0)
				{
				Root=new MapParentGroup<TKey, TValue>(root as MapParentGroup<TKey, TValue>);
				}
			else
				{
				Root=new MapItemGroup<TKey, TValue>(root as MapItemGroup<TKey, TValue>);
				}
			}
		}
	public bool Remove(TKey key)
		{
		var item=new MapItem<TKey, TValue>(Comparer, key);
		lock(Mutex)
			{
			if(Root==null)
				return false;
			if(Root.Remove(item, ref item, Comparer))
				{
				UpdateRoot();
				return true;
				}
			return false;
			}
		}
	public void Set(TKey key, TValue value)
		{
		var item=new MapItem<TKey, TValue>(Comparer, key, value);
		lock(Mutex)
			{
			if(Root==null)
				Root=new MapItemGroup<TKey, TValue>();
			bool exists=false;
			if(Root.Set(item, false, ref exists, Comparer))
				return;
			Root=new MapParentGroup<TKey, TValue>(Root);
			Root.Set(item, true, ref exists, Comparer);
			}
		}

	// Enumeration
	public MapEnumerator<TKey, TValue> At(uint pos)
		{
		var it=new MapEnumerator<TKey, TValue>(this);
		it.SetPosition(pos);
		return it;
		}
	public MapEnumerator<TKey, TValue> Begin()
		{
		var it=new MapEnumerator<TKey, TValue>(this);
		it.SetPosition(0);
		return it;
		}
	public MapEnumerator<TKey, TValue> End()
		{
		var it=new MapEnumerator<TKey, TValue>(this);
		it.SetPosition(uint.MaxValue);
		return it;
		}
	public MapEnumerator<TKey, TValue> Find(TKey key, FindFunc func=FindFunc.Any)
		{
		bool exists=false;
		return Find(key, func, ref exists);
		}
	public MapEnumerator<TKey, TValue> Find(TKey key, FindFunc func, ref bool exists)
		{
		var it=new MapEnumerator<TKey, TValue>(this);
		it.Find(key, func, ref exists);
		return it;
		}
	IEnumerator IEnumerable.GetEnumerator()
		{
		return new MapEnumerator<TKey, TValue>(this);
		}
	public virtual IEnumerator<MapItem<TKey, TValue>> GetEnumerator()
		{
		return new MapEnumerator<TKey, TValue>(this);
		}
	}


//============
// Enumerator
//============

public class MapEnumerator<TKey, TValue>: ClusterEnumerator<MapItem<TKey, TValue>>
	where TKey: IComparable<TKey>
	{
	// Con-/Destructors
	internal MapEnumerator(Map<TKey, TValue> map): base(map) {}

	// Common
	private Map<TKey, TValue> Map { get { return Cluster as Map<TKey, TValue>; } }

	// Enumeration
	public bool Find(TKey key, FindFunc func=FindFunc.Any)
		{
		bool exists=false;
		return Find(key, func, ref exists);
		}
	public bool Find(TKey key, FindFunc func, ref bool exists)
		{
		Reset();
 		var group=Cluster.Root as IMapGroup<TKey, TValue>;
		if(group==null)
			return false;
		var item=new MapItem<TKey, TValue>(Map.Comparer, key);
		ushort pos=0;
		for(int i=0; i<Pointers.Length; i++)
			{
			Pointers[i].Group=group;
			if(!group.Find(item, func, ref pos, ref exists, Map.Comparer))
				return false;
			Pointers[i].Position=pos;
			var parent=group as MapParentGroup<TKey, TValue>;
			if(parent==null)
				break;
			group=parent.Children[pos] as IMapGroup<TKey, TValue>;
			}
		var item_group=group as ClusterItemGroup<MapItem<TKey, TValue>>;
		_Current=item_group.Items[pos];
		_HasCurrent=true;
		return true;
		}
	}
