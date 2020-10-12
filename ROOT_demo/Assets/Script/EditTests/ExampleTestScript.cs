using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using ROOT;

namespace Tests
{
    public class ExampleTestScript
    {
        private int[] inputArray;
        private string[] outputArray;
        [SetUp]
        public void Setup() 
        {
            inputArray = new int[] { 10000, 1000, 100, 10, 1 };
            outputArray = new string[] { "????", "1000", "0100", "0010", "0001" };
        }
        // A Test behaves as an ordinary method
        [Test]
        public void PaddingNum4DigitTest()
        {
            // Use the Assert class to test conditions
            for (int index = 0; index < inputArray.Length; index++) {
                Assert.AreEqual(outputArray[index], ROOT.Utils.PaddingNum4Digit(inputArray[index]));
            }
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator ExampleTestScriptWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }
    }
}
