//============
// Cluster.cs
//============

// Interfaces and base-classes for the List and the Index

// Copyright 2022, Sven Bieg (svenbieg@web.de)
// http://github.com/svenbieg/Clusters.NET


//=======
// Using
//=======

using System.Collections;


//===========
// Namespace
//===========

namespace Clusters {


//=======
// Group
//=======

public interface IClusterGroup<T>
{
// Access
public int ChildCount { get; }
public T GetAt(long Position);
public long ItemCount { get; }
public int Level { get; }

// Modification
public void RemoveAt(long Position);
public void SetAt(long Position, T Item);
}


//============
// Item-Group
//============

internal abstract class ClusterItemGroup<T>: IClusterGroup<T>
{
// Con-/Destructors
protected ClusterItemGroup(int Capacity)
	{
	Items=new System.Collections.Generic.List<T>(Capacity);
	}
protected ClusterItemGroup(IEnumerable<T> Items)
	{
	this.Items=new System.Collections.Generic.List<T>(Items);
	}
~ClusterItemGroup()
	{
	Items.Clear();
	}

// Access
public virtual int ChildCount
	{
	get { return Items.Count; }
	}
public virtual T GetAt(long Position)
	{
	if(Position>=Items.Count)
		throw new IndexOutOfRangeException();
	return Items[(int)Position];
	}
protected System.Collections.Generic.List<T> Items;
public virtual long ItemCount
	{
	get { return Items.Count; }
	}
public virtual int Level
	{
	get { return 0; }
	}

// Modification
public bool Append(T Item, bool Again)
	{
	if(Items.Count==Items.Capacity)
		return false;
	Items.Add(Item);
	return true;
	}
internal void AppendItems(IEnumerable<T> Append)
	{
	Items.InsertRange(Items.Count, Append);
	}
public bool InsertAt(long Position, T Item, bool Again)
	{
	if(Items.Count==Items.Capacity)
		return false;
	Items.Insert((int)Position, Item);
	return true;
	}
internal void InsertItems(int Position, IEnumerable<T> Insert)
	{
	Items.InsertRange(Position, Insert);
	}
public void RemoveAt(long Position)
	{
	Items.RemoveAt((int)Position);
	}
internal System.Collections.Generic.List<T> RemoveItems(int Position, int Count)
	{
	var items=Items.GetRange(Position, Count);
	Items.RemoveRange(Position, Count);
	return items;
	}
public virtual void SetAt(long Position, T Item)
	{
	Items[(int)Position]=Item;
	}
}


//==============
// Parent-Group
//==============

internal abstract class ClusterParentGroup<T, TGroup>: IClusterGroup<T> where TGroup: IClusterGroup<T>
{
// Con-/Destructors
protected ClusterParentGroup(int Capacity, int Level)
	{
	Children=new System.Collections.Generic.List<TGroup>(Capacity);
	_ItemCount=0;
	_Level=Level;
	}
protected ClusterParentGroup(int Capacity, TGroup Child)
	{
	Children=new System.Collections.Generic.List<TGroup>(Capacity);
	Children.Add(Child);
	_ItemCount=Child.ItemCount;
	_Level=Child.Level+1;
	}
protected ClusterParentGroup(ClusterParentGroup<T, TGroup> Group)
	{
	Children=new System.Collections.Generic.List<TGroup>(Group.Children);
	_ItemCount=Group.ItemCount;
	_Level=Group.Level;
	}
~ClusterParentGroup()
	{
	Children.Clear();
	}

// Access
internal System.Collections.Generic.List<TGroup> Children;
public virtual int ChildCount
	{
	get { return Children.Count; }
	}
public virtual T GetAt(long Position)
	{
	int group=GetGroup(ref Position);
	if(group==-1)
		throw new IndexOutOfRangeException();
	return Children[group].GetAt(Position);
	}
public virtual IClusterGroup<T> GetChild(int Position)
	{
	return Children[Position];
	}
protected int GetGroup(ref long Position)
	{
	int group=0;
	foreach(var child in Children)
		{
		if(Position<child.ItemCount)
			return group;
		Position-=child.ItemCount;
		group++;
		}
	return -1;
	}
protected int GetNearestGroupNotFull(int Position)
	{
	int before=Position-1;
	int after=Position+1;
	while(before>=0||after<Children.Count)
		{
		if(before>=0)
			{
			if(Children[before].ChildCount<Children.Capacity)
				return before;
			before--;
			}
		if(after<Children.Count)
			{
			if(Children[after].ChildCount<Children.Capacity)
				return after;
			after++;
			}
		}
	return Children.Capacity;
	}
public virtual long ItemCount
	{
	get { return _ItemCount; }
	}
protected long _ItemCount;
public virtual int Level
	{
	get { return _Level; }
	}
protected int _Level;

// Modification
protected virtual void AppendGroups(IEnumerable<TGroup> Groups)
	{
	foreach(var group in Groups)
		{
		Children.Add(group);
		_ItemCount+=group.ItemCount;
		}
	}
protected virtual void InsertGroups(int Position, IEnumerable<TGroup> Groups)
	{
	foreach(var group in Groups)
		{
		Children.Insert(Position++, group);
		_ItemCount+=group.ItemCount;
		}
	}
protected bool CombineChildren(int Position)
	{
	int current=Children[Position].ChildCount;
	if(current==0)
		{
		Children.RemoveAt(Position);
		return true;
		}
	if(Position>0)
		{
		int before=Children[Position-1].ChildCount;
		if(current+before<=Children.Capacity)
			{
			MoveChildren(Position, Position-1, current);
			Children.RemoveAt(Position);
			return true;
			}
		}
	if(Position+1<Children.Count)
		{
		int after=Children[Position+1].ChildCount;
		if(current+after<=Children.Capacity)
			{
			MoveChildren(Position+1, Position, after);
			Children.RemoveAt(Position+1);
			return true;
			}
		}
	return false;
	}
protected void MoveChildren(int Source, int Destination, int Count)
	{
	if(Level>1)
		{
		var src=Children[Source] as ClusterParentGroup<T, TGroup>;
		var dst=Children[Destination] as ClusterParentGroup<T, TGroup>;
		if(Source>Destination)
			{
			var groups=src.RemoveGroups(0, Count);
			dst.AppendGroups(groups);
			}
		else
			{
			var groups=src.RemoveGroups(src.ChildCount-Count, Count);
			dst.InsertGroups(0, groups);
			}
		}
	else
		{
		var src=Children[Source] as ClusterItemGroup<T>;
		var dst=Children[Destination] as ClusterItemGroup<T>;
		if(Source>Destination)
			{
			var items=src.RemoveItems(0, Count);
			dst.AppendItems(items);
			}
		else
			{
			var items=src.RemoveItems(src.ChildCount-Count, Count);
			dst.InsertItems(0, items);
			}
		}
	}
public void MoveEmptySlot(int Source, int Destination)
	{
	if(Source<Destination)
		{
		for(int pos=Source; pos<Destination; pos++)
			MoveChildren(pos+1, pos, 1);
		}
	else
		{
		for(int pos=Source; pos>Destination; pos--)
			MoveChildren(pos-1, pos, 1);
		}
	}
public virtual void RemoveAt(long Position)
	{
	if(Position>=_ItemCount)
		throw new IndexOutOfRangeException();
	int group=GetGroup(ref Position);
	Children[group].RemoveAt(Position);
	_ItemCount--;
	CombineChildren(group);
	}
protected virtual System.Collections.Generic.List<TGroup> RemoveGroups(int Position, int Count)
	{
	var groups=Children.GetRange(Position, Count);
	Children.RemoveRange(Position, Count);
	foreach(var group in groups)
		{
		_ItemCount-=group.ItemCount;
		}
	return groups;
	}
public virtual void SetAt(long Position, T Item)
	{
	int group=GetGroup(ref Position);
	if(group==-1)
		throw new IndexOutOfRangeException();
	Children[group].SetAt(Position, Item);
	}
protected bool ShiftChildren(int Group, int Count)
	{
	int empty=GetNearestGroupNotFull(Group);
	if(empty<0)
		return false;
	if(Count>1&&empty>Group)
		Group++;
	MoveEmptySlot(empty, Group);
	return true;
	}
}


//=========
// Cluster
//=========

public abstract class Cluster<T, TGroup>: IEnumerable<T> where TGroup: IClusterGroup<T>
{
// Con-/Destructors
protected Cluster()
	{
	CriticalSection=new object();
	}

// Access
public ClusterEnumerator<T, TGroup> At(long Position)
	{
	return new ClusterEnumerator<T, TGroup>(this, Position);
	}
public long Count
	{
	get
		{
		lock(CriticalSection)
			{
			if(Root==null)
				return 0;
			return Root.ItemCount;
			}
		}
	}
internal object CriticalSection;
public ClusterEnumerator<T, TGroup> First()
	{
	return new ClusterEnumerator<T, TGroup>(this, 0);
	}
public T GetAt(long Position)
	{
	lock(CriticalSection)
		{
		if(Root==null)
			throw new IndexOutOfRangeException();
		return Root.GetAt(Position);
		}
	}
IEnumerator IEnumerable.GetEnumerator()
	{
	return new ClusterEnumerator<T, TGroup>(this);
	}
public virtual IEnumerator<T> GetEnumerator()
	{
	return new ClusterEnumerator<T, TGroup>(this);
	}
internal const int GroupSize=10;
public ClusterEnumerator<T, TGroup> Last()
	{
	return new ClusterEnumerator<T, TGroup>(this, -1);
	}
internal abstract IClusterGroup<T>? Root { get; }

// Modification
public void RemoveAt(long Position)
	{
	lock(CriticalSection)
		{
		if(Root==null)
			throw new IndexOutOfRangeException();
		Root.RemoveAt(Position);
		UpdateRoot();
		}
	}
internal abstract void UpdateRoot();
}


//============
// Enumerator
//============

internal class ClusterPointer<T>
{
public ClusterPointer()
	{
	Group=null;
	Position=0;
	}
public IClusterGroup<T>? Group;
public int Position;
}

public class ClusterEnumerator<T, TGroup>: IEnumerator<T> where TGroup: IClusterGroup<T>
{
// Con-/Destructors
internal ClusterEnumerator(Cluster<T, TGroup> Cluster)
	{
	this.Cluster=Cluster;
	Monitor.Enter(Cluster.CriticalSection);
	var level_count=1;
	var root=Cluster.Root;
	if(root!=null)
		level_count=root.Level+1;
	Pointers=new System.Collections.Generic.List<ClusterPointer<T>>(level_count);
	for(int i=0; i<level_count; i++)
		{
		Pointers.Add(new ClusterPointer<T>());
		}
	}
internal ClusterEnumerator(Cluster<T, TGroup> Cluster, long Position): this(Cluster)
	{
	if(!SetPosition(Position))
		throw new IndexOutOfRangeException();
	}
~ClusterEnumerator()
	{
	Monitor.Exit(Cluster.CriticalSection);
	Pointers.Clear();
	}

// Access
object IEnumerator.Current
	{
	get
		{
		if(!HasCurrent)
			throw new IndexOutOfRangeException();
		return _Current;
		}
	}
public virtual T Current
	{
	get
		{
		if(!HasCurrent)
			throw new IndexOutOfRangeException();
		return _Current;
		}
	}
private T? _Current;
public bool HasCurrent
	{
	get
		{
		return Pointers[Pointers.Count-1].Group!=null;
		}
	}

// Navigation
private int GetGroup(IClusterGroup<T> Group, ref long Position)
	{
	int count=Group.ChildCount;
	int level=Group.Level;
	if(level==0)
		{
		int pos=(int)Position;
		Position=0;
		return pos;
		}
	var parent_group=Group as ClusterParentGroup<T, TGroup>;
	if(parent_group==null)
		throw new NullReferenceException();
	for(int i=0; i<count; i++)
		{
		var child=parent_group.GetChild(i);
		long item_count=child.ItemCount;
		if(Position<item_count)
			return i;
		Position-=item_count;
		}
	return -1;
	}
public long GetPosition()
	{
	var ptr=Pointers[Pointers.Count-1];
	if(ptr.Group==null)
		throw new IndexOutOfRangeException();
	long pos=ptr.Position;
	for(int i=0; i<Pointers.Count-1; i++)
		{
		ptr=Pointers[i];
		var group=ptr.Group as ClusterParentGroup<T, TGroup>;
		if(group==null)
			throw new NullReferenceException();
		for(int j=0; j<ptr.Position; j++)
			pos+=group.GetChild(j).ItemCount;
		}
	return pos;
	}
public virtual bool MoveNext()
	{
	var ptr=Pointers[Pointers.Count-1];
	var group=ptr.Group;
	if(group==null)
		return SetPosition(0);
	int count=group.ChildCount;
	if(ptr.Position+1<count)
		{
		ptr.Position++;
		_Current=group.GetAt(ptr.Position);
		return true;
		}
	for(int i=Pointers.Count-1; i>0; i--)
		{
		ptr=Pointers[i-1];
		group=ptr.Group;
		if(group==null)
			throw new NullReferenceException();
		count=group.ChildCount;
		if(ptr.Position+1>=count)
			continue;
		ptr.Position++;
		for(; i<Pointers.Count; i++)
			{
			var parent_group=group as ClusterParentGroup<T, TGroup>;
			if(parent_group==null)
				throw new NullReferenceException();
			group=parent_group.GetChild(ptr.Position);
			ptr=Pointers[i];
			ptr.Group=group;
			ptr.Position=0;
			}
		_Current=group.GetAt(0);
		return true;
		}
	ptr=Pointers[Pointers.Count-1];
	ptr.Group=null;
	return false;
	}
public bool MovePrevious()
	{
	var ptr=Pointers[Pointers.Count-1];
	var group=ptr.Group;
	if(group==null)
		return false;
	if(ptr.Position>0)
		{
		ptr.Position--;
		_Current=group.GetAt(ptr.Position);
		return true;
		}
	for(int i=Pointers.Count-1; i>0; i--)
		{
		ptr=Pointers[i-1];
		if(ptr.Position==0)
			continue;
		group=ptr.Group;
		if(group==null)
			throw new NullReferenceException();
		ptr.Position--;
		int last=0;
		for(; i<Pointers.Count; i++)
			{
			var parent_group=group as ClusterParentGroup<T, TGroup>;
			if(parent_group==null)
				throw new NullReferenceException();
			group=parent_group.GetChild(ptr.Position);
			last=group.ChildCount-1;
			ptr=Pointers[i];
			ptr.Group=group;
			ptr.Position=last;
			}
		_Current=group.GetAt(last);
		return true;
		}
	ptr=Pointers[Pointers.Count-1];
	ptr.Group=null;
	return false;
	}
public long Position
	{
	get => GetPosition();
	set
		{
		if(!SetPosition(value))
			throw new IndexOutOfRangeException();
		}
	}
public virtual void Reset()
	{
	Position=0;
	}
public bool SetPosition(long Position)
	{
	Pointers[Pointers.Count-1].Group=null;
 	var group=Cluster.Root;
	if(group==null)
		return false;
	if(Position==-1)
		Position=group.ItemCount-1;
	int group_pos=GetGroup(group, ref Position);
	if(group_pos==-1)
		return false;
	var ptr=Pointers[0];
	ptr.Group=group;
	ptr.Position=group_pos;
	for(int i=0; i<Pointers.Count-1; i++)
		{
		var parent_group=group as ClusterParentGroup<T, TGroup>;
		if(parent_group==null)
			throw new NullReferenceException();
		group=parent_group.GetChild(group_pos);
		group_pos=GetGroup(group, ref Position);
		if(group_pos<0)
			return false;
		ptr=Pointers[i+1];
		ptr.Group=group;
		ptr.Position=group_pos;
		}
	if(group_pos>=group.ItemCount)
		{
		Pointers[Pointers.Count-1].Group=null;
		return false;
		}
	_Current=group.GetAt(group_pos);
	return true;
	}

// Modification
public virtual void Dispose() {}
public void RemoveCurrent()
	{
	var ptr=Pointers[Pointers.Count-1];
	if(ptr.Group==null)
		throw new IndexOutOfRangeException();
	long pos=GetPosition();
	var root=Cluster.Root;
	if(root==null)
		throw new IndexOutOfRangeException();
	root.RemoveAt(pos);
	SetPosition(pos);
	}

// Common
Cluster<T, TGroup> Cluster;
System.Collections.Generic.List<ClusterPointer<T>> Pointers;
}

}
