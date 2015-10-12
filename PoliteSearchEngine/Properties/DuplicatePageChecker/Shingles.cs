using System;
using System.Data.HashFunction;
using System.Collections.Generic;
using System.Security;
using System.Linq;

namespace PoliteSearchEngine
{
    public class Shingles
    {
        //Can easily make a lillt memory trick that makes sure that
        //only the _superShingles will stay in memory while the rest will be deallocted
        private List<String> _shingles = new List<string>();
        private List<byte[]> _sketch = new List<byte[]>();
        private List<UInt64> _superShingles = new List<UInt64>();

        public Shingles(HTMLParser x, int shingleStep, int sketchSize ,int numberOfSuperShingles)
        {
            //HTMLParser x = new HTMLParser(url);
            if (x.WordsOnPage.Count > 0)
            {
                _shingles = CreateShingles(x.WordsOnPage, shingleStep);
                _sketch = CreateSketch(_shingles, sketchSize);
                _superShingles = CreateSuperShingles(_sketch, numberOfSuperShingles);
            }
        }


        public List<String> CreateShingles(List<String> wordList, int shingleDistance)
        {
            List<String> Shingles = new List<String>();
            int shingleDistanceCheck=0;
            String shingle = "";

            for(int i=0; i<wordList.Count(); i++)
            {
                shingle = wordList[i];

                //Quick error handling to make sure we doesn't go outside of wordlist
                if (i + shingleDistance > wordList.Count()){
                    shingleDistanceCheck = wordList.Count;
                }
                else
                {
                    shingleDistanceCheck = i + shingleDistance;
                }

                for (int j = i + 1; j < shingleDistanceCheck; j++)
                {
                    shingle += wordList[j];
                }
                Shingles.Add(shingle);
            }

            return Shingles;
        }
            
        public List<byte[]> CreateSketch(List<String> shingles, int sketchSize)
        {
            List<byte[]> Sketch = new List<byte[]>();
            byte[] lowestHash, temp;

            for (int i = 0; i < sketchSize; i++)
            {
                lowestHash = MathFunctions.Instance.HashString(shingles[0], i);
                foreach (String s in shingles)
                {
                    temp = MathFunctions.Instance.HashString(s, i);
                    if (CompareTwoByteArray(temp,lowestHash))
                    {
                        lowestHash = temp;
                    }
                }
                Sketch.Add(lowestHash);
            }
            return Sketch;
        }

        //To be used when not creating supershingles. Look at return type
        public List<UInt64> CreateSketchJaccard(List<String> shingles, int sketchSize)
        {
            List<UInt64> Sketch = new List<UInt64>();
            UInt64 lowestHash, temp;

            for (int i = 0; i < sketchSize; i++)
            {
                lowestHash = MathFunctions.Instance.HashStringJaccard(shingles[0], i);
                foreach (String s in shingles)
                {
                    temp = MathFunctions.Instance.HashStringJaccard(s, i);
                    if (temp < lowestHash)
                    {
                        lowestHash = temp;
                    }
                }
                Sketch.Add(lowestHash);
            }
            return Sketch;
        }

        public static bool CompareTwoByteArray(byte[] tempByte, byte[]currentByte)
        {
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(tempByte);
                Array.Reverse(currentByte);
            }
            for (int i = 0; i < tempByte.Length; i++)
            {
                
                if (tempByte[i] < currentByte[i])
                {
                    return true;
                }
                else if(tempByte[i] > currentByte[i])
                {
                    return false;
                }
            }
            return false;
        }

        #region Supershingles  
        public List<UInt64> CreateSuperShingles(List<byte[]> sketch, int numberOfSuperShingles)
        {
            List<UInt64> superShingles = new List<UInt64>();
            int shingleStep = sketch.Count / numberOfSuperShingles, scanStep;
            byte[] temp;
            for(int i=0; i< sketch.Count; i++)
            {
                temp = sketch[i];
                if (i + shingleStep > sketch.Count)
                {
                    scanStep = sketch.Count;
                }
                else
                {
                    scanStep = i + shingleStep;
                }

                for(i=i+1; i < scanStep; i++)
                {
                    temp = temp.Concat(sketch[i]).ToArray();
                }
                superShingles.Add(MathFunctions.Instance.HashByteArray(temp));
            }
            return superShingles;

        }
        #endregion

        //We only use this when creating sketches of the type List<UInt64>.
        //But we prefer SUPERSHINGLES!
        public int Jaccard(List<ulong> firstSketch, List<ulong> secondSketch)
        {
            int count=0;
            foreach (ulong u in firstSketch)
            {
                foreach (ulong l in secondSketch)
                {
                    if (u == l)
                    {
                        count++;
                    }
                }
            }
            return count;
        }
            
        public bool IsIdenticalTo(List<UInt64> superShingleSet2, int criteria)
        {
            int count = 0;
            foreach (UInt64 x in _superShingles)
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

        #region Getters and setters
        public List<String> ShingleList
        {
            get{return _shingles;}
        }

        public List<byte[]> SketchList
        {
            get{ return _sketch; }
        }

        public List<UInt64> SuperShingles
        {
            get{ return _superShingles; }
        }
        #endregion

    }
}