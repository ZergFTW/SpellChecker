using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SpellChecker
{
   class Program
   {
      static void Main(string[] args)
      {

         List<int>[] arr = new List<int>[2];
         arr[0] = new List<int>();
         arr[0].Add(1);
         Console.WriteLine(arr[0][0]);


         /*List<String> GenerateDeletions(String word, int numberOfDeletions = 1)
         {
            if (numberOfDeletions < 1)
            {
               throw new ArgumentException(nameof(numberOfDeletions));
            }

            // word should be long enough to have letters for all deletes AND all deletion gaps
            if (String.IsNullOrWhiteSpace(word) || word.Length < numberOfDeletions * 2 - 1)
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
         }*/

         /*String RemoveLetters(String word, int[] letters)
         {
            for (int i = letters.Length - 1; i >= 0; --i)
            {
               word = word.Remove(letters[i], 1);
            }
            return word;
         }*/
      }

      public static List<int> GnomeSort(List<int> list)
      {
         int current = 0;
         while (current < list.Count() - 1)
         {
            if (list[current] > list[current + 1])
            {
               Swap(ref list, current, current + 1);
               --current;
            }
            else
            {
               ++current;
            }
         }

         return list;
      }

      public static void Swap(ref List<int> list, int i, int j)
      {
         int temp = list[i];
         list[i] = list[j];
         list[j] = temp;
      }

      public static int MySearch(List<int> list, int value)
      {
         // not shure is it perfomance hit or improvement
         // probably, on dictionary loading - improvement, but on dictionary use - hit
         /*if (list.Count() == 0 || list[0].CompareTo(value) < 0)
         {
            return ~0;
         }

         if (list[list.Count() - 1].CompareTo(value) > 0)
         {
            return ~list.Count();
         }*/

         int left = 0;
         int right = list.Count();
         int mid = 0;

         while (left < right)
         {
            // no need in overflow protection - alphabet is too short
            mid = (left + right) / 2;
            int result = value.CompareTo(list[mid]);
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
         // should be right, not mid - we need to return lastIndex + 1, if value > last
         return ~right;
      }
   }
}
