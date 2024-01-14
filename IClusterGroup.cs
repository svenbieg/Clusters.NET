//==================
// IClusterGroup.cs
//==================

// Copyright 2024, Sven Bieg (svenbieg@web.de)
// http://github.com/svenbieg/Clusters.NET

namespace Clusters;

internal interface IClusterGroup<T>
	{
	#region Properties
	ushort ChildCount { get; }
	uint ItemCount { get; }
	ushort Level { get; }
	#endregion

	#region Access
	T GetAt(uint Position);
	#endregion

	#region Modification
	T RemoveAt(uint Position);
	void SetAt(uint Position, T Item);
	#endregion
	}
