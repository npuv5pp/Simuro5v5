using System;
using System.Collections.Generic;
using UnityEngine;

namespace Simuro5v5
{
    using Simuro5v5.EventSystem;

    /// <summary>
    /// 用于记录赛场数据。
    /// 数据格式为<see cref="MatchInfo"/>的集合
    /// </summary>
    public class DataRecorder
    {
        public String Name;
        private DateTime BeginTime;

        private List<MatchInfo> Data = new List<MatchInfo>();

        /// <summary>
        /// 已记录的数据长度
        /// </summary>
        public int DataLength { get { return Data.Count; } }

        public DataRecorder() { }

        /// <summary>
        /// 开始记录
        /// </summary>
        public void Begin()
        {
            BeginTime = DateTime.Now;
            Name = $"{BeginTime.Year}-{BeginTime.Month}-{BeginTime.Day}-{BeginTime.Hour}-{BeginTime.Minute}";
            Event.Register(Event.EventType1.MatchInfoUpdate, Record);
        }

        /// <summary>
        /// 停止记录
        /// </summary>
        public void Stop()
        {
            Event.UnRegister(Event.EventType1.MatchInfoUpdate, Record);
        }

        /// <summary>
        /// <see cref="Event.EventType1.MatchInfoUpdate"/>事件的回调函数，将当前的MatchInfo记录到List中。
        /// </summary>
        /// <param name="obj"></param>
        private void Record(object obj)
        {
            MatchInfo matchInfo = obj as MatchInfo;
            if (matchInfo != null)
            {
                Data.Add(matchInfo.Clone());
            }
            else
            {
                Data.Add(null);
            }
        }

        public void Add(MatchInfo match)
        {
            if (Name == null)
            {
                BeginTime = DateTime.Now;
                Name = $"{BeginTime.Year}-{BeginTime.Month}-{BeginTime.Day}-{BeginTime.Hour}-{BeginTime.Minute}";
            }
            Data.Add(match);
        }

        /// <summary>
        /// 清空历史记录
        /// </summary>
        public void Clear()
        {
            Data.Clear();
        }

        /// <summary>
        /// 获取最后一拍的数据
        /// </summary>
        /// <returns></returns>
        public MatchInfo GetLastInfo()
        {
            return Data.Count == 0 ? null : Data[Data.Count - 1];
        }

        /// <summary>
        /// 索引第<paramref name="i"/>个数据,下标从0开始
        /// </summary>
        /// <param name="i">下标</param>
        /// <returns>第i个MatchInfo数据</returns>
        public MatchInfo Get(int i)
        {
            if (i >= DataLength)
            {
                return null;
            }
            return Data[i];
        }
    }
}
