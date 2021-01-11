using System.Collections;
using System.Collections.Generic;
using Rewired;
using UnityEngine;

namespace ROOT
{
    public class ControllingEventMgr : MonoBehaviour
    {
        public int playerId;
        private Player player;

        public static WorldEvent.ControllingEventHandler ControllingEvent;

        void Awake()
        {
            player = ReInput.players.GetPlayer(playerId);
            player.AddInputEventDelegate(OnInputUpdate, UpdateLoopType.Update);
        }

        private void OnInputUpdate(InputActionEventData obj)
        {
            //throw new System.NotImplementedException();
            ControllingEvent?.Invoke(new ActionPack());
        }
    }
}