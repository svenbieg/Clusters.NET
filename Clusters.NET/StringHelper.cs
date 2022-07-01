//=================
// StringHelper.cs
//=================

// Helper-class for string-comparison

// Copyright 2022, Sven Bieg (svenbieg@web.de)
// http://github.com/svenbieg/Clusters.NET


//===========
// Namespace
//===========

namespace Clusters {


//==============
// String-Class
//==============

internal class StringHelper
{
// Comparison
private static byte[] CharCompareValues=
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
private static int CharCompare(char Char1, char Char2)
	{
	char[] chars={ Char1, Char2 };
	var bytes=System.Text.Encoding.ASCII.GetBytes(chars);
	byte id1=CharCompareValues[bytes[0]];
	byte id2=CharCompareValues[bytes[1]];
	if(id1>id2)
		return 1;
	if(id1<id2)
		return -1;
	if(id1!=255)
		return 0;
	if(Char1>Char2)
		return 1;
	if(Char1<Char2)
		return -1;
	return 0;
	}
private static bool CharIsNumber(Char Char)
{
if(Char>='0'&&Char<='9')
	return true;
return false;
}
internal static int StringCompare(string String1, string String2)
	{
	unsafe
		{
		fixed(char* pstr1=String1, pstr2=String2)
			{
			if(pstr1[0]==0)
				{
				if(pstr2[0]==0)
					return 0;
				return -1;
				}
			if(pstr2[0]==0)
				return 1;
			int pos1=0;
			int pos2=0;
			while(pstr1[pos1]!=0&&pstr2[pos2]!=0)
				{
				(int len1, int int1)=ScanInt(&pstr1[pos1]);
				(int len2, int int2)=ScanInt(&pstr2[pos2]);
				if(len1==0)
					{
					if(len2==0)
						{
						int cmp=CharCompare(pstr1[pos1], pstr2[pos2]);
						if(cmp==0)
							{
							pos1++;
							pos2++;
							continue;
							}
						return cmp;
						}
					return 1;
					}
				if(len2==0)
					return -1;
				if(int1>int2)
					return 1;
				if(int1<int2)
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
		}
	}
private static unsafe (int Length, int Integer) ScanInt(char* Value)
	{
	int pos=0;
	int value=0;
	for(; Value[pos]!=0; pos++)
		{
		if(!CharIsNumber(Value[pos]))
			break;
		int i=Value[pos]-'0';
		value*=10;
		value+=i;
		}
	return (pos, value);
	}
}

}
