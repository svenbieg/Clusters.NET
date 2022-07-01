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

public class MapItem<TKey, TValue> where TKey: IComparable<TKey>
{
// Con-/Destructors
public MapItem(TKey key, TValue? value)
	{
	_Key=key;
	_Value=value;
	}

// Common
public TKey Key { get { return _Key; }}
private TKey _Key;
public TValue? Value
	{
	get { return _Value; }
	set { _Value = value; }
	}
private TValue? _Value;
};


//=======
// Group
//=======

public interface IMapGroup<TKey, TValue>: IClusterGroup<MapItem<TKey, TValue>> where TKey: IComparable<TKey>
{
// Access
public bool Contains(TKey Key);
public int Find(TKey Key);
public MapItem<TKey, TValue>? First { get; }
public TValue? Get(TKey Key);
public MapItem<TKey, TValue>? Last { get; }

// Modification
public bool Add(TKey Key, TValue? Value, bool Again, ref bool Exists);
public bool Remove(TKey Key);
public bool Set(TKey Key, TValue? Value, bool Again, ref bool Exists);
};


//============
// Item-Group
//============

internal class MapItemGroup<TKey, TValue>: ClusterItemGroup<MapItem<TKey, TValue>>, IMapGroup<TKey, TValue> where TKey: IComparable<TKey>
{
// Con-/Destructors
public MapItemGroup(int Capacity): base(Capacity) {}
public MapItemGroup(MapItemGroup<TKey, TValue> Group): base(Group.Items) {}

// Access
public virtual bool Contains(TKey Key)
	{
	return GetItemPos(Key)>=0;
	}
public virtual int Find(TKey Key)
	{
	return GetItemPos(Key);
	}
public virtual MapItem<TKey, TValue>? First
	{
	get { return Items[0]; }
	}
public virtual TValue? Get(TKey Key)
	{
	int pos=GetItemPos(Key);
	if(pos<0)
		return default;
	return Items[pos].Value;
	}
private int GetInsertPos(TKey Key, ref bool Exists)
	{
	int start=0;
	int end=Items.Count;
	while(start<end)
		{
		int pos=start+(end-start)/2;
		var item=Items[pos];
		if(item.Key.CompareTo(Key)>0)
			{
			end=pos;
			continue;
			}
		if(item.Key.CompareTo(Key)<0)
			{
			start=pos+1;
			continue;
			}
		Exists=true;
		return pos;
		}
	return start;
	}
private int GetItemPos(TKey Key)
	{
	int start=0;
	int end=Items.Count;
	int pos=0;
	while(start<end)
		{
		pos=start+(end-start)/2;
		var item=Items[pos];
		if(item.Key.CompareTo(Key)>0)
			{
			end=pos;
			continue;
			}
		if(item.Key.CompareTo(Key)<0)
			{
			start=pos+1;
			continue;
			}
		return pos;
		}
	return -pos-1;
	}
public virtual MapItem<TKey, TValue>? Last
	{
	get { return Items[Items.Count-1]; }
	}

// Modification
public virtual bool Add(TKey Key, TValue? Value, bool Again, ref bool Exists)
	{
	int pos=GetInsertPos(Key, ref Exists);
	if(Exists)
		return false;
	return AddInternal(Key, Value, pos);
	}
private bool AddInternal(TKey Key, TValue? Value, int pos)
	{
	if(Items.Count==Items.Capacity)
		return false;
	Items.Insert(pos, new MapItem<TKey, TValue>(Key, Value));
	return true;
	}
public virtual bool Remove(TKey Key)
	{
	int pos=GetItemPos(Key);
	if(pos<0)
		return false;
	RemoveAt(pos);
	return true;
	}
public virtual bool Set(TKey Key, TValue? Value, bool Again, ref bool Exists)
	{
	int pos=GetInsertPos(Key, ref Exists);
	if(Exists)
		{
		Items[pos].Value=Value;
		return true;
		}
	return AddInternal(Key, Value, pos);
	}
};


//==============
// Parent-Group
//==============

