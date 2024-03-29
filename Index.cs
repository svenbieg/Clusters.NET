//==========
// Index.cs
//==========

// .NET-Implementation of a sorted list.
// Items and can be inserted, removed and looked-up in constant low time.

// Copyright 2024, Sven Bieg (svenbieg@web.de)
// http://github.com/svenbieg/Clusters.NET

using System.Collections;


//===========
// Namespace
//===========

namespace Clusters;


//===============
// Find-Function
//===============

public enum FindFunc
	{
	Above,
	AboveOrEqual,
	Any,
	Below,
	BelowOrEqual,
	Equal
	}


//======
// Item
//======

public class IndexItem<T> where T: IComparable<T>
	{
	internal IndexItem(IComparer<T> comparer, T id)
		{
		Hash=comparer.GetHashCode(id);
		_Id=id;
		}
	internal int Hash;
	public T Id { get { return _Id; } }
	private T _Id;
	}


//=======
// Group
//=======

internal interface IIndexGroup<T>: IClusterGroup<IndexItem<T>>
	where T: IComparable<T>
	{
	// Common
	IndexItem<T> First { get; }
	IndexItem<T> Last { get; }

	// Access
	bool Find(IndexItem<T> item, FindFunc func, ref ushort pos, ref bool exists, IComparer<T> comparer);
	bool TryGet(IndexItem<T> item, ref IndexItem<T> found, IComparer<T> comparer);

	// Modification
	bool Add(IndexItem<T> item, bool again, ref bool exists, IComparer<T> comparer);
	bool Remove(IndexItem<T> item, ref IndexItem<T> removed, IComparer<T> comparer);
	bool Set(IndexItem<T> item, bool again, ref bool exists, IComparer<T> comparer);
	}


//============
// Item-Group
//============

