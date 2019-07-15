using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Simuro5v5
{

    // 平台运行时的配置
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

            static GeneralConfig()
            {
                // 默认时间流速
                TimeScale = 1.0f;
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
                SidewayDragFactor = 30000;

                // angular
                TorqueFactor = 1156.1817018313f;
                AngularDragFactor = 3769.775104018879f;
                ZeroAngularDragFactor = 305500;
            }
        }

        #region Stub objects
        [JsonProperty] private static StrategyConfig strategyConfig;
        [JsonProperty] private static ParameterConfig parameterConfig;
        [JsonProperty] private static GeneralConfig generalConfig;
        #endregion

        public static void ReadFromFileOrCreate(string fileName)
        {
            if (File.Exists(fileName))
            {
                string json = File.ReadAllText(fileName);
                JsonConvert.DeserializeObject<Configuration>(json);
            }
            else
            {
                SaveToFile(fileName);
            }
        }

        public static void SaveToFile(string fileName)
        {
            string json = JsonConvert.SerializeObject(new Configuration(), Formatting.Indented);
            File.WriteAllText(fileName, json);
        }
    }

}

