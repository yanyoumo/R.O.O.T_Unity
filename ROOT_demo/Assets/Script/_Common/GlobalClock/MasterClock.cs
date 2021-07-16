using System;
using System.Linq;
using System.Reflection;
using com.ootii.Messages;
using DG.Tweening;
using ROOT.Message;
using ROOT.RTAttribute;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ROOT.Clock
{
    public class MasterClock : MonoBehaviour
    {
        private static MasterClock instance = null;

        public static MasterClock Instance
        {
            get
            {
                if (instance == null)
                {
                    throw new ApplicationException("MasterClock not exist!!");
                }

                return instance;
            }
        }

        public bool GamePausedStatus { get; private set; } = false;
        public bool HasTelemetryPauseModule => TelemetryPauseModule != null;
        public ClockTelemetryPauseModule TelemetryPauseModule => GetComponent<ClockTelemetryPauseModule>();

        private void ToggleGamePause()
        {
            GamePausedStatus = !GamePausedStatus;
            DOTween.TogglePauseAll();
            MessageDispatcher.SendMessage(new GamePauseInfo {GamePaused = GamePausedStatus});
        }

        public void Reset()
        {
            if (TelemetryPauseModule != null)
            {
                TelemetryPauseModule.ResetClockPauseModule();
            }

            InitCycler();
        }

        public int Step => ApparentStep;

        private bool? RawNeedAutoDriveStep
        {
            get
            {
                if (ApparentStep == ExpectedStep) return null;
                return ExpectedStep > ApparentStep;
            }
        }

        /// <summary>
        /// NULL: 不需要自动演进。
        /// True: 需要自动往前演进。
        /// False:需要自动逆向演进。
        /// </summary>
        public bool? NeedAutoDriveStep
        {
            get
            {
                if (TelemetryPauseModule != null)
                {
                    if (TelemetryPauseModule.TelemetryStage)
                    {
                        if (TelemetryPauseModule.TelemetryPause)
                        {
                            return null;
                        }

                        return true;
                    }
                }

                return RawNeedAutoDriveStep;
            }
        }

        public int RawStep { private set; get; }
        public int ApparentOffset { private set; get; }
        public int ExpectedStep { private set; get; }

        public int ApparentStep
        {
            set => RawStep = value - ApparentOffset;
            get => RawStep + ApparentOffset;
        }


        public void InitCycler()
        {
            Debug.Log("RawStep = 0;");
            RawStep = 0;
            ApparentOffset = 0;
            ExpectedStep = 0;
        }

        public void StepUp()
        {
            if (ExpectedStep < ApparentStep)
            {
                throw new Exception("Should not further Increase Step when ExpectedStep is Lower");
            }
            else if (ExpectedStep > ApparentStep)
            {
                ApparentStep++;
            }
            else if (ExpectedStep == ApparentStep)
            {
                ApparentStep++;
                ExpectedStep++;
            }
        }

        public void StepDown()
        {
            if (ExpectedStep > ApparentStep)
            {
                throw new Exception("Should not further Decrease Step when ExpectedStep is Higher");
            }
            else if (ExpectedStep < ApparentStep)
            {
                ApparentStep--;
            }
            else if (ExpectedStep == ApparentStep)
            {
                ApparentStep--;
                ExpectedStep--;
            }
        }

        public void ExpectedStepIncrement(int amount)
        {
            ExpectedStep += amount;
        }

        public void ExpectedStepDecrement(int amount)
        {
            ExpectedStep -= amount;
        }

        public void ResetApparentStep()
        {
            ApparentOffset = -RawStep;
            ExpectedStep = 0;
            MessageDispatcher.SendMessage(WorldEvent.ApparentStepResetedEvent);
        }

        private void Awake()
        {
            try
            {
                if (Instance != null)
                {
                    Debug.LogWarning("MasterClock exists, Init abort");
                    Destroy(gameObject);
                    return;
                }
            }
            catch (ApplicationException)
            {
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            MessageDispatcher.AddListener(WorldEvent.ControllingEvent, RespondToKeyGamePauseEvent);
            MessageDispatcher.AddListener(WorldEvent.RequestGamePauseEvent, RespondToGamePauseEvent);
        }

        private void RespondToKeyGamePauseEvent(IMessage rMessage)
        {
            if (rMessage is ActionPack actPak)
            {
                if (actPak.IsAction(RewiredConsts.Action.Button.PauseMenu))
                {
                    ToggleGamePause();
                }
            }
        }

        private void RespondToGamePauseEvent(IMessage rMessage)
        {
            ToggleGamePause();
        }

        [Button]
        private void TestRegister()
        {
            TestFunction();
        }

        [ReplaceableAction] 
        public Action TestFunction = () => Debug.Log("TestFunction_Base");
        
        internal void RegisterClockModule(ClockModuleBase module)
        {
            //Debug.Log("RegisterClockModule");
            var repActionField = typeof(ClockModuleBase).GetFields().Where(f => f.IsDefined(typeof(ReplaceableActionAttribute), false));
            //Debug.Log("repActionField.Count=" + repActionField.Count());
            foreach (var fieldInfo in repActionField)
            {
                FieldInfo target;
                try
                {
                    target = GetType().GetFields(BindingFlags.Public | BindingFlags.Instance).First(f => f.IsDefined(typeof(ReplaceableActionAttribute), false) && f.Name == fieldInfo.Name);
                }
                catch (InvalidOperationException e)
                {
                    Debug.LogWarning("No corresponding ReplaceableAction in master clock for:" + fieldInfo.Name);
                    continue;
                }
                target.SetValue(this, fieldInfo.GetValue(module));
            }
        }
    }
}