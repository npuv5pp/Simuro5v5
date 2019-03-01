using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// 平台运行时的配置
/// </summary>
namespace Simuro5v5.Config
{
    /// <summary>
    /// 配置组需要包含的特性。
    /// <para>
    /// 每个配置组均为静态类，类中每个包含get、set属性访问器的静态public成员均作为
    /// </para>
    /// </summary>
    class ConfigGroupAttribute : Attribute { }

    [ConfigGroup]
    public static class StrategyConfig
    {
        public static int BlueStrategyPort { get; set; }
        public static int YellowStrategyPort { get; set; }

        public static bool RunStrategyServer { get; set; }
        public static bool EnableStrategyLog { get; set; }
        public static string BlueStrategyLogFile { get; set; }
        public static string YellowStrategyLogFile { get; set; }
        public static string StrategyServer { get; set; }
        public static string StrategyServerScript { get; set; }
        public static bool UseUdp { get; set; }

        static StrategyConfig()
        {
            BlueStrategyPort = 20000;
            YellowStrategyPort = 20001;

            RunStrategyServer = true;
            EnableStrategyLog = true;
            BlueStrategyLogFile = @"BlueStrategy.log";
            YellowStrategyLogFile = @"YellowStrategy.log";

            StrategyServer = @"StrategyServer\dist\server\server.exe";
            StrategyServerScript = @"";
            UseUdp = false;
        }
    }

    [ConfigGroup]
    public static class GeneralConfig
    {
        public static bool EnableConvertYellowData { get; set; }

        static GeneralConfig()
        {
            EnableConvertYellowData = false;
        }
    }

    public static class ConfigManager
    {
        private static readonly List<Type> PlatformConfig = new List<Type>
        {
            typeof(GeneralConfig),
            typeof(StrategyConfig),
        };

        private static readonly string defaultConfigJson = ToJson();

        public static void ReadConfigFile(string configpath)
        {
            if (File.Exists(configpath))
            {
                try
                {
                    FromFile(configpath);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("Reseting to default config. Invalid config file: " + ex.Message);
                    ResetToDefault();
                }
            }
            // 无论如何重新写回文件，可以修正文件中缺失的键
            ToFile(configpath);
        }

        public static void ResetToDefault()
        {
            if (defaultConfigJson != null)
            {
                FromJson(defaultConfigJson);
            }
            else
            {
                throw new NullReferenceException("null default Config");
            }
        }

        public static void ToFile(string path)
        {
            using (var filestream = System.IO.File.Create(path))
            {
                var bs = System.Text.Encoding.UTF8.GetBytes(ToJson());
                filestream.Write(bs, 0, bs.Length);
            }
        }

        public static string ToJson()
        {
            Dictionary<string, object> configDict = new Dictionary<string, object>();

            foreach (var configtype in PlatformConfig)
            {
                configDict[configtype.Name] = TypeToDict(configtype);
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
            Dictionary<string, object> dict = JsonConvert.
                DeserializeObject<Dictionary<string, object>>(json);

            foreach (var configtype in PlatformConfig)
            {
                var jobj = dict[configtype.Name] as Newtonsoft.Json.Linq.JObject;
                if (jobj != null)
                {
                    var d = jobj.ToObject<Dictionary<string, object>>();
                    DictToType(d, configtype);
                }
            }
        }

        private static void DictToType(Dictionary<string, object> dict, Type type)
        {
            foreach (var p in type.GetProperties())
            {
                object obj = p.GetValue(null, null);
                Type objtype = obj.GetType();
                if (dict.ContainsKey(p.Name))
                {
                    if (HasConfigAttribute(obj))
                    {
                        var d = ((Newtonsoft.Json.Linq.JObject)dict[p.Name]).ToObject<Dictionary<string, object>>();
                        DictToType(d, objtype);
                    }
                    else
                    {
                        try
                        {
                            object val = dict[p.Name];
                            if (val.GetType() == typeof(System.Int64))
                            {
                                val = Convert.ToInt32(val);
                            }
                            p.SetValue(null, val, null);
                        }
                        catch
                        { }
                    }
                }
            }
        }


        private static Dictionary<string, object> TypeToDict(Type t)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            foreach (var p in t.GetProperties())
            {
                object obj = p.GetValue(null, null);
                Type objtype = obj.GetType();
                if (obj == null)
                {
                    continue;
                }
                else if (HasConfigAttribute(obj))
                {
                    dict[objtype.Name] = TypeToDict(objtype);
                }
                else
                {
                    dict[p.Name] = obj;
                }
            }
            return dict;
        }


        private static bool HasConfigAttribute(object obj)
        {
            foreach (object attr in obj.GetType().GetCustomAttributes(false))
            {
                if (typeof(ConfigGroupAttribute).IsInstanceOfType(attr))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
