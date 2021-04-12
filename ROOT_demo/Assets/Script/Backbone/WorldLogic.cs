using System;
using Sirenix.Utilities;
using UnityEngine;
using CommandDir = ROOT.RotationDirection;
using Object = UnityEngine.Object;

// ReSharper disable PossiblyImpureMethodCallOnReadonlyVariable
namespace ROOT
{

    [Flags]
    public enum LogicCommand
    {
        Nop = 0,
        UpdateShop = 1 << 0,
        RotateUnit = 1 << 1,
        UpdateUnitCursor = 1 << 2,
        UpdateBoardData = 1 << 3,
        TelemetryUnpaused = 1 << 4,
        TelemetryTryUnpause = 1 << 5,

        //这个既然作为ESC命令、又是标记这个enum的结尾、
        //加的所有值需要在它上面，并且调整它的值
        ESC = 1 << 6
    }

    //之前的ControllingPack实质上是Deferred的处理；
    //这里LogicCommand大部分时间是即时的。
    //这种东西的数据交互一般都是个大问题；但是LevelAsset是好东西。
    //此时LogicPack可能没有Command本身意义大（enum本身就可以与）
    [Serializable]
    public struct LogicPack
    {
        public LogicCommand LogicCMD;

        public static bool HasFlag(LogicCommand a, LogicCommand b)
        {
            return (a & b) == b;
        }
    }
    
    #region WorldCycler
    
    //telemetry：遥感（Boss）
    
    internal static class WorldCycler
    {
        public static void Reset()
        {
            TelemetryStage = false;
            TelemetryPause = false;
            InitCycler();
        }

        public static bool AnimationTimeLongSwitch => TelemetryStage && !TelemetryPause;

        public static int Step => ActualStep;

        public static bool TelemetryStage = false;
        public static bool TelemetryPause = false;

        private static bool? RawNeedAutoDriveStep
        {
            get
            {
                if (ActualStep == ExpectedStep) return null;
                return ExpectedStep > ActualStep;
            }
        }

        /// <summary>
        /// NULL: 不需要自动演进。
        /// True: 需要自动往前演进。
        /// False:需要自动逆向演进。
        /// </summary>
        public static bool? NeedAutoDriveStep
        {
            get
            {
                if (TelemetryStage)
                {
                    if (TelemetryPause)
                    {
                        return null;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return RawNeedAutoDriveStep;
                }
            }
        }

        public static int ActualStep { private set; get; }
        public static int ExpectedStep { private set; get; }

        public static void InitCycler()
        {
            ActualStep = 0;
            ExpectedStep = 0;
        }

        public static void StepUp()
        {
            if (ExpectedStep < ActualStep)
            {
                throw new Exception("Should not further Increase Step when ExpectedStep is Lower");
            }
            else if (ExpectedStep > ActualStep)
            {
                ActualStep++;
            }
            else if (ExpectedStep == ActualStep)
            {
                ActualStep++;
                ExpectedStep++;
            }
        }

        public static void StepDown()
        {
            if (ExpectedStep > ActualStep)
            {
                throw new Exception("Should not further Decrease Step when ExpectedStep is Higher");
            }
            else if (ExpectedStep < ActualStep)
            {
                ActualStep--;
            }
            else if (ExpectedStep == ActualStep)
            {
                ActualStep--;
                ExpectedStep--;
            }
        }

        public static void ExpectedStepIncrement(int amount)
        {
            ExpectedStep += amount;
        }

        public static void ExpectedStepDecrement(int amount)
        {
            ExpectedStep -= amount;
        }
    }

    #endregion

    #region WorldController

    [Flags]
    public enum ControllingCommand
    {
        Nop = 0,
        Move = 1 << 0,
        Drag = 1 << 1,
        Rotate = 1 << 2,
        Buy = 1 << 3,
        PlayHint = 1 << 4,
        SignalHint = 1 << 5,
        NextButton = 1 << 6,
        CycleNext = 1 << 7,
        Cancel = 1 << 8,
        Confirm = 1 << 9,
        BuyRandom = 1 << 10,
        RemoveUnit = 1 << 11,
        Skill = 1 << 12,
        TelemetryResume = 1 << 13,
        CameraMov = 1 << 14,
        ClickOnGrid = 1 << 15,//日了，这个还是要铺满场地。
        FloatingOnGrid = 1 << 16//估计也能搞，而且早晚也得搞。
    }

    public enum BreakingCommand
    {
        Nop,
        TelemetryPause,
        TutorialContinue,
        QuitGame,
    }

    [Serializable]
    public struct ControllingPack
    {
        public ControllingCommand CtrlCMD;
        public CommandDir CommandDir;
        public Vector2Int CurrentPos;
        public Vector2Int NextPos;
        public Vector2 CameraMovement;
        public int ShopID;
        public int SkillID;

        public bool AnyFlag()
        {
            return CtrlCMD != ControllingCommand.Nop;
        }

        public bool IsFlag(ControllingCommand a)
        {
            return CtrlCMD == a;
        }

        public void MaskFlag(ControllingCommand a)
        {
            CtrlCMD &= a;
        }

        public bool HasFlag(ControllingCommand a)
        {
            return (CtrlCMD & a) == a;
        }

        public void ReplaceFlag(ControllingCommand a)
        {
            CtrlCMD = a;
        }

        public void SetFlag(ControllingCommand a)
        {
            CtrlCMD |= a;
        }

        public void UnsetFlag(ControllingCommand a)
        {
            CtrlCMD &= (~a);
        }

        public void ToggleFlag(ControllingCommand a)
        {
            CtrlCMD ^= a;
        }

        public static bool HasFlag(ControllingCommand a, ControllingCommand b)
        {
            return (a & b) == b;
        }
    }

