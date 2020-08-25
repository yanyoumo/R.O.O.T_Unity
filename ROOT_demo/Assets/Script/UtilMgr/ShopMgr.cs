using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = System.Object;
using Random = UnityEngine.Random;

namespace ROOT
{
    public sealed partial class ShopMgr:MonoBehaviour
    {
        public GameObject UnitTemplate;

        public Board GameBoard;
        public GameStateMgr CurrentGameStateMgr;

        private GameObject[] _items;
        private float[] _prices;

        private Vector3[] currentPosS;
        private Vector3[] nextPosS;

        public TextMeshPro[] ItemPriceTexts_TMP;

        private readonly Vector3 _posA = new Vector3(2.273f, 0.718f, -2.801f);
        private float _posDisplace = (6.78f - 2.273f) / 3.0f;

        private Dictionary<CoreType, float> _priceByCore;
        private Dictionary<SideType, float> _priceBySide;

        private Dictionary<SideType, float> _defaultSideWeight;
        private Dictionary<SideType, float> _processorSideWeight;
        private Dictionary<SideType, float> _serverSideWeight;
        private Dictionary<SideType, float> _hddSideWeight;
        private Dictionary<SideType, float> _netCableSideWeight;

        private Dictionary<CoreType, float> _defaultCoreWeight;
        private Dictionary<CoreType, float> _noServerCoreWeight;
        private Dictionary<CoreType, float> _noProcessorCoreWeight;
        private Dictionary<CoreType, float> _nandServerProcessorCoreWeight;
        private Dictionary<CoreType, Dictionary<SideType, float>> _sideWeightLib;

        //KeySide minCount
        private Dictionary<CoreType, Tuple<SideType, int>> _keySideLib;

        private readonly float[] _priceCof = { 0.8f, 1.0f, 1.2f, 1.5f };

        public List<CoreType> excludedTypes=new List<CoreType>();

        private void NormalizeDicVal(ref Dictionary<CoreType, float> lib)
        {
            float totalWeight = 0;
            foreach (var weight in lib.Values)
            {
                totalWeight += weight;
            }
            if (!(Mathf.Abs(totalWeight - 1) < 1e-3))
            {
                var keys = lib.Keys.ToArray().Clone() as CoreType[];
                foreach (var coreType in keys)
                {
                    lib[coreType] /= totalWeight;
                }
            }
        }

        private CoreType GenerateRandomCore()
        {
            //这里不会生成HQ核心。
            Dictionary<CoreType, float> lib= _defaultCoreWeight;
            if (GameBoard.GetCountByType(CoreType.Server)<1)
            {
                lib = _noServerCoreWeight;
            }
            if (GameBoard.GetCountByType(CoreType.Processor) < 1)
            {
                lib = _noProcessorCoreWeight;
            }
            if ((GameBoard.GetCountByType(CoreType.Server) < 1)&& (GameBoard.GetCountByType(CoreType.Processor) < 1))
            {
                lib = _nandServerProcessorCoreWeight;
            }
            if (excludedTypes.Count>0)
            {
                foreach (var excludedType in excludedTypes)
                {
                    if (lib.TryGetValue(excludedType,out float value))
                    {
                        lib.Remove(excludedType);
                    }
                }
            }
            NormalizeDicVal(ref lib);
            return Utils.GenerateWeightedRandom(lib);
        }

        private bool CheckConformKeySide(CoreType core, SideType[] sides)
        {

            if (!_keySideLib.TryGetValue(core, out var data))
            {
                //no constrain always ok
                return true;
            }

            return Utils.GetSideCount(data.Item1, sides) >= data.Item2;
        }

        private SideType[] GenerateRandomSideArray(CoreType core=CoreType.PCB)
        {
            if (!_sideWeightLib.TryGetValue(core, out var lib))
            {
                Debug.Assert(false);
                lib = _defaultSideWeight;
            }
            SideType[] res=new SideType[4];
            const int cutoff = 1000;
            int cutoffCounter = 0;
            do
            {
                res[0] = Utils.GenerateWeightedRandom(lib);
                res[1] = Utils.GenerateWeightedRandom(lib);
                res[2] = Utils.GenerateWeightedRandom(lib);
                res[3] = Utils.GenerateWeightedRandom(lib);
                cutoffCounter++;
                if (cutoffCounter >= cutoff)
                {
                    Debug.Assert(false, "Can't generate good Unit");
                    break;
                }
            } while (!CheckConformKeySide(core, res));
            
            return res;
        }

        public void ShopUpdateStack()
        {
            if (_items[0])
            {
                Destroy(_items[0].gameObject);
            }
            _items[0] = null;
            for (int i = 0; i < _items.Length; i++)
            {
                if (_items[i])
                {
                    for (int j = 0; j < _items.Length; j++)
                    {
                        if (!_items[j])
                        {
                            _items[j] = _items[i];
                            _items[i] = null;
                            _prices[j] = _prices[i];
                            _prices[i] = -1;
                            currentPosS[j] = currentPosS[i];
                            break;
                        }
                    }
                }
            }
        }

        public void ShopUpdateRandom()
        {
            foreach (var item in _items)
            {
                if (item)
                {
                    Destroy(item.gameObject);
                }
            }
            _items = new GameObject[4];
            for (int i = 0; i < _items.Length; i++)
            {
                CoreType core = GenerateRandomCore();
                _items[i] = InitUnitShop(core, GenerateRandomSideArray(core), out _prices[i],i);
            }
        }

