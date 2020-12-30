using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
// ReSharper disable PossibleNullReferenceException

namespace ROOT
{
    public class ScanSignalAsset : SignalAssetBase
    {
        void Awake()
        {
            UnitSignalCoreType = gameObject.AddComponent<ScanUnitSignalCore>().GetType();
        }

        //����Ĵ���δ����Ҫ���ǵ���Щ���ݵ�λ�û�䡣
        public override SignalType Type => SignalType.Scan;
        public override CoreType CoreUnitType => CoreType.Server;
        public override CoreType FieldUnitType => CoreType.NetworkCable;

        public override bool ShowSignal(RotationDirection dir, Unit unit, Unit otherUnit)
        {
            //������������ģ���Ҫ����Ϊ֮ǰΪ�˱����ƽ�����ǿ��һ��һ����������ô���û������Tier�������ݡ�
            //��������ҪServerDepth��HardwareDepth����ƽ�����ݡ��ٷ���������������ź��Ǳߣ���һ��FromDir��
            var ShowNetLED = unit.SignalCore.InServerGrid && otherUnit.SignalCore.InServerGrid;
            ShowNetLED &= Math.Abs(unit.SignalCore.ServerSignalDepth - otherUnit.SignalCore.ServerSignalDepth) <= 1;
            return ShowNetLED;
        }
        public override int SignalVal(RotationDirection dir, Unit unit, Unit otherUnit)
        {
            var showSig = ShowSignal(dir, unit, otherUnit);
            return showSig ? Math.Min(unit.SignalCore.ServerSignalDepth, otherUnit.SignalCore.ServerSignalDepth) : 0;
        }

        public override float CalAllScore(Board gameBoard, out int hardwareCount)
        {
            int maxCount = Board.BoardLength * Board.BoardLength;
            var maxScore = Int32.MinValue;
            var maxLength = maxCount;
            float res = 0;
            foreach (var signalCore in gameBoard.FindUnitWithCoreType(CoreUnitType).Select(unit => unit.SignalCore as ScanUnitSignalCore))
            {
                //���ˣ���Ҫ��CalScore����֮����Ҫ�������������ݡ�
                //������Ը�ģ�дһ��CalScore����д������д����Ҫ��out���������ˡ�
                //������ScanUnitSignalCore.cs��129���������˸�������ݡ�
                res = signalCore.CalScore(out var count, ref maxCount, ref maxScore, ref maxLength);
            }
            BoardDataCollector.MaxNetworkDepth = hardwareCount = maxScore;
            return res;
        }

    }
}