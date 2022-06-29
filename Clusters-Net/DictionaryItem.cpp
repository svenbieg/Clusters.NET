//====================
// DictionaryItem.cpp
//====================

#include "pch.h"


//=======
// Using
//=======

#include "DictionaryItem.h"


//===========
// Namespace
//===========

namespace Clusters {


//==================
// Con-/Destructors
//==================

DictionaryItem::DictionaryItem(System::String^ key, System::Object^ value):
hKey(key),
hValue(value)
{}


//========
// Common
//========

System::String^ DictionaryItem::Key::get()
{
return hKey;
}

System::Object^ DictionaryItem::Value::get()
{
return hValue;
}

VOID DictionaryItem::Value::set(System::Object^ value)
{
hValue=value;
}

}