        public void ShopInit()
        {
            _items = new GameObject[4];
            _prices = new float[_items.Length];
            ItemPriceTexts_TMP = new TextMeshPro[_items.Length];
            currentPosS = new Vector3[_items.Length];
            nextPosS = new Vector3[_items.Length];
        }

        public void ShopStart()
        {
            InitPrice();
            InitSideCoreWeight();

            ShopPreAnimationUpdate();
            ShopPostAnimationUpdate();
        }

        public void ShopInitPos()
        {
            for (int i = 0; i < _items.Length; i++)
            {
                currentPosS[i] = _posA + new Vector3(_posDisplace * i, 0, 0);
                nextPosS[i] = _posA + new Vector3(_posDisplace * i, 0, 0);
            }
        }

        public void ShopPreAnimationUpdate()
        {
            ShopUpdateStack();

            for (int i = 0; i < _items.Length; i++)
            {
                nextPosS[i] = _posA + new Vector3(_posDisplace * i, 0, 0);
            }
        }

        private Vector3 lerpVec3(Vector3 vecA,Vector3 vecB,float lerp)
        {
            return new Vector3(Mathf.Lerp(vecA.x, vecB.x, lerp), Mathf.Lerp(vecA.y, vecB.y, lerp),
                Mathf.Lerp(vecA.z, vecB.z, lerp));
        }

        public void ShopUpdateAnimation(float lerp)
        {
            for (int i = 0; i < _items.Length; i++)
            {
                if (_items[i])
                {
                    _items[i].gameObject.transform.position = lerpVec3(currentPosS[i], nextPosS[i], lerp);
                }
            }
        }

        public void ShopPostAnimationUpdate()
        {
            for (int i = 0; i < _items.Length; i++)
            {
                currentPosS[i] = new Vector3(nextPosS[i].x, nextPosS[i].y, nextPosS[i].z);
            }

            for (int i = 0; i < _items.Length; i++)
            {
                if (!_items[i])
                {
                    CoreType core = GenerateRandomCore();
                    _items[i] = InitUnitShop(core, GenerateRandomSideArray(core), out _prices[i], i);
                    currentPosS[i] = _posA + new Vector3(_posDisplace * i, 0, 0);
                    nextPosS[i] = _posA + new Vector3(_posDisplace * i, 0, 0);
                    _items[i].gameObject.transform.position = currentPosS[i];
                }
                _items[i].gameObject.GetComponentInChildren<Unit>().ShopID = i;
                ItemPriceTexts_TMP[i].text = Utils.PaddingNum3Digit(_prices[i] * _priceCof[i]);
            }
        }

        public void ResetPendingBuy()
        {
            foreach (var item in _items)
            {
                item.GetComponentInChildren<Unit>().SetPendingBuying = false;
            }
        }

        public bool RequestBuy(int idx)
        {
            if (_items[idx])
            {
                float totalPrice = _prices[idx] * _priceCof[idx];
                if (CurrentGameStateMgr.GetCurrency() >= totalPrice)
                {
                    _items[idx].GetComponentInChildren<Unit>().SetPendingBuying = true;
                    return true;
                }
            }
            return false;
        }

        public bool BuyToPos(int idx, Vector2Int pos)
        {
            if (_items[idx])
            {
                float unitPrice = _prices[idx] * _priceCof[idx];

                //TODO Yuxuan 设计邮费系统框架。
                //VVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVV
                float postalPrice = 10.0f;
                float totalPrice = unitPrice + postalPrice;
                //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
                //V/^ 范围的代码输入一个unitPrice之后输出一个totalPrice。并且需要使用out输出差价。
                //在常规的游戏设计中，一般都是程序员设计一个框架，之后由策划在其他一小片简单的代码部分进行制定代码。
                //对于这个框架，需要在CoreSide.cs文件中的partial ShopMgr类中建立新的函数并且完善yml-summary。
                //并且函数里面添加placeHolder逻辑。
                //**这个TODO很简单，不用想太多，就是简单的在别的文件里面声明一个函数并且调一下，
                //**主要是要传达游戏行业中，程序员和策划进行互动的一种方式。以及partial class的一种应用。

                if (CurrentGameStateMgr.SpendShopCurrency(totalPrice))
                {
                    _items[idx].gameObject.GetComponentInChildren<Unit>().ShopID = -1;
                    //RISK00 现在购买的Unit无法移动。
                    //_items[idx].gameObject.GetComponentInChildren<Unit>().SetupStationUnit();
                    GameBoard.DeliverUnitAssignedPlace(_items[idx], pos);
                    _items[idx] = null;
                    return true;
                }
            }
            return false;
        }

        public bool BuyToRandom(int idx)
        {
            if (_items[idx])
            {
                if (CurrentGameStateMgr.SpendShopCurrency(_prices[idx]*_priceCof[idx]))
                {
                    _items[idx].gameObject.GetComponentInChildren<Unit>().ShopID = -1;
                    GameBoard.DeliverUnitRandomPlace(_items[idx]);
                    _items[idx] = null;
                    return true;
                }
            }
            return false;
        }
    }
}
