using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ROOT
{
    public interface IPlaceable
    {      
        Vector2Int board_position { get; set; }
        void UpdateTransform(Vector3 pos);
    }

    public abstract class MoveableBase : MonoBehaviour, IPlaceable
    {
        public Vector2Int board_position { get; set; }
        public abstract void UpdateTransform(Vector3 pos);

        #region MoveToNeigbour

        public virtual void MoveLeft()
        {
            board_position = new Vector2Int(board_position.x - 1, board_position.y);
        }

        public virtual void MoveRight()
        {
            board_position = new Vector2Int(board_position.x + 1, board_position.y);
        }

        public virtual void MoveUp()
        {
            board_position = new Vector2Int(board_position.x, board_position.y + 1);
        }

        public virtual void MoveDown()
        {
            board_position = new Vector2Int(board_position.x, board_position.y - 1);
        }

        #endregion

        #region GetNeigbourCoord

        public Vector2Int GetEastUnit()
        {
            return new Vector2Int(board_position.x + 1, board_position.y);
        }
        public Vector2Int GetWestUnit()
        {
            return new Vector2Int(board_position.x - 1, board_position.y);
        }
        public Vector2Int GetSouthUnit()
        {
            return new Vector2Int(board_position.x, board_position.y - 1);
        }
        public Vector2Int GetNorthUnit()
        {
            return new Vector2Int(board_position.x, board_position.y + 1);
        }

        #endregion
    }
}
