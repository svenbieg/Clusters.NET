//====================
// ListParentGroup.cs
//====================

// Copyright 2024, Sven Bieg (svenbieg@web.de)
// http://github.com/svenbieg/Clusters.NET

namespace Clusters
	{
	internal class ListParentGroup<T>: ClusterParentGroup<T>, IListGroup<T>
		{
		#region Con-Destructors
		internal ListParentGroup(int level): base(level) {}
		internal ListParentGroup(IListGroup<T> child): base(child) {}
		internal ListParentGroup(ListParentGroup<T> copy): base(copy) {}
		#endregion

		#region Common
		public T First
			{
			get
				{
				var first_child=Children[0] as IListGroup<T>;
				return first_child.First;
				}
			}
		#endregion

		#region Access
		private ushort GetInsertPos(ref uint pos, ref ushort group)
			{
			for(ushort u=0; u<_ChildCount; u++)
				{
				var item_count=Children[u].ItemCount;
				if(pos<=item_count)
					{
					group=u;
					if(pos==item_count&&u+1<_ChildCount)
						return 2;
					return 1;
					}
				pos-=item_count;
				}
			return 0;
			}
		#endregion

		#region Modification
		public bool Append(T item, bool again)
			{
			var pos=_ChildCount-1;
			var child=Children[pos] as IListGroup<T>;
			if(!again)
				{
				if(child.Append(item, false))
					{
					_ItemCount++;
					return true;
					}
				int space=0;
				if(GetNearestSpace(ref space, pos))
					{
					MoveEmptySlot(space, pos);
					child.Append(item, false);
					_ItemCount++;
					return true;
					}
				}
			if(!SplitChild(pos))
				return false;
			child=Children[pos+1] as IListGroup<T>;
			if(!child.Append(item, true))
				return false;
			_ItemCount++;
			return true;
			}
		public bool InsertAt(uint position, T item, bool again)
			{
			uint pos=position;
			ushort group=0;
			ushort count=GetInsertPos(ref pos, ref group);
			if(count==0)
				return false;
			IListGroup<T> child;
			if(!again)
				{
				uint at=pos;
				for(ushort u=0; u<count; u++)
					{
					child=Children[group+u] as IListGroup<T>;
					if(child.InsertAt(at, item, false))
						{
						_ItemCount++;
						return true;
						}
					at=0;
					}
				if(ShiftChildren(group, count))
					{
					pos=position;
					count=GetInsertPos(ref pos, ref group);
					at=pos;
					for(ushort u=0; u<count; u++)
						{
						child=Children[group+u] as IListGroup<T>;
						if(child.InsertAt(at, item, false))
							{
							_ItemCount++;
							return true;
							}
						at=0;
						}
					}
				}
			if(!SplitChild(group))
				return false;
			uint item_count=Children[group].ItemCount;
			if(pos>=item_count)
				{
				group++;
				pos-=item_count;
				}
			child=Children[group] as IListGroup<T>;
			child.InsertAt(pos, item, true);
			_ItemCount++;
			return true;
			}
		private bool SplitChild(int pos)
			{
			if(_ChildCount==GroupSize)
				return false;
			IListGroup<T> group;
			if(_Level>1)
				{
				group=new ListParentGroup<T>(_Level-1);
				}
			else
				{
				group=new ListItemGroup<T>();
				}
			InsertGroup(pos+1, group);
			MoveChildren(pos, pos+1, 1);
			return true;
			}
		#endregion
		}
	}
