//============
// Cluster.cs
//============

// Base-class for Index and Map

// Copyright 2024, Sven Bieg (svenbieg@web.de)
// http://github.com/svenbieg/Clusters.NET

using System.Collections;


//===========
// Namespace
//===========

namespace Clusters;


//=======
// Group
//=======

internal interface IClusterGroup<T>
	{
	// Common
	ushort ChildCount { get; }
	uint ItemCount { get; }
	ushort Level { get; }

	// Access
	T GetAt(uint Position);

	// Modification
	T RemoveAt(uint Position);
	void SetAt(uint Position, T Item);
	}


//============
// Item-Group
//============

internal class ClusterItemGroup<T>: IClusterGroup<T>
	{
	// Con-/Destructors
	internal ClusterItemGroup()
		{
		Items=new T[GroupSize];
		_ItemCount=0;
		}
	internal ClusterItemGroup(ClusterItemGroup<T> copy)
		{
		Items=new T[GroupSize];
		_ItemCount=copy._ItemCount;
		for(int i=0; i<_ItemCount; i++)
			Items[i]=copy.Items[i];
		}

	// Common
	public ushort ChildCount { get { return _ItemCount; } }
	internal const ushort GroupSize=Cluster<T>.GroupSize;
	public uint ItemCount { get { return _ItemCount; } }
	protected ushort _ItemCount;
	internal T[] Items;
	public ushort Level { get { return 0; } }

	// Access
	public T GetAt(uint pos) { return Items[pos]; }

	// Modification
	internal bool AppendItem(T item)
		{
		if(_ItemCount==GroupSize)
			return false;
		Items[_ItemCount++]=item;
		return true;
		}
	internal void AppendItems(T[] items, ushort pos, ushort count)
		{
		for(int i=0; i<count; i++)
			Items[_ItemCount+i]=items[pos+i];
		_ItemCount+=count;
		}
	internal bool InsertItem(ushort at, T item)
		{
		if(_ItemCount==GroupSize)
			return false;
		for(int i=_ItemCount; i>=at+1; i--)
			Items[i]=Items[i-1];
		Items[at]=item;
		_ItemCount++;
		return true;
		}
	internal void InsertItems(ushort at, T[] items, ushort pos, ushort count)
		{
		for(int i=_ItemCount+count-1; i>=at+count; i--)
			Items[i]=Items[i-count];
		for(int i=0; i<count; i++)
			Items[at+i]=items[pos+i];
		_ItemCount+=count;
		}
	public T RemoveAt(uint pos)
		{
		var item=Items[pos];
		for(int i=(int)pos; i+1<_ItemCount; i++)
			Items[i]=Items[i+1];
		Items[_ItemCount-1]=default;
		_ItemCount--;
		return item;
		}
	internal void RemoveItems(ushort pos, ushort count)
		{
		for(int i=pos; i+count<_ItemCount; i++)
			Items[i]=Items[i+count];
		for(int i=0; i<count; i++)
			Items[_ItemCount-i-1]=default;
		_ItemCount-=count;
		}
	public void SetAt(uint pos, T item)
		{
		Items[pos]=item;
		}
	}


//==============
// Parent-Group
//==============

