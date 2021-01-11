using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace ROOT
{
    public class SignalMasterMgr : MonoBehaviour
    {
        [NotNull] private static SignalMasterMgr _instance;
        public static SignalMasterMgr Instance => _instance;
        public static int MaxNetworkDepth;

        public static float GetPerDriverIncome = 1.5f;

        void Awake()
        {
            DontDestroyOnLoad(gameObject);
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                _instance = this;
            }
        }

        private Dictionary<SignalType, SignalAssetBase> signalAssetLib;
        public SignalType[] SignalLib => signalAssetLib.Keys.ToArray();

        public void UnitTypeFromSignal(SignalType signalType,out CoreType coreUnit,out CoreType fieldUnit)
        {
            coreUnit = signalAssetLib[signalType].CoreUnitAsset.UnitType;
            fieldUnit = signalAssetLib[signalType].FieldUnitAsset.UnitType;
        }

        #region Getter

        private SignalAssetBase GetSignalAssetByUnitType(CoreType unitType)
        {
            try
            {
                var v1 = signalAssetLib.Values.First(v => v.CoreUnitAsset.UnitType == unitType);
                if (v1 != null) return v1;
            }
            catch (InvalidOperationException)
            {
                try
                {

                    var v2 = signalAssetLib.Values.First(v => v.FieldUnitAsset.UnitType == unitType);
                    if (v2 != null) return v2;
                }
                catch (InvalidOperationException)
                {
                    throw new ArgumentException();
                }
            }
            throw new ArgumentException();
        }

        public UnitAsset GetUnitAssetByUnitType(CoreType unitType)
        {
            var signalBase = GetSignalAssetByUnitType(unitType);
            return signalBase.CoreUnitAsset.UnitType == unitType ? signalBase.CoreUnitAsset : signalBase.FieldUnitAsset;
        }

        public float PriceFromUnit(CoreType unitType)
        {
            return GetUnitAssetByUnitType(unitType).UnitPrice;
        }

        public SignalType SignalTypeFromUnit(CoreType unitType)
        {
            return GetSignalAssetByUnitType(unitType).Type;
        }

        public Material GetMatByUnitType(CoreType unitType)
        {
            return GetUnitAssetByUnitType(unitType).UnitMat;
        }

        #endregion

        void Start()
        {
            //���ʱ��������Awake��Unit��Awake֮������⣻
            //�С����ʱ�򡱰�æ�����أ�unit��Ȼ��������ʼ������֮��ű�������
            signalAssetLib = new Dictionary<SignalType, SignalAssetBase>();
            foreach (var signalBase in GetComponentsInChildren<SignalAssetBase>())
            {
                signalAssetLib.Add(signalBase.Type, signalBase);
            }
            //�������ﻹҪ��һ����֤������
        }

        #region Delegate

        //������ͨ���������Ե���SignalAsset����ġ�׼��̬��������
        //���׼��̬�������ͼ��ˣ�����ֻ���ֶ��ӡ�
        public bool ShowSignal(SignalType type, RotationDirection dir, Unit unit, Unit otherUnit)
        {
            return signalAssetLib[type].ShowSignal(dir, unit, otherUnit);
        }

        public int SignalVal(SignalType type, RotationDirection dir, Unit unit, Unit otherUnit)
        {
            return signalAssetLib[type].SignalVal(dir, unit, otherUnit);
        }

        public Type SignalUnitCore(SignalType type)
        {
            return signalAssetLib[type].UnitSignalCoreType;
        }

        public float CalAllScoreBySignal(SignalType type, Board gameBoard, out int hardwareCount)
        {
            return signalAssetLib[type].CalAllScore(gameBoard, out hardwareCount);
        }

        public float CalAllScoreBySignal(SignalType type, Board gameBoard)
        {
            return signalAssetLib[type].CalAllScore(gameBoard);
        }

        public float CalAllScoreAllSignal(Board gameBoard)
        {
            return signalAssetLib.Values.Sum(v => v.CalAllScore(gameBoard));
        }

        #endregion
    }
}