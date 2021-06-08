using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Rewired.Dev;
using ROOT.Consts;
using Sirenix.OdinInspector;
using UnityEngine;
// ReSharper disable InconsistentNaming

namespace ROOT.SetupAsset
{
    [Serializable]
    [CreateAssetMenu(fileName = "NewActionAsset", menuName = "ActionAsset/New ActionAsset")]
    public class LevelActionAsset : SerializedScriptableObject
    {
        [Header("Basic Data"),PropertyOrder(-99)] 
        //[InfoBox("这里应该用反射流程加一个Title的筛选",InfoMessageType.Warning)]
        //[ValueDropdown("GetValidTerms")]//TODO 这个玩意儿怎么处理测试关卡的种种。
        public string TitleTerm;

        private IEnumerable GetValidTerms()
        {
            var res = new ValueDropdownList<string>();
            foreach (var fieldInfo in typeof(ValidLevelNameTerm).GetFields())
            {
                var actionAttribute = Attribute.GetCustomAttribute(fieldInfo, typeof(ActionIdFieldInfoAttribute));
                if (actionAttribute is ActionIdFieldInfoAttribute)
                {
                    res.Add(fieldInfo.Name, (string) fieldInfo.GetValue(null));
                }
            }
            return res;
        }
        [AssetSelector(Filter = "t:Sprite", Paths = "Assets/Resources/UIThumbnail/TutorialThumbnail"),PropertyOrder(-99)]
        public Sprite Thumbnail;

        [Required] [AssetSelector(Filter = "t:Prefab", Paths = "Assets/Resources/LevelLogicPrefab"),PropertyOrder(-98)]
        public GameObject LevelLogic;

        [PropertyOrder(-97)] 
        public int AcessID = -1;
        
        [Range(0, 1000),PropertyOrder(-96)] 
        public int InitialCurrency = 36;

        [HorizontalGroup("Split")] [VerticalGroup("Split/Left")] [LabelText("Shop has cost")][PropertyOrder(-95)]
        public bool ShopCost = true;

        [VerticalGroup("Split/Right")] [LabelText("Unit could cost")][PropertyOrder(-94)]
        public bool UnitCost = true;

        [Space][PropertyOrder(-2)] public LevelType levelType;

        [PropertyOrder(-1)] 
        [LabelText("Level For Test")]
        public bool IsTestingLevel;

        [PropertyOrder(0)]public LevelType DisplayedlevelType;

        [Header("Detail")][PropertyOrder(1)]
        public AdditionalGameSetup AdditionalGameSetup;

        [PropertyOrder(2)]
        public UnitGist[] InitalBoard;

        [ShowIf("levelType", LevelType.Career)] [OnValueChanged("HasBossChanged")][PropertyOrder(3)]
        public bool HasBossRound;

        [ShowIf("levelType", LevelType.Career)] [HideIf("HasBossRound")][PropertyOrder(4)]
        public bool Endless;

        [ShowIf("levelType", LevelType.Career)] 
        [ShowIf("HasBossRound")] 
        [OnValueChanged("BossTypeChanged")]
        [PropertyOrder(5)]
        public BossStageType BossStage;

        [PropertyOrder(6)]
        public List<RoundData> RoundLib;

        [ShowInInspector] [ShowIf("HasBossRound")][PropertyOrder(7)]
        public BossAdditionalSetupAsset BossSetup;
        
        [Header("Tutorial")]
        [ShowIf("levelType", LevelType.Tutorial)]
        [TableList(DrawScrollView = true, MinScrollViewHeight = 500, MaxScrollViewHeight = 1000)]
        [PropertyOrder(8)]
        public TutorialActionData[] Actions;

        [Space] 
        [ShowIf("levelType", LevelType.Tutorial)]
        [PropertyOrder(86)]
        [LabelText("Main Index to Insert")]
        public int mainIDXToinsert;
        
        [ShowIf("levelType", LevelType.Tutorial)]
        [PropertyOrder(87)]
        [LabelText("Sub Index to Insert")]
        public int subIDXToinsert;

        [ShowIf("levelType", LevelType.Tutorial)]
        [Button("Insert Tutorial Action")]
        [HorizontalGroup("Operation Buttons")]
        [PropertyOrder(88)]
        public void InsertTutorialActions()
        {
            ReorderTutorialActions();//得考虑到在插入前本身顺序不齐的情况。
            if (Actions.Count(a => a.ActionIdx == mainIDXToinsert && a.ActionSubIdx == subIDXToinsert) > 0)
            {
                //Has pre-existing Action, push back then insert.
                for (var i = 0; i < Actions.Length; i++)
                {
                    if (Actions[i].ActionIdx >= mainIDXToinsert)
                    {
                        Actions[i].ActionIdx++;
                    }
                }
            }

            Actions = Actions.Append(new TutorialActionData
            {
                ActionIdx = mainIDXToinsert,
                ActionSubIdx = subIDXToinsert
            }).ToArray();
            
            ReorderTutorialActions();
        }

        [ShowIf("levelType", LevelType.Tutorial)]
        [Button("Shrink Tutorial Actions")]
        [HorizontalGroup("Operation Buttons")]
        [PropertyOrder(89)]
        public void ShrinkTutorialActions()
        {
            ReorderTutorialActions();
            var oldInx = Actions.Select(a => a.ActionIdx).OrderBy(i => i).ToArray();
            var currentMissingIndCount = 0;
            for (var i = 0; i < Actions.Length; i++)
            {
                var oldIndCVal = i - 1 >= 0 ? oldInx[i - 1] : oldInx[i];
                var oldIndNVal = oldInx[i];
                currentMissingIndCount += Mathf.Max((oldIndNVal - oldIndCVal) - 1, 0);
                Actions[i].ActionIdx -= currentMissingIndCount;
            }
        }

