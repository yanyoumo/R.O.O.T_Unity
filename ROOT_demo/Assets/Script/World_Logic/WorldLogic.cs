using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YamlDotNet.Core.Tokens;
using CommandDir = ROOT.RotationDirection;

namespace ROOT
{
    [Flags]
    internal enum ControllingCommand
    {
        Nop = 0b_00000000,
        Move = 0b_00000001,
        Drag = 0b_00000010,
        Rotate = 0b_00000100,
        Buy = 0b_00001000,
        PlayHint = 0b_00010000,
        SignalHint = 0b_00100000,
        NextButton = 0b_01000000,
    }

    internal struct ControllingPack
    {
        public ControllingCommand CtrlCMD;
        public CommandDir CommandDir;
        public Vector2Int CurrentPos;
        public Vector2Int NextPos;
        public int ShopID;
    }


    //要把Asset和Logic，把Controller也要彻底拆开。
    internal static class WorldController
    {
        private static float _moveValThreadhold = 0.1f;
        /*//这里不处理命令的合法性。是更接近硬件的版本
        //hmmmmmmm但是是Drag与否还要读取Board……//这里对于board应该是从概念上只读的
        internal static ControllingCommand UpdateCommand(GameAssets currentLevelAsset,out Vector2Int CommandLoc)
        {
            ControllingCommand command = 0b_0000000000000000;
            CommandLoc = Vector2Int.zero;
            if (StartGameMgr.DetectedInputScheme == InputScheme.TouchScreen)
            {

            }
            else
            {

            }

            return command;
        }*/
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

