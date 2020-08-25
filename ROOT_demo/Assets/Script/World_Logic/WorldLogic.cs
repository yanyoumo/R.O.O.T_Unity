using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommandDir = ROOT.RotationDirection;
// ReSharper disable PossiblyImpureMethodCallOnReadonlyVariable

namespace ROOT
{
    [Flags]
    public enum ControllingCommand
    {
        Nop = 0b_00000000000,
        Move = 0b_00000000001,
        Drag = 0b_00000000010,
        Rotate = 0b_00000000100,
        Buy = 0b_00000001000,
        PlayHint = 0b_00000010000,
        SignalHint = 0b_00000100000,
        NextButton = 0b_00001000000,
        CycleNext = 0b_00010000000,
        BuyCanceled = 0b_00100000000,
        BuyConfirm = 0b_01000000000,
        BuyRandom = 0b_10000000000,
    }

    public struct ControllingPack
    {
        public ControllingCommand CtrlCMD;
        public CommandDir CommandDir;
        public Vector2Int CurrentPos;
        public Vector2Int NextPos;
        public int ShopID;

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
        private static float _moveValThreadhold = 0.1f;
        private static bool GetCommandDir(out CommandDir dir)
        {
            bool anyDir = false;
            dir = CommandDir.North;

            if (Input.GetButtonDown(StaticName.INPUT_BUTTON_NAME_CURSORUP))
            {
                dir = CommandDir.North;
                anyDir = true;
            }

            if (Input.GetButtonDown(StaticName.INPUT_BUTTON_NAME_CURSORDOWN))
            {
                dir = CommandDir.South;
                anyDir = true;
            }

            if (Input.GetButtonDown(StaticName.INPUT_BUTTON_NAME_CURSORLEFT))
            {
                dir = CommandDir.West;
                anyDir = true;
            }

            if (Input.GetButtonDown(StaticName.INPUT_BUTTON_NAME_CURSORRIGHT))
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

        //TODO Digong Default的工作，熟悉这个函数的代码，未来工作会围绕触摸屏的适配展开。
        internal static void GetCommand_Touch(GameAssets currentLevelAsset, out ControllingPack ctrlPack)
        {
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
                        //f^ck，在DoubleTap前会出现一次SingleTap
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
            if (WorldController.GetCommandDir(out ctrlPack.CommandDir))
            {
                ctrlPack.ReplaceFlag(ControllingCommand.Move); //Replace
                if (Input.GetButton(StaticName.INPUT_BUTTON_NAME_MOVEUNIT))
                {
                    ctrlPack.ReplaceFlag(ControllingCommand.Drag);  //Replace
                }
            }

            ctrlPack.CurrentPos = currentLevelAsset.Cursor.CurrentBoardPosition;
            ctrlPack.NextPos = currentLevelAsset.Cursor.GetCoord(ctrlPack.CommandDir);

            if (Input.GetButtonDown(StaticName.INPUT_BUTTON_NAME_ROTATEUNIT)&& ctrlPack.CtrlCMD == ControllingCommand.Nop)
            {
                //移动和拖动的优先级比旋转高。
                ctrlPack.CurrentPos = currentLevelAsset.Cursor.CurrentBoardPosition;
                ctrlPack.SetFlag(ControllingCommand.Rotate);
            }

            if (Input.GetButton(StaticName.INPUT_BUTTON_NAME_HINTHDD)|| Input.GetButton(StaticName.INPUT_BUTTON_NAME_HINTNET))
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

            if (currentLevelAsset.BuyingCursor)
            {
                if (Input.GetButtonDown(StaticName.INPUT_BUTTON_NAME_SHOPCANCELED))
                {
                    ctrlPack.SetFlag(ControllingCommand.BuyCanceled);
                }

                if (WorldController.GetCommandDir(out ctrlPack.CommandDir))
                {
                    Unit[] HQUnits = currentLevelAsset.GameBoard.FindUnitWithCoreType(CoreType.HQ);
                    if (HQUnits.Length > 0)
                    {
                        ctrlPack.NextPos = HQUnits[0].GetCoord(ctrlPack.CommandDir);
                        ctrlPack.SetFlag(ControllingCommand.BuyConfirm);
                    }
                }

                if (Input.GetButtonDown(StaticName.INPUT_BUTTON_NAME_SHOPRANDOM))
                {
                    ctrlPack.SetFlag(ControllingCommand.BuyRandom);
                }
            }
            
            bool anyBuy = false;
            if (Input.GetButtonDown(StaticName.INPUT_BUTTON_NAME_SHOPBUY1))
            {
                anyBuy = true;
                ctrlPack.ShopID = 0;
            }
            else if (Input.GetButtonDown(StaticName.INPUT_BUTTON_NAME_SHOPBUY2))
            {
                anyBuy = true;
                ctrlPack.ShopID = 1;
            }
            else if (Input.GetButtonDown(StaticName.INPUT_BUTTON_NAME_SHOPBUY3))
            {
                anyBuy = true;
                ctrlPack.ShopID = 2;
            }
            else if (Input.GetButtonDown(StaticName.INPUT_BUTTON_NAME_SHOPBUY4))
            {
                anyBuy = true;
                ctrlPack.ShopID = 3;
            }
            if (anyBuy)
            {
                ctrlPack.SetFlag(ControllingCommand.Buy);
            }
        }
    }


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
            newPos.x = Mathf.Clamp(newPos.x, 0, gameBoard.BoardLength - 1);
            newPos.y = Mathf.Clamp(newPos.y, 0, gameBoard.BoardLength - 1);
            return newPos;
        }

