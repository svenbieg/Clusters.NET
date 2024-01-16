//===============
// IListGroup.cs
//===============

// Copyright 2024, Sven Bieg (svenbieg@web.de)
// http://github.com/svenbieg/Clusters.NET

namespace Clusters;

internal interface IListGroup<T>: IClusterGroup<T>
	{
	#region Common
	T First { get; }
	T Last { get; }
	#endregion

	#region Modification
	bool Append(T item, bool again);
	bool InsertAt(uint pos, T item, bool again);
	#endregion
	}
