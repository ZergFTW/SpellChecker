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

      public char Value { get; }
      public Node ParentNode { get; }

      /* retunrs word if node marked as end of word, otherwise return null */
      public String Word { get; private set; }
      public bool IsEndOfWord
      {
         get { return (Word != null) ;}
      }
      
      /* unique index for each correct word
      root node contains smallest unused index */
      public int Index { get; private set; }

      private List<Node> children;

      /* array of lists of possible misprinted words 
      array index points on number of misprints in given word
      each list is sorted in alphabet order */
      private HashSet<Node>[] misprints;

      public Node(char value, Node parent = null)
      {
         this.Value = value;
         this.ParentNode = parent;
         children = new List<Node>();
         misprints = null;
      }

      /* Adds new word to tree. Words saves in same registry as they appear,
      but it is impossible to add same word with different registry i.e. if dictionary already has Cat, you can't add CAT
      return last node on success, null if word is already presented
      throw ArgumentException on empty/null word 
      throw ArgumentException if root node has parent */
      public static Node AddWord(Node rootNode, String newWord)
      {
         if (String.IsNullOrEmpty(newWord))
            throw new ArgumentException(nameof(newWord));

         if (rootNode.ParentNode != null)
            throw new ArgumentException(nameof(rootNode));

         Node endNode = rootNode.AddNodesForWord(newWord);

         if (endNode.IsEndOfWord) // word already exists
         {
            return null;
         }

         // new word was created, need to set it
         endNode.Word = newWord;
         // add index
         endNode.Index = rootNode.Index;
         ++rootNode.Index;
         return endNode;
      }

      /* returns string which ends on this node.
      This method much slower than Word property, but it works on nodes NOT marked as end of word
      return null on root node 
      returned string always lowercase */
      public String GetNodeString()
      {
         // root node can't contain word
         if (ParentNode == null)
         {
            return null;
         }

         StringBuilder sb = new StringBuilder();
         Node currentNode = this;
         do
         {
            sb.Insert(0, currentNode.Value);
            currentNode = currentNode.ParentNode;
         } while (currentNode.ParentNode != null);

         return sb.ToString();
      }

      /* searches for given word,
      returns end node of this word or null if word not found 
      this method will return word even if it is not marked as end of word */
      public static Node FindNodeByWord(Node rootNode, String word)
      {
         if (rootNode.ParentNode != null)
            throw new ArgumentException(nameof(rootNode));

         char[] chWord = word.ToLower().ToCharArray();

         Node currentNode = rootNode;

         for (int i = 0; i < chWord.Length; ++i)
         {
            int nodeIndex = Node.SearchNodeByValue(currentNode.children, chWord[i]);
            if (nodeIndex < 0)
            {
               // word not found in dictionary
               return null;
            }
            currentNode = currentNode.children[nodeIndex];
         }
         
         // root node can be in search results - it contains all misprints for 1 letter words 
         return currentNode;
      }

      /* return hashSet of misprints for given node and count of misprints
      return empty hashSet if there is no misprints for given count
      if count = 0, returns original word in hashSet 
      throw ArgumentException if count < 0 */
      public HashSet<Node> GetMisprints(int misprintsCount)
      {
         if (misprintsCount < 0)
            throw new ArgumentException(nameof(misprintsCount));


         if (misprintsCount == 0)
         {
            return IsEndOfWord ? new HashSet<Node>() {this} : new HashSet<Node>();
         }

         if (misprints == null || misprints.Length < misprintsCount)
         {
            return new HashSet<Node>();
         }

         return misprints[misprintsCount - 1];
      }

      /* Adds new misprint to tree. 
      return end node of misprinted word
      return null if such misprint for such word already exists
      doesn't misprint for correctness i.e. it is possible to add "cat" as a misprint of "dog" */
      public static Node AddMisprintedWord(Node rootNode, String misprintedWord, int misprintsCount, Node correctWordNode)
      {
         if (correctWordNode == null)
            throw new ArgumentNullException(nameof(correctWordNode));

         if (!correctWordNode.IsEndOfWord)
            throw new ArgumentException(nameof(correctWordNode));

         if (misprintsCount < 1)
            throw new ArgumentException(nameof(misprintsCount));

         if (rootNode.ParentNode != null)
            throw new ArgumentException(nameof(rootNode));

         Node endNode = rootNode.AddNodesForWord(misprintedWord);

         if (endNode.AddMisprint(correctWordNode, misprintsCount))
         {
            return endNode;
         }
         else
         {
            return null;
         }
      }

      /* Returns position of the node in nodes.
      list MUST be sorted
      If there is no such node, returns ~position (like List<T>.BinarySearch) */
      public static int SearchNodeByValue(List<Node> nodes, char value)
      {
         int left = 0;
         int right = nodes.Count();
         int mid = 0;
         value = Char.ToLower(value);

         while (left < right)
         {
            // no need in overflow protection - alphabet is too short
            mid = (left + right) / 2;
            int result = value.CompareTo(Char.ToLower(nodes[mid].Value));
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
      
      /* helper for AddMisprintedWord */
      private bool AddMisprint(Node correctWordNode, int misprintsCount)
      {
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

         return misprints[misprintsCount - 1].Add(correctWordNode);
      }

      /* helper for AddWord and Addmisprint - adds all nodes needed for word or misprint */
      private Node AddNodesForWord(String word)
      {
         char[] chWord = word.ToLower().ToCharArray();

         Node currentNode = this;
         for (int i = 0; i < chWord.Length; ++i)
         {
            currentNode = currentNode.AddChildNode(chWord[i]);
         }
         return currentNode;
      }

      /* helper for AddNodesForWord - returns Node (new, or existing), which contains current char */ 
      private Node AddChildNode(char c)
      {
         int nodePosition = SearchNodeByValue(children, c);
         if (nodePosition < 0) // new node required
         {
            nodePosition = ~nodePosition;
            Node newNode = new Node(c, this);
            children.Insert(nodePosition, newNode);
         }

         return children[nodePosition];
      }
   }
}
