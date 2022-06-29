//==================
// ListIterator.cpp
//==================

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


//========
// Access
//========

System::Object^ ListIterator::Current::get()
{
return cIt.get_current();
}

bool ListIterator::HasCurrent::get()
{
return cIt.has_current();
}


//============
// Navigation
//============

bool ListIterator::MoveNext()
{
return cIt.move_next();
}

bool ListIterator::MovePrevious()
{
return cIt.move_previous();
}

INT ListIterator::Position::get()
{
return cIt.get_position();
}

VOID ListIterator::Position::set(INT pos)
{
cIt.set_position(pos);
}

VOID ListIterator::Reset()
{
cIt.set_position(0);
}


//==============
// Modification
//==============

VOID ListIterator::RemoveCurrent()
{
cIt.remove_current();
}


//===========================
// Con-/Destructors Internal
//===========================

ListIterator::ListIterator(List^ list, INT pos):
cIt(%list->cList, pos),
hList(list)
{
Monitor::Enter(%hList->cLock);
}

ListIterator::~ListIterator()
{
Monitor::Exit(%hList->cLock);
}

}