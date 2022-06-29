//==================
// StringHelper.cpp
//==================

#include "pch.h"


//=======
// Using
//=======

#include <vcclr.h>
#include "StringHelper.h"


//========
// Common
//========

const BYTE CharCompareCaseSensitive[]=
	{
	255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, // 0x00
	255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, // 0x10
	255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, // 0x20
//    0    1    2    3    4    5    6    7    8    9
	  0,   1,   2,   3,   4,   5,   6,   7,   8,   9, 255, 255, 255, 255, 255, 255, // 0x30
//         A    B    C    D    E    F    G    H    I    J    K    L    M    N    O
	255,  10,  26,  28,  32,  36,  46,  48,  50,  52,  62,  64,  66,  68,  70,  74, // 0x40
//    P    Q    R    S    T    U    V    W    X    Y    Z
	 86,  88,  90,  92,  95,  97, 107, 109, 111, 113, 117, 255, 255, 255, 255, 255, // 0x50
//         a    b    c    d    e    f    g    h    i    j    k    l    m    n    o
	255,  11,  27,  29,  33,  37,  47,  49,  51,  53,  63,  65,  67,  69,  71,  75, // 0x60
//    p    q    r    s    t    u    v    w    x    y    z
	 87,  89,  91,  93,  96,  98, 108, 110, 112, 114, 118, 255, 255, 255, 255, 255, // 0x70
	255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, // 0x80
	255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, // 0x90
	255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, // 0xA0
	255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, // 0xB0
//    À    Á    Â    Ã    Ä    Å    Æ    Ç    È    É    Ê    Ë    Ì    Í    Î    Ï
	 12,  14,  16,  18,  20,  22,  24,  30,  38,  40,  42,  44,  54,  56,  58,  60, // 0xC0
//    Ð    Ñ    Ò    Ó    Ô    Õ    Ö    ×    Ø    Ù    Ú    Û    Ü    Ý    Þ    ß
	 34,  72,  76,  78,  80,  82,  84, 255, 255,  99, 101, 103, 105, 115, 255,  94, // 0xD0
//    à    á    â    ã    ä    å    æ    ç    è    é    ê    ë    ì    í    î    ï
	 13,  15,  17,  19,  21,  23,  25,  31,  39,  41,  43,  45,  55,  57,  59,  61, // 0xE0
//    ð    ñ    ò    ó    ô    õ    ö    ÷    ø    ù    ú    û    ü    ý    þ    ÿ
	 35,  73,  77,  79,  81,  83,  85, 255, 255, 100, 102, 104, 106, 116, 255, 255, // 0xF0
	};

INT CharCompare(WCHAR c1, WCHAR c2)
{
BYTE id1=CharCompareCaseSensitive[c1];
BYTE id2=CharCompareCaseSensitive[c2];
if(id1>id2)
	return 1;
if(id1<id2)
	return -1;
if(id1!=255)
	return 0;
if(c1>c2)
	return 1;
if(c1<c2)
	return -1;
return 0;
}

inline bool CharIsNumber(WCHAR Char)
{
if(Char>=L'0'&&Char<=L'9')
	return true;
return false;
}

INT StringCompare(System::String^ str1, System::String^ str2)
{
pin_ptr<const wchar_t> pstr1=PtrToStringChars(str1);
pin_ptr<const wchar_t> pstr2=PtrToStringChars(str2);
if(!pstr1)
	{
	if(!pstr2)
		return 0;
	if(!pstr2[0])
		return 0;
	return -1;
	}
if(!pstr2)
	{
	if(!pstr1[0])
		return 0;
	return 1;
	}
UINT pos1=0;
UINT pos2=0;
while(pstr1[pos1]&&pstr2[pos2])
	{
	FLOAT f1=0;
	UINT len1=StringScanFloat(&pstr1[pos1], &f1);
	FLOAT f2=0;
	UINT len2=StringScanFloat(&pstr2[pos2], &f2);
	if(len1==0)
		{
		if(len2==0)
			{
			INT cmp=CharCompare(pstr1[pos1], pstr2[pos2]);
			if(cmp==0)
				{
				pos1++;
				pos2++;
				continue;
				}
			return cmp;
			}
		return -1;
		}
	if(len2==0)
		return 1;
	if(f1>f2)
		return 1;
	if(f2>f1)
		return -1;
	pos1+=len1;
	pos2+=len2;
	}
if(pstr1[pos1]==0)
	{
	if(pstr2[pos2]==0)
		return 0;
	return -1;
	}
return 1;
}

UINT StringScanFloat(LPCWSTR str, FLOAT* pf)
{
if(!str)
	return 0;
UINT pos=0;
bool negative=false;
for(; str[pos]==L'-'; pos++)
	{
	negative=!negative;
	}
if(!CharIsNumber(str[pos]))
	return 0;
FLOAT f=(FLOAT)str[pos]-L'0';
for(pos++; str[pos]; pos++)
	{
	if(!CharIsNumber(str[pos]))
		break;
	f*=10;
	f+=(FLOAT)str[pos]-'0';
	}
if(str[pos])
	{
	if((str[pos]==L'.')||(str[pos]==L','))
		{
		FLOAT div=10.f;
		for(pos++; str[pos]; pos++)
			{
			if(!CharIsNumber(str[pos]))
				break;
			f+=((FLOAT)str[pos]-L'0')/div;
			div*=10.f;
			}
		}
	}
if(str[pos])
	{
	if((str[pos]==L'E')||(str[pos]==L'e'))
		{
		pos++;
		INT ex=0;
		UINT ex_len=StringScanInt(&str[pos], &ex);
		if(ex_len==0)
			return 0;
		pos+=ex_len;
		ex*=10;
		if(ex<0)
			{
			f/=(FLOAT)-ex;
			}
		else
			{
			f*=(FLOAT)ex;
			}
		}
	}
if(negative)
	f*=-1;
if(pf)
	*pf=f;
return pos;
}

UINT StringScanInt(LPCWSTR str, INT* pint)
{
if(!str)
	return 0;
UINT pos=0;
bool negative=false;
for(; str[pos]==L'-'; pos++)
	{
	negative=!negative;
	}
if(!CharIsNumber(str[pos]))
	return 0;
INT i=(INT)str[pos]-L'0';
for(pos++; str[pos]; pos++)
	{
	if(!CharIsNumber(str[pos]))
		break;
	i*=10;
	i+=(INT)str[pos]-L'0';
	}
if(negative)
	i*=-1;
if(pint)
	*pint=i;
return pos;
}
