//===================
// ClusterPointer.cs
//===================

// Copyright 2024, Sven Bieg (svenbieg@web.de)
// http://github.com/svenbieg/Clusters.NET

namespace Clusters;

internal struct ClusterPointer<T>
	{
	internal IClusterGroup<T> Group;
	internal ushort Position;
	}
