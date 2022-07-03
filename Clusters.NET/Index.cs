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


//=======
// Group
//=======

public interface IIndexGroup<TKey, TItem>: IClusterGroup<TItem>
	where TKey: IComparable<TKey>
	where TItem: IComparable<TKey>, IComparable<TItem>
{
// Access
public bool Contains(TKey Key);
public int Find(TKey Key);
public TItem? Get(TKey Key);
public TItem? First { get; }
public TItem? Last { get; }

// Modification
public bool Add(TKey Key, TItem Item, bool Again, ref bool Exists);
public bool Remove(TKey Key);
public bool Set(TKey Key, TItem Item, bool Again, ref bool Exists);
};


//============
// Item-Group
//============

internal class IndexItemGroup<TKey, TItem>:
	ClusterItemGroup<TItem>, IIndexGroup<TKey, TItem>
	where TKey: IComparable<TKey>
	where TItem: IComparable<TKey>, IComparable<TItem>
{
// Con-/Destructors
public IndexItemGroup(int Capacity): base(Capacity) {}
public IndexItemGroup(IndexItemGroup<TKey, TItem> Group): base(Group.Items) {}

// Access
public virtual bool Contains(TKey Key)
	{
	return GetItemPos(Key)>=0;
	}
public virtual int Find(TKey Key)
	{
	return GetItemPos(Key);
	}
public virtual TItem? First
	{
	get { return Items[0]; }
	}
public virtual TItem? Get(TKey Key)
	{
	int pos=GetItemPos(Key);
	if(pos<0)
		return default;
	return Items[pos];
	}
private int GetInsertPos(TKey Key, ref bool Exists)
	{
	int start=0;
	int end=Items.Count;
	while(start<end)
		{
		int pos=start+(end-start)/2;
		var item=Items[pos];
		if(item.CompareTo(Key)>0)
			{
			end=pos;
			continue;
			}
		if(item.CompareTo(Key)<0)
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
		if(item.CompareTo(Key)>0)
			{
			end=pos;
			continue;
			}
		if(item.CompareTo(Key)<0)
			{
			start=pos+1;
			continue;
			}
		return pos;
		}
	return -pos-1;
	}
public virtual TItem? Last
	{
	get { return Items[Items.Count-1]; }
	}

// Modification
public virtual bool Add(TKey Key, TItem Item, bool Again, ref bool Exists)
	{
	int pos=GetInsertPos(Key, ref Exists);
	if(Exists)
		return false;
	return AddInternal(Item, pos);
	}
private bool AddInternal(TItem Item, int pos)
	{
	if(Items.Count==Items.Capacity)
		return false;
	Items.Insert(pos, Item);
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
public virtual bool Set(TKey Key, TItem Item, bool Again, ref bool Exists)
	{
	int pos=GetInsertPos(Key, ref Exists);
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

internal class IndexParentGroup<TKey, TItem>:
	ClusterParentGroup<TItem, IIndexGroup<TKey, TItem>>,
	IIndexGroup<TKey, TItem>
	where TKey: IComparable<TKey>
	where TItem: IComparable<TKey>, IComparable<TItem>
{
// Con-Destructors
public IndexParentGroup(int Capacity, int Level): base(Capacity, Level) {}
public IndexParentGroup(int Capacity, IIndexGroup<TKey, TItem> Child): base(Capacity, Child)
	{
	_First=Child.First;
	_Last=Child.Last;
	}
public IndexParentGroup(IndexParentGroup<TKey, TItem> Group): base(Group)
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
public virtual TItem? First
	{
	get { return _First; }
	}
private TItem? _First;
public virtual TItem? Get(TKey Key)
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
		if(child.First.CompareTo(Key)==0||child.Last.CompareTo(Key)==0)
			{
			Exists=true;
			Group=pos;
			return 1;
			}
		if(child.First.CompareTo(Key)>0)
			{
			end=pos;
			continue;
			}
		if(child.Last.CompareTo(Key)<0)
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
		if(Children[start].First.CompareTo(Key)>=0)
			{
			Group=start-1;
			return 2;
			}
		}
	if(start+1<Children.Count)
		{
		if(Children[start].Last.CompareTo(Key)<=0)
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
		if(child.First.CompareTo(Key)>0)
			{
			end=pos;
			continue;
			}
		if(child.Last.CompareTo(Key)<0)
			{
			start=pos+1;
			continue;
			}
		return pos;
		}
	return -pos-1;
	}
public virtual TItem? Last
	{
	get { return _Last; }
	}
private TItem? _Last;

// Modification
public virtual bool Add(TKey Key, TItem Item, bool Again, ref bool Exists)
	{
	if(AddInternal(Key, Item, Again, ref Exists))
		{
		_ItemCount++;
		UpdateBounds();
		return true;
		}
	return false;
	}
private bool AddInternal(TKey Key, TItem Item, bool Again, ref bool Exists)
	{
	int group=0;
	int insert_count=GetInsertPos(Key, ref group, ref Exists);
	if(Exists)
		return false;
	if(!Again)
		{
		for(int i=0; i<insert_count; i++)
			{
			if(Children[group+i].Add(Key, Item, false, ref Exists))
				return true;
			if(Exists)
				return false;
			}
		if(ShiftChildren(group, insert_count))
			{
			insert_count=GetInsertPos(Key, ref group, ref Exists);
			for(int i=0; i<insert_count; i++)
				{
				if(Children[group+i].Add(Key, Item, false, ref Exists))
					return true;
				}
			}
		}
	if(!SplitChild(group))
		return false;
	insert_count=GetInsertPos(Key, ref group, ref Exists);
	for(int i=0; i<insert_count; i++)
		{
		if(Children[group+i].Add(Key, Item, true, ref Exists))
			return true;
		}
	return false;
	}
protected override void AppendGroups(IEnumerable<IIndexGroup<TKey, TItem>> Groups)
	{
	base.AppendGroups(Groups);
	UpdateBounds();
	}
protected override void InsertGroups(int Position, IEnumerable<IIndexGroup<TKey, TItem>> Groups)
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
protected override System.Collections.Generic.List<IIndexGroup<TKey, TItem>> RemoveGroups(int Position, int Count)
	{
	var groups=base.RemoveGroups(Position, Count);
	UpdateBounds();
	return groups;
	}
public virtual bool Set(TKey Key, TItem Item, bool Again, ref bool Exists)
	{
	if(SetInternal(Key, Item, Again, ref Exists))
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
private bool SetInternal(TKey Key, TItem Item, bool Again, ref bool Exists)
	{
	int pos=GetItemPos(Key);
	if(pos>=0)
		{
		if(Children[pos].Set(Key, Item, Again, ref Exists))
			return true;
		}
	return AddInternal(Key, Item, Again, ref Exists);
	}
private bool SplitChild(int Position)
	{
	if(Children.Count==Children.Capacity)
		return false;
	IIndexGroup<TKey, TItem> group;
	if(Level>1)
		{
		group=new IndexParentGroup<TKey, TItem>(Children.Capacity, Level-1);
		}
	else
		{
		group=new IndexItemGroup<TKey, TItem>(Children.Capacity);
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

public class Index<TKey, TItem>:
	Cluster<TItem, IIndexGroup<TKey, TItem>>
	where TKey: IComparable<TKey>
	where TItem: IComparable<TKey>, IComparable<TItem>
{
// Con-/Destructors
protected Index(): base() {}
protected Index(Index<TKey, TItem> Index): base()
	{
	CopyFrom(Index);
	}

// Access
public bool Contains(TKey Key)
	{
	lock(CriticalSection)
		{
		if(_Root==null)
			return false;
		return _Root.Contains(Key);
		}
	}
internal TItem? Get(TKey Key)
	{
	lock (CriticalSection)
		{
		if(_Root==null)
			return default;
		return _Root.Get(Key);
		}
	}
internal override IClusterGroup<TItem>? Root
	{
	get => _Root;
	}
internal IIndexGroup<TKey, TItem>? _Root;

// Modification
internal bool Add(TKey Key, TItem Item)
	{
	lock(CriticalSection)
		{
		if(_Root==null)
			_Root=new IndexItemGroup<TKey, TItem>(GroupSize);
		bool exists=false;
		if(_Root.Add(Key, Item, false, ref exists))
			return true;
		if(exists)
			return false;
		_Root=new IndexParentGroup<TKey, TItem>(GroupSize, _Root);
		return _Root.Add(Key, Item, true, ref exists);
		}
	}
public void Clear()
	{
	lock(CriticalSection)
		{
		_Root=null;
		}
	}
public void CopyFrom(Index<TKey, TItem> Index)
	{
	lock(CriticalSection)
		{
		_Root=null;
		var root=Index._Root;
		if(root==null)
			return;
		if(root.Level>0)
			{
			_Root=new IndexParentGroup<TKey, TItem>((IndexParentGroup<TKey, TItem>)root);
			}
		else
			{
			_Root=new IndexItemGroup<TKey, TItem>((IndexItemGroup<TKey, TItem>)root);
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
internal void Set(TKey Key, TItem Item)
	{
	lock(CriticalSection)
		{
		if(_Root==null)
			_Root=new IndexItemGroup<TKey, TItem>(GroupSize);
		bool exists=false;
		if(_Root.Set(Key, Item, false, ref exists))
			return;
		_Root=new IndexParentGroup<TKey, TItem>(GroupSize, _Root);
		_Root.Set(Key, Item, true, ref exists);
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
	var root=(IndexParentGroup<TKey, TItem>)_Root;
	_Root=root.Children[0];
	}
};

public class Index<T>: Index<T, T> where T: IComparable<T>
{
// Con-/Destructors
public Index(): base() {}
public Index(Index<T> Index): base(Index) {}

// Modification
public bool Add(T Item)
	{
	return Add(Item, Item);
	}
}

} // namespace
