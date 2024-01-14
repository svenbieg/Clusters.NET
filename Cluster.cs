//============
// Cluster.cs
//============

// Copyright 2024, Sven Bieg (svenbieg@web.de)
// http://github.com/svenbieg/Clusters.NET

namespace Clusters;

public abstract class Cluster<T>
	{
	#region Common
	internal const ushort GroupSize=10;
	public uint Length
		{
		get
			{
			lock(Mutex)
				{
				if(Root==null)
					return 0;
				return Root.ItemCount;
				}
			}
		}
	internal Object Mutex=new Object();
	internal IClusterGroup<T> Root;
	#endregion

	#region Access
	public T GetAt(uint pos)
		{
		lock(Mutex)
			{
			if(Root==null||pos>=Root.ItemCount)
				throw new IndexOutOfRangeException();
			return Root.GetAt(pos);
			}
		}
	#endregion

	#region Modification
	public void Clear()
		{
		lock(Mutex)
			{
			Root=null;
			}
		}
	public void RemoveAt(uint pos)
		{
		lock(Mutex)
			{
			if(Root==null||pos>=Root.ItemCount)
				throw new IndexOutOfRangeException();
			Root.RemoveAt(pos);
			UpdateRoot();
			}
		}
	internal void UpdateRoot()
		{
		if(Root==null)
			return;
		if(Root.Level==0)
			{
			if(Root.ChildCount==0)
				Root=null;
			return;
			}
		if(Root.ChildCount>1)
			return;
		var root=Root as ClusterParentGroup<T>;
		Root=root.Children[0];
		}
	#endregion
	}

