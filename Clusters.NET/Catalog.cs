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


//=========
// Catalog
//=========

public class Catalog<T>: Map<Guid, T>
{
// Con-/Destructors
public Catalog() {}
public Catalog(Catalog<T> Catalog): base(Catalog) {}
}

}
