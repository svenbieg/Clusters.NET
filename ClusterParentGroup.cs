//=======================
// ClusterParentGroup.cs
//=======================

// Copyright 2024, Sven Bieg (svenbieg@web.de)
// http://github.com/svenbieg/Clusters.NET

namespace Clusters
	{
	internal class ClusterParentGroup<T>: IClusterGroup<T>
		{
		#region Con-/Destructors
		internal ClusterParentGroup(int level)
			{
			Children=new IClusterGroup<T>[GroupSize];
			_ChildCount=0;
			_ItemCount=0;
			_Level=(ushort)level;
			}
		internal ClusterParentGroup(IClusterGroup<T> child)
			{
			Children=new IClusterGroup<T>[GroupSize];
			Children[0]=child;
			_ChildCount=1;
			_ItemCount=child.ItemCount;
			_Level=(ushort)(child.Level+1);
			}
		internal ClusterParentGroup(ClusterParentGroup<T> copy)
			{
			_ChildCount=copy._ChildCount;
			_ItemCount=copy._ItemCount;
			_Level=copy._Level;
			Children=new IClusterGroup<T>[GroupSize];
			if(_Level>1)
				{
				for(int i=0; i<copy.ChildCount; i++)
					{
					var child=copy.Children[i] as ClusterParentGroup<T>;
					Children[i]=new ClusterParentGroup<T>(child);
					}
				}
			else
				{
				for(int i=0; i<copy.ChildCount; i++)
					{
					var child=copy.Children[i] as ClusterItemGroup<T>;
					Children[i]=new ClusterItemGroup<T>(child);
					}
				}
			}
		#endregion

		#region Properties
		public ushort ChildCount { get { return _ChildCount; } }
		public uint ItemCount { get { return _ItemCount; } }
		public ushort Level { get { return _Level; } }
		#endregion

		#region Access
		public T GetAt(uint pos)
			{
			ushort group=GetGroup(ref pos);
			return Children[group].GetAt(pos);
			}
		internal ushort GetGroup(ref uint pos)
			{
			ushort group=0;
			foreach(var child in Children)
				{
				if(pos<child.ItemCount)
					return group;
				pos-=child.ItemCount;
				group++;
				}
			return ushort.MaxValue;
			}
		protected bool GetNearestSpace(ref int space, int pos)
			{
			int before=pos-1;
			int after=pos+1;
			while(before>=0||after<_ChildCount)
				{
				if(before>=0)
					{
					if(Children[before].ChildCount<GroupSize)
						{
						space=before;
						return true;
						}
					before--;
					}
				if(after<_ChildCount)
					{
					if(Children[after].ChildCount<GroupSize)
						{
						space=after;
						return true;
						}
					after++;
					}
				}
			return false;
			}
		#endregion

		#region Modification
		protected virtual void AppendGroups(IClusterGroup<T>[] groups, int pos, ushort count)
			{
			for(int i=0; i<count; i++)
				{
				Children[_ChildCount+i]=groups[pos+i];
				_ItemCount+=groups[pos+i].ItemCount;
				}
			_ChildCount+=count;
			}
		protected virtual void InsertGroup(int at, IClusterGroup<T> group)
			{
			_ItemCount+=group.ItemCount;
			for(int i=_ChildCount; i>=at+1; i--)
				Children[i]=Children[i-1];
			Children[at]=group;
			_ChildCount++;
			}
		protected virtual void InsertGroups(int at, IClusterGroup<T>[] groups, int pos, ushort count)
			{
			for(int i=0; i<count; i++)
				_ItemCount+=groups[pos+i].ItemCount;
			for(int i=_ChildCount+count-1; i>=at+count; i--)
				Children[i]=Children[i-count];
			for(int i=0; i<count; i++)
				Children[at+i]=groups[pos+i];
			_ChildCount+=count;
			}
		protected bool CombineChildren(int pos)
			{
			ushort current=Children[pos].ChildCount;
			if(current==0)
				{
				RemoveGroup(pos);
				return true;
				}
			if(pos>0)
				{
				ushort before=Children[pos-1].ChildCount;
				if(current+before<=GroupSize)
					{
					MoveChildren(pos, pos-1, current);
					RemoveGroup(pos);
					return true;
					}
				}
			if(pos+1<_ChildCount)
				{
				ushort after=Children[pos+1].ChildCount;
				if(current+after<=GroupSize)
					{
					MoveChildren(pos+1, pos, after);
					RemoveGroup(pos+1);
					return true;
					}
				}
			return false;
			}
		protected void MoveChildren(int from, int to, ushort count)
			{
			if(_Level>1)
				{
				var src=Children[from] as ClusterParentGroup<T>;
				var dst=Children[to] as ClusterParentGroup<T>;
				if(from>to)
					{
					dst.AppendGroups(src.Children, 0, count);
					src.RemoveGroups(0, count);
					}
				else
					{
					ushort pos=(ushort)(src.ChildCount-count);
					dst.InsertGroups(0, src.Children, pos, count);
					src.RemoveGroups(pos, count);
					}
				}
			else
				{
				var src=Children[from] as ClusterItemGroup<T>;
				var dst=Children[to] as ClusterItemGroup<T>;
				if(from>to)
					{
					dst.AppendItems(src.Items, 0, count);
					src.RemoveItems(0, count);
					}
				else
					{
					ushort pos=(ushort)(src.ChildCount-count);
					dst.InsertItems(0, src.Items, pos, count);
					src.RemoveItems(pos, count);
					}
				}
			}
		protected void MoveEmptySlot(int from, int to)
			{
			if(from<to)
				{
				for(int pos=from; pos<to; pos++)
					MoveChildren(pos+1, pos, 1);
				}
			else
				{
				for(int pos=from; pos>to; pos--)
					MoveChildren(pos-1, pos, 1);
				}
			}
		public virtual T RemoveAt(uint pos)
			{
			ushort group=GetGroup(ref pos);
			var item=Children[group].RemoveAt(pos);
			_ItemCount--;
			CombineChildren(group);
			return item;
			}
		protected void RemoveGroup(int pos)
			{
			_ItemCount-=Children[pos].ItemCount;
			for(int i=pos; i+1<_ChildCount; i++)
				Children[i]=Children[i+1];
			Children[_ChildCount-1]=null;
			_ChildCount--;
			}
		protected virtual void RemoveGroups(int pos, ushort count)
			{
			for(int i=0; i<count; i++)
				_ItemCount-=Children[pos+i].ItemCount;
			for(int i=pos; i+count<_ChildCount; i++)
				Children[i]=Children[i+count];
			for(int i=0; i<count; i++)
				Children[_ChildCount-i-1]=null;
			_ChildCount-=count;
			}
		public void SetAt(uint pos, T item)
			{
			ushort group=GetGroup(ref pos);
			Children[group].SetAt(pos, item);
			}
		protected bool ShiftChildren(int pos, ushort count)
			{
			int space=0;
			if(!GetNearestSpace(ref space, pos))
				return false;
			if(count>1&&space>pos)
				pos++;
			MoveEmptySlot(space, pos);
			return true;
			}
		#endregion

		#region Members
		internal IClusterGroup<T>[] Children;
		protected ushort _ChildCount;
		internal const ushort GroupSize=Cluster<T>.GroupSize;
		protected uint _ItemCount;
		protected ushort _Level;
		#endregion
		}
	}
