using System;
using System.Collections.Generic;
using UnityEngine;

namespace Simuro5v5.EventSystem
{
    /// <summary>
    /// 事件类。静态类。
    /// 在订阅事件之后，每当事件发生时，相应的回调函数会被调用
    /// 用于各模块之间的通讯，相比直接调用函数，这样可以有效地降低模块之间的耦合。
    /// </summary>
    /// <example>
    /// 假设有一个需要进行赛场数据记录的类Subscriber，
    /// 同时有一个每当赛场更新就触发的事件<see cref="Event.EventType1.MatchInfoUpdate"/>，
    ///
    /// <para>在Subscriber中就可以这样写：</para>
    /// <code>
    /// void reigster()
    /// {
    ///     Event.Register(Event.EventType1.MatchInfoUpdate, HookFunc);
    /// }
    /// 
    /// public void HookFunc(object obj)
    /// {
    ///     MatchInfo matchinfo = obj as MatchInfo;
    ///     if (matchinfo != null)
    ///         Debug.log(MatchInfo.whosBall);
    /// }
    /// // 这样每当有其他代码触发这个事件，就会调试输出whosBall信息。
    /// </code>
    ///
    /// 下面这个类去触发这个事件：
    /// <code>
    /// public void UpdateMatchInfo(MatchInfo matchinfo)
    /// {
    ///     // .......
    ///     Event.Send(Event.EventType1.MatchInfoUpdate, matchinfo);
    /// }
    /// 每次运行这个函数，事件系统会调用订阅这个事件的函数。
    /// </code>
    /// 在整个过程中，Match类不需要了解有哪些对象需要监测这个事件，
    /// 订阅事件的类也不需要了解是哪个类触发的事件。
    /// </example>
    ///
    /// <remarks>
    /// <para>在事件的回调函数的参数列表中，包含类事件触发函数传递进来的对象，这些对象均为object类型，使用的时候需要进行合适的转换：</para>
    /// <para>如果是参数是引用类型，则可以使用as运算符进行转换；如果参数是值类型，则可以先使用is判断是否可以转换，然后使用强制类型转换。</para>
    /// 例如
    /// <code>
    /// Event.Register(Event.EventType1.StrategyBlueLoaded, delegate (object obj)
    /// {
    ///     var strategy = obj as IStrategy;
    ///     if (strategy != null)
    ///     {
    ///         Debug.Log(string.Format("Blue {0} loaded {1}.", strategy.Description, 
    ///             strategy.IsConnected() ? "succeed" : "failed"));
    ///     }
    /// });
    /// </code>
    /// </remarks>
    public static class Event
    {
        /// <summary>
        /// 参数数量为1的事件类型；
        /// </summary>
        public enum EventType0
        {
            // 事件名称
            PlatformStarted,

            MatchStart,
            MatchStop,
            RoundStart,
            RoundPause,
            RoundResume,
            RoundStop,
        }

        /// <summary>
        /// 参数数量为1的事件类型；
        /// </summary>
        public enum EventType1
        {
            // 事件名称                 // 传入参数
            MatchInfoUpdate,            // 更新后的赛场信息
            Goal,                       // 进球
            LogUpdate,                  // 更新比赛状态信息

            StrategyBlueLoaded,         // IStrategy策略实例
            StrategyYellowLoaded,       // IStrategy策略实例
            StrategyBlueFreed,
            StrategyYellowFreed,
            
            ReplayInfoUpdate
        }

        /// <summary>
        /// 参数数量为1的事件类型；
        /// </summary>
        public enum EventType2
        {
            WheelSetUpdate,
        }

        /// <summary>
        /// 参数数量为0的事件回调函数
        /// </summary>
        public delegate void ZeroObjEventHandler();

        /// <summary>
        /// 参数数量为1的事件回调函数
        /// </summary>
        /// <param name="obj">回调函数接受的参数</param>
        public delegate void OneObjEventHandler(object obj);

        /// <summary>
        /// 参数数量为2的事件回调函数
        /// </summary>
        /// <param name="obj1">回调函数接受的第一个参数</param>
        /// <param name="obj2">回调函数接受的第二个参数</param>
        public delegate void TwoObjEventHandler(object obj1, object obj2);

