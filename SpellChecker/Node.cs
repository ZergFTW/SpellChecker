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
      
      /* Array of possible misprinted words 
      first array index points on number of misprints in given word */
      private Node[][] misprints;
      
      /* return array of misprints for given node and count of misprints
      return empty array if there is no misprints
      return this node packed to new array if count = 0 */
      public Node[] GetMisprints(int misprintsCount)
      {
         if (misprintsCount < 0)
            throw new ArgumentException(nameof(misprintsCount));

         if (misprintsCount == 0)
         {
            return IsCorrectWord ? new Node[] {this} : new Node[0];
         }

         if (misprints == null || misprints.Length < misprintsCount)
         {
            return new Node[0];
         }

         return misprints[misprintsCount - 1];
      }

      /* Adds new misprint to tree. 
      doesn't check misprint for correctness i.e. it is possible to add "cat" as a misprint of "dog" 
      throw ArgumentNullException if correctWordNode == null
      throw ArgumentException if correctWordNode is not a correct word
      throw ArgumentException if misprints count < 1 */
      public void AddMisprint(int misprintsCount, ref Node correctWordNode)
      {
         if (correctWordNode == null)
            throw new ArgumentNullException(nameof(correctWordNode));

         if (!correctWordNode.IsCorrectWord)
            throw new ArgumentException(nameof(correctWordNode));

         if (misprintsCount < 1)
            throw new ArgumentException(nameof(misprintsCount));

         // if there is no misptints in ths node - create array and fill it
         if (misprints == null)
         {
            misprints = new Node[misprintsCount][];
            for (int i = 0; i < misprintsCount; ++i)
            {
               misprints[i] = new Node[0];
            }
         }
         else if (misprints.Length < misprintsCount) // if array is too short, extend and fill it with empty lists
         {
            int oldSize = misprints.Length;
            Array.Resize(ref misprints, misprintsCount);
            for (int i = oldSize; i < misprintsCount; ++i)
            {
               misprints[i] = new Node[0];
            }
         }
         Node.AddUnique(ref misprints[misprintsCount - 1], ref correctWordNode);
      }

      static public void AddUnique(ref Node[] array, ref Node newNode)
      {
         if (Array.IndexOf(array, newNode) < 0)
         {
            Array.Resize(ref array, array.Length + 1);
            array[array.Length - 1] = newNode;
         }
      }
   }
}
