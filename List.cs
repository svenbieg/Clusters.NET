//=========
// List.cs
//=========

// Windows.NET-implementation of an ordererd list.
// Items can be inserted and removed at random positions in constant low time.

// Copyright 2022, Sven Bieg (svenbieg@web.de)
// http://github.com/svenbieg/Clusters.NET


//===========
// Namespace
//===========

namespace Clusters {


//=======
// Group
//=======

public interface IListGroup<T>: IClusterGroup<T>
{
// Modification
public bool Append(T Item, bool Again);
public bool InsertAt(long Position, T Item, bool Again);
};


//============
// Item-Group
//============

internal class ListItemGroup<T>: ClusterItemGroup<T>, IListGroup<T>
{
// Con-/Destructors
public ListItemGroup(int Capacity): base(Capacity) {}
public ListItemGroup(ListItemGroup<T> Group): base(Group.Items) {}
}


//==============
// Parent-Group
//==============

internal class ListParentGroup<T>: ClusterParentGroup<T, IListGroup<T>>, IListGroup<T>
{
// Con-Destructors
public ListParentGroup(int Capacity, int Level): base(Capacity, Level) {}
public ListParentGroup(int Capacity, IListGroup<T> Child): base(Capacity, Child) {}
public ListParentGroup(ListParentGroup<T> Group): base(Group) {}

// Access
private (int Group, long Position, int Count) GetInsertPos(long Position)
	{
	int group=0;
	long position=Position;
	int count=0;
	foreach(var child in Children)
		{
		long item_count=child.ItemCount;
		if(position<item_count)
			{
			count=1;
			break;
			}
		if(position==item_count)
			{
			if(group<Children.Count)
				count=2;
			break;
			}
		position-=item_count;
		group++;
		}
	return (group, position, count);
	}

// Modification
public virtual bool Append(T Item, bool Again)
	{
	int group=Children.Count-1;
	if(!Again)
		{
		if(Children[group].Append(Item, false))
			{
			_ItemCount++;
			return true;
			}
		int empty=GetNearestGroupNotFull(group);
		if(empty<Children.Count)
			{
			MoveEmptySlot(empty, group);
			Children[group].Append(Item, false);
			_ItemCount++;
			return true;
			}
		}
	if(!SplitChild(group))
		return false;
	if(!Children[group+1].Append(Item, true))
		return false;
	_ItemCount++;
	return true;
	}
public virtual bool InsertAt(long Position, T Item, bool Again)
	{
	if(Position>ItemCount)
		throw new IndexOutOfRangeException();
	(int group, long pos, int insert_count)=GetInsertPos(Position);
	if(insert_count==0)
		return false;
	if(!Again)
		{
		long at=pos;
		for(int i=0; i<insert_count; i++)
			{
			if(Children[group+i].InsertAt(at, Item, false))
				{
				_ItemCount++;
				return true;
				}
			at=0;
			}
		if(ShiftChildren(group, insert_count))
			{
			(group, pos, insert_count)=GetInsertPos(Position);
			at=pos;
			for(int i=0; i<insert_count; i++)
				{
				if(Children[group+i].InsertAt(at, Item, false))
					{
					_ItemCount++;
					return true;
					}
				at=0;
				}
			}
		}
	if(!SplitChild(group))
		return false;
	long count=Children[group].ItemCount;
	if(pos>=count)
		{
		group++;
		pos-=count;
		}
	Children[group].InsertAt(pos, Item, true);
	_ItemCount++;
	return true;
	}
public bool SplitChild(int Position)
	{
	if(Children.Count==Children.Capacity)
		return false;
	IListGroup<T> group;
	if(Level>1)
		{
		group=new ListParentGroup<T>(Children.Capacity, Level-1);
		}
	else
		{
		group=new ListItemGroup<T>(Children.Capacity);
		}
	Children.Insert(Position+1, group);
	MoveChildren(Position, Position+1, 1);
	return true;
	}
}


//======
// List
//======

public class List<T>: Cluster<T, IListGroup<T>>
{
// Con-/Destructors
public List(): base() {}
public List(List<T> List): base()
	{
	CopyFrom(List);
	}

// Access
public T this[long Position]
	{
	get => GetAt(Position);
	set => SetAt(Position, value);
	}
internal override IClusterGroup<T>? Root
	{
	get => _Root;
	}
IListGroup<T>? _Root;

// Modification
public void Append(T Item)
	{
	lock(CriticalSection)
		{
		if(_Root==null)
			_Root=new ListItemGroup<T>(GroupSize);
		if(_Root.Append(Item, false))
			return;
		_Root=new ListParentGroup<T>(GroupSize, _Root);
		_Root.Append(Item, true);
		}
	}
public void Clear()
	{
	lock(CriticalSection)
		{
		_Root=null;
		}
	}
public void CopyFrom(List<T> List)
	{
	lock(CriticalSection)
		{
		_Root=null;
		var root=List._Root;
		if(root==null)
			return;
		if(root.Level>0)
			{
			_Root=new ListParentGroup<T>((ListParentGroup<T>)root);
			}
		else
			{
			_Root=new ListItemGroup<T>((ListItemGroup<T>)root);
			}
		}
	}
public void InsertAt(long Position, T Item)
	{
	lock(CriticalSection)
		{
		if(_Root==null)
			{
			if(Position>0)
				throw new IndexOutOfRangeException();
			_Root=new ListItemGroup<T>(GroupSize);
			_Root.Append(Item, false);
			return;
			}
		if(_Root.InsertAt(Position, Item, false))
			return;
		_Root=new ListParentGroup<T>(GroupSize, _Root);
		_Root.InsertAt(Position, Item, true);
		}
	}
public void SetAt(long Position, T Item)
	{
	lock(CriticalSection)
		{
		if(_Root==null)
			throw new IndexOutOfRangeException();
		_Root.SetAt(Position, Item);
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
	var root=(ListParentGroup<T>)_Root;
	_Root=root.Children[0];
	}
}

} // namespace
