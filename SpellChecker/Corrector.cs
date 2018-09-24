using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace SpellChecker
{
   public class Corrector
   {
      public int WordsCount { get { return dictionary.Count; }}
      public int MaxMisprints { get; }

      // only correct words, int represents index of word,
      // so we could print results in the order they were added 
      private IDictionary<string, int> dictionary;

      // everything considered misprints. key is the hash of the word with generated misprint,
      // string[] contains correct words
      private IDictionary<int, string[]> misprints;
      
      public Corrector(int maxMisprints = 2)
      {
         this.MaxMisprints = maxMisprints;
         dictionary = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
         misprints = new Dictionary<int, string[]>();
      }

      /* Use this if no more words will be added to the dictionary
      when called for the first time, converts dictionarys to sortedLists
      following calls just TrimExcess those lists */
      public void TrimExcess()
      {
         if (dictionary.GetType() == typeof(SortedList<string, int>))
         {
            ((SortedList<string, int>)dictionary).TrimExcess();
            ((SortedList<int, string>)dictionary).TrimExcess();
         }
         else
         {
            dictionary = new SortedList<string, int>(dictionary, StringComparer.InvariantCultureIgnoreCase);
            misprints = new SortedList<int, string[]>(misprints);
         }
         GC.Collect();
      }

      /* Adds new word to dictionary.
      Generates and adds all misprints for this word (only deletes)
      throw ArgumentException if word is empty or null */
      public bool AddWord(String word)
      {
         if (String.IsNullOrEmpty(word))
            throw new ArgumentException(nameof(word));

         if (dictionary.ContainsKey(word))
         {
            // already in the dictionary as correct word
            return false;
         }

         dictionary.Add(word, dictionary.Count);
         
         int maxDeletions = MaxDeletionsForWordLength(word.Length, MaxMisprints);
         for (int deletionsCount = 1; deletionsCount <= maxDeletions; ++deletionsCount)
         {
            // list of misprinted variants (deletions only)
            List<String> misprintsList = GenerateDeletions(word, deletionsCount);

            // add misprints to dictionary
            for (int i = 0; i < misprintsList.Count; ++i)
            {
               int misprintHash = misprintsList[i].GetHashCode();
               if (!misprints.ContainsKey(misprintHash))
               {
                  misprints.Add(misprintHash, new string[1] {word});
               }
               else
               {
                  string[] mpArr = misprints[misprintHash];
                  AddUnique(ref mpArr, word);
                  misprints[misprintHash] = mpArr;
               }

            }
         }
         return true;
      }

      /* searches for input in dictionary
      if word found - returns it as is in input

      if one misprint with minimal edits found - return it as in dictionary
      otherwise returns all possible misprints with minimal number of edits as {possibleWord1, PossibleWord2, ...}

      misprints with higher number of edits ignored, if there is some lower edits misprints

      if no word and misprints found - return original word as {word?} */
      public String GetCorrectedWord(string inputWord)
      {
         if (String.IsNullOrEmpty(inputWord))
            throw new ArgumentNullException(nameof(inputWord));

         // exact word found! Yay!
         if (dictionary.ContainsKey(inputWord))
         {
            return inputWord;
         }

         List<string> misprints = GetAllPossibleCorrectWordsForMisprint(inputWord);

         if (misprints.Count == 0)
         {
            return String.Concat("{", inputWord, "?}");
         }
         else if (misprints.Count == 1)
         {
            return misprints[0];
         }
         else
         {
            StringBuilder sb = new StringBuilder();
            sb.Append('{');
            for (int i = 0; i < misprints.Count; ++i)
            {
               sb.Append(misprints[i]);
               sb.Append(" ");
            }

            sb[sb.Length - 1] = '}';
            return sb.ToString();
         }
      }

      // helper for GetCorrectedWord
      private List<string> GetAllPossibleCorrectWordsForMisprint(string misprintedWord)
      {
         // put results here
         HashSet<String> resultHashSet = new HashSet<String>();
         misprintedWord = misprintedWord.ToLower();

         // keep generated deletions for deeper level of misprints
         int maxDeletionsForInput = MaxDeletionsForWordLength(misprintedWord.Length, MaxMisprints);
         List<string>[] inputWithDels = new List<string>[maxDeletionsForInput + 1];

         // for consistency we put original word as word with 0 deletes
         inputWithDels[0] = new List<string> { misprintedWord.ToLower() };

         // starting from one misprint
         for (int misprintsLevel = 1; misprintsLevel <= MaxMisprints; ++misprintsLevel)
         {
            // first, lets generate deletions for current level of misprints
            if (maxDeletionsForInput >= misprintsLevel)
            {
               inputWithDels[misprintsLevel] = GenerateDeletions(misprintedWord, misprintsLevel);
            }

            /* we need to iterate over all possible misprints combinations at this misprint level
            for brevity d - deletion, i - insertion
            i.e. if we search for 3 misprints - it can be 3d + 0i or 2d + 1i or 1d + 2i or 0d + 3i 
            and we need all of them */
            for (int deletes = 0; deletes <= misprintsLevel; ++deletes)
            {
               int inserts = misprintsLevel - deletes;

               // input word is too short - there is not enough letters for such number
               // of insertion misprints (we didn't generated deletions for this level)
               if (inserts >= inputWithDels.Length)
               {
                  continue; 
               }

               /* to find correct words for insertion misprints, remove as many letters from
               input and search for it in the dictionary
               deletions already in the misprints dictionary so we need just to look at misprints 
               and for combined misprints we remove some letters and search in misprints */
               for (int i = 0; i < inputWithDels[inserts].Count; ++i)
               {
                  resultHashSet.UnionWith(GetCorrectWordsForMisprint(inputWithDels[inserts][i], deletes));
               }
            }

            if (resultHashSet.Count > 0) // we found something! 
            {
               break;
            }
         } // global misprints loop

         List<string> resultList = resultHashSet.ToList();
         // sorting by index - results must be in the same order as they are in the dictionary :-/
         resultList.Sort((a, b) => dictionary[a].CompareTo(dictionary[b]));
         return resultList;
      }

      /* helper for GetAllPossibleCorrectWordsForMisprint
      returns all possible correct words for given word and deletions number 
      0 deletions searches in correct words dictionary
      if nothing gound - returns empry string array */
      private string[] GetCorrectWordsForMisprint(string misprintedWord, int deletions)
      {
         string[] result = new string[0];

         if (deletions == 0)
         {
            if (dictionary.ContainsKey(misprintedWord))
            {
               // return packed original word (for consistency)
               result = new string[] {misprintedWord};
            }
            return result;
         }

         int wordHash = misprintedWord.GetHashCode();
         if (!misprints.ContainsKey(wordHash))
         {
            return result;
         }

         string[] correctWordsArray = misprints[wordHash];
         if (correctWordsArray == null || correctWordsArray.Length == 0)
         {
            return result;
         }

         for (int i = 0; i < correctWordsArray.Length; ++i)
         {
            if (IsPossibleMissprint(correctWordsArray[i], misprintedWord, deletions))
            {
               AddUnique(ref result, correctWordsArray[i]);
            }
         }

         return result;

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

      /* Checks is it possible misprint for given word
      Checks only deletions misprints 
      We need to do this because of collisions and because all correct words 
      saves to one array per misprint (no matters how many deletes in misprint) */
      static public bool IsPossibleMissprint(string correctWord, string misprintedWord, int numberOfDeletions)
      {
         if (numberOfDeletions < 1)
            throw new ArgumentException(nameof(numberOfDeletions));

         if (String.IsNullOrEmpty(correctWord))
            throw new ArgumentException(nameof(correctWord));

         if (misprintedWord == null) // CAN be empty word
            throw new ArgumentException(nameof(misprintedWord));

         // we cant delete this much letters from given word
         if (numberOfDeletions > MaxDeletionsForWordLength(correctWord.Length))
         {
            return false;
         }

         // correct word should be exactly this long
         if (correctWord.Length != misprintedWord.Length + numberOfDeletions)
         {
            return false;
         }

         correctWord = correctWord.ToLower();
         misprintedWord = misprintedWord.ToLower();

         // we need to check, is it possible to get misprinted word from correct via exactly given
         // number of deletes, and we can't delete two adjasted letters
         int correctionsMade = 0;
         int previosDeletedCharacter = -2;
         for (int currentChar = 0; currentChar < correctWord.Length; ++currentChar)
         {
            // different chars - remove it!
            if (currentChar >= misprintedWord.Length || correctWord[currentChar] != misprintedWord[currentChar])
            {
               // we need to check, maybe previous characters same as current - so we can remove them to
               // increase gap for next potential deletion
               while (currentChar > 0 && correctWord[currentChar] == correctWord[currentChar - 1] 
                      && currentChar - 1 != previosDeletedCharacter) // we can't delete two adjasted letters
               {
                  --currentChar;
               }
               correctWord = correctWord.Remove(currentChar, 1);
               previosDeletedCharacter = currentChar;
               ++correctionsMade;
               if (correctionsMade == numberOfDeletions)
               {
                  break;
               }
            }

         }

         return correctWord == misprintedWord;
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
      private static int FindClosestUnshiftedDeletePos(int[] deletePositions)
      {
         if (deletePositions.Length == 1)
         {
            return -1;
         }

         // searching from right to left, starting from penult element
         for (int i = deletePositions.Length - 2; i >= 0; --i)
         {
            // there is more than 1 letter beetwen this letter and letter to the right
            if (deletePositions[i] < deletePositions[i + 1] - 2) 
            {
               return i;
            }
         }

         // everything shifted to the right
         return -1;
      }

      // helper for GenerateDeletions
      private static String RemoveLetters(String word, int[] letterPositions)
      {
         for (int i = letterPositions.Length - 1; i >= 0; --i)
         {
            word = word.Remove(letterPositions[i], 1);
         }
         return word;
      }


      static private void AddUnique(ref string[] array, string word)
      {
         if (Array.IndexOf(array, word) < 0)
         {
            Array.Resize(ref array, array.Length + 1);
            array[array.Length - 1] = word;
         }
      }
   }
}
