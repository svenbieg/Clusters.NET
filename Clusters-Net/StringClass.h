//===============
// StringClass.h
//===============

#pragma once


//=======
// Using
//=======

#include "StringHelper.h"


//==============
// String-Class
//==============

ref class StringClass
{
public:
	// Con-/Destructors
	inline StringClass(System::String^ Value): hString(Value) {}
	inline StringClass(StringClass const% Value): hString(Value.hString) {}

	// Access
	inline operator System::String^() { return hString; }

	// Comparison
	inline bool operator==(StringClass const% Value) { return StringCompare(hString, Value.hString)==0; }
	inline bool operator!=(StringClass const% Value) { return StringCompare(hString, Value.hString)!=0; }
	inline bool operator>(StringClass const% Value) { return StringCompare(hString, Value.hString)>0; }
	inline bool operator>=(StringClass const% Value) { return StringCompare(hString, Value.hString)>=0; }
	inline bool operator<(StringClass const% Value) { return StringCompare(hString, Value.hString)<0; }
	inline bool operator<=(StringClass const% Value) { return StringCompare(hString, Value.hString)<=0; }

private:
	// Common
	System::String^ hString;
};
