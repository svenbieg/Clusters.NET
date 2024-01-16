//==============
// IMapGroup.cs
//==============

// Copyright 2024, Sven Bieg (svenbieg@web.de)
// http://github.com/svenbieg/Clusters.NET

namespace Clusters;

internal interface IMapGroup<TKey, TValue>: IClusterGroup<MapEntry<TKey, TValue>>
	where TKey: IComparable<TKey>
	{
	#region Common
	TKey First { get; }
	TKey Last { get; }
	#endregion

	#region Access
	bool Find(TKey key, FindFunc func, ref ushort pos, ref bool exists, IComparer<TKey> comparer);
	bool TryGet(TKey key, ref TValue value, IComparer<TKey> comparer);
	#endregion

	#region Modification
	bool Add(TKey key, TValue value, bool again, ref bool exists, IComparer<TKey> comparer);
	bool Remove(TKey key, IComparer<TKey> comparer);
	bool Set(TKey key, TValue value, bool again, ref bool exists, IComparer<TKey> comparer);
	#endregion
	}
