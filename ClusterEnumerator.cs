//======================
// ClusterEnumerator.cs
//======================

// Copyright 2024, Sven Bieg (svenbieg@web.de)
// http://github.com/svenbieg/Clusters.NET

using System;
using System.Threading;

namespace Clusters
	{
	public abstract class ClusterEnumerator<T>
		{
		#region Con-/Destructors
		internal ClusterEnumerator(Cluster<T> cluster)
			{
			Monitor.Enter(cluster.Mutex);
			Cluster=cluster;
			var level_count=1;
			var root=cluster.Root;
			if(root!=null)
				level_count=root.Level+1;
			Pointers=new ClusterPointer<T>[level_count];
			}
		public void Dispose()
			{
			if(Cluster==null)
				return;
			Monitor.Exit(Cluster.Mutex);
			Cluster=null;
			Pointers=null;
			_HasCurrent=false;
			}
		#endregion

		#region Common
		internal Cluster<T> Cluster;
		public bool HasCurrent { get { return _HasCurrent; } }
		internal bool _HasCurrent=false;
		internal ClusterPointer<T>[] Pointers;
		#endregion

		#region Navigation
		public uint GetPosition()
			{
			int level=Pointers.Length-1;
			if(Pointers[level].Group==null)
				throw new IndexOutOfRangeException();
			uint pos=Pointers[level].Position;
			for(int i=0; i<Pointers.Length-1; i++)
				{
				var group=Pointers[i].Group as ClusterParentGroup<T>;
				for(int j=0; j<Pointers[i].Position; j++)
					pos+=group.Children[j].ItemCount;
				}
			return pos;
			}
		public virtual bool MoveNext()
			{
			int level=Pointers.Length-1;
			var group=Pointers[level].Group;
			if(group==null)
				return SetPosition(0);
			int count=group.ChildCount;
			if(Pointers[level].Position+1<count)
				{
				Pointers[level].Position++;
				return true;
				}
			for(int i=Pointers.Length-1; i>0; i--)
				{
				group=Pointers[i-1].Group;
				count=group.ChildCount;
				if(Pointers[i-1].Position+1>=count)
					continue;
				Pointers[i-1].Position++;
				for(; i<Pointers.Length; i++)
					{
					var pos=Pointers[i-1].Position;
					var parent_group=group as ClusterParentGroup<T>;
					group=parent_group.Children[pos];
					Pointers[i].Group=group;
					Pointers[i].Position=0;
					}
				return true;
				}
			Pointers[level].Group=null;
			_HasCurrent=false;
			return false;
			}
		public bool MovePrevious()
			{
			int level=Pointers.Length-1;
			var group=Pointers[level].Group;
			if(group==null)
				return false;
			if(Pointers[level].Position>0)
				{
				Pointers[level].Position--;
				return true;
				}
			for(int i=Pointers.Length-1; i>0; i--)
				{
				if(Pointers[i-1].Position==0)
					continue;
				group=Pointers[i-1].Group;
				Pointers[i-1].Position--;
				for(; i<Pointers.Length; i++)
					{
					var pos=Pointers[i-1].Position;
					var parent_group=group as ClusterParentGroup<T>;
					group=parent_group.Children[pos];
					ushort last=(ushort)(group.ChildCount-1);
					Pointers[i].Group=group;
					Pointers[i].Position=last;
					}
				return true;
				}
			Pointers[level].Group=null;
			_HasCurrent=false;
			return false;
			}
		public uint Position
			{
			get { return GetPosition(); }
			set
				{
				if(!SetPosition(value))
					throw new IndexOutOfRangeException();
				}
			}
		public virtual void Reset()
			{
			int level=Pointers.Length-1;
			Pointers[level].Group=null;
			_HasCurrent=false;
			}
		public virtual bool SetPosition(uint pos)
			{
			Reset();
 			var group=Cluster.Root;
			if(group==null)
				return false;
			if(pos==uint.MaxValue)
				pos=group.ItemCount-1;
			var level=Pointers.Length-1;
			for(int i=0; i<level; i++)
				{
				Pointers[i].Group=group;
				var parent=group as ClusterParentGroup<T>;
				ushort group_pos=parent.GetGroup(ref pos);
				if(group_pos==ushort.MaxValue)
					return false;
				group=parent.Children[group_pos];
				}
			if(pos>=group.ChildCount)
				return false;
			Pointers[level].Group=group;
			Pointers[level].Position=(ushort)pos;
			_HasCurrent=true;
			return true;
			}
		#endregion

		#region Modification
		public void RemoveCurrent()
			{
			int level=Pointers.Length-1;
			if(Pointers[level].Group==null)
				throw new IndexOutOfRangeException();
			var pos=GetPosition();
			Cluster.Root.RemoveAt(pos);
			SetPosition(pos);
		}
		#endregion
		}
	}
