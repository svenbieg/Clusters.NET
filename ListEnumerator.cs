//===================
// ListEnumerator.cs
//===================

// Copyright 2024, Sven Bieg (svenbieg@web.de)
// http://github.com/svenbieg/Clusters.NET

using System;
using System.Collections;
using System.Collections.Generic;

namespace Clusters
	{
	public class ListEnumerator<T>: ClusterEnumerator<T>, IEnumerator<T>
		{
		#region Con-/Destructors
		internal ListEnumerator(List<T> list): base(list) {}
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
				var group=Pointers[level].Group as ListItemGroup<T>;
				var pos=Pointers[level].Position;
				return group.Items[pos];
				}
			set
				{
				if(!_HasCurrent)
					throw new IndexOutOfRangeException();
				var level=Pointers.Length-1;
				var group=Pointers[level].Group as ListItemGroup<T>;
				var pos=Pointers[level].Position;
				group.Items[pos]=value;
				}
			}
		#endregion
		}
	}
