using System;
using System.Net;
using System.Xml.XPath;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Configuration;
using System.Text;
using System.Linq;

namespace PoliteSearchEngine
{
    public class HTMLParser
    {
        
        private List<String> _wordsOnPage;
        private List<UrlStamp> _aHref;
        private static List<string> stopWords = new List<string>() { "a", "about", "above", "after", "again", "against", "all", "am", "an", "and", "any", "are", "aren't", "as", "at", "be", "because", "been", "before", "being", "below", "between", "both", "but", "by", "cant", "cannot", "could", "couldnt", "did", "didnt", "do", "does", "doesnt", "doing", "dont", "down", "during", "each", "few", "for", "from", "further", "had", "hadnt", "has", "hasn't", "have", "havent", "having", "he", "hed" , "hes", "her", "here", "heres", "hers", "herself", "him", "himself", "his", "how", "hows", "im", "ive", "if", "in", "into", "is", "isnt", "its", "itself", "lets", "me", "more", "most", "mustnt", "my", "myself", "no", "nor", "not", "of", "off", "on", "once", "only", "or", "other", "ought", "our", "ours", "ourselves", "out", "over", "own", "same", "shant", "she", "shes", "should", "shouldnt", "so", "some", "such", "than", "that", "thats", "the", "their", "theirs", "them", "themselves", "then", "there", "theres", "these", "they", "theyd", "theyll", "theyre", "theyve", "this", "those", "through", "to", "too", "under", "until", "up", "very", "was", "wasnt", "we", "wed", "weve", "were", "werent", "what", "whats", "when", "whens", "where", "wheres", "which", "while", "who", "whom", "why", "whys", "with", "wont", "would", "wouldnt", "you", "youd", "youll", "youre", "youve", "your", "yours", "yourself", "yourselves"};
        private HtmlNode body;
        private List<TermVector> _terms = new List<TermVector>();

        public HTMLParser(List<string> words)
        {
            MakeTermList(words);
        }

        public HTMLParser(UrlStamp url)
        {
            body = null;
            _wordsOnPage = new List<String>();
            
            HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();

            // There are various options, set as needed
            htmlDoc.OptionFixNestedTags=true;
            //I can get the character set by looking into the meta tags in the header and look for charset
            //CultureInfo pt = CultureInfo.GetCultureInfo("da-DK");

            //online
            /*HtmlWeb hw = new HtmlWeb();
            try{
                htmlDoc = hw.Load(url.Url);
            }catch(HtmlWebException ex){
                Console.WriteLine("htmlParser.cs " + ex.Message);
            }*/

            //http://www.webr2.com/htmlagilitypack-webget-load-gives-error-object-reference-not-set-to-an-instance-of-an-object/
            try{
                var temp = new Uri(url.Url);
                var request = (HttpWebRequest)WebRequest.Create(temp);
                request.Method = "GET";
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    using (var stream = response.GetResponseStream())
                    {
                        htmlDoc.Load(stream, Encoding.GetEncoding("iso-8859-9"));
                    }
                }
            }catch(WebException ex){
                Console.WriteLine(ex.Message);
            }
                

            //offline
            //htmlDoc.Load(url.Url, Encoding.ASCII,true);

            // ParseErrors is an ArrayList containing any errors from the Load statement
            if (htmlDoc.ParseErrors == null) { }else {}

            //We're only interested in the bodytag for now. later comes keywords
            if (htmlDoc.DocumentNode != null) {
                //Find the Schedule in the html doc
                body = htmlDoc.DocumentNode.SelectSingleNode ("//body");
            }

            if (body != null)
            {
                try{
                //Console.WriteLine(body.InnerText);
                    _wordsOnPage= ReadParagraphs(body);
                    _aHref = ReadHyperLinkReferences(body, url.Depth);
                    MakeTermList(_wordsOnPage);
                }catch(Exception ex){Console.WriteLine("HTMLParser.cs "+ex);}
            }
        }

        //Should be moved to a singletonClass or move it to TermVector.cs.
        //because we have to use this method for when making the termlsit for the search query
        private void MakeTermList(List<String> wordsOnPage)
        {
            string strTemp = "";
            foreach (string s in wordsOnPage)
            {
                int stopWordLoc = stopWords.BinarySearch(s);
                if (stopWordLoc >= 0)
                {
                    strTemp += s + " ";
                }
                else
                {
                    //Combined stopwords and a nonstopwords equals a term in this searchengine.
                    //Just because I want to be different.
                    if (strTemp.Length > 1)
                    {
                        _terms.Add(new TermVector(strTemp + s));
                        strTemp = "";
                    }      
                    _terms.Add(new TermVector(s));
                }
            }
            _terms = SortAndDistinctTermList(ref _terms);
        }