    //要把Asset和Logic，把Controller也要彻底拆开。
    /*internal static class WorldController
    {
        private static bool idle = false;
        private const float minHoldTime = 1.5f;
        private const float minHoldShift = 1e-4f;
        private static Vector2? holdPos = null;
        private static float holdTime = 0;
        private static GameObject _pressedObj = null;
        private static bool _isSinglePress = false;
        private static Vector2Int? startPos;

        private static float pressTime = 0;

        //Somehow PlayerId 0 is 9999999 NOW!
        //没什么特别的，没有新建Player的SYSTEM系统才是9999999。
        private static int playerID = 0;
        private static readonly Player player = ReInput.players.GetPlayer(playerID);

        /// <summary>
        /// 技能系统要从这个地方接入。而且Cycle的管理部分要再整理起来。
        /// 比较蛋疼的是，Cycle完整管理起来，需要有一个前置条件，就是：Animation系统要整理明白。
        /// 就是Cycle系统要和现在耦合挺深的余下系统拆开，再Worldlogic里面再加一个夹层系统。
        ///  </summary>
        private static float _moveValThreadhold = 0.1f;

        private static bool GetCommandDir(out CommandDir dir)
        {
            bool anyDir = false;
            dir = CommandDir.North;

            if (player.GetButtonDown(StaticName.INPUT_BUTTON_NAME_CURSORUP))
            {
                dir = CommandDir.North;
                anyDir = true;
            }

            if (player.GetButtonDown(StaticName.INPUT_BUTTON_NAME_CURSORDOWN))
            {
                dir = CommandDir.South;
                anyDir = true;
            }

            if (player.GetButtonDown(StaticName.INPUT_BUTTON_NAME_CURSORLEFT))
            {
                dir = CommandDir.West;
                anyDir = true;
            }

            if (player.GetButtonDown(StaticName.INPUT_BUTTON_NAME_CURSORRIGHT))
            {
                dir = CommandDir.East;
                anyDir = true;
            }

            return anyDir;
        }

        private static bool GetCamMovementVec_KB(out Vector2 dir)
        {
            bool anyDir = false;
            dir = Vector2.zero;

            if (player.GetButton(StaticName.INPUT_BUTTON_NAME_CURSORUP))
            {
                dir += Vector2.up;
                anyDir = true;
            }

            if (player.GetButton(StaticName.INPUT_BUTTON_NAME_CURSORDOWN))
            {
                dir += Vector2.down;
                anyDir = true;
            }

            if (player.GetButton(StaticName.INPUT_BUTTON_NAME_CURSORLEFT))
            {
                dir += Vector2.left;
                anyDir = true;
            }

            if (player.GetButton(StaticName.INPUT_BUTTON_NAME_CURSORRIGHT))
            {
                dir += Vector2Int.right;
                anyDir = true;
            }

            dir = Vector3.Normalize(dir);
            return anyDir;
        }

        private static int _lastFingerID = -1;
        private static int _ObsoleteFingerID = -2;
        private static bool swiping = false;
        private static Vector2 _moveVal = Vector2.zero;
        private static Unit _touchingUnit = null;
        private static readonly float _holdThreadhold = 0.5f;
        private static float _holdTimer = 0.0f;
        private static bool _holdAntiSpam = false;

        private static GameObject GetTouchedOnGameObject(in Touch touch)
        {
            Ray ray = Camera.main.ScreenPointToRay(touch.position);
            if (Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                return hitInfo.collider.gameObject;
            }

            return null;
        }

        private static Unit GetTouchedOnUnit(in GameObject go)
        {
            if (go != null && go.CompareTag("Unit"))
            {
                return go.GetComponentInChildren<Unit>();
            }

            return null;
        }

        private static bool GetBeginSwipingOnUnit(in Touch touch, in Unit tmpTouchingUnit)
        {
            if (tmpTouchingUnit == null)
            {
                return false;
            }
            else
            {
                if (tmpTouchingUnit.ShopID == -1)
                {
                    _lastFingerID = touch.fingerId;
                    _touchingUnit = tmpTouchingUnit;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        private static void ResetSwipeStatus()
        {
            _lastFingerID = -1;
            _ObsoleteFingerID = -2;
            swiping = false;
            _touchingUnit = null;
            _moveVal = Vector2.zero;
        }

        private static CommandDir ConvertValToOffset(Vector2 val, out Vector2Int offset)
        {
            offset = Vector2Int.zero;
            var A = Vector2.Dot(Vector2.up, val);
            var B = Vector2.Dot(Vector2.right, val);
            var r1 = A > B;
            var r2 = A > -B;
            if (r1 && r2)
            {
                offset = new Vector2Int(0, 1);
                return CommandDir.North;
            }
            else if (r1)
            {
                offset = new Vector2Int(-1, 0);
                return CommandDir.West;
            }
            else if (r2)
            {
                offset = new Vector2Int(1, 0);
                return CommandDir.East;
            }
            else
            {
                offset = new Vector2Int(0, -1);
                return CommandDir.South;
            }
        }

        //更新触摸屏的控制逻辑，需要和键盘版同步。（先熟悉代码和特性，具体控制逻辑待定
        internal static void GetCommand_Touch(GameAssets currentLevelAsset, out ControllingPack ctrlPack)
        {
            //滑动这边，滑动过长会失效，这个也是个很神奇的bug，有空要看看
            //商店的新版流程正在弄。
            //先特么只考虑一个手指的情况。
            ctrlPack = new ControllingPack {CtrlCMD = ControllingCommand.Nop};
            if (Input.touchCount > 0)
            {
                if (Input.touchCount > 1)
                {
                    //Dual Finger touch
                    ctrlPack.SetFlag(ControllingCommand.SignalHint);
                }
                else
                {
                    Touch touch = Input.touches[0];
                    //不允许在滑动的同时还有一次别的手指点击的可能。
                    var touchedGo = GetTouchedOnGameObject(in touch);
                    Unit tmpTouchingUnit = GetTouchedOnUnit(in touchedGo); //这个在滑动过程中不要了。
                    Debug.Log("touch.tapCount ==" + touch.tapCount);
                    if (touch.tapCount >= 2)
                    {
                        //f[ʌ]ck，在DoubleTap前会出现一次SingleTap
                        //日，在接收到doubleTap前就已经被Swipe挪走了。
                        //在识别到是双击前，就已经完成一次单击了。 
                        //就是这里处理是：双击单元
                        if (_lastFingerID == touch.fingerId)
                        {
                            ResetSwipeStatus(); //是一次双击，要撤销上一次滑动的指令。
                        }

                        if (tmpTouchingUnit != null)
                        {
                            if (_ObsoleteFingerID != _lastFingerID)
                            {
                                _ObsoleteFingerID = _lastFingerID;
                                ctrlPack.SetFlag(ControllingCommand.Rotate);
                                ctrlPack.CurrentPos = tmpTouchingUnit.CurrentBoardPosition;
                            }
                        }
                    }
                    else
                    {
                        //目前滑动优先级高。//不行，双击需要可以打断滑动。
                        if (!swiping)
                        {
                            if (touch.tapCount == 1)
                            {
                                //就是这里处理是：开始滑动/一次点击
                                swiping = GetBeginSwipingOnUnit(in touch, in tmpTouchingUnit);
                                if (!swiping)
                                {
                                    //就是点击了一个商店的Unit。并且应该是是由一帧的状态。
                                    //这里的Buy会反复触发，但是有BoughtOnce帮忙挡住。

                                    //f^ck，如何判断是点击了一个一般单位还是滑动了一个一般单位？
                                    //干脆目前弄成双击旋转吧。
                                    if (tmpTouchingUnit)
                                    {
                                        //Anti-Spam
                                        ctrlPack.SetFlag(ControllingCommand.Buy);
                                        ctrlPack.ShopID = tmpTouchingUnit.ShopID;
                                    }

                                    if (touchedGo != null)
                                    {
                                        if (touchedGo.CompareTag("HelpScreen"))
                                        {
                                            ctrlPack.SetFlag(ControllingCommand.PlayHint);
                                        }
                                        else if (touchedGo.CompareTag("TutorialTextFrame"))
                                        {
                                            if (touch.phase == TouchPhase.Began) //Anti-Spam
                                            {
                                                ctrlPack.SetFlag(ControllingCommand.NextButton);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        switch (touch.phase)
                                        {
                                            case TouchPhase.Began:
                                                _holdTimer = 0.0f;
                                                break;
                                            case TouchPhase.Stationary:
                                            {
                                                _holdTimer += Time.deltaTime;
                                                if (!_holdAntiSpam)
                                                {
                                                    if (_holdTimer >= _holdThreadhold)
                                                    {
                                                        //Debug.Log("Hold Detected");
                                                        ctrlPack.SetFlag(ControllingCommand.CycleNext);
                                                        _holdAntiSpam = true;
                                                    }
                                                }

                                                break;
                                            }
                                            case TouchPhase.Canceled:
                                            case TouchPhase.Moved:
                                            case TouchPhase.Ended:
                                                _holdTimer = 0.0f;
                                                _holdAntiSpam = false;
                                                break;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            //进入滑动状态。
                            Debug.Assert(_lastFingerID == touch.fingerId);
                            switch (touch.phase)
                            {
                                case TouchPhase.Began:
                                    //DO NOTHING
                                    break;
                                case TouchPhase.Stationary:
                                    //Stationary说明可能是doubleTap
                                    ResetSwipeStatus(); //多少可以用。
                                    break;
                                case TouchPhase.Moved:
                                    _moveVal += touch.deltaPosition;
                                    break;
                                case TouchPhase.Ended:
                                    _moveVal += touch.deltaPosition;
                                    //_moveVal有一个最小阈值，和一般的单击分开（和慢速双击混淆）。
                                    if (_moveVal.magnitude >= _moveValThreadhold)
                                    {
                                        ctrlPack.ReplaceFlag(ControllingCommand.Drag);
                                        ctrlPack.CurrentPos = _touchingUnit.CurrentBoardPosition; //这里要反转一下？？
                                        ctrlPack.CommandDir = ConvertValToOffset(_moveVal, out var offset);
                                        ctrlPack.NextPos = ctrlPack.CurrentPos + offset;
                                    }

                                    ResetSwipeStatus();
                                    break;
                                case TouchPhase.Canceled:
                                    ResetSwipeStatus();
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                        }
                    }
                }
            }
        }

        internal static void GetCommand_Keyboard(GameAssets currentLevelAsset, out ControllingPack ctrlPack)
        {
            ctrlPack = new ControllingPack {CtrlCMD = ControllingCommand.Nop};
            var anyDir = GetCommandDir(out var Direction);
            var anyDirAxis = GetCamMovementVec_KB(out var directionAxis);
            if (anyDir)
            {
                ctrlPack.CommandDir = Direction;
                ctrlPack.ReplaceFlag(ControllingCommand.Move); //Replace
                if (player.GetButton(StaticName.INPUT_BUTTON_NAME_MOVEUNIT))
                {
                    ctrlPack.ReplaceFlag(ControllingCommand.Drag); //Replace
                }
            }

            ctrlPack.CurrentPos = currentLevelAsset.Cursor.CurrentBoardPosition;
            ctrlPack.NextPos = currentLevelAsset.Cursor.GetCoord(ctrlPack.CommandDir);

            if (player.GetButtonDown(StaticName.INPUT_BUTTON_NAME_REMOVEUNIT))
            {
                //ctrlPack.CurrentPos = currentLevelAsset.Cursor.CurrentBoardPosition;
                //ctrlPack.SetFlag(ControllingCommand.RemoveUnit);
            }

            if (player.GetButtonDown(StaticName.INPUT_BUTTON_NAME_ROTATEUNIT) &&
                ctrlPack.CtrlCMD == ControllingCommand.Nop)
            {
                //移动和拖动的优先级比旋转高。
                ctrlPack.CurrentPos = currentLevelAsset.Cursor.CurrentBoardPosition;
                ctrlPack.SetFlag(ControllingCommand.Rotate);
            }

            if (player.GetButton(StaticName.INPUT_BUTTON_NAME_HINTHDD) ||
                player.GetButton(StaticName.INPUT_BUTTON_NAME_HINTNET))
            {
                ctrlPack.SetFlag(ControllingCommand.SignalHint);
            }

            if (player.GetButton(StaticName.INPUT_BUTTON_NAME_HINTCTRL))
            {
                ctrlPack.SetFlag(ControllingCommand.PlayHint);
            }

            if (player.GetButtonDown(StaticName.INPUT_BUTTON_NAME_CYCLENEXT))
            {
                ctrlPack.SetFlag(ControllingCommand.CycleNext);
            }

            if (player.GetButtonDown(StaticName.INPUT_BUTTON_NAME_CONFIRM))
            {
                ctrlPack.SetFlag(ControllingCommand.Confirm);
            }

            if (player.GetButtonDown(StaticName.INPUT_BUTTON_NAME_CANCELED))
            {
                ctrlPack.SetFlag(ControllingCommand.Cancel);
            }

            if (currentLevelAsset.BuyingCursor)
            {
                if (player.GetButtonDown(StaticName.INPUT_BUTTON_NAME_SHOPRANDOM))
                {
                    ctrlPack.SetFlag(ControllingCommand.BuyRandom);
                }
            }

            var anyBuy = ShopBuyID(ref ctrlPack);
            var anySkill = SkillID(ref ctrlPack);

            //TODO 下面是对Camera写一段测试代码，尽快整合起来。
            if (Input.GetKey(KeyCode.LeftAlt) && anyDirAxis)
            {
                ctrlPack.SetFlag(ControllingCommand.CameraMov);
                Debug.Log("KeyCode.LeftAlt");
                ctrlPack.CameraMovement = directionAxis;
            }
        }

        private static bool GetPlayerMouseOverObject(out RaycastHit hitInfo)
        {
            var mouseScnPos = player.controllers.Mouse.screenPosition;
            var ray = Camera.main.ScreenPointToRay(mouseScnPos);
            var hit = Physics.Raycast(ray, out hitInfo);
            return hit;
        }

        internal static void GetCommand_Mouse(GameAssets currentLevelAsset, out ControllingPack ctrlPack)
        {
            //TEMP 现在鼠标的输入是可以挂属在键盘之后的。
            //光标：不需要。
            //移动单位：拖动。
            //旋转单位：双击单位。
            //购买和技能：点击。
            //下一回合：
            //Boss阶段暂停：因为时序问题还是键盘。
            ctrlPack = new ControllingPack {CtrlCMD = ControllingCommand.Nop};
            GetPlayerMouseOverObject(out var hitInfo2);
            if (idle == false && Board.WorldPosToXZGrid(hitInfo2.point).HasValue)
            {
                ctrlPack.SetFlag(ControllingCommand.FloatingOnGrid);
                ctrlPack.CurrentPos = Board.WorldPosToXZGrid(hitInfo2.point).Value;
            }

            if (idle == false && player.GetButtonDoublePressDown("Confirm0"))
            {
                if (_pressedObj != null && Utils.IsBoardUint(_pressedObj))
                {
                    DoublePress(ref ctrlPack, Utils.GetUnit(_pressedObj));
                }

                _pressedObj = null;
                _isSinglePress = false;
            }

            else if (idle == false && player.GetButtonDown("Confirm0"))
            {
                var hit = GetPlayerMouseOverObject(out var hitInfo);
                holdPos = player.controllers.Mouse.screenPosition;
                holdTime = Time.time;
                if (hit)
                {
                    _pressedObj = hitInfo.transform.gameObject;
                    Debug.Log("Single press down " + _pressedObj.name);
                    if (Utils.IsUnit(_pressedObj) ||
                        Utils.IsSkillPalette(_pressedObj) ||
                        Utils.IsOnGrid(hitInfo))
                    {
                        pressTime = Time.time;
                        startPos = Board.WorldPosToXZGrid(hitInfo.point);
                    }
                    else
                    {
                        _pressedObj = null;
                        startPos = null;
                    }
                }
                else
                {
                    _pressedObj = null;
                }
            }

            else if (player.GetButtonUp("Confirm0"))
            {
                if (idle)
                {
                    idle = false;
                }

                var hit = GetPlayerMouseOverObject(out var hitInfo);
                //hitInfo.point;
                if (hit)
                {
                    var pressedObj2 = hitInfo.transform.gameObject;
                    Debug.Log("Single press up " + pressedObj2.name);
                    if (_pressedObj == pressedObj2 ||
                        (Utils.IsOnGrid(hitInfo)) && startPos == Board.WorldPosToXZGrid(hitInfo.point))
                    {
                        _isSinglePress = true;
                    }
                    else
                    {
                        Drag(ref ctrlPack, startPos, Board.WorldPosToXZGrid(hitInfo.point));
                    }
                }
                else
                {
                    _pressedObj = null;
                }

                holdPos = null;
                holdTime = 0;
            }
            //双击的时间阈值是0.3s
            else if (idle == false && _isSinglePress && Time.time - pressTime >= 0.3)
            {
                Debug.Log("Regarded as single press");

                SinglePress(ref ctrlPack);

                _pressedObj = null;
                _isSinglePress = false;
            }

            if (idle == false && holdTime != 0 && Time.time - holdTime >= minHoldTime && holdPos.HasValue &&
                Utils.GetCustomizedDistance(holdPos.Value, player.controllers.Mouse.screenPosition) < minHoldShift)
            {
                idle = true;
                ctrlPack.SetFlag(ControllingCommand.CycleNext);
                holdPos = null;
                holdTime = 0;
                _pressedObj = null;
            }
        }

        private static void SinglePress(ref ControllingPack ctrlPack)
        {
            if (!Utils.IsBoardUint(_pressedObj))
            {
                if (Utils.IsUnit(_pressedObj))
                {
                    Debug.Log("Single press on " + _pressedObj.name);
                    ctrlPack.SetFlag(ControllingCommand.Buy);
                    ctrlPack.ShopID = Utils.GetUnit(_pressedObj).ShopID;
                }
                else if (Utils.IsSkillPalette(_pressedObj))
                {
                    Debug.Log("Single press on " + _pressedObj.name);
                    ctrlPack.SetFlag(ControllingCommand.Skill);
                    ctrlPack.SkillID = Utils.GetSkillPalette(_pressedObj).SkillID;
                }
                else
                {
                    ctrlPack.SetFlag(ControllingCommand.ClickOnGrid);
                    ctrlPack.CurrentPos = startPos.Value;
                    Debug.Log("Single press on grid " + startPos);
                }
            }
            else
            {
                ctrlPack.SetFlag(ControllingCommand.ClickOnGrid);
                ctrlPack.CurrentPos = startPos.Value;
                Debug.Log("Single press on grid " + startPos);
            }
        }

        private static void Drag(ref ControllingPack ctrlPack, Vector2Int? from, Vector2Int? to)
        {
            if (from.HasValue == false || to.HasValue == false)
                return;
            var dir = GetDir(from.Value, to.Value);
            if (dir.HasValue == false)
                return;
            ctrlPack.CommandDir = dir.Value;
            ctrlPack.CurrentPos = from.Value;
            ctrlPack.NextPos = to.Value;
            ctrlPack.SetFlag(ControllingCommand.Drag);
            ctrlPack.SetFlag(ControllingCommand.Move);
            Debug.Log("Drag from " + from + " to " + to);
        }

        private static void DoublePress(ref ControllingPack ctrlPack, Unit unit)
        {
            ctrlPack.CurrentPos = unit.CurrentBoardPosition;
            ctrlPack.SetFlag(ControllingCommand.Rotate);
        }

        private static CommandDir? GetDir(Vector2Int from, Vector2Int to)
        {
            if (to - from == Vector2Int.up)
                return CommandDir.North;
            if (to - from == Vector2Int.right)
                return CommandDir.East;
            if (to - from == Vector2Int.down)
                return CommandDir.South;
            if (to - from == Vector2Int.left)
                return CommandDir.West;
            return null;
        }

        private static bool SkillID(ref ControllingPack ctrlPack)
        {
            var anySkill = false;

            for (var i = 0; i < StaticName.INPUT_BUTTON_NAME_SKILLS.Length; i++)
            {
                if (player.GetButtonDown(StaticName.INPUT_BUTTON_NAME_SKILLS[i]))
                {
                    anySkill = true;
                    ctrlPack.SkillID = i;
                    break;
                }
            }

            if (anySkill)
            {
                ctrlPack.SetFlag(ControllingCommand.Skill);
            }

            return anySkill;
        }

        private static bool ShopBuyID(ref ControllingPack ctrlPack)
        {
            var anyBuy = false;

            for (var i = 0; i < StaticName.INPUT_BUTTON_NAME_SHOPBUYS.Length; i++)
            {
                if (player.GetButtonDown(StaticName.INPUT_BUTTON_NAME_SHOPBUYS[i]))
                {
                    anyBuy = true;
                    ctrlPack.ShopID = i;
                    break;
                }
            }

            if (anyBuy) ctrlPack.SetFlag(ControllingCommand.Buy);
            return anyBuy;
        }

        public static ControllingPack UpdateInputScheme(
            GameAssets currentLevelAsset)
        {
            var ctrlPack = new ControllingPack {CtrlCMD = ControllingCommand.Nop};
            if (StartGameMgr.UseTouchScreen)
            {
                GetCommand_Touch(currentLevelAsset, out ctrlPack);
            }
            else
            {
                if (StartGameMgr.UseKeyboard)
                {
                    if (player.controllers.Mouse.GetAnyButton())
                    {
                        StartGameMgr.SetUseMouse();
                    }
                }
                else if (StartGameMgr.UseMouse)
                {
                    if (player.controllers.Keyboard.GetAnyButton())
                    {
                        StartGameMgr.SetUseKeyboard();
                    }
                }

                if (currentLevelAsset.CursorEnabled)
                {

                    if (StartGameMgr.UseKeyboard)
                    {
                        GetCommand_Keyboard(currentLevelAsset, out ctrlPack);
                    }
                    else if (StartGameMgr.UseMouse)
                    {
                        GetCommand_Mouse(currentLevelAsset, out ctrlPack);
                    }
                }

                if (player.GetButtonDown(StaticName.INPUT_BUTTON_NAME_NEXT))
                {
                    ctrlPack.SetFlag(ControllingCommand.NextButton);
                }
            }

            if (currentLevelAsset.BuyingCursor)
            {
                ctrlPack.MaskFlag(ControllingCommand.BuyRandom
                                  | ControllingCommand.Cancel
                                  | ControllingCommand.Confirm
                                  | ControllingCommand.Move);
            }

            return ctrlPack;
        }
    }*/

