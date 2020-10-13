using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ROOT;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class BoardGistTestScript
    {
        private int[] BoardLengthList={2,3,4,5,6,7,8};

        private int[][] NotVaildIDAnswer =
        {
            new []{0,2,3,7},
            new []{0,2,4,5,11,17},
            new []{0,2,4,6,7,15,23,31},
            new []{0,2,4,6,8,9,19,29,39,49},
            new []{0,2,4,6,8,10,11,23,35,47,59,71},
            new []{0,2,4,6,8,10,12,13,27,41,55,69,83,97},
            new []{0,2,4,6,8,10,12,14,15,31,47,63,79,95,111,127}
        };

        // A Test behaves as an ordinary method
        [Test]
        public void BoardGistGenerationTest()
        {
            // Use the Assert class to test conditions
            for (var i = 0; i < BoardLengthList.Length; i++)
            {
                var boardGist = new BoardGist(BoardLengthList[i]);
                for (var i1 = 0; i1 < boardGist.ConnectionList.Length; i1++)
                {
                    if (NotVaildIDAnswer[i].Contains(i1))
                    {
                        Assert.False(boardGist.ConnectionList[i1].HasValue, "ID=" + i1 + " in Length=" + i + " is not correct");
                    }
                    else
                    {
                        Assert.True(boardGist.ConnectionList[i1].HasValue, "ID=" + i1 + " in Length=" + i + " is not correct");
                    }
                }
            }
        }


    }
}
