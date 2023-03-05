using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectPool : PoolBase
{
    //取物体:在池子中查找,如果有物体就激活,没有就返回Null
    public override Object Spawn(string name)
    {
        Object obj = base.Spawn(name);
        if (obj == null)
        {
            return null;
        }

        GameObject go = obj as GameObject;
        go.SetActive(true);
        return obj;
    }

    //存物体:把物体设置非激活,然后放进池子回收
    public override void UnSpawn(string name, Object obj)
    {
        GameObject go = obj as GameObject;
        go?.SetActive(false);
        go.transform.SetParent(this.transform, false);
        base.UnSpawn(name, obj);
    }

    public override void Release()
    {
        base.Release();
        foreach (PoolObject item in m_Objects)
        {
            if (System.DateTime.Now.Ticks - item.LastUseTime.Ticks >= 
                m_LastReleaseTime * 10000000)
            {
                Debug.Log("GameObjectPool release time:" + System.DateTime.Now);
                Destroy(item.Object);
                Manager.Resource.MinusBundleCount(item.Name);
                m_Objects.Remove(item);
                //因为删除了List之后，不能用迭代器了，需要递归一下跳出
                Release();
                return;
            }
        }
    }
}
