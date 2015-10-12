using System;
using System.Data.HashFunction;
using System.Collections.Generic;
using System.Security;
using System.Net;


namespace PoliteSearchEngine
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            try{
                Certificates.Instance.GetCertificatesAutomatically();

                Console.WriteLine("Hello. I am Polite - The Polite Search Engine. \n");
                Crawler crawl = new Crawler(4, 200);
                SearchTable search = new SearchTable(crawl.Pages, crawl.CompleteTermVector);
                string searchQuery="";

                Console.WriteLine("Press any key to clear console and proceed with searching");
                Console.ReadKey();
                Console.Clear();

                while(true){
                    Console.WriteLine("\n\nMake a new search: ");
                    searchQuery = Console.ReadLine();

                    if(searchQuery == ",q")
                    {
                        break;
                    }
                    search.Search(searchQuery);
                    search.PrintSearchResults();
                }
            }catch(Exception ex){
                Console.WriteLine(ex.Message);
            }

        }
            
    }
}
