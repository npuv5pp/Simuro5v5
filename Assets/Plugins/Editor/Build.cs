using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class BuildBatch
{
    private static void BuildGame(BuildTarget target, string path = null)
    {
        string truePath = path ?? EditorUtility.SaveFolderPanel("Choose Build Location", "", "");
        truePath = Path.Combine(truePath, "Simuro5v5.exe");
        var scenes =
            EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path).ToArray();
        var options = new BuildOptions();
        BuildPipeline.BuildPlayer(scenes, truePath, target, options);
    }
    
    [MenuItem("File/Batch Build/Win32")]
    public static void BuildWindows32()
    {
        BuildGame(BuildTarget.StandaloneWindows);
    }
    
    [MenuItem("File/Batch Build/Win64")]
    public static void BuildWindows64()
    {
        BuildGame(BuildTarget.StandaloneWindows64);
    }

    [MenuItem("File/Batch Build/Win32 and Win64")]
    public static void BuildWindows32And64()
    {
        string path = EditorUtility.SaveFolderPanel("Choose Build Location", "", "");
        string win32Path = Path.Combine(path, "x86");
        string win64Path = Path.Combine(path, "x64");
        BuildGame(BuildTarget.StandaloneWindows, win32Path);
        BuildGame(BuildTarget.StandaloneWindows64, win64Path);
    }
}
