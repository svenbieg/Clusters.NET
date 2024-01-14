//==========
// Index.cs
//==========

// Windows.NET-Implementation of a sorted list.
// Items and can be inserted, removed and looked-up in constant low time.

// Copyright 2024, Sven Bieg (svenbieg@web.de)
// http://github.com/svenbieg/Clusters.NET

using System;
using System.Collections.Generic;

namespace Clusters
	{
	public class Index<T>: Cluster<T> where T: class, IComparable<T>
		{
		#region Con-/Destructors
		public Index(IComparer<T> comparer=default)
			{
			Comparer=comparer;
			}
		public Index(Index<T> copy, IComparer<T> comparer=default)
			{
			Comparer=comparer;
			CopyFrom(copy);
			}
		#endregion

		#region Common
		private IComparer<T> Comparer;
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
				ushort pos=0;
				bool exists=false;
				if(!Root.Find(item, FindFunc.Equal, ref pos, ref exists, Comparer))
					return false;
				return exists;
				}
			}
		#endregion

		#region Modification
		internal bool Add(T item)
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
		internal void Set(T item)
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
		}
	}
