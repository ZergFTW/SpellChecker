using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SpellChecker
{
   public class Node
   {
      /* unique index for each correct word,
      we need this to sort results in the same order as they are in the dictionary */
      public int Index { get; set; }

      public String Word { get; set; } // can be null
      public bool IsCorrectWord{ get { return (Word != null) ;} }
      
      /* Array of HashSets of possible misprinted words 
      array index points on number of misprints in given word
      each list is sorted in alphabet order */
      private HashSet<Node>[] misprints;
      
      /* return hashSet of misprints for given node and count of misprints
      return empty hashSet if there is no misprints for given count
      if count = 0, returns this word in hashSet (or empty hash set if this is not correct word)
      throw ArgumentException if count < 0*/
      public HashSet<Node> GetMisprints(int misprintsCount)
      {
         if (misprintsCount < 0)
            throw new ArgumentException(nameof(misprintsCount));

         if (misprintsCount == 0)
         {
            return IsCorrectWord ? new HashSet<Node>() {this} : new HashSet<Node>();
         }

         if (misprints == null || misprints.Length < misprintsCount)
         {
            return new HashSet<Node>();
         }

         return misprints[misprintsCount - 1];
      }

      /* Adds new misprint to tree. 
      doesn't check misprint for correctness i.e. it is possible to add "cat" as a misprint of "dog" 
      throw ArgumentNullException if correctWordNode == null
      throw ArgumentException if correctWordNode is not a correct word
      throw ArgumentException if misprints count < 1 */
      public void AddMisprint(int misprintsCount, Node correctWordNode)
      {
         if (correctWordNode == null)
            throw new ArgumentNullException(nameof(correctWordNode));

         if (!correctWordNode.IsCorrectWord)
            throw new ArgumentException(nameof(correctWordNode));

         if (misprintsCount < 1)
            throw new ArgumentException(nameof(misprintsCount));

         // if there is no misptints in ths node - create array and fill it with empty HashSets
         if (misprints == null)
         {
            misprints = new HashSet<Node>[misprintsCount];
            for (int i = 0; i < misprintsCount; ++i)
            {
               misprints[i] = new HashSet<Node>();
            }
         }
         else if (misprints.Length < misprintsCount) // if array is too short, extend and fill it with empty lists
         {
            int oldSize = misprints.Length;
            Array.Resize(ref misprints, misprintsCount);
            for (int i = oldSize; i < misprintsCount; ++i)
            {
               misprints[i] = new HashSet<Node>();
            }
         }

         misprints[misprintsCount - 1].Add(correctWordNode);
      }
   }
}
