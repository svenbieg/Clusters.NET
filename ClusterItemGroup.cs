//=====================
// ClusterItemGroup.cs
//=====================

// Copyright 2024, Sven Bieg (svenbieg@web.de)
// http://github.com/svenbieg/Clusters.NET

namespace Clusters;

internal class ClusterItemGroup<T>: IClusterGroup<T>
	{
	#region Con-/Destructors
	internal ClusterItemGroup()
		{
		Items=new T[GroupSize];
		_ItemCount=0;
		}
	internal ClusterItemGroup(ClusterItemGroup<T> copy)
		{
		Items=new T[GroupSize];
		_ItemCount=copy._ItemCount;
		for(int i=0; i<_ItemCount; i++)
			Items[i]=copy.Items[i];
		}
	#endregion

	#region Properties
	public ushort ChildCount { get { return _ItemCount; } }
	public uint ItemCount { get { return _ItemCount; } }
	public ushort Level { get { return 0; } }
	#endregion

	#region Access
	public T GetAt(uint pos) { return Items[pos]; }
	#endregion

	#region Modification
	internal bool AppendItem(T item)
		{
		if(_ItemCount==GroupSize)
			return false;
		Items[_ItemCount++]=item;
		return true;
		}
	internal void AppendItems(T[] items, ushort pos, ushort count)
		{
		for(int i=0; i<count; i++)
			Items[_ItemCount+i]=items[pos+i];
		_ItemCount+=count;
		}
	internal bool InsertItem(ushort at, T item)
		{
		if(_ItemCount==GroupSize)
			return false;
		for(int i=_ItemCount; i>=at+1; i--)
			Items[i]=Items[i-1];
		Items[at]=item;
		_ItemCount++;
		return true;
		}
	internal void InsertItems(ushort at, T[] items, ushort pos, ushort count)
		{
		for(int i=_ItemCount+count-1; i>=at+count; i--)
			Items[i]=Items[i-count];
		for(int i=0; i<count; i++)
			Items[at+i]=items[pos+i];
		_ItemCount+=count;
		}
	public T RemoveAt(uint pos)
		{
		var item=Items[pos];
		for(int i=(int)pos; i+1<_ItemCount; i++)
			Items[i]=Items[i+1];
		Items[_ItemCount-1]=default;
		_ItemCount--;
		return item;
		}
	internal void RemoveItems(ushort pos, ushort count)
		{
		for(int i=pos; i+count<_ItemCount; i++)
			Items[i]=Items[i+count];
		for(int i=0; i<count; i++)
			Items[_ItemCount-i-1]=default;
		_ItemCount-=count;
		}
	public void SetAt(uint pos, T item)
		{
		Items[pos]=item;
		}
	#endregion

	#region Members
	internal const ushort GroupSize=Cluster<T>.GroupSize;
	internal T[] Items;
	protected ushort _ItemCount;
	#endregion
	}

