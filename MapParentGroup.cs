﻿//===================
// MapParentGroup.cs
//===================

// Copyright 2024, Sven Bieg (svenbieg@web.de)
// http://github.com/svenbieg/Clusters.NET

namespace Clusters;

internal class MapParentGroup<TKey, TValue>:
	ClusterParentGroup<MapEntry<TKey, TValue>>, IMapGroup<TKey, TValue>
	where TKey: IComparable<TKey>
	{
	#region Con-Destructors
	public MapParentGroup(ushort level): base(level) {}
	public MapParentGroup(IMapGroup<TKey, TValue> child): base(child)
		{
		_First=child.First;
		_Last=child.Last;
		}
	public MapParentGroup(MapParentGroup<TKey, TValue> copy): base(copy)
		{
		UpdateBounds();
		}
	#endregion

	#region Common
	public TKey First { get { return _First; } }
	private TKey _First;
	public TKey Last { get { return _Last; } }
	private TKey _Last;
	#endregion

	#region Access
	public bool Find(TKey key, FindFunc func, ref ushort pos, ref bool exists, IComparer<TKey> comparer)
		{
		ushort count=GetItemPos(key, ref pos, false, comparer);
		if(count==0)
			return false;
		if(count==1)
			{
			switch(func)
				{
				case FindFunc.Above:
					{
					var child=Children[pos] as IMapGroup<TKey, TValue>;
					if(comparer.Compare(child.Last, key)==0)
						{
						if(pos+1>=_ChildCount)
							return false;
						pos++;
						}
					break;
					}
				case FindFunc.Below:
					{
					var child=Children[pos] as IMapGroup<TKey, TValue>;
					if(comparer.Compare(child.First, key)==0)
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
	private ushort GetItemPos(TKey key, ref ushort group, bool must_exist, IComparer<TKey> comparer)
		{
		ushort start=0;
		ushort end=_ChildCount;
		while(start<end)
			{
			ushort pos=(ushort)(start+(end-start)/2);
			var child=Children[pos] as IMapGroup<TKey, TValue>;
			if(comparer.Compare(child.First, key)>0)
				{
				end=pos;
				continue;
				}
			if(comparer.Compare(child.Last, key)<0)
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
			var child=Children[start] as IMapGroup<TKey, TValue>;
			if(comparer.Compare(child.First, key)>0)
				{
				group=(ushort)(start-1);
				return 2;
				}
			}
		if(start+1<_ChildCount)
			{
			var child=Children[start] as IMapGroup<TKey, TValue>;
			if(comparer.Compare(child.Last, key)<0)
				return 2;
			}
		return 1;
		}
	public bool TryGet(TKey key, ref TValue value, IComparer<TKey> comparer)
		{
		ushort pos=0;
		ushort count=GetItemPos(key, ref pos, true, comparer);
		if(count!=1)
			return false;
		var child=Children[pos] as IMapGroup<TKey, TValue>;
		return child.TryGet(key, ref value, comparer);
		}
	#endregion

	#region Modification
	public virtual bool Add(TKey key, TValue value, bool again, ref bool exists, IComparer<TKey> comparer)
		{
		if(AddInternal(key, value, again, ref exists, comparer))
			{
			_ItemCount++;
			UpdateBounds();
			return true;
			}
		return false;
		}
	private bool AddInternal(TKey key, TValue value, bool again, ref bool exists, IComparer<TKey> comparer)
		{
		ushort group=0;
		ushort count=GetItemPos(key, ref group, false, comparer);
		if(!again)
			{
			for(ushort u=0; u<count; u++)
				{
				var child=Children[group+u] as IMapGroup<TKey, TValue>;
				if(child.Add(key, value, false, ref exists, comparer))
					return true;
				if(exists)
					return false;
				}
			if(ShiftChildren(group, count))
				{
				count=GetItemPos(key, ref group, false, comparer);
				for(ushort u=0; u<count; u++)
					{
					var child=Children[group+u] as IMapGroup<TKey, TValue>;
					if(child.Add(key, value, false, ref exists, comparer))
						return true;
					}
				}
			}
		if(!SplitChild(group))
			return false;
		count=GetItemPos(key, ref group, false, comparer);
		for(ushort u=0; u<count; u++)
			{
			var child=Children[group+u] as IMapGroup<TKey, TValue>;
			if(child.Add(key, value, true, ref exists, comparer))
				return true;
			}
		return false;
		}
	protected override void AppendGroups(IClusterGroup<MapEntry<TKey, TValue>>[] groups, int pos, ushort count)
		{
		base.AppendGroups(groups, pos, count);
		UpdateBounds();
		}
	protected override void InsertGroups(int at, IClusterGroup<MapEntry<TKey, TValue>>[] groups, int pos, ushort count)
		{
		base.InsertGroups(at, groups, pos, count);
		UpdateBounds();
		}
	public bool Remove(TKey key, IComparer<TKey> comparer)
		{
		ushort pos=0;
		if(GetItemPos(key, ref pos, true, comparer)==0)
			return false;
		var child=Children[pos] as IMapGroup<TKey, TValue>;
		if(!child.Remove(key, comparer))
			return false;
		_ItemCount--;
		CombineChildren(pos);
		UpdateBounds();
		return true;
		}
	public override MapEntry<TKey, TValue> RemoveAt(uint pos)
		{
		var item=base.RemoveAt(pos);
		UpdateBounds();
		return item;
		}
	protected override void RemoveGroups(int pos, ushort count)
		{
		base.RemoveGroups(pos, count);
		UpdateBounds();
		}
	public bool Set(TKey key, TValue value, bool again, ref bool exists, IComparer<TKey> comparer)
		{
		if(SetInternal(key, value, again, ref exists, comparer))
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
	private bool SetInternal(TKey key, TValue value, bool again, ref bool exists, IComparer<TKey> comparer)
		{
		ushort pos=0;
		var count=GetItemPos(key, ref pos, true, comparer);
		if(count>0)
			{
			var child=Children[pos] as IMapGroup<TKey, TValue>;
			if(child.Set(key, value, again, ref exists, comparer))
				return true;
			}
		return AddInternal(key, value, again, ref exists, comparer);
		}
	private bool SplitChild(int pos)
		{
		if(_ChildCount==GroupSize)
			return false;
		IMapGroup<TKey, TValue> group;
		if(_Level>1)
			{
			group=new MapParentGroup<TKey, TValue>((ushort)(_Level-1));
			}
		else
			{
			group=new MapItemGroup<TKey, TValue>();
			}
		InsertGroup(pos+1, group);
		MoveChildren(pos, pos+1, 1);
		return true;
		}
	private void UpdateBounds()
		{
		if(_ChildCount>0)
			{
			var first_child=Children[0] as IMapGroup<TKey, TValue>;
			var last_child=Children[_ChildCount-1] as IMapGroup<TKey, TValue>;
			_First=first_child.First;
			_Last=last_child.Last;
			}
		}
	#endregion
	}

