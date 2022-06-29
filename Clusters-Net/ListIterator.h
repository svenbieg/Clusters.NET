//================
// ListIterator.h
//================

#pragma once


//=======
// Using
//=======

#include "list.hpp"


//===========
// Namespace
//===========

namespace Clusters {


//======================
// Forward-Declarations
//======================

ref class List;


//===============
// List-Iterator
//===============

public ref class ListIterator sealed: public System::Collections::IEnumerator
{
public:
	// Access
	virtual property System::Object^ Current { System::Object^ get(); }
	virtual property bool HasCurrent { bool get(); }

	// Navigation
	virtual bool MoveNext();
	bool MovePrevious();
	property INT Position { INT get(); VOID set(INT Position); }
	virtual void Reset();

	// Modification
	VOID RemoveCurrent();

internal:
	// Con-/Destructors
	ListIterator(List^ List, INT Position);
	~ListIterator();

private:
	// Using
	using list_t=_list<System::Object>;
	using it_t=list_t::_it_t;

	// Common
	it_t cIt;
	List^ hList;
};

}