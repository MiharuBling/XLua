using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ����ʱ����pool�´���һ��poolName�������壬���ؽű���AssetBundle����AssetPool��
/// GameObject����GameObjectPool�ű������صĽű����Ƕ���أ���Init��ʼ������أ�
/// Ȼ������������ӵ�PoolManager�н��й���
/// ͨ��PoolManager��m_Pools��������Ķ���أ��ֱ�ȡ���ͷŻض���
/// </summary>
public class PoolManager : MonoBehaviour
{
    //����ظ��ڵ�
    private Transform m_PoolParent;

    //������ֵ�
    private Dictionary<string, PoolBase> m_Pools =
        new Dictionary<string, PoolBase>();

    private void Awake()
    {
        m_PoolParent = this.transform.parent.Find("Pool");
    }

    //��������أ�T�����Ǽ̳�PoolBase������
    private void CreatePool<T>(string poolName, float releaseTime) where T : PoolBase
    {
        if (!m_Pools.TryGetValue(poolName, out PoolBase pool))
        {
            GameObject go = new GameObject(poolName);
            go.transform.SetParent(m_PoolParent);
            pool.Init(releaseTime);
            m_Pools.Add(poolName, pool);
        }
    }

    //������������
    public void CreateGameObjectPool(string poolName, float releaseTime)
    {
        CreatePool<GameObjectPool>(poolName, releaseTime);
    }

    //������Դ�����
    public void CreateAssetPool(string poolName, float releaseTime)
    {
        CreatePool<AssetPool>(poolName, releaseTime);
    }

    //ȡ������
    public Object Spawn(string poolName, string assetName)
    {
        if (m_Pools.TryGetValue(poolName, out PoolBase pool))
        {
            return pool.Spawn(assetName);
        }
        return null;
    }

    //���ն���
    public void UnSpawn(string poolName, string assetName, Object asset)
    {
        if (m_Pools.TryGetValue(poolName, out PoolBase pool))
        {
            pool.UnSpawn(assetName, asset);
        }
    }
}
