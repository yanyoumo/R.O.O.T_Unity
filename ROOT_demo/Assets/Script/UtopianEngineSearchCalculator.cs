using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace UtopianEngine
{
    using ValInBox=Tuple<int,int>;
    public class UtopianEngineSearchCalculator : MonoBehaviour
    {
        int rollD6()
        {
            return Random.Range(1, 7);
        }

        (int, int) pairOfD6()
        {
            return (rollD6(), rollD6());
        }

        public Vector2Int firstPair;

        private (int, int)[] firstRollIndexLibSideA = new (int, int)[]
        {
            (0,1),
            (0,2),
            (0,3),
            (0,4),
            (0,5),
            (1,2),
            (1,3),
            (1,4),
            (1,5),
            (2,3),
            (2,4),
            (2,5),
            (3,4),
            (3,5),
            (4,5),
        };
        
        private List<int[]> Get2DiceAllPremutation()
        {
            var reslist = new List<int[]>();
            int[] res = {-1, 0};
            do
            {
                res[0]++;
                if (res[0] > 5)
                {
                    res[0] = 0;
                    res[1]++;
                }
                if (res[1] > 5)
                {
                    throw new InternalBufferOverflowException();
                }
                reslist.Add(new []{res[0],res[1]});
            } while (res[0] != 5 || res[1] != 5);
            return reslist;
        }
        
        private List<int[]> Get4DiceAllPremutation()
        {
            var reslist = new List<int[]>();
            int[] res = {-1, 0, 0, 0};
            do
            {
                res[0]++;
                if (res[0] > 5)
                {
                    res[0] = 0;
                    res[1]++;
                }
                if (res[1] > 5)
                {
                    res[1] = 0;
                    res[2]++;
                }
                if (res[2] > 5)
                {
                    res[2] = 0;
                    res[3]++;
                }
                if (res[3] > 5)
                {
                    throw new InternalBufferOverflowException();
                }
                reslist.Add(new []{res[0],res[1],res[2],res[3]});
            } while (res[0] != 5 || res[1] != 5 || res[2] != 5 || res[3] != 5);
            return reslist;
        }

        struct DataRes
        {
            public Vector2Int firstPairVal;
            public Vector2Int firstPairPos;
        
            //[0][1][2]
            //[3][4][5]
            public int[] BoxVals; 
            
            public float BadPer;
            public float critPer;
            public float goodPer;
            public float fairPer;

            public override string ToString()
            {
                return "You could place " + firstPairVal.x + " at Pos " + firstPairPos.x + ",then " + firstPairVal.y +
                       " at Pos " + firstPairPos.y + " then--" + "bad:" + BadPer + "% crit:" + critPer +
                       "% goodPer:" + goodPer + "% fairPer:" + fairPer + "%";
            }
        }

        [Button]
        void CalcFirstPairVal()
        {
            var totalRes = new List<DataRes>();
            //需要按照后两种位置进行便利。
            foreach (var valueTuple in firstRollIndexLibSideA)
            {
                var resA = CalcSecondPredict((firstPair.x, valueTuple.Item1), (firstPair.y, valueTuple.Item2));
                totalRes.Add(resA);
                var resB = CalcSecondPredict((firstPair.x, valueTuple.Item2), (firstPair.y, valueTuple.Item1));
                totalRes.Add(resB);
            }
            var orderByDescending = totalRes.OrderByDescending(v => v.goodPer+v.fairPer);
            foreach (var dataRes in orderByDescending)
            {
                Debug.Log(dataRes);
            }
        }

        [Button]
        void TestCalcThirdPairChance()
        {
            ValInBox A = new ValInBox(1, 0);
            ValInBox B = new ValInBox(5, 2);
            ValInBox C = new ValInBox(1, 3);
            ValInBox D = new ValInBox(3, 4);
            Debug.Log(CalcThirdPairChance(A,B,C,D));
        }

        DataRes CalcSecondPairChanceW2Rolls(ValInBox A,ValInBox B,int rollA,int rollB)
        {
            //这里需要为第二组的数据挑一组最好的结果。（对、按照什么标准挑呢？）
            throw new NotImplementedException();
        }
        
        DataRes CalcThirdPairChance(ValInBox A,ValInBox B,ValInBox C,ValInBox D)
        {
            float totalBadPer = 0.0f;
            float totalcritPer = 0.0f;
            float totalgoodPer = 0.0f;
            float totalfairPer = 0.0f;

            int badCount = 0;
            int critCount = 0;
            int goodCount = 0;
            int fairCount = 0;
            int Trial = 0;
            var Roll2 = Get2DiceAllPremutation();
            int[] val = new int[6];
            foreach (var ints in Roll2)
            {
                val = CalcThirdPairChanceW2Rolls(A, B, C, D, ints[0], ints[1]);
                int res = SearchBoxRes(val);
                if (res < 0 || res >= 100)
                {
                    badCount++;
                }
                else if (res == 0)
                {
                    critCount++;
                }
                else if (res <= 10)
                {
                    goodCount++;
                }
                else
                {
                    fairCount++;
                }

                Trial++;
            }

            totalBadPer = badCount / (float) Roll2.Count;
            totalcritPer = critCount / (float) Roll2.Count;
            totalgoodPer = goodCount / (float) Roll2.Count;
            totalfairPer = fairCount / (float) Roll2.Count;

            var resData = new DataRes
            {
                BoxVals = val,
                BadPer = totalBadPer * 100,
                critPer = totalcritPer * 100,
                goodPer = totalgoodPer * 100,
                fairPer = totalfairPer * 100
            };

            return resData;
        }

        int[] CalcThirdPairChanceW2Rolls(ValInBox A,ValInBox B,ValInBox C,ValInBox D,int RollValA,int RollValB)
        {
            int emptyPosA=0;
            int emptyPosB=0;
            for (int i = 0; i < 6; i++)
            {
                if (i != A.Item2 && i != B.Item2 && i != C.Item2 && i != D.Item2)
                {
                    emptyPosA = i;
                    break;
                }
            }

            for (int i = 0; i < 6; i++)
            {
                if (i != A.Item2 && i != B.Item2 && i != C.Item2 && i != D.Item2 && i != emptyPosA)
                {
                    emptyPosB = i;
                    break;
                }
            }

            ValInBox E0 = new ValInBox(RollValA, emptyPosA);
            ValInBox F0 = new ValInBox(RollValB, emptyPosB);
            
            ValInBox E1 = new ValInBox(RollValA, emptyPosB);
            ValInBox F1 = new ValInBox(RollValB, emptyPosA);
            
            var res0 = new int[6];
            res0[A.Item2] = A.Item1;
            res0[B.Item2] = B.Item1;
            res0[C.Item2] = C.Item1;
            res0[D.Item2] = D.Item1;
            res0[E0.Item2] = E0.Item1;
            res0[F0.Item2] = F0.Item1;
            
            var res1 = new int[6];
            res1[A.Item2] = A.Item1;
            res1[B.Item2] = B.Item1;
            res1[C.Item2] = C.Item1;
            res1[D.Item2] = D.Item1;
            res1[E1.Item2] = E1.Item1;
            res1[F1.Item2] = F1.Item1;

            int val0 = SearchBoxRes(res0);
            int val1 = SearchBoxRes(res1);

            if ((val0 < 0 && val1 < 0) || (val0 > 0 && val1 > 0))
            {
                return Math.Abs(val0) <= Math.Abs(val1) ? res0 : res1;
            }
            else
            {
                return val0 > val1 ? res0 : res1;
            }
        }

        //[0][1][2]
        //[3][4][5]
        
        private readonly (int, int, int, int)[] _afterFirstRollIndexLib = new (int, int, int, int)[]
        {
            (0,1,2,3),
            (1,0,2,3),
            (2,0,1,3),
            (0,2,1,3),
            (1,2,0,3),
            (2,1,0,3),
            (2,1,3,0),
            (1,2,3,0),
            (3,2,1,0),
            (2,3,1,0),
            (1,3,2,0),
            (3,1,2,0),
            (3,0,2,1),
            (0,3,2,1),
            (2,3,0,1),
            (3,2,0,1),
            (0,2,3,1),
            (2,0,3,1),
            (1,0,3,2),
            (0,1,3,2),
            (3,1,0,2),
            (1,3,0,2),
            (0,3,1,2),
            (3,0,1,2),
        };

        int SearchBoxRes(int[] boxContent)
        {
            var resT1 = boxContent[0] * 100 + boxContent[1] * 10 + boxContent[2];
            var resT2 = boxContent[3] * 100 + boxContent[4] * 10 + boxContent[5];
            return resT1 - resT2;
        }

        (float,float,float,float) CalcSecondPredictW4Rolls((int, int) firstValAndPosA, (int, int) firstValAndPosB, int[] FourRolls)
        {
            var nextPos = new int[4];
            int j = 0;
            for (int i = 0; i < 6; i++)
            {
                if (i != firstValAndPosA.Item2 && i != firstValAndPosB.Item2)
                {
                    nextPos[j] = i;
                    j++;
                }
            }

            int Trial = 0;
            int badCount = 0;
            int critCount = 0;
            int goodCount = 0;
            int fairCount = 0;
            foreach (var valueTuple in _afterFirstRollIndexLib)
            {
                var boxContent = new int[6];
                boxContent[firstValAndPosA.Item2] = firstValAndPosA.Item1;
                boxContent[firstValAndPosB.Item2] = firstValAndPosB.Item1;
                boxContent[nextPos[valueTuple.Item1]] = FourRolls[0];
                boxContent[nextPos[valueTuple.Item2]] = FourRolls[1];
                boxContent[nextPos[valueTuple.Item3]] = FourRolls[2];
                boxContent[nextPos[valueTuple.Item4]] = FourRolls[3];
                int res = SearchBoxRes(boxContent);
                if (res < 0 || res >= 100)
                {
                    badCount++;
                }
                else if (res == 0)
                {
                    critCount++;
                }
                else if (res <= 10)
                {
                    goodCount++;
                }
                else
                {
                    fairCount++;
                }
                Trial++;
            }
            float badPer = (badCount / (float) Trial) * 100;
            float critPer = (critCount / (float) Trial) * 100;
            float goodPer = (goodCount / (float) Trial) * 100;
            float fairPer = (fairCount / (float) Trial) * 100;
            //Debug.Log("bad:" + badPer + "% crit:" + critPer + "% goodPer:" + goodPer + "% fairPer:" + fairPer+"%");
            return (badPer, critPer, goodPer, fairPer);
        }
        
        DataRes CalcSecondPredict((int,int) firstValAndPosA,(int,int) firstValAndPosB)
        {
            float totalBadPer=0.0f;
            float totalcritPer=0.0f;
            float totalgoodPer=0.0f;
            float totalfairPer=0.0f;
            var allPre = Get4DiceAllPremutation();
            foreach (var ints in allPre)
            {
                var (TmptotalBadPer, TmptotalcritPer, TmptotalgoodPer, TmptotalfairPer)=CalcSecondPredictW4Rolls(firstValAndPosA, firstValAndPosB, ints);
                totalBadPer += TmptotalBadPer;
                totalcritPer += TmptotalcritPer;
                totalgoodPer += TmptotalgoodPer;
                totalfairPer += TmptotalfairPer;
            }
            totalBadPer /= allPre.Count;
            totalcritPer /= allPre.Count;
            totalgoodPer /= allPre.Count;
            totalfairPer /= allPre.Count;
            //Debug.Log("bad:" + totalBadPer + "% crit:" + totalcritPer + "% goodPer:" + totalgoodPer + "% fairPer:" + totalfairPer+"%");
            var res = new DataRes
            {
                firstPairVal = new Vector2Int(firstValAndPosA.Item1, firstValAndPosB.Item1),
                firstPairPos = new Vector2Int(firstValAndPosA.Item2, firstValAndPosB.Item2),
                BadPer = totalBadPer,
                critPer = totalcritPer,
                goodPer = totalgoodPer,
                fairPer = totalfairPer
            };
            return res;
        }
        
        #region HumanTechniqueTest

        int doSearch_pureRandom()
        {
            var res = new int[6];
            for (var i = 0; i < res.Length; i++)
            {
                res[i] = rollD6();
            }
            var resT1 = res[0] * 100 + res[1] * 10 + res[2];
            var resT2 = res[3] * 100 + res[4] * 10 + res[5];
            return resT1 - resT2;
        }

        int doSearch_BasicTechniqueA()
        {
            var res = new int[6];
            int valA0 = rollD6();
            int valA1 = rollD6();
            res[0] = valA0 > valA1 ? valA0 : valA1;
            res[3] = valA0 < valA1 ? valA0 : valA1;
            int valA2 = rollD6();
            int valA3 = rollD6();
            res[1] = valA2 > valA3 ? valA2 : valA3;
            res[4] = valA2 < valA3 ? valA2 : valA3;
            int valA4 = rollD6();
            int valA5 = rollD6();
            res[2] = valA4 > valA5 ? valA4 : valA5;
            res[5] = valA4 < valA5 ? valA4 : valA5;
            var resT1 = res[0] * 100 + res[1] * 10 + res[2];
            var resT2 = res[3] * 100 + res[4] * 10 + res[5];
            return resT1 - resT2;
        }
        
        int doSearch_BasicTechniqueB()
        {
            var res = new int[] {0, 0, 0, 0, 0, 0};

            int valA0 = rollD6();
            int valA1 = rollD6();
            int delA = Math.Abs(valA0 - valA1);
            if (delA<=1)
            {
                res[0] = valA0 >= valA1 ? valA0 : valA1;
                res[3] = valA0 >= valA1 ? valA1 : valA0;
            }
            else if (delA <= 3)
            {
                res[1] = valA0 >= valA1 ? valA0 : valA1;
                res[4] = valA0 >= valA1 ? valA1 : valA0;
            }
            else
            {
                res[2] = valA0 >= valA1 ? valA0 : valA1;
                res[5] = valA0 >= valA1 ? valA1 : valA0;
            }

            int valB0 = rollD6();
            int valB1 = rollD6();
            int delB = Math.Abs(valA0 - valA1);

            if (delB<=1)
            {
                if (res[0]!=0)
                {
                    res[1] = valB0 >= valB1 ? valB1 : valB0;
                    res[4] = valB0 >= valB1 ? valB0 : valB1;
                }
                else
                {
                    res[0] = valB0 >= valB1 ? valB0 : valB1;
                    res[3] = valB0 >= valB1 ? valB1 : valB0;
                }
            }
            else if (delA <= 3)
            {
                if (res[0] != 0)
                {
                    res[1] = valB0 >= valB1 ? valB0 : valB1;
                    res[4] = valB0 >= valB1 ? valB1 : valB0;
                }
                else
                {
                    res[2] = valB0 >= valB1 ? valB0 : valB1;
                    res[5] = valB0 >= valB1 ? valB1 : valB0;
                }
            }
            else
            {
                if (res[2]!=0)
                {
                    res[1] = valB0 >= valB1 ? valB0 : valB1;
                    res[4] = valB0 >= valB1 ? valB1 : valB0;
                }
                else
                {
                    res[2] = valB0 >= valB1 ? valB0 : valB1;
                    res[5] = valB0 >= valB1 ? valB1 : valB0;
                }
            }
            
            int valC0 = rollD6();
            int valC1 = rollD6();

            int indexC0 = -1;
            int indexC1 = -1;
            
            for (int i = 0; i < 6; i++)
            {
                if (res[i]==0)
                {
                    res[i] = valC0;
                    indexC0 = i;
                    break;
                }
            }
            
            for (int i = 0; i < 6; i++)
            {
                if (res[i]==0)
                {
                    res[i] = valC1;
                    indexC1 = i;
                    break;
                }
            }
            
            var resTC0 = res[0] * 100 + res[1] * 10 + res[2];
            var resTC1 = res[3] * 100 + res[4] * 10 + res[5];
            var resTCA=resTC0 - resTC1;

            var tmp = res[indexC1];
            res[indexC1] = res[indexC0];
            res[indexC0] = tmp;
            
            var resTC2 = res[0] * 100 + res[1] * 10 + res[2];
            var resTC3 = res[3] * 100 + res[4] * 10 + res[5];
            var resTCB= resTC2 - resTC3;
            
            if (resTCA<0&&resTCB<0)
            {
                return resTCA;
            }
            if (resTCA < 0 || resTCB < 0)
            {
                return resTCA >= 0 ? resTCA : resTCB;
            }
            return resTCA <= resTCB ? resTCA : resTCB;
        }
        
        int doSearch_SemiTheoreticalBest()
        {
            var raw_res = new int[6];
            for (var i = 0; i < raw_res.Length; i++)
            {
                raw_res[i] = rollD6();
            }

            var indexLib = new (int, int)[15];
            indexLib[0] = (0, 1);
            indexLib[1] = (0, 2);
            indexLib[2] = (0, 3);
            indexLib[3] = (0, 4);
            indexLib[4] = (0, 5);
            indexLib[5] = (1, 2);
            indexLib[6] = (1, 3);
            indexLib[7] = (1, 4);
            indexLib[8] = (1, 5);
            indexLib[9] = (2, 3);
            indexLib[10] = (2, 4);
            indexLib[11] = (2, 5);
            indexLib[12] = (3, 4);
            indexLib[13] = (3, 5);
            indexLib[14] = (4, 5);

            var dels = new (int, int)[15];
            for (var i = 0; i < indexLib.Length; i++)
            {
                dels[i].Item1 = Math.Abs(raw_res[indexLib[i].Item1] - raw_res[indexLib[i].Item2]);
                dels[i].Item2 = i;
            }

            var delOrdered = dels.OrderBy(v => v.Item1).ToArray();

            (int, int) first = (0, 0);
            (int, int) second = (0, 0);
            (int, int) third = (0, 0);

            first = indexLib[delOrdered[0].Item1];
            int j = 0;
            for (int i = 1; i < delOrdered.Length; i++)
            {
                var tmp = indexLib[delOrdered[i].Item1];
                if (first.Item1 != tmp.Item1 && first.Item1 != tmp.Item2)
                {
                    if (first.Item2 != tmp.Item1 && first.Item2 != tmp.Item2)
                    {
                        second = tmp;
                        j = 1;
                    }
                }
            }

            for (int i = j + 1; i < delOrdered.Length; i++)
            {
                var tmp = indexLib[delOrdered[i].Item1];
                if (second.Item1 != tmp.Item1 && second.Item1 != tmp.Item2)
                {
                    if (second.Item2 != tmp.Item1 && second.Item2 != tmp.Item2)
                    {
                        third = tmp;
                    }
                }
            }

            var res = new int[6];
            res[0] = raw_res[first.Item1] >= raw_res[first.Item2] ? raw_res[first.Item1] : raw_res[first.Item2];
            res[3] = raw_res[first.Item1] >= raw_res[first.Item2] ? raw_res[first.Item2] : raw_res[first.Item1];
            res[1] = raw_res[second.Item1] >= raw_res[second.Item2] ? raw_res[second.Item2] : raw_res[second.Item1];
            res[4] = raw_res[second.Item1] >= raw_res[second.Item2] ? raw_res[second.Item1] : raw_res[second.Item2];
            res[2] = raw_res[third.Item1] >= raw_res[third.Item2] ? raw_res[third.Item2] : raw_res[third.Item1];
            res[5] = raw_res[third.Item1] >= raw_res[third.Item2] ? raw_res[third.Item1] : raw_res[third.Item2];

            var resT1 = res[0] * 100 + res[1] * 10 + res[2];
            var resT2 = res[3] * 100 + res[4] * 10 + res[5];
            return resT1 - resT2;
        }

        void HumanSearchTechniqueTest(Func<int> technique,string techniqueName)
        {
            int Trial = 1000000;
            int badCount = 0;
            int critCount = 0;
            int goodCount = 0;
            int fairCount = 0;
            for (int i = 0; i < Trial; i++)
            {
                int res = technique();
                if (res < 0||res>=100)
                {
                    badCount++;
                }
                else if (res == 0)
                {
                    critCount++;
                }
                else if(res<=10)
                {
                    goodCount++;
                }
                else
                {
                    fairCount++;
                }
            }

            float badPer = (badCount / (float) Trial) * 100;
            float critPer = (critCount / (float) Trial) * 100;
            float goodPer = (goodCount / (float) Trial) * 100;
            float fairPer = (fairCount / (float) Trial) * 100;
            Debug.Log("Using "+techniqueName+"--bad:" + badPer + "% crit:" + critPer +
                      "% goodPer:" + goodPer + "% fairPer:" + fairPer+"%");
        }
        
        void SemiTheoreticalBest()
        {
            HumanSearchTechniqueTest(doSearch_SemiTheoreticalBest,"Semi-Theoretical Best");
        }
        
        void PureRandomSearch()
        {
            HumanSearchTechniqueTest(doSearch_pureRandom,"Random Method Searching");
        }

        void BasicTechniqueA()
        {
            HumanSearchTechniqueTest(doSearch_BasicTechniqueA,"Basic TechniqueA");
        }

        void BasicTechniqueB()
        {
            HumanSearchTechniqueTest(doSearch_BasicTechniqueB, "Basic TechniqueB");
        }

        #endregion
    }
}

