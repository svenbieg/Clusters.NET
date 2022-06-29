//================
// Dictionary.cpp
//================

#include "pch.h"


//=======
// Using
//=======

#include "Dictionary.h"

using namespace System::Threading;


//===========
// Namespace
//===========

namespace Clusters {


//==================
// Con-/Destructors
//==================

Dictionary::Dictionary()
{}

Dictionary::Dictionary(Dictionary^ dict)
{
Monitor::Enter(%dict->cLock);
cList.set(%dict->cList);
Monitor::Exit(%dict->cLock);
}


//========
// Access
//========

bool Dictionary::Contains(System::String^ key)
{
Monitor::Enter(%cLock);
bool contains=cList.contains(StringClass(key));
Monitor::Exit(%cLock);
return contains;
}

INT Dictionary::Count::get()
{
Monitor::Enter(%cLock);
INT count=cList.get_count();
Monitor::Exit(%cLock);
return count;
}

System::Object^ Dictionary::Get(System::String^ key)
{
Monitor::Enter(%cLock);
auto obj=cList.get(StringClass(key));
Monitor::Exit(%cLock);
return obj;
}


//============
// Navigation
//============

DictionaryIterator^ Dictionary::At(INT pos)
{
return gcnew DictionaryIterator(this, pos);
}

DictionaryIterator^ Dictionary::Find(System::String^ key)
{
return gcnew DictionaryIterator(this, key);
}

DictionaryIterator^ Dictionary::First()
{
return gcnew DictionaryIterator(this, 0);
}

System::Collections::IEnumerator^ Dictionary::GetEnumerator()
{
return gcnew DictionaryIterator(this, 0);
}

DictionaryIterator^ Dictionary::Last()
{
return gcnew DictionaryIterator(this, -1);
}


//==============
// Modification
//==============

bool Dictionary::Add(System::String^ key, System::Object^ value)
{
Monitor::Enter(%cLock);
bool added=cList.add(StringClass(key), value);
Monitor::Exit(%cLock);
return added;
}

VOID Dictionary::Clear()
{
Monitor::Enter(%cLock);
cList.clear();
Monitor::Exit(%cLock);
}

bool Dictionary::Remove(System::String^ key)
{
Monitor::Enter(%cLock);
bool removed=cList.remove(StringClass(key));
Monitor::Exit(%cLock);
return removed;
}

VOID Dictionary::Set(System::String^ key, System::Object^ value)
{
Monitor::Enter(%cLock);
cList.set(StringClass(key), value);
Monitor::Exit(%cLock);
}

}