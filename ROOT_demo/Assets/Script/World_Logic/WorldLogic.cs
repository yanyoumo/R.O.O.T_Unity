using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ROOT
{
    //要把Asset和Logic彻底拆开。
    /// <summary>
    /// 世界本身的运行逻辑、应该类比于物理世界，高程度独立。
    /// </summary>
    internal static class WorldLogic //WORLD-LOGIC
    {
        private static int lastFingerID=0;
        private static bool swiping=false;
        private static Vector2 moveVal =Vector2.zero;
        private static Unit swipingUnit = null;
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

        private static void UpdateShop(ShopMgr shopMgr, ref bool boughtOnce)
        {
            if (!boughtOnce)
            {
                bool successBought = false;
                if (Input.GetButtonDown(StaticName.INPUT_BUTTON_NAME_SHOPBUY1))
                {
                    successBought = shopMgr.Buy(0);
                }
                else if (Input.GetButtonDown(StaticName.INPUT_BUTTON_NAME_SHOPBUY2))
                {
                    successBought = shopMgr.Buy(1);
                }
                else if (Input.GetButtonDown(StaticName.INPUT_BUTTON_NAME_SHOPBUY3))
                {
                    successBought = shopMgr.Buy(2);
                }
                else if (Input.GetButtonDown(StaticName.INPUT_BUTTON_NAME_SHOPBUY4))
                {
                    successBought = shopMgr.Buy(3);
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
                    currentLevelAsset.WarningGo[i] =currentLevelAsset.Owner.WorldLogicRequestInstantiate(currentLevelAsset.CursorTemplate);
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

        internal static void UpdateRotate(GameAssets currentLevelAsset)
        {
            if (Input.GetButtonDown(StaticName.INPUT_BUTTON_NAME_ROTATEUNIT))
            {
                if (currentLevelAsset.GameBoard.CheckBoardPosValidAndFilled(currentLevelAsset.Cursor
                    .CurrentBoardPosition))
                {
                    GameObject unit =
                        currentLevelAsset.GameBoard.FindUnitUnderBoardPos(currentLevelAsset.Cursor
                            .CurrentBoardPosition);
                    if (unit)
                    {
                        unit.GetComponentInChildren<Unit>().UnitRotateCw();
                        currentLevelAsset.GameBoard.UpdateBoard();
                    }
                }
            }
        }

        internal static void UpdateCursor(GameAssets currentLevelAsset, out bool movedTile, out bool movedCursor)
        {
            movedTile = false;
            movedCursor = false;
            currentLevelAsset.AnimationPendingObj.Add(currentLevelAsset.Cursor);
            Unit movingUnit = null;
            if (Input.GetButton(StaticName.INPUT_BUTTON_NAME_MOVEUNIT) &&
                currentLevelAsset.GameBoard.CheckBoardPosValidAndFilled(currentLevelAsset.Cursor.CurrentBoardPosition))
            {
                if (Input.GetButtonDown(StaticName.INPUT_BUTTON_NAME_CURSORLEFT))
                {
                    if (currentLevelAsset.GameBoard.CheckBoardPosValidAndEmpty(currentLevelAsset.Cursor.GetWestCoord()))
                    {
                        GameObject unit =
                            currentLevelAsset.GameBoard.FindUnitUnderBoardPos(currentLevelAsset.Cursor
                                .CurrentBoardPosition);
                        if (unit)
                        {
                            Vector2Int oldKey = currentLevelAsset.Cursor.CurrentBoardPosition;
                            movingUnit = unit.GetComponentInChildren<Unit>();
                            movingUnit.MoveLeft();
                            currentLevelAsset.GameBoard.UpdateUnitBoardPosAnimation(oldKey);
                            movedTile = true;
                        }

                        currentLevelAsset.Cursor.MoveLeft();
                    }
                }

                if (Input.GetButtonDown(StaticName.INPUT_BUTTON_NAME_CURSORUP))
                {
                    if (currentLevelAsset.GameBoard.CheckBoardPosValidAndEmpty(currentLevelAsset.Cursor.GetNorthCoord())
                    )
                    {
                        GameObject unit =
                            currentLevelAsset.GameBoard.FindUnitUnderBoardPos(currentLevelAsset.Cursor
                                .CurrentBoardPosition);
                        if (unit)
                        {
                            Vector2Int oldKey = currentLevelAsset.Cursor.CurrentBoardPosition;
                            movingUnit = unit.GetComponentInChildren<Unit>();
                            movingUnit.MoveUp();
                            currentLevelAsset.GameBoard.UpdateUnitBoardPosAnimation(oldKey);
                            movedTile = true;
                        }

                        currentLevelAsset.Cursor.MoveUp();
                    }
                }

                if (Input.GetButtonDown(StaticName.INPUT_BUTTON_NAME_CURSORDOWN))
                {
                    if (currentLevelAsset.GameBoard.CheckBoardPosValidAndEmpty(currentLevelAsset.Cursor.GetSouthCoord())
                    )
                    {
                        GameObject unit =
                            currentLevelAsset.GameBoard.FindUnitUnderBoardPos(currentLevelAsset.Cursor
                                .CurrentBoardPosition);
                        if (unit)
                        {
                            Vector2Int oldKey = currentLevelAsset.Cursor.CurrentBoardPosition;
                            movingUnit = unit.GetComponentInChildren<Unit>();
                            movingUnit.MoveDown();
                            currentLevelAsset.GameBoard.UpdateUnitBoardPosAnimation(oldKey);
                            movedTile = true;
                        }

                        currentLevelAsset.Cursor.MoveDown();
                    }
                }

                if (Input.GetButtonDown(StaticName.INPUT_BUTTON_NAME_CURSORRIGHT))
                {
                    if (currentLevelAsset.GameBoard.CheckBoardPosValidAndEmpty(currentLevelAsset.Cursor.GetEastCoord()))
                    {
                        GameObject unit =
                            currentLevelAsset.GameBoard.FindUnitUnderBoardPos(currentLevelAsset.Cursor
                                .CurrentBoardPosition);
                        if (unit)
                        {
                            Vector2Int oldKey = currentLevelAsset.Cursor.CurrentBoardPosition;
                            movingUnit = unit.GetComponentInChildren<Unit>();
                            movingUnit.MoveRight();
                            currentLevelAsset.GameBoard.UpdateUnitBoardPosAnimation(oldKey);
                            movedTile = true;
                        }

                        currentLevelAsset.Cursor.MoveRight();
                    }
                }
            }
            else
            {
                if (currentLevelAsset.GameBoard.CheckBoardPosValid(currentLevelAsset.Cursor.CurrentBoardPosition))
                {
                    if (Input.GetButtonDown(StaticName.INPUT_BUTTON_NAME_CURSORLEFT))
                    {
                        movedCursor = true;
                        currentLevelAsset.Cursor.MoveLeft();
                    }

                    if (Input.GetButtonDown(StaticName.INPUT_BUTTON_NAME_CURSORRIGHT))
                    {
                        movedCursor = true;
                        currentLevelAsset.Cursor.MoveRight();
                    }

                    if (Input.GetButtonDown(StaticName.INPUT_BUTTON_NAME_CURSORUP))
                    {
                        movedCursor = true;
                        currentLevelAsset.Cursor.MoveUp();
                    }

                    if (Input.GetButtonDown(StaticName.INPUT_BUTTON_NAME_CURSORDOWN))
                    {
                        movedCursor = true;
                        currentLevelAsset.Cursor.MoveDown();
                    }
                }
            }

            if (movingUnit)
            {
                Debug.Assert(movingUnit);
                currentLevelAsset.AnimationPendingObj.Add(movingUnit);
            }

            movedCursor |= movedTile;
            UpdateCursorPos(currentLevelAsset);
        }

        private static void ApplyMove(GameAssets currentLevelAsset, Vector2 val,Unit unit)
        {
            //再转译成上下左右即可。
            //TODO 日，这里还得排除Shop里面那些Unit
            Vector2 pivot = Vector2.one.normalized;
            float angle=Vector2.SignedAngle(pivot, val.normalized);
            angle -= 90;

            Vector2Int oldKey = unit.CurrentBoardPosition;
            oldKey = new Vector2Int(oldKey.y, oldKey.x);

            if (angle<0&&angle>=-90)
            {
                //North
                unit.MoveUp();
                Debug.Log("North");
            }
            else if (angle < -90 && angle >=-180)
            {
                //WEST
                unit.MoveRight();
                Debug.Log("WEST");
            }
            else if (angle > 0 && angle <= 90)
            {
                //EAST
                unit.MoveLeft();
                Debug.Log("EAST");
            }
            else
            {
                //SOUTH
                unit.MoveDown();
                Debug.Log("SOUTH");
            }

            currentLevelAsset.GameBoard.UpdateUnitBoardPosAnimation_Touch(unit);
            currentLevelAsset.AnimationPendingObj.Add(unit);

            Debug.Log(val);
        }

        internal static void UpdateInput(GameAssets currentLevelAsset, out bool movedTile, out bool movedCursor,
            ref bool boughtOnce)
        {
            movedTile = false;
            movedCursor = false;
            if (currentLevelAsset.ShopEnabled)
            {
                UpdateShop(currentLevelAsset.ShopMgr, ref boughtOnce);
            }

            if (StartGameMgr.DetectedInputScheme == InputScheme.TouchScreen)
            {
                if (Input.touchCount > 0)
                {
                    foreach (var touch in Input.touches)
                    {
                        if (!swiping)
                        {
                            Ray ray = Camera.main.ScreenPointToRay(touch.position);
                            if (Physics.Raycast(ray, out RaycastHit hitInfo))
                            {
                                if (hitInfo.collider != null)
                                {
                                    if (hitInfo.transform.name == "UnitRoot")
                                    {
                                        swipingUnit = hitInfo.transform.GetComponentInChildren<Unit>();
                                        lastFingerID = touch.fingerId;
                                        Debug.Assert(touch.phase == TouchPhase.Began);
                                        swiping = true;
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (touch.fingerId == lastFingerID)
                            {
                                switch (touch.phase)
                                {
                                    case TouchPhase.Began:
                                    case TouchPhase.Stationary:
                                        //DO NOTHING
                                        break;
                                    case TouchPhase.Moved:
                                        moveVal += touch.deltaPosition;
                                        break;
                                    case TouchPhase.Ended:
                                        lastFingerID = touch.fingerId;
                                        moveVal += touch.deltaPosition;
                                        swiping = false;
                                        ApplyMove(currentLevelAsset, moveVal, swipingUnit);
                                        swipingUnit = null;
                                        moveVal = Vector2.zero;
                                        movedTile = true;
                                        break;
                                    case TouchPhase.Canceled:
                                        lastFingerID = touch.fingerId;
                                        swiping = false;
                                        swipingUnit = null;
                                        moveVal = Vector2.zero;
                                        break;
                                    default:
                                        throw new ArgumentOutOfRangeException();
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                if (currentLevelAsset.CursorEnabled)
                {
                    UpdateCursor(currentLevelAsset, out movedTile, out movedCursor);
                }
            }

            if (currentLevelAsset.RotateEnabled)
            {
                //旋转的动画先没有吧。
                UpdateRotate(currentLevelAsset);
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
                currentLevelAsset.DataScreen.SetAlertLevel(currentLevelAsset.GameStateMgr.GetCurrencyRatio(), RowEnum.CurrentMoney);
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
                currentLevelAsset.GameStateMgr.PerMove(new ScoreSet(),new PerMoveData(currentLevelAsset.DeltaCurrency, 1));
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
                    UpdateInput(currentLevelAsset, out movedTile, out movedCursor,
                        ref currentLevelAsset.BoughtOnce);
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