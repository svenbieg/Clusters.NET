//=========
// List.cs
//=========

// Windows.NET-implementation of an ordererd list.
// Items can be inserted and removed at random positions in constant low time.

// Copyright 2024, Sven Bieg (svenbieg@web.de)
// http://github.com/svenbieg/Clusters.NET

using System;

namespace Clusters
	{
	public class List<T>: Cluster<T> where T: class
		{
		#region Con-/Destructors
		public List() {}
		public List(List<T> copy) { CopyFrom(copy); }
		#endregion

		#region Common
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
						return default;
					return Root.First;
					}
				}
			}
		internal new IListGroup<T> Root
			{
			get { return base.Root as IListGroup<T>; }
			set { base.Root=value; }
			}
		#endregion

		#region Modification
		public void Append(T item)
			{
			lock(Mutex)
				{
				if(Root==null)
					Root=new ListItemGroup<T>();
				if(Root.Append(item, false))
					return;
				Root=new ListParentGroup<T>(Root);
				Root.Append(item, true);
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
		public T Pop()
			{
			lock(Mutex)
				{
				if(Root==null)
					throw new IndexOutOfRangeException();
				return Root.RemoveAt(Root.ItemCount-1);
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
		#endregion
		}
	}