internal class IndexItemGroup<T>: ClusterItemGroup<IndexItem<T>>, IIndexGroup<T>
	where T: IComparable<T>
	{
	// Con-/Destructors
	internal IndexItemGroup() {}
	internal IndexItemGroup(IndexItemGroup<T> copy): base(copy) {}

	// Common
	private int Compare(IndexItem<T> item1, IndexItem<T> item2, IComparer<T> comparer)
		{
		if(item1.Hash<item2.Hash)
			return -1;
		if(item1.Hash>item2.Hash)
			return 1;
		return comparer.Compare(item1.Id, item2.Id);
		}
	public IndexItem<T> First { get { return Items[0]; } }
	public virtual IndexItem<T> Last { get { return Items[_ItemCount-1]; } }

	// Access
	public bool Find(IndexItem<T> item, FindFunc func, ref ushort pos, ref bool exists, IComparer<T> comparer)
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
	private ushort GetItemPos(IndexItem<T> item, ref bool exists, IComparer<T> comparer)
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
	public bool TryGet(IndexItem<T> item, ref IndexItem<T> found, IComparer<T> comparer)
		{
		bool exists=false;
		var pos=GetItemPos(item, ref exists, comparer);
		if(exists)
			found=Items[pos];
		return exists;
		}

	// Modification
	public bool Add(IndexItem<T> item, bool again, ref bool exists, IComparer<T> comparer)
		{
		var pos=GetItemPos(item, ref exists, comparer);
		if(exists)
			return false;
		return InsertItem(pos, item);
		}
	public bool Remove(IndexItem<T> item, ref IndexItem<T> removed, IComparer<T> comparer)
		{
		bool exists=false;
		var pos=GetItemPos(item, ref exists, comparer);
		if(!exists)
			return false;
		removed=RemoveAt(pos);
		return true;
		}
	public bool Set(IndexItem<T> item, bool again, ref bool exists, IComparer<T> comparer)
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

internal class IndexParentGroup<T>: ClusterParentGroup<IndexItem<T>>, IIndexGroup<T>
	where T: IComparable<T>
	{
	// Con-Destructors
	public IndexParentGroup(ushort level): base(level) {}
	public IndexParentGroup(IIndexGroup<T> child): base(child)
		{
		_First=child.First;
		_Last=child.Last;
		}
	public IndexParentGroup(IndexParentGroup<T> copy): base(copy)
		{
		UpdateBounds();
		}

	// Common
	private int Compare(IndexItem<T> item1, IndexItem<T> item2, IComparer<T> comparer)
		{
		if(item1.Hash<item2.Hash)
			return -1;
		if(item1.Hash>item2.Hash)
			return 1;
		return comparer.Compare(item1.Id, item2.Id);
		}
	public IndexItem<T> First { get { return _First; } }
	private IndexItem<T> _First;
	public IndexItem<T> Last { get { return _Last; } }
	private IndexItem<T> _Last;

	// Access
	public bool Find(IndexItem<T> item, FindFunc func, ref ushort pos, ref bool exists, IComparer<T> comparer)
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
					var child=Children[pos] as IIndexGroup<T>;
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
					var child=Children[pos] as IIndexGroup<T>;
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
	private ushort GetItemPos(IndexItem<T> item, ref ushort group, bool must_exist, IComparer<T> comparer)
		{
		ushort start=0;
		ushort end=_ChildCount;
		while(start<end)
			{
			ushort pos=(ushort)(start+(end-start)/2);
			var child=Children[pos] as IIndexGroup<T>;
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
			var child=Children[start] as IIndexGroup<T>;
			if(Compare(child.First, item, comparer)>0)
				{
				group=(ushort)(start-1);
				return 2;
				}
			}
		if(start+1<_ChildCount)
			{
			var child=Children[start] as IIndexGroup<T>;
			if(Compare(child.Last, item, comparer)<0)
				return 2;
			}
		return 1;
		}
	public bool TryGet(IndexItem<T> item, ref IndexItem<T> found, IComparer<T> comparer)
		{
		ushort pos=0;
		ushort count=GetItemPos(item, ref pos, true, comparer);
		if(count!=1)
			return false;
		var child=Children[pos] as IIndexGroup<T>;
		return child.TryGet(item, ref found, comparer);
		}

	// Modification
	public virtual bool Add(IndexItem<T> item, bool again, ref bool exists, IComparer<T> comparer)
		{
		if(AddInternal(item, again, ref exists, comparer))
			{
			_ItemCount++;
			UpdateBounds();
			return true;
			}
		return false;
		}
	private bool AddInternal(IndexItem<T> item, bool again, ref bool exists, IComparer<T> comparer)
		{
		ushort group=0;
		ushort count=GetItemPos(item, ref group, false, comparer);
		if(!again)
			{
			for(ushort u=0; u<count; u++)
				{
				var child=Children[group+u] as IIndexGroup<T>;
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
					var child=Children[group+u] as IIndexGroup<T>;
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
			var child=Children[group+u] as IIndexGroup<T>;
			if(child.Add(item, true, ref exists, comparer))
				return true;
			}
		return false;
		}
	protected override void AppendGroups(IClusterGroup<IndexItem<T>>[] groups, int pos, ushort count)
		{
		base.AppendGroups(groups, pos, count);
		UpdateBounds();
		}
	protected override void InsertGroups(int at, IClusterGroup<IndexItem<T>>[] groups, int pos, ushort count)
		{
		base.InsertGroups(at, groups, pos, count);
		UpdateBounds();
		}
	public bool Remove(IndexItem<T> item, ref IndexItem<T> removed, IComparer<T> comparer)
		{
		ushort pos=0;
		if(GetItemPos(item, ref pos, true, comparer)==0)
			return false;
		var child=Children[pos] as IIndexGroup<T>;
		if(!child.Remove(item, ref removed, comparer))
			return false;
		_ItemCount--;
		CombineChildren(pos);
		UpdateBounds();
		return true;
		}
	public override IndexItem<T> RemoveAt(uint pos)
		{
		IndexItem<T> item=base.RemoveAt(pos);
		UpdateBounds();
		return item;
		}
	protected override void RemoveGroups(int pos, ushort count)
		{
		base.RemoveGroups(pos, count);
		UpdateBounds();
		}
	public bool Set(IndexItem<T> item, bool again, ref bool exists, IComparer<T> comparer)
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
	private bool SetInternal(IndexItem<T> item, bool again, ref bool exists, IComparer<T> comparer)
		{
		ushort pos=0;
		var count=GetItemPos(item, ref pos, true, comparer);
		if(count>0)
			{
			var child=Children[pos] as IIndexGroup<T>;
			if(child.Set(item, again, ref exists, comparer))
				return true;
			}
		return AddInternal(item, again, ref exists, comparer);
		}
	private bool SplitChild(int pos)
		{
		if(_ChildCount==GroupSize)
			return false;
		IIndexGroup<T> group;
		if(_Level>1)
			{
			group=new IndexParentGroup<T>((ushort)(_Level-1));
			}
		else
			{
			group=new IndexItemGroup<T>();
			}
		InsertGroup(pos+1, group);
		MoveChildren(pos, pos+1, 1);
		return true;
		}
	private void UpdateBounds()
		{
		if(_ChildCount>0)
			{
			var first_child=Children[0] as IIndexGroup<T>;
			var last_child=Children[_ChildCount-1] as IIndexGroup<T>;
			_First=first_child.First;
			_Last=last_child.Last;
			}
		}
	}


//=======
// Index
//=======

public class Index<T>: Cluster<IndexItem<T>>, IEnumerable<T>
	where T: IComparable<T>
	{
	// Con-/Destructors
	public Index(IComparer<T> comparer=null)
		{
		if(comparer==null)
			comparer=Comparer<T>.Default;
		Comparer=comparer;
		}
	public Index(T[] copy)
		{
		CopyFrom(copy);
		}
	public Index(Index<T> copy, IComparer<T> comparer=null): this(comparer)
		{
		CopyFrom(copy);
		}

	// Common
	internal IComparer<T> Comparer;
	public IndexItem<T> First
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
	public IndexItem<T> Last
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
	internal new IIndexGroup<T> Root
		{
		get { return base.Root as IIndexGroup<T>; }
		set { base.Root=value; }
		}

	// Access
	public bool Contains(T id)
		{
		lock(Mutex)
			{
			if(Root==null)
				return false;
			var item=new IndexItem<T>(Comparer, id);
			return Root.TryGet(item, ref item, Comparer);
			}
		}

	// Modification
	public bool Add(T id)
		{
		lock(Mutex)
			{
			if(Root==null)
				Root=new IndexItemGroup<T>();
			return AddInternal(id);
			}
		}
	internal bool AddInternal(T id)
		{
		var item=new IndexItem<T>(Comparer, id);
		bool exists=false;
		if(Root.Add(item, false, ref exists, Comparer))
			return true;
		if(exists)
			return false;
		Root=new IndexParentGroup<T>(Root);
		return Root.Add(item, true, ref exists, Comparer);
		}
	public void CopyFrom(T[] copy)
		{
		lock(Mutex)
			{
			if(Root==null)
				Root=new IndexItemGroup<T>();
			foreach(T id in copy)
				AddInternal(id);
			}
		}
	public void CopyFrom(Index<T> copy)
		{
		lock(Mutex)
			{
			Root=null;
			var root=copy.Root;
			if(root==null)
				return;
			if(root.Level>0)
				{
				Root=new IndexParentGroup<T>(root as IndexParentGroup<T>);
				}
			else
				{
				Root=new IndexItemGroup<T>(root as IndexItemGroup<T>);
				}
			}
		}
	public bool Remove(T id)
		{
		var item=new IndexItem<T>(Comparer, id);
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
	public void Set(T id)
		{
		var item=new IndexItem<T>(Comparer, id);
		lock(Mutex)
			{
			if(Root==null)
				Root=new IndexItemGroup<T>();
			bool exists=false;
			if(Root.Set(item, false, ref exists, Comparer))
				return;
			Root=new IndexParentGroup<T>(Root);
			Root.Set(item, true, ref exists, Comparer);
			}
		}

	// Enumeration
	public IndexEnumerator<T> At(uint pos)
		{
		var it=new IndexEnumerator<T>(this);
		it.SetPosition(pos);
		return it;
		}
	public IndexEnumerator<T> Begin()
		{
		var it=new IndexEnumerator<T>(this);
		it.SetPosition(0);
		return it;
		}
	public IndexEnumerator<T> End()
		{
		var it=new IndexEnumerator<T>(this);
		it.SetPosition(uint.MaxValue);
		return it;
		}
	public IndexEnumerator<T> Find(T item, FindFunc func=FindFunc.Any)
		{
		bool exists=false;
		return Find(item, func, ref exists);
		}
	public IndexEnumerator<T> Find(T item, FindFunc func, ref bool exists)
		{
		var it=new IndexEnumerator<T>(this);
		it.Find(item, func, ref exists);
		return it;
		}
	IEnumerator IEnumerable.GetEnumerator()
		{
		return new IndexEnumerator<T>(this);
		}
	public virtual IEnumerator<T> GetEnumerator()
		{
		return new IndexEnumerator<T>(this);
		}
	}


//============
// Enumerator
//============

public class IndexEnumerator<T>: ClusterEnumerator<IndexItem<T>>, IEnumerator<T>
	where T: IComparable<T>
	{
	// Con-/Destructors
	internal IndexEnumerator(Index<T> index): base(index) {}

	// Common
	Object IEnumerator.Current { get { return Current; } }
	public T Current
		{
		get
			{
			if(!_HasCurrent)
				throw new IndexOutOfRangeException();
			var level=Pointers.Length-1;
			var item_group=Pointers[level].Group as IndexItemGroup<T>;
			var pos=Pointers[level].Position;
			return item_group.Items[pos].Id;
			}
		}
	private Index<T> Index { get { return Cluster as Index<T>; } }

	// Enumeration
	public bool Find(T id, FindFunc func=FindFunc.Any)
		{
		bool exists=false;
		return Find(id, func, ref exists);
		}
	public bool Find(T id, FindFunc func, ref bool exists)
		{
		Reset();
 		var group=Cluster.Root as IIndexGroup<T>;
		if(group==null)
			return false;
		var item=new IndexItem<T>(Index.Comparer, id);
		ushort pos=0;
		for(int i=0; i<Pointers.Length; i++)
			{
			Pointers[i].Group=group;
			if(!group.Find(item, func, ref pos, ref exists, Index.Comparer))
				return false;
			Pointers[i].Position=pos;
			var parent=group as IndexParentGroup<T>;
			if(parent==null)
				break;
			group=parent.Children[pos] as IIndexGroup<T>;
			}
		_HasCurrent=true;
		return true;
		}
	}