        private static void UpdateShopBuy(GameAssets currentLevelAsset, ShopMgr shopMgr, in ControllingPack ctrlPack,
            ref bool boughtOnce)
        {
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
                        currentLevelAsset.BuyingCursor = shopMgr.RequestBuy(ctrlPack.ShopID);
                        currentLevelAsset.BuyingID = ctrlPack.ShopID;
                    }
                }
                else
                {
                    //然后进入送货地址选择的位置时候，这个时候玩家可以取消也可以选择随机
                    //如果随机的话，比送过价格要低一些。
                    //目前商店上的价格写的是随机送的价格，即要送货再加X元，随机送不加价。
                    if (ctrlPack.HasFlag(ControllingCommand.BuyCanceled))
                    {
                        shopMgr.ResetPendingBuy();
                        currentLevelAsset.BuyingCursor = false;
                        currentLevelAsset.BuyingID = -1;
                    }
                    else if (ctrlPack.HasFlag(ControllingCommand.BuyConfirm))
                    {
                        //试图本地购买。
                        if (currentLevelAsset.GameBoard.CheckBoardPosValidAndEmpty(ctrlPack.NextPos))
                        {
                            successBought = shopMgr.BuyToPos(currentLevelAsset.BuyingID, ctrlPack.NextPos);
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

        internal static GameObject CreateIndicator(GameAssets currentLevelAsset,Vector2Int pos,Color col)
        {
            GameObject indicator= currentLevelAsset.Owner.WorldLogicRequestInstantiate(currentLevelAsset.CursorTemplate);
            Cursor indicatorCursor = indicator.GetComponent<Cursor>();
            indicatorCursor.SetIndMesh();
            indicatorCursor.InitPosWithAnimation(pos);
            UpdateCursorPos(currentLevelAsset);
            indicatorCursor.UpdateTransform(currentLevelAsset.GameBoard.GetFloatTransform(indicatorCursor.CurrentBoardPosition));
            indicatorCursor.CursorColor = col;
            return indicator;
        }

        internal static void UpdateDestoryer(GameAssets currentLevelAsset)
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

            if (currentLevelAsset.WarningDestoryer.GetStatus != WarningDestoryerStatus.Dormant)
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
            currentLevelAsset.Cursor.SetPosWithAnimation(
                ClampPosInBoard(currentLevelAsset.Cursor.CurrentBoardPosition, currentLevelAsset.GameBoard),
                PosSetFlag.Current);
            currentLevelAsset.Cursor.SetPosWithAnimation(
                ClampPosInBoard(currentLevelAsset.Cursor.NextBoardPosition, currentLevelAsset.GameBoard),
                PosSetFlag.Next);
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

            if (currentLevelAsset.CursorEnabled && movedCursor)
            {
                currentLevelAsset.AnimationPendingObj.Add(currentLevelAsset.Cursor);
                UpdateCursorPos(currentLevelAsset);
                movedCursor = true;
            }
        }

        internal static ControllingPack UpdateInputScheme(GameAssets currentLevelAsset, out bool movedTile,
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
                                  | ControllingCommand.BuyCanceled
                                  | ControllingCommand.BuyConfirm);
            }

            return ctrlPack;
        }

        internal static void UpdateBoardData(GameAssets currentLevelAsset)
        {
            currentLevelAsset.DeltaCurrency = 0.0f;
            currentLevelAsset.DeltaCurrency += currentLevelAsset.BoardDataCollector.CalculateProcessorScore(out int A);
            currentLevelAsset.DeltaCurrency += currentLevelAsset.BoardDataCollector.CalculateServerScore(out int B);
            currentLevelAsset.DeltaCurrency -= currentLevelAsset.BoardDataCollector.CalculateCost();

            if (currentLevelAsset.LCDCurrencyEnabled)
            {
                currentLevelAsset.DataScreen.SetLcd(currentLevelAsset.GameStateMgr.GetCurrency(), RowEnum.CurrentMoney);
                currentLevelAsset.DataScreen.SetAlertLevel(currentLevelAsset.GameStateMgr.GetCurrencyRatio(), RowEnum.CurrentMoney);
            }

            if (currentLevelAsset.LCDDeltaCurrencyEnabled)
            {
                currentLevelAsset.DataScreen.SetLcd(currentLevelAsset.DeltaCurrency, RowEnum.DeltaMoney);
            }
        }

        internal static void UpdateCycle(GameAssets currentLevelAsset, bool shouldCycle = true)
        {
            if (currentLevelAsset.LCDTimeEnabled)
            {
                currentLevelAsset.DataScreen.SetLcd(currentLevelAsset.GameStateMgr.GetGameTime(), RowEnum.Time);
                currentLevelAsset.DataScreen.SetAlertLevel(currentLevelAsset.GameStateMgr.GetTimeRatio(), RowEnum.Time);
            }

            if (shouldCycle)
            {
                //HACK Timeline的StepCount已经整合到这里，可以在一些LevelLogic里面统一 重置了。
                //因为玩家有可能在开始按动ctrl，于是在相关教程中，在开启时间轴的时候StepCount会清零。
                currentLevelAsset.StepCount++;
                currentLevelAsset.TimeLine.Step();
                currentLevelAsset.GameStateMgr.PerMove(currentLevelAsset.DeltaCurrency);
                if (currentLevelAsset.BoughtOnce)
                {
                    currentLevelAsset.BoughtOnce = false;
                }

                if (currentLevelAsset.ShopEnabled)
                {
                    currentLevelAsset.ShopMgr.ShopPreAnimationUpdate();
                }

                if (currentLevelAsset.WarningDestoryer != null && currentLevelAsset.DestroyerEnabled)
                {
                    currentLevelAsset.WarningDestoryer.Step(out currentLevelAsset.DestoryedCoreType);
                }
            }
        }

        public static void UpdateLogic(GameAssets currentLevelAsset, out ControllingPack ctrlPack, out bool movedTile, out bool movedCursor)
        {
            //其实这个流程问题不是特别大、主要是各种flag要整理
            currentLevelAsset.DeltaCurrency = 0.0f;
            movedTile = false;
            movedCursor = false;
            ctrlPack = new ControllingPack {CtrlCMD = ControllingCommand.Nop};

            if (currentLevelAsset.DestroyerEnabled) UpdateDestoryer(currentLevelAsset);

            ctrlPack = UpdateInputScheme(currentLevelAsset, out movedTile, out movedCursor, ref currentLevelAsset._boughtOnce);

            if (currentLevelAsset.InputEnabled)
            {
                if (currentLevelAsset.ShopEnabled)
                {
                    UpdateShopBuy(currentLevelAsset, currentLevelAsset.ShopMgr, in ctrlPack, ref currentLevelAsset._boughtOnce);
                }

                UpdateCursor_Unit(currentLevelAsset, in ctrlPack, out movedTile, out movedCursor);

                if (currentLevelAsset.RotateEnabled) UpdateRotate(currentLevelAsset, in ctrlPack);

                currentLevelAsset.GameBoard.UpdateBoardRotate(); //TODO 旋转现在还是闪现的。这个不用着急做。
            }

            movedTile |= ctrlPack.HasFlag(ControllingCommand.CycleNext);
            currentLevelAsset.HintMaster.ShouldShowShopHint = currentLevelAsset.Cursor.Targeting = currentLevelAsset.BuyingCursor;

            if (currentLevelAsset.CurrencyEnabled) UpdateBoardData(currentLevelAsset);
            if (currentLevelAsset.CycleEnabled) UpdateCycle(currentLevelAsset, movedTile);
        }
    }
}