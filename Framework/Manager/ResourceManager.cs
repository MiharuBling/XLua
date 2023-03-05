using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using UObject = UnityEngine.Object;

/// <summary>
/// 资源管理类：
/// 定义了所有使用bundle信息可用的bundleinfo类，该类用于加载bundle时使用，
/// 另外字典m_BundleInfos存放资源名与所保存该资源的bundle信息
/// ParseVersionFile解析版本文件：获得版本文件中所有要加载的资源名和
/// 其所在的bundle信息以及该资源又依赖哪些bundle，，并把lua文件的信息传给luaManager保存
/// 加载资源：加载前需要解析版本文件，LoadBundleAsync通过m_BundleInfos查找资源递归
/// （因为有的bundle依赖别的bundle）的异步加载bundle资源，
/// 或在编辑器模式下使用EditorLoadAsset加载资源
/// 封装了loadxx的方法，用来加载指定类型资源，例如loadUI，从UI目录加载UI类型资源
/// </summary>
public class ResourceManager : MonoBehaviour
{
    //定义一个类，用来解析版本信息
    internal class BundleInfo
    {
        public string AssetName;
        public string BundleName;
        public List<String> Dependeces;
    }

    internal class BundleData
    {
        public AssetBundle Bundle;

        //引用计数
        public int Count;
        public BundleData(AssetBundle ab)
        {
            Bundle = ab;
            Count = 1;
        }
    }

    //存放bundle信息的集合
    private Dictionary<string, BundleInfo> m_BundleInfos = 
        new Dictionary<string, BundleInfo>();

    //存放加载过的bundle
    //private Dictionary<string, AssetBundle> m_AssetBundles =
    //    new Dictionary<string, AssetBundle>();
    private Dictionary<string, BundleData> m_AssetBundles =
        new Dictionary<string, BundleData>();

    public void ParseVersionFile()
    {
        //拿到版本文件路径
        string url = Path.Combine(PathUtil.BuildResourcesPath, AppConst.FileListName);
        //对文件进行读取
        string[] data = File.ReadAllLines(url);

        //解析文件信息
        for (int i = 0; i < data.Length; i++)
        {
            BundleInfo bundleInfo = new BundleInfo();
            string[] info = data[i].Split('|');
            bundleInfo.AssetName = info[0];
            bundleInfo.BundleName = info[1];
            //List特性，本质是数组，可以动态扩容
            bundleInfo.Dependeces = new List<string>(info.Length - 2);
            for (int j = 2; j < info.Length; j++)
            {
                bundleInfo.Dependeces.Add(info[j]);
            }
            m_BundleInfos.Add(bundleInfo.AssetName, bundleInfo);

            //查找LuaScriptes下的Lua文件，添加到Luamanager中
            if (info[0].IndexOf("LuaScripts") > 0)
            {
                Manager.Lua.LuaNames.Add(info[0]);
            }
        }

    }

    //从已经加载的Bundle字典中查询是否已经有了
    BundleData GetBundle(string name)
    {
        BundleData bundle = null;
        if (m_AssetBundles.TryGetValue(name, out bundle))
        {
            //拿到了BundleData说明要使用了
            bundle.Count++;
            return bundle;
        }
        return null;
    }


    IEnumerator LoadBundleAsync(string assetName, Action<UObject> action = null)
    {
        
        string bundleName = m_BundleInfos[assetName].BundleName;
        //这个是小写的bundle.ab的路径名
        string bundlePath = Path.Combine(PathUtil.BuildResourcesPath, bundleName);
        List<string> dependences = m_BundleInfos[assetName].Dependeces;

        //判断是否已经加载过bundle了
        BundleData bundle = GetBundle(bundleName);
        if (bundle == null)
        {
            //如果没有取到就要去对象池取
            UObject obj = Manager.Pool.Spawn("AssetBundle", bundleName);
            if (obj != null)
            {
                AssetBundle ab = obj as AssetBundle;
                bundle = new BundleData(ab);
            }
            else//如果对象池没取到，就去加载Bundle
            {
                //创建异步加载budle申请
                AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(bundlePath);
                yield return request;
                bundle = new BundleData(request.assetBundle);
            }
            m_AssetBundles.Add(bundleName, bundle);
            
        }
        //先加载需要的Bundle，然后检测依赖资源，如果依赖资源也卸载掉了，就加载依赖资源
        if (dependences != null && dependences.Count > 0)
        {
            //递归加载依赖bundle，因为依赖的资源目录就是bundle资源名
            for (int i = 0; i < dependences.Count; i++)
            {
                yield return LoadBundleAsync(dependences[i]);
            }
        }

        if (assetName.EndsWith(".unity"))
        {
            action?.Invoke(null);
            yield break;
        }

        //如果action==null,说明是依赖资源，依赖资源不需要加载asset,也不需要执行回调
        if (action == null)
        {
            yield break;
        }
        //从bundle申请加载指定路径名的文件，例如prefab
        AssetBundleRequest bundleRequest = bundle.Bundle.LoadAssetAsync(assetName);
        yield return bundleRequest;

        //如果回调和request都不为空，语法糖
        action?.Invoke(bundleRequest?.asset);
    }

    

