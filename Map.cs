//========
// Map.cs
//========

// Windows.NET-Implementation of a sorted map.
// Items and can be inserted, removed and looked-up in constant low time.

// Copyright 2024, Sven Bieg (svenbieg@web.de)
// http://github.com/svenbieg/Clusters.NET

using System;
using System.Collections.Generic;

namespace Clusters
	{
	public class Map<TKey, TValue>: Index<MapItem<TKey, TValue>>
		where TKey: class, IComparable<TKey>
		where TValue: class
		{
		#region Con-/Destructors
		public Map(IComparer<TKey> comparer=default):
			base(new MapComparer<TKey, TValue>(comparer))
			{}
		public Map(Map<TKey, TValue> copy, IComparer<TKey> comparer=default):
			base(copy, new MapComparer<TKey, TValue>(comparer))
			{}
		#endregion
		}
	}
