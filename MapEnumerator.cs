//==================
// MapEnumerator.cs
//==================

// Copyright 2024, Sven Bieg (svenbieg@web.de)
// http://github.com/svenbieg/Clusters.NET

using System.Collections;

namespace Clusters;

public class MapEnumerator<TKey, TValue>:
	ClusterEnumerator<MapEntry<TKey, TValue>>, IEnumerator<MapItem<TKey, TValue>>
	where TKey: IComparable<TKey>
	{
	#region Con-/Destructors
	internal MapEnumerator(Map<TKey, TValue> map): base(map) {}
	#endregion

	#region Common
	Object IEnumerator.Current { get { return Current; } }
	public MapItem<TKey, TValue> Current
		{
		get
			{
			if(!_HasCurrent)
				throw new IndexOutOfRangeException();
			var level=Pointers.Length-1;
			var group=Pointers[level].Group as MapItemGroup<TKey, TValue>;
			var pos=Pointers[level].Position;
			return new MapItem<TKey, TValue>(group, pos);
			}
		}
	public bool Find(TKey key, FindFunc func=FindFunc.Any)
		{
		bool exists=false;
		return Find(key, func, ref exists);
		}
	public bool Find(TKey key, FindFunc func, ref bool exists)
		{
		Reset();
 			var group=Cluster.Root as IMapGroup<TKey, TValue>;
		if(group==null)
			return false;
		ushort pos=0;
		for(int i=0; i<Pointers.Length; i++)
			{
			Pointers[i].Group=group;
			if(!group.Find(key, func, ref pos, ref exists))
				return false;
			Pointers[i].Position=pos;
			var parent=group as MapParentGroup<TKey, TValue>;
			if(parent==null)
				break;
			group=parent.Children[pos] as IMapGroup<TKey, TValue>;
			}
		_HasCurrent=true;
		return true;
		}
	#endregion
	}

