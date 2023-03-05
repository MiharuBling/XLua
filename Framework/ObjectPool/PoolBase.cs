using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolBase : MonoBehaviour
{
    //�Զ��ͷ�ʱ��
    protected float m_ReleaseTime;

    //�ϴ��ͷ���Դ��ʱ��/��΢��1(��)=10000000(��΢��)
    protected long m_LastReleaseTime = 0;

    //�����ض����
    protected List<PoolObject> m_Objects;

    public void Start()
    {
        m_LastReleaseTime = System.DateTime.Now.Ticks;
    }

    //��ʼ�������
    public void Init(float time)
    {
        m_ReleaseTime = time;
        m_Objects = new List<PoolObject>();
    }

    //ȡ������
    public virtual Object Spawn(string name)
    {
        foreach (PoolObject po in m_Objects)
        {
            if (po.Name == name)
            {
                //���ڳ��������һ���ǻ�û���õ��ģ��õ��ľ��ó���
                m_Objects.Remove(po);
                return po.Object;
            }
        }
        return null;
    }

    //�ջض���
    public virtual void UnSpawn(string name, Object obj)
    {
        PoolObject po = new PoolObject(name, obj);
        m_Objects.Add(po);
    }

    //�ͷ�
    public virtual void Release()
    { 
    }

    private void Update()
    {
        //������Ҫ������ʱ��ִ��һ��Release,���ǵ����ͷ�û�У���Ҫ���ݼ̳и���
        //��������ȷ�������ڲ���ʱ���ͷŵ�������ʱ��
        if (System.DateTime.Now.Ticks - m_LastReleaseTime >= m_ReleaseTime * 10000000)
        {
            //ˢ��ʱ��
            m_LastReleaseTime = System.DateTime.Now.Ticks;
            Release();
        }
    }
}
