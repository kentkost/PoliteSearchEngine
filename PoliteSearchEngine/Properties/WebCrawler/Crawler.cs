using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Net;
using System.Text.RegularExpressions;
using System.Linq;

//This class should contain a list of robots.
//A list of pages
//A URL FILTER
//A URL DUPLICATE REMOVER
//A URL Frontier

namespace PoliteSearchEngine
{
    public class Crawler
    {
        List<UrlStamp> urlFrontier = new List<UrlStamp>();
        List<Page> pages = new List<Page>();
        List<Robot> robots = new List<Robot>();
        List<TermVector> completeTermVector = new List<TermVector>();
        int numbPages=0;
        Page tempPage; 
        int numberOfPages=0;

        public Crawler(int depth, int maxPages)
        {
            numberOfPages = maxPages;
            SetStartFrontier();
            Crawl(depth);
        }

        private void Crawl(int depth)
        {
            int robotIndex;
            string currentUrl="";
            UrlStamp currentCrawl;

            if (!(urlFrontier.Count > 0))
            {
                Console.WriteLine("No more Urls to check! Crawler Finished");
            }
            else
            {
                currentCrawl = urlFrontier[0];
            

                if (currentCrawl.Depth > depth)
                {
                    RemoveCurrentUrlFromFrontier(urlFrontier[0].Url);
                    Crawl(depth);
                    return;
                }

                //currentCrawl.Url = ErrorHandleUrl(currentCrawl.Url);
                Console.WriteLine("Attempting to Crawl: " + currentCrawl.Url);

                //normalize s in a temp string, so that the robots.txt can be found
                String tempS = NormalizeURL(currentCrawl.Url);
                if (tempS == "not valid")
                {
                    Console.WriteLine("URL IS NOT VALID: "+tempS);
                    RemoveCurrentUrlFromFrontier(urlFrontier[0].Url);
                    Crawl(depth);
                    return;
                }
                //Find the rules or create a new robot with the rules.
                robotIndex = FindRobotsTxt(tempS);
                Console.WriteLine("Found robot!");

                //now get absolute url. I forgot why. Something about avoiding duplicate pages, but shouldn't 
                //Shingles avoid that for us?
                currentUrl = AbsoluteUrl(currentCrawl.Url);

                //Now check if the current url is allowed to be parsed
                if (!RobotsTxtAllowsIt(currentCrawl.Url, robotIndex))
                {
                    Console.WriteLine("\trobots.txt did not allow this page");
                    RemoveCurrentUrlFromFrontier(urlFrontier[0].Url);
                    Crawl(depth);
                    return;
                }
                Console.WriteLine("Robot allows!");

                //check if the currentURL has already been parsed within the last year
                if (HasAlreadyBeenParsed(currentUrl))
                {
                    Console.WriteLine("\tHas been parsed before");
                    RemoveCurrentUrlFromFrontier(urlFrontier[0].Url);
                    Crawl(depth);
                    return;
                }
                Console.WriteLine("Has not been parsed before");
                //Then create a temp page with Dated Visited, links, shingles, depth

                //BE POLITE: IF THE PAGE TOOK X seconds to load. IT will WAIT 10*X seconds to load the next page
                //timer start. In here. Get links, make shingles, read paragraphs

                System.Threading.Thread.Sleep(500);
                tempPage = new Page(new UrlStamp(currentUrl, currentCrawl.Depth + 1), numbPages);
                //timer end

                //check if the temppage matched the shingles of another page in pages or if page is empty
                if (tempPage.SuperShingles.Count==0 || (MatchesOtherPageSuperShingles(tempPage)))
                {
                    RemoveCurrentUrlFromFrontier(urlFrontier[0].Url);
                    Crawl(depth);
                    return;
                }

                //REmove duplicate urls from tempPage(done in htmlparser), 
                //also those that are in urlFrontier already => Maybe not necessary because of urlsFrontier.Remove("Finished url");
                tempPage.UrlsOnSite = RemoveDuplicateURLS(tempPage.UrlsOnSite);

                //complete irrelevant since we check the robots at the begining.
                //tempPage.UrlsOnSite = RemoveUrlsAccRobot(tempPage.UrlsOnSite, robotIndex);

                //NOW INDEX PAGE AND ADD THE REMAINING URLS to the FRONTIER
                MergeUrlFrontierPageUrls(ref urlFrontier, tempPage.UrlsOnSite);

                pages.Add(tempPage);
                Console.WriteLine("\tPage added");
                RemoveCurrentUrlFromFrontier(urlFrontier[0].Url);
                numbPages++;

                //Make complete termlist here.
                MergeTermListFromPage(tempPage);


                if (numbPages > numberOfPages)
                {
                    Console.WriteLine("Reached Page limit. Crawler Finished");
                }
                else
                {
                    Crawl(depth);
                }
            }
        }

        private void MergeTermListFromPage(Page p)
        {
            completeTermVector = completeTermVector.Concat(p.Terms).ToList();
            completeTermVector = SortAndDistinctTermList(completeTermVector);
        }

