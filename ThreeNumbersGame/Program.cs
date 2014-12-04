//Kitovet DevDraft 2014 "Numbers Game" challenge
//by Michael Afrides 9/25/2014

using System;
using System.Collections.Generic;

public class Test
{
    //input
    private static readonly char[] TestCaseValueSplitter = { ' ' };
    private static readonly int MaxTestCases = 50;
    private static readonly Func<string> InputMethod = Console.ReadLine;

    //output
    private static readonly string FirstPlayerWinsString = "First";
    private static readonly string SecondPlayerWinsString = "Second";
    private static readonly string NoTestCasesString = "No_Test_Cases";
    private static readonly string BadTestCaseInputString = "Bad_Test_Case";
    private static readonly Action<string> OutputMethod = Console.WriteLine;

    //lists of differences
    private static readonly List<BigInt100> deltasBigInteger = new List<BigInt100>();
    private static readonly List<ulong> deltasULong = new List<ulong>();

    //lists to memoize winning ranges as they are calculated
    private static readonly List<Range<ulong>> smallRangeList = new List<Range<ulong>>() { new Range<ulong>(2UL, 2UL) };
    private static readonly List<Range<BigInt100>> largeRangeList = new List<Range<BigInt100>>();
    private static bool isSmallRangeCalculated = false;
    private static bool isLargeRangeCalculated = false;

    //problem range-dependent constants for computing lists of ranges
    private static readonly ulong SmallRangeAbsoluteMax = ulong.MaxValue;
    private static readonly ulong SmallRangeTopMinValue = SmallRangeAbsoluteMax / 4UL; //to prevent overflow
    private static readonly ulong SmallRangeTopMaxValue = (SmallRangeAbsoluteMax - 2UL) / 4UL; //to prevent overflow
    private static readonly BigInt100 LargeRangeMax = BigInt100.TenE100;

    static void Main(string[] args)
    {
        int numTestCases;
        if (!int.TryParse(InputMethod(), out numTestCases) || numTestCases <= 0)
        {
            OutputMethod(NoTestCasesString);
            return;
        }

        //still reads test cases if number too large, just in case
        if (numTestCases > MaxTestCases)
        {
            numTestCases = MaxTestCases;
        }

        string input;
        string[] gameValues = new string[3];
        ulong x1, x2, x3;
        BigInt100 b1, b2, b3;

        for (int i = 0; i < numTestCases; ++i)
        {
            //read a test case
            if (string.IsNullOrEmpty(input = InputMethod()) ||
               (gameValues = input.Split(TestCaseValueSplitter)).Length != 3)
            {
                OutputMethod(BadTestCaseInputString);
                continue;
            }

            //shortcut for all values < ulong.MaxValue
            if (ulong.TryParse(gameValues[0], out x1) &&
               ulong.TryParse(gameValues[1], out x2) &&
               ulong.TryParse(gameValues[2], out x3))
            {
                if (firstPlayerWins(x1, x2, x3))
                {
                    OutputMethod(FirstPlayerWinsString);
                }
                else
                {
                    OutputMethod(SecondPlayerWinsString);
                }
            }
            //general case
            else if (BigInt100.TryParse(gameValues[0], out b1) &&
                     BigInt100.TryParse(gameValues[1], out b2) &&
                     BigInt100.TryParse(gameValues[2], out b3))
            {
                if (firstPlayerWins(b1, b2, b3))
                {
                    OutputMethod(FirstPlayerWinsString);
                }
                else
                {
                    OutputMethod(SecondPlayerWinsString);
                }
            }
            //Bad test Case
            else
            {
                OutputMethod(BadTestCaseInputString);
            }
        }
    }

