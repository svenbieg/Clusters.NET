//============
// MapItem.cs
//============

// Copyright 2024, Sven Bieg (svenbieg@web.de)
// http://github.com/svenbieg/Clusters.NET

using System;

namespace Clusters
	{
	public class MapItem<TKey, TValue>:
		IComparable<MapItem<TKey, TValue>>
		where TKey: class, IComparable<TKey>
		where TValue: class
		{
		#region Con-/Destructors
		internal MapItem(TKey key, TValue value)
			{
			_Key=key;
			_Value=value;
			}
		#endregion

		#region Common
		public TKey Key { get { return _Key; } }
		private TKey _Key;
		public TValue Value
			{
			get { return _Value; }
			set { _Value=value; }
			}
		private TValue _Value;
		#endregion

		#region IComparable
		public int CompareTo(MapItem<TKey, TValue> other)
			{
			return _Key.CompareTo(other._Key);
			}
		#endregion
		}
	}
