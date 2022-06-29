//======================
// DictionaryIterator.h
//======================

#pragma once


//=======
// Using
//=======

#include "slist.hpp"
#include "StringClass.h"


//===========
// Namespace
//===========

namespace Clusters {


//======================
// Forward-Declarations
//======================

ref class Dictionary;


//=====================
// Dictionary-Iterator
//=====================

public ref class DictionaryIterator sealed: public System::Collections::IEnumerator
{
public:
	// Access
	virtual property System::Object^ Current { System::Object^ get(); }
	virtual property bool HasCurrent { bool get(); }

	// Modification
	VOID RemoveCurrent();

	// Navigation
	bool Find(System::String^ Key);
	virtual bool MoveNext();
	bool MovePrevious();
	property INT Position { INT get(); VOID set(INT Position); }
	virtual void Reset();

internal:
	// Con-/Destructors
	DictionaryIterator(Clusters::Dictionary^ Dictionary, INT Position);
	DictionaryIterator(Clusters::Dictionary^ Dictionary, System::String^ Key);
	~DictionaryIterator();

private:
	// Using
	using slist_t=_slist<StringClass, System::Object^>;
	using it_t=slist_t::_it_t;

	// Common
	it_t cIt;
	Dictionary^ hDictionary;
};

}