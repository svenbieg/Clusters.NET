//=====================
// IndexParentGroup.cs
//=====================

// Copyright 2024, Sven Bieg (svenbieg@web.de)
// http://github.com/svenbieg/Clusters.NET

namespace Clusters;

internal class IndexParentGroup<T>:
	ClusterParentGroup<T>, IIndexGroup<T>
	where T: IComparable<T>
	{
	#region Con-Destructors
	public IndexParentGroup(ushort level): base(level) {}
	public IndexParentGroup(IIndexGroup<T> child): base(child)
		{
		_First=child.First;
		_Last=child.Last;
		}
	public IndexParentGroup(IndexParentGroup<T> copy): base(copy)
		{
		UpdateBounds();
		}
	#endregion

	#region Common
	public T First { get { return _First; } }
	private T _First;
	public T Last { get { return _Last; } }
	private T _Last;
	#endregion

	#region Access
	public bool Find(T item, FindFunc func, ref ushort pos, ref bool exists)
		{
		ushort count=GetItemPos(item, ref pos, false);
		if(count==0)
			return false;
		if(count==1)
			{
			switch(func)
				{
				case FindFunc.Above:
					{
					var child=Children[pos] as IIndexGroup<T>;
					if(child.Last.CompareTo(item)==0)
						{
						if(pos+1>=_ChildCount)
							return false;
						pos++;
						}
					break;
					}
				case FindFunc.Below:
					{
					var child=Children[pos] as IIndexGroup<T>;
					if(child.First.CompareTo(item)==0)
						{
						if(pos==0)
							return false;
						pos--;
						}
					break;
					}
				default:
					break;
				}
			}
		else if(count==2)
			{
			switch(func)
				{
				case FindFunc.Above:
				case FindFunc.AboveOrEqual:
					{
					pos++;
					break;
					}
				case FindFunc.Equal:
					return false;
				default:
					break;
				}
			}
		return true;
		}
	private ushort GetItemPos(T item, ref ushort group, bool must_exist)
		{
		ushort start=0;
		ushort end=_ChildCount;
		while(start<end)
			{
			ushort pos=(ushort)(start+(end-start)/2);
			var child=Children[pos] as IIndexGroup<T>;
			if(child.First.CompareTo(item)>0)
				{
				end=pos;
				continue;
				}
			if(child.Last.CompareTo(item)<0)
				{
				start=(ushort)(pos+1);
				continue;
				}
			group=pos;
			return 1;
			}
		if(must_exist)
			return 0;
		if(start>=_ChildCount)
			start=(ushort)(_ChildCount-1);
		group=start;
		if(start>0)
			{
			var child=Children[start] as IIndexGroup<T>;
			if(child.First.CompareTo(item)>0)
				{
				group=(ushort)(start-1);
				return 2;
				}
			}
		if(start+1<_ChildCount)
			{
			var child=Children[start] as IIndexGroup<T>;
			if(child.Last.CompareTo(item)<0)
				return 2;
			}
		return 1;
		}
	public bool TryGet(T item, ref T found)
		{
		ushort pos=0;
		ushort count=GetItemPos(item, ref pos, true);
		if(count!=1)
			return false;
		var child=Children[pos] as IIndexGroup<T>;
		return child.TryGet(item, ref found);
		}
	#endregion

	#region Modification
	public virtual bool Add(T item, bool again, ref bool exists)
		{
		if(AddInternal(item, again, ref exists))
			{
			_ItemCount++;
			UpdateBounds();
			return true;
			}
		return false;
		}
	private bool AddInternal(T item, bool again, ref bool exists)
		{
		ushort group=0;
		ushort count=GetItemPos(item, ref group, false);
		if(!again)
			{
			for(ushort u=0; u<count; u++)
				{
				var child=Children[group+u] as IIndexGroup<T>;
				if(child.Add(item, false, ref exists))
					return true;
				if(exists)
					return false;
				}
			if(ShiftChildren(group, count))
				{
				count=GetItemPos(item, ref group, false);
				for(ushort u=0; u<count; u++)
					{
					var child=Children[group+u] as IIndexGroup<T>;
					if(child.Add(item, false, ref exists))
						return true;
					}
				}
			}
		if(!SplitChild(group))
			return false;
		count=GetItemPos(item, ref group, false);
		for(ushort u=0; u<count; u++)
			{
			var child=Children[group+u] as IIndexGroup<T>;
			if(child.Add(item, true, ref exists))
				return true;
			}
		return false;
		}
	protected override void AppendGroups(IClusterGroup<T>[] groups, int pos, ushort count)
		{
		base.AppendGroups(groups, pos, count);
		UpdateBounds();
		}
	protected override void InsertGroups(int at, IClusterGroup<T>[] groups, int pos, ushort count)
		{
		base.InsertGroups(at, groups, pos, count);
		UpdateBounds();
		}
	public bool Remove(T item, ref T removed)
		{
		ushort pos=0;
		if(GetItemPos(item, ref pos, true)==0)
			return false;
		var child=Children[pos] as IIndexGroup<T>;
		if(!child.Remove(item, ref removed))
			return false;
		_ItemCount--;
		CombineChildren(pos);
		UpdateBounds();
		return true;
		}
	public override T RemoveAt(uint pos)
		{
		T item=base.RemoveAt(pos);
		UpdateBounds();
		return item;
		}
	protected override void RemoveGroups(int pos, ushort count)
		{
		base.RemoveGroups(pos, count);
		UpdateBounds();
		}
	public bool Set(T item, bool again, ref bool exists)
		{
		if(SetInternal(item, again, ref exists))
			{
			if(!exists)
				{
				_ItemCount++;
				UpdateBounds();
				}
			return true;
			}
		return false;
		}
	private bool SetInternal(T item, bool again, ref bool exists)
		{
		ushort pos=0;
		var count=GetItemPos(item, ref pos, true);
		if(count>0)
			{
			var child=Children[pos] as IIndexGroup<T>;
			if(child.Set(item, again, ref exists))
				return true;
			}
		return AddInternal(item, again, ref exists);
		}
	private bool SplitChild(int pos)
		{
		if(_ChildCount==GroupSize)
			return false;
		IIndexGroup<T> group;
		if(_Level>1)
			{
			group=new IndexParentGroup<T>((ushort)(_Level-1));
			}
		else
			{
			group=new IndexItemGroup<T>();
			}
		InsertGroup(pos+1, group);
		MoveChildren(pos, pos+1, 1);
		return true;
		}
	private void UpdateBounds()
		{
		if(_ChildCount>0)
			{
			var first_child=Children[0] as IIndexGroup<T>;
			var last_child=Children[_ChildCount-1] as IIndexGroup<T>;
			_First=first_child.First;
			_Last=last_child.Last;
			}
		}
	#endregion
	}

