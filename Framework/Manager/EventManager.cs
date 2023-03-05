using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    //һ��������luaʹ��,lua�Ķ���������Է�װ��table������
    public delegate void EventHandler(object args);

    private Dictionary<int, EventHandler> m_Events =
        new Dictionary<int, EventHandler>();

    /// <summary>
    /// �����¼�
    /// </summary>
    /// <param name="id"></param>
    /// <param name="e"></param>
    public void Subscribe(int id, EventHandler e)
    {
        if (m_Events.ContainsKey(id))
            //�൱��ʵ�ֶಥί��
            m_Events[id] += e;
        else
            m_Events.Add(id, e);
    }

    /// <summary>
    /// ȡ�������¼�
    /// </summary>
    /// <param name="id"></param>
    /// <param name="e"></param>
    public void UnSubscribe(int id, EventHandler e)
    {
        if (m_Events.ContainsKey(id))
        {
            if (m_Events[id] != null)
                m_Events[id] -= e;
        }
        else
        {
            if (m_Events[id] == null)
                m_Events.Remove(id);
        }
    }

    /// <summary>
    /// ִ���¼�
    /// </summary>
    /// <param name="id"></param>
    /// <param name="args"></param>
    public void Fire(int id, object args = null)
    {
        EventHandler hander;
        if (m_Events.TryGetValue(id, out hander))
        {
            hander(args);
        }
    }
}
