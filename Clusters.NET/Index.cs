//==========
// Index.cs
//==========

// Windows.NET-Implementation of a sorted list.
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

public class IndexItem<TKey, TValue>: IComparable<IndexItem<TKey, TValue>> where TKey: IComparable<TKey>
{
// Con-/Destructors
public IndexItem(TKey key)
	{
	_Key=key;
	_Value=default;
	}
public IndexItem(TKey key, TValue? value)
	{
	_Key=key;
	_Value=value;
	}

// Common
public virtual int CompareTo(IndexItem<TKey, TValue>? Item)
	{
	if(Item==null)
		{
		if(_Key==null)
			return 0;
		return 1;
		}
	return _Key.CompareTo(Item._Key);
	}
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

public interface IIndexGroup<T>: IClusterGroup<T> where T: IComparable<T>
{
// Access
public bool Contains(T Item);
public int Find(T Item);
public T? First { get; }
public T Get(T Item);
public T? Last { get; }
public bool TryGet(T Item, ref T Found);

// Modification
public bool Add(T Item, bool Again, ref bool Exists);
public bool Remove(T Item);
public bool Set(T Item, bool Again, ref bool Exists);
};


//============
// Item-Group
//============

internal class IndexItemGroup<T>: ClusterItemGroup<T>, IIndexGroup<T> where T: IComparable<T>
{
// Con-/Destructors
public IndexItemGroup(int Capacity): base(Capacity) {}
public IndexItemGroup(IndexItemGroup<T> Group): base(Group.Items) {}

// Access
public virtual bool Contains(T Item)
	{
	return GetItemPos(Item)>=0;
	}
public virtual int Find(T Item)
	{
	return GetItemPos(Item);
	}
public virtual T? First
	{
	get { return Items[0]; }
	}
public virtual T Get(T Item)
	{
	int pos=GetItemPos(Item);
	if(pos<0)
		throw new KeyNotFoundException();
	return Items[pos];
	}
private int GetInsertPos(T Item, ref bool Exists)
	{
	int start=0;
	int end=Items.Count;
	while(start<end)
		{
		int pos=start+(end-start)/2;
		T item=Items[pos];
		if(item.CompareTo(Item)>0)
			{
			end=pos;
			continue;
			}
		if(item.CompareTo(Item)<0)
			{
			start=pos+1;
			continue;
			}
		Exists=true;
		return pos;
		}
	return start;
	}
private int GetItemPos(T Item)
	{
	int start=0;
	int end=Items.Count;
	int pos=0;
	while(start<end)
		{
		pos=start+(end-start)/2;
		T item=Items[pos];
		if(item.CompareTo(Item)>0)
			{
			end=pos;
			continue;
			}
		if(item.CompareTo(Item)<0)
			{
			start=pos+1;
			continue;
			}
		return pos;
		}
	return -pos-1;
	}
public virtual T? Last
	{
	get { return Items[Items.Count-1]; }
	}
public virtual bool TryGet(T Item, ref T Found)
	{
	int pos=GetItemPos(Item);
	if(pos<0)
		return false;
	Found=Items[pos];
	return true;
	}

// Modification
public virtual bool Add(T Item, bool Again, ref bool Exists)
	{
	int pos=GetInsertPos(Item, ref Exists);
	if(Exists)
		return false;
	return AddInternal(Item, pos);
	}
private bool AddInternal(T Item, int pos)
	{
	if(Items.Count==Items.Capacity)
		return false;
	Items.Insert(pos, Item);
	return true;
	}
public virtual bool Remove(T Item)
	{
	int pos=GetItemPos(Item);
	if(pos<0)
		return false;
	RemoveAt(pos);
	return true;
	}
public virtual bool Set(T Item, bool Again, ref bool Exists)
	{
	int pos=GetInsertPos(Item, ref Exists);
	if(Exists)
		{
		Items[pos]=Item;
		return true;
		}
	return AddInternal(Item, pos);
	}
};


//==============
// Parent-Group
//==============

internal class IndexParentGroup<T>: ClusterParentGroup<T, IIndexGroup<T>>, IIndexGroup<T> where T: IComparable<T>
{
// Con-Destructors
public IndexParentGroup(int Capacity, int Level): base(Capacity, Level) {}
public IndexParentGroup(int Capacity, IIndexGroup<T> Child): base(Capacity, Child)
	{
	_First=Child.First;
	_Last=Child.Last;
	}
public IndexParentGroup(IndexParentGroup<T> Group): base(Group)
	{
	UpdateBounds();
	}

// Access
public virtual bool Contains(T Item)
	{
	return GetItemPos(Item)>=0;
	}
public virtual int Find(T Item)
	{
	return GetItemPos(Item);
	}
public virtual T? First
	{
	get { return _First; }
	}
private T? _First;
public virtual T Get(T Item)
	{
	int pos=GetItemPos(Item);
	if(pos<0)
		throw new KeyNotFoundException();
	return Children[pos].Get(Item);
	}
private int GetInsertPos(T Item, ref int Group, ref bool Exists)
	{
	if(Children.Count==0)
		return 0;
	int start=0;
	int end=Children.Count;
	while(start<end)
		{
		int pos=start+(end-start)/2;
		var child=Children[pos];
		if(child.First.CompareTo(Item)==0||child.Last.CompareTo(Item)==0)
			{
			Exists=true;
			Group=pos;
			return 1;
			}
		if(child.First.CompareTo(Item)>0)
			{
			end=pos;
			continue;
			}
		if(child.Last.CompareTo(Item)<0)
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
		if(Children[start].First.CompareTo(Item)>=0)
			{
			Group=start-1;
			return 2;
			}
		}
	if(start+1<Children.Count)
		{
		if(Children[start].Last.CompareTo(Item)<=0)
			return 2;
		}
	return 1;
	}
private int GetItemPos(T Item)
	{
	int start=0;
	int end=Children.Count;
	int pos=0;
	while(start<end)
		{
		pos=start+(end-start)/2;
		var child=Children[pos];
		if(child.First.CompareTo(Item)>0)
			{
			end=pos;
			continue;
			}
		if(child.Last.CompareTo(Item)<0)
			{
			start=pos+1;
			continue;
			}
		return pos;
		}
	return -pos-1;
	}
public virtual T? Last
	{
	get { return _Last; }
	}
private T? _Last;
public virtual bool TryGet(T Item, ref T Found)
	{
	int pos=GetItemPos(Item);
	if(pos<0)
		return false;
	return Children[pos].TryGet(Item, ref Found);
	}

// Modification
public virtual bool Add(T Item, bool Again, ref bool Exists)
	{
	if(AddInternal(Item, Again, ref Exists))
		{
		_ItemCount++;
		UpdateBounds();
		return true;
		}
	return false;
	}
private bool AddInternal(T Item, bool Again, ref bool Exists)
	{
	int group=0;
	int insert_count=GetInsertPos(Item, ref group, ref Exists);
	if(Exists)
		return false;
	if(!Again)
		{
		for(int i=0; i<insert_count; i++)
			{
			if(Children[group+i].Add(Item, false, ref Exists))
				return true;
			if(Exists)
				return false;
			}
		if(ShiftChildren(group, insert_count))
			{
			insert_count=GetInsertPos(Item, ref group, ref Exists);
			for(int i=0; i<insert_count; i++)
				{
				if(Children[group+i].Add(Item, false, ref Exists))
					return true;
				}
			}
		}
	if(!SplitChild(group))
		return false;
	insert_count=GetInsertPos(Item, ref group, ref Exists);
	for(int i=0; i<insert_count; i++)
		{
		if(Children[group+i].Add(Item, true, ref Exists))
			return true;
		}
	return false;
	}
protected override void AppendGroups(IEnumerable<IIndexGroup<T>> Groups)
	{
	base.AppendGroups(Groups);
	UpdateBounds();
	}
protected override void InsertGroups(int Position, IEnumerable<IIndexGroup<T>> Groups)
	{
	base.AppendGroups(Groups);
	UpdateBounds();
	}
public virtual bool Remove(T Item)
	{
	int pos=GetItemPos(Item);
	if(pos<0)
		return false;
	if(!Children[pos].Remove(Item))
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
protected override System.Collections.Generic.List<IIndexGroup<T>> RemoveGroups(int Position, int Count)
	{
	var groups=base.RemoveGroups(Position, Count);
	UpdateBounds();
	return groups;
	}
public virtual bool Set(T Item, bool Again, ref bool Exists)
	{
	if(SetInternal(Item, Again, ref Exists))
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
private bool SetInternal(T Item, bool Again, ref bool Exists)
	{
	int pos=GetItemPos(Item);
	if(pos>=0)
		{
		if(Children[pos].Set(Item, Again, ref Exists))
			return true;
		}
	return AddInternal(Item, Again, ref Exists);
	}
private bool SplitChild(int Position)
	{
	if(Children.Count==Children.Capacity)
		return false;
	IIndexGroup<T> group;
	if(Level>1)
		{
		group=new IndexParentGroup<T>(Children.Capacity, Level-1);
		}
	else
		{
		group=new IndexItemGroup<T>(Children.Capacity);
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


//=======
// Index
//=======

public class Index<T>: Cluster<T, IIndexGroup<T>> where T: IComparable<T>
{
// Con-/Destructors
public Index(): base() {}
public Index(Index<T> Index): base()
	{
	CopyFrom(Index);
	}

// Access
public bool Contains(T Item)
	{
	lock(CriticalSection)
		{
		if(_Root==null)
			return false;
		return _Root.Contains(Item);
		}
	}
internal override IClusterGroup<T>? Root
	{
	get => _Root;
	}
private IIndexGroup<T>? _Root;

// Modification
public bool Add(T Item)
	{
	lock(CriticalSection)
		{
		if(_Root==null)
			_Root=new IndexItemGroup<T>(GroupSize);
		bool exists=false;
		if(_Root.Add(Item, false, ref exists))
			return true;
		if(exists)
			return false;
		_Root=new IndexParentGroup<T>(GroupSize, _Root);
		return _Root.Add(Item, true, ref exists);
		}
	}
public void Clear()
	{
	lock(CriticalSection)
		{
		_Root=null;
		}
	}
public void CopyFrom(Index<T> Index)
	{
	lock(CriticalSection)
		{
		_Root=null;
		var root=Index._Root;
		if(root==null)
			return;
		if(root.Level>0)
			{
			_Root=new IndexParentGroup<T>((IndexParentGroup<T>)root);
			}
		else
			{
			_Root=new IndexItemGroup<T>((IndexItemGroup<T>)root);
			}
		}
	}
public bool Remove(T Item)
	{
	lock(CriticalSection)
		{
		if(_Root==null)
			return false;
		if(_Root.Remove(Item))
			{
			UpdateRoot();
			return true;
			}
		return false;
		}
	}
public void Set(T Item)
	{
	lock(CriticalSection)
		{
		if(_Root==null)
			_Root=new IndexItemGroup<T>(GroupSize);
		bool exists=false;
		if(_Root.Set(Item, false, ref exists))
			return;
		_Root=new IndexParentGroup<T>(GroupSize, _Root);
		_Root.Set(Item, true, ref exists);
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
	var root=(IndexParentGroup<T>)_Root;
	_Root=root.Children[0];
	}
};

} // namespace
