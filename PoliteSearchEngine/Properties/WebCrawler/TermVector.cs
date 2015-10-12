using System;

namespace PoliteSearchEngine
{
    public class TermVector
    {
        int _frequency=0;
        String _term;
        double _freqScore;
        double wt;
        double normWeight;

        public TermVector(){}

        public TermVector(String term, int frequency)
        {
            _term = term;
            _frequency = frequency;
        }

        public TermVector(String term, int frequency, double freqScore)
        {
            _term = term;
            _frequency = frequency;
            _freqScore = freqScore;
        }

        public TermVector(string term)
        {
            _term = term;
        }
            

        public string TermString
        {
            get{ return _term; }
            set{ _term = value;}
        }

        public int TermFreq
        {
            get{ return _frequency; }
            set{ _frequency = value; }
        }

        public double TermScore
        {
            get{ return _freqScore; }
            set{ _freqScore = value; }
        }

        public double Weight
        {
            get{ return wt; }
            set{ wt = value; }
        }

        public double NormalizedWeight
        {
            get{ return normWeight; }
            set{ normWeight = value; }
        }

        public void IncrementFreq()
        {
            _frequency++;
        }

    }
}