    #endregion

    #region WorldExecutor

    /*public static class WorldExecutor_Dispatcher
    {

        //TODO 正在做FSM重构；这里的代码先都停掉。
        //RISK 这里应该尽量变成一个Dispatcher（尽量做dispatch），实际的逻辑放到WorldUtils
        //说白了，这个函数应该只发命令不管别的。
        public static void Root_Executor_void_PUBLIC(LogicCommand cmd, ref GameAssets gameAsset)
        {
            Root_Executor(cmd, ref gameAsset);
        }

        internal static void Root_Executor_Compound_Unordered(
            in LogicCommand cmd, ref GameAssets gameAsset,
            in ControllingPack ctrlPack)
        {
            var counter = 0;
            do
            {
                var tick = (LogicCommand) (1 << counter);
                if (tick >= LogicCommand.ESC) break;
                if (LogicPack.HasFlag(cmd, tick))
                {
                    Root_Executor(tick, ref gameAsset, in ctrlPack);
                }
                counter++;
            } while (true);
        }

        internal static void Root_Executor_Compound_Ordered(
            in LogicCommand[] cmds, ref GameAssets gameAsset,
            in ControllingPack ctrlPack, out Dictionary<LogicCommand, object> Res)
        {
            var resArray = new Dictionary<LogicCommand,object>();
            foreach (var t in cmds)
            {
                Root_Executor(t, ref gameAsset, in ctrlPack, out var tRes);
                resArray.Add(t, tRes);
            }
            Res = resArray;
        }

        internal static void Root_Executor(in LogicCommand cmd, ref GameAssets gameAsset)
        {
            var ctrlPack=new ControllingPack();
            Root_Executor(cmd, ref gameAsset, in ctrlPack, out var pRes);
        }

        internal static void Root_Executor(in LogicCommand cmd, ref GameAssets gameAsset, in ControllingPack ctrlPack)
        {
            Root_Executor(cmd, ref gameAsset, in ctrlPack, out var pRes);
        }

        //这个应该是最核心的、别的种类应该都是它的shell。
        internal static void Root_Executor(
            in LogicCommand cmd,ref GameAssets gameAsset,
            in ControllingPack ctrlPack,out object Res)
        {
            Res = null;
            //这个玩意儿的逻辑核心尽量就是这么简单的一个switch、Overhead尽量小。
            switch (cmd)
            {
                case LogicCommand.UpdateShop:
                    Res=WorldExecutor.UpdateShopBuy(ref gameAsset, in ctrlPack);
                    break;
                case LogicCommand.RotateUnit:
                    WorldExecutor.UpdateRotate(ref gameAsset, in ctrlPack);
                    break;
                case LogicCommand.UpdateUnitCursor:
                    WorldExecutor.UpdateCursor_Unit(ref gameAsset, in ctrlPack, out var movedTile, out var movedCursor);
                    Res = new [] {movedTile, movedCursor };
                    break;
                case LogicCommand.UpdateBoardData:
                    WorldExecutor.UpdateBoardData(ref gameAsset);
                    break;
                case LogicCommand.BossUnpaused:
                    WorldExecutor.BossStagePauseRunStop(ref gameAsset);
                    break;
                case LogicCommand.BossTryUnpause:
                    WorldExecutor.BossStagePauseTriggered(ref gameAsset);
                    break;
                case LogicCommand.Nop:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(cmd), cmd, null);
            }
        }
    }*/
    
