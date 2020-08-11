﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommandDir = ROOT.RotationDirection;

namespace ROOT
{
    public enum PosSetFlag
    {
        NONE,
        Current,
        Next,
        Lerping,
        NextAndLerping,
        CurrentAndLerping,
        CurrentAndNext,
        All,
    }

    public abstract class MoveableBase : MonoBehaviour//, IPlaceable
    {
        public MeshRenderer ImmovableRenderer;
        private bool _immovable = false;
        public bool Immovable
        {
            set
            {
                _immovable = value;
                ImmovableRenderer.enabled = _immovable;
            }
            get => _immovable;
        }
        public Vector2Int CurrentBoardPosition { get;protected set; }
        public Vector2Int NextBoardPosition { get; protected set; }
        public Vector2 LerpingBoardPosition { get; set; }
        public abstract void UpdateTransform(Vector3 pos);

        public int PosHash => CurrentBoardPosition.x * 10 + CurrentBoardPosition.y;

        public Vector2 LerpBoardPos(float lerp)
        {
            float x = Mathf.Lerp((CurrentBoardPosition.x), (NextBoardPosition.x),lerp);
            float y = Mathf.Lerp((CurrentBoardPosition.y), (NextBoardPosition.y),lerp);
            return new Vector2(x, y);
        }

        #region MoveToNeigbour

        public virtual void Move(CommandDir Dir)
        {
            switch (Dir)
            {
                case CommandDir.North:
                    MoveUp();
                    return;
                case CommandDir.East:
                    MoveRight();
                    return;
                case CommandDir.West:
                    MoveLeft();
                    return;
                case CommandDir.South:
                    MoveDown();
                    return;
                default:
                    throw new ArgumentOutOfRangeException(nameof(Dir), Dir, null);
            }
        }

        public virtual Vector2Int GetCoord(CommandDir Offset)
        {
            switch (Offset)
            {
                case CommandDir.North:
                    return GetNorthCoord();
                case CommandDir.East:
                    return GetEastCoord();
                case CommandDir.West:
                    return GetWestCoord();
                case CommandDir.South:
                    return GetSouthCoord();
                default:
                    throw new ArgumentOutOfRangeException(nameof(Offset), Offset, null);
            }
        }

        protected virtual void MoveLeft()
        {
            NextBoardPosition = new Vector2Int(CurrentBoardPosition.x - 1, CurrentBoardPosition.y);
        }

        protected virtual void MoveRight()
        {
            NextBoardPosition = new Vector2Int(CurrentBoardPosition.x + 1, CurrentBoardPosition.y);
        }

        protected virtual void MoveUp()
        {
            NextBoardPosition = new Vector2Int(CurrentBoardPosition.x, CurrentBoardPosition.y + 1);
        }

        protected virtual void MoveDown()
        {
            NextBoardPosition = new Vector2Int(CurrentBoardPosition.x, CurrentBoardPosition.y - 1);
        }

        private Vector2Int _convertVector2ToVector2Int(Vector2 pos)
        {
            return new Vector2Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y));
        }

        public void SetPosWithAnimation(Vector2 pos, PosSetFlag flag)
        {
            switch (flag)
            {
                case PosSetFlag.NONE:
                    return;
                case PosSetFlag.Current:
                    CurrentBoardPosition = _convertVector2ToVector2Int(pos);
                    return;
                case PosSetFlag.Next:
                    NextBoardPosition = _convertVector2ToVector2Int(pos);
                    return;
                case PosSetFlag.Lerping:
                    LerpingBoardPosition = pos;
                    return;
                case PosSetFlag.NextAndLerping:
                    NextBoardPosition = _convertVector2ToVector2Int(pos);
                    LerpingBoardPosition = pos;
                    return;
                case PosSetFlag.CurrentAndLerping:
                    CurrentBoardPosition = _convertVector2ToVector2Int(pos);
                    LerpingBoardPosition = pos;
                    return;
                case PosSetFlag.CurrentAndNext:
                    CurrentBoardPosition = _convertVector2ToVector2Int(pos);
                    NextBoardPosition = _convertVector2ToVector2Int(pos);
                    return;
                case PosSetFlag.All:
                    CurrentBoardPosition = _convertVector2ToVector2Int(pos);
                    NextBoardPosition = _convertVector2ToVector2Int(pos);
                    LerpingBoardPosition = pos;
                    return;
                default:
                    throw new ArgumentOutOfRangeException(nameof(flag), flag, null);
            }
        }

        public void InitPosWithAnimation(Vector2 pos)
        {
            SetPosWithAnimation(pos, PosSetFlag.All);
        }

        #endregion

        #region GetNeigbourCoord

        protected Vector2Int GetEastCoord()
        {
            return new Vector2Int(CurrentBoardPosition.x + 1, CurrentBoardPosition.y);
        }
        protected Vector2Int GetWestCoord()
        {
            return new Vector2Int(CurrentBoardPosition.x - 1, CurrentBoardPosition.y);
        }
        protected Vector2Int GetSouthCoord()
        {
            return new Vector2Int(CurrentBoardPosition.x, CurrentBoardPosition.y - 1);
        }
        protected Vector2Int GetNorthCoord()
        {
            return new Vector2Int(CurrentBoardPosition.x, CurrentBoardPosition.y + 1);
        }

        public Vector2Int GetNeigbourCoord(CommandDir rotation)
        {
            switch (rotation)
            {
                case CommandDir.North:
                    return GetNorthCoord();
                case CommandDir.East:
                    return GetEastCoord();
                case CommandDir.West:
                    return GetWestCoord();
                case CommandDir.South:
                    return GetSouthCoord();
                default:
                    return CurrentBoardPosition;
            }
        }

        #endregion
    }
}