    //Evaluate 1 complete test case for 3 values in range 1 - ulong.MaxValue
    private static bool firstPlayerWins(ulong x1, ulong x2, ulong x3)
    {
        //Evaluate special cases:
        //triple: player 2 wins
        if (x1 == x2 && x2 == x3)
        {
            return false;
        }
        //double: player 1 wins
        if (x1 == x2 || x1 == x3 || x2 == x3)
        {
            return true;
        }

        //find absolute differences
        ulong d12 = x1 > x2 ? x1 - x2 : x2 - x1;
        ulong d13 = x1 > x3 ? x1 - x3 : x3 - x1;
        ulong d23 = x2 > x3 ? x2 - x3 : x3 - x2;

        //find averages
        ulong avg12 = (x1 + x2) / 2UL;
        ulong avg13 = (x1 + x3) / 2UL;
        ulong avg23 = (x2 + x3) / 2UL;

        //find set of differences that represent playable moves
        deltasULong.Clear();
        if (avg12 != x3)
        {
            deltasULong.Add(d12);
        }
        if (avg13 != x2)
        {
            if (!deltasULong.Contains(d13))
            {
                deltasULong.Add(d13);
            }
        }
        if (avg23 != x1)
        {
            if (!deltasULong.Contains(d23))
            {
                deltasULong.Add(d23);
            }
        }

        return checkFirstPlayerWinsRanges(deltasULong);
    }

    //Evaluates 1 complete test case for any 3 values in range 1 - 10^100
    private static bool firstPlayerWins(BigInt100 b1, BigInt100 b2, BigInt100 b3)
    {
        //Evaluate special cases:
        //triple: player 2 wins
        if (b1 == b2 && b2 == b3)
        {
            return false;
        }
        //double: player 1 wins
        if (b1 == b2 || b1 == b3 || b2 == b3)
        {
            return true;
        }

        //find differences
        BigInt100 D12 = b1 - b2;
        BigInt100 D13 = b1 - b3;
        BigInt100 D23 = b2 - b3;

        //find averages
        BigInt100 avg12 = (b1 + b2) / 2;
        BigInt100 avg13 = (b1 + b3) / 2;
        BigInt100 avg23 = (b2 + b3) / 2;

        //find set of differences that represent playable moves
        //split into 2 sets by size of difference
        deltasULong.Clear();
        deltasBigInteger.Clear();
        if (avg12 != b3)
        {
            if (D12 > BigInt100.ULongMax)
            {
                deltasBigInteger.Add(D12);
            }
            else
            {
                ulong d12 = (ulong)D12;
                deltasULong.Add(d12);
            }
        }
        if (avg13 != b2)
        {
            if (D13 > BigInt100.ULongMax)
            {
                if (!deltasBigInteger.Contains(D13))
                {
                    deltasBigInteger.Add(D13);
                }
            }
            else
            {
                ulong d13 = (ulong)D13;
                if (!deltasULong.Contains(d13))
                {
                    deltasULong.Add(d13);
                }
            }
        }
        if (avg23 != b1)
        {
            if (D23 > BigInt100.ULongMax)
            {
                if (!deltasBigInteger.Contains(D23))
                {
                    deltasBigInteger.Add(D23);
                }
            }
            else
            {
                ulong d23 = (ulong)D23;
                if (!deltasULong.Contains(d23))
                {
                    deltasULong.Add(d23);
                }
            }
        }

        return checkFirstPlayerWinsRanges(deltasULong) || checkFirstPlayerWinsRanges(deltasBigInteger);
    }

