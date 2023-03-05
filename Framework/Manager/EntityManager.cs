using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityManager : MonoBehaviour
{
    //缓存实体
    Dictionary<string, GameObject> m_Entities = new Dictionary<string, GameObject>();
    //缓存实体分组的transform
    Dictionary<string, Transform> m_Groups = new Dictionary<string, Transform>();

    private Transform m_EntityParent;

    private void Awake()
    {
        m_EntityParent = this.transform.parent.Find("Entity");
    }

    /// <summary>
    /// 给lua提供接口，方便添加实体分组
    /// </summary>
    /// <param name="groups">要添加的UI层级名称的List</param>
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
    /// 传入一个实体名字和lua名字，自动给ui预设体绑定C#脚本，自动执行Lua脚本
    /// </summary>
    /// <param name="name">实体名字</param>
    /// <param name="group"></param>
    /// <param name="luaName">lua名字</param>
    public void ShowEntity(string name, string group, string luaName)
    {
        GameObject entity = null;
        //如果prefab已经加载过了(从ab包取出放到Dictionary中),就只执行OnShow(Start),
        //不执行Init(Awake)
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
            entityLogic.Init(luaName);//Init对应Awake，OnShow对应Start
            entityLogic.OnShow();
        });
    }
}