internal class MapParentGroup<TKey, TValue>: ClusterParentGroup<MapItem<TKey, TValue>, IMapGroup<TKey, TValue>>, IMapGroup<TKey, TValue> where TKey: IComparable<TKey>
{
// Con-Destructors
public MapParentGroup(int Capacity, int Level): base(Capacity, Level) {}
public MapParentGroup(int Capacity, IMapGroup<TKey, TValue> Child): base(Capacity, Child)
	{
	_First=Child.First;
	_Last=Child.Last;
	}
public MapParentGroup(MapParentGroup<TKey, TValue> Group): base(Group)
	{
	UpdateBounds();
	}

// Access
public virtual bool Contains(TKey Key)
	{
	return GetItemPos(Key)>=0;
	}
public virtual int Find(TKey Key)
	{
	return GetItemPos(Key);
	}
public virtual MapItem<TKey, TValue>? First
	{
	get { return _First; }
	}
private MapItem<TKey, TValue>? _First;
public virtual TValue? Get(TKey Key)
	{
	int pos=GetItemPos(Key);
	if(pos<0)
		return default;
	return Children[pos].Get(Key);
	}
private int GetInsertPos(TKey Key, ref int Group, ref bool Exists)
	{
	if(Children.Count==0)
		return 0;
	int start=0;
	int end=Children.Count;
	while(start<end)
		{
		int pos=start+(end-start)/2;
		var child=Children[pos];
		if(child.First.Key.CompareTo(Key)==0||child.Last.Key.CompareTo(Key)==0)
			{
			Exists=true;
			Group=pos;
			return 1;
			}
		if(child.First.Key.CompareTo(Key)>0)
			{
			end=pos;
			continue;
			}
		if(child.Last.Key.CompareTo(Key)<0)
			{
			start=pos+1;
			continue;
			}
		start=pos;
		break;
		}
	if(start>Children.Count-1)
		start=Children.Count-1;
	Group=start;
	if(start>0)
		{
		if(Children[start].First.Key.CompareTo(Key)>=0)
			{
			Group=start-1;
			return 2;
			}
		}
	if(start+1<Children.Count)
		{
		if(Children[start].Last.Key.CompareTo(Key)<=0)
			return 2;
		}
	return 1;
	}
private int GetItemPos(TKey Key)
	{
	int start=0;
	int end=Children.Count;
	int pos=0;
	while(start<end)
		{
		pos=start+(end-start)/2;
		var child=Children[pos];
		if(child.First.Key.CompareTo(Key)>0)
			{
			end=pos;
			continue;
			}
		if(child.Last.Key.CompareTo(Key)<0)
			{
			start=pos+1;
			continue;
			}
		return pos;
		}
	return -pos-1;
	}
public virtual MapItem<TKey, TValue>? Last
	{
	get { return _Last; }
	}
private MapItem<TKey, TValue>? _Last;

// Modification
public virtual bool Add(TKey Key, TValue? Value, bool Again, ref bool Exists)
	{
	if(AddInternal(Key, Value, Again, ref Exists))
		{
		_ItemCount++;
		UpdateBounds();
		return true;
		}
	return false;
	}
private bool AddInternal(TKey Key, TValue? Value, bool Again, ref bool Exists)
	{
	int group=0;
	int insert_count=GetInsertPos(Key, ref group, ref Exists);
	if(Exists)
		return false;
	if(!Again)
		{
		for(int i=0; i<insert_count; i++)
			{
			if(Children[group+i].Add(Key, Value, false, ref Exists))
				return true;
			if(Exists)
				return false;
			}
		if(ShiftChildren(group, insert_count))
			{
			insert_count=GetInsertPos(Key, ref group, ref Exists);
			for(int i=0; i<insert_count; i++)
				{
				if(Children[group+i].Add(Key, Value, false, ref Exists))
					return true;
				}
			}
		}
	if(!SplitChild(group))
		return false;
	insert_count=GetInsertPos(Key, ref group, ref Exists);
	for(int i=0; i<insert_count; i++)
		{
		if(Children[group+i].Add(Key, Value, true, ref Exists))
			return true;
		}
	return false;
	}
protected override void AppendGroups(IEnumerable<IMapGroup<TKey, TValue>> Groups)
	{
	base.AppendGroups(Groups);
	UpdateBounds();
	}
protected override void InsertGroups(int Position, IEnumerable<IMapGroup<TKey, TValue>> Groups)
	{
	base.AppendGroups(Groups);
	UpdateBounds();
	}
public virtual bool Remove(TKey Key)
	{
	int pos=GetItemPos(Key);
	if(pos<0)
		return false;
	if(!Children[pos].Remove(Key))
		return false;
	_ItemCount--;
	CombineChildren(pos);
	UpdateBounds();
	return true;
	}
public override void RemoveAt(long Position)
	{
	base.RemoveAt(Position);
	UpdateBounds();
	}
protected override System.Collections.Generic.List<IMapGroup<TKey, TValue>> RemoveGroups(int Position, int Count)
	{
	var groups=base.RemoveGroups(Position, Count);
	UpdateBounds();
	return groups;
	}
public virtual bool Set(TKey Key, TValue? Value, bool Again, ref bool Exists)
	{
	if(SetInternal(Key, Value, Again, ref Exists))
		{
		if(!Exists)
			{
			_ItemCount++;
			UpdateBounds();
			}
		return true;
		}
	return false;
	}
private bool SetInternal(TKey Key, TValue? Value, bool Again, ref bool Exists)
	{
	int pos=GetItemPos(Key);
	if(pos>=0)
		{
		if(Children[pos].Set(Key, Value, Again, ref Exists))
			return true;
		}
	return AddInternal(Key, Value, Again, ref Exists);
	}
private bool SplitChild(int Position)
	{
	if(Children.Count==Children.Capacity)
		return false;
	IMapGroup<TKey, TValue> group;
	if(Level>1)
		{
		group=new MapParentGroup<TKey, TValue>(Children.Capacity, Level-1);
		}
	else
		{
		group=new MapItemGroup<TKey, TValue>(Children.Capacity);
		}
	Children.Insert(Position+1, group);
	MoveChildren(Position, Position+1, 1);
	return true;
	}
private void UpdateBounds()
	{
	_First=Children[0].First;
	_Last=Children[Children.Count-1].Last;
	}
};


