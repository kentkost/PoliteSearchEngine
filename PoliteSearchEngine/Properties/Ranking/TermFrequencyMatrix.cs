using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace PoliteSearchEngine
{
    public class TermFrequencyMatrix
    {
        List<PostingList> postings = new List<PostingList>();
       
        public TermFrequencyMatrix(List<Page> pages){
            foreach (Page p in pages)
            {
                InsertPosting(p);
            }
        }

        public void PrintPostings()
        {
            foreach (PostingList pl in postings)
            {
                Console.Write(pl.Term + " appears in: ");
                foreach (int i in pl.DocAppearances)
                {
                    Console.Write(i + ", ");
                }
                Console.WriteLine();
            }
        }

        public void InsertPosting(Page page)
        {
            bool foundIt;
            if (page.Terms.Count > 0)
            {
                foreach (TermVector t in page.Terms)
                {
                    foundIt = false;
                    foreach (PostingList pl in postings)
                    {
                        if (t.TermString == pl.Term)
                        {
                            foundIt = true;
                            pl.AddToDocAppearances(page.DocId);
                            break;
                        }
                    }
                    if (!foundIt)
                    {
                        postings.Add(new PostingList(t.TermString, page.DocId));
                    }
                }
            }
            postings = postings.OrderBy(p=>p.Term).ToList();
        }

        //Kinda tested. Should be changed to accept one string, that will be turned into a search query.
        public List<int> BooleanScore(string t1, string t2)
        {
            List<int> intersection = new List<int>();
            int term1, term2, indexTerm1=0, indexTerm2=0;
            term1 = FindTermPosting(t1);
            term2 = FindTermPosting(t2);

            if (term1 < 0 || term2 < 0)
            {
                intersection.Add(-1);
                return intersection;
            }

            while (indexTerm1 < postings[term1].DocAppearances.Count &&
                  indexTerm2 < postings[term2].DocAppearances.Count)
            {
                if (postings[term1].DocAppearances[indexTerm1] ==
                    postings[term2].DocAppearances[indexTerm2])
                {
                    intersection.Add(postings[term1].DocAppearances[indexTerm1]);
                    indexTerm1++;
                    indexTerm2++;
                }
                else if (postings[term1].DocAppearances[indexTerm1] <
                         postings[term2].DocAppearances[indexTerm2])
                {
                    indexTerm1++;
                }
                else
                {
                    indexTerm2++;
                }
            }
            return intersection;

        }
            

        public int FindTermPosting(string term)
        {
            try{    
                return BinarySearchIndexOf(postings,term);
            }catch(Exception ex){
                Console.WriteLine(ex.Message);
            }
            return -1;
        }

        public int BinarySearchIndexOf(List<PostingList> list, string value, IComparer<PostingList> comparer = null)
        {
            if (list == null)
                throw new ArgumentNullException("list");

            comparer = comparer ?? Comparer<PostingList>.Default;

            Int32 lower = 0;
            Int32 upper = list.Count - 1;

            while (lower <= upper)
            {
                Int32 middle = lower + (upper - lower) / 2;
                Int32 comparisonResult = String.Compare(value, list[middle].Term);
                if (comparisonResult == 0)
                    return middle;
                else if (comparisonResult < 0)
                    upper = middle - 1;
                else
                    lower = middle + 1;
            }

            return -1;
        }

        public List<PostingList> Postings
        {
            get{ return postings; }
        }

        public int PostingListLength
        {
            get{ return postings.Count; }
        }
    }
}