    //Checks differences against memoized ranges, then calculates new ranges
    //up to limit of ulong
    private static bool checkFirstPlayerWinsRanges(List<ulong> differences)
    {
        ulong min = 0;
        ulong max = 0;

        //Check memoized ranges for wins
        for (int i = 0; i < smallRangeList.Count; ++i)
        {
            min = smallRangeList[i].Min;
            max = smallRangeList[i].Max;
            for (int j = differences.Count - 1; j >= 0; --j)
            {
                //difference is between winning ranges
                if (differences[j] < min)
                {
                    differences.RemoveAt(j);
                    if (differences.Count == 0)
                    {
                        return false;
                    }
                }
                //difference is in a winning range
                else if (differences[j] <= max)
                {
                    return true;
                }
            }
        }

        if (isSmallRangeCalculated)
        {
            return false;
        }

        //calculate ranges, memoize, check new ranges
        while (differences.Count > 0)
        {
            //if the next min calculation will overflow min, stop
            if (min > SmallRangeTopMinValue)
            {
                isSmallRangeCalculated = true;
                return false;
            }

            //the heart of the algorithm, derived from examining cases
            min = (4UL * min) - 2UL;
            max = max > SmallRangeTopMaxValue ? SmallRangeAbsoluteMax : (4UL * max) + 2UL;

            //memoize
            smallRangeList.Add(new Range<ulong>(min, max));

            //check the new range
            for (int i = differences.Count - 1; i >= 0; --i)
            {
                if (differences[i] < min)
                {
                    differences.RemoveAt(i);
                }
                else if (differences[i] <= max)
                {
                    return true;
                }
            }
        }

        return false;
    }

    //Used in case a big number case happens first, to compute the small range
    //so that the big range can be seeded
    private static void calculateSmallRange()
    {
        //find the last calculated range in small ranges
        int lastIndex = smallRangeList.Count - 1;
        ulong min = smallRangeList[lastIndex].Min;
        ulong max = smallRangeList[lastIndex].Max;

        while (true)
        {
            //if next range will overflow min, stop
            if (min > SmallRangeTopMinValue)
            {
                isSmallRangeCalculated = true;
                return;
            }

            //calculate next range
            min = (4UL * min) - 2UL;
            max = max > SmallRangeTopMaxValue ? SmallRangeAbsoluteMax : (4UL * max) + 2UL;

            //memoize
            smallRangeList.Add(new Range<ulong>(min, max));
        }
    }

    //works only if small range is calculated;
    //only use if large range list is empty
    private static void seedBigRange()
    {
        //find seed min for big range
        int lastIndex = smallRangeList.Count - 1;
        BigInt100 Min = new BigInt100(smallRangeList[lastIndex].Min);

        //find seed max for big range
        //recalculate max if at end of small range,
        //as max will have overflowed
        BigInt100 Max;
        ulong max = smallRangeList[lastIndex].Max;
        if (max == SmallRangeAbsoluteMax)
        {
            ulong lastMax = smallRangeList[lastIndex - 1].Max;
            Max = (new BigInt100(lastMax) * 4UL) + 2UL;
        }
        else
        {
            Max = new BigInt100(max);
        }

        //seed large range
        largeRangeList.Add(new Range<BigInt100>(Min, Max));
    }

    private static bool checkFirstPlayerWinsRanges(List<BigInt100> differences)
    {
        if (!isSmallRangeCalculated)
        {
            calculateSmallRange();
        }
        if (largeRangeList.Count == 0)
        {
            seedBigRange();
        }

        BigInt100 min = null;
        BigInt100 max = null;

        //check differences against memoized big range
        for (int i = 0; i < largeRangeList.Count; ++i)
        {
            min = largeRangeList[i].Min;
            max = largeRangeList[i].Max;
            for (int j = differences.Count - 1; j >= 0; --j)
            {
                //difference between winning ranges
                if (differences[j] < min)
                {
                    differences.RemoveAt(j);
                    if (differences.Count == 0)
                    {
                        return false;
                    }
                }
                //difference in winning range
                else if (differences[j] <= max)
                {
                    return true;
                }
            }
        }

        if (isLargeRangeCalculated)
        {
            return false;
        }

        //calculate ranges, memoize, check new ranges
        while (differences.Count > 0)
        {
            //no need to check overflow
            //LargeRangeMax is a problem constant
            min = (min * 4UL) - 2UL;
            if (min > LargeRangeMax)
            {
                isLargeRangeCalculated = true;
                return false;
            }

            //No need to check overflow
            max = (max * 4UL) + 2UL;

            //memoize
            largeRangeList.Add(new Range<BigInt100>(min, max));

            for (int i = differences.Count - 1; i >= 0; --i)
            {
                //between winning ranges
                if (differences[i] < min)
                {
                    differences.RemoveAt(i);
                }
                //in winning range
                else if (differences[i] <= max)
                {
                    return true;
                }
            }
        }

        return false;
    }
}

