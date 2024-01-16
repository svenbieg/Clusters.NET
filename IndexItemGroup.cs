//===================
// IndexItemGroup.cs
//===================

// Copyright 2024, Sven Bieg (svenbieg@web.de)
// http://github.com/svenbieg/Clusters.NET

namespace Clusters;

internal class IndexItemGroup<T>:
	ClusterItemGroup<T>, IIndexGroup<T>
	where T: IComparable<T>
	{
	#region Con-/Destructors
	internal IndexItemGroup() {}
	internal IndexItemGroup(IndexItemGroup<T> copy): base(copy) {}
	#endregion

	#region Common
	public T First { get { return Items[0]; } }
	public virtual T Last { get { return Items[_ItemCount-1]; } }
	#endregion

	#region Access
	public bool Find(T item, FindFunc func, ref ushort pos, ref bool exists, IComparer<T> comparer)
		{
		pos=GetItemPos(item, ref exists, comparer);
		if(exists)
			{
			switch(func)
				{
				case FindFunc.Above:
					{
					if(pos+1>=_ItemCount)
						return false;
					pos++;
					break;
					}
				case FindFunc.Below:
					{
					if(pos==0)
						return false;
					pos--;
					break;
					}
				default:
					{
					break;
					}
				}
			return true;
			}
		switch(func)
			{
			case FindFunc.Above:
			case FindFunc.AboveOrEqual:
				{
				if(pos==_ItemCount)
					return false;
				break;
				}
			case FindFunc.Any:
				{
				if(pos>0)
					pos--;
				break;
				}
			case FindFunc.Below:
			case FindFunc.BelowOrEqual:
				{
				if(pos==0)
					return false;
				pos--;
				break;
				}
			case FindFunc.Equal:
				{
				return false;
				}
			}
		return true;
		}
	private ushort GetItemPos(T item, ref bool exists, IComparer<T> comparer)
		{
		if(_ItemCount==0)
			return 0;
		ushort start=0;
		ushort end=_ItemCount;
		while(start<end)
			{
			ushort pos=(ushort)(start+(end-start)/2);
			if(comparer.Compare(Items[pos], item)>0)
				{
				end=pos;
				continue;
				}
			if(comparer.Compare(Items[pos], item)<0)
				{
				start=(ushort)(pos+1);
				continue;
				}
			exists=true;
			return pos;
			}
		return start;
		}
	public bool TryGet(T item, ref T found, IComparer<T> comparer)
		{
		bool exists=false;
		var pos=GetItemPos(item, ref exists, comparer);
		if(exists)
			found=Items[pos];
		return exists;
		}
	#endregion

	#region Modification
	public bool Add(T item, bool again, ref bool exists, IComparer<T> comparer)
		{
		var pos=GetItemPos(item, ref exists, comparer);
		if(exists)
			return false;
		return InsertItem(pos, item);
		}
	public bool Remove(T item, ref T removed, IComparer<T> comparer)
		{
		bool exists=false;
		var pos=GetItemPos(item, ref exists, comparer);
		if(!exists)
			return false;
		removed=RemoveAt(pos);
		return true;
		}
	public bool Set(T item, bool again, ref bool exists, IComparer<T> comparer)
		{
		var pos=GetItemPos(item, ref exists, comparer);
		if(exists)
			{
			Items[pos]=item;
			return true;
			}
		return InsertItem(pos, item);
		}
	#endregion
	}

