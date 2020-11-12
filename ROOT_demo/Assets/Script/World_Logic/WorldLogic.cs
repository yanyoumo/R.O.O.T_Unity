using System;
using Rewired;
using UnityEngine;
using CommandDir = ROOT.RotationDirection;
// ReSharper disable PossiblyImpureMethodCallOnReadonlyVariable

namespace ROOT
{
    #region WorldCycler

    /// <summary>
    /// WorldLogic或者Controller还是要通过这个来影响Cycle，毕竟之后可以快速往前还能往后。
    /// 现在的一个问题就是Level对Step的动作是一个单向的。就是Level单纯地提高Step的计数。
    /// </summary>
    internal static class WorldCycler
    {
        public static int Step => ActualStep;

        /// <summary>
        /// NULL: 不需要自动演进。
        /// True: 需要自动往前演进。
        /// False:需要自动逆向演进。
        /// </summary>
        public static bool? NeedAutoDriveStep
        {
            get
            {
                if (ActualStep==ExpectedStep) return null;
                return ExpectedStep > ActualStep;
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
            if (ExpectedStep>ActualStep)
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
        Nop =         0,
        Move =        1<<0,
        Drag =        1<<1,
        Rotate =      1<<2,
        Buy =         1<<3,
        PlayHint =    1<<4,
        SignalHint =  1<<5,
        NextButton =  1<<6,
        CycleNext =   1<<7,
        Cancel =      1<<8,
        Confirm =     1<<9,
        BuyRandom =   1<<10,
        RemoveUnit =  1<<11,
        Skill =       1<<12,
    }

    public struct ControllingPack
    {
        public ControllingCommand CtrlCMD;
        public CommandDir CommandDir;
        public Vector2Int CurrentPos;
        public Vector2Int NextPos;
        public int ShopID;
        public int SkillID;

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
    internal static class WorldController
    {
        // BUG Somehow PlayerId 0 is 9999999 NOW!
        private static Player player = ReInput.players.GetPlayer(9999999);
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
                if (Input.touchCount>1)
                {
                    //Dual Finger touch
                    ctrlPack.SetFlag(ControllingCommand.SignalHint);
                }
                else
                {
                    Touch touch = Input.touches[0];
                    //不允许在滑动的同时还有一次别的手指点击的可能。
                    var touchedGo=GetTouchedOnGameObject(in touch);
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

        internal static void GetCommand_KM(GameAssets currentLevelAsset, out ControllingPack ctrlPack)
        {
            ctrlPack = new ControllingPack {CtrlCMD = ControllingCommand.Nop};
            if (GetCommandDir(out ctrlPack.CommandDir))
            {
                ctrlPack.ReplaceFlag(ControllingCommand.Move); //Replace
                if (Input.GetButton(StaticName.INPUT_BUTTON_NAME_MOVEUNIT))
                {
                    ctrlPack.ReplaceFlag(ControllingCommand.Drag); //Replace
                }
            }

            ctrlPack.CurrentPos = currentLevelAsset.Cursor.CurrentBoardPosition;
            ctrlPack.NextPos = currentLevelAsset.Cursor.GetCoord(ctrlPack.CommandDir);

            if (Input.GetButtonDown(StaticName.INPUT_BUTTON_NAME_REMOVEUNIT))
            {
                ctrlPack.CurrentPos = currentLevelAsset.Cursor.CurrentBoardPosition;
                ctrlPack.SetFlag(ControllingCommand.RemoveUnit);
            }

            if (Input.GetButtonDown(StaticName.INPUT_BUTTON_NAME_ROTATEUNIT) &&
                ctrlPack.CtrlCMD == ControllingCommand.Nop)
            {
                //移动和拖动的优先级比旋转高。
                ctrlPack.CurrentPos = currentLevelAsset.Cursor.CurrentBoardPosition;
                ctrlPack.SetFlag(ControllingCommand.Rotate);
            }

            if (Input.GetButton(StaticName.INPUT_BUTTON_NAME_HINTHDD) ||
                Input.GetButton(StaticName.INPUT_BUTTON_NAME_HINTNET))
            {
                ctrlPack.SetFlag(ControllingCommand.SignalHint);
            }

            if (Input.GetButton(StaticName.INPUT_BUTTON_NAME_HINTCTRL))
            {
                ctrlPack.SetFlag(ControllingCommand.PlayHint);
            }

            if (Input.GetButtonDown(StaticName.INPUT_BUTTON_NAME_CYCLENEXT))
            {
                ctrlPack.SetFlag(ControllingCommand.CycleNext);
            }

            if (Input.GetButtonDown(StaticName.INPUT_BUTTON_NAME_CONFIRM))
            {
                ctrlPack.SetFlag(ControllingCommand.Confirm);
            }

            if (Input.GetButtonDown(StaticName.INPUT_BUTTON_NAME_CANCELED))
            {
                ctrlPack.SetFlag(ControllingCommand.Cancel);
            }

            if (currentLevelAsset.BuyingCursor)
            {

                /*if (Input.GetButtonDown(StaticName.INPUT_BUTTON_NAME_SHOPCONFIRM))
                {
                    ctrlPack.SetFlag(ControllingCommand.Confirm);
                }*/

                if (Input.GetButtonDown(StaticName.INPUT_BUTTON_NAME_SHOPRANDOM))
                {
                    ctrlPack.SetFlag(ControllingCommand.BuyRandom);
                }
            }

            var anyBuy = ShopBuyID(ref ctrlPack);
            var anySkill = SkillID(ref ctrlPack);
        }

        private static bool SkillID(ref ControllingPack ctrlPack)
        {
            var anySkill = false;

            for (var i = 0; i < StaticName.INPUT_BUTTON_NAME_SKILLS.Length; i++)
            {
                if (Input.GetButtonDown(StaticName.INPUT_BUTTON_NAME_SKILLS[i]))
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
                if (Input.GetButtonDown(StaticName.INPUT_BUTTON_NAME_SHOPBUYS[i]))
                {
                    anyBuy = true;
                    ctrlPack.ShopID = i;
                    break;
                }
            }

            if (anyBuy) ctrlPack.SetFlag(ControllingCommand.Buy);
            return anyBuy;
        }
    }

    #endregion
    
    //要把Asset和Logic彻底拆开。
    /// <summary>
    /// 世界本身的运行逻辑、应该类比于物理世界，高程度独立。
    /// </summary>
    internal static class WorldLogic //WORLD-LOGIC
    {
        //对，这种需要影响场景怎么办？
        //本来是为了保证WRD-LOGIC的独立性（体现形而上学的概念）；
        //就是弄成了静态类，但是现在看估计得弄成单例？
        private static Vector2Int ClampPosInBoard(Vector2Int pos, Board gameBoard)
        {
            var newPos = pos;
            newPos.x = Mathf.Clamp(newPos.x, 0, Board.BoardLength - 1);
            newPos.y = Mathf.Clamp(newPos.y, 0, Board.BoardLength - 1);
            return newPos;
        }

        private static bool UpdateShopBuy(GameAssets currentLevelAsset, in ControllingPack ctrlPack)
        {
            //先简单一些，只允许随机购买。
            if (ctrlPack.HasFlag(ControllingCommand.Buy) && currentLevelAsset.Shop.ShopOpening)
            {
                return currentLevelAsset.Shop.BuyToRandom(ctrlPack.ShopID);
            }

            return false;
        }

        private static void UpdateShopBuy(
            GameAssets currentLevelAsset, ShopBase shopMgr,
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

        internal static GameObject CreateIndicator(GameAssets currentLevelAsset, Vector2Int pos, Color col)
        {
            GameObject indicator =
                currentLevelAsset.Owner.WorldLogicRequestInstantiate(currentLevelAsset.CursorTemplate);
            Cursor indicatorCursor = indicator.GetComponent<Cursor>();
            indicatorCursor.SetIndMesh();
            indicatorCursor.InitPosWithAnimation(pos);
            UpdateCursorPos(currentLevelAsset);
            indicatorCursor.UpdateTransform(
                currentLevelAsset.GameBoard.GetFloatTransform(indicatorCursor.CurrentBoardPosition));
            indicatorCursor.CursorColor = col;
            return indicator;
        }

        internal static void CleanDestoryer(GameAssets currentLevelAsset)
        {
            if (currentLevelAsset.WarningGo != null)
            {
                if (currentLevelAsset.WarningGo.Length > 0)
                {
                    foreach (var go in currentLevelAsset.WarningGo)
                    {
                        currentLevelAsset.Owner.WorldLogicRequestDestroy(go);
                        currentLevelAsset.WarningGo = null;
                    }
                }
            }
        }

        internal static void UpdateDestoryer(GameAssets currentLevelAsset)
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
                    currentLevelAsset.WarningGo[i] = CreateIndicator(currentLevelAsset, incomings[i], col);
                }
            }
        }

        internal static void UpdateCursorPos(GameAssets currentLevelAsset)
        {
            currentLevelAsset.Cursor.SetPosWithAnimation(ClampPosInBoard(currentLevelAsset.Cursor.CurrentBoardPosition, currentLevelAsset.GameBoard), PosSetFlag.Current);
            currentLevelAsset.Cursor.SetPosWithAnimation(ClampPosInBoard(currentLevelAsset.Cursor.NextBoardPosition, currentLevelAsset.GameBoard), PosSetFlag.Next);
        }

        internal static void UpdateRotate(GameAssets currentLevelAsset, in ControllingPack ctrlPack)
        {
            if (ctrlPack.HasFlag(ControllingCommand.Rotate))
            {
                if (currentLevelAsset.GameBoard.CheckBoardPosValidAndFilled(ctrlPack.CurrentPos))
                {
                    var unit = currentLevelAsset.GameBoard.FindUnitUnderBoardPos(ctrlPack.CurrentPos);
                    System.Diagnostics.Debug.Assert(unit != null, nameof(unit) + " != null");
                    unit.GetComponentInChildren<Unit>().UnitRotateCw();
                    currentLevelAsset.GameBoard.UpdateBoard();
                }
            }
        }

        internal static void UpdateCursor_Unit(GameAssets currentLevelAsset, in ControllingPack ctrlPack,
            out bool movedTile, out bool movedCursor)
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
                    currentLevelAsset.Cursor.Move(ctrlPack.CommandDir);
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
        }

        internal static ControllingPack UpdateInputScheme(
            GameAssets currentLevelAsset, out bool movedTile,
            out bool movedCursor, ref bool boughtOnce)
        {
            movedTile = false;
            movedCursor = false;

            var ctrlPack = new ControllingPack {CtrlCMD = ControllingCommand.Nop};
            if (StartGameMgr.UseTouchScreen)
            {
                WorldController.GetCommand_Touch(currentLevelAsset, out ctrlPack);
            }
            else
            {
                if (currentLevelAsset.CursorEnabled)
                {
                    WorldController.GetCommand_KM(currentLevelAsset, out ctrlPack);
                }

                if (Input.GetButtonDown(StaticName.INPUT_BUTTON_NAME_NEXT))
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

        //TEMP 每次修改这两个值的时候才应该改一次。
        private static int lastInCome = -1;

        private static int lastCost = -1;

        //TEMP 每一步应该才计算一次，帧间时都这么临时存着。
        private static int occupiedHeatSink;

        internal static void UpdateUICurrencyVal(GameAssets currentLevelAsset)
        {
            if (currentLevelAsset.CostChart != null)
            {
                currentLevelAsset.CostChart.CurrencyVal = Mathf.RoundToInt(currentLevelAsset.GameStateMgr.GetCurrency());
            }
        }

        /// <summary>
        /// 这个函数主要是将目前场面上的数据反映到UI和玩家的视野中，这个很可能是不能单纯用flow的框架来搞？
        /// 这个真的不能瞎调，每调用一次就会让场上单位调用一次。
        /// </summary>
        /// <param name="currentLevelAsset"></param>
        internal static void UpdateBoardData(GameAssets currentLevelAsset)
        {
            //RISK 其实这里调用到的currentLevelAsset里面的数据修改后，这个函数其实都得重新调…………
            //也有解决方案，其实就是对其使用的数据进行变化监听……不知道靠谱不靠谱。
            int inCome = 0, cost = 0;

            var tmpInComeA = Mathf.FloorToInt(currentLevelAsset.BoardDataCollector.CalculateProcessorScore(out int A));
            var tmpInComeB = Mathf.FloorToInt(currentLevelAsset.BoardDataCollector.CalculateServerScore(out int B));

            if (currentLevelAsset.CurrencyIOEnabled)
            {
                inCome += tmpInComeA;
                inCome += tmpInComeB;
                inCome = Mathf.RoundToInt(inCome * currentLevelAsset.CurrencyRebate);
                //cost = Mathf.FloorToInt(currentLevelAsset.BoardDataCollector.CalculateCost());
                //TEMP 现在只有热力消耗。
                if (!currentLevelAsset.CurrencyIncomeEnabled)
                {
                    //RISK 现在在红色区间没有任何价格收入。靠谱吗？
                    inCome = 0;
                }

                cost = ShopMgr.HeatSinkCost(occupiedHeatSink, currentLevelAsset.GameBoard.MinHeatSinkCount);
            }

            currentLevelAsset.CostChart.Active = currentLevelAsset.CurrencyIOEnabled;
            currentLevelAsset.DeltaCurrency = inCome - cost;

            if (currentLevelAsset.CostChart != null)
            {
                currentLevelAsset.CostChart.CurrencyVal = Mathf.RoundToInt(currentLevelAsset.GameStateMgr.GetCurrency());
                currentLevelAsset.CostChart.IncomesVal = Mathf.RoundToInt(currentLevelAsset.DeltaCurrency);
            }
        }

        internal static void UpdateReverseCycle(GameAssets currentLevelAsset)
        {
            //TODO 从实施上还得想想时间反演的时候很多别的机制怎么办……
            //而且还有一个问题，这个作为一个反抗正反馈的机制（负反馈机制）（越穷越没时间、越没时间越穷）
            //如果还需要花钱，那么效率可能不高；但是这个机制如果没有门槛，那么就会被滥用。
            //这种负反馈的机制最好参考马车，但是马车里面有个很方便的量化“负状态”的参量——排名。
            //目前这个系统也需要一个这样的参量，有几个候选：钱数、离红色区段太近等等。
            WorldCycler.StepDown();
            currentLevelAsset.TimeLine.Reverse();
        }

        internal static void UpdateCycle(GameAssets currentLevelAsset, StageType type)
        {
            WorldCycler.StepUp();
            currentLevelAsset.TimeLine.Step();
            currentLevelAsset.BoughtOnce = false;

            if (currentLevelAsset.DestroyerEnabled) UpdateDestoryer(currentLevelAsset);
            //目前整个游戏的流程框架太过简单了，现在只有流程调用和flag。可能UpdateBoardData这个需要类似基于事件和触发的事件更新(?)
            if (currentLevelAsset.CurrencyEnabled) UpdateBoardData(currentLevelAsset);

            //RISK DeltaCurrency在UpdateBoardData里面才弄完。
            currentLevelAsset.GameStateMgr.PerMove(currentLevelAsset.DeltaCurrency);

            if (currentLevelAsset.ShopEnabled && (currentLevelAsset.Shop is IAnimatableShop shop))
                shop.ShopPreAnimationUpdate();

            if (currentLevelAsset.WarningDestoryer != null && currentLevelAsset.DestroyerEnabled)
                currentLevelAsset.WarningDestoryer.Step(out currentLevelAsset.DestoryedCoreType);
        }

        private static bool ShouldCycle(in ControllingPack ctrlPack, in bool pressedAny, in bool movedTile,
            in bool movedCursor)
        {
            var shouldCycle = false;
            var hasCycleNext = ctrlPack.HasFlag(ControllingCommand.CycleNext);
            if (StartGameMgr.UseTouchScreen)
            {
                shouldCycle = movedTile | hasCycleNext;
            }
            else
            {
                shouldCycle = (pressedAny & (movedTile | movedCursor)) | hasCycleNext;
            }

            return shouldCycle;
        }

        public static void UpdateLogic(GameAssets currentLevelAsset, in StageType type, out ControllingPack ctrlPack,
            out bool movedTile, out bool movedCursor, out bool shouldCycle,out bool? autoDrive)
        {
            currentLevelAsset.DeltaCurrency = 0.0f;
            movedTile = movedCursor = false;
            ctrlPack = new ControllingPack {CtrlCMD = ControllingCommand.Nop};

            #region UpKeep

            //这个Section认为是每帧都调的东西。
            CleanDestoryer(currentLevelAsset);
            //RISK 为了和商店同步，这里就先这样，但是可以检测只有购买后那一次才查一次。
            //总之稳了后，这个不能这么每帧调用。
            occupiedHeatSink = currentLevelAsset.GameBoard.CheckHeatSink(type);
            currentLevelAsset.SkillMgr.UpKeepSkill(currentLevelAsset);

            #endregion

            autoDrive = WorldCycler.NeedAutoDriveStep;

            //不一定必然是相反的，有可能是双false。
            var forwardCycle = false;
            var reverseCycle = false;

            if (!autoDrive.HasValue)
            {
                #region UserIO

                ctrlPack = UpdateInputScheme(currentLevelAsset,
                    out movedTile, out movedCursor,
                    ref currentLevelAsset._boughtOnce);

                if (currentLevelAsset.InputEnabled)
                {
                    UpdateCursor_Unit(currentLevelAsset, in ctrlPack, out movedTile, out movedCursor);
                    UpdateRotate(currentLevelAsset, in ctrlPack);
                    currentLevelAsset.GameBoard.UpdateBoardRotate(); //TODO 旋转现在还是闪现的。这个不用着急做。

                    if (currentLevelAsset.ShopEnabled)
                    {
                        //RISK 这里让购买单元也变成强制移动一步。
                        movedTile |= UpdateShopBuy(currentLevelAsset, ctrlPack);
                    }

                    movedTile |= ctrlPack.HasFlag(ControllingCommand.CycleNext);//这个flag的实际含义和名称有冲突。

                    currentLevelAsset.SkillMgr.SkillEnabled = currentLevelAsset.SkillEnabled;
                    //BUG !!!重大Bug，自动演进的时候不会计Mission的数字。
                    currentLevelAsset.SkillMgr.TriggerSkill(currentLevelAsset, ctrlPack);
                }

                forwardCycle = movedTile;

                #endregion
            }
            else
            {
                forwardCycle = autoDrive.Value;
                reverseCycle = !autoDrive.Value;
            }


            if (currentLevelAsset.SkillMgr.CurrentSkillType.HasValue && currentLevelAsset.SkillMgr.CurrentSkillType.Value == SkillType.Swap)
            {
                currentLevelAsset.SkillMgr.SwapTick(currentLevelAsset, ctrlPack);
                movedTile = false;
            }
            else
            {
                if (forwardCycle)
                {
                    #region FORWARD

                    UpdateCycle(currentLevelAsset, type);

                    #endregion
                }
                else if (reverseCycle)
                {
                    #region REVERSE

                    UpdateReverseCycle(currentLevelAsset);

                    #endregion
                }
            }

            #region CLEANUP

            shouldCycle = autoDrive.HasValue || ShouldCycle(in ctrlPack, Input.anyKeyDown, in movedTile, in movedCursor);

            #endregion
        }
    }
}