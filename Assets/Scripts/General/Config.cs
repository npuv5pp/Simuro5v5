using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

// 平台运行时的配置
namespace Simuro5v5.Config
{
    /// <summary>
    /// 配置组需要包含的特性。
    /// <para>
    /// 每个配置组均为静态类，类中每个包含get、set属性访问器的静态public成员均作为
    /// </para>
    /// </summary>
    class ConfigGroupAttribute : Attribute
    {
    }

    [ConfigGroup]
    public static class StrategyConfig
    {
        public static int BlueStrategyPort { get; set; }
        public static int YellowStrategyPort { get; set; }

        public static int ConnectTimeout { get; set; }

        static StrategyConfig()
        {
            BlueStrategyPort = 20000;
            YellowStrategyPort = 20001;

            ConnectTimeout = 3000;
        }
    }

    [ConfigGroup]
    public static class GeneralConfig
    {
    }

    [ConfigGroup]
    public static class ParameterConfig
    {
        // linear
        public static float ForwardForceFactor { get; set; }
        public static float DragFactor { get; set; }
        public static float DoubleZeroDragFactor { get; set; }
        public static float SidewayDragFactor { get; set; }

        // angular
        public static float TorqueFactor { get; set; }
        public static float AngularDragFactor { get; set; }
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

    public static class ConfigManager
    {
        private static readonly List<Type> PlatformConfig = new List<Type>
        {
            typeof(GeneralConfig),
            typeof(StrategyConfig),
            typeof(ParameterConfig),
        };

        private static readonly string DefaultConfigJson = ToJson();

        public static void ReadConfigFile(string configPath)
        {
            if (File.Exists(configPath))
            {
                try
                {
                    FromFile(configPath);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("Resetting to default config. Invalid config file: " + ex.Message);
                    ResetToDefault();
                }
            }

            // 无论如何重新写回文件，可以修正文件中缺失的键
            ToFile(configPath);
        }

        public static void ResetToDefault()
        {
            if (DefaultConfigJson != null)
            {
                FromJson(DefaultConfigJson);
            }
            else
            {
                throw new NullReferenceException("null default Config");
            }
        }

        public static void ToFile(string path)
        {
            using (var fileStream = File.Create(path))
            {
                var bs = System.Text.Encoding.UTF8.GetBytes(ToJson());
                fileStream.Write(bs, 0, bs.Length);
            }
        }

        public static string ToJson()
        {
            Dictionary<string, object> configDict = new Dictionary<string, object>();

            foreach (var configType in PlatformConfig)
            {
                configDict[configType.Name] = TypeToDict(configType);
            }

            return JsonConvert.SerializeObject(configDict, Formatting.Indented);
        }

        public static void FromFile(string path)
        {
            var str = File.ReadAllText(path);
            FromJson(str);
        }

        public static void FromJson(string json)
        {
            Dictionary<string, object> dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

            foreach (var configType in PlatformConfig)
            {
                if (dict[configType.Name] is JObject jObj)
                {
                    var d = jObj.ToObject<Dictionary<string, object>>();
                    DictToType(d, configType);
                }
            }
        }

        private static void DictToType(Dictionary<string, object> dict, Type type)
        {
            foreach (var p in type.GetProperties())
            {
                object obj = p.GetValue(null, null);
                Type objType = obj.GetType();
                if (!dict.ContainsKey(p.Name)) continue;
                if (HasConfigAttribute(obj))
                {
                    var d = ((Newtonsoft.Json.Linq.JObject) dict[p.Name]).ToObject<Dictionary<string, object>>();
                    DictToType(d, objType);
                }
                else
                {
                    try
                    {
                        object val = dict[p.Name];
                        if (val is long)
                        {
                            val = Convert.ToInt32(val);
                        }

                        p.SetValue(null, val, null);
                    }
                    catch
                    {
                    }
                }
            }
        }


        private static Dictionary<string, object> TypeToDict(Type t)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            foreach (var p in t.GetProperties())
            {
                // TODO: 为什么要对 null 调用？
                object obj = p.GetValue(null, null);
                if (obj != null)
                {
                    if (HasConfigAttribute(obj))
                    {
                        Type objType = obj.GetType();
                        dict[objType.Name] = TypeToDict(objType);
                    }
                    else
                    {
                        dict[p.Name] = obj;
                    }
                }
            }

            return dict;
        }


        private static bool HasConfigAttribute(object obj)
        {
            foreach (object attr in obj.GetType().GetCustomAttributes(false))
            {
                if (attr is ConfigGroupAttribute)
                {
                    return true;
                }
            }

            return false;
        }
    }
}