        /// <summary>
        /// 检测时候触碰了一个Unit.
        /// </summary>
        /// <param name="touch">输入Touch结构体</param>
        /// <returns>得到的Unit，如果没有则返回null</returns>
        private static Unit GetTouchedOnUnit(in Touch touch)
        {
            Ray ray = Camera.main.ScreenPointToRay(touch.position);
            if (Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                if (hitInfo.collider != null && hitInfo.collider.gameObject.CompareTag("Unit"))
                {
                    return hitInfo.transform.GetComponentInChildren<Unit>();
                }
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

        internal static void GetCommand_Touch(GameAssets currentLevelAsset, out ControllingPack ctrlPack)
        {
            //先特么只考虑一个手指的情况。
            ctrlPack = new ControllingPack {CtrlCMD = ControllingCommand.Nop};
            if (Input.touchCount > 0)
            {
                Touch touch = Input.touches[0];
                //不允许在滑动的同时还有一次别的手指点击的可能。
                Unit tmpTouchingUnit = GetTouchedOnUnit(in touch); //这个在滑动过程中不要了。
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
                            ctrlPack.CtrlCMD |= ControllingCommand.Rotate;
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
                                    ctrlPack.CtrlCMD |= ControllingCommand.Buy;
                                    ctrlPack.ShopID = tmpTouchingUnit.ShopID;
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
                                    ctrlPack.CtrlCMD = ControllingCommand.Drag;
                                    //Debug.Log("Completed Once");
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

        internal static void GetCommand_KM(GameAssets currentLevelAsset, out ControllingPack ctrlPack)
        {
            ctrlPack = new ControllingPack {CtrlCMD = ControllingCommand.Nop};
            if (WorldController.GetCommandDir(out ctrlPack.CommandDir))
            {
                ctrlPack.CtrlCMD = ControllingCommand.Move; //Replace
                if (Input.GetButton(StaticName.INPUT_BUTTON_NAME_MOVEUNIT))
                {
                    ctrlPack.CtrlCMD = ControllingCommand.Drag; //Replace
                }
            }

            ctrlPack.CurrentPos = currentLevelAsset.Cursor.CurrentBoardPosition;
            ctrlPack.NextPos = currentLevelAsset.Cursor.GetCoord(ctrlPack.CommandDir);

            if (Input.GetButtonDown(StaticName.INPUT_BUTTON_NAME_ROTATEUNIT)&& ctrlPack.CtrlCMD == ControllingCommand.Nop)
            {
                //移动和拖动的优先级比旋转高。
                ctrlPack.CurrentPos = currentLevelAsset.Cursor.CurrentBoardPosition;
                ctrlPack.CtrlCMD |= ControllingCommand.Rotate;
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
                ctrlPack.CtrlCMD |= ControllingCommand.Buy;
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
            Vector2Int newPos = pos;
            newPos.x = Mathf.Clamp(newPos.x, 0, gameBoard.BoardLength - 1);
            newPos.y = Mathf.Clamp(newPos.y, 0, gameBoard.BoardLength - 1);
            return newPos;
        }

        private static void UpdateShopBuy(ShopMgr shopMgr,in ControllingPack ctrlPack, ref bool boughtOnce)
        {
            if (!boughtOnce)
            {
                bool successBought = false;

                if (HasFlag(ctrlPack.CtrlCMD, ControllingCommand.Buy))
                {
                    successBought = shopMgr.Buy(ctrlPack.ShopID);
                }

                if (successBought)
                {
                    boughtOnce = true;
                }
            }
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

            if (currentLevelAsset.WarningDestoryer.GetStatus() != WarningDestoryerStatus.Dormant)
            {
                Vector2Int[] incomings = currentLevelAsset.WarningDestoryer.NextStrikingPos(out int count);
                currentLevelAsset.WarningGo = new GameObject[count];
                for (int i = 0; i < count; i++)
                {
                    currentLevelAsset.WarningGo[i] =
                        currentLevelAsset.Owner.WorldLogicRequestInstantiate(currentLevelAsset.CursorTemplate);
                    var mIndCursor = currentLevelAsset.WarningGo[i].GetComponent<Cursor>();
                    mIndCursor.SetIndMesh();
                    mIndCursor.InitPosWithAnimation(incomings[i]);
                    UpdateCursorPos(currentLevelAsset);
                    mIndCursor.UpdateTransform(currentLevelAsset.GameBoard.GetFloatTransform(mIndCursor.CurrentBoardPosition));

                    Material tm = currentLevelAsset.WarningGo[i].GetComponentInChildren<MeshRenderer>().material;

                    if (currentLevelAsset.WarningDestoryer.GetStatus() == WarningDestoryerStatus.Warning)
                    {
                        tm.SetColor("_Color", Color.yellow);
                    }
                    else if (currentLevelAsset.WarningDestoryer.GetStatus() == WarningDestoryerStatus.Striking)
                    {
                        tm.SetColor("_Color", new Color(1.0f, 0.2f, 0.0f));
                    }
                    else
                    {
                        Debug.Assert(false, "Internal Error");
                    }
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
            if (HasFlag(ctrlPack.CtrlCMD,ControllingCommand.Rotate))
            {
                if (currentLevelAsset.GameBoard.CheckBoardPosValidAndFilled(ctrlPack.CurrentPos))
                {
                    GameObject unit = currentLevelAsset.GameBoard.FindUnitUnderBoardPos(ctrlPack.CurrentPos);
                    System.Diagnostics.Debug.Assert(unit != null, nameof(unit) + " != null");
                    unit.GetComponentInChildren<Unit>().UnitRotateCw();
                    currentLevelAsset.GameBoard.UpdateBoard();
                }
            }
        }

        internal static bool HasFlag(ControllingCommand A, ControllingCommand B)
        {
            return (A & B) == B;
        }

        internal static void UpdateCursor_Unit(GameAssets currentLevelAsset,in ControllingPack ctrlPack, out bool movedTile, out bool movedCursor)
        {
            movedTile = false;
            movedCursor = false;
            Unit movingUnit = null;

            bool validAction = currentLevelAsset.GameBoard.CheckBoardPosValidAndFilled(ctrlPack.CurrentPos) &&
                               currentLevelAsset.GameBoard.CheckBoardPosValidAndEmpty(ctrlPack.NextPos);

            ControllingCommand extractedCommand = ctrlPack.CtrlCMD & (ControllingCommand.Drag | ControllingCommand.Move);

            if (HasFlag(extractedCommand, ControllingCommand.Drag) && validAction)
            {
                GameObject unit = currentLevelAsset.GameBoard.FindUnitUnderBoardPos(ctrlPack.CurrentPos);
                System.Diagnostics.Debug.Assert(unit != null, nameof(unit) + " != null");
                movingUnit = unit.GetComponentInChildren<Unit>();
                movingUnit.Move(ctrlPack.CommandDir);
                currentLevelAsset.GameBoard.UpdateUnitBoardPosAnimation(ctrlPack.CurrentPos);
                currentLevelAsset.Cursor.Move(ctrlPack.CommandDir);
                movedTile = true;
            }
            else if (HasFlag(extractedCommand, ControllingCommand.Move))
            {
                currentLevelAsset.Cursor.Move(ctrlPack.CommandDir);
                movedCursor = true;
            }


            if (movingUnit)
            {
                currentLevelAsset.AnimationPendingObj.Add(movingUnit);
            }

            movedCursor |= movedTile;
            if (movedCursor)
            {
                currentLevelAsset.AnimationPendingObj.Add(currentLevelAsset.Cursor);
            }

            UpdateCursorPos(currentLevelAsset);
        }

        internal static void UpdateInput(GameAssets currentLevelAsset, out bool movedTile, out bool movedCursor, ref bool boughtOnce)
        {
            movedTile = false;
            movedCursor = false;

            ControllingPack ctrlPack = new ControllingPack {CtrlCMD = ControllingCommand.Nop};
            if (StartGameMgr.DetectedInputScheme == InputScheme.TouchScreen)
            {
                WorldController.GetCommand_Touch(currentLevelAsset, out ctrlPack);
            }
            else
            {
                if (currentLevelAsset.CursorEnabled)
                {
                    WorldController.GetCommand_KM(currentLevelAsset, out ctrlPack);
                }
            }

            if (currentLevelAsset.ShopEnabled)
            {
                UpdateShopBuy(currentLevelAsset.ShopMgr,in ctrlPack, ref boughtOnce);
            }

            UpdateCursor_Unit(currentLevelAsset, in ctrlPack, out movedTile, out movedCursor);

            if (currentLevelAsset.RotateEnabled)
            {
                //旋转的动画先没有吧。
                UpdateRotate(currentLevelAsset, in ctrlPack);
            }
        }

        internal static void UpdateCurrency(GameAssets currentLevelAsset)
        {
            currentLevelAsset.DeltaCurrency = 0.0f;
            currentLevelAsset.DeltaCurrency += currentLevelAsset.CurrencyIoCalculator.CalculateProcessorScore();
            currentLevelAsset.DeltaCurrency += currentLevelAsset.CurrencyIoCalculator.CalculateServerScore();
            currentLevelAsset.DeltaCurrency -= currentLevelAsset.CurrencyIoCalculator.CalculateCost();

            if (currentLevelAsset.LCDCurrencyEnabled)
            {
                currentLevelAsset.DataScreen.SetLCD(currentLevelAsset.GameStateMgr.GetCurrency(), RowEnum.CurrentMoney);
                currentLevelAsset.DataScreen.SetAlertLevel(currentLevelAsset.GameStateMgr.GetCurrencyRatio(),
                    RowEnum.CurrentMoney);
                currentLevelAsset.DataScreen.SetLCD(currentLevelAsset.DeltaCurrency, RowEnum.DeltaMoney);
            }
        }

        internal static void UpdateCycle(GameAssets currentLevelAsset, bool movedTile = true)
        {
            if (currentLevelAsset.LCDTimeEnabled)
            {
                currentLevelAsset.DataScreen.SetLCD(currentLevelAsset.GameStateMgr.GetGameTime(), RowEnum.Time);
                currentLevelAsset.DataScreen.SetAlertLevel(currentLevelAsset.GameStateMgr.GetTimeRatio(), RowEnum.Time);
            }

            if (movedTile)
            {
                currentLevelAsset.GameStateMgr.PerMove(new ScoreSet(),
                    new PerMoveData(currentLevelAsset.DeltaCurrency, 1));
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
                    currentLevelAsset.WarningDestoryer.Step();
                }
            }
        }

        public static void UpdateLogic(GameAssets currentLevelAsset, out bool movedTile, out bool movedCursor)
        {
            currentLevelAsset.DeltaCurrency = 0.0f;
            movedTile = false;
            movedCursor = false;
            {
                if (currentLevelAsset.DestroyerEnabled)
                {
                    UpdateDestoryer(currentLevelAsset);
                }

                if (currentLevelAsset.InputEnabled)
                {
                    var cursor = currentLevelAsset.GameCursor.GetComponent<Cursor>();
                    UpdateInput(currentLevelAsset, out movedTile, out movedCursor, ref currentLevelAsset.BoughtOnce);
                    currentLevelAsset.GameBoard.UpdateBoardRotate(); //TODO 旋转现在还是闪现的。这个不用着急做。
                }

                if (currentLevelAsset.CurrencyEnabled)
                {
                    UpdateCurrency(currentLevelAsset);
                }

                if (currentLevelAsset.CycleEnabled)
                {
                    UpdateCycle(currentLevelAsset, movedTile);
                }
            }
        }
    }
}