        private static Dictionary<EventType0, ZeroObjEventHandler> EventMap0;
        private static Dictionary<EventType1, OneObjEventHandler> EventMap1;
        private static Dictionary<EventType2, TwoObjEventHandler> EventMap2;

        /// <summary>
        /// 静态构造函数初始化事件类
        /// </summary>
        static Event()
        {
            EventMap0 = new Dictionary<EventType0, ZeroObjEventHandler>();
            EventMap1 = new Dictionary<EventType1, OneObjEventHandler>();
            EventMap2 = new Dictionary<EventType2, TwoObjEventHandler>();
            foreach (EventType0 evt in Enum.GetValues(typeof(EventType0)))
            {
                EventMap0.Add(evt, null);
            }
            foreach (EventType1 evt in Enum.GetValues(typeof(EventType1)))
            {
                EventMap1.Add(evt, null);
            }
            foreach (EventType2 evt in Enum.GetValues(typeof(EventType2)))
            {
                EventMap2.Add(evt, null);
            }
        }

        /// <summary>
        /// 注册事件；事件接受0个参数
        /// </summary>
        /// <param name="eventType">事件类型</param>
        /// <param name="handler">事件回调函数</param>
        public static void Register(EventType0 eventType, ZeroObjEventHandler handler)
        {
            if (EventMap0.ContainsKey(eventType))
                EventMap0[eventType] += handler;
        }

        /// <summary>
        /// 注册事件；事件接受一个参数
        /// </summary>
        /// <param name="eventType">事件类型</param>
        /// <param name="handler">事件回调函数</param>
        public static void Register(EventType1 eventType, OneObjEventHandler handler)
        {
            if (EventMap1.ContainsKey(eventType))
                EventMap1[eventType] += handler;
        }

        /// <summary>
        /// 注册事件；事件接受两个参数
        /// </summary>
        /// <param name="eventType">事件类型</param>
        /// <param name="handler">事件回调函数</param>
        public static void Register(EventType2 eventType, TwoObjEventHandler handler)
        {
            if (EventMap2.ContainsKey(eventType))
                EventMap2[eventType] += handler;
        }

        /// <summary>
        /// 触发事件；事件接受0参数
        /// </summary>
        /// <param name="eventType">事件类型</param>
        public static void Send(EventType0 eventType)
        {
            if (EventMap0.ContainsKey(eventType) && EventMap0[eventType] != null)
            {
                try
                {
                    EventMap0[eventType]();
                }
                catch (Exception ex)
                {
                    Debug.LogError("An exception occured while event(" + eventType.ToString() + ") send. " + ex.ToString());
                }
            }
        }

        /// <summary>
        /// 触发事件；事件接受一个参数
        /// </summary>
        /// <param name="eventType">事件类型</param>
        /// <param name="obj">传递给所有事件回调函数的参数</param>
        public static void Send(EventType1 eventType, object obj)
        {
            if (EventMap1.ContainsKey(eventType) && EventMap1[eventType] != null)
            {
                try
                {
                    EventMap1[eventType](obj);
                }
                catch (Exception ex)
                {
                    Debug.LogError("An exception occured while event(" + eventType.ToString() + ") send. " + ex.ToString());
                }
            }
        }

        /// <summary>
        /// 触发事件；事件接受两个参数
        /// </summary>
        /// <param name="eventType">事件类型</param>
        /// <param name="obj1">传递给所有事件回调函数的第一个参数</param>
        /// <param name="obj2">传递给所有事件回调函数的第二个参数</param>
        public static void Send(EventType2 eventType, object obj1, object obj2)
        {
            if (EventMap2.ContainsKey(eventType) && EventMap2[eventType] != null)
            {
                try
                {
                    EventMap2[eventType](obj1, obj2);
                }
                catch (Exception ex)
                {
                    Debug.LogError("An exception occured while event(" + eventType.ToString() + ") send. " + ex.ToString());
                }
            }
        }
    }
}