//=====
// Map
//=====

public class Map<TKey, TValue>: Cluster<MapItem<TKey, TValue>, IMapGroup<TKey, TValue>> where TKey: IComparable<TKey>
{
// Con-/Destructors
public Map(): base() {}
public Map(Map<TKey, TValue> Map): base()
	{
	CopyFrom(Map);
	}

// Access
public TValue? this[TKey Key]
	{
	get => Get(Key);
	set => Set(Key, value);
	}
public bool Contains(TKey Key)
	{
	lock(CriticalSection)
		{
		if(_Root==null)
			return false;
		return _Root.Contains(Key);
		}
	}
public TValue? Get(TKey Key)
	{
	lock(CriticalSection)
		{
		if(_Root==null)
			return default;
		return _Root.Get(Key);
		}
	}
internal override IClusterGroup<MapItem<TKey, TValue>>? Root
	{
	get => _Root;
	}
private IMapGroup<TKey, TValue>? _Root;

// Modification
public bool Add(TKey Key, TValue? Value)
	{
	lock(CriticalSection)
		{
		if(_Root==null)
			_Root=new MapItemGroup<TKey, TValue>(GroupSize);
		bool exists=false;
		if(_Root.Add(Key, Value, false, ref exists))
			return true;
		if(exists)
			return false;
		_Root=new MapParentGroup<TKey, TValue>(GroupSize, _Root);
		return _Root.Add(Key, Value, true, ref exists);
		}
	}
public void Clear()
	{
	lock(CriticalSection)
		{
		_Root=null;
		}
	}
public void CopyFrom(Map<TKey, TValue> Map)
	{
	lock(CriticalSection)
		{
		_Root=null;
		var root=Map._Root;
		if(root==null)
			return;
		if(root.Level>0)
			{
			_Root=new MapParentGroup<TKey, TValue>((MapParentGroup<TKey, TValue>)root);
			}
		else
			{
			_Root=new MapItemGroup<TKey, TValue>((MapItemGroup<TKey, TValue>)root);
			}
		}
	}
public bool Remove(TKey Key)
	{
	lock(CriticalSection)
		{
		if(_Root==null)
			return false;
		if(_Root.Remove(Key))
			{
			UpdateRoot();
			return true;
			}
		return false;
		}
	}
public void Set(TKey Key, TValue? Value)
	{
	lock(CriticalSection)
		{
		if(_Root==null)
			_Root=new MapItemGroup<TKey, TValue>(GroupSize);
		bool exists=false;
		if(_Root.Set(Key, Value, false, ref exists))
			return;
		_Root=new MapParentGroup<TKey, TValue>(GroupSize, _Root);
		_Root.Set(Key, Value, true, ref exists);
		}
	}
internal override void UpdateRoot()
	{
	if(_Root==null)
		return;
	if(_Root.Level==0)
		{
		if(_Root.ChildCount==0)
			_Root=null;
		return;
		}
	if(_Root.ChildCount>1)
		return;
	var root=(MapParentGroup<TKey, TValue>)_Root;
	_Root=root.Children[0];
	}
};

} // namespace
