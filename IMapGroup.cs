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
	MapEntry<TKey, TValue> First { get; }
	MapEntry<TKey, TValue> Last { get; }
	#endregion

	#region Access
	bool Find(TKey key, FindFunc func, ref ushort pos, ref bool exists);
	bool TryGet(TKey key, ref TValue value);
	#endregion

	#region Modification
	bool Add(TKey key, TValue value, bool again, ref bool exists);
	bool Remove(TKey key);
	bool Set(TKey key, TValue value, bool again, ref bool exists);
	#endregion
	}
