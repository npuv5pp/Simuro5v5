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
    public class DataRecorder : IDisposable
    {
        public enum DataType
        {
            AutoPlacement,
            NewMatch,
            NewRound,
            InPlaying,
        }

        public string Name;
        private DateTime BeginTime;

        private List<BaseRecodeData> Data = new List<BaseRecodeData>();

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
            Event.Register(Event.EventType1.MatchInfoUpdate, RecordMatchInfo);
            Event.Register(Event.EventType0.MatchStart, RecodeNewMatch);
            Event.Register(Event.EventType0.RoundStart, RecodeNewRound);
        }

        /// <summary>
        /// 停止记录
        /// </summary>
        public void Stop()
        {
            Event.UnRegister(Event.EventType1.MatchInfoUpdate, RecordMatchInfo);
            Event.UnRegister(Event.EventType0.MatchStart, RecodeNewMatch);
            Event.UnRegister(Event.EventType0.RoundStart, RecodeNewRound);
        }

        /// <summary>
        /// <see cref="Event.EventType1.MatchInfoUpdate"/>MatchInfoUpdate事件的回调函数，将当前的MatchInfo记录到List中。
        /// </summary>
        /// <param name="obj"></param>
        private void RecordMatchInfo(object obj)
        {
            MatchInfo matchInfo = obj as MatchInfo;
            if (matchInfo != null)
            {
                Data.Add(new StateRecodeData(DataType.InPlaying, matchInfo.Clone()));
            }
            else
            {
                Data.Add(null);
            }
        }

        private void RecodeNewMatch()
        {
            RecodeState(DataType.NewMatch);
        }

        private void RecodeNewRound()
        {
            RecodeState(DataType.NewRound);
        }

        private void RecodeState(DataType type)
        {
            Data.Add(new StateRecodeData(type));
        }

        public  void Add(BaseRecodeData data)
        {
            if (Name == null)
            {
                BeginTime = DateTime.Now;
                Name = $"{BeginTime.Year}-{BeginTime.Month}-{BeginTime.Day}-{BeginTime.Hour}-{BeginTime.Minute}";
            }
            Data.Add(data);
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
        public BaseRecodeData GetLastInfo()
        {
            return Data.Count == 0 ? null : Data[Data.Count - 1];
        }

        /// <summary>
        /// 索引第<paramref name="i"/>个数据,下标从0开始
        /// </summary>
        /// <param name="i">下标</param>
        /// <returns>第i个MatchInfo数据</returns>
        public BaseRecodeData Get(int i)
        {
            if (i >= DataLength)
            {
                return null;
            }
            return Data[i];
        }

        public void Dispose()
        {
            Stop();
        }


        /// <summary>
        /// 基本数据类型
        /// </summary>
        public abstract class BaseRecodeData
        {
            public DataType type;
        }

        public class StateRecodeData : BaseRecodeData
        {
            public MatchInfo matchInfo;

            public StateRecodeData(DataType type)
            {
                base.type = type;
            }

            public StateRecodeData(DataType type, MatchInfo match)
            {
                base.type = DataType.InPlaying;
                matchInfo = match;
            }
        }
    }
}
