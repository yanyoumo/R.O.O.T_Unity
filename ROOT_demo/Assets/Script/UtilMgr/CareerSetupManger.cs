using System;
using System.Collections;
using Doozy.Engine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Doozy.Engine.Progress;
using I2.Loc;
using ROOT.Consts;
using ROOT.SetupAsset;
using ROOT.Signal;
using TMPro;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace ROOT.UI
{
    public class CareerSetupManger : MonoBehaviour
    {
        public TextMeshProUGUI LevelName;
        public TextMeshProUGUI MainPassage;
        public TextMeshProUGUI SubPassage;
        public Image LevelIcon;
        public LevelFeatureIconArray_UI FeatureIconArrayUI;

        public RectTransform SignalSelectionBlocker;
        //public static int levelId;
        public static LevelActionAsset currentUsingAsset;

        public Progressor LoadingProgressor;
        public GameObject LoadingLabel;
        public UIView SignalSelectionPanel;
        public GameObject SignalToggleTemplate;
        public TextMeshProUGUI SignalSelectionText;
        public TextMeshProUGUI SignalSelectionHint;
        private AdditionalGameSetup _additionalGameSetup = new AdditionalGameSetup();

        public Tooltip_UI TooltipUI;

        //private LevelActionAsset actionAsset => LevelLib.Instance.ActionAsset(levelId);
        private LevelActionAsset actionAsset => currentUsingAsset;
        private bool LevelIsTutorial => (actionAsset.levelType == LevelType.Tutorial);//用这个方式判断这个关卡是不是教程.

        private SignalType[] SelectingSignals => toggles?.Where(v => v.Value.CoreToggle.isOn).Select(v1 => v1.Key).ToArray();
        private int SelectingSignalCount => SelectingSignals?.Length ?? 0;
        
        public void Back()
        {
            SceneManager.LoadScene(StaticName.SCENE_ID_BST_CAREER, LoadSceneMode.Additive);
            SceneManager.UnloadSceneAsync(StaticName.SCENE_ID_CAREERSETUP);
        }

        public void Continue()
        {
            if (SelectingSignalCount == 2 || LevelIsTutorial)
            {
                if (!LevelIsTutorial)
                {
                    //如果是Tutorial那么就无视玩家选择、只使用内部数据。
                    _additionalGameSetup.PlayingSignalTypeA = SelectingSignals[0];
                    _additionalGameSetup.PlayingSignalTypeB = SelectingSignals[1];
                    _additionalGameSetup.OrderingSignal();
                    actionAsset.AdditionalGameSetup = _additionalGameSetup;
                }
                
                loadingProgressorCallBack(0.1f);
                LevelMasterManager.Instance.LoadLevelThenPlay(actionAsset, null, loadingProgressorCallBack);
                //这个卸载函数现在放到那个回调函数里面去了。-youmo
            }
            else
            {
                RootDebug.Log("additionalGameSetup is not properly setup, player hasn't selected two cores", NameID.SuYuxuan_Log);
                //不要用popUp了，直接弄个文字抖一抖
                SignalSelectionHint.enabled = true;
                SignalSelectionHint.transform.DOShakePosition(0.5f, 7.5f);
            }
        }

        private IEnumerator Pendingkill()
        {
            yield return new WaitForSeconds(Mathf.Lerp(0.025f, 0.15f, Random.value));
            SceneManager.UnloadSceneAsync(StaticName.SCENE_ID_CAREERSETUP);
        }

        private bool loadingProgressorCallBack(float val, bool completed = false)
        {
            LoadingProgressor.SetProgress(val);
            if (!completed) return true;
            StartCoroutine(Pendingkill());
            return true;
        }
        
        private Dictionary<SignalType, UnitSelectionToggle_Neo> toggles;

        private void CreateSignalSelectionPanel(bool devMode)
        {
            var scanUnlocked = PlayerPrefs.GetInt(StaticPlayerPrefName.SCAN_UNLOCKED, 0) > 0;

            SignalSelectionBlocker.gameObject.SetActive(LevelIsTutorial);

            var signalMaster = SignalMasterMgr.Instance;
            toggles = new Dictionary<SignalType, UnitSelectionToggle_Neo>();
            for (var i = 0; i < signalMaster.SignalLib.Length; i++)
            {
                if (!devMode && !scanUnlocked && signalMaster.SignalLib[i] == SignalType.Scan)
                {
                    continue;
                }
                var toggle = Instantiate(SignalToggleTemplate, SignalSelectionPanel.transform);
                var toggleCore = toggle.GetComponentInChildren<UnitSelectionToggle_Neo>();
                toggleCore.LabelTextTerm = signalMaster.GetSignalNameTerm(signalMaster.SignalLib[i]);
                toggleCore.SignalIcon.sprite = signalMaster.GetSignalIcon(signalMaster.SignalLib[i]);
                toggleCore.SignalInfo = signalMaster.GetSignalInfo(signalMaster.SignalLib[i]);
                toggles.Add(signalMaster.SignalLib[i], toggleCore);
                toggleCore.CoreToggle.isOn = (i < 2);
                if (!LevelIsTutorial)
                {
                    toggleCore.TooltipUI = TooltipUI;
                }
                else
                {
                    toggleCore.CoreToggle.isOn = false;
                    toggleCore.CoreToggle.interactable = false;
                    toggleCore.ToggleText.text = "?????";
                }
            }
        }
        
        void Awake()
        {
            LevelName.text = LocalizationManager.GetTranslation(currentUsingAsset.TitleTerm);
            LevelIcon.sprite = currentUsingAsset.Thumbnail;
            FeatureIconArrayUI.SetLevelFeature(currentUsingAsset.LevelFeature);
            MainPassage.text = currentUsingAsset.StoryContent;
            SubPassage.text = currentUsingAsset.LevelInfo;
            TooltipUI.DeactivateTooltip();
            
            var devMode = PlayerPrefs.GetInt(StaticPlayerPrefName.DEV_MODE, 0) > 0;
            CreateSignalSelectionPanel(devMode);
        }
    }
}
