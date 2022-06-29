//=============
// GuidClass.h
//=============

#pragma once


//======
// Guid
//======

ref class GuidClass
{
public:
	// Con-/Destructors
	GuidClass() { hId=gcnew System::Guid(); }
	GuidClass(GuidClass const% Id): hId(Id.hId) {}
	GuidClass(System::Guid const% Id): hId(Id) {}

	// Conversion
	operator System::Guid() { return *hId; }

	// Comparison
	inline bool operator==(GuidClass const% Id) { return hId->CompareTo(Id.hId)==0; }
	inline bool operator>(GuidClass const% Id) { return hId->CompareTo(Id.hId)>0; }
	inline bool operator>=(GuidClass const% Id) { return hId->CompareTo(Id.hId)>=0; }
	inline bool operator<(GuidClass const% Id) { return hId->CompareTo(Id.hId)<0; }
	inline bool operator<=(GuidClass const% Id) { return hId->CompareTo(Id.hId)<=0; }

private:
	// Common
	System::Guid^ hId;
};
