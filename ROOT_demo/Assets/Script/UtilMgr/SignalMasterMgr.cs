using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ROOT
{
    public class SignalMasterMgr : MonoBehaviour
    {
        private static SignalMasterMgr _instance;
        public static SignalMasterMgr Instance => _instance;
        
        void Awake()
        {
            DontDestroyOnLoad(gameObject);
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                _instance = this;
            }
        }

        private Dictionary<SignalType, SignalAssetBase> signalAssetLib;
        public SignalType[] SignalLib => signalAssetLib.Keys.ToArray();

        void Start()
        {
            //���ʱ��������Awake��Unit��Awake֮������⣻
            //�С����ʱ�򡱰�æ�����أ�unit��Ȼ��������ʼ������֮��ű�������
            signalAssetLib = new Dictionary<SignalType, SignalAssetBase>();
            foreach (var signalBase in GetComponentsInChildren<SignalAssetBase>())
            {
                signalAssetLib.Add(signalBase.Type, signalBase);
            }
            //�������ﻹҪ��һ����֤������
        }

        #region Delegate

        //������ͨ���������Ե���SignalAsset����ġ�׼��̬��������
        //���׼��̬�������ͼ��ˣ�����ֻ���ֶ��ӡ�
        public bool ShowSignal(SignalType type, Unit unit, Unit otherUnit)
        {
            return signalAssetLib[type].ShowSignal(unit, otherUnit);
        }

        public int SignalVal(SignalType type, Unit unit, Unit otherUnit)
        {
            return signalAssetLib[type].SignalVal(unit, otherUnit);
        }

        public Type SignalUnitCore(SignalType type)
        {
            return signalAssetLib[type].UnitSignalCoreType;
        }

        public float CalAllScoreBySignal(SignalType type, Board gameBoard, out int hardwareCount)
        {
            return signalAssetLib[type].CalAllScore(gameBoard, out hardwareCount);
        }

        public float CalAllScoreBySignal(SignalType type, Board gameBoard)
        {
            return signalAssetLib[type].CalAllScore(gameBoard);
        }

        public float CalAllScoreAllSignal(Board gameBoard)
        {
            return signalAssetLib.Values.Sum(v => v.CalAllScore(gameBoard));
        }

        #endregion
    }
}