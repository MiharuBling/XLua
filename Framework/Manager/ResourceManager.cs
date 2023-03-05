using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using UObject = UnityEngine.Object;

/// <summary>
/// ��Դ�����ࣺ
/// ����������ʹ��bundle��Ϣ���õ�bundleinfo�࣬�������ڼ���bundleʱʹ�ã�
/// �����ֵ�m_BundleInfos�����Դ�������������Դ��bundle��Ϣ
/// ParseVersionFile�����汾�ļ�����ð汾�ļ�������Ҫ���ص���Դ����
/// �����ڵ�bundle��Ϣ�Լ�����Դ��������Щbundle��������lua�ļ�����Ϣ����luaManager����
/// ������Դ������ǰ��Ҫ�����汾�ļ���LoadBundleAsyncͨ��m_BundleInfos������Դ�ݹ�
/// ����Ϊ�е�bundle�������bundle�����첽����bundle��Դ��
/// ���ڱ༭��ģʽ��ʹ��EditorLoadAsset������Դ
/// ��װ��loadxx�ķ�������������ָ��������Դ������loadUI����UIĿ¼����UI������Դ
/// </summary>
public class ResourceManager : MonoBehaviour
{
    //����һ���࣬���������汾��Ϣ
    internal class BundleInfo
    {
        public string AssetName;
        public string BundleName;
        public List<String> Dependeces;
    }

    internal class BundleData
    {
        public AssetBundle Bundle;

        //���ü���
        public int Count;
        public BundleData(AssetBundle ab)
        {
            Bundle = ab;
            Count = 1;
        }
    }

    //���bundle��Ϣ�ļ���
    private Dictionary<string, BundleInfo> m_BundleInfos = 
        new Dictionary<string, BundleInfo>();

    //��ż��ع���bundle
    //private Dictionary<string, AssetBundle> m_AssetBundles =
    //    new Dictionary<string, AssetBundle>();
    private Dictionary<string, BundleData> m_AssetBundles =
        new Dictionary<string, BundleData>();

    public void ParseVersionFile()
    {
        //�õ��汾�ļ�·��
        string url = Path.Combine(PathUtil.BuildResourcesPath, AppConst.FileListName);
        //���ļ����ж�ȡ
        string[] data = File.ReadAllLines(url);

        //�����ļ���Ϣ
        for (int i = 0; i < data.Length; i++)
        {
            BundleInfo bundleInfo = new BundleInfo();
            string[] info = data[i].Split('|');
            bundleInfo.AssetName = info[0];
            bundleInfo.BundleName = info[1];
            //List���ԣ����������飬���Զ�̬����
            bundleInfo.Dependeces = new List<string>(info.Length - 2);
            for (int j = 2; j < info.Length; j++)
            {
                bundleInfo.Dependeces.Add(info[j]);
            }
            m_BundleInfos.Add(bundleInfo.AssetName, bundleInfo);

            //����LuaScriptes�µ�Lua�ļ�����ӵ�Luamanager��
            if (info[0].IndexOf("LuaScripts") > 0)
            {
                Manager.Lua.LuaNames.Add(info[0]);
            }
        }

    }

    //���Ѿ����ص�Bundle�ֵ��в�ѯ�Ƿ��Ѿ�����
    BundleData GetBundle(string name)
    {
        BundleData bundle = null;
        if (m_AssetBundles.TryGetValue(name, out bundle))
        {
            //�õ���BundleData˵��Ҫʹ����
            bundle.Count++;
            return bundle;
        }
        return null;
    }


    IEnumerator LoadBundleAsync(string assetName, Action<UObject> action = null)
    {
        
        string bundleName = m_BundleInfos[assetName].BundleName;
        //�����Сд��bundle.ab��·����
        string bundlePath = Path.Combine(PathUtil.BuildResourcesPath, bundleName);
        List<string> dependences = m_BundleInfos[assetName].Dependeces;

        //�ж��Ƿ��Ѿ����ع�bundle��
        BundleData bundle = GetBundle(bundleName);
        if (bundle == null)
        {
            //���û��ȡ����Ҫȥ�����ȡ
            UObject obj = Manager.Pool.Spawn("AssetBundle", bundleName);
            if (obj != null)
            {
                AssetBundle ab = obj as AssetBundle;
                bundle = new BundleData(ab);
            }
            else//��������ûȡ������ȥ����Bundle
            {
                //�����첽����budle����
                AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(bundlePath);
                yield return request;
                bundle = new BundleData(request.assetBundle);
            }
            m_AssetBundles.Add(bundleName, bundle);
            
        }
        //�ȼ�����Ҫ��Bundle��Ȼ����������Դ�����������ԴҲж�ص��ˣ��ͼ���������Դ
        if (dependences != null && dependences.Count > 0)
        {
            //�ݹ��������bundle����Ϊ��������ԴĿ¼����bundle��Դ��
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

        //���action==null,˵����������Դ��������Դ����Ҫ����asset,Ҳ����Ҫִ�лص�
        if (action == null)
        {
            yield break;
        }
        //��bundle�������ָ��·�������ļ�������prefab
        AssetBundleRequest bundleRequest = bundle.Bundle.LoadAssetAsync(assetName);
        yield return bundleRequest;

        //����ص���request����Ϊ�գ��﷨��
        action?.Invoke(bundleRequest?.asset);
    }

    

    //ֱ��������첽�ķ����������ṩ�ӿ�ʹ��StartCorountine�����㣬�ṩһ���ӿ�
    public void LoadAsset(string assetName, Action<UObject> action)
    {
        //����Ǳ༭��ģʽ���ͼ���Assets/BuildResources/UI/Prefabs/TestUI.prefab
#if UNITY_EDITOR
        if (AppConst.GameMode == GameMode.EditorMode)
            EditorLoadAsset(assetName, action);
        else//�������StreamingAssets/ui/prefabs/testui.prefab.ab
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

    //��Ϊprefab����̫�࣬���뱣֤�������ľ����ƶ����͵�·��������effect��·����
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

    //��ȥһ��bundle�����ü���
    private void MinusOneBundleCount(string bundleName)
    {
        if (m_AssetBundles.TryGetValue(bundleName, out BundleData bundle))
        {
            if (bundle.Count > 0)
            {
                bundle.Count--;
                Debug.Log("bundle���ü�����" + bundleName + "count:" + bundle.Count);
            }

            if (bundle.Count <= 0)
            {
                Debug.Log("����bundle����أ�" + bundleName);
                Manager.Pool.UnSpawn("AssetBundle", bundleName, bundle.Bundle);
                m_AssetBundles.Remove(bundleName);
            }
        }
    }

    //��ȥBundle��������Դ�����ü���
    public void MinusBundleCount(string assetName)
    {
        //�ȼ��������bundle���ü���
        string bundleName = m_BundleInfos[assetName].BundleName;
        MinusOneBundleCount(bundleName);

        //������Դ �ڲ���������Դ ����������Դ�����ü���
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

    //ж��assetbundle��Դ
    public void UnloadBundle(UObject obj)
    {
        AssetBundle ab = obj as AssetBundle;
        ab.Unload(true);
    }

#if UNITY_EDITOR
    /// <summary>
    /// �ڱ༭��������ʹ�ã���Ȼ��ͬ�����أ�����ģ���ǰ����첽���غ���
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
