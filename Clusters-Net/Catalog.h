//===========
// Catalog.h
//===========

#pragma once


//=======
// Using
//=======

#include "CatalogIterator.h"


//===========
// Namespace
//===========

namespace Clusters {


//=========
// Catalog
//=========

public ref class Catalog sealed: public System::Collections::IEnumerable
{
public:
	// Con-/Destructors
	Catalog();
	Catalog(Catalog^ Catalog);

	// Access
	bool Contains(System::Guid Id);
	property INT Count { INT get(); }
	System::Object^ Get(System::Guid Id);

	// Navigation
	CatalogIterator^ At(INT Position);
	CatalogIterator^ Find(System::Guid Id);
	CatalogIterator^ First();
	virtual System::Collections::IEnumerator^ GetEnumerator();
	CatalogIterator^ Last();

	// Modification
	bool Add(System::Guid Id, System::Object^ Item);
	VOID Clear();
	bool Remove(System::Guid Id);
	VOID Set(System::Guid Id, System::Object^ Item);

internal:
	// Using
	using slist_t=_slist<GuidClass, System::Object^>;

	// Common
	slist_t cList;
	System::Object cLock;
};

}