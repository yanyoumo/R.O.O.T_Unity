using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using UnityEngine;

namespace ROOT.Signal
{
    using SignalDataPack = Tuple<int, int, int, Unit>;
    public abstract class SignalAssetBase : MonoBehaviour
    {
        public Type UnitSignalCoreType { protected set; get; }

        public abstract SignalType SignalType { get; }

        public UnitAsset CoreUnitAsset;
        public UnitAsset FieldUnitAsset;

        private int RectifyInt(int a)
        {
            return a == int.MaxValue ? 0 : a;
        }

        private (int, int, int) rectifyIntTuple((int, int, int) a)
        {
            return (RectifyInt(a.Item1), RectifyInt(a.Item2), RectifyInt(a.Item3));
        }

        //在LED屏幕上是否显示本信号的的逻辑。
        public virtual bool ShowSignal(RotationDirection dir, Unit unit, Unit otherUnit)
        {
            var checkA = unit.SignalCore.HasCertainSignal(SignalType) && otherUnit.SignalCore.HasCertainSignal(SignalType);
            /*var signalStrengthComplex = rectifyIntTuple(unit.SignalCore.CorrespondingSignalData);
            var othersignalStrengthComplex = rectifyIntTuple(otherUnit.SignalCore.CorrespondingSignalData);*/
            //var delTieredSignal = Math.Abs(othersignalStrengthComplex.Item3 - signalStrengthComplex.Item3);
            //var signalTier = unit.UnitSignal == SignalType ? unit.Tier : 0;
            return checkA;
        }
        public virtual int SignalVal(RotationDirection dir, Unit unit, Unit otherUnit)
        {
            var showSig = ShowSignal(dir, unit, otherUnit);
            var ValA = unit.SignalCore.SignalDataPackList[SignalType.Matrix].Item3;
            var ValB = otherUnit.SignalCore.SignalDataPackList[SignalType.Matrix].Item3;
            return showSig ? Math.Max(ValA, ValB) : 0;
        }

        public virtual float CalAllScore(Board gameBoard, out int hardwareCount)
        {
            var targetSignalCore = gameBoard.Units.Where(u => u.UnitSignal == SignalType).Select(u => u.SignalCore).ToArray();
            hardwareCount = targetSignalCore.Count(s => s.IsUnitActive);
            return targetSignalCore.Sum(s => s.SingleUnitScore);
        }

        public float CalAllScore(Board gameBoard)
        {
            return CalAllScore(gameBoard, out var A);
        }



