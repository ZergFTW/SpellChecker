using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SpellChecker.UnitTests
{
   [TestClass]
   public class NodeTests
   {
      [TestClass]
      public class SearchNodeByValueMethod
      {
         private readonly List<Node> nodesBCDE = new List<Node>()
         {
            new Node('b'),
            new Node('c'),
            new Node('d'),
            new Node('e')
         };

         [TestMethod]
         public void SearchBInBCDE_Return0()
         {
            Assert.AreEqual(0, Node.SearchNodeByValue(nodesBCDE, 'b'));
         }

         [TestMethod]
         public void SearchDInBCDE_Return2()
         {
            Assert.AreEqual(2, Node.SearchNodeByValue(nodesBCDE, 'd'));
         }

         [TestMethod]
         public void SearchAInBCDE_ReturnComplement0()
         {
            Assert.AreEqual(~0, Node.SearchNodeByValue(nodesBCDE, 'a'));
         }

         [TestMethod]
         public void SearchFInBCDE_ReturnComplement4()
         {
            Assert.AreEqual(~4, Node.SearchNodeByValue(nodesBCDE, 'f'));
         }

         [TestMethod]
         public void SearchInEmptyList_ReturnComplement0()
         {
            List<Node> nodes = new List<Node>();
            Assert.AreEqual(~0, Node.SearchNodeByValue(nodes, 'a'));
         }


         [TestMethod]
         public void SearchAInA_Return0()
         {
            List<Node> nodes = new List<Node>() { new Node('a') };
            Assert.AreEqual(0, Node.SearchNodeByValue(nodes, 'a'));
         }
      }

      [TestClass]
      public class AddGetWordMethods
      {
         private Node rootNode;

         [TestInitialize]
         public void InitializeTestRoot()
         {
            rootNode = new Node('\0');
         }

         [TestMethod]
         public void AddWordCat_ReturnNodeT()
         {
            Assert.AreEqual(Node.AddWord(rootNode, "cat").Value, 't');
         }

         [TestMethod]
         public void AddWordCat_ReturnNodeIsEndOfWord()
         {
            Assert.IsTrue(Node.AddWord(rootNode, "cat").IsEndOfWord);
         }

         [TestMethod]
         public void AddWordCat_ParentNodeValueIsA()
         {
            Node n = Node.AddWord(rootNode, "cat");
            Assert.AreEqual(n.ParentNode.Value, 'a');
         }

         [TestMethod]
         public void AddMixedCaseWord_GetWordReturnMixedCaseWord()
         {
            Assert.AreEqual(Node.AddWord(rootNode, "CaT").Word, "CaT");
         }
      }

      [TestClass]
      public class AddGetMisprintsMethods
      {
         private Node rootNode;
         private Node correctWordNode;

         [TestInitialize]
         public void InitializeTestRoot()
         {
            rootNode = new Node('\0');
            correctWordNode = Node.AddWord(rootNode, "hello");
         }

         [TestMethod]
         public void AddMisprintedWordHllo_ReturnNodeWithValueO()
         {
            Node n = Node.AddMisprintedWord(rootNode, "hllo", 1 , correctWordNode);
            Assert.AreEqual(n.Value, 'o');
         }

         [TestMethod]
         public void AddMisprintedWordHllo_ReturnNodeWithWordHllo()
         {
            Node n = Node.AddMisprintedWord(rootNode, "hllo", 1, correctWordNode);
            Assert.AreEqual(n.GetNodeString(), "hllo");
         }

         [TestMethod]
         public void AddMisprint_GetMissprintReturnListWithCorrectNode()
         {
            Node n = Node.AddMisprintedWord(rootNode, "helo", 1, correctWordNode);
            Assert.IsTrue(n.GetMisprints(1).Contains(correctWordNode));
         }

         [TestMethod]
         public void AddSameMisprintForSameWord_ReturnNull()

         {
            Node.AddMisprintedWord(rootNode, "helo", 1, correctWordNode);
            Node n = Node.AddMisprintedWord(rootNode, "helo", 1, correctWordNode);
            Assert.AreEqual(n, null);
         }

         [TestMethod]
         public void AddSameMisprintForSameWord_DoesntAddSecondMissprint()
         {
            Node n = Node.AddMisprintedWord(rootNode, "helo", 1, correctWordNode);
            Node.AddMisprintedWord(rootNode, "helo", 1, correctWordNode);
            Assert.AreEqual(n.GetMisprints(1).Count, 1);
         }

      }

      [TestClass]
      public class FindNodeByWordMethod
      {
         private Node root;
         private Node apple;
         private Node orange;
         private Node banana;
         private Node folder;

         [TestInitialize]
         public void InitializeDictionary()
         {
            root = new Node('\0');
            apple = Node.AddWord(root, "apple");
            orange = Node.AddWord(root, "orange");
            banana = Node.AddWord(root, "banana");
            folder = Node.AddWord(root, "FOLder");
         }

         [TestMethod]
         public void FindUpperCaseWordInLowerDictionary_ReturnWord()
         {
            Assert.AreSame(Node.FindNodeByWord(root, "APPle"), apple);
         }

         [TestMethod]
         public void FindLowerCaseWordInUpperDictionary_ReturnWord()
         {
            Assert.AreSame(Node.FindNodeByWord(root, "folder"), folder);
         }
      }

      [TestClass]
      public class CompareToMethod
      {
         private Node root;
         private Node test;
         private Node cat;

         [TestInitialize]
         public void InitializeTree()
         {
            root = new Node('\0');
            test = Node.AddWord(root, "Test");
            cat = Node.AddWord(root, "Cat");
         }
      }
   }
}  
