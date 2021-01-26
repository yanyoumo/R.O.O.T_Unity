using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ROOT;
using UnityEngine;
using UnityEngine.TestTools;
using Random=UnityEngine.Random;

namespace Tests
{
    public partial class BoardRelatedScript
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
            for(var i = 0; i < BoardLengthList.Length; i++)
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

        private void RandomCore(out SignalType signal,out CoreGenre genre)
        {
            Array valuesA = Enum.GetValues(typeof(SignalType));
            Array valuesB = Enum.GetValues(typeof(CoreGenre));
            signal = (SignalType) valuesA.GetValue(Mathf.FloorToInt(Random.value * valuesA.Length));
            genre = (CoreGenre) valuesB.GetValue(Mathf.FloorToInt(Random.value * valuesB.Length));
        }

        private RotationDirection RandomDir()
        {
            var val = Random.value;
            if (val<0.25f)
            {
                return RotationDirection.East;
            }
            else if (val < 0.5f)
            {
                return RotationDirection.South;
            }
            else if (val < 0.75f)
            {
                return RotationDirection.West;
            }
            else
            {
                return RotationDirection.North;
            }
        }

        [Test]
        public void BoardGistSimpleIOTest()
        {
            for (var i = 0; i < BoardLengthList.Length; i++)
            {
                var boardGist = new BoardGist(BoardLengthList[i]);
                var randomX = Mathf.FloorToInt(Random.value * BoardLengthList[i]);
                var randomY = Mathf.FloorToInt(Random.value * BoardLengthList[i]);
                var pos = new Vector2Int(randomX, randomY);
                RandomCore(out var signal,out var genre);
                boardGist.SetCoreType(pos, signal, genre);
                var outCoreType = boardGist.GetCoreType(pos);
                //Assert.AreEqual(coreType, outCoreType);
                Vector2Int diffPos;
                do
                {
                    randomX = Mathf.FloorToInt(Random.value * BoardLengthList[i]);
                    randomY = Mathf.FloorToInt(Random.value * BoardLengthList[i]);
                    diffPos = new Vector2Int(randomX, randomY);
                } while (diffPos == pos);
                var outCoreTypeB = boardGist.GetCoreType(diffPos);
                //Assert.Null(outCoreTypeB);

                var randomCount = 100;
                for (var j = 0; j < randomCount; j++)
                {
                    randomX = Mathf.FloorToInt(Random.value * BoardLengthList[i]);
                    randomY = Mathf.FloorToInt(Random.value * BoardLengthList[i]);
                    pos = new Vector2Int(randomX, randomY);
                    var randbool = Random.value > 0.5;
                    var dir = RandomDir();
                    var success=boardGist.SetConnectivity(pos, dir, randbool);
                    var outBool=boardGist.GetConnectivity(pos, dir);
                    if (success)
                    {
                        Assert.True(outBool.HasValue);
                        Assert.AreEqual(randbool, outBool.Value);
                    }
                    else
                    {
                        Assert.False(outBool.HasValue);
                    }
                }
            }
        }

        [Test]
        public void BoardGistComplexIOTest()
        {
            for (var i = 0; i < BoardLengthList.Length; i++)
            {
                var boardGist = new BoardGist(BoardLengthList[i]);
                var randomCount = 100;
                for (var j = 0; j < randomCount; j++)
                {
                    var randomX = Mathf.FloorToInt(Random.value * BoardLengthList[i]);
                    var randomY = Mathf.FloorToInt(Random.value * BoardLengthList[i]);
                    var pos = new Vector2Int(randomX, randomY);

                    var randDir = RandomDir();
                    var randbool = Random.value > 0.5;

                    var success = boardGist.SetConnectivity(pos, randDir, randbool);

                    if (success)
                    {
                        var nextPos = pos + Utils.ConvertDirectionToBoardPosOffset(randDir);
                        var invertDir = Utils.GetInvertDirection(randDir);
                        var readRes = boardGist.GetConnectivity(nextPos, invertDir);
                        Assert.True(readRes.HasValue, "HasValue:pos=" + pos + ",dir=" + randDir + ",next=" + nextPos + ",invDir=" + invertDir + ";");
                        Assert.AreEqual(randbool, readRes.Value, "readRes:pos=" + pos + ",dir=" + randDir + ";");
                    }
                    else
                    {
                        var outBool = boardGist.GetConnectivity(pos, randDir);
                        Assert.False(outBool.HasValue);
                    }
                }
            }
        }

        /*[Test]
        */
    }
}