        //把新的流程在这里再正式化一下：
        //目的：标准量化多个种类信号的信号值；对齐不同信号的计分时序；并且为一些简单、通用计分标准提供大幅简化的基本。
        //流程：所有计分流程都是以这套强度修改的时序来同步；具体表现为、所有计分还是都以【强度数据+单元连接拓扑】；
        //     保证这些数据都是同步更新的、那么所有计分流程和数据的更新就会是同步的。
        //实现：在每个单元上都有一个类型为：Dictionary<SignalType, (int, int)> 的变量。
        //      key是SignalType；自然就是存储信号类型。
        //      value是(int, int)；里面两个数据都是int大体上都是深度，第一个为“硬件深度”、第二个为“信号深度”。
        //      深度的具体含义为：场单元到任意一个对应信号核心的最短距离；
        //          ·硬件深度是所有路径中，任意场单元数量最少路径上场单元的计数。
        //          ·信号深度是所有路径中，【对应信号场单元】数量最少路径上【对应单元场信号】的计数。
        //      *相邻与某个信号核心单元的对应硬件和信号深度都为1。
        //     **核心单元也需要如此进行计算、何其不对应信号时、视为一个场单元；如果信号是自己对应的、那么两个深度都是0.
        //备注：这个深度的计算流程（信号深度）和现有扫描信号的“必要最短”很像，所以可以从代码复用角度搞一搞。
        //      即使现有“必要最短”函数复杂度是O(n)，那么这个计算强度的总复杂度就是：
        //          per信号*per场单元*per核心单元*O(n);理论上高达O(n^4)。
        //          但是考虑到核心单元和信号的实际数量不会太高，就先实现出来，再优化。
        public void RefreshBoardSignalStrength(Board board)
        {
            //而且除了最后具体存储的两个int、所有别的数据最好都写成类变量。
            var signalType = SignalType;
            board.Units.ForEach(unit => unit.SignalCore.SignalDataPackList[signalType] = new SignalDataPack(int.MaxValue, 0, 0, null));
            foreach (var coreUnit in board.FindUnitWithCoreType(SignalType, HardwareType.Core))
            {
                board.Units.ForEach(unit => unit.SignalCore.Visited = false);
                var queue = new Queue<Unit>();
                coreUnit.SignalCore.SignalDataPackList[signalType] = new SignalDataPack(0, 0, 0, null);
                coreUnit.SignalCore.Visited = true;
                queue.Enqueue(coreUnit);
                while (queue.Count != 0)
                {
                    var now = queue.Dequeue();
                    var physicalDepth = now.SignalCore.SignalDataPackList[signalType].Item1;
                    var scoringDepth = now.SignalCore.SignalDataPackList[signalType].Item2;
                    var tieredDepth = now.SignalCore.SignalDataPackList[signalType].Item3;
                    foreach (var unit in now.GetConnectedOtherUnit.Where(unit => unit.SignalCore.Visited == false))
                    {
                        unit.SignalCore.Visited = true;
                        if (unit.UnitSignal == signalType && unit.UnitHardware == HardwareType.Core)
                            continue;
                        var item1 = unit.SignalCore.SignalDataPackList[signalType].Item1;
                        var item2 = unit.SignalCore.SignalDataPackList[signalType].Item2;
                        var item3 = unit.SignalCore.SignalDataPackList[signalType].Item3;
                        var renew = false;
                        if (physicalDepth + 1 < item1)
                        {
                            item1 = physicalDepth + 1;
                            renew = true;
                            if (unit.UnitSignal == signalType && unit.UnitHardware == HardwareType.Field)
                            {
                                item3 = tieredDepth + unit.Tier;
                                item2 = scoringDepth + 1;
                            }

                            else
                            {
                                item3 = tieredDepth;
                                item2 = scoringDepth;
                            }
                        }

                        else if (physicalDepth + 1 == item1)
                        {
                            if (unit.UnitSignal == signalType && unit.UnitHardware == HardwareType.Field)
                            {
                                if (tieredDepth + unit.Tier > item3)
                                {
                                    item3 = tieredDepth + unit.Tier;
                                    item2 = scoringDepth + 1;
                                    renew = true;
                                }

                                else if (tieredDepth + unit.Tier == item3 && scoringDepth + 1 < item2)
                                {
                                    item2 = scoringDepth + 1;
                                    renew = true;
                                }
                            }

                            else
                            {
                                if (tieredDepth > item3)
                                {
                                    item3 = tieredDepth;
                                    item2 = scoringDepth;
                                    renew = true;
                                }

                                else if (scoringDepth < item2)
                                {
                                    item2 = scoringDepth;
                                    renew = true;
                                }
                            }
                        }

                        if (!renew) continue;
                        unit.SignalCore.SignalDataPackList[signalType] = new SignalDataPack(item1, item2, item3, now);
                        queue.Enqueue(unit);
                    }
                }
            }

            //就是把所有都maxValue了；这个还是遍历。
            foreach (var fieldUnit in board.Units)
            {
                if (Equals(fieldUnit.SignalCore.SignalDataPackList[signalType], new SignalDataPack(int.MaxValue, 0, 0, null)))
                    fieldUnit.SignalCore.SignalDataPackList[signalType] = new SignalDataPack(0, 0, 0, null);
            }

            var maxHardwareDepth = board.Units.Max(u => u.SignalCore.CertainSignalData(signalType).Item1);
            foreach (var unit in board.Units.Where(u => u.SignalCore.CertainSignalData(signalType).Item1 == maxHardwareDepth))
            {
                unit.SetInSignalTypeMesh_Iter(signalType);
            }

            foreach (var boardUnit in board.Units)
            {
                if (boardUnit.UnitSignal == signalType && boardUnit.GetConnectedOtherUnit.Count == 1)
                {
                    Debug.Log(signalType + "/" + boardUnit.CurrentBoardPosition);
                    boardUnit.SetInSignalTypeMesh_Iter(signalType);
                }
            }
        }
    }
}