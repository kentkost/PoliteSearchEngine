using System;
using System.Collections.Generic;

namespace PoliteSearchEngine
{
    public class DuplicatePageChecker
    {
        public DuplicatePageChecker()
        {
            
        }

        /// <summary>
        /// This returns all the words divided by spaces.
        /// So that they can be hashed, then shingled, then supershingled then hashed
        /// </summary>
        /// <returns>The words.</returns>
        public List<String> DivideWords(String phrase)
        {
            String temp = "";
            List <String> wordsDivided = new List<String>();

            for (int i = 0; i <phrase.Length; i++)
            {   
                if (!phrase[i].Equals(' '))
                {
                    temp += phrase[i];
                }
                if (phrase[i].Equals(' ') || i==phrase.Length -1)
                {
                    wordsDivided.Add(temp);
                    temp = "";
                }

            }
            return wordsDivided;
        }
    }
}

