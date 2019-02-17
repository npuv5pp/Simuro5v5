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
        private List<MatchInfo> Data = new List<MatchInfo>();

        /// <summary>
        /// 已记录的数据长度
        /// </summary>
        public int DataLength { get { return Data.Count; } }

        public DataRecorder() { }

        public DataRecorder(bool InitNow)
        {
            if (InitNow)
            {
                Initialize();
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize()
        {
            Event.Register(Event.EventType1.MatchInfoUpdate, Record);
            //Event.Register(Event.EventType1.NewRound, Remove);
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
                Debug.Log("new matchinfo recorded");
                Data.Add(matchInfo.Clone());
            }
            else
            {
                Debug.Log("error matchinfo");
                Data.Add(null);
            }
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
        /// 索引第<paramref name="i"/>个数据
        /// </summary>
        /// <param name="i">下标</param>
        /// <returns>第i个MatchInfo数据</returns>
        public MatchInfo Index(int i)
        {
            return Data[i];
        }
    }
}
