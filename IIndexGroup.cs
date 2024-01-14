//================
// IIndexGroup.cs
//================

// Copyright 2024, Sven Bieg (svenbieg@web.de)
// http://github.com/svenbieg/Clusters.NET

namespace Clusters;

public enum FindFunc
	{
	Above,
	AboveOrEqual,
	Any,
	Below,
	BelowOrEqual,
	Equal
	}

internal interface IIndexGroup<T>: IClusterGroup<T>
	where T: IComparable<T>
	{
	#region Common
	T First { get; }
	T Last { get; }
	#endregion

	#region Access
	bool Find(T item, FindFunc func, ref ushort pos, ref bool exists);
	bool TryGet(T item, ref T found);
	#endregion

	#region Modification
	bool Add(T item, bool again, ref bool exists);
	bool Remove(T item, ref T removed);
	bool Set(T item, bool again, ref bool exists);
	#endregion
	}