//Created this struct because System.Tuple not supported by Sphere Engine
//Used to represent winning ranges of differences in problem space
public struct Range<T>
{
    public readonly T Min;
    public readonly T Max;

    public Range(T min, T max)
    {
        Min = min;
        Max = max;
    }
}

//Roled my own quasi arbitrary precision big integer
//because System.Numerics.BigInteger not supported by Sphere Engine
//
//Represents numbers up to 10^100
//Uses 1 ulong per 15 digit base 10 block
public class BigInt100
{
    //used for cast operator
    public static readonly BigInt100 ULongMax;
    //problem dependent constant
    public static readonly BigInt100 TenE100;

    //constants needed for number representation
    private static readonly int Blocks = 7;
    private static readonly int BlockLength = 15;
    private static readonly ulong BlockMask;

    static BigInt100()
    {
        BlockMask = 1UL;
        for (int i = 0; i < BlockLength; ++i)
        {
            BlockMask *= 10UL;
        }

        ULongMax = new BigInt100(ulong.MaxValue);

        //Block Mask is 1e15, or 1 in 106th digit
        //Need 1 in 101st digit, or 1e10 in most signigicant block
        //Calculation is a little artificial, and not scalable
        //with other BigInt100 static constants
        TenE100 = new BigInt100();
        ulong highDigit = BlockMask;
        for (int i = 0; i < 5; ++i)
        {
            highDigit /= 10;
        }
        TenE100.blockArray[Blocks - 1] = highDigit;
    }

    private ulong[] blockArray;

    public BigInt100()
    {
        blockArray = new ulong[Blocks];
    }

    public BigInt100(ulong u)
        : this()
    {
        blockArray[0] = u % BlockMask;
        blockArray[1] = u / BlockMask;
    }

    public BigInt100(string s)
        : this()
    {
        initialize(s, this);
    }

    //A copy constructor!
    //Lame excuse: Class semantics were easier for me 
    //to write than Struct semantics
    public BigInt100(BigInt100 b)
        : this()
    {
        for (int i = 0; i < Blocks; ++i)
        {
            this.blockArray[i] = b.blockArray[i];
        }
    }

    //Bad input: produces an undefined value
    //Overflow: parses the lease significant 105 digits
    public static bool TryParse(string s, out BigInt100 result)
    {
        result = new BigInt100();
        return initialize(s, result);
    }

    //Needed initialize to maintain .Net TryParse semantics
    //stores least significant 15 digits at position 0
    private static bool initialize(string s, BigInt100 result)
    {
        int startIndex = s.Length;
        int length = BlockLength;
        for (int i = 0; i < Blocks; ++i)
        {
            startIndex -= BlockLength;
            if (startIndex < 0)
            {
                startIndex = 0;
                length = s.Length - (BlockLength * i);
            }
            //Value is undefined if input is bad
            if (!ulong.TryParse(s.Substring(startIndex, length), out result.blockArray[i]))
            {
                return false;
            }
            if (startIndex == 0)
            {
                break;
            }
        }
        return true;
    }

    //returns ulong.Maxvalue if b is out of range
    public static explicit operator ulong(BigInt100 b)
    {
        if (b > ULongMax)
        {
            return ulong.MaxValue;
        }

        return b.blockArray[1] * BlockMask + b.blockArray[0];
    }

    public static implicit operator BigInt100(ulong u)
    {
        return new BigInt100(u);
    }

