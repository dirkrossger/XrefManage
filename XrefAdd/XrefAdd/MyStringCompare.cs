using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace XrefAdd
{
    public class MyStringCompare
    {
        public MyStringCompare() { }

        public static int Compare(object obj1, object obj2)
        {
            string str1 = (string)obj1, str2 = (string)obj2;
            int i1 = 0,
                i2 = 0,
                CompareResult,
                l1 = str1.Length,
                l2 = str2.Length,
                tempI1,
                tempI2;
            string s1, s2;
            bool b1, b2;

            while (true)
            {
                b1 = Char.IsDigit(str1, i1);
                b2 = Char.IsDigit(str2, i2);
                if (!b1 && b2)
                    return -1;
                if (b1 && !b2)
                    return 1;
                if (b1 && b2)
                {
                    FindLastDigit(str1, ref i1, out s1);
                    FindLastDigit(str2, ref i2, out s2);
                    tempI1 = Convert.ToInt32(s1);
                    tempI2 = Convert.ToInt32(s2);
                    if (tempI1.Equals(tempI2))
                        CompareResult = 0;
                    else if (tempI1 < tempI2)
                        CompareResult = -1;
                    else
                        CompareResult = 1;
                    if (!CompareResult.Equals(0))
                        return CompareResult;
                }
                else
                {
                    FindLastLetter(str1, ref i1, out s1);
                    FindLastLetter(str2, ref i2, out s2);
                    CompareResult = string.Compare(s1, s2);
                    if (!CompareResult.Equals(0))
                        return CompareResult;
                }
                if (l1 <= i1)
                {
                    if (l2 <= i2)
                    {
                        return 0;
                    }
                    else
                    {
                        return -1;
                    }
                }
                if (l2 <= i2)
                {
                    if (l1 < i1)
                    {
                        return 0;
                    }
                    else
                    {
                        return 1;
                    }
                }
            }
        }
        private static void FindLastLetter(string MainStr, ref int i, out string OutStr)
        {
            int StartPos = i;
            int StrLen = MainStr.Length;
            ++i;
            while (i < StrLen && !Char.IsDigit(MainStr, i))
                ++i;
            OutStr = MainStr.Substring(StartPos, i - StartPos);
        }
        private static void FindLastDigit(string MainStr, ref int i, out string OutStr)
        {
            int StartPos = i;
            int StrLen = MainStr.Length;
            ++i;
            while (i < StrLen && Char.IsDigit(MainStr, i))
                ++i;
            OutStr = MainStr.Substring(StartPos, i - StartPos);
        }
    }

    internal class MyStringCompare1 : IComparer
    {
        public MyStringCompare1() { }
        public int Compare(object x, object y)
        {
            return MyStringCompare.Compare(x, y);
        }
    }
}
