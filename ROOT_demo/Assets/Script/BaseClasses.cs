using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ROOT
{
    /*public interface IPlaceable
    {      
        Vector2Int CurrentBoardPosition { get; set; }
        Vector2Int NextBoardPosition { get; set; }
        Vector2 LerpingBoardPosition { get; set; }
        void UpdateTransform(Vector3 pos);
    }*/

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
        public Vector2Int CurrentBoardPosition { get;protected set; }
        public Vector2Int NextBoardPosition { get; protected set; }
        public Vector2 LerpingBoardPosition { get; set; }
        public abstract void UpdateTransform(Vector3 pos);

        public Vector2 LerpBoardPos(float lerp)
        {
            float x = Mathf.Lerp((CurrentBoardPosition.x), (NextBoardPosition.x),lerp);
            float y = Mathf.Lerp((CurrentBoardPosition.y), (NextBoardPosition.y),lerp);
            return new Vector2(x, y);
        }

        #region MoveToNeigbour

        public virtual void MoveLeft()
        {
            NextBoardPosition = new Vector2Int(CurrentBoardPosition.x - 1, CurrentBoardPosition.y);
        }

        public virtual void MoveRight()
        {
            NextBoardPosition = new Vector2Int(CurrentBoardPosition.x + 1, CurrentBoardPosition.y);
        }

        public virtual void MoveUp()
        {
            NextBoardPosition = new Vector2Int(CurrentBoardPosition.x, CurrentBoardPosition.y + 1);
        }

        public virtual void MoveDown()
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

        public Vector2Int GetEastCoord()
        {
            return new Vector2Int(CurrentBoardPosition.x + 1, CurrentBoardPosition.y);
        }
        public Vector2Int GetWestCoord()
        {
            return new Vector2Int(CurrentBoardPosition.x - 1, CurrentBoardPosition.y);
        }
        public Vector2Int GetSouthCoord()
        {
            return new Vector2Int(CurrentBoardPosition.x, CurrentBoardPosition.y - 1);
        }
        public Vector2Int GetNorthCoord()
        {
            return new Vector2Int(CurrentBoardPosition.x, CurrentBoardPosition.y + 1);
        }

        public Vector2Int GetNeigbourCoord(RotationDirection rotation)
        {
            switch (rotation)
            {
                case RotationDirection.North:
                    return GetNorthCoord();
                case RotationDirection.East:
                    return GetEastCoord();
                case RotationDirection.West:
                    return GetWestCoord();
                case RotationDirection.South:
                    return GetSouthCoord();
                default:
                    return CurrentBoardPosition;
            }
        }

        #endregion
    }
}