internal class ClusterParentGroup<T>: IClusterGroup<T>
	{
	// Con-/Destructors
	internal ClusterParentGroup(int level)
		{
		Children=new IClusterGroup<T>[GroupSize];
		_ChildCount=0;
		_ItemCount=0;
		_Level=(ushort)level;
		}
	internal ClusterParentGroup(IClusterGroup<T> child)
		{
		Children=new IClusterGroup<T>[GroupSize];
		Children[0]=child;
		_ChildCount=1;
		_ItemCount=child.ItemCount;
		_Level=(ushort)(child.Level+1);
		}
	internal ClusterParentGroup(ClusterParentGroup<T> copy)
		{
		_ChildCount=copy._ChildCount;
		_ItemCount=copy._ItemCount;
		_Level=copy._Level;
		Children=new IClusterGroup<T>[GroupSize];
		if(_Level>1)
			{
			for(int i=0; i<copy.ChildCount; i++)
				{
				var child=copy.Children[i] as ClusterParentGroup<T>;
				Children[i]=new ClusterParentGroup<T>(child);
				}
			}
		else
			{
			for(int i=0; i<copy.ChildCount; i++)
				{
				var child=copy.Children[i] as ClusterItemGroup<T>;
				Children[i]=new ClusterItemGroup<T>(child);
				}
			}
		}

	// Common
	internal IClusterGroup<T>[] Children;
	public ushort ChildCount { get { return _ChildCount; } }
	protected ushort _ChildCount;
	internal const ushort GroupSize=Cluster<T>.GroupSize;
	public uint ItemCount { get { return _ItemCount; } }
	protected uint _ItemCount;
	public ushort Level { get { return _Level; } }
	protected ushort _Level;

	// Access
	public T GetAt(uint pos)
		{
		ushort group=GetGroup(ref pos);
		return Children[group].GetAt(pos);
		}
	internal ushort GetGroup(ref uint pos)
		{
		ushort group=0;
		foreach(var child in Children)
			{
			if(pos<child.ItemCount)
				return group;
			pos-=child.ItemCount;
			group++;
			}
		return ushort.MaxValue;
		}
	protected bool GetNearestSpace(ref int space, int pos)
		{
		int before=pos-1;
		int after=pos+1;
		while(before>=0||after<_ChildCount)
			{
			if(before>=0)
				{
				if(Children[before].ChildCount<GroupSize)
					{
					space=before;
					return true;
					}
				before--;
				}
			if(after<_ChildCount)
				{
				if(Children[after].ChildCount<GroupSize)
					{
					space=after;
					return true;
					}
				after++;
				}
			}
		return false;
		}

	// Modification
	protected virtual void AppendGroups(IClusterGroup<T>[] groups, int pos, ushort count)
		{
		for(int i=0; i<count; i++)
			{
			Children[_ChildCount+i]=groups[pos+i];
			_ItemCount+=groups[pos+i].ItemCount;
			}
		_ChildCount+=count;
		}
	protected virtual void InsertGroup(int at, IClusterGroup<T> group)
		{
		_ItemCount+=group.ItemCount;
		for(int i=_ChildCount; i>=at+1; i--)
			Children[i]=Children[i-1];
		Children[at]=group;
		_ChildCount++;
		}
	protected virtual void InsertGroups(int at, IClusterGroup<T>[] groups, int pos, ushort count)
		{
		for(int i=0; i<count; i++)
			_ItemCount+=groups[pos+i].ItemCount;
		for(int i=_ChildCount+count-1; i>=at+count; i--)
			Children[i]=Children[i-count];
		for(int i=0; i<count; i++)
			Children[at+i]=groups[pos+i];
		_ChildCount+=count;
		}
	protected bool CombineChildren(int pos)
		{
		ushort current=Children[pos].ChildCount;
		if(current==0)
			{
			RemoveGroup(pos);
			return true;
			}
		if(pos>0)
			{
			ushort before=Children[pos-1].ChildCount;
			if(current+before<=GroupSize)
				{
				MoveChildren(pos, pos-1, current);
				RemoveGroup(pos);
				return true;
				}
			}
		if(pos+1<_ChildCount)
			{
			ushort after=Children[pos+1].ChildCount;
			if(current+after<=GroupSize)
				{
				MoveChildren(pos+1, pos, after);
				RemoveGroup(pos+1);
				return true;
				}
			}
		return false;
		}
	protected void MoveChildren(int from, int to, ushort count)
		{
		if(_Level>1)
			{
			var src=Children[from] as ClusterParentGroup<T>;
			var dst=Children[to] as ClusterParentGroup<T>;
			if(from>to)
				{
				dst.AppendGroups(src.Children, 0, count);
				src.RemoveGroups(0, count);
				}
			else
				{
				ushort pos=(ushort)(src.ChildCount-count);
				dst.InsertGroups(0, src.Children, pos, count);
				src.RemoveGroups(pos, count);
				}
			}
		else
			{
			var src=Children[from] as ClusterItemGroup<T>;
			var dst=Children[to] as ClusterItemGroup<T>;
			if(from>to)
				{
				dst.AppendItems(src.Items, 0, count);
				src.RemoveItems(0, count);
				}
			else
				{
				ushort pos=(ushort)(src.ChildCount-count);
				dst.InsertItems(0, src.Items, pos, count);
				src.RemoveItems(pos, count);
				}
			}
		}
	protected void MoveEmptySlot(int from, int to)
		{
		if(from<to)
			{
			for(int pos=from; pos<to; pos++)
				MoveChildren(pos+1, pos, 1);
			}
		else
			{
			for(int pos=from; pos>to; pos--)
				MoveChildren(pos-1, pos, 1);
			}
		}
	public virtual T RemoveAt(uint pos)
		{
		ushort group=GetGroup(ref pos);
		var item=Children[group].RemoveAt(pos);
		_ItemCount--;
		CombineChildren(group);
		return item;
		}
	protected void RemoveGroup(int pos)
		{
		_ItemCount-=Children[pos].ItemCount;
		for(int i=pos; i+1<_ChildCount; i++)
			Children[i]=Children[i+1];
		Children[_ChildCount-1]=null;
		_ChildCount--;
		}
	protected virtual void RemoveGroups(int pos, ushort count)
		{
		for(int i=0; i<count; i++)
			_ItemCount-=Children[pos+i].ItemCount;
		for(int i=pos; i+count<_ChildCount; i++)
			Children[i]=Children[i+count];
		for(int i=0; i<count; i++)
			Children[_ChildCount-i-1]=null;
		_ChildCount-=count;
		}
	public void SetAt(uint pos, T item)
		{
		ushort group=GetGroup(ref pos);
		Children[group].SetAt(pos, item);
		}
	protected bool ShiftChildren(int pos, ushort count)
		{
		int space=0;
		if(!GetNearestSpace(ref space, pos))
			return false;
		if(count>1&&space>pos)
			pos++;
		MoveEmptySlot(space, pos);
		return true;
		}
	}


