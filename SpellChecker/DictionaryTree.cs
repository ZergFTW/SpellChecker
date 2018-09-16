using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpellChecker
{
   class DictionaryTree
   {
      private const int MaxMisspels = 2;
      private readonly Node root;

      public DictionaryTree()
      {
         root = new Node(value:'\0', parent:null);
      }

      // return false if wolr already in dictionary, throw Argument
      public bool AddWord(String word)
      {
         if (String.IsNullOrWhiteSpace(word))
            throw new ArgumentException(nameof(word));

         Node correctWordNode = root.AddWord(word);

         if (correctWordNode == null) // word already in dictionary
            return false;

         int max = MaxWordDeletions(word.Length);
         for (int d = 1; d <= max; ++d)
         {
            List<String> deletionsList = GenerateDeletions(word, d);
            for (int i = 0; i < deletionsList.Count; ++i)
            {
               root.AddWord(deletionsList[i], d, correctWordNode);
            }
         }

         return true;
      }

      // calculates, how much deletions can be in word with given length
      // word should be long enough to have letters for all deletes AND all deletion gaps
      private static int MaxWordDeletions(int wordLength)
      {
         int maxPossibleDeletions = (wordLength + 1) / 2;
         return maxPossibleDeletions < MaxMisspels ? maxPossibleDeletions : MaxMisspels;
      }

      /* generates possible mistakes - only deletions, excluding deletions in adjasted positions
      throws ArgumentException if word is empty or too short
      throws ArgumentException if number of deletions < 1 */
      private static List<String> GenerateDeletions(String word, int numberOfDeletions = 1)
      {
         if (numberOfDeletions < 1)
         {
            throw new ArgumentException(nameof(numberOfDeletions));
         }

         if (String.IsNullOrWhiteSpace(word) || numberOfDeletions > MaxWordDeletions(word.Length))
         {
            throw new ArgumentException(nameof(word));
         }

         var result = new List<String>();
         // contains positions of letters marked for deletion
         int[] del = new int[numberOfDeletions];
         for (int i = 0; i < del.Length; i++)
         {
            // setting gaps between dels
            del[i] = i * 2;
         }

         // loop through deletions, untill first deletion position reaches last awailable point
         // each cycle we getting new variation of misspell
         int delLast = del.Length - 1;
         while (true)
         {
            result.Add(RemoveLetters(word, del));
            if (del[delLast] < word.Length - 1)
            {
               ++del[delLast];
            }
            else
            // del[delLast] in rightmost position
            {
               // only one deletion needed
               if (delLast == 0)
               {
                  return result;
               }

               for (int i = delLast - 1; i >= 0; --i)
               {
                  // searching, what else can be shifted to the right
                  if (del[i] < del[i + 1] - 2)
                  {
                     // shifting
                     ++del[i];

                     // shifting everiting on right side to the left
                     for (int j = i + 1; i < del.Length; ++i)
                     {
                        del[j] = del[j - 1] + 2;
                     }
                     break;
                  }
                  else // element can't be shifted to the right
                  {
                     // del[0] can't be shiftet to the right - done
                     if (i == 0)
                     {
                        return result;
                     }
                  }
               }
            }
         }
      }

      // helper for GenerateDeletions
      private static String RemoveLetters(String word, int[] letters)
      {
         for (int i = letters.Length - 1; i >= 0; --i)
         {
            word = word.Remove(letters[i], 1);
         }
         return word;
      }

      private class Node
      {
         private readonly char value;
         private readonly Node parent;
         private List<Node> children;
         private bool isEndOfWord;
         private List<Node>[] misspells;

         public char Value { get; }
         public Node Parent { get; }
         public bool IsEndOfWord { get; set; }
         public List<Node>[] Misspells { get; } // warning! misspels list can be null
         
         public Node(char value, Node parent, bool isEndOfWord = false)
         {
            this.value = value;
            this.parent = parent;
            this.isEndOfWord = isEndOfWord;
            children = new List<Node>();
            misspells = new List<Node>[MaxMisspels];
         }
         
         public String Word
         {
            // returns word, which ends on this node
            get
            {
               // root node
               if (this.parent == null)
               {
                  return "";
               }
               StringBuilder sb = new StringBuilder();
               Node n = this;
               do
               {
                  sb.Insert(0, n.value);
                  n = n.parent;
               } while (n.parent != null);

               return sb.ToString();
            }
         }

         // return endnode on success, null if word is repeated (only for correct words
         // warning! there is no repetition checks for misspelled words
         public Node AddWord(String word, int deletions = 0, Node correctWordNode = null)
         {
            if (String.IsNullOrWhiteSpace(word) || word.Length < 1)
               throw new ArgumentException(nameof(word));

            if (deletions < 0 || deletions > MaxMisspels)
               throw new ArgumentException(nameof(deletions));

            if (deletions > 0 && correctWordNode == null)
               throw new ArgumentException(nameof(correctWordNode));

            char[] chWord = word.ToLower().ToCharArray();

            Node currentNode = this;

            
            for (int i = 0; i < chWord.Length - 1; ++i)
            {
               currentNode = currentNode.AddNode(chWord[i]);
            }
            // last node need special threatment
            currentNode = currentNode.AddNode(chWord[chWord.Length - 1], true, deletions, correctWordNode);

            return currentNode;
         }


         /* helper for Node.AddWord arguments must be checked in AddWord
         return null if word is already exists, only for last node in the word (isEndOfWord = true && deletions = 0)
         returns Node (new, or existing), which contains current char
         to add misspeled variant of word, isEndOfWord must be true and deletions must be > 0;
         deletions ignored if isEndOfWord == false */
         private Node AddNode(char c, bool isEndOfWord = false, int deletions = 0, Node correctWordNode = null)
         {
            int position = SearchChildrenByChar(c);
            if (position >= 0) // node aldreaady exists
            {
               if (isEndOfWord) // last node
               {
                  if (deletions == 0) // correct word
                  {
                     // word already exists (dictionary has repeats)! Abort mission!
                     if (children[position].IsEndOfWord)
                        return null;

                     // node already exists, but not marked as word end - new word
                     children[position].IsEndOfWord = true;
                  }
                  else // misspeled word - add it to list of misspells
                  {
                     AddMisspell(deletions, correctWordNode);
                  }
               }
            }
            else // new node required
            {
               position = ~position;
               Node newNode = new Node(c, this, isEndOfWord);
               children.Insert(position, newNode);
            }

            return children[position];
         }

         // helper for AddNode
         // doesn't check arguments, doesn't check for repeats - should be checked earlier
         private void AddMisspell(int deletions, Node correctWord)
         {
            if (misspells[deletions - 1] == null)
            {
               misspells[deletions - 1] = new List<Node>();
            }

            misspells[deletions - 1].Add(correctWord);
         }

         // Helper method, returns position of the node in children.
         // If there is no such node, returns ~position (like List<T>.BinarySearch)
         private int SearchChildrenByChar(char c)
         {
            /* not shure is it perfomance hit or improvement
            probably, on dictionary loading - improvement,
            but on dictionary use - hit, especially with large dictionary */

            /*
            if (list.Count() == 0 || list[0].CompareTo(value) < 0)
            {
               return ~0;
            }

            if (list[list.Count() - 1].CompareTo(value) > 0)
            {
               return ~list.Count();
            }*/

            int left = 0;
            int right = children.Count();
            int mid = 0;

            while (left < right)
            {
               // no need in overflow protection - alphabet is too short
               mid = (left + right) / 2;
               int result = value.CompareTo(children[mid].value);
               if (result < 0)
               {
                  right = mid;
               }
               else if (result > 0)
               {
                  left = mid + 1;
               }
               else
               {
                  return mid;
               }
            }

            // should be right, not mid - we need to return lastIndex + 1, if value > lastValue
            return ~right;
         }


      }
   }
}
