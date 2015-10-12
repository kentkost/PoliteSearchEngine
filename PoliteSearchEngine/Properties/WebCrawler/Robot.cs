using System;
using System.Timers;
using System.Collections.Generic;
using System.Net;
using System.IO;

namespace PoliteSearchEngine
{
    public class Robot
    {
        private string _siteURL;
        private List<String> _robotText;
        private DateTime _dayArchived;

        public Robot(){}

        public Robot(String uri)
        {
            _robotText = GetRobotsTxt(uri);
            _siteURL = uri;
            _dayArchived = DateTime.Now;
            
        }


        //THIS TAKES SUSPECIOUSLY LONG TIME: MAYBE MOVE AWAY FROM STREAM?
        //I changed it to a streamreader because wikipedia.org has an incredibly long robots.txt
        //And a string can't contain all that information
        private List<string> GetRobotsTxt(String url)
        {
            List<String> rules= new List<String>();
            bool foundMe = false;
            WebClient client = new WebClient();

            try{
                url= url.TrimEnd('/');
                using (Stream stream = client.OpenRead(new Uri(url+"/robots.txt")))
                {
                    using (StreamReader reader = new StreamReader(stream, System.Text.Encoding.ASCII))
                    {
                        
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (foundMe)
                            {   
                                if ((line.ToLower().Contains("allow") || line.ToLower().Contains("disallow")) && !line.ToLower().StartsWith("#"))
                                {
                                    rules.Add(line);
                                }
                                //else{break;}
                            }
                            if (foundMe && line.ToLower().Contains("user-agent:"))
                            {
                                //foundMe =false; //If my rules are separated in different places of te robots.txt then it is just bad formattin on their part
                                break;
                            }
                            if (line.ToLower().Contains("user-agent: *"))
                            {
                                foundMe = true;
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                return rules;
                //Console.Write(url + " Robot.cs: ");
                //Console.WriteLine(ex.Message);
            }
            return rules;
        }


        public List<String> RobotTxt
        {
            get{ return _robotText; }
        }

        public string SiteName
        {
            get{ return _siteURL; }
        }

        public DateTime DateArchived
        {
            get{ return _dayArchived; }
        }

    }
}

