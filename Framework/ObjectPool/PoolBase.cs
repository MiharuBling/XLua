using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolBase : MonoBehaviour
{
    //自动释放时间
    protected float m_ReleaseTime;

    //上次释放资源的时间/毫微秒1(秒)=10000000(毫微秒)
    protected long m_LastReleaseTime = 0;

    //真正地对象池
    protected List<PoolObject> m_Objects;

    public void Start()
    {
        m_LastReleaseTime = System.DateTime.Now.Ticks;
    }

    //初始化对象池
    public void Init(float time)
    {
        m_ReleaseTime = time;
        m_Objects = new List<PoolObject>();
    }

    //取出对象
    public virtual Object Spawn(string name)
    {
        foreach (PoolObject po in m_Objects)
        {
            if (po.Name == name)
            {
                //放在池子里面的一定是还没有用到的，用到的就拿出来
                m_Objects.Remove(po);
                return po.Object;
            }
        }
        return null;
    }

    //收回对象
    public virtual void UnSpawn(string name, Object obj)
    {
        PoolObject po = new PoolObject(name, obj);
        m_Objects.Add(po);
    }

    //释放
    public virtual void Release()
    { 
    }

    private void Update()
    {
        //父类需要按给定时间执行一次Release,但是到底释放没有，需要根据继承父类
        //的子类来确定，其内部的时间释放到了销毁时间
        if (System.DateTime.Now.Ticks - m_LastReleaseTime >= m_ReleaseTime * 10000000)
        {
            //刷新时间
            m_LastReleaseTime = System.DateTime.Now.Ticks;
            Release();
        }
    }
}