    //直接上面的异步的方法向外面提供接口使用StartCorountine不方便，提供一个接口
    public void LoadAsset(string assetName, Action<UObject> action)
    {
        //如果是编辑器模式，就加载Assets/BuildResources/UI/Prefabs/TestUI.prefab
#if UNITY_EDITOR
        if (AppConst.GameMode == GameMode.EditorMode)
            EditorLoadAsset(assetName, action);
        else//否则加载StreamingAssets/ui/prefabs/testui.prefab.ab
#endif
            StartCoroutine(LoadBundleAsync(assetName, action));
    }

    public void LoadLua(string assetName, Action<UnityEngine.Object> action = null)
    {
        LoadAsset(assetName, action);
    }

    public void LoadUI(string assetName, Action<UnityEngine.Object> action = null)
    {
        LoadAsset(assetName, action);
    }

    //因为prefab类型太多，必须保证传进来的就是制定类型的路径，例如effect的路径等
    public void LoadPrefab(string assetName, Action<UnityEngine.Object> action = null)
    {
        LoadAsset(assetName, action);
    }

    public void LoadScene(string assetName, Action<UnityEngine.Object> action = null)
    {
        LoadAsset(assetName, action);
    }

    public void LoadMusic(string assetName, Action<UnityEngine.Object> action = null)
    {
        LoadAsset(assetName, action);
    }
    private void Start()
    {
        //ParseVersionFile();
        //LoadAsset("AssAssets/BuildResources/UI/Prefabs/TestUI.prefab", OnComplete);

    }

    public void UnloadBundle(string name)
    {
        
    }

    //减去一个bundle的引用计数
    private void MinusOneBundleCount(string bundleName)
    {
        if (m_AssetBundles.TryGetValue(bundleName, out BundleData bundle))
        {
            if (bundle.Count > 0)
            {
                bundle.Count--;
                Debug.Log("bundle引用计数：" + bundleName + "count:" + bundle.Count);
            }

            if (bundle.Count <= 0)
            {
                Debug.Log("放入bundle对象池：" + bundleName);
                Manager.Pool.UnSpawn("AssetBundle", bundleName, bundle.Bundle);
                m_AssetBundles.Remove(bundleName);
            }
        }
    }

    //减去Bundle和依赖资源的引用计数
    public void MinusBundleCount(string assetName)
    {
        //先减掉自身的bundle引用计数
        string bundleName = m_BundleInfos[assetName].BundleName;
        MinusOneBundleCount(bundleName);

        //依赖资源 在查找依赖资源 减掉依赖资源的引用计数
        List<string> dependences = m_BundleInfos[assetName].Dependeces;
        if (dependences != null)
        {
            foreach (string dependence in dependences)
            {
                string name = m_BundleInfos[dependence].BundleName;
                MinusOneBundleCount(name);
            }
        }
    }

    //卸载assetbundle资源
    public void UnloadBundle(UObject obj)
    {
        AssetBundle ab = obj as AssetBundle;
        ab.Unload(true);
    }

#if UNITY_EDITOR
    /// <summary>
    /// 在编辑器环境下使用，虽然是同步加载，但是模拟成前面的异步加载函数
    /// </summary>
    /// <param name="assetName"></param>
    /// <param name="action"></param>
    void EditorLoadAsset(string assetName, Action<UObject> action = null)
    {
        UObject obj = UnityEditor.AssetDatabase.LoadAssetAtPath(assetName, typeof(UObject));
        if (obj = null)
        {
            Debug.LogError("asset name is not exist:" + assetName);
            action?.Invoke(obj);
        }
    }
#endif


}
