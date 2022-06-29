//===============
// CatalogItem.h
//===============

#pragma once


//===========
// Namespace
//===========

namespace Clusters {


//==============
// Catalog-Item
//==============

public ref class CatalogItem sealed
{
internal:
	// Con-/Destructors
	CatalogItem(System::Guid Id, System::Object^ Item);

public:
	// Common
	property System::Guid Id { System::Guid get(); }
	property System::Object^ Item { System::Object^ get(); VOID set(System::Object^ Value); }

private:
	// Common
	System::Object^ hItem;
	System::Guid uId;
};

}