using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    //一个参数给lua使用,lua的多个参数可以封装成table传进来
    public delegate void EventHandler(object args);

    private Dictionary<int, EventHandler> m_Events =
        new Dictionary<int, EventHandler>();

    /// <summary>
    /// 订阅事件
    /// </summary>
    /// <param name="id"></param>
    /// <param name="e"></param>
    public void Subscribe(int id, EventHandler e)
    {
        if (m_Events.ContainsKey(id))
            //相当于实现多播委托
            m_Events[id] += e;
        else
            m_Events.Add(id, e);
    }

    /// <summary>
    /// 取消订阅事件
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
    /// 执行事件
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
