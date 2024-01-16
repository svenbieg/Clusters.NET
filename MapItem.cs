//============
// MapItem.cs
//============

// Copyright 2024, Sven Bieg (svenbieg@web.de)
// http://github.com/svenbieg/Clusters.NET

namespace Clusters;

public struct MapEntry<TKey, TValue>
	{
	internal TKey Key;
	internal TValue Value;
	}
public class MapItem<TKey, TValue> where TKey: IComparable<TKey>
	{
	#region Con-/Destructors
	internal MapItem(Map<TKey, TValue> map, TKey key)
		{
		Map=map;
		_Key=key;
		_Value=map.Get(key);
		}
	internal MapItem(MapItemGroup<TKey, TValue> group, ushort pos)
		{
		Group=group;
		Position=pos;
		_Key=Group.Items[Position].Key;
		_Value=Group.Items[Position].Value;
		}
	#endregion

	#region Common
	private MapItemGroup<TKey, TValue> Group;
	public TKey Key { get { return _Key; } }
	private TKey _Key;
	private Map<TKey, TValue> Map;
	private ushort Position=0;
	public TValue Value
		{
		get { return _Value; }
		set
			{
			if(Group!=null)
				{
				Group.Items[Position].Value=value;
				}
			else
				{
				Map.Set(_Key, value);
				}
			_Value=value;
			}
		}
	private TValue _Value;
	#endregion
	}