        private List<TermVector> SortAndDistinctTermList(ref List<TermVector> termList)
        {
            List<TermVector> termVectors= new List<TermVector>();
            //Sort and get frequency. yaaaay! I made lambda expression. Je suis le professional!
            var x = termList.OrderBy(tL => tL.TermString).GroupBy(l => l.TermString).Select(g => new
                {
                    term = g.Key,
                    freq = g.Select(l => l).Count(),
                });


            //Convert var x to Term List
            foreach (var tv in x)
            {
                double freqScore = 1 + (Math.Log10(Convert.ToDouble(tv.freq)));
                termVectors.Add(new TermVector(tv.term, tv.freq, freqScore));
            }
            return termVectors;
        }


        private List<String> ReadParagraphs(HtmlNode body)
        {
            List<String> textList = new List<String>();
            string[] skod = { ",", ".", "!", "?", "\'", ";", ":", "-", "(", ")", "#", "¤", "&", 
                "/" , "{","[", "]", "}","+","^", "<", ">", "§","_" };
            
            string cleaned ="";
            if (body.SelectNodes("//p") == null)
            {
                return new List<String>(){ };
            }
            foreach (HtmlNode h in body.SelectNodes("//p"))
            {
                //No nested paragraphs
                if (NumberOfParagraphsXpath(h.XPath) > 1)
                {
                    continue;
                }
                if (!String.IsNullOrWhiteSpace(h.InnerText))
                { 
                    cleaned = h.InnerText.ToLower();
                    //Optimise later. With sorted skod array and use binary search to see if it matches any in skod
                    for (int i = 0; i < skod.Length; i++)
                    {
                        cleaned = cleaned.Replace(skod[i], "");
                    }
                    //Do not clean on making shingles. However do do it when parsing
                    //cleaned = Regex.Replace(cleaned, "\\b" + string.Join("\\b|\\b", stopWords) + "\\b", "");

                    string[] splitCleaned = cleaned.Split(null);

                    if (splitCleaned.Length > 1)
                    {
                        foreach (string s in splitCleaned)
                        {
                            if (!String.IsNullOrWhiteSpace(s))
                            {
                                textList.Add(s);
                            }
                        }
                    }
                }
            }
            return textList;
        }

        private int NumberOfParagraphsXpath(String xpath)
        {
            string[] xpathSplit = xpath.Split(new string[] { "/" }, StringSplitOptions.None);
            int count = 0;
            foreach(string s in xpathSplit)
            {
                if (s.Length>3 && s[0]=='p' && s[1]=='[' && s[s.Length-1]==']') //REGEX HELP ME
                {
                    count++;
                }
            }
            return count; 
        }

        //Could include a prioritizer in this and then add a new variable to UrlStamp class
        //Actually a perfect place to add a score to the utlstamp. 
        //Since I have the parent url and the depth. Relevance vs. Freshness. Also tell how long the crawler should wait before parsing page
        private List<UrlStamp> ReadHyperLinkReferences(HtmlNode body, int depth)
        {
            List<UrlStamp> urlStamps = new List<UrlStamp>();
            List<string> urls = new List<string>();

            foreach (HtmlNode h in body.SelectNodes("//a"))
            {
                string link = h.GetAttributeValue("href", "");
                if (link != String.Empty && link.Contains("www."))
                {
                    string extension = link.Substring(link.LastIndexOf(".") + 1, link.Length - link.LastIndexOf(".") - 1);
                    switch (extension)
                    {
                        case "jpg":
                        case "css":
                        case "png":
                        case "ico":
                        case "rss":
                        case "ece":
                            continue;
                        default:
                            urls.Add(h.GetAttributeValue("href", ""));
                            break;
                    }

                }
            }

            urls=urls.Distinct().ToList();
            foreach (string s in urls)
            {
                urlStamps.Add(new UrlStamp(s, depth));
            }

            return urlStamps;
        }


        //GETTERS AND SETTERS FROM HERE
        public List<String> WordsOnPage
        {
            get{ return _wordsOnPage; }
        }

        public List<UrlStamp> hRef
        {
            get{ return _aHref; }
            set{ _aHref = value; }
        }

        public List<TermVector> Terms
        {
            get{ return _terms; }
            set{ _terms = value; }
        }
    }
}

