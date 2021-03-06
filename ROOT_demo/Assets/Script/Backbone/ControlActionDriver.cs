using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using com.ootii.Messages;
using ROOT.Message;
using UnityEngine;
using static RewiredConsts.Action.Button;
using static RewiredConsts.Action.Cursor;
using static RewiredConsts.Action.Composite;
using static RewiredConsts.Action.Passthough;
using static ROOT.WorldEvent;

namespace ROOT
{
    using RespToCtrlEvent = Func<ActionPack, bool>;//bool? 的代表是，true需要加入队伍；false不需要加入队伍；

    public abstract class ControlActionDriver
    {
        private readonly FSMLevelLogic _ownerLogic;
        private RootFSM _mainFsm;
        private readonly Queue<ControllingPack> _ctrlPackQueue;
        private readonly Queue<BreakingCommand> _breakingCMDQueue;

        private const int InputAntiSpamTime = 6; //in ms.
        private float _inputAntiSpamTimer = .0f; //s.
        private bool _inputAntiSpam;

        private IEnumerator AntiSpamCoolDown()
        {
            yield return 0;
            do
            {
                _inputAntiSpamTimer += Time.deltaTime;
                yield return 0;
            } while (_inputAntiSpamTimer < InputAntiSpamTime / 1000.0f);

            _inputAntiSpam = false;
        }

        private void RequestEnqueueCtrlPack()
        {
            _ctrlPackQueue.Enqueue(CtrlPack);
            //BUG 这里有Bug.
            /*if (!_inputAntiSpam)
            {
                _ctrlPackQueue.Enqueue(CtrlPack);
                _inputAntiSpam = true;
                _inputAntiSpamTimer = 0.0f;
            }*/

            //_ownerLogic.StartCoroutine(AntiSpamCoolDown()); //
        }

        /// <summary>
        ///上面也说了；这个Driver本质上是要建立一棵树、没有特别好将这棵树进行量化和配置化的流程；
        ///目前的流程是每个不同的Driver都从头建立一棵树。（重载RespondToControlEvent函数）
        ///先用下面这个流程去弄，这个List是有序的。
        /// </summary>
        protected abstract List<RespToCtrlEvent> RespondList { get; }

        protected void EnqueueBreakingCommand(BreakingCommand brkingCmd)
        {
            _breakingCMDQueue.Enqueue(brkingCmd);
        }

        private bool CtrlQueueNonEmpty => _ctrlPackQueue.Count != 0;
        public bool PendingRequestedBreak => _breakingCMDQueue.Count != 0;

        private Ray MouseScreenPosToRay(Vector2 screenpos) => Camera.main.ScreenPointToRay(new Vector3(screenpos.x, screenpos.y, 0.0f));

        private bool MouseDrivingFunction(ActionPack actionPack)
        {
            if (Camera.main == null)
            {
                Debug.LogWarning("not able to find main camera");
                return true;
                //throw new ApplicationException("无法获得主相机");
            }

            switch (actionPack.ActionID)
            {
                case MouseLeft:
                    var ray = MouseScreenPosToRay(actionPack.MouseScreenPosA);
                    if (Physics.Raycast(ray, out var hit))
                    {
                        //现在姑且规定、和Collider同一Transform才会调用IClickable
                        var clickablelist = hit.transform.gameObject.GetComponents<MonoBehaviour>()
                            .OfType<IClickable>();
                        foreach (var clickable in clickablelist) clickable.Clicked();
                    }

                    return true;
                case Drag:
                    RootDebug.Log("From " + actionPack.MouseScreenPosA + " to " + actionPack.MouseScreenPosB,
                        NameID.YanYoumo_Log);
                    Debug.LogError("NotImplementedException");
                    return true;
                default:
                    return false;
            }
        }

