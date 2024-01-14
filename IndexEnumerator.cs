//====================
// IndexEnumerator.cs
//====================

// Copyright 2024, Sven Bieg (svenbieg@web.de)
// http://github.com/svenbieg/Clusters.NET

using System;
using System.Collections;
using System.Collections.Generic;

namespace Clusters
	{
	public class IndexEnumerator<T>:
		ClusterEnumerator<T>, IEnumerator<T>
		where T: IComparable<T>
		{
		#region Con-/Destructors
		internal IndexEnumerator(Index<T> index): base(index) {}
		#endregion

		#region Common
		Object IEnumerator.Current { get { return Current; } }
		public T Current
			{
			get
				{
				if(!_HasCurrent)
					throw new IndexOutOfRangeException();
				var level=Pointers.Length-1;
				var group=Pointers[level].Group as IndexItemGroup<T>;
				var pos=Pointers[level].Position;
				return group.Items[pos];
				}
			}
		public bool Find(T item, FindFunc func=FindFunc.Any)
			{
			bool exists=false;
			return Find(item, func, ref exists);
			}
		public bool Find(T item, FindFunc func, ref bool exists)
			{
			Reset();
 			var group=Cluster.Root as IIndexGroup<T>;
			if(group==null)
				return false;
			ushort pos=0;
			for(int i=0; i<Pointers.Length; i++)
				{
				Pointers[i].Group=group;
				if(!group.Find(item, func, ref pos, ref exists))
					return false;
				Pointers[i].Position=pos;
				var parent=group as IndexParentGroup<T>;
				if(parent==null)
					break;
				group=parent.Children[pos] as IIndexGroup<T>;
				}
			_HasCurrent=true;
			return true;
			}
		#endregion
		}
	}
