using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;
using UnityEngine.Profiling;

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

        /// <summary>
        /// 基本数据类型
        /// </summary>
        public class RecordData
        {
            public readonly MatchInfo matchInfo;
            [JsonConverter(typeof(StringEnumConverter))]
            public readonly DataType type;

            public RecordData(DataType type)
            {
                this.type = type;
            }

            [JsonConstructor]
            public RecordData(DataType type, MatchInfo matchInfo)
            {
                this.type = type;
                this.matchInfo = matchInfo;
            }
        }

        public string Name => $"{beginTime.Year}/{beginTime.Month}/{beginTime.Day} {beginTime.Hour}:{beginTime.Minute}";

        public bool IsRecording { get; private set; }

        private DateTime beginTime;

        private readonly List<RecordData> data = new List<RecordData>();

        /// <summary>
        /// 已记录的数据长度
        /// </summary>
        public int DataLength => data.Count;

        public DataRecorder()
        {
        }

        public DataRecorder(string json)
        {
            data = JsonConvert.DeserializeObject<List<RecordData>>(json);
        }

        /// <summary>
        /// 开始记录
        /// </summary>
        public void Start()
        {
            IsRecording = true;
            beginTime = DateTime.Now;
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
                data.Add(new RecordData(DataType.InPlaying, new MatchInfo(matchInfo)));
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
            data.Add(new RecordData(type));
        }

        public static DataRecorder PlaceHolder()
        {
            var recorder = new DataRecorder()
            {
                beginTime = DateTime.Now,
            };
            recorder.data.Add(new RecordData(DataType.InPlaying, new MatchInfo()));
            recorder.data.Add(new RecordData(DataType.InPlaying, MatchInfo.NewDefaultPreset()));

            return recorder;
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
        public RecordData GetLastInfo()
        {
            return data.Count == 0 ? null : data[data.Count - 1];
        }

        /// <summary>
        /// 索引第<paramref name="i"/>个数据,下标从0开始
        /// </summary>
        /// <param name="i">下标</param>
        /// <returns>第i个MatchInfo数据</returns>
        public RecordData IndexOf(int i)
        {
            return data[i];
        }

        /// <summary>
        /// 将历史记录序列化为 Json 格式
        /// </summary>
        public string Serialize()
        {
            return JsonConvert.SerializeObject(data);
        }
    }
}
