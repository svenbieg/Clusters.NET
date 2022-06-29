//==========
// List.cpp
//==========

#include "pch.h"


//=======
// Using
//=======

#include "List.h"

using namespace System::Threading;


//===========
// Namespace
//===========

namespace Clusters {


//==================
// Con-/Destructors
//==================

List::List()
{}

List::List(List^ list)
{
Monitor::Enter(%list->cLock);
cList.set(%list->cList);
Monitor::Exit(%list->cLock);
}


//========
// Access
//========

INT List::Count::get()
{
Monitor::Enter(%cLock);
INT count=cList.get_count();
Monitor::Exit(%cLock);
return count;
}

System::Object^ List::GetAt(INT pos)
{
Monitor::Enter(%cLock);
auto obj=cList.get_at(pos);
Monitor::Exit(%cLock);
return obj;
}


//============
// Navigation
//============

ListIterator^ List::At(INT pos)
{
return gcnew ListIterator(this, pos);
}

ListIterator^ List::First()
{
return gcnew ListIterator(this, 0);
}

System::Collections::IEnumerator^ List::GetEnumerator()
{
return gcnew ListIterator(this, 0);
}

ListIterator^ List::Last()
{
return gcnew ListIterator(this, -1);
}


//==============
// Modification
//==============

VOID List::Append(Object^ hobj)
{
Monitor::Enter(%cLock);
cList.append(hobj);
Monitor::Exit(%cLock);
}

VOID List::Clear()
{
Monitor::Enter(%cLock);
cList.clear();
Monitor::Exit(%cLock);
}

VOID List::InsertAt(INT pos, System::Object^ obj)
{
Monitor::Enter(%cLock);
cList.insert_at(pos, obj);
Monitor::Exit(%cLock);
}

VOID List::RemoveAt(INT pos)
{
Monitor::Enter(%cLock);
cList.remove_at(pos);
Monitor::Exit(%cLock);
}

}