        protected bool CoreDrivingFunction(ActionPack actionPack)
        {
            if (WorldCycler.GamePausedStatus) return false;
            //Debug.Log("CoreDrivingFunction");
            //TODO 这个里面有些问题、还需要处理。
            if (MouseDrivingFunction(actionPack)) return false;

            FilterDir(actionPack, out var dir);
            if (dir.HasValue)
            {
                var cursor = _ownerLogic.LevelAsset.Cursor;
                if (cursor == null)
                {
                    Debug.LogWarning("try to move but no cursor");
                    return false;
                }

                CtrlPack.CommandDir = dir.Value;
                CtrlPack.ReplaceFlag(actionPack.HoldForDrag ? ControllingCommand.Drag : ControllingCommand.Move);
                CtrlPack.CurrentPos = _ownerLogic.LevelAsset.Cursor.CurrentBoardPosition;
                CtrlPack.NextPos = _ownerLogic.LevelAsset.Cursor.GetNeigbourCoord(CtrlPack.CommandDir);
            }
            else if (actionPack.IsAction(RotateUnit))
            {
                var cursor = _ownerLogic.LevelAsset.Cursor;
                if (cursor == null)
                {
                    Debug.LogWarning("try to move but no cursor");
                    return false;
                }
                CtrlPack.CurrentPos = _ownerLogic.LevelAsset.Cursor.CurrentBoardPosition;
                CtrlPack.SetFlag(ControllingCommand.Rotate);
            }

            if (actionPack.IsAction(CycleNext)) CtrlPack.SetFlag(ControllingCommand.CycleNext);
            if (actionPack.IsAction(Confirm0)) CtrlPack.SetFlag(ControllingCommand.Confirm);
            if (actionPack.IsAction(SwapConfirm)) CtrlPack.SetFlag(ControllingCommand.SwapConfirm);
            if (actionPack.IsAction(LeftAlt)) CtrlPack.SetFlag(ControllingCommand.Cancel);

            if (actionPack.IsAction(HintControl))
            {
                MessageDispatcher.SendMessage(actionPack.HoldForHint ? ToggleHintUIUpEvent : ToggleHintUIDownEvent);
                return false;
            }
            
            if (actionPack.IsAction(InGameOverLayToggle))
            {
                MessageDispatcher.SendMessage(InGameOverlayToggleEvent);
                return false;
            }

            //Debug.Log("CoreDrivingFunction");
            if (actionPack.IsAction(ShopTierUp))
            {
                //Debug.Log("SendMessage(new ShopTierOffsetChangedData {UpwardOrDownward = true})");
                MessageDispatcher.SendMessage(new ShopTierOffsetChangedData {UpwardOrDownward = true});
                return false;
            }

            if (actionPack.IsAction(ShopTierDown))
            {
                MessageDispatcher.SendMessage(new ShopTierOffsetChangedData {UpwardOrDownward = false});
                return false;
            }

            //TODO 下面两套的流程应该能有更好的管理方法。
            ShopBuyID(ref CtrlPack, in actionPack);
            SkillID(ref CtrlPack, in actionPack);
            return true;
        }

        public BreakingCommand RequestedBreakType
        {
            get
            {
                if (PendingRequestedBreak)
                {
                    // ReSharper disable once PossibleInvalidOperationException
                    return _breakingCMDQueue.Dequeue();
                }

                throw new ArgumentException();
            }
        }

        public ControllingPack CtrlQueueHeader
        {
            get
            {
                if (CtrlQueueNonEmpty) return _ctrlPackQueue.Dequeue();
                return new ControllingPack {CtrlCMD = ControllingCommand.Nop};
            }
        }

        protected ControlActionDriver(FSMLevelLogic ownerLogic, RootFSM fsm)
        {
            _ownerLogic = ownerLogic;
            _mainFsm = fsm;
            _ctrlPackQueue = new Queue<ControllingPack>();
            _breakingCMDQueue = new Queue<BreakingCommand>();
            MessageDispatcher.AddListener(ControllingEvent, RespondToControlEvent);
        }

        private void FilterDir(ActionPack actionPack, out RotationDirection? direction)
        {
            switch (actionPack.ActionID)
            {
                case CursorUp:
                    direction = RotationDirection.North;
                    return;
                case CursorDown:
                    direction = RotationDirection.South;
                    return;
                case CursorLeft:
                    direction = RotationDirection.West;
                    return;
                case CursorRight:
                    direction = RotationDirection.East;
                    return;
            }

            direction = null;
        }

