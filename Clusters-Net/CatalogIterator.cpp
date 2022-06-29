//=====================
// CatalogIterator.cpp
//=====================

#include "pch.h"


//=======
// Using
//=======

#include "Catalog.h"

using namespace System::Threading;


//===========
// Namespace
//===========

namespace Clusters {


//========
// Access
//========

System::Object^ CatalogIterator::Current::get()
{
if(!cIt.has_current())
	return nullptr;
auto id=cIt.get_current_id();
auto item=cIt.get_current_item();
return gcnew CatalogItem(id, item);
}

bool CatalogIterator::HasCurrent::get()
{
return cIt.has_current();
}


//============
// Navigation
//============

bool CatalogIterator::Find(System::Guid id)
{
return cIt.find(GuidClass(id));
}

bool CatalogIterator::MoveNext()
{
return cIt.move_next();
}

bool CatalogIterator::MovePrevious()
{
return cIt.move_previous();
}

INT CatalogIterator::Position::get()
{
return cIt.get_position();
}

VOID CatalogIterator::Position::set(INT pos)
{
cIt.set_position(pos);
}

VOID CatalogIterator::Reset()
{
cIt.set_position(0);
}


//==============
// Modification
//==============

VOID CatalogIterator::RemoveCurrent()
{
cIt.remove_current();
}


//===========================
// Con-/Destructors Internal
//===========================

CatalogIterator::CatalogIterator(Catalog^ catalog, INT pos):
cIt(%catalog->cList, pos),
hCatalog(catalog)
{
Monitor::Enter(%hCatalog->cLock);
}

CatalogIterator::CatalogIterator(Catalog^ catalog, System::Guid id):
cIt(%catalog->cList, 0, GuidClass(id)),
hCatalog(catalog)
{
Monitor::Enter(%hCatalog->cLock);
}

CatalogIterator::~CatalogIterator()
{
Monitor::Exit(%hCatalog->cLock);
}

}