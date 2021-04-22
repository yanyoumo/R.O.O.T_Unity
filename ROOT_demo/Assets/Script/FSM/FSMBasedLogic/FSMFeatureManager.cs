using System;
using System.Collections.Generic;
using System.Linq;
using ROOT.Common;
using Sirenix.Utilities;
using UnityEngine;

namespace ROOT
{
    //切换的时候的cascade参数是切换时候层叠调用的；
    //如果是开的时候cas，那就是把所有这个feature依赖的所有feature都打开（都是指internal）。
    //如果是关的时候cas，那就是把所有依赖这个feature的所有feature也都关了（都是指internal）。

    //向外暴露的时候cas锁为true，如果是false那么就只开关那种internal，别开关的不管，技术上应该没问题。

    //但是能做的有一个大前提、所有的feature的依赖只能用“与”判断，不能有互斥和其他的逻辑；
    //如果从feature角度上客观互斥的话、那么就需要从继承层面拆class。
    //虽然shop和skill不能让玩家同时操作，但是在feature层面上不互斥。

    //一个FSM需要将它的feature和所依赖的features注册进来。
    public class FSMFeatureManager
    {
        public Action FeaturesChanged;

        private Dictionary<FSMFeatures, bool> _interalToggles;
        private Dictionary<FSMFeatures, FSMFeatures[]> _externalToggles;

        public void RegistFSMFeature(FSMFeatures feat, FSMFeatures[] dependingFeats, bool defaultVal)
        {
            _interalToggles.Add(feat, defaultVal);
            _externalToggles.Add(feat, dependingFeats);
        }

        private IEnumerable<FSMFeatures> GetDependedFeatures(FSMFeatures feat) => _externalToggles.Where(v => v.Value.Contains(feat)).Select(v => v.Key).ToArray();

        public bool RequestChangeFeature(FSMFeatures feat, bool value, bool cascadeChange = false)
        {
            if (!cascadeChange)
            {
                _interalToggles[feat] = value;
                FeaturesChanged();
                return GetExternalToggleVal(feat);
            }

            if (value)
            {
                _interalToggles[feat] = true;
                _externalToggles[feat].ForEach(f => _interalToggles[f] = true);
                Debug.Assert(GetExternalToggleVal(feat));
                FeaturesChanged();
                return true;
            }
            else
            {
                _interalToggles[feat] = false;
                GetDependedFeatures(feat).ForEach(f => _interalToggles[f] = false);
                Debug.Assert(!GetExternalToggleVal(feat));
                FeaturesChanged();
                return true;
            }
        }

        private bool GetInternalToggleVal(FSMFeatures feats)
        {
            try
            {
                return _interalToggles[feats];
            }
            catch (KeyNotFoundException e)
            {
                Debug.LogError("Requesting feature not present.");
                return false;
            }
        }

        public bool GetExternalToggleVal(FSMFeatures feats)
        {
            try
            {
                return GetInternalToggleVal(feats) && _externalToggles[feats].All(GetInternalToggleVal);
            }
            catch (Exception e)
            {
                Debug.LogError("Requesting feature not present.");
                return false;
            }
        }

        public FSMFeatureManager()
        {
            _interalToggles = new Dictionary<FSMFeatures, bool>();
            _externalToggles = new Dictionary<FSMFeatures, FSMFeatures[]>();
        }
    }
}