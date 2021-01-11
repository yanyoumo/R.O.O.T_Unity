using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Rewired;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ROOT
{
    //��������Ƚϵ��ۡ����̡��ֱ�����ڰ����Ĳ�����������ֻ���ʱ��Ĳ�����ͬ��
    //�����е�����·����ƴ��һ��о�������Ϊ�����ʱ��ϵͳ�Ľ��Ŀǰ��ֻ����ô�졣
    [Serializable]
    public struct ActionPack
    {
        public int ActionID => ActionEventData.actionId;
        public InputActionEventData ActionEventData;
        public RotationDirection ActionDirection;
        public Vector2 MouseScreenPosA;
        public Vector2 MouseScreenPosB;
        public int FuncID;//��֮ǰFunc0~9��ID�����ϵ�һ��compositeAction���桢Ȼ��Ѿ����IDд�������
    }

    //������ʵ��������س�ʼ���Ѿ��㶨��
    //�������Ϊ�Ƕ������Ը�Ľ��͡����õ�RAW��Ӳ���¼��󡢡���ͼ�������ҵ���ͼ��Ȼ��ת��ΪActionʵ����
    //��һ���ǣ������ֻ����ʵ�жϡ���ȥ�����Ƿ�Ϸ���ֻ�����ӵط��������ͼ��Action��
    public class ControllingEventMgr : MonoBehaviour
    {
        [NotNull] private static ControllingEventMgr _instance;
        public static ControllingEventMgr Instance => _instance;

        [ReadOnly] public int playerId;
        private Player player;

        public static WorldEvent.ControllingEventHandler ControllingEvent;

        void Awake()
        {
            DontDestroyOnLoad(gameObject);
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            player = ReInput.players.GetPlayer(playerId);
            player.AddInputEventDelegate(OnInputUpdate, UpdateLoopType.Update,InputActionEventType.ButtonJustPressed);
        }

        private void OnInputUpdate(InputActionEventData obj)
        {
            var actionPack = new ActionPack
            {
                //so on......
                ActionEventData = obj
            };
            ControllingEvent?.Invoke(actionPack);
            //Debug.Log("OnInputUpdate:" + obj.actionName);
        }
    }
}