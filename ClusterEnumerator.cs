//======================
// ClusterEnumerator.cs
//======================

// Copyright 2024, Sven Bieg (svenbieg@web.de)
// http://github.com/svenbieg/Clusters.NET

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Clusters
	{
	public class ClusterEnumerator<T>: IEnumerator<T> where T: class
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
		internal ClusterEnumerator(Cluster<T> cluster, uint pos): this(cluster)
			{
			if(!SetPosition(pos))
				throw new IndexOutOfRangeException();
			}
		public void Dispose()
			{
			if(Cluster==null)
				return;
			Monitor.Exit(Cluster.Mutex);
			Cluster=null;
			Pointers=null;
			}
		#endregion

		#region Properties
		Object IEnumerator.Current
			{
			get
				{
				if(_Current==null)
					throw new IndexOutOfRangeException();
				return _Current;
				}
			}
		public virtual T Current
			{
			get
				{
				if(_Current==null)
					throw new IndexOutOfRangeException();
				return _Current;
				}
			}
		private T _Current;
		public bool HasCurrent
			{
			get
				{
				return _Current!=null;
				}
			}
		#endregion

		#region Navigation
		private ushort GetGroup(IClusterGroup<T> group, ref uint pos)
			{
			ushort count=group.ChildCount;
			ushort level=group.Level;
			if(level==0)
				{
				ushort u=(ushort)pos;
				pos=0;
				return u;
				}
			var parent_group=group as ClusterParentGroup<T>;
			for(ushort u=0; u<count; u++)
				{
				var child=parent_group.Children[u];
				if(pos<child.ItemCount)
					return u;
				pos-=child.ItemCount;
				}
			return ushort.MaxValue;
			}
		public uint GetPosition()
			{
			var ptr=Pointers[Pointers.Length-1];
			if(ptr.Group==null)
				throw new IndexOutOfRangeException();
			uint pos=ptr.Position;
			for(int i=0; i<Pointers.Length-1; i++)
				{
				ptr=Pointers[i];
				var group=ptr.Group as ClusterParentGroup<T>;
				if(group==null)
					throw new NullReferenceException();
				for(int j=0; j<ptr.Position; j++)
					pos+=group.Children[j].ItemCount;
				}
			return pos;
			}
		public virtual bool MoveNext()
			{
			var ptr=Pointers[Pointers.Length-1];
			var group=ptr.Group;
			if(group==null)
				return SetPosition(0);
			int count=group.ChildCount;
			if(ptr.Position+1<count)
				{
				ptr.Position++;
				_Current=group.GetAt(ptr.Position);
				return true;
				}
			for(int i=Pointers.Length-1; i>0; i--)
				{
				ptr=Pointers[i-1];
				group=ptr.Group;
				if(group==null)
					throw new NullReferenceException();
				count=group.ChildCount;
				if(ptr.Position+1>=count)
					continue;
				ptr.Position++;
				for(; i<Pointers.Length; i++)
					{
					var parent_group=group as ClusterParentGroup<T>;
					if(parent_group==null)
						throw new NullReferenceException();
					group=parent_group.Children[ptr.Position];
					ptr=Pointers[i];
					ptr.Group=group;
					ptr.Position=0;
					}
				_Current=group.GetAt(0);
				return true;
				}
			ptr=Pointers[Pointers.Length-1];
			ptr.Group=null;
			return false;
			}
		public bool MovePrevious()
			{
			var ptr=Pointers[Pointers.Length-1];
			var group=ptr.Group;
			if(group==null)
				return false;
			if(ptr.Position>0)
				{
				ptr.Position--;
				_Current=group.GetAt(ptr.Position);
				return true;
				}
			for(int i=Pointers.Length-1; i>0; i--)
				{
				ptr=Pointers[i-1];
				if(ptr.Position==0)
					continue;
				group=ptr.Group;
				if(group==null)
					throw new NullReferenceException();
				ptr.Position--;
				ushort last=0;
				for(; i<Pointers.Length; i++)
					{
					var parent_group=group as ClusterParentGroup<T>;
					if(parent_group==null)
						throw new NullReferenceException();
					group=parent_group.Children[ptr.Position];
					last=(ushort)(group.ChildCount-1);
					ptr=Pointers[i];
					ptr.Group=group;
					ptr.Position=last;
					}
				_Current=group.GetAt(last);
				return true;
				}
			ptr=Pointers[Pointers.Length-1];
			ptr.Group=null;
			return false;
			}
		public uint Position
			{
			get => GetPosition();
			set
				{
				if(!SetPosition(value))
					throw new IndexOutOfRangeException();
				}
			}
		public virtual void Reset()
			{
			Position=0;
			}
		public bool SetPosition(uint pos)
			{
			Pointers[Pointers.Length-1].Group=null;
 			var group=Cluster.Root;
			if(group==null)
				return false;
			if(pos==uint.MaxValue)
				pos=group.ItemCount-1;
			var group_pos=GetGroup(group, ref pos);
			if(group_pos==ushort.MaxValue)
				return false;
			var ptr=Pointers[0];
			ptr.Group=group;
			ptr.Position=group_pos;
			for(int i=0; i<Pointers.Length-1; i++)
				{
				var parent_group=group as ClusterParentGroup<T>;
				group=parent_group.Children[group_pos];
				group_pos=GetGroup(group, ref pos);
				if(group_pos<0)
					return false;
				ptr=Pointers[i+1];
				ptr.Group=group;
				ptr.Position=group_pos;
				}
			if(group_pos>=group.ItemCount)
				{
				Pointers[Pointers.Length-1].Group=null;
				return false;
				}
			_Current=group.GetAt(group_pos);
			return true;
			}
		#endregion

		#region Modification
		public void RemoveCurrent()
			{
			var ptr=Pointers[Pointers.Length-1];
			if(ptr.Group==null)
				throw new IndexOutOfRangeException();
			var pos=GetPosition();
			var root=Cluster.Root;
			if(root==null)
				throw new IndexOutOfRangeException();
			root.RemoveAt(pos);
			SetPosition(pos);
		}
		#endregion

		#region Members
		Cluster<T> Cluster;
		ClusterPointer<T>[] Pointers;
		#endregion
		}
	}
