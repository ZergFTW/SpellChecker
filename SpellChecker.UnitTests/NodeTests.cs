using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SpellChecker.UnitTests
{
   [TestClass]
   public class NodeTests
   {
      [TestClass]
      public class AddGetMisprintsMethods
      {
         [TestMethod]
         public void AddMisprint_GetSameMisprint()
         {
            Node correctNode = new Node
            {
               Word = "Hello"
            };

            Node misprintedNode = new Node
            {
               Word = "ello"
            };

            misprintedNode.AddMisprint(1, ref correctNode);
            Assert.AreEqual(correctNode, misprintedNode.GetMisprints(1)[0]);
         }
      }
   }
}  
