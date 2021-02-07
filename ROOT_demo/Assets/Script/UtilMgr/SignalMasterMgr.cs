using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace ROOT.Signal
{
    using SignalDataPack = Tuple<int, int, int, Unit>;
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

        #region Getter

        /*private SignalAssetBase GetSignalAssetByUnitType(CoreType unitType)
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
        }*/

        public UnitAsset GetUnitAssetByUnitType(SignalType signalType, HardwareType genre)
        {
            var asset = signalAssetLib[signalType];
            switch (genre)
            {
                case HardwareType.Core:
                    return asset.CoreUnitAsset;
                case HardwareType.Field:
                    return asset.FieldUnitAsset;
                default:
                    throw new ArgumentOutOfRangeException(nameof(genre), genre, null);
            }
        }

        public float PriceFromUnit(SignalType signalType, HardwareType genre)
        {
            return GetUnitAssetByUnitType(signalType, genre).UnitPrice;
        }

        /*public SignalType SignalTypeFromUnit(CoreType unitType)
        {
            return GetSignalAssetByUnitType(unitType).Type;
        }*/

        public Material GetMatByUnitType(SignalType signalType, HardwareType genre)
        {
            return GetUnitAssetByUnitType(signalType, genre).UnitMat;
        }

        #endregion

        void Start()
        {
            signalAssetLib = new Dictionary<SignalType, SignalAssetBase>();
            foreach (var signalBase in GetComponentsInChildren<SignalAssetBase>())
            {
                signalAssetLib.Add(signalBase.SignalType, signalBase);
            }
        }

        //这里需要找到一个方法，检测单元的拓扑修改了，发现一次改一次。
        public void RefreshBoardAllSignalStrength(Board board)
        {
            RefreshBoardSelectedSignalStrength(board, SignalLib);
        }

        [ReadOnly]
        [ShowInInspector]
        public Dictionary<SignalType, List<List<Vector2Int>>> Paths;
        
        public List<Unit> tempScanPath;
        private void RefreshBoardSelectedSignalStrength(Board board, SignalType[] selectedTypes)
        {
            Paths = new Dictionary<SignalType, List<List<Vector2Int>>>();
            board.Units.Select(u => u.SignalCore).ForEach(s => s.ResetSignalStrengthComplex());
            foreach (var signalAssetBase in signalAssetLib.Where(v => selectedTypes.Contains(v.Key))
                .Select(v => v.Value))
            {
                signalAssetBase.RefreshBoardSignalStrength(board);
                if (signalAssetBase.SignalType == SignalType.Scan)
                {
                    var asset = signalAssetBase as ScanSignalAsset;
                    tempScanPath = asset.CalAllScore(board);
                    //clear path
                    foreach (var unit in board.Units)
                        unit.SignalCore.SignalDataPackList[SignalType.Scan] = new SignalDataPack(
                            unit.SignalCore.SignalDataPackList[SignalType.Scan].Item1,
                            unit.SignalCore.SignalDataPackList[SignalType.Scan].Item2,
                            unit.SignalCore.SignalDataPackList[SignalType.Scan].Item3,
                            null);
                    for (var i = tempScanPath.Count - 1; i >= 1; --i)
                        tempScanPath[i].SignalCore.SignalDataPackList[SignalType.Scan] = new SignalDataPack(
                            tempScanPath[i].SignalCore.SignalDataPackList[SignalType.Scan].Item1,
                            tempScanPath[i].SignalCore.SignalDataPackList[SignalType.Scan].Item2,
                            tempScanPath[i].SignalCore.SignalDataPackList[SignalType.Scan].Item3,
                            tempScanPath[i - 1]);
                }

                Paths[signalAssetBase.SignalType] = signalAssetBase.FindAllPathSingleLayer(board);
            }
        }

        #region Delegate
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