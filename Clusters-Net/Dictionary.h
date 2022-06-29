//==============
// Dictionary.h
//==============

#pragma once


//=======
// Using
//=======

#include "DictionaryIterator.h"


//===========
// Namespace
//===========

namespace Clusters {


//============
// Dictionary
//============

public ref class Dictionary sealed: public System::Collections::IEnumerable
{
public:
	// Con-/Destructors
	Dictionary();
	Dictionary(Dictionary^ Dictionary);

	// Access
	bool Contains(System::String^ Key);
	virtual property INT Count { INT get(); }
	System::Object^ Get(System::String^ Key);

	// Navigation
	DictionaryIterator^ At(INT Position);
	DictionaryIterator^ Find(System::String^ Key);
	DictionaryIterator^ First();
	virtual System::Collections::IEnumerator^ GetEnumerator();
	DictionaryIterator^ Last();
	
	// Modification
	bool Add(System::String^ Key, System::Object^ Value);
	virtual VOID Clear();
	bool Remove(System::String^ Key);
	VOID Set(System::String^ Key, System::Object^ Value);

internal:
	// Using
	using slist_t=_slist<StringClass, System::Object^>;

	// Common
	slist_t cList;
	System::Object cLock;
};

}