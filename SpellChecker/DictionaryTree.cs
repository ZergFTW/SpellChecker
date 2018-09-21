using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace SpellChecker
{
   public class DictionaryTree
   {
      private int MaxMisprints { get; }
      private readonly Node root;

      public DictionaryTree(int maxMisprints = 2)
      {
         this.MaxMisprints = maxMisprints;
         root = new Node(value:'\0');
      }

      public int WordsCount
      {
         get { return root.Index;  }
      }
      
      /* Adds new word to dictionary.
      Generates and adds all misprints for this word (only deletes)
      throw ArgumentException if word is empty or null */
      public bool AddWord(String word)
      {
         Node correctWordNode = Node.AddWord(root, word);

         if (correctWordNode == null)
         {
            // word already in the dictionary, we dont need to generate misprints
            return false;
         }

         int maxDeletions = MaxDeletionsForWordLength(word.Length, MaxMisprints);

         for (int deletionsCount = 1; deletionsCount <= maxDeletions; ++deletionsCount)
         {
            // list of misprinted variants (deletions only)
            List<String> misprintsList = GenerateDeletions(word, deletionsCount);

            // push misprints to dictionary
            for (int i = 0; i < misprintsList.Count; ++i)
            {
               Node.AddMisprintedWord(root, misprintsList[i], deletionsCount, correctWordNode);
            }
         }

         return true;
      }

      /* searches for input in dictionary
      if word found - returns it as is

      if one misprint with minimal edits found - return it as in dictionary
      otherwise returns all possible misprints with minimal number of edits as {possibleWord1, PossibleWord2, ...}

      misprints with higher number of edits ignored, if there is some lower edits misprints

      if no word and misprints found - return original word as {word?} */
      public String GetCorrectedWord(String inputWord)
      {
         if (String.IsNullOrEmpty(inputWord))
            throw new ArgumentNullException(nameof(inputWord));

         Node endNode = Node.FindNodeByWord(root, inputWord);

         // exact word found! Yay!
         if (endNode != null && endNode.IsEndOfWord)
         {
            return inputWord;
         }

         List<Node> misprints = GetPossibleMisprints(inputWord);

         if (misprints.Count == 0)
         {
            return String.Concat("{", inputWord, "?}");
         }
         else if (misprints.Count == 1)
         {
            return misprints[0].Word;
         }
         else
         {
            StringBuilder sb = new StringBuilder();
            sb.Append('{');
            for (int i = 0; i < misprints.Count; ++i)
            {
               sb.Append(misprints[i].Word);
               sb.Append(" ");
            }

            sb[sb.Length - 1] = '}';
            return sb.ToString();
         }
      }

      // helper for GetCorrectedWord
      private List<Node> GetPossibleMisprints(String inputWord)
      {
         // put results here
         HashSet<Node> resultHash = new HashSet<Node>();

         // keep generated deletions for deeper level of misprints
         int maxDeletionsForInput = MaxDeletionsForWordLength(inputWord.Length, MaxMisprints);
         List<string>[] inputWithDels = new List<string>[maxDeletionsForInput + 1];
         inputWithDels[0] = new List<string>
         {
            inputWord.ToLower() // for consistency we put original word as word with 0 deletes
         };

         // starting from one misprint
         for (int misprintsLevel = 1; misprintsLevel <= MaxMisprints; ++misprintsLevel)
         {
            // first, lets generate deletions for current level of misprints
            if (maxDeletionsForInput >= misprintsLevel)
            {
               inputWithDels[misprintsLevel] = GenerateDeletions(inputWord, misprintsLevel);
            }

            /* we need to iterate on all possible misprints combinations on this misprint level
            for brevity d - deletion, i - insertion
            i.e. if we search for 3 misprints - it can be 3d + 0i or 2d + 1i or 1d + 2i or 0d + 3i 
            and we need all of them */
            for (int deletes = 0; deletes <= misprintsLevel; ++deletes)
            {
               int inserts = misprintsLevel - deletes;
               if (inserts >= inputWithDels.Length)
               {
                  continue; // word is too short - there is not enough letters for such number of insert mistakes
               }

               /* to find insertion misprints, remove as many letters from input and search for it in dictionary
               first element of input with dels contains original word
               deletions already in the dictionary so we need just to look at dictionary misprints 
               and for combined misprints we remove some letters and search for deletions */
               for (int i = 0; i < inputWithDels[inserts].Count; ++i)
               {
                  resultHash.UnionWith(GetDeletionMisprints(deletes, inputWithDels[inserts][i]));
               }
            }

            if (resultHash.Count > 0) // we found something! 
            {
               break;
            }

         } // global misprints loop

         List<Node> resultNodesList = resultHash.ToList();
         // sorting by index - results must be in the same order as in the dictionary :-/
         resultNodesList.Sort((a, b) => a.Index.CompareTo(b.Index));
         return resultNodesList;
      }

      // helper for GetPossibleMisprints
      private HashSet<Node> GetDeletionMisprints(int deletions, String word)
      {
         Node node = Node.FindNodeByWord(root, word);
         if (node == null)
         {
            return new HashSet<Node>();
         }
         return node.GetMisprints(deletions);
      }

      /* calculates, how much deletes can be in word with given length
      word should be long enough to have letters for all deletes AND all deletion gaps 
      if maxMisprintsForDictionary < 0 - ignores it */
      public static int MaxDeletionsForWordLength(int wordLength, int maxMisprintsForDictionary = -1)
      {
         int maxPossibleDeletions = (wordLength + 1) / 2;

         if (maxMisprintsForDictionary < 0)
         {
            return maxPossibleDeletions;
         }
         else
         {
            return maxPossibleDeletions < maxMisprintsForDictionary ? maxPossibleDeletions : maxMisprintsForDictionary;
         }
      }

      /* generates possible mistakes - only deletes, excluding deletes in adjasted positions
      generated list CAN contain equal strings (for example if word contain doubled letter e.g. miss -> iss, mss, mis, mis)
      throws ArgumentException if word is empty or too short
      throws ArgumentException if number of deletes < 0 */
      public static List<String> GenerateDeletions(String word, int numberOfDeletions = 1)
      {
         if (numberOfDeletions < 0)
            throw new ArgumentException(nameof(numberOfDeletions));

         if (String.IsNullOrEmpty(word) || numberOfDeletions > MaxDeletionsForWordLength(word.Length))
            throw new ArgumentException(nameof(word));
         
         word = word.ToLower();
         if (numberOfDeletions == 0)
         {
            return new List<string>() {word};
         }

         var result = new List<String>();
         // contains positions of letters marked for deletion
         int[] deletes = new int[numberOfDeletions];
         for (int i = 0; i < deletes.Length; i++)
         {
            // setting gaps between deletes
            deletes[i] = i * 2;
         }

         // loop through deletes, untill first deletion position reaches last awailable point
         // each cycle we getting new variation of misprint
         int lastDeletes = deletes.Length - 1;
         while (true)
         {
            result.Add(RemoveLetters(word, deletes));
            if (deletes[lastDeletes] < word.Length - 1) // we can move deletes.last to the right
            {
               ++deletes[lastDeletes];
            }
            else // deletes.last in rightmost position
            {
               int delPosForRightShift = FindClosestUnshiftedDeletePos(deletes);

               if (delPosForRightShift < 0) // nothing left to shift
               {
                  return result;
               }
               else
               {
                  ++deletes[delPosForRightShift];
                  // shifting everithing else to the left
                  for (int delPosForLeftShift = delPosForRightShift + 1; delPosForLeftShift < deletes.Length; ++delPosForLeftShift)
                  {
                     deletes[delPosForLeftShift] = deletes[delPosForLeftShift - 1] + 2;
                  }
               }
            }
         }
      }

      // helper for GenerateDeletions
      private static int FindClosestUnshiftedDeletePos(int[] deletes)
      {
         if (deletes.Length == 1)
         {
            return -1;
         }

         // searching from right to left, starting from penult element
         for (int i = deletes.Length - 2; i >= 0; --i)
         {
            // there is more than 1 letter beetwen this letter and letter to the right
            if (deletes[i] < deletes[i + 1] - 2) 
            {
               return i;
            }
         }

         // everything shifted to the right
         return -1;
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
   }
}
