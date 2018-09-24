using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SpellChecker.UnitTests
{
   [TestClass]
   public class CorrectorTests
   {
      [TestClass]
      public class MaxDeletionsForWordLengthMethod
      {
         [TestMethod]
         public void Length0Max5_Return0()
         {
            Assert.AreEqual(Corrector.MaxDeletionsForWordLength(0, 5), 0);
         }

         [TestMethod]
         public void Length1Max2_Return1()
         {
            Assert.AreEqual(Corrector.MaxDeletionsForWordLength(1, 2), 1);
         }

         [TestMethod]
         public void Length5Max10_Return3()
         {
            Assert.AreEqual(Corrector.MaxDeletionsForWordLength(5, 10), 3);
         }

         [TestMethod]
         public void Length5Max2_Return2()
         {
            Assert.AreEqual(Corrector.MaxDeletionsForWordLength(5, 2), 2);
         }

         [TestMethod]
         public void Length10_Return5()
         {
            Assert.AreEqual(Corrector.MaxDeletionsForWordLength(10), 5);
         }
      }

      [TestClass]
      public class GenerateDeletionsMethod
      {
         [TestMethod]
         public void Generate1DeletionForWord_EquivalentGivenList()
         {
            var generated = Corrector.GenerateDeletions("orange", 1);
            List<String> equivalent = new List<String>()
            {
               "range", "oange", "ornge", "orage", "orane", "orang"
            };
            CollectionAssert.AreEquivalent(generated, equivalent);
         }

         [TestMethod]
         public void Generate2DeletionsForWord_EquivalentGivenList()
         {
            var generated = Corrector.GenerateDeletions("orange", 2);
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
            var generated = Corrector.GenerateDeletions("oranges", 3);
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
            var generated = Corrector.GenerateDeletions("cats", 3);
            Assert.IsTrue(generated.Count == 0);
         }
      }

      [TestClass]
      public class IsPossibleMisprintMethod
      {
         [TestMethod]
         public void IsPossibleForOneDeletion_ReturnTrue()
         {
            Assert.IsTrue(Corrector.IsPossibleMissprint("Hello", "hllo", 1));
         }

         [TestMethod]
         public void IsPossibleForTwoSeparateDeletions_ReturnTrue()
         {
            Assert.IsTrue(Corrector.IsPossibleMissprint("Hello", "hlo", 2));
         }

         [TestMethod]
         public void IsPossibleForTwoAdjastedDeletions_ReturnFalse()
         {
            Assert.IsFalse(Corrector.IsPossibleMissprint("Hello", "heo", 2));
         }

         [TestMethod]
         public void IsPossibleForFirstDeleted_ReturnTrue()
         {
            Assert.IsTrue(Corrector.IsPossibleMissprint("hello", "ello", 1));
         }

         [TestMethod]
         public void IsPossibleForLastDeleted_ReturnTrue()
         {
            Assert.IsTrue(Corrector.IsPossibleMissprint("hello", "hell", 1));
         }

         [TestMethod]
         public void IsPossibleForTwoSeparateWithSameLetter_returnTrue()
         {
            Assert.IsTrue(Corrector.IsPossibleMissprint("helll", "hel", 2));
         }

         [TestMethod]
         public void IsPossibleForThreeAdjastedWithSameLetterAtTheEnd_returnFalse()
         {
            Assert.IsFalse(Corrector.IsPossibleMissprint("hellll", "hel", 3));
         }

         [TestMethod]
         public void IsPossibleForThreeAdjastedWithSameLetterAtTheBegin_returnFalse()
         {
            Assert.IsFalse(Corrector.IsPossibleMissprint("hhhhelo", "helo", 3));
         }
      }

      [TestClass]
      public class AddWordMethod
      {
         private Corrector dict;

         [TestInitialize]
         public void InitializeDictionay()
         {
            dict = new Corrector(0);
         }

         [TestMethod]
         public void AddWord_ReturnTrue()
         {
            Assert.IsTrue(dict.AddWord("Hello"));
         }

         [TestMethod]
         public void AddSameWord_ReturnFalse()
         {
            dict.AddWord("hello");
            Assert.IsFalse(dict.AddWord("hello"));
         }
      }

      [TestClass]
      public class GetCorrectedWordMethod
      {
         private Corrector dict = new Corrector(2);

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
            Assert.AreEqual("Rain", dict.GetCorrectedWord("Rain"));
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
