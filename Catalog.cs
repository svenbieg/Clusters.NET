//============
// Catalog.cs
//============

// Windows.NET-implementation of a GUID-catalog

// Copyright 2022, Sven Bieg (svenbieg@web.de)
// http://github.com/svenbieg/Clusters.NET


//===========
// Namespace
//===========

namespace Clusters {


//======
// Item
//======

public class CatalogItem<T>: IndexItem<Guid, T>
{
public CatalogItem(Guid Key): base(Key) {}
public CatalogItem(Guid Key, T? Value): base(Key, Value) {}
}


//=========
// Catalog
//=========

public class Catalog<T>: Index<CatalogItem<T>>
{
// Con-/Destructors
public Catalog() {}
public Catalog(Catalog<T> Catalog): base(Catalog) {}

// Modification
public void Add(Guid Key)
	{
	Add(new CatalogItem<T>(Key));
	}
public void Add(Guid Key, T? Value)
	{
	Add(new CatalogItem<T>(Key, Value));
	}
}

}
