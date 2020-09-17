using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ROOT
{
    public enum NameID
    {
        YanYoumo_Log,
        SuYuxuan_Log,
        JiangDigong_Log,
    }

    public enum WatchID
    {
        YanYoumo_ExampleA,
        SuYuxuan_ExampleA,
        JiangDigong_ExampleA,
    }

    /// <summary>
    /// 这里是对Console Pro的重做Wrapper，那个版本的对多人支持并不好。
    /// 在使用RootDebug的时候，遵守以下几个原则和特点：
    /// 1、所有人在处理Log的时候，要考虑这个Log是否是只有自己用。
    /// 2、将“仅自己用”和“需要通知他人”的Log进行分类。
    /// 3、“需要通知他人”的Log使用常规Debug.Log/Warning/Error进行使用。
    ///     3a、需要主意，这种Log请能省则省。
    /// 4、“仅自己用”请使用RootDebug和NameID进行Log
    ///     4a、这种Log原则上尽自己方便使用即可
    ///     4b、这种Log只有Log一种，没有Warning和Error等。
    /// 5、关于Watch的使用。
    ///     5a、主要是便于实时跟踪某个数据。
    ///     5b、鉴于其无法被Filter跟踪，如果想使用，请在WatchID中注册。
    ///     5c、WatchID中注册自己的实际enum后就可以删掉WatchID中对应的占位符。
    /// </summary>
    public static class RootDebug
    {
        private static string WatchFilter(string inStr, WatchID id)
        {
            return inStr + "{\"cmd\":\"Watch\" \"name\":\"" + id + "\"}";
        }

        private static string NameFilter(string inStr,NameID id)
        {
            return inStr + "{\"cmd\":\"Filter\" \"name\":\"" + id + "\"}";
        }

        private static string CPAPI(string inStr)
        {
            return inStr + "\nCPAPI:";
        }

        public static void Log(string inLog, NameID id, UnityEngine.Object inContext = null)
        {
            Debug.Log(NameFilter(CPAPI(inLog), id), inContext);
        }

        public static void Watch(string inWatch, WatchID id, UnityEngine.Object inContext = null)
        {
            Debug.Log(WatchFilter(CPAPI(inWatch), id), inContext);
        }
    }
}