//=========
// Cluster
//=========

public abstract class Cluster<T>
	{
	// Common
	internal const ushort GroupSize=10;
	public uint Length
		{
		get
			{
			lock(Mutex)
				{
				if(Root==null)
					return 0;
				return Root.ItemCount;
				}
			}
		}
	internal Object Mutex=new Object();
	internal IClusterGroup<T> Root;

	// Access
	public T GetAt(uint pos)
		{
		lock(Mutex)
			{
			if(Root==null||pos>=Root.ItemCount)
				throw new IndexOutOfRangeException();
			return Root.GetAt(pos);
			}
		}

	// Modification
	public void Clear()
		{
		lock(Mutex)
			{
			Root=null;
			}
		}
	public T RemoveAt(uint pos)
		{
		lock(Mutex)
			{
			if(Root==null)
				throw new IndexOutOfRangeException();
			if(pos==uint.MaxValue)
				pos=Root.ItemCount-1;
			if(pos>=Root.ItemCount)
				throw new IndexOutOfRangeException();
			T item=Root.RemoveAt(pos);
			UpdateRoot();
			return item;
			}
		}
	internal void UpdateRoot()
		{
		if(Root==null)
			return;
		if(Root.Level==0)
			{
			if(Root.ChildCount==0)
				Root=null;
			return;
			}
		if(Root.ChildCount>1)
			return;
		var root=Root as ClusterParentGroup<T>;
		Root=root.Children[0];
		}
	}


//============
// Enumerator
//============

internal struct ClusterPointer<T>
	{
	internal IClusterGroup<T> Group;
	internal ushort Position;
	}

