//===================
// CatalogIterator.h
//===================

#pragma once


//=======
// Using
//=======

#include "CatalogItem.h"
#include "GuidClass.h"
#include "slist.hpp"


//===========
// Namespace
//===========

namespace Clusters {


//======================
// Forward-Declarations
//======================

ref class Catalog;


//==================
// Catalog-Iterator
//==================

public ref class CatalogIterator sealed: public System::Collections::IEnumerator
{
public:
	// Access
	virtual property System::Object^ Current { System::Object^ get(); }
	property bool HasCurrent { bool get(); }

	// Navigation
	bool Find(System::Guid Id);
	virtual bool MoveNext();
	bool MovePrevious();
	property INT Position { INT get(); VOID set(INT Position); }
	virtual void Reset();

	// Modification
	VOID RemoveCurrent();

internal:
	// Con-/Destructors
	CatalogIterator(Catalog^ Catalog, INT Position);
	CatalogIterator(Catalog^ Catalog, System::Guid Id);
	~CatalogIterator();

private:
	// Using
	using slist_t=_slist<GuidClass, System::Object^>;
	using it_t=slist_t::_it_t;

	// Common
	it_t cIt;
	Catalog^ hCatalog;
};

}