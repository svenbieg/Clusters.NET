//================
// MapComparer.cs
//================

using System;
using System.Collections.Generic;

namespace Clusters
	{
	internal class MapComparer<TKey, TValue>:
		IComparer<MapItem<TKey, TValue>>
		where TKey: class, IComparable<TKey>
		where TValue: class
		{
		#region Con-/Destructors
		internal MapComparer(IComparer<TKey> comparer)
			{
			Comparer=comparer;
			}
		#endregion

		#region Common
		public int Compare(MapItem<TKey, TValue> x, MapItem<TKey, TValue> y)
			{
			return Comparer.Compare(x.Key, y.Key);
			}
		private IComparer<TKey> Comparer;
		#endregion
		}
	}
