using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 运行时，在pool下创建一个poolName的子物体，挂载脚本，AssetBundle挂载AssetPool，
/// GameObject挂载GameObjectPool脚本，挂载的脚本就是对象池，先Init初始化对象池，
/// 然后将这个对象池添加到PoolManager中进行管理。
/// 通过PoolManager的m_Pools管理里面的对象池，分别取出和放回对象
/// </summary>
public class PoolManager : MonoBehaviour
{
    //对象池父节点
    private Transform m_PoolParent;

    //对象池字典
    private Dictionary<string, PoolBase> m_Pools =
        new Dictionary<string, PoolBase>();

    private void Awake()
    {
        m_PoolParent = this.transform.parent.Find("Pool");
    }

    //创建对象池，T必须是继承PoolBase的类型
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

    //创建物体对象池
    public void CreateGameObjectPool(string poolName, float releaseTime)
    {
        CreatePool<GameObjectPool>(poolName, releaseTime);
    }

    //创建资源对象池
    public void CreateAssetPool(string poolName, float releaseTime)
    {
        CreatePool<AssetPool>(poolName, releaseTime);
    }

    //取出对象
    public Object Spawn(string poolName, string assetName)
    {
        if (m_Pools.TryGetValue(poolName, out PoolBase pool))
        {
            return pool.Spawn(assetName);
        }
        return null;
    }

    //回收对象
    public void UnSpawn(string poolName, string assetName, Object asset)
    {
        if (m_Pools.TryGetValue(poolName, out PoolBase pool))
        {
            pool.UnSpawn(assetName, asset);
        }
    }
}
