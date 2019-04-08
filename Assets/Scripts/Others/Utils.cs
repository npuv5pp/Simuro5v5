using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

static class Utils
{
    public static string GetExceptionFullStackTrace(System.Exception exception)
    {
        var stringBuilder = new StringBuilder();
        while (exception != null)
        {
            stringBuilder.Append(exception.Message);
            stringBuilder.Append(exception.StackTrace);

            exception = exception.InnerException;
        }

        return stringBuilder.ToString();
    }

    /// <summary>
    /// 转义命令行中的路径，避免空格影响到命令行的传参
    /// </summary>
    /// <param name="path">路径</param>
    /// <returns>转义后的路径</returns>
    public static string FixCmdPath(string path)
    {
        return "\"" + path + "\"";
    }

    public static void ForEach<T>(this IEnumerable<T> list, Action<T> action)
    {
        foreach (T item in list)
            action(item);
    }
}