        private int Base_1_0Conversion(int a)
        {
            a -= 1; //base 1=>0的转换。
            if (a < 0)
            {
                a += 10;
            }

            return a;
        }

        private bool SkillID(ref ControllingPack ctrlPack, in ActionPack actionPack)
        {
            if (actionPack.ActionID != FuncComp) return false;
            ctrlPack.SetFlag(ControllingCommand.Skill);
            ctrlPack.SkillID = Base_1_0Conversion(actionPack.FuncID);
            return true;
        }

        private bool ShopBuyID(ref ControllingPack ctrlPack, in ActionPack actionPack)
        {
            if (actionPack.ActionID != FuncComp) return false;
            ctrlPack.SetFlag(ControllingCommand.Buy);
            ctrlPack.ShopID = Base_1_0Conversion(actionPack.FuncID);
            return true;
        }

        protected ControllingPack CtrlPack;
        private bool _shouldQueue;


        //这个一定实要进行配置的、但是这个本质上是要构建一棵树、但是可以构建一棵树的方法还是FSM。
        private void RespondToControlEvent(IMessage rMessage)
        {
            var actionPack = rMessage as ActionPack;
            CtrlPack = new ControllingPack {CtrlCMD = ControllingCommand.Nop};
            _shouldQueue = false;
            //Debug.Log("RespondToControlEvent");
            foreach (var rsp in RespondList)
            {
                _shouldQueue |= rsp(actionPack);
            }

            if (_shouldQueue)
            {
                RequestEnqueueCtrlPack();
            }
        }

        public void Unsubscribe()
        {
            MessageDispatcher.RemoveListener(ControllingEvent, RespondToControlEvent, true);
        }
    }

    public class BaseControlActionDriver : ControlActionDriver
    {
        public BaseControlActionDriver(FSMLevelLogic ownerLogic, RootFSM fsm) : base(ownerLogic, fsm)
        {
        }

        protected override List<RespToCtrlEvent> RespondList
        {
            get
            {
                var res = new List<RespToCtrlEvent>
                {
                    CoreDrivingFunction,
                };
                return res;
            }
        }
    }

    public class CareerControlActionDriver : ControlActionDriver
    {
        public CareerControlActionDriver(FSMLevelLogic ownerLogic, RootFSM fsm) : base(ownerLogic, fsm)
        {
        }

        protected override List<RespToCtrlEvent> RespondList
        {
            get
            {
                var res = new List<RespToCtrlEvent>
                {
                    CoreDrivingFunction,
                    TelemetryRespondToControlEvent
                };
                return res;
            }
        }

        private bool TelemetryRespondToControlEvent(ActionPack actionPack)
        {
            if ((WorldCycler.TelemetryStage && actionPack.IsAction(TelemetryPause)))
            {
                if (WorldCycler.TelemetryPause)
                {
                    CtrlPack.SetFlag(ControllingCommand.TelemetryResume);
                }
                else
                {
                    EnqueueBreakingCommand(BreakingCommand.TelemetryPause);
                }
            }

            //Boss阶段非暂停的时候、输入不进入队列。
            //RISK 主要是下面这个、总觉得可能有冲突的问题。
            return !WorldCycler.TelemetryStage || WorldCycler.TelemetryPause;
        }
    }

    public class TutorialControlActionDriver : ControlActionDriver
    {
        private bool TutorialRespondToControlEvent(ActionPack actionPack)
        {
            if (actionPack.IsAction(ForceFlyUnit))
            {
                //Debug.Log("TutorialRespondToControlEvent");
                CtrlPack.SetFlag(ControllingCommand.ForceFlyUnit);
                CtrlPack.NextPos = actionPack.OnBoardPos;
                return true;
            }
            return false;
        }

        public TutorialControlActionDriver(FSMLevelLogic _owner, RootFSM fsm) : base(_owner, fsm)
        {
            
        }
        
        protected override List<RespToCtrlEvent> RespondList
        {
            get
            {
                var res = new List<RespToCtrlEvent>
                {
                    TutorialRespondToControlEvent,
                    CoreDrivingFunction,
                };
                return res;
            }
        }
    }
}