using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ROOT
{
    public abstract class UnitSignalCoreBase : MonoBehaviour
    {
        public Unit Owner;

        [ReadOnly] public RotationDirection SignalFromDir;
        [ReadOnly] public bool Visited;//dequeue
        [ReadOnly] public bool Visiting;//enqueue
        [ReadOnly] public int HardDiskVal;
        [ReadOnly] public bool InHddGrid;
        [ReadOnly] public bool InHddSignalGrid;

        /// <summary>
        /// ��¼�������ź���ȵı������ͷ�����������Network����ֵӦ��Ϊ1.
        /// ������Ϊ�м�������ʹ����������и�ֵ���������
        /// </summary>
        [ReadOnly] public int ServerDepth; //for scoring purpose

        /// <summary>
        /// ���һ�μƷֺ󣬱���Ԫ�Ƿ��ڱ�Ҫ������С������ڵ���Ҫ��ʽ��Ϊfalse��
        /// </summary>
        [ReadOnly] public bool InServerGrid; //for scoring purpose

        /// <summary>
        /// ������ʾLED��field��������ӽ��������ĸ���ֵӦΪȫ����ȣ���֦�˵���ʾֵ��ҪΪ1��
        /// </summary>
        public int NetworkVal => ServerDepth;

        void Awake()
        {
            Visited = false;
            InServerGrid = false;
            InHddGrid = false;
        }

        public float CalScore()
        {
            return CalScore(out var a);
        }
        public abstract float CalScore(out int hardwareCount);
    }
}