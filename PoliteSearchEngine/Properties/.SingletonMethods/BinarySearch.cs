using System;
using System.Collections.Generic;

namespace PoliteSearchEngine
{
    public sealed class BinarySearch
    {
        private static BinarySearch instance = null;
        private static readonly object padlock = new object();

        BinarySearch()
        {
        }

        public static BinarySearch Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new BinarySearch();
                    }
                    return instance;
                }
            }
        }

        public int BinarySearchTermVector(string value, List<TermVector> terms)
        {
            if (terms == null)
                throw new ArgumentNullException("list");


            Int32 lower = 0;
            Int32 upper = terms.Count - 1;

            while (lower <= upper)
            {
                Int32 middle = lower + (upper - lower) / 2;
                Int32 comparisonResult = String.Compare(value, terms[middle].TermString);
                if (comparisonResult == 0)
                    return middle;
                else if (comparisonResult < 0)
                    upper = middle - 1;
                else
                    lower = middle + 1;
            }

            return -1;
        }
    }
}