        //Has to be changed since it doesn't count the freq of a word.
        private List<TermVector> SortAndDistinctTermList(List<TermVector> termList)
        {
            List<TermVector> termVectors= new List<TermVector>();
            //Sort and get frequency. yaaaay! I made lambda expression. Je suis le professional!
            List<TermVector> x = termList.OrderBy(tL => tL.TermString).ToList();

            int i=0, j = 1;
            double termScore=0;
            while (j < x.Count)
            {
                if (x[i].TermString == x[j].TermString)
                {
                    x[i].TermFreq += x[j].TermFreq;
                    j++;
                }
                else
                {   
                    //Console.WriteLine("term: " + x[i].TermString +" freq: "+ x[i].TermFreq);
                    termScore = Math.Log10(Convert.ToDouble(pages.Count)/Convert.ToDouble(x[i].TermFreq));
                    termVectors.Add(new TermVector(x[i].TermString, x[i].TermFreq, termScore));
                    i = j;
                    j++;
                }
                if (j == x.Count)
                {
                    termScore = Math.Log10(Convert.ToDouble(pages.Count)/Convert.ToDouble(x[i].TermFreq));

                    termVectors.Add(new TermVector(x[i].TermString, x[i].TermFreq, termScore));
                }
            }
            /*foreach (TermVector t in termVectors)
            {
                Console.WriteLine("term: " + t.TermString +" termfreq "+t.TermFreq + " freqscore: " + t.TermScore);
            }*/
            return termVectors;
        }

        private string ErrorHandleUrl(string url)
        {
            for (int i = 0; i < url.Length; i++)
            {
                if (url[i] == '/' && url[i+1]=='/')
                {
                    url=url.Remove(0, i);
                    break;
                }
            }
            return "https:" + url;
        }

        //My very first lamda expression.
        private void RemoveCurrentUrlFromFrontier(string url)
        {
            urlFrontier.RemoveAll(i => i.Url == url);       
        }



        //NOt thoroughly tested
        private List<UrlStamp> RemoveUrlsAccRobot(List<UrlStamp> urlsPage, int robotIndex)
        {
            foreach (UrlStamp us in urlsPage.ToList())
            {
                if (!RobotsTxtAllowsIt(us.Url, robotIndex))
                {
                    urlsPage.Remove(us);
                }
            }
            return urlsPage;
        }

        //NOT TESTED.
        private void MergeUrlFrontierPageUrls(ref List<UrlStamp> urlFrontier, List<UrlStamp> urlsPage)
        {
            urlFrontier = urlFrontier.Concat(urlsPage).ToList();
        }

        //HAS NOT BEEN thoroughly TESTED. You could use a lambda expression but fuck it
        private List<UrlStamp> RemoveDuplicateURLS(List<UrlStamp> urlStamps)
        {
            foreach (UrlStamp us in urlStamps.ToList())
            {
                foreach (UrlStamp ufs in urlFrontier.ToList())
                {
                    if (us.Url == ufs.Url)
                    {
                        urlStamps.Remove(us);
                    }
                }
            }
            return urlStamps;
        }

        //has not been thoroughly tested
        private bool MatchesOtherPageSuperShingles(Page tempPage)
        {
            foreach (Page p in pages)
            {
                if (tempPage.GetShingles.IsIdenticalTo(p.SuperShingles, 10))
                {
                    Console.WriteLine("Page is identical to: " + p.Url);
                    return true;
                }
            }
            return false;
        }

        //has not been thoroughly tested
        private bool MatchesOtherPageSS(Page tempPage)
        {
            foreach (Page p in pages)
            {
                if (MathFunctions.Instance.TwoPagesAreIdentical(tempPage.SuperShingles, p.SuperShingles, 2))
                {
                    return true;
                }
            }
            return false;
        }

        //HAS NOT BEEN TESTED
        private bool HasAlreadyBeenParsed(string url)
        {
            foreach(Page p in pages)
            {
                if (p.Url == url)
                {
                    if (p.DateVisited.AddYears(1) > DateTime.Now)
                    {
                        return true;
                   }
                }
            }
            return false;
        }

        public void SetStartFrontier()
        {
            //online
            urlFrontier.Add(new UrlStamp("https://en.wikipedia.org/wiki/Portal:Current_events", 0));
            urlFrontier.Add(new UrlStamp("https://www.reddit.com/r/worldnews/", 0));
            urlFrontier.Add(new UrlStamp("http://www.bbc.com/news", 0));
            urlFrontier.Add(new UrlStamp("http://edition.cnn.com/", 0));
            //urlFrontier.Add(new UrlStamp("http://www.bbc.com/news/world-middle-east-26116868", 0));
            //urlFrontier.Add(new UrlStamp("http://www.reddit.com/", 0));
            //urlFrontier.Add(new UrlStamp("http://www.wikipedia.org",0));
            //urlFrontier.Add(new UrlStamp("http://www.codedread.com/testbed/pure-html.html", 0));
            //urlFrontier.Add(new UrlStamp("http://www.hanshuttel.dk/wordpress/",  0));

            //offline test
            //urlFrontier.Add(new UrlStamp("/testfolder/eb.htm", 0));
        }
            
