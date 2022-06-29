//=============
// Catalog.cpp
//=============

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


//==================
// Con-/Destructors
//==================

Catalog::Catalog()
{}

Catalog::Catalog(Catalog^ catalog)
{
Monitor::Enter(%catalog->cLock);
cList.set(%catalog->cList);
Monitor::Exit(%catalog->cLock);
}


//========
// Access
//========

bool Catalog::Contains(System::Guid id)
{
Monitor::Enter(%cLock);
bool contains=cList.contains(GuidClass(id));
Monitor::Exit(%cLock);
return contains;
}

INT Catalog::Count::get()
{
Monitor::Enter(%cLock);
INT count=cList.get_count();
Monitor::Exit(%cLock);
return count;
}

System::Object^ Catalog::Get(System::Guid id)
{
Monitor::Enter(%cLock);
System::Object^ obj=cList.get(GuidClass(id));
Monitor::Exit(%cLock);
return obj;
}


//============
// Navigation
//============

CatalogIterator^ Catalog::At(INT pos)
{
return gcnew CatalogIterator(this, pos);
}

CatalogIterator^ Catalog::Find(System::Guid id)
{
return gcnew CatalogIterator(this, id);
}

CatalogIterator^ Catalog::First()
{
return gcnew CatalogIterator(this, 0);
}

System::Collections::IEnumerator^ Catalog::GetEnumerator()
{
return gcnew CatalogIterator(this, 0);
}

CatalogIterator^ Catalog::Last()
{
return gcnew CatalogIterator(this, -1);
}


//==============
// Modification
//==============

bool Catalog::Add(System::Guid id, System::Object^ item)
{
Monitor::Enter(%cLock);
bool added=cList.add(GuidClass(id), item);
Monitor::Exit(%cLock);
return added;
}

VOID Catalog::Clear()
{
Monitor::Enter(%cLock);
cList.clear();
Monitor::Exit(%cLock);
}

bool Catalog::Remove(System::Guid id)
{
Monitor::Enter(%cLock);
bool removed=cList.remove(GuidClass(id));
Monitor::Exit(%cLock);
return removed;
}

VOID Catalog::Set(System::Guid id, System::Object^ item)
{
Monitor::Enter(%cLock);
cList.set(GuidClass(id), item);
Monitor::Exit(%cLock);
}

}