    //原WORLD-UTILS 是为了进一步解耦，和一般的Utils只能基于基础数学不同，这个允许基于游戏逻辑和游戏制度。
    //现为：WorldExecutor，主要是执行具体的内部逻辑；想象为舰长和执行舰长的感觉吧。
    public static class WorldExecutor //WORLD-EXECUTOR
    {
        /*private static void UpdateLevelAsset(ref GameAssets levelAsset,ref FSMLevelLogic lvlLogic)
        {
            var lastStage = lvlLogic.RoundLibDriver.PreviousRoundGist?.Type ?? StageType.Shop;
            var lastDestoryBool = lastStage == StageType.Destoryer;
            
            if (lvlLogic.RoundLibDriver.IsRequireRound && lvlLogic.IsForwardCycle)
            {
                levelAsset.GameBoard.BoardGirdDriver.UpdatePatternDiminishing();
            }

            if ((lastDestoryBool && !lvlLogic.RoundLibDriver.IsDestoryerRound) && !WorldCycler.NeedAutoDriveStep.HasValue)
            {
                levelAsset.GameBoard.BoardGirdDriver.DestoryHeatsinkOverlappedUnit();
            }

            if ((levelAsset.DestroyerEnabled && !lvlLogic.RoundLibDriver.IsDestoryerRound) && !WorldCycler.TelemetryStage)
            {
                levelAsset.WarningDestoryer.ForceReset();
            }
        }*/

        private static float TypeASignalScore = 0;//Instance写
        private static float TypeBSignalScore = 0;//Stepped读
        private static int TypeASignalCount = 0;//Instance写
        private static int TypeBSignalCount = 0;//Stepped读

        //这里哪敢随便改成基于事件的啊；这里都是很看重时序的东西。
        //但是改成基于事件的解耦特性还是值得弄的、但是得注意。
        public static void InitCursor(GameAssets currentLevelAsset,Vector2Int pos)
        {
            currentLevelAsset.GameCursor = Object.Instantiate(currentLevelAsset.CursorTemplate);
            Cursor cursor = currentLevelAsset.GameCursor.GetComponent<Cursor>();
            cursor.InitPosWithAnimation(pos);
            cursor.UpdateTransform(currentLevelAsset.GameBoard.GetFloatTransformAnimation(cursor.LerpingBoardPosition));
        }

        public static void InitDestoryer(ref GameAssets LevelAsset)
        {
            if (LevelAsset.WarningDestoryer == null)
            {
                Debug.LogError("WarningDestoryer is null, please fix."); return;
            }
            LevelAsset.WarningDestoryer = new MeteoriteBomber {GameBoard = LevelAsset.GameBoard};
            LevelAsset.WarningDestoryer.Init(4, 1);
        }

        public static void InitAndStartShop(GameAssets LevelAsset)
        {
            try
            {
                LevelAsset.Shop.ShopStart();
            }
            catch (NullReferenceException)
            {
                Debug.LogError("Shop is null, please fix.");
            }
        }

        public static void UpdateRotate(ref GameAssets currentLevelAsset, in ControllingPack ctrlPack)
        {
            if (ctrlPack.HasFlag(ControllingCommand.Rotate))
            {
                if (currentLevelAsset.GameBoard.CheckBoardPosValidAndFilled(ctrlPack.CurrentPos))
                {
                    var unit = currentLevelAsset.GameBoard.FindUnitUnderBoardPos(ctrlPack.CurrentPos);
                    //RISK 下面这句话控制stationary单位是否能旋转、目前出于教程的用途（实际玩法框架中目前没有这个机制）；也禁止旋转。
                    //RISK 但是、如果这个feature要放到实际玩法框架中、可能还要移动、旋转细分。
                    if (unit.GetComponentInChildren<Unit>().StationUnit) return;
                    System.Diagnostics.Debug.Assert(unit != null, nameof(unit) + " != null");
                    unit.GetComponentInChildren<Unit>().UnitRotateCw();
                    //currentLevelAsset.GameBoard.UpdateBoard();
                }
            }
        }