public class ClusterEnumerator<T>: IEnumerator<T>
	{
	// Con-/Destructors
	internal ClusterEnumerator(Cluster<T> cluster)
		{
		Monitor.Enter(cluster.Mutex);
		Cluster=cluster;
		var level_count=1;
		var root=cluster.Root;
		if(root!=null)
			level_count=root.Level+1;
		Pointers=new ClusterPointer<T>[level_count];
		}
	public void Dispose()
		{
		if(Cluster==null)
			return;
		Monitor.Exit(Cluster.Mutex);
		Cluster=null;
		Pointers=null;
		_HasCurrent=false;
		}

	// Common
	internal Cluster<T> Cluster;
	Object IEnumerator.Current { get { return Current; } }
	public T Current
		{
		get
			{
			if(!_HasCurrent)
				throw new IndexOutOfRangeException();
			return _Current;
			}
		}
	protected T _Current;
	public bool HasCurrent { get { return _HasCurrent; } }
	internal bool _HasCurrent=false;
	internal ClusterPointer<T>[] Pointers;

	// Enumeration
	public uint GetPosition()
		{
		int level=Pointers.Length-1;
		if(Pointers[level].Group==null)
			throw new IndexOutOfRangeException();
		uint pos=Pointers[level].Position;
		for(int i=0; i<Pointers.Length-1; i++)
			{
			var group=Pointers[i].Group as ClusterParentGroup<T>;
			for(int j=0; j<Pointers[i].Position; j++)
				pos+=group.Children[j].ItemCount;
			}
		return pos;
		}
	public virtual bool MoveNext()
		{
		int level=Pointers.Length-1;
		var group=Pointers[level].Group;
		if(group==null)
			return SetPosition(0);
		int count=group.ChildCount;
		ushort pos=Pointers[level].Position;
		if(pos+1<count)
			{
			pos++;
			Pointers[level].Position=pos;
			var item_group=Pointers[level].Group as ClusterItemGroup<T>;
			_Current=item_group.Items[pos];
			return true;
			}
		for(int i=Pointers.Length-1; i>0; i--)
			{
			group=Pointers[i-1].Group;
			count=group.ChildCount;
			if(Pointers[i-1].Position+1>=count)
				continue;
			Pointers[i-1].Position++;
			for(; i<Pointers.Length; i++)
				{
				pos=Pointers[i-1].Position;
				var parent_group=group as ClusterParentGroup<T>;
				group=parent_group.Children[pos];
				pos=0;
				Pointers[i].Group=group;
				Pointers[i].Position=pos;
				}
			var item_group=Pointers[level].Group as ClusterItemGroup<T>;
			_Current=item_group.Items[pos];
			return true;
			}
		Pointers[level].Group=null;
		_HasCurrent=false;
		return false;
		}
	public bool MovePrevious()
		{
		int level=Pointers.Length-1;
		var group=Pointers[level].Group;
		if(group==null)
			return false;
		ushort pos=Pointers[level].Position;
		if(pos>0)
			{
			pos--;
			Pointers[level].Position=pos;
			var item_group=Pointers[level].Group as ClusterItemGroup<T>;
			_Current=item_group.Items[pos];
			return true;
			}
		for(int i=Pointers.Length-1; i>0; i--)
			{
			if(Pointers[i-1].Position==0)
				continue;
			group=Pointers[i-1].Group;
			Pointers[i-1].Position--;
			for(; i<Pointers.Length; i++)
				{
				pos=Pointers[i-1].Position;
				var parent_group=group as ClusterParentGroup<T>;
				group=parent_group.Children[pos];
				pos=(ushort)(group.ChildCount-1);
				Pointers[i].Group=group;
				Pointers[i].Position=pos;
				}
			var item_group=Pointers[level].Group as ClusterItemGroup<T>;
			_Current=item_group.Items[pos];
			return true;
			}
		Pointers[level].Group=null;
		_HasCurrent=false;
		return false;
		}
	public uint Position
		{
		get { return GetPosition(); }
		set
			{
			if(!SetPosition(value))
				throw new IndexOutOfRangeException();
			}
		}
	public virtual void Reset()
		{
		int level=Pointers.Length-1;
		Pointers[level].Group=null;
		_HasCurrent=false;
		}
	public virtual bool SetPosition(uint pos)
		{
		Reset();
 			var group=Cluster.Root;
		if(group==null)
			return false;
		if(pos==uint.MaxValue)
			pos=group.ItemCount-1;
		var level=Pointers.Length-1;
		for(int i=0; i<level; i++)
			{
			Pointers[i].Group=group;
			var parent=group as ClusterParentGroup<T>;
			ushort group_pos=parent.GetGroup(ref pos);
			if(group_pos==ushort.MaxValue)
				return false;
			group=parent.Children[group_pos];
			}
		if(pos>=group.ChildCount)
			return false;
		Pointers[level].Group=group;
		Pointers[level].Position=(ushort)pos;
		var item_group=group as ClusterItemGroup<T>;
		_Current=item_group.Items[pos];
		_HasCurrent=true;
		return true;
		}

	// Modification
	public void RemoveCurrent()
		{
		int level=Pointers.Length-1;
		if(Pointers[level].Group==null)
			throw new IndexOutOfRangeException();
		var pos=GetPosition();
		Cluster.Root.RemoveAt(pos);
		SetPosition(pos);
		}
	}
