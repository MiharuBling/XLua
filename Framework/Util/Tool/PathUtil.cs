using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ·�������ࣺ
/// 1�����������õ���·��
/// 2���ر�׼·���򷵻�Unity�µļ����ļ��е����·��
/// </summary>
public class PathUtil
{
    //ΪʲôҪ��Application�����������Ϊÿһ�η��ʶ���ҪGCһ�Σ���������ͷ���һ��
    public static readonly string AssetPath = Application.dataPath;

    //ֻ���ģ���Ҫ��Bundle��Ŀ¼
    public static readonly string BuildResourcesPath = AssetPath + "/BuildResources/";

    //Bundle���Ŀ¼
    public static readonly string BundleOutPath = Application.streamingAssetsPath;

    public static readonly string LuaPath = "Assets/BuildResources/LuaScripts/";
    public static string BundleResourcePath
    {
        get
        {
            //ֱ�Ӷ���õ�·��������ʹ��Application,����GC
            if (AppConst.GameMode == GameMode.UpdateMode)
                return ReadWritePath;
            return ReadPath;
        }
    }

    //ֻ��Ŀ¼
    public static readonly string ReadPath = Application.streamingAssetsPath;

    //�ɶ�дĿ¼
    public static readonly string ReadWritePath = Application.persistentDataPath;

    


    /// <summary>
    /// ��ȡUnity�����·��
    /// </summary>
    /// <param name="path">����·��</param>
    /// <returns></returns>
    public static string GetUnityPath(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return string.Empty;
        }
        //��Assetsλ���õ����Ŀ¼
        return path.Substring(path.IndexOf("Assets"));
    }

    /// <summary>
    /// ��ȡ��׼·��
    /// </summary>
    /// <param name="path">·��</param>
    /// <returns></returns>
    public static string GetStandardPath(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return string.Empty;
        }
        //�ȴ���ո��ڴ���б��
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
