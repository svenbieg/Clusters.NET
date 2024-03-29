﻿//=========
// List.cs
//=========

// .NET-implementation of an ordererd list.
// Items can be inserted and removed at random positions in constant low time.

// Copyright 2024, Sven Bieg (svenbieg@web.de)
// http://github.com/svenbieg/Clusters.NET

using System.Collections;
using System.Reflection.Emit;


//===========
// Namespace
//===========

namespace Clusters;


//=======
// Group
//=======

internal interface IListGroup<T>: IClusterGroup<T>
	{
	// Common
	T First { get; }
	T Last { get; }

	// Modification
	bool Append(T item, bool again);
	bool InsertAt(uint pos, T item, bool again);
	}


//============
// Item-Group
//============

internal class ListItemGroup<T>: ClusterItemGroup<T>, IListGroup<T>
	{
	// Con-/Destructors
	internal ListItemGroup() {}
	internal ListItemGroup(ListItemGroup<T> copy): base(copy) {}

	// Common
	public T First { get { return Items[0]; } }
	public T Last { get { return Items[_ItemCount-1]; } }

	// Modification
	public bool Append(T item, bool again)
		{
		return AppendItem(item);
		}
	public bool InsertAt(uint pos, T item, bool again)
		{
		return InsertItem((ushort)pos, item);
		}
	}


//==============
// Parent-Group
//==============