        private String NormalizeURL(String url)
        {
            //Temporary fix, because I need HTTP. Or else it will try to look up the address locally.
            if (url.ToLower()[4] == 's')
            {
                return "https://" + new UriBuilder(new Uri(url)).Host;    
            }
            else if (url.ToLower()[4] == ':')
            {
                return "http://" + new UriBuilder(new Uri(url)).Host;
            }
            else if (url[0] != '/' || url[0] != ':')
            {
                return "not valid";
            }
            else
            {
                for (int i = 0; i < url.Length; i++)
                {
                    if (i == url.Length - 3)
                    {
                        return "not valid";
                    }
                    if (url.ToLower()[i] == 'w' && url.ToLower()[i + 1] == 'w' && url.ToLower()[i + 2] == 'w')
                    {
                        url= url.Remove(0, i);

                    }
                }
                return "http://" + new UriBuilder(new Uri(url)).Host;
            }

        }
        
        /// <summary>
        /// http://stackoverflow.com/questions/704956/getting-the-redirected-url-from-the-original-url
        /// </summary>
        /// <returns>The URL.</returns>
        /// <param name="url">URL.</param>
        private string AbsoluteUrl(string url)
        {
            if(string.IsNullOrWhiteSpace(url))
                return url;

            int maxRedirCount = 8;  // prevent infinite loops
            string newUrl = url;
            do
            {
                HttpWebRequest req = null;
                HttpWebResponse resp = null;
                try
                {
                    req = (HttpWebRequest) HttpWebRequest.Create(url);
                    req.Method = "HEAD";
                    req.AllowAutoRedirect = false;
                    resp = (HttpWebResponse)req.GetResponse();
                    switch (resp.StatusCode)
                    {
                        case HttpStatusCode.OK:
                            return newUrl;
                        case HttpStatusCode.Redirect:
                        case HttpStatusCode.MovedPermanently:
                        case HttpStatusCode.RedirectKeepVerb:
                        case HttpStatusCode.RedirectMethod:
                            newUrl = resp.Headers["Location"];

                            if (newUrl == null)
                                return url;

                            if (newUrl.IndexOf("://", System.StringComparison.Ordinal) == -1)
                            {
                                // Doesn't have a URL Schema, meaning it's a relative or absolute URL
                                Uri u = new Uri(new Uri(url), newUrl);
                                newUrl = u.ToString();
                            }
                            break;
                        default:
                            return newUrl;
                    }
                    url = newUrl;
                }
                catch (WebException)
                {
                    // Return the last known good URL
                    return newUrl;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return null;
                }
                finally
                {
                    if (resp != null)
                        resp.Close();
                }
            } while (maxRedirCount-- > 0);

            return newUrl;
        }

        /// <summary>
        /// Robotses the text allows it. Not testes
        /// </summary>
        /// <returns><c>true</c>, if text allows it was robotsed, <c>false</c> otherwise.</returns>
        /// <param name="currentUrl">Current URL.</param>
        private bool RobotsTxtAllowsIt(String currentUrl, int relevantRobot)
        {
            bool allowed = true;

            if (relevantRobot < 0)
            {
                return true;
            }

            foreach (string s in robots[relevantRobot].RobotTxt)
            {
                string temp;
                if (s.ToLower().StartsWith("allow"))
                {
                    temp = TrimAllowDisallow(s);
                    if (currentUrl.Contains(temp))
                    {
                        return true;
                    }
                }
                else if (s.ToLower().StartsWith("disallow"))
                {
                    temp = TrimAllowDisallow(s);
                    if (currentUrl.Contains(temp))
                    {
                        allowed = false;
                    }
                }
            }

            return allowed;  
        }

        private string TrimAllowDisallow(string guideLine)
        {
            //Remove "disallow:" and "allow:"
            for (int i = 0; i < guideLine.Length; i++)
            {
                if (guideLine[i] == ':')
                {
                    guideLine = guideLine.Remove(0, i+1);
                    break;
                }
            }

            //removes potenial idiot comments
            for (int i = 0; i < guideLine.Length; i++)
            {
                if (guideLine[i] == '#')
                {
                    guideLine = guideLine.Remove(i);
                    break;
                }
            }

            //removes white space
            guideLine = Regex.Replace(guideLine, @"\s+", "");
            return guideLine;
        }

        //Should be changed to timespan
        private int FindRobotsTxt(String uri)
        {
            for(int i=0; i< robots.Count; i++)
            {
                if (robots[i].SiteName == uri)
                {
                    if (robots[i].DateArchived.AddYears(1) < DateTime.Now)
                    {
                        break;
                    }
                    else
                    {
                        return i;   
                    }
                }
            }
            robots.Add(new Robot(uri));
            return robots.Count - 1;
        }

        //GETTERS AND SETTERS FROM HERE
        public List<Page> Pages
        {
            get{ return pages; }
        }

        public List<UrlStamp> UrlFrontier
        {
            get{ return urlFrontier; }
        }

        public List<TermVector> CompleteTermVector
        {
            get{ return completeTermVector; }
        }

    }
}

