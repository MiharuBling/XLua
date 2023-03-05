using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityManager : MonoBehaviour
{
    //����ʵ��
    Dictionary<string, GameObject> m_Entities = new Dictionary<string, GameObject>();
    //����ʵ������transform
    Dictionary<string, Transform> m_Groups = new Dictionary<string, Transform>();

    private Transform m_EntityParent;

    private void Awake()
    {
        m_EntityParent = this.transform.parent.Find("Entity");
    }

    /// <summary>
    /// ��lua�ṩ�ӿڣ��������ʵ�����
    /// </summary>
    /// <param name="groups">Ҫ��ӵ�UI�㼶���Ƶ�List</param>
    public void SetEntityGroup(List<string> groups)
    {
        for (int i = 0; i < groups.Count; i++)
        {
            GameObject group = new GameObject("Group-" + groups[i]);
            group.transform.SetParent(m_EntityParent, false);
            m_Groups[groups[i]] = group.transform;
        }
    }

    Transform GetGroup(string group)
    {
        if (!m_Groups.ContainsKey(group))
        {
            Debug.LogError("group is not exist");
        }
        return m_Groups[group];
    }

    /// <summary>
    /// ����һ��ʵ�����ֺ�lua���֣��Զ���uiԤ�����C#�ű����Զ�ִ��Lua�ű�
    /// </summary>
    /// <param name="name">ʵ������</param>
    /// <param name="group"></param>
    /// <param name="luaName">lua����</param>
    public void ShowEntity(string name, string group, string luaName)
    {
        GameObject entity = null;
        //���prefab�Ѿ����ع���(��ab��ȡ���ŵ�Dictionary��),��ִֻ��OnShow(Start),
        //��ִ��Init(Awake)
        if (m_Entities.TryGetValue(name, out entity))
        {
            EntityLogic logic = entity.GetComponent<EntityLogic>();
            logic.OnShow();
            return;
        }

        Manager.Resource.LoadPrefab(name, (UnityEngine.Object obj) =>
        {
            entity = Instantiate(obj) as GameObject;
            Transform parent = GetGroup(group);
            entity.transform.SetParent(parent, false);
            m_Entities.Add(name, entity);
            EntityLogic entityLogic = entity.GetComponent<EntityLogic>();
            entityLogic.Init(luaName);//Init��ӦAwake��OnShow��ӦStart
            entityLogic.OnShow();
        });
    }
}