        private class TutorialActionComparer : IComparer<TutorialActionData>
        {
            public int Compare(TutorialActionData x, TutorialActionData y) => (x.ActionIdx != y.ActionIdx) ? (x.ActionIdx - y.ActionIdx) : (x.ActionSubIdx - y.ActionSubIdx);
        }

        private class TutorialTextActionComparer : IComparer<Tuple<int, int, string>>
        {
            public int Compare(Tuple<int, int, string> x, Tuple<int, int, string> y)
            {
                if (x==null||y==null) return 0;
                return (x.Item1 != y.Item1) ? (x.Item1 - y.Item1) : (x.Item2 - y.Item2);
            }
        }
        
        [ShowIf("levelType", LevelType.Tutorial)]
        [Button("Reorder Tutorial Actions")]
        [HorizontalGroup("Operation Buttons")]
        [PropertyOrder(90)]
        public void ReorderTutorialActions() => Actions = Actions.OrderBy(a => a, new TutorialActionComparer()).ToArray();

        [Space]
        [ShowIf("levelType", LevelType.Tutorial), PropertyOrder(91)]
        public string CSVFileName;

        [ShowIf("levelType", LevelType.Tutorial), Button("Export Text To CSV")]
        [PropertyOrder(92)]
        [HorizontalGroup("CSV Operation Buttons")]
        public void ExportTextToCSV()
        {
            var sw = new StreamWriter(@"Assets/" + CSVFileName + ".csv", false, Encoding.UTF8);
            var title = string.Join(",", "MainIdx", "SubIdx", "Content");
            sw.WriteLine(title);
            var contents = Actions.Where(a => a.ActionType == TutorialActionType.Text).Select(a => new Tuple<int, int, string>(a.ActionIdx, a.ActionSubIdx, a.Text));
            contents = contents.OrderBy(t => t, new TutorialTextActionComparer()).ToArray();
            foreach (var content in contents)
            {
                var contentStr = string.Join(",", content.Item1, content.Item2, "\"" + content.Item3 + "\"");
                sw.WriteLine(contentStr);
            }

            sw.Close();
        }

        [ShowIf("levelType", LevelType.Tutorial), Button("Try Replace Text From CSV")]
        [PropertyOrder(93)]
        [HorizontalGroup("CSV Operation Buttons")]
        public void TryReplaceTextFromCSV()
        {
            var sw = new StreamReader(@"Assets/" + CSVFileName + ".csv", Encoding.UTF8);
            sw.ReadLine();//SkipTitleLine
            var contentBuffer = new List<Tuple<int, int, string>>();
            do
            {
                var contents = sw.ReadLine().Split(',');
                var mainIdx = -1;
                var subIdx = -1;
                try
                {
                    mainIdx = int.Parse(contents[0]);
                    subIdx = int.Parse(contents[1]);
                }
                catch (Exception)
                {
                    Debug.LogWarning("Error when parsing index, Replace abort, please check!!");
                    sw.Close();
                    return;
                }
                contentBuffer.Add(new Tuple<int, int, string>(mainIdx, subIdx, contents[2]));
            } while (!sw.EndOfStream);
            sw.Close();

            var typeValid=contentBuffer.All(t => Actions.Where(a => a.ActionIdx == t.Item1 && a.ActionSubIdx == t.Item2).All(a => a.ActionType == TutorialActionType.Text));
            if (!typeValid)
            {
                Debug.LogWarning("Actions type is not text, Replace abort, please check!!");
                return;
            }
            for (var i = 0; i < Actions.Length; i++)
            {
                var li = contentBuffer.Where(t => t.Item1 == Actions[i].ActionIdx && t.Item2 == Actions[i].ActionSubIdx).ToArray();
                if (li.Length>0)
                {
                    var rawStr = li[0].Item3;
                    Actions[i].Text =rawStr.Substring(1, rawStr.Length - 2);
                }
            }
        }

        //public LevelQuadDataPack LevelQuadDataPack => new LevelQuadDataPack(AcessID, TitleTerm, "Play", Thumbnail);

        public BossStageType? GetBossStage => HasBossRound ? BossStage : (BossStageType?) null;

        public BossStageType BossStageVal
        {
            get
            {
                if (GetBossStage.HasValue) return GetBossStage.Value;
                throw new ArgumentException("this lib has no bossStage.");
            }
        }

        private void HasBossChanged()
        {
            if (HasBossRound)
            {
                Endless = false;
            }
        }

        private void BossTypeChanged()
        {
            try
            {
                BossSetup.BossStageTypeVal = BossStageVal;
            }
            catch (ArgumentException)
            {
                Debug.Log("Has no boss");
            }
        }

        public (int, bool, bool) GameStartingData => (InitialCurrency, ShopCost, UnitCost);

        [PropertyOrder(200)] 
        [Header("Unlocking Level")]
        public LevelActionAsset[] UnlockingLevel;
        
        [PropertyOrder(201)]
        public LevelActionAsset[] UnlockingLevel_Upper;

        [Obsolete("Why?")] public Vector2Int[] StationaryRateList => null;

        [Obsolete("Why?")] public List<SignalType> ShopExcludedType => null;

        [Obsolete("Why?")] [HideInInspector] public bool ExcludedShop;
    }
}