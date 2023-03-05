using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 路径工具类：
/// 1定义了所有用到的路径
/// 2返回标准路径或返回Unity下的几个文件夹的相对路径
/// </summary>
public class PathUtil
{
    //为什么要把Application定义出来，因为每一次访问都需要GC一次，定义出来就访问一次
    public static readonly string AssetPath = Application.dataPath;

    //只读的，需要打Bundle的目录
    public static readonly string BuildResourcesPath = AssetPath + "/BuildResources/";

    //Bundle输出目录
    public static readonly string BundleOutPath = Application.streamingAssetsPath;

    public static readonly string LuaPath = "Assets/BuildResources/LuaScripts/";
    public static string BundleResourcePath
    {
        get
        {
            //直接定义好的路径，避免使用Application,减少GC
            if (AppConst.GameMode == GameMode.UpdateMode)
                return ReadWritePath;
            return ReadPath;
        }
    }

    //只读目录
    public static readonly string ReadPath = Application.streamingAssetsPath;

    //可读写目录
    public static readonly string ReadWritePath = Application.persistentDataPath;

    


    /// <summary>
    /// 获取Unity的相对路径
    /// </summary>
    /// <param name="path">绝对路径</param>
    /// <returns></returns>
    public static string GetUnityPath(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return string.Empty;
        }
        //从Assets位置拿到相对目录
        return path.Substring(path.IndexOf("Assets"));
    }

    /// <summary>
    /// 获取标准路径
    /// </summary>
    /// <param name="path">路径</param>
    /// <returns></returns>
    public static string GetStandardPath(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return string.Empty;
        }
        //先处理空格，在处理反斜杠
        return path.Trim().Replace("\\", "/");
    }
    public static string GetLuaPath(string name)
    {
        return string.Format("Assets/BuildResources/Sprites/{0}", name);
    }
    public static string GetUIPath(string name)
    {
        return string.Format("Assets/BuildResources/UI/Prefabs/{0}.prefab", name);
    }
    public static string GetMusicPath(string name)
    {
        return string.Format("Assets/BuildResources/Audio/Music/{0}", name);
    }
    public static string GetSoundPath(string name)
    {
        return string.Format("Assets/BuildResources/Audio/Sound/{0}", name);
    }
    public static string GetEffectPath(string name)
    {
        return string.Format("Assets/BuildResources/Effect/Prefabs/{0}.prefab", name);
    }
    public static string GetModelPath(string name)
    {
        return string.Format("Assets/BuildResources/Model/Prefabs/{0}.prefab", name);
    }
    public static string GetSpritePath(string name)
    {
        return string.Format("Assets/BuildResources/Sprite/{0}", name);
    }
    public static string GetScenePath(string name)
    {
        return string.Format("Assets/BuildResources/Scenes/{0}.unity", name);
    }
}