internal class ListParentGroup<T>: ClusterParentGroup<T>, IListGroup<T>
	{
	// Con-Destructors
	internal ListParentGroup(int level): base(level) {}
	internal ListParentGroup(IListGroup<T> child): base(child) {}
	internal ListParentGroup(ListParentGroup<T> copy): base(copy) {}

	// Common
	public T First
		{
		get
			{
			var first_child=Children[0] as IListGroup<T>;
			return first_child.First;
			}
		}
	public T Last
		{
		get
			{
			var last_child=Children[_ChildCount-1] as IListGroup<T>;
			return last_child.Last;
			}
		}

	// Access
	private ushort GetInsertPos(ref uint pos, ref ushort group)
		{
		for(ushort u=0; u<_ChildCount; u++)
			{
			var item_count=Children[u].ItemCount;
			if(pos<=item_count)
				{
				group=u;
				if(pos==item_count&&u+1<_ChildCount)
					return 2;
				return 1;
				}
			pos-=item_count;
			}
		return 0;
		}

	// Modification
	public bool Append(T item, bool again)
		{
		var pos=_ChildCount-1;
		var child=Children[pos] as IListGroup<T>;
		if(!again)
			{
			if(child.Append(item, false))
				{
				_ItemCount++;
				return true;
				}
			int space=0;
			if(GetNearestSpace(ref space, pos))
				{
				MoveEmptySlot(space, pos);
				child.Append(item, false);
				_ItemCount++;
				return true;
				}
			}
		if(!SplitChild(pos))
			return false;
		child=Children[pos+1] as IListGroup<T>;
		if(!child.Append(item, true))
			return false;
		_ItemCount++;
		return true;
		}
	public bool InsertAt(uint position, T item, bool again)
		{
		uint pos=position;
		ushort group=0;
		ushort count=GetInsertPos(ref pos, ref group);
		if(count==0)
			return false;
		IListGroup<T> child;
		if(!again)
			{
			uint at=pos;
			for(ushort u=0; u<count; u++)
				{
				child=Children[group+u] as IListGroup<T>;
				if(child.InsertAt(at, item, false))
					{
					_ItemCount++;
					return true;
					}
				at=0;
				}
			if(ShiftChildren(group, count))
				{
				pos=position;
				count=GetInsertPos(ref pos, ref group);
				at=pos;
				for(ushort u=0; u<count; u++)
					{
					child=Children[group+u] as IListGroup<T>;
					if(child.InsertAt(at, item, false))
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
		uint item_count=Children[group].ItemCount;
		if(pos>=item_count)
			{
			group++;
			pos-=item_count;
			}
		child=Children[group] as IListGroup<T>;
		child.InsertAt(pos, item, true);
		_ItemCount++;
		return true;
		}
	private bool SplitChild(int pos)
		{
		if(_ChildCount==GroupSize)
			return false;
		IListGroup<T> group;
		if(_Level>1)
			{
			group=new ListParentGroup<T>(_Level-1);
			}
		else
			{
			group=new ListItemGroup<T>();
			}
		InsertGroup(pos+1, group);
		MoveChildren(pos, pos+1, 1);
		return true;
		}
	}


//======
// List
//======

public class List<T>: Cluster<T>, IEnumerable<T>
	{
	// Con-/Destructors
	public List() {}
	public List(T[] copy) { CopyFrom(copy); }
	public List(List<T> copy) { CopyFrom(copy); }

	// Common
	public T this[uint pos]
		{
		get { return GetAt(pos); }
		set { SetAt(pos, value); }
		}
	public T First
		{
		get
			{
			lock(Mutex)
				{
				if(Root==null)
					throw new IndexOutOfRangeException();
				return Root.First;
				}
			}
		}
	public T Last
		{
		get
			{
			lock(Mutex)
				{
				if(Root==null)
					throw new IndexOutOfRangeException();
				return Root.Last;
				}
			}
		}
	internal new IListGroup<T> Root
		{
		get { return base.Root as IListGroup<T>; }
		set { base.Root=value; }
		}

	// Modification
	public void Append(T item)
		{
		lock(Mutex)
			{
			if(Root==null)
				Root=new ListItemGroup<T>();
			AppendInternal(item);
			}
		}
	internal void AppendInternal(T item)
		{
		if(Root.Append(item, false))
			return;
		Root=new ListParentGroup<T>(Root);
		Root.Append(item, true);
		}
	public void CopyFrom(T[] copy)
		{
		lock(Mutex)
			{
			Root=new ListItemGroup<T>();
			foreach(T item in copy)
				AppendInternal(item);
			}
		}
	public void CopyFrom(List<T> copy)
		{
		lock(Mutex)
			{
			Root=null;
			var root=copy.Root;
			if(root==null)
				return;
			if(root.Level>0)
				{
				Root=new ListParentGroup<T>(root as ListParentGroup<T>);
				}
			else
				{
				Root=new ListItemGroup<T>(root as ListItemGroup<T>);
				}
			}
		}
	public void InsertAt(uint pos, T item)
		{
		lock(Mutex)
			{
			if(Root==null)
				{
				if(pos>0)
					throw new IndexOutOfRangeException();
				Root=new ListItemGroup<T>();
				Root.Append(item, false);
				return;
				}
			if(Root.InsertAt(pos, item, false))
				return;
			Root=new ListParentGroup<T>(Root);
			Root.InsertAt(pos, item, true);
			}
		}
	public void SetAt(uint pos, T item)
		{
		lock(Mutex)
			{
			if(Root==null||pos>=Root.ItemCount)
				throw new IndexOutOfRangeException();
			Root.SetAt(pos, item);
			}
		}

	// Enumeration
	public ListEnumerator<T> At(uint pos)
		{
		var it=new ListEnumerator<T>(this);
		it.SetPosition(pos);
		return it;
		}
	public ListEnumerator<T> Begin()
		{
		var it=new ListEnumerator<T>(this);
		it.SetPosition(0);
		return it;
		}
	public ListEnumerator<T> End()
		{
		var it=new ListEnumerator<T>(this);
		it.SetPosition(uint.MaxValue);
		return it;
		}
	IEnumerator IEnumerable.GetEnumerator()
		{
		return new ListEnumerator<T>(this);
		}
	public virtual IEnumerator<T> GetEnumerator()
		{
		return new ListEnumerator<T>(this);
		}
	}


//============
// Enumerator
//============

public class ListEnumerator<T>: ClusterEnumerator<T>, IEnumerator<T>
	{
	// Con-/Destructors
	internal ListEnumerator(List<T> list): base(list) {}

	// Common
	Object IEnumerator.Current { get { return Current; } }
	public T Current
		{
		get
			{
			if(!_HasCurrent)
				throw new IndexOutOfRangeException();
			var level=Pointers.Length-1;
			var item_group=Pointers[level].Group as ListItemGroup<T>;
			var pos=Pointers[level].Position;
			return item_group.Items[pos];
			}
		set
			{
			if(!_HasCurrent)
				throw new IndexOutOfRangeException();
			var level=Pointers.Length-1;
			var item_group=Pointers[level].Group as ListItemGroup<T>;
			var pos=Pointers[level].Position;
			item_group.Items[pos]=value;
			}
		}
	}
