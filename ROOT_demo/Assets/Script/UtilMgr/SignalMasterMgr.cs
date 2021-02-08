using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace ROOT.Signal
{
    //using SignalData = Tuple<int, int, int, Unit>;

    [Serializable]
    public sealed class SignalData
    {
        [ReadOnly] public int HardwareDepth = 0;
        [ReadOnly] public int FlatSignalDepth = 0;
        [ReadOnly] public int SignalDepth = 0;
        [ReadOnly] public Unit UpstreamUnit = null;

        public SignalData()
        {
            HardwareDepth = 0;
            FlatSignalDepth = 0;
            SignalDepth = 0;
            UpstreamUnit = null;
        }

        public SignalData(int _hardwareDepth, int _flatSignalDepth, int _signalDepth, Unit unit)
        {
            HardwareDepth = _hardwareDepth;
            FlatSignalDepth = _flatSignalDepth;
            SignalDepth = _signalDepth;
            UpstreamUnit = unit;
        }
    }

    public sealed class SignalPath: IList<Unit>
    {
        private List<Unit> _core;

        public SignalPath()
        {
            _core = new List<Unit>();
        }

        public SignalPath(IList<Unit> core)
        {
            _core = core.ToList();
        }

        public SignalPath(SignalPath other):this(other._core) { }
        
        ~SignalPath()
        {
            _core = null;
        }

        public bool IsValidPath=>_core.Count > 1;

        public void TruncatePath(SignalType signalType)
        {
            var truncateAmount = 0;

            for (var i = _core.Count - 1; i >= 0; i--)
            {
                var unit = _core[i];
                if (unit.UnitSignal != signalType)
                {
                    truncateAmount++;
                }
                else
                {
                    break;
                }
            }

            if (truncateAmount == 0) return;
            for (var i = 0; i < truncateAmount; i++)
            {
                var rIndex = _core.Count - 1 - i;
                if (rIndex >= 0 && rIndex <= _core.Count - 1)
                {
                    _core.RemoveAt(rIndex);
                }
                else
                {
                    //Truncate to Null;
                    _core = new List<Unit>();
                    return;
                }
            }
        }

        
        public override string ToString()
        {
            List<Vector2Int> listV2 = this;
            var res = "";
            for (var index = 0; index < listV2.Count; index++)
            {
                var vector2Int = listV2[index];
                res += "[" + vector2Int.x + "," + vector2Int.y + "]";
                if (index!=listV2.Count-1)
                {
                    res += ",";
                }
            }
            return res;
        }

        public static implicit operator List<Vector2Int>(SignalPath path) => path._core.Select(unit => unit.CurrentBoardPosition).ToList();

        public static explicit operator SignalPath(List<Unit> path) => new SignalPath(path);

        
        public override bool Equals(object obj)
        {
            if (!(obj is SignalPath)) return false;
            return GetHashCode() == ((SignalPath) obj).GetHashCode();
        }

        public override int GetHashCode()
        {
            if (_core.Count==0) return 0;
            return _core.Aggregate(0, (current, unit) => current ^ unit.GetHashCode());
        }

        public IEnumerator<Unit> GetEnumerator()
        {
            return _core.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(Unit item)
        {
            _core.Add(item);
        }

        public void Clear()
        {
            _core.Clear();
        }

        public bool Contains(Unit item)
        {
            return _core.Contains(item);
        }

        public void CopyTo(Unit[] array, int arrayIndex)
        {
            _core.CopyTo(array, arrayIndex);
        }

        public bool Remove(Unit item)
        {
            return _core.Remove(item);
        }

        public int Count => _core.Count;
        public bool IsReadOnly => false;
        public int IndexOf(Unit item)
        {
            return _core.IndexOf(item);
        }

        public void Insert(int index, Unit item)
        {
            _core.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _core.RemoveAt(index);
        }

        public Unit this[int index]
        {
            get => _core[index];
            set => _core[index] = value;
        }
    }
    
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
        public Dictionary<SignalType, List<List<Vector2Int>>> PathsVec2
        {
            get
            {
                if (Paths == null || Paths.Count == 0)
                {
                    return new Dictionary<SignalType, List<List<Vector2Int>>>();
                }

                var res = new Dictionary<SignalType, List<List<Vector2Int>>>();
                foreach (var keyValuePair in Paths)
                {
                    res[keyValuePair.Key] =
                        keyValuePair.Value.Select(signalPath => (List<Vector2Int>) signalPath).ToList();
                }

                return res;
            }
        }

        public Dictionary<SignalType, List<SignalPath>> Paths => _paths == null ? new Dictionary<SignalType, List<SignalPath>>() : _paths;
        private Dictionary<SignalType, List<SignalPath>> _paths;


        private void RefreshBoardSelectedSignalStrength(Board board, SignalType[] selectedTypes)
        {
            _paths = new Dictionary<SignalType, List<SignalPath>>();
            board.Units.Select(u => u.SignalCore).ForEach(s => s.ResetSignalStrengthComplex());
            foreach (var signalAssetBase in signalAssetLib.Where(v => selectedTypes.Contains(v.Key)).Select(v => v.Value))
            {
                signalAssetBase.RefreshBoardSignalStrength(board);
                Paths[signalAssetBase.SignalType] = signalAssetBase.FindAllPathSingleLayer(board).ToList();
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