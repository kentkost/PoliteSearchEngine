using System;
using System.Collections.Generic;

namespace PoliteSearchEngine
{
    public class PostingList
    {
        string _term;
        List<int> _appearsInDoc = new List<int>();

        public PostingList()
        {
        }

        public PostingList(string term, int id)
        {
            _term = term;
            _appearsInDoc.Add(id);
        }

        //Getters
        public string Term
        {
            get{ return _term; }
        }
        public List<int> DocAppearances
        {
            get{ return _appearsInDoc; }
        }

        public void AddToDocAppearances(int id)
        {
            _appearsInDoc.Add(id);
        }

    }
}