        public static void UpdateCursor_Unit(ref GameAssets currentLevelAsset, in ControllingPack ctrlPack, out bool movedTile, out bool movedCursor)
        {
            //这个还要能够处理enableCursor是false的情况。
            //相当于现在这个函数是Cursor和Unit混在一起的，可能还需要拆开。
            movedTile = false;
            movedCursor = false;

            var validAction = currentLevelAsset.GameBoard.CheckBoardPosValidAndFilled(ctrlPack.CurrentPos) &&
                              currentLevelAsset.GameBoard.CheckBoardPosValidAndEmpty(ctrlPack.NextPos);
            var extractedCommand = ctrlPack.CtrlCMD & (ControllingCommand.Drag | ControllingCommand.Move);

            if (ControllingPack.HasFlag(extractedCommand, ControllingCommand.Drag) && validAction)
            {
                var unit = currentLevelAsset.GameBoard.FindUnitUnderBoardPos(ctrlPack.CurrentPos);
                if (!unit.GetComponentInChildren<Unit>().StationUnit)
                {
                    System.Diagnostics.Debug.Assert(unit != null, nameof(unit) + " != null");
                    var movingUnit = unit.GetComponentInChildren<Unit>();
                    movingUnit.Move(ctrlPack.CommandDir);
                    currentLevelAsset.GameBoard.UpdateUnitBoardPosAnimation(ctrlPack.CurrentPos);
                    if (StartGameMgr.UseKeyboard)
                    {
                        currentLevelAsset.Cursor.Move(ctrlPack.CommandDir);
                    }
                    currentLevelAsset.AnimationPendingObj.Add(movingUnit);
                    movedTile = true;
                    movedCursor = true;
                }
            }
            else if (ControllingPack.HasFlag(extractedCommand, ControllingCommand.Move))
            {
                currentLevelAsset.Cursor.Move(ctrlPack.CommandDir);
                movedCursor = true;
            }
            else if (ctrlPack.HasFlag(ControllingCommand.RemoveUnit))
            {
                currentLevelAsset.GameBoard.TryDeleteCertainUnit(ctrlPack.CurrentPos);
                //movedTile = true;//RISK 这里可以调整删除单位是否强制移动。目前不移动。
                movedCursor = true;
            }

            if (currentLevelAsset.CursorEnabled && movedCursor)
            {
                currentLevelAsset.AnimationPendingObj.Add(currentLevelAsset.Cursor);
                UpdateCursorPos(currentLevelAsset);
                movedCursor = true;
            }

            //Debug.Log("movedTile = true;" + movedTile);
        }

        public static bool UpdateShopBuy(ref GameAssets currentLevelAsset, in ControllingPack ctrlPack)
        {
            //先简单一些，只允许随机购买。
            //现在不是因为简单而购买了；而是设计上随机位置变成重要的一环。
            if (ctrlPack.HasFlag(ControllingCommand.Buy) && currentLevelAsset.Shop.ShopOpening)
            {
                return currentLevelAsset.Shop.BuyToRandom(ctrlPack.ShopID);
            }

            return false;
        }

