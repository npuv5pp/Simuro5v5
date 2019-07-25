using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Simuro5v5
{

    /// 平台运行时的配置
    class Configuration
    {
        public class StrategyConfig
        {
            [JsonProperty]
            public static int BlueStrategyPort { get; set; }
            [JsonProperty]
            public static int YellowStrategyPort { get; set; }

            [JsonProperty]
            public static int ConnectTimeout { get; set; }

            static StrategyConfig()
            {
                BlueStrategyPort = 20000;
                YellowStrategyPort = 20001;

                ConnectTimeout = 3000;
            }
        }

        public class GeneralConfig
        {
            [JsonProperty]
            public static float TimeScale { get; set; }

            [JsonProperty]
            public static int EndOfHalfGameTime { get; set; }

            [JsonProperty]
            public static int EndOfOverGameTime { get; set; }

            static GeneralConfig()
            {
                // 默认时间流速
                TimeScale = 1.0f;
                EndOfHalfGameTime = 5 * 60 * 66;
                EndOfOverGameTime = 3 * 60 * 66;
            }
        }

        
        public class ParameterConfig
        {
            // linear
            [JsonProperty]
            public static float ForwardForceFactor { get; set; }
            [JsonProperty]
            public static float DragFactor { get; set; }
            [JsonProperty]
            public static float DoubleZeroDragFactor { get; set; }
            [JsonProperty]
            public static float SidewayDragFactor { get; set; }

            // angular
            [JsonProperty]
            public static float TorqueFactor { get; set; }
            [JsonProperty]
            public static float AngularDragFactor { get; set; }
            [JsonProperty]
            public static float ZeroAngularDragFactor { get; set; }

            static ParameterConfig()
            {
                // linear
                ForwardForceFactor = 102.89712678726376f;
                DragFactor = 79.81736047779975f;
                DoubleZeroDragFactor = 760;
                SidewayDragFactor = 1000;

                // angular
                TorqueFactor = 1156.1817018313f;
                AngularDragFactor = 3769.775104018879f;
                ZeroAngularDragFactor = 2097.9773f;
            }
        }

        #region Stub objects
        [JsonProperty] private static StrategyConfig strategyConfig = new StrategyConfig();
        [JsonProperty] private static GeneralConfig generalConfig = new GeneralConfig();
        #if UNITY_EDITOR
        // 物理参数只能在开发时使用
        [JsonProperty] private static ParameterConfig parameterConfig = new ParameterConfig();
        #endif
        #endregion

        /// <summary>
        /// 读取配置并总是写回。如果配置不存在，则创建一个空的
        /// </summary>
        /// <param name="fileName">配置文件名</param>
        public static void ReadFromFileOrCreate(string fileName)
        {
            if (File.Exists(fileName))
            {
                string json = File.ReadAllText(fileName);
                try
                {
                    JsonConvert.DeserializeObject<Configuration>(json);
                }
                catch (JsonReaderException e)
                {
                    Debug.LogError(e);
                    Win32Dialog.ShowMessageBox($"Config file parse error: \n{e.Message}", "Config Error");
                    return;
                }
            }
            // 总是将配置写回
            SaveToFile(fileName);
        }

        public static void SaveToFile(string fileName)
        {
            string json = JsonConvert.SerializeObject(new Configuration(), Formatting.Indented);
            File.WriteAllText(fileName, json);
        }
    }

}

