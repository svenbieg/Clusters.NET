//=============
// Comparer.cs
//=============

// Copyright 2024, Sven Bieg (svenbieg@web.de)
// http://github.com/svenbieg/Clusters.NET

using System.Text;


//===========
// Namespace
//===========

namespace Clusters;


//===========
// Interface
//===========

public interface IComparer<T>
	{
	int Compare(T key1, T key2);
	uint GetHashCode(T key);
	}


//==========
// Comparer
//==========

public class Comparer<T>: IComparer<T> where T: IComparable<T>
	{
	#region Common
	public static IComparer<T> Default
		{
		get
			{
			if(typeof(T)==typeof(String))
				return (IComparer<T>)new StringComparer();
			return new Comparer<T>();
			}
		}
	#endregion

	#region IComparer
	public int Compare(T key1, T key2)
		{
		return key1.CompareTo(key2);
		}
	public uint GetHashCode(T key)
		{
		return (uint)key.GetHashCode();
		}
	#endregion
	}


//=================
// String-Comparer
//=================

public class StringComparer: IComparer<String>
	{
	#region Con-/Destructors
	public StringComparer() {}
	#endregion

	#region IComparer
	public int Compare(String key1, String key2)
		{
		var bytes1=Encoding.ASCII.GetBytes(key1);
		var bytes2=Encoding.ASCII.GetBytes(key2);
		int len1=bytes1.Length;
		int len2=bytes2.Length;
		int pos1=0;
		int pos2=0;
		while(true)
			{
			if(pos1==len1)
				{
				if(pos2==len2)
					return 0;
				return -1;
				}
			if(pos2==len2)
				return 1;
			byte b1=bytes1[pos1];
			byte b2=bytes2[pos2];
			byte s1=CharSort[b1];
			byte s2=CharSort[b2];
			if(s1==254)
				{
				if(s2==254)
					{
					int i1;
					int i2;
					pos1=ReadNumber(bytes1, len1, pos1, out i1);
					pos2=ReadNumber(bytes2, len2, pos2, out i2);
					if(i1<i2)
						return -1;
					if(i1>i2)
						return 1;
					continue;
					}
				return -1;
				}
			if(s2==254)
				return 1;
			if(s1<s2)
				return -1;
			if(s1>s2)
				return 1;
			if(b1<b2)
				return -1;
			if(b1>b2)
				return 1;
			pos1++;
			pos2++;
			}
		}
	public uint GetHashCode(String key)
		{
		int len=key.Length;
		int copy=Math.Min(len, CharsPerHash);
		uint hash=0;
		var bytes=Encoding.ASCII.GetBytes(key, 0, copy);
		int pos=0;
		for(; pos<copy; pos++)
			{
			byte b=CharHash[bytes[pos]];
			if(b==0)
				break;
			hash=hash<<BitsPerChar;
			hash|=b;
			}
		hash=hash<<((CharsPerHash-pos)*BitsPerChar);
		return hash;
		}
	#endregion

	#region Sorting
	private const int BitsPerChar=5;
	private const int CharsPerHash=6;
	private static byte[] CharHash=
		{
	//    0    1    2    3    4    5    6    7    8    9    A    B    C    D    E    F
		  0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0, // 0x00
		  0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0, // 0x10
	//         !    "    #    $    %    &    '    (    )    *    +    ,    -    .    /
		  1,   1,   1,   1,   1,   1,   1,   1,   1,   1,   1,   1,   1,   1,   1,   1, // 0x20
	//    0    1    2    3    4    5    6    7    8    9    :    ;    <    =    >    ?
		  0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   1,   1,   1,   1,   1,   1, // 0x30
	//         A    B    C    D    E    F    G    H    I    J    K    L    M    N    O
		  1,   2,   3,   4,   5,   6,   7,   8,   9,  10,  11,  12,  13,  14,  15,  16, // 0x40
	//    P    Q    R    S    T    U    V    W    X    Y    Z    [    \    ]    ^    _
		 17,  18,  19,  20,  21,  22,  23,  24,  25,  26,  27,   1,   1,   1,   1,   1, // 0x50
	//    `    a    b    c    d    e    f    g    h    i    j    k    l    m    n    o
		  1,   2,   3,   4,   5,   6,   7,   8,   9,  10,  11,  12,  13,  14,  15,  16, // 0x60
	//    p    q    r    s    t    u    v    w    x    y    z    {    |    }    ~
		 17,  18,  19,  20,  21,  22,  23,  24,  25,  26,  27,   1,   1,   1,   1,   0, // 0x70
	//    €
		  1,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0, // 0x80
		  0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0, // 0x90
	//                                       §
		  0,   0,   0,   0,   0,   0,   0,   1,   0,   0,   0,   0,   0,   0,   0,   0, // 0xA0
	//              ²    ³         µ
		  0,   0,   1,   1,   0,   1,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0, // 0xB0
	//    À    Á    Â    Ã    Ä    Å    Æ    Ç    È    É    Ê    Ë    Ì    Í    Î    Ï
		  2,   2,   2,   2,   2,   2,   2,   4,   6,   6,   6,   6,  10,  10,  10,  10, // 0xC0
	//    Ð    Ñ    Ò    Ó    Ô    Õ    Ö    ×    Ø    Ù    Ú    Û    Ü    Ý    Þ    ß
		  5,  15,  16,  16,  16,  16,  16,   1,   1,  22,  22,  22,  22,  26,   1,  20, // 0xD0
	//    à    á    â    ã    ä    å    æ    ç    è    é    ê    ë    ì    í    î    ï
		  2,   2,   2,   2,   2,   2,   2,   4,   6,   6,   6,   6,  10,  10,  10,  10, // 0xE0
	//    ð    ñ    ò    ó    ô    õ    ö    ÷    ø    ù    ú    û    ü    ý    þ    ÿ
		  5,  15,  16,  16,  16,  16,  16,   1,   1,  22,  22,  22,  22,  26,   1,  26, // 0xF0
		};
	private static byte[] CharSort=
		{
	//    0    1    2    3    4    5    6    7    8    9    A    B    C    D    E    F
		255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, // 0x00
		255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, // 0x10
	//         !    "    #    $    %    &    '    (    )    *    +    ,    -    .    /
		255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, // 0x20
	//    0    1    2    3    4    5    6    7    8    9    :    ;    <    =    >    ?
		254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 255, 255, 255, 255, 255, 255, // 0x30
	//         A    B    C    D    E    F    G    H    I    J    K    L    M    N    O
		255,  10,  26,  28,  32,  36,  46,  48,  50,  52,  62,  64,  66,  68,  70,  74, // 0x40
	//    P    Q    R    S    T    U    V    W    X    Y    Z    [    \    ]    ^    _
		 86,  88,  90,  92,  95,  97, 107, 109, 111, 113, 117, 255, 255, 255, 255, 255, // 0x50
	//    `    a    b    c    d    e    f    g    h    i    j    k    l    m    n    o
		255,  11,  27,  29,  33,  37,  47,  49,  51,  53,  63,  65,  67,  69,  71,  75, // 0x60
	//    p    q    r    s    t    u    v    w    x    y    z    {    |    }    ~
		 87,  89,  91,  93,  96,  98, 108, 110, 112, 114, 118, 255, 255, 255, 255, 255, // 0x70
	//    €
		255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, // 0x80
		255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, // 0x90
	//                                       §
		255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, // 0xA0
	//              ²    ³         µ
		255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, // 0xB0
	//    À    Á    Â    Ã    Ä    Å    Æ    Ç    È    É    Ê    Ë    Ì    Í    Î    Ï
		 12,  14,  16,  18,  20,  22,  24,  30,  38,  40,  42,  44,  54,  56,  58,  60, // 0xC0
	//    Ð    Ñ    Ò    Ó    Ô    Õ    Ö    ×    Ø    Ù    Ú    Û    Ü    Ý    Þ    ß
		 34,  72,  76,  78,  80,  82,  84, 255, 255,  99, 101, 103, 105, 115, 255,  94, // 0xD0
	//    à    á    â    ã    ä    å    æ    ç    è    é    ê    ë    ì    í    î    ï
		 13,  15,  17,  19,  21,  23,  25,  31,  39,  41,  43,  45,  55,  57,  59,  61, // 0xE0
	//    ð    ñ    ò    ó    ô    õ    ö    ÷    ø    ù    ú    û    ü    ý    þ    ÿ
		 35,  73,  77,  79,  81,  83,  85, 255, 255, 100, 102, 104, 106, 116, 255,  26, // 0xF0
		};
	#endregion

	#region Conversion
	private int ReadNumber(byte[] bytes, int len, int pos, out int num)
		{
		num=bytes[pos]-0x30;
		for(; pos<len; pos++)
			{
			if(CharSort[bytes[pos]]!=254)
				break;
			num*=10;
			num+=bytes[pos]-0x30;
			}
		return pos;
		}
	#endregion
	}