//========================
// DictionaryIterator.cpp
//========================

#include "pch.h"


//=======
// Using
//=======

#include "Dictionary.h"
#include "DictionaryItem.h"

using namespace System::Threading;


//===========
// Namespace
//===========

namespace Clusters {


//========
// Access
//========

System::Object^ DictionaryIterator::Current::get()
{
auto key=cIt.get_current_id();
auto value=cIt.get_current_item();
return gcnew DictionaryItem(key, value);
}

bool DictionaryIterator::HasCurrent::get()
{
return cIt.has_current();
}


//============
// Navigation
//============

bool DictionaryIterator::Find(System::String^ key)
{
return cIt.find(StringClass(key));
}

bool DictionaryIterator::MoveNext()
{
return cIt.move_next();
}

bool DictionaryIterator::MovePrevious()
{
return cIt.move_previous();
}

INT DictionaryIterator::Position::get()
{
return cIt.get_position();
}

VOID DictionaryIterator::Position::set(INT pos)
{
cIt.set_position(pos);
}

VOID DictionaryIterator::Reset()
{
cIt.set_position(0);
}


//==============
// Modification
//==============

VOID DictionaryIterator::RemoveCurrent()
{
cIt.remove_current();
}


//===========================
// Con-/Destructors Internal
//===========================

DictionaryIterator::DictionaryIterator(Dictionary^ dict, INT pos):
cIt(%dict->cList, pos),
hDictionary(dict)
{
Monitor::Enter(%hDictionary->cLock);
}

DictionaryIterator::DictionaryIterator(Dictionary^ dict, System::String^ key):
cIt(%dict->cList, 0, StringClass(key)),
hDictionary(dict)
{
Monitor::Enter(%hDictionary->cLock);
}

DictionaryIterator::~DictionaryIterator()
{
Monitor::Exit(%hDictionary->cLock);
}

}