using System;
using System.Collections.Generic;
using System.Linq;

namespace PoliteSearchEngine
{
    public class SearchTable
    {
        List<int> documentIDs = new List<int>();
        List<Page> pages = new List<Page>();
        List<TermVector> allTerms = new List<TermVector>();
        TermFrequencyMatrix termFreqMatrix;
        Page searchQuery;

        public SearchTable(List<Page> _pages, List<TermVector> _allTerms)
        {
            Console.WriteLine("Start making Searchtable");
            pages = _pages;
            allTerms = _allTerms;
            termFreqMatrix = new TermFrequencyMatrix(pages);
        }

        public void Search(string _searchQuery)
        {
            searchQuery = new Page(_searchQuery);
            //SearchQuery made manageable
            SearchQueryWeightAndLength();
            NormalizeSearchQuery();
            //Getting all the documents that have the terms that the Searchquery has
            documentIDs = GetInvolvedDocuments();

            //Weight And length of the relevant documents
            CalculateWeightAndDocLengthForTerms();
            NormalizeWeight();
            //Score of the documents
            CosineSimilarity();
        }

        public void PrintSearchResults()
        {
            List<Page> tempPages = new List<Page>();
            //Go trough each document and
            foreach (int id in documentIDs)
            {
                tempPages.Add(pages[id]);
            }
            tempPages = tempPages.OrderBy(s => s.SearchScore).ToList();

            foreach (Page p in tempPages)
            {
                Console.WriteLine(p.Url + " has score " + p.SearchScore);
            }
        }

        private void SearchQueryWeightAndLength()
        {
            double docLength = 0;
            foreach (TermVector tv in searchQuery.Terms)
            {
                int termPosGlobal = BinarySearch.Instance.BinarySearchTermVector(tv.TermString,
                    allTerms);

                if (termPosGlobal >= 0)
                {
                    tv.Weight = tv.TermScore * allTerms[termPosGlobal].TermScore;
                    docLength += Math.Pow(tv.Weight, 2.0);
                }
            }
            searchQuery.DocLength = Math.Sqrt(docLength);
        }

        private void NormalizeSearchQuery()
        {
            foreach (TermVector tv in searchQuery.Terms)
            {
                tv.NormalizedWeight = tv.Weight / searchQuery.DocLength;
            }

        }

        private void CalculateWeightAndDocLengthForTerms()
        {
            
            //Go trough each document and
            foreach (int id in documentIDs)
            { 
                if (pages[id].DocLength != 0)
                {
                    //weight has been calculated before
                    continue;
                }
                double docLength = 0;
                foreach (TermVector tv in pages[id].Terms) //in searchQuery.Terms
                {
                    int termPosGlobal = BinarySearch.Instance.BinarySearchTermVector(tv.TermString,
                        allTerms);

                    if (termPosGlobal >= 0)
                    {
                        //Actually not sure if I am suppose to multiply it allTerms.termscore
                        tv.Weight = tv.TermScore /* * allTerms[termPosGlobal].TermScore*/;
                        docLength += Math.Pow(tv.Weight, 2.0);
                    }
                    else
                    {
                        //Seems be some kind of bug where not all the terms are added to globaltermvector list
                        //Console.WriteLine(tv.TermString + " could not be found");
                    }
                }
                pages[id].DocLength = Math.Sqrt(docLength);
            }
        }

        private void NormalizeWeight()
        {
            //Go trough each document and
            foreach (int id in documentIDs)
            {
                foreach (TermVector tv in pages[id].Terms)
                {
                    tv.NormalizedWeight = tv.Weight / pages[id].DocLength;
                }
            }
        }


        private void CosineSimilarity()
        {
            foreach (int id in documentIDs)
            {
                pages[id].SearchScore = 0;
                foreach (TermVector tv in searchQuery.Terms)
                {
                    int termPosDoc = BinarySearch.Instance.BinarySearchTermVector(tv.TermString,
                        pages[id].Terms);

                    if (termPosDoc >= 0)
                    {
                        //Console.WriteLine(pages[id].Terms[termPosDoc].NormalizedWeight);
                        pages[id].SearchScore += (pages[id].Terms[termPosDoc].NormalizedWeight *
                            tv.NormalizedWeight);
                    }
                }
            }
        }


        //Now I ahve all the onvolved Documents.
        private List<int> GetInvolvedDocuments()
        {
            List<int> invDocuments = new List<int>();
            //Documents containing one or more of the term
            foreach (TermVector tv in searchQuery.Terms)
            {
                //sorry

                foreach (PostingList x in termFreqMatrix.Postings)
                {
                    if (x.Term == tv.TermString)
                    {
                        foreach (int y in x.DocAppearances)
                        {
                            invDocuments.Add(y);
                        }
                    }
                }
                    
            }
            invDocuments = invDocuments.Distinct().ToList();
            invDocuments.Sort();
            return invDocuments;
        }
    }
}

