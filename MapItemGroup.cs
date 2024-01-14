//=================
// MapItemGroup.cs
//=================

// Copyright 2024, Sven Bieg (svenbieg@web.de)
// http://github.com/svenbieg/Clusters.NET

using System;

namespace Clusters
	{
	internal class MapItemGroup<TKey, TValue>:
		ClusterItemGroup<MapEntry<TKey, TValue>>, IMapGroup<TKey, TValue>
		where TKey: IComparable<TKey>
		{
		#region Con-/Destructors
		internal MapItemGroup() {}
		internal MapItemGroup(MapItemGroup<TKey, TValue> copy): base(copy) {}
		#endregion

		#region Common
		public MapEntry<TKey, TValue> First { get { return Items[0]; } }
		public virtual MapEntry<TKey, TValue> Last { get { return Items[_ItemCount-1]; } }
		#endregion

		#region Access
		public bool Find(TKey key, FindFunc func, ref ushort pos, ref bool exists)
			{
			pos=GetItemPos(key, ref exists);
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
		private ushort GetItemPos(TKey key, ref bool exists)
			{
			if(_ItemCount==0)
				return 0;
			ushort start=0;
			ushort end=_ItemCount;
			while(start<end)
				{
				ushort pos=(ushort)(start+(end-start)/2);
				if(Items[pos].Key.CompareTo(key)>0)
					{
					end=pos;
					continue;
					}
				if(Items[pos].Key.CompareTo(key)<0)
					{
					start=(ushort)(pos+1);
					continue;
					}
				exists=true;
				return pos;
				}
			return start;
			}
		public bool TryGet(TKey item, ref TValue value)
			{
			bool exists=false;
			var pos=GetItemPos(item, ref exists);
			if(exists)
				value=Items[pos].Value;
			return exists;
			}
		#endregion

		#region Modification
		public bool Add(TKey key, TValue value, bool again, ref bool exists)
			{
			var pos=GetItemPos(key, ref exists);
			if(exists)
				return false;
			return InsertItem(pos, key, value);
			}
		internal bool InsertItem(ushort at, TKey key, TValue value)
			{
			if(_ItemCount==GroupSize)
				return false;
			for(int i=_ItemCount; i>=at+1; i--)
				Items[i]=Items[i-1];
			Items[at].Key=key;
			Items[at].Value=value;
			_ItemCount++;
			return true;
			}
		public bool Remove(TKey key)
			{
			bool exists=false;
			var pos=GetItemPos(key, ref exists);
			if(!exists)
				return false;
			RemoveAt(pos);
			return true;
			}
		public bool Set(TKey key, TValue value, bool again, ref bool exists)
			{
			var pos=GetItemPos(key, ref exists);
			if(exists)
				{
				Items[pos].Value=value;
				return true;
				}
			if(_ItemCount==GroupSize)
				return false;
			return InsertItem(pos, key, value);
			}
		#endregion
		}
	}