    //implemented as abs(difference)
    public static BigInt100 operator -(BigInt100 x1, BigInt100 x2)
    {
        return x1 > x2 ? difference(x1, x2) : difference(x2, x1);
    }

    //subtraction for x1 > x2
    private static BigInt100 difference(BigInt100 x1, BigInt100 x2)
    {
        BigInt100 result = new BigInt100();
        ulong borrow = 0UL;
        for (int i = 0; i < Blocks; ++i)
        {
            if ((x1.blockArray[i] - borrow) < x2.blockArray[i])
            {
                result.blockArray[i] = x1.blockArray[i] - borrow + BlockMask - x2.blockArray[i];
                borrow = 1UL;
            }
            else
            {
                result.blockArray[i] = x1.blockArray[i] - borrow - x2.blockArray[i];
                borrow = 0UL;
            }
        }
        return result;
    }

    //only implementing for y = single digit
    public static BigInt100 operator +(BigInt100 x, ulong y)
    {
        BigInt100 result = new BigInt100();
        ulong sum;
        ulong carry = y;
        for (int i = 0; i < Blocks; ++i)
        {
            sum = x.blockArray[i] + carry;
            result.blockArray[i] = sum % BlockMask;
            carry = sum / BlockMask;
        }
        return result;
    }

    public static BigInt100 operator +(BigInt100 x1, BigInt100 x2)
    {
        BigInt100 result = new BigInt100();
        ulong carry = 0UL;
        ulong sum;
        for (int i = 0; i < Blocks; ++i)
        {
            sum = x1.blockArray[i] + x2.blockArray[i] + carry;
            result.blockArray[i] = sum % BlockMask;
            carry = sum / BlockMask;
        }
        return result;
    }
    //only implementing for y = single digit, only used for *4
    public static BigInt100 operator *(BigInt100 x, ulong y)
    {
        BigInt100 result = new BigInt100();
        ulong carry = 0UL;
        ulong product;
        for (int i = 0; i < Blocks; ++i)
        {
            product = (x.blockArray[i] * y) + carry;
            result.blockArray[i] = product % BlockMask;
            carry = product / BlockMask;
        }
        return result;
    }

    //only implemented for y = single digit
    //implements int-style division: remainder is dropped
    //divide by 0 returns 0 to avoid exceptions
    public static BigInt100 operator /(BigInt100 x, ulong y)
    {
        BigInt100 result = new BigInt100();
        if (y == 0UL)
        {
            return result;
        }
        ulong carry = 0UL;
        ulong dividend;
        for (int i = Blocks - 1; i >= 0; --i)
        {
            dividend = (carry * BlockMask) + x.blockArray[i];
            result.blockArray[i] = dividend / y;
            carry = dividend % y;
        }
        return result;
    }

    public static bool operator ==(BigInt100 x1, BigInt100 x2)
    {
        for (int i = 0; i < Blocks; ++i)
        {
            if (x1.blockArray[i] != x2.blockArray[i])
            {
                return false;
            }
        }
        return true;
    }

    public static bool operator !=(BigInt100 x1, BigInt100 x2)
    {
        return !(x1 == x2);
    }

    public static bool operator >(BigInt100 x1, BigInt100 x2)
    {
        for (int i = Blocks - 1; i >= 0; --i)
        {
            if (x1.blockArray[i] > x2.blockArray[i])
            {
                return true;
            }
            if (x2.blockArray[i] > x1.blockArray[i])
            {
                return false;
            }
        }
        return false;
    }

    public static bool operator <(BigInt100 x1, BigInt100 x2)
    {
        return x2 > x1;
    }

    public static bool operator <=(BigInt100 x1, BigInt100 x2)
    {
        return !(x1 > x2);
    }

    public static bool operator >=(BigInt100 x1, BigInt100 x2)
    {
        return !(x1 < x2);
    }
}