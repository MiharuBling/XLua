using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    ////��������Ϊkey��������Ϊvalue  ����UI 
    //Dictionary<string, GameObject> m_UI = new Dictionary<string, GameObject>();

    //UI����
    Dictionary<string, Transform> m_UIGroups = new Dictionary<string, Transform>();

    private Transform m_UIParent;

    private void Awake()
    {
        m_UIParent = this.transform.parent.Find("UI");
    }

    /// <summary>
    /// ��Lua�ṩ�ӿڣ�������ӷ���("��һ����" "��������"),�����ȸ���Canvas UI�ڵ���Ӱ���UI�㼶��
    /// </summary>
    /// <param name="group">Ҫ��ӵ�UI�㼶���Ƶ�List</param>
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
    /// ����ָ���㼶��transform
    /// </summary>
    /// <param name="group">ui�㼶����</param>
    /// <returns>�����ֵ��ж�Ӧ�㼶���Ƶ�transform</returns>
    
    Transform GetUIGroup(string group)
    {
        if (!m_UIGroups.ContainsKey(group))
        {
            Debug.LogError("group is not exist");
        }
        return m_UIGroups[group];
    }

    /// <summary>
    /// ����һ��ui���ֺ�lua���֣��Զ���uiԤ�����C#�ű����Զ�����lua�ű�
    /// </summary>
    /// <param name="uiName">ui����</param>
    /// <param name="luaName">lua����</param>
    public void OpenUI(string uiName, string group, string luaName) 
    {
        GameObject ui = null;
        //����ui�ɹ���,���ø��ڵ�
        Transform parent = GetUIGroup(group);

        //ȥ����ز���,�����ui��ֱ�Ӽ���,û����ȥLoad
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
 
            //��UIԤ�����UILogic��C#�ű�
            UILogic uiLogic = ui.AddComponent<UILogic>();
            uiLogic.AssetName = uiPath;
            //��ʼ�����Lua�ű�(Awake)
            uiLogic.Init(luaName);
            //UI��Start
            uiLogic.OnOpen();
        });
    }
}
