﻿//========
// Map.cs
//========

// Windows.NET-Implementation of a sorted map.
// Items and can be inserted, removed and looked-up in constant low time.

// Copyright 2024, Sven Bieg (svenbieg@web.de)
// http://github.com/svenbieg/Clusters.NET

using System;
using System.Collections;
using System.Collections.Generic;

namespace Clusters
	{
	public class Map<TKey, TValue>:
		Cluster<MapEntry<TKey, TValue>>, IEnumerable<MapItem<TKey, TValue>>
		where TKey: IComparable<TKey>
		{
		#region Con-/Destructors
		public Map() {}
		public Map(Map<TKey, TValue> copy)
			{
			CopyFrom(copy);
			}
		#endregion

		#region Common
		public TValue this[TKey key]
			{
			get { return Get(key); }
			set { Set(key, value); }
			}
		public MapItem<TKey, TValue> First
			{
			get
				{
				lock(Mutex)
					{
					if(Root==null)
						throw new IndexOutOfRangeException();
					return new MapItem<TKey, TValue>(this, Root.First);
					}
				}
			}
		internal new IMapGroup<TKey, TValue> Root
			{
			get { return base.Root as IMapGroup<TKey, TValue>; }
			set { base.Root=value; }
			}
		#endregion

		#region Access
		public bool Contains(TKey key)
			{
			lock(Mutex)
				{
				if(Root==null)
					return false;
				TValue found=default;
				return Root.TryGet(key, ref found);
				}
			}
		public TValue Get(TKey key)
			{
			lock(Mutex)
				{
				TValue found=default;
				if(Root==null||!Root.TryGet(key, ref found))
					throw new KeyNotFoundException();
				return found;
				}
			}
		public bool TryGet(TKey key, ref TValue value)
			{
			lock(Mutex)
				{
				if(Root==null)
					return false;
				return Root.TryGet(key, ref value);
				}
			}
		#endregion

		#region Modification
		public bool Add(TKey key, TValue value)
			{
			lock(Mutex)
				{
				if(Root==null)
					Root=new MapItemGroup<TKey, TValue>();
				bool exists=false;
				if(Root.Add(key, value, false, ref exists))
					return true;
				if(exists)
					return false;
				Root=new MapParentGroup<TKey, TValue>(Root);
				return Root.Add(key, value, true, ref exists);
				}
			}
		public void CopyFrom(Map<TKey, TValue> copy)
			{
			lock(Mutex)
				{
				Root=null;
				var root=copy.Root;
				if(root==null)
					return;
				if(root.Level>0)
					{
					Root=new MapParentGroup<TKey, TValue>(root as MapParentGroup<TKey, TValue>);
					}
				else
					{
					Root=new MapItemGroup<TKey, TValue>(root as MapItemGroup<TKey, TValue>);
					}
				}
			}
		public bool Remove(TKey key)
			{
			lock(Mutex)
				{
				if(Root==null)
					return false;
				if(Root.Remove(key))
					{
					UpdateRoot();
					return true;
					}
				return false;
				}
			}
		public void Set(TKey key, TValue value)
			{
			lock(Mutex)
				{
				if(Root==null)
					Root=new MapItemGroup<TKey, TValue>();
				bool exists=false;
				if(Root.Set(key, value, false, ref exists))
					return;
				Root=new MapParentGroup<TKey, TValue>(Root);
				Root.Set(key, value, true, ref exists);
				}
			}
		#endregion

		#region Enumeration
		public MapEnumerator<TKey, TValue> At(uint pos)
			{
			var it=new MapEnumerator<TKey, TValue>(this);
			it.SetPosition(pos);
			return it;
			}
		public MapEnumerator<TKey, TValue> Find(TKey key, FindFunc func=FindFunc.Any)
			{
			bool exists=false;
			return Find(key, func, ref exists);
			}
		public MapEnumerator<TKey, TValue> Find(TKey key, FindFunc func, ref bool exists)
			{
			var it=new MapEnumerator<TKey, TValue>(this);
			it.Find(key, func, ref exists);
			return it;
			}
		IEnumerator IEnumerable.GetEnumerator()
			{
			return new MapEnumerator<TKey, TValue>(this);
			}
		public virtual IEnumerator<MapItem<TKey, TValue>> GetEnumerator()
			{
			return new MapEnumerator<TKey, TValue>(this);
			}
		public MapEnumerator<TKey, TValue> Last()
			{
			var it=new MapEnumerator<TKey, TValue>(this);
			it.SetPosition(uint.MaxValue);
			return it;
			}
		#endregion
		}
	}