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
            //这个时候不用在意Awake和Unit的Awake之间的问题；
            //有“宏观时序”帮忙守着呢，unit必然是在它初始化场景之后才被制作。
            signalAssetLib = new Dictionary<SignalType, SignalAssetBase>();
            foreach (var signalBase in GetComponentsInChildren<SignalAssetBase>())
            {
                signalAssetLib.Add(signalBase.Type, signalBase);
            }
            //可能这里还要放一个验证函数？
        }

        #region Delegate

        //这里是通过代理，可以调用SignalAsset里面的“准静态”函数。
        //如果准静态函数类型加了，这里只能手动加。
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