//==========
// Index.cs
//==========

// .NET-Implementation of a sorted list.
// Items and can be inserted, removed and looked-up in constant low time.

// Copyright 2024, Sven Bieg (svenbieg@web.de)
// http://github.com/svenbieg/Clusters.NET

using System.Collections;

namespace Clusters;

public class Index<T>: Cluster<T>, IEnumerable<T>
	where T: IComparable<T>
	{
	#region Con-/Destructors
	public Index(IComparer<T> comparer=null)
		{
		if(comparer==null)
			comparer=Comparer<T>.Default;
		Comparer=comparer;
		}
	public Index(Index<T> copy, IComparer<T> comparer=null): this(comparer)
		{
		CopyFrom(copy);
		}
	#endregion

	#region Common
	internal IComparer<T> Comparer;
	public T First
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
	internal new IIndexGroup<T> Root
		{
		get { return base.Root as IIndexGroup<T>; }
		set { base.Root=value; }
		}
	#endregion

	#region Access
	public bool Contains(T item)
		{
		lock(Mutex)
			{
			if(Root==null)
				return false;
			T found=default;
			return Root.TryGet(item, ref found, Comparer);
			}
		}
	#endregion

	#region Modification
	public bool Add(T item)
		{
		lock(Mutex)
			{
			if(Root==null)
				Root=new IndexItemGroup<T>();
			bool exists=false;
			if(Root.Add(item, false, ref exists, Comparer))
				return true;
			if(exists)
				return false;
			Root=new IndexParentGroup<T>(Root);
			return Root.Add(item, true, ref exists, Comparer);
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
	public bool Remove(T item)
		{
		lock(Mutex)
			{
			if(Root==null)
				return false;
			T removed=default;
			if(Root.Remove(item, ref removed, Comparer))
				{
				UpdateRoot();
				return true;
				}
			return false;
			}
		}
	public void Set(T item)
		{
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
	#endregion

	#region Enumeration
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
	#endregion
	}

