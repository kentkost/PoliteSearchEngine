﻿using System;
using System.Data.HashFunction.xxHash;
using System.Data.HashFunction.Core;
using System.Collections.Generic;
using System.Security;
using System.Linq;
using System.Text;

/// <summary>
/// Thread safe Singleton
/// http://csharpindepth.com/articles/general/singleton.aspx
/// Use it by sating MathFunctions.Instance.[METHOD]
/// </summary>
namespace PoliteSearchEngine
{
    public sealed class MathFunctions
    {
        private static MathFunctions instance = null;
        private static readonly object padlock = new object();

        MathFunctions()
        {
        }

        public static MathFunctions Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new MathFunctions();
                    }
                    return instance;
                }
            }
        }


        public byte[] PseudoRandomByte(int iterations)
        {
            byte firstFib=1, secondFib=1, temp;
            byte maxValue = 255;
            byte[] pseudoRandomByte = new byte[4];

            for (int bytePos=0; bytePos < 4; bytePos++)
            {
                for (int i = 0; i < iterations; i++)
                {
                    temp = secondFib;
                    secondFib = (byte)(firstFib + secondFib);
                    firstFib = temp;
                    //Can make it faster by breaking when secondfib > maxValue
                }
                if (secondFib > maxValue)
                {
                    secondFib = (byte)(secondFib % maxValue);
                }
                pseudoRandomByte[bytePos] = secondFib;
            }
            return pseudoRandomByte;
        }

        public UInt64 ConvertByteToULong(byte[] value)
        {
            return BitConverter.ToUInt64(value, 0);
        }

        public byte[] HashString(String s, int iteration)
        {
            byte[] input = Encoding.ASCII.GetBytes(s);
            xxHashConfig config = new xxHashConfig() { HashSizeInBits = 64 };
            var fac = xxHashFactory.Instance.Create(config);
            byte[] x = fac.ComputeHash(input).Hash;
            //xxHash h = new xxHash(64);
            //byte[] x = h.ComputeHash(s);

            //lav et loop der bliver ved med at hashe den.
            for (int i = 0; i < iteration; i++)
            {
                x = fac.ComputeHash(x).Hash;
            }
            return x;
        }

        public UInt64 HashStringJaccard(String s, int iteration)
        {
            byte[] input = Encoding.ASCII.GetBytes(s);
            xxHashConfig config = new xxHashConfig() { HashSizeInBits = 64 };
            var fac = xxHashFactory.Instance.Create(config);
            byte[] x = fac.ComputeHash(input).Hash;

            //xxHash h = new xxHash();
            //byte[] x = fac.ComputeHash(s);

            for (int i = 0; i < iteration; i++)
            {
                x = fac.ComputeHash(x).Hash;
            }
            return BitConverter.ToUInt64(x, 0);
        }

        public UInt64 HashByteArray(byte[] b)
        {
            xxHashConfig config = new xxHashConfig() { HashSizeInBits = 64 };
            var fac = xxHashFactory.Instance.Create(config);

            //xxHash h = new xxHash(64);
            //byte[] x = h.ComputeHash(b);

            byte[] x = fac.ComputeHash(b).Hash;

            return BitConverter.ToUInt64(x, 0);
        }

        public bool TwoPagesAreIdentical(List<UInt64> superShingleSet1, List<UInt64> superShingleSet2, int criteria)
        {
            int count = 0;
            foreach (UInt64 x in superShingleSet1)
            {
                foreach (UInt64 y in superShingleSet2)
                {
                    if (x == y)
                    {
                        count++;
                    }
                    if (count >= criteria)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    } 
}

