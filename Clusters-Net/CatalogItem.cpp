//=================
// CatalogItem.cpp
//=================

#include "pch.h"


//=======
// Using
//=======

#include "Catalog.h"


//===========
// Namespace
//===========

namespace Clusters {


//==================
// Con-/Destructors
//==================

CatalogItem::CatalogItem(System::Guid id, System::Object^ item):
hItem(item),
uId(id)
{}


//========
// Common
//========

System::Guid CatalogItem::Id::get()
{
return uId;
}

System::Object^ CatalogItem::Item::get()
{
return hItem;
}

VOID CatalogItem::Item::set(System::Object^ item)
{
hItem=item;
}

}