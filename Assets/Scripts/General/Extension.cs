using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simuro5v5
{
    /// <summary>
    /// 一些内部类的扩展方法
    /// </summary>
    public static class Extension
    {
        // 从Unity标准规整角度到旧标准
        public static float FormatUnity2Old(this float rot)
        {
            return 90.0f - rot;
        }

        // 从Unity标准规整角度到旧标准
        public static float FormatOld2Unity(this float rot)
        {
            return 90.0f - rot;
        }

        // 规定rot∈(-180, 180]
        // 扩展方法谨慎使用
        // 按照旧的标准规整角度
        public static float FormatOld(this float rot)
        {
            while (rot > 180.0f)
            {
                rot -= 360.0f;
            }

            while (rot < -180.0f)
            {
                rot += 360.0f;
            }

            return rot;
        }
    }
}
