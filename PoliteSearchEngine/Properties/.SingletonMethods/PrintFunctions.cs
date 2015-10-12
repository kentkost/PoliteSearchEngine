using System;
using System.Collections.Generic;

namespace PoliteSearchEngine
{
    public sealed class PrintFunctions
    {
        private static PrintFunctions instance = null;
        private static readonly object padlock = new object();

        PrintFunctions()
        {
        }

        public static PrintFunctions Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new PrintFunctions();
                    }
                    return instance;
                }
            }
        }

        public void PrintStringList(List<String> s)
        {
            foreach (string x in s)
            {
                Console.WriteLine(x);
            }
        }

        public void PrintUint64List(List<UInt64> ui)
        {
            foreach (UInt64 u in ui)
            {
                Console.WriteLine(u);
            }
        }
        public void PrintListOfBytearrays(List<byte[]> bytes)
        {
            foreach (byte[] b in bytes)
            {
                for (int i = 0; i < b.Length; i++)
                {
                    Console.Write(b[i] + ", ");
                }
                Console.WriteLine("");
            }    
        }
           
    }
}

