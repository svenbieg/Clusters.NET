//==================
// ListItemGroup.cs
//==================

// Copyright 2024, Sven Bieg (svenbieg@web.de)
// http://github.com/svenbieg/Clusters.NET

namespace Clusters;

internal class ListItemGroup<T>: ClusterItemGroup<T>, IListGroup<T>
	{
	#region Con-/Destructors
	internal ListItemGroup() {}
	internal ListItemGroup(ListItemGroup<T> copy): base(copy) {}
	#endregion

	#region Common
	public T First { get { return Items[0]; } }
	public T Last { get { return Items[_ItemCount-1]; } }
	#endregion

	#region Modification
	public bool Append(T item, bool again)
		{
		return AppendItem(item);
		}
	public bool InsertAt(uint pos, T item, bool again)
		{
		return InsertItem((ushort)pos, item);
		}
	#endregion
	}
