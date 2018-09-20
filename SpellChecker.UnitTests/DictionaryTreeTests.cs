using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SpellChecker.UnitTests
{
   [TestClass]
   public class DictionaryTreeTests
   {
      [TestClass]
      public class MaxDeletionsForWordLengthMethod
      {
         [TestMethod]
         public void Length0Max5_Return0()
         {
            Assert.AreEqual(DictionaryTree.MaxDeletionsForWordLength(0, 5), 0);
         }

         [TestMethod]
         public void Length1Max2_Return1()
         {
            Assert.AreEqual(DictionaryTree.MaxDeletionsForWordLength(1, 2), 1);
         }

         [TestMethod]
         public void Length5Max10_Return3()
         {
            Assert.AreEqual(DictionaryTree.MaxDeletionsForWordLength(5, 10), 3);
         }

         [TestMethod]
         public void Length5Max2_Return2()
         {
            Assert.AreEqual(DictionaryTree.MaxDeletionsForWordLength(5, 2), 2);
         }

         [TestMethod]
         public void Length10_Return5()
         {
            Assert.AreEqual(DictionaryTree.MaxDeletionsForWordLength(10), 5);
         }
      }

      [TestClass]
      public class GenerateDeletionsMethod
      {
         [TestMethod]
         public void Generate1DeletionsForWord_EquivalentGivenList()
         {
            var generated = DictionaryTree.GenerateDeletions("orange", 1);
            List<String> equivalent = new List<String>()
            {
               "range", "oange", "ornge", "orage", "orane", "orang"
            };
            CollectionAssert.AreEquivalent(generated, equivalent);
         }

         [TestMethod]
         public void Generate2DeletionsForWord_EquivalentGivenList()
         {
            var generated = DictionaryTree.GenerateDeletions("orange", 2);
            List<String> equivalent = new List<String>()
            {
               "rnge", "rage", "rane", "rang",
               "oage", "oane", "oang",
               "orne", "orng",
               "orag"
            };
            CollectionAssert.AreEquivalent(generated, equivalent);
         }

         [TestMethod]
         public void Generate3DeletionsForWord_EquivalentGivenList()
         {
            var generated = DictionaryTree.GenerateDeletions("oranges", 3);
            List<String> equivalent = new List<String>()
            {
               "rnes", "rngs", "rnge",
               "rags", "rage",
               "rane",
               "oags", "oage",
               "oane",
               "orne"
            };
            CollectionAssert.AreEquivalent(generated, equivalent);
         }

         [TestMethod]
         [ExpectedException(typeof(ArgumentException))]
         public void Generate3DeletionsForShorterWord_EmptyList()
         {
            var generated = DictionaryTree.GenerateDeletions("cats", 3);
         }
      }

      [TestClass]
      public class AddWordMethod
      {
         private DictionaryTree dict;

         [TestInitialize]
         public void InitializeDictionay()
         {
            dict = new DictionaryTree(0);
         }

         [TestMethod]
         public void AddWord_AddsWord()
         {
            Assert.IsTrue(dict.AddWord("Hello"));
         }

         [TestMethod]
         public void AddWord_AddsMisprints()
         {
            dict.AddWord("hello");
            Assert.IsFalse(dict.AddWord("hello"));
         }
      }

      [TestClass]
      public class GetCorrectedWordMethod
      {
         private DictionaryTree dict = new DictionaryTree(2);

         [TestInitialize]
         public void InitializeDictionary()
         {
            String unsplittedStr = "rain spain plain plaint pain main mainly the in on fall falls his was";
            String[] strArr = unsplittedStr.Split(' ');
            foreach (var s in strArr)
            {
               dict.AddWord(s);
            }
         }

         [TestMethod]
         public void SearchForCorrectWord_ReturnOriginalWord()
         {
            Assert.AreEqual(dict.GetCorrectedWord("Rain"), "Rain");
         }

         [TestMethod]
         public void SearchForWordWithOneDelete_ReturnCorrectedWord()
         {
            Assert.AreEqual("pain", dict.GetCorrectedWord("pai"));
         }

         [TestMethod]
         public void SearchForWordWithOneDelete_ReturnCorrectedWords()
         {
            Assert.AreEqual("{rain in}", dict.GetCorrectedWord("rin"));
         }

         [TestMethod]
         public void SearchForWordWithOneInsert_ReturnCorrectedWord()
         {
            Assert.AreEqual("pain", dict.GetCorrectedWord("patin"));
         }

         [TestMethod]
         public void SearchForWordWithOneInsert_ReturnCorrectedWords()
         {
            Assert.AreEqual("{pain main}", dict.GetCorrectedWord("mpain"));
         }

         [TestMethod]
         public void SearchForWordWithOneDeleteOneInsert_ReturnCorrectedWord()
         {
            Assert.AreEqual("his", dict.GetCorrectedWord("hiz"));
         }

         [TestMethod]
         public void SearchForWordWithOneDeleteOneInsert_ReturnCorrectedWords()
         {
            Assert.AreEqual("{rain pain main}", dict.GetCorrectedWord("zain"));
         }

         [TestMethod]
         public void SearchForNonPresentedWord_ReturnOriginalWord()
         {
            Assert.AreEqual("{orange?}", dict.GetCorrectedWord("orange"));
         }

         [TestMethod]
         public void SearchForShortNonPresentedWord_ReturnOriginalWord()
         {
            Assert.AreEqual("{z?}", dict.GetCorrectedWord("z"));
         }
      }
   }
}