        [Obsolete]
        public static void UpdateShopBuy(
            GameAssets currentLevelAsset, ShopSelectableMgr shopMgr,
            in ControllingPack ctrlPack, bool crashable,
            ref bool boughtOnce, out int postalPrice)
        {
            postalPrice = -1;
            if (!boughtOnce)
            {
                var successBought = false;

                //需要处理在选择送货地址时不许做其他操作(移动、旋转、跳过等)的逻辑。
                //购买的时候显示定位和随机和取消的操作提示，定位上要显示添加的价格。
                //玩家在选择送货地址的时候，要标记出哪个是准备要购买的（利用station类似的系统）
                //TODO 购买失败还是需要一些提示（之前也没有）
                //TEMP 基于HQ购买的还需要将指针处理一下。
                if (!currentLevelAsset.BuyingCursor)
                {
                    if (ctrlPack.HasFlag(ControllingCommand.Buy))
                    {
                        //商店系统要大改，首先先选择一个单元，先判断能不能买。
                        currentLevelAsset.BuyingCursor = shopMgr.RequestBuy(ctrlPack.ShopID, out postalPrice);
                        currentLevelAsset.BuyingID = ctrlPack.ShopID;
                    }
                }
                else
                {
                    //然后进入送货地址选择的位置时候，这个时候玩家可以取消也可以选择随机
                    //如果随机的话，比送过价格要低一些。
                    //目前商店上的价格写的是随机送的价格，即要送货再加X元，随机送不加价。
                    if (ctrlPack.HasFlag(ControllingCommand.Cancel))
                    {
                        shopMgr.ResetPendingBuy();
                        currentLevelAsset.BuyingCursor = false;
                        currentLevelAsset.BuyingID = -1;
                    }
                    else if (ctrlPack.HasFlag(ControllingCommand.Confirm))
                    {
                        //试图本地购买。
                        var requirement = crashable
                            ? currentLevelAsset.GameBoard.CheckBoardPosValid(ctrlPack.CurrentPos)
                            : currentLevelAsset.GameBoard.CheckBoardPosValidAndEmpty(ctrlPack.CurrentPos);

                        if (requirement)
                        {
                            successBought = shopMgr.BuyToPos(currentLevelAsset.BuyingID, ctrlPack.CurrentPos,
                                crashable);
                            if (successBought)
                            {
                                currentLevelAsset.BuyingCursor = false;
                                currentLevelAsset.BuyingID = -1;
                            }
                        }
                    }
                    else if (ctrlPack.HasFlag(ControllingCommand.BuyRandom))
                    {
                        //试图随机购买。
                        successBought = shopMgr.BuyToRandom(currentLevelAsset.BuyingID);
                        currentLevelAsset.BuyingCursor = false;
                        currentLevelAsset.BuyingID = -1;
                    }
                }

                boughtOnce = successBought;
            }
        }

        public static GameObject CreateIndicator(GameAssets currentLevelAsset, Vector2Int pos, Color col, bool playerCursor = false)
        {
            var indicator = Object.Instantiate(currentLevelAsset.CursorTemplate);
            var indicatorCursor = indicator.GetComponent<Cursor>();
            indicatorCursor.SetIndMesh();
            indicatorCursor.InitPosWithAnimation(pos);
            if (playerCursor)
            {
                UpdateCursorPos(currentLevelAsset);
            }
            indicatorCursor.UpdateTransform(currentLevelAsset.GameBoard.GetFloatTransform(indicatorCursor.CurrentBoardPosition));
            indicatorCursor.CursorColor = col;
            return indicator;
        }

        private static void UpdateCursorPos(GameAssets currentLevelAsset)
        {
            currentLevelAsset.Cursor.SetPosWithAnimation(Board.ClampPosInBoard(currentLevelAsset.Cursor.CurrentBoardPosition), PosSetFlag.Current);
            currentLevelAsset.Cursor.SetPosWithAnimation(Board.ClampPosInBoard(currentLevelAsset.Cursor.NextBoardPosition), PosSetFlag.Next);
        }

        public static void UpdateDestoryer(GameAssets currentLevelAsset)
        {
            //if (currentLevelAsset.WarningDestoryer.GetStatus != WarningDestoryerStatus.Dormant)
            //TEMP 现在警告-1；
            if (currentLevelAsset.WarningDestoryer.GetStatus == WarningDestoryerStatus.Striking)
            {
                var incomings = currentLevelAsset.WarningDestoryer.NextStrikingPos(out var count);
                currentLevelAsset.WarningGo = new GameObject[count];
                for (var i = 0; i < count; i++)
                {
                    Color col = currentLevelAsset.WarningDestoryer.GetWaringColor;
                    currentLevelAsset.WarningGo[i] = CreateIndicator(currentLevelAsset, incomings[i], col, true);
                }
            }
        }

        public static void CleanDestoryer(GameAssets currentLevelAsset)
        {
            if (currentLevelAsset.WarningGo == null || currentLevelAsset.WarningGo.Length <= 0) return;
            currentLevelAsset.WarningGo.ForEach(go => Object.Destroy(go));
            currentLevelAsset.WarningGo = null;
        }

        public static void LightUpBoard(ref GameAssets currentLevelAsset,ControllingPack _ctrlPack)
        {
            //TODO 这里的代码未来要自己去取鼠标的值。
            if (_ctrlPack.HasFlag(ControllingCommand.FloatingOnGrid) ||
                _ctrlPack.HasFlag(ControllingCommand.ClickOnGrid))
            {
                if (_ctrlPack.HasFlag(ControllingCommand.FloatingOnGrid))
                {
                    currentLevelAsset.GameBoard.BoardGirdDriver.LightUpBoardGird(_ctrlPack.CurrentPos);
                }

                if (_ctrlPack.HasFlag(ControllingCommand.ClickOnGrid))
                {
                    currentLevelAsset.GameBoard.BoardGirdDriver.LightUpBoardGird(_ctrlPack.CurrentPos,
                        LightUpBoardGirdMode.REPLACE,
                        LightUpBoardColor.Clicked);
                }
            }
            else
            {
                currentLevelAsset.GameBoard.BoardGirdDriver.LightUpBoardGird(Vector2Int.zero, LightUpBoardGirdMode.CLEAR);
            }
        }
    }

    #endregion
}