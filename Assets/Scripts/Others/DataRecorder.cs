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
        public enum DataType
        {
            AutoPlacement,
            NewMatch,
            NewRound,
            InPlaying,
        }

        public string Name;
        private DateTime beginTime;
        public bool IsRecording { get; set; }

        private readonly List<BaseRecordData> data = new List<BaseRecordData>();

        /// <summary>
        /// 已记录的数据长度
        /// </summary>
        public int DataLength => data.Count;

        /// <summary>
        /// 开始记录
        /// </summary>
        public void Start()
        {
            IsRecording = true;
            beginTime = DateTime.Now;
            Name = $"{beginTime.Year}/{beginTime.Month}/{beginTime.Day} {beginTime.Hour}:{beginTime.Minute}";
            Event.Register(Event.EventType1.MatchInfoUpdate, RecordMatchInfo);
            Event.Register(Event.EventType0.MatchStart, RecordNewMatch);
            Event.Register(Event.EventType0.RoundStart, RecordNewRound);
            Event.Register(Event.EventType0.AutoPlacement, RecordAutoPlacement);
        }

        /// <summary>
        /// 停止记录
        /// </summary>
        public void Stop()
        {
            IsRecording = false;
            Event.UnRegister(Event.EventType1.MatchInfoUpdate, RecordMatchInfo);
            Event.UnRegister(Event.EventType0.MatchStart, RecordNewMatch);
            Event.UnRegister(Event.EventType0.RoundStart, RecordNewRound);
            Event.UnRegister(Event.EventType0.AutoPlacement, RecordAutoPlacement);
        }

        /// <summary>
        /// <see cref="Event.EventType1.MatchInfoUpdate"/>MatchInfoUpdate事件的回调函数，将当前的MatchInfo记录到List中。
        /// </summary>
        /// <param name="obj"></param>
        private void RecordMatchInfo(object obj)
        {
            if (obj is MatchInfo matchInfo)
            {
                data.Add(new StateRecordData(DataType.InPlaying, new MatchInfo(matchInfo)));
            }
            else
            {
                data.Add(null);
            }
        }

        private void RecordNewMatch()
        {
            RecordState(DataType.NewMatch);
        }

        private void RecordNewRound()
        {
            RecordState(DataType.NewRound);
        }

        private void RecordAutoPlacement()
        {
            RecordState(DataType.AutoPlacement);
        }

        private void RecordState(DataType type)
        {
            data.Add(new StateRecordData(type));
        }

        public void Add(BaseRecordData recodeData)
        {
            if (Name == null)
            {
                beginTime = DateTime.Now;
                Name = $"{beginTime.Year}/{beginTime.Month}/{beginTime.Day} {beginTime.Hour}:{beginTime.Minute}";
            }
            this.data.Add(recodeData);
        }

        /// <summary>
        /// 清空历史记录
        /// </summary>
        public void Clear()
        {
            data.Clear();
        }

        /// <summary>
        /// 获取最后一拍的数据
        /// </summary>
        /// <returns></returns>
        public BaseRecordData GetLastInfo()
        {
            return data.Count == 0 ? null : data[data.Count - 1];
        }

        /// <summary>
        /// 索引第<paramref name="i"/>个数据,下标从0开始
        /// </summary>
        /// <param name="i">下标</param>
        /// <returns>第i个MatchInfo数据</returns>
        public BaseRecordData Get(int i)
        {
            if (i >= DataLength)
            {
                return null;
            }
            return data[i];
        }

        /// <summary>
        /// 基本数据类型
        /// </summary>
        public abstract class BaseRecordData
        {
            public DataType type;
        }

        public class StateRecordData : BaseRecordData
        {
            public MatchInfo matchInfo;

            public StateRecordData(DataType type)
            {
                base.type = type;
            }

            public StateRecordData(DataType type, MatchInfo match)
            {
                base.type = DataType.InPlaying;
                matchInfo = match;
            }
        }
    }
}
