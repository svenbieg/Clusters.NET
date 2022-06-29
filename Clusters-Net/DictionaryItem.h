//==================
// DictionaryItem.h
//==================

#pragma once


//===========
// Namespace
//===========

namespace Clusters {


//=================
// Dictionary-Item
//=================

public ref class DictionaryItem sealed
{
internal:
	// Con-/Destructors
	DictionaryItem(System::String^ Key, System::Object^ Value);

public:
	// Common
	property System::String^ Key { System::String^ get(); }
	property System::Object^ Value { System::Object^ get(); VOID set(System::Object^ Value); }

private:
	// Common
	System::String^ hKey;
	System::Object^ hValue;
};

}