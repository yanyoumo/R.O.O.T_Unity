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

        private GameObject[] _items;
        private float[] _prices;

        /*private GameObject _itemA;
        private GameObject _itemB;
        private GameObject _itemC;
        private GameObject _itemD;*/

        /*private float _priceA;
        private float _priceB;
        private float _priceC;
        private float _priceD;*/

        /*public Text Item1PriceText;
        public Text Item2PriceText;
        public Text Item3PriceText;
        public Text Item4PriceText;*/

        public Text[] ItemPriceTexts;

        private readonly Vector3 _posA=new Vector3(2.0f,0.0f,-2.3f);
        private float _posDisplace = 5.5f/3.0f;

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
                    break;
                }
            } while (!CheckConformKeySide(core, res));
            
            return res;
        }

        public void InitShop()
        {
            _items = new GameObject[4];
            _prices = new float[4];
            ItemPriceTexts=new Text[4];
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
                            break;
                        }
                    }
                }
            }

            for (int i = 0; i < _items.Length; i++)
            {
                if (!_items[i])
                {
                    CoreType core = GenerateRandomCore();
                    _items[i] = InitUnitShop(core, GenerateRandomSideArray(core), out _prices[i]);
                }
                //_prices[i] *= _priceCof[i];
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
                _items[i] = InitUnitShop(core, GenerateRandomSideArray(core), out _prices[i]);
            }
        }

        public void ShopUpdate()
        {
            ShopUpdateStack();//还需要一个价格的衰减//这个在视觉上必须是顺序的。

            //应该弄个动画，但是这个对整个项目的视觉效果都有影响，提上日程，但是现在不做。（开个分支可以
            for (int i = 0; i < _items.Length; i++)
            {
                _items[i].gameObject.transform.position = _posA + new Vector3(_posDisplace * i, 0, 0);
                ItemPriceTexts[i].text = Utils.PaddingFloat3Digit(_prices[i] * _priceCof[i]);
            }
        }

        public bool Buy(int idx)
        {
            if (_items[idx])
            {
                if (CurrentGameStateMgr.SpendCurrency(_prices[idx]*_priceCof[idx]))
                {
                    GameBoard.DeliverUnitRandomPlace(_items[idx]);
                    _items[idx] = null;
                    return true;
                }
            }
            return false;
        }
    }
}
