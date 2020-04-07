using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = System.Object;
using Random = UnityEngine.Random;

namespace ROOT
{
    public sealed partial class ShopMgr:MonoBehaviour
    {
        public GameObject UnitTemplate; //TODO 应该要改成AssetLoad那种

        public Board GameBoard;
        /*
         *D C
         *A B
         */
        public GameStateMgr CurrentGameStateMgr;
        //public GlobalAssetLib _globalAssetLib;

        private GameObject _itemA;
        private GameObject _itemB;
        private GameObject _itemC;
        private GameObject _itemD;

        private float _priceA;
        private float _priceB;
        private float _priceC;
        private float _priceD;

        public Text ItemAPriceText;
        public Text ItemBPriceText;
        public Text ItemCPriceText;
        public Text ItemDPriceText;

        private readonly Vector3 _posA=new Vector3(5.0f,0.0f,-2.5f);
        private float _posDisplace = 2.0f;

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

        private CoreType GenerateRandomCore()
        {
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
                }
            } while (!CheckConformKeySide(core, res));
            
            return res;
        }

        private GameObject InitUnitShop(CoreType core, SideType[] sides, out float price)
        {
            var go = Instantiate(UnitTemplate);
            go.name = "Unit_" + Hash128.Compute(Utils.LastRandom.ToString());
            var unit = go.GetComponentInChildren<Unit>();
            unit.board_position = Vector2Int.zero;
            unit.InitUnit(core, sides);
            _priceByCore.TryGetValue(core, out float corePrice);
            _priceBySide.TryGetValue(sides[0], out float sidePrice0);
            _priceBySide.TryGetValue(sides[1], out float sidePrice1);
            _priceBySide.TryGetValue(sides[2], out float sidePrice2);
            _priceBySide.TryGetValue(sides[3], out float sidePrice3);
            price = corePrice + sidePrice0 + sidePrice1 + sidePrice2 + sidePrice3;
            return go;
        }

        public void ShopUpdate()
        {
            if (_itemA)
            {
                Destroy(_itemA.gameObject);
                _itemA = null;
            }
            if (_itemB)
            {
                Destroy(_itemB.gameObject);
                _itemB = null;
            }
            if (_itemC)
            {
                Destroy(_itemC.gameObject);
                _itemC = null;
            }
            if (_itemD)
            {
                Destroy(_itemD.gameObject);
                _itemD = null;
            }

            CoreType coreA = GenerateRandomCore();
            _itemA = InitUnitShop(coreA, GenerateRandomSideArray(coreA), out _priceA);
            CoreType coreB = GenerateRandomCore();
            _itemB = InitUnitShop(coreB, GenerateRandomSideArray(coreB), out _priceB);
            CoreType coreC = GenerateRandomCore();
            _itemC = InitUnitShop(coreC, GenerateRandomSideArray(coreC), out _priceC);
            CoreType coreD = GenerateRandomCore();
            _itemD = InitUnitShop(coreD, GenerateRandomSideArray(coreD), out _priceD);

            _itemA.gameObject.transform.position = _posA;
            _itemB.gameObject.transform.position = _posA + new Vector3(_posDisplace, 0, 0);
            _itemC.gameObject.transform.position = _posA + new Vector3(_posDisplace, 0, _posDisplace);
            _itemD.gameObject.transform.position = _posA + new Vector3(0, 0, _posDisplace);

            ItemAPriceText.text = Mathf.FloorToInt(_priceA).ToString();
            ItemBPriceText.text = Mathf.FloorToInt(_priceB).ToString();
            ItemCPriceText.text = Mathf.FloorToInt(_priceC).ToString();
            ItemDPriceText.text = Mathf.FloorToInt(_priceD).ToString();
        }

        public bool BuyA()
        {
            //Debug.Log("Buy A");
            if (_itemA)
            {
                if (CurrentGameStateMgr.SpendCurrency(_priceA))
                {
                    GameBoard.DeliverUnitRandomPlace(_itemA);
                    _itemA = null;
                    return true;
                }
            }

            return false;
        }

        public bool BuyB()
        {
            //Debug.Log("Buy B");
            if (_itemB)
            {
                if (CurrentGameStateMgr.SpendCurrency(_priceB))
                {
                    GameBoard.DeliverUnitRandomPlace(_itemB);
                    _itemB = null;
                    return true;
                }
            }

            return false;
        }

        public bool BuyC()
        {
            //Debug.Log("Buy C");
            if (_itemC)
            {
                if (CurrentGameStateMgr.SpendCurrency(_priceC))
                {
                    GameBoard.DeliverUnitRandomPlace(_itemC);
                    _itemC = null;
                    return true;
                }
            }

            return false;
        }

        public bool BuyD()
        {
            //Debug.Log("Buy D");
            if (_itemD)
            {
                if (CurrentGameStateMgr.SpendCurrency(_priceD))
                {
                    GameBoard.DeliverUnitRandomPlace(_itemD);
                    _itemD = null;
                    return true;
                }
            }

            return false;
        }
    }
}
