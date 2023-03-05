using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    ////用名字作为key，对象作为value  缓存UI 
    //Dictionary<string, GameObject> m_UI = new Dictionary<string, GameObject>();

    //UI分组
    Dictionary<string, Transform> m_UIGroups = new Dictionary<string, Transform>();

    private Transform m_UIParent;

    private void Awake()
    {
        m_UIParent = this.transform.parent.Find("UI");
    }

    /// <summary>
    /// 给Lua提供接口，方便添加分组("第一界面" "二级弹窗"),用于热更给Canvas UI节点添加包含UI层级组
    /// </summary>
    /// <param name="group">要添加的UI层级名称的List</param>
    public void SetUIGroup(List<string> group)
    {
        for (int i = 0; i < group.Count; i++)
        {
            GameObject go = new GameObject("Group-" + group[i]);
            go.transform.SetParent(m_UIParent, false);
            m_UIGroups.Add(group[i], go.transform);
        }
    }


    /// <summary>
    /// 返回指定层级的transform
    /// </summary>
    /// <param name="group">ui层级名称</param>
    /// <returns>返回字典中对应层级名称的transform</returns>
    
    Transform GetUIGroup(string group)
    {
        if (!m_UIGroups.ContainsKey(group))
        {
            Debug.LogError("group is not exist");
        }
        return m_UIGroups[group];
    }

    /// <summary>
    /// 传入一个ui名字和lua名字，自动给ui预设体绑定C#脚本，自动运行lua脚本
    /// </summary>
    /// <param name="uiName">ui名字</param>
    /// <param name="luaName">lua名字</param>
    public void OpenUI(string uiName, string group, string luaName) 
    {
        GameObject ui = null;
        //加载ui成功后,设置父节点
        Transform parent = GetUIGroup(group);

        //去对象池查找,如果有ui就直接加载,没有再去Load
        string uiPath = PathUtil.GetUIPath(uiName);
        Object uiObj = Manager.Pool.Spawn("UI", uiPath);
        if (uiObj != null)
        {
            ui = uiObj as GameObject;
            ui.transform.SetParent(parent, false);
            UILogic uiLogic = ui.GetComponent<UILogic>();
            uiLogic.OnOpen();
            return;
        }


        Manager.Resource.LoadUI(uiName, (UnityEngine.Object obj) =>
        {
            ui = Instantiate(obj) as GameObject;
            ui.transform.SetParent(parent, false);
            //m_UI.Add(uiName, ui);

            if (ui.name.Contains("TestUI"))
            {
                UISelet uiSelect = ui.transform.GetChild(0).GetChild(1).gameObject.AddComponent<UISelect>();
                uiSelect.Init("ui.SelectMenuUI");
                uiSelect.OnOpen();

                OptionsSelect optionsSelect = ui.transform.GetChild(1).gameObject.AddComponent<OptionsSelect>();
                optionsSelect.Init("ui.SelectOptions");
                optionsSelect.OnOpen();
            }
 
            //给UI预制体绑定UILogic的C#脚本
            UILogic uiLogic = ui.AddComponent<UILogic>();
            uiLogic.AssetName = uiPath;
            //初始化这个Lua脚本(Awake)
            uiLogic.Init(luaName);
            //UI的Start
            uiLogic.OnOpen();
        });
    }
}
