using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ROOT
{
    public abstract class UnitSignalCoreBase : MonoBehaviour
    {
        public Unit Owner;
        protected Board GameBoard => Owner.GameBoard;
        public abstract SignalType Type { get; }
        public abstract List<Vector2Int> SingleInfoCollectorZone { get; }

        [ReadOnly] public RotationDirection SignalFromDir;
        [ReadOnly] public bool Visited;//dequeue
        [ReadOnly] public bool Visiting;//enqueue
        [ReadOnly] public int MatrixVal;
        [ReadOnly] public bool InMatrix;
        [ReadOnly] public bool InMatrixSignal;

        /// <summary>
        /// ��¼������Ӳ����ȵı�����������Ϊ�м�������ʹ����������и�ֵ���������
        /// �������Ϊɨ���źŵ�Ӳ����ȣ�ĩ�˱�ȻΪ1�����ҷ�ɨ���ź���Ȼ�������ֵ��
        /// ���������ֵ��ĩ�˵�ɨ����ĵ�ԪӦ���ǵ����ġ�
        /// </summary>
        [ReadOnly] public int ServerDepth; //for scoring purpose

        /// <summary>
        /// ��¼�������ź���ȵı�����������Ϊ�м�������ʹ����������и�ֵ���������
        /// �������Ϊɨ���źŵ��ź���ȣ���ɨ���źŲ��������ֵ��
        /// </summary>
        [ReadOnly] public int ServerSignalDepth; //for scoring purpose

        /// <summary>
        /// ���һ�μƷֺ󣬱���Ԫ�Ƿ��ڱ�Ҫ������С������ڵ���Ҫ��ʽ��Ϊfalse��
        /// </summary>
        [ReadOnly] public bool InServerGrid; //for scoring purpose

        void Awake()
        {
            Visited = false;
            InServerGrid = false;
            InMatrix = false;
        }

        public float CalScore()
        {
            return CalScore(out var a);
        }
        public abstract float CalScore(out int hardwareCount);
    }
}