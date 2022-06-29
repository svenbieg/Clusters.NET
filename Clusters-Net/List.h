//========
// List.h
//========

#pragma once


//=======
// Using
//=======

#include "ListIterator.h"


//===========
// Namespace
//===========

namespace Clusters {


//======
// List
//======

public ref class List sealed: public System::Collections::IEnumerable
{
public:
	// Con-/Destructors
	List();
	List(List^ List);
	
	// Access
	property INT Count { INT get(); }
	System::Object^ GetAt(INT Position);

	// Navigation
	ListIterator^ At(INT Position);
	ListIterator^ First();
	virtual System::Collections::IEnumerator^ GetEnumerator();
	ListIterator^ Last();
	
	// Modification
	VOID Append(System::Object^ Item);
	VOID Clear();
	VOID InsertAt(INT Position, System::Object^ Item);
	VOID RemoveAt(INT Position);

internal:
	// Using
	using list_t=_list<System::Object>;

	// Common
	list_t cList;
	System::Object cLock;
};

}