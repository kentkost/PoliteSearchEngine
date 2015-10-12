using System;

namespace PoliteSearchEngine
{
    public class UrlStamp
    {
        string _url;
        int _depth;
        public UrlStamp(string url, int depth)
        {
            _url = url;
            _depth = depth;
        }

        public string Url
        {
            get{ return _url;}
            set{ _url = value; }
        }

        public int Depth
        {
            get{ return _depth; }
            set{ _depth = value; }
        }
    }
}

