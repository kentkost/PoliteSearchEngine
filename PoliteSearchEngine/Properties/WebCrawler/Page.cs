using System;
using System.Collections.Generic;


namespace PoliteSearchEngine
{
    public class Page
    {
        HTMLParser parser;
        Shingles shingles;
        UrlStamp _url;
        DateTime _dateVisited;
        int _depth;
        int _ID;
        double _docLength=0;
        double _searchScore=0;
       
        public Page(UrlStamp url, int ID)
        {
            _url = url;
            parser = new HTMLParser(_url);
            Console.WriteLine("\tparser finished!");
            shingles = new Shingles(parser, 4, 84, 12);
            Console.WriteLine("\tshingles finished!");

           
            _dateVisited = DateTime.Now;
            _depth = url.Depth;
            _ID=ID;
        }

        //searchQuery. This way i don't have to create a new class. also I can use the termVector creator from before.
        public Page(string _searchQuery)
        {
            List<string> x = new List<string>();
            x = DivideWords(_searchQuery);
            parser = new HTMLParser(x);
            //Create vectorLength
            //Skal gøres i searchtable istedet.
        }

        private List<string> DivideWords(string searchQuery)
        {
            string[] skod = { ",", ".", "!", "?", "\'", ";", ":", "-", "(", ")", "#", "¤", "&", 
                "/" , "{","[", "]", "}","+","^", "<", ">", "§","_" };
            List<string> textList = new List<string>();
            string temp="";
            if (!String.IsNullOrWhiteSpace(searchQuery))
            { 
                searchQuery = searchQuery.ToLower();

                for (int i = 0; i < skod.Length; i++)
                {
                    searchQuery = searchQuery.Replace(skod[i], "");
                }

                for(int i=0; i<searchQuery.Length; i++)
                {

                    if (searchQuery[i] == ' ')
                    {
                        textList.Add(temp);
                        temp = "";
                    }
                    else
                    {
                        temp += searchQuery[i];
                    }
                    if (i == searchQuery.Length - 1 &&  (!string.IsNullOrEmpty(temp)))
                    {
                        textList.Add(temp);
                    }
                }

            }
            return textList;
        }
            

        public String Url
        {
            get{ return _url.Url; }
            set{ _url.Url = value; }
        }

        public int Depth
        {
            get { return _depth; }
        }

        public List<UInt64> SuperShingles
        {
            get{ return shingles.SuperShingles; }
        }

        public DateTime DateVisited
        {
            get{ return _dateVisited; }
        }

        public List<string> testShingleList
        {
            get{ return parser.WordsOnPage; }
        }

        public List<TermVector> Terms
        {
            get{ return parser.Terms; }
        }

        public List<UrlStamp> UrlsOnSite
        {
            
            get{ return parser.hRef; }
            set{ parser.hRef = value; }
        }

        public Shingles GetShingles
        {
            get{ return shingles; }
        }

        public int DocId
        {
            get{ return _ID; }
        }

        public double SearchScore
        {
            get{ return _searchScore; }
            set{ _searchScore = value; }
        }

        public double DocLength
        {
            get{ return _docLength;}
            set{ _docLength = value; }
        }
            
        public int BinarySearchTermVector(string value)
        {
            if (this.Terms == null)
                throw new ArgumentNullException("list");


            Int32 lower = 0;
            Int32 upper = this.Terms.Count - 1;

            while (lower <= upper)
            {
                Int32 middle = lower + (upper - lower) / 2;
                Int32 comparisonResult = String.Compare(value, this.Terms[middle].TermString);
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

