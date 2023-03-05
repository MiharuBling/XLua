using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using XLua;

/// <summary>
/// LuaNames��ResourceManager�н����汾�ļ�ParseVersionFileʱ��ֵ�ģ�����LuaNames��
/// ��Bundle����Editor���أ�����LoadLua�����غþͷ���m_LuaScripts��
/// ȫ�����������LuaNames
/// ��ʼ��Init�����һ���ص�������new LuaEnv()������������LuaEnv.AddLoader(loader)
/// (�ⲿStartLua����DoString��lua��loader����GetLuaScript�ҵ�lua�ű�)��
/// ��ģʽ����lua(bundle����editor)���õ�ǰ����ȫ���������ˣ�ִ���˻ص�֪ͨ������ϣ��ſ��Ե��á�
/// ��InitOk
/// ���ã�1.Init��2.StartLua���ڲ���LuaEnv.DoStringloader����GetLuaScript�ҵ�lua�ű�����
/// 3.���ú���XLua.LuaFunction func = Global.Get<xxx>("xxx");func.Call();
/// </summary>
public class LuaManager : MonoBehaviour
{
    //���е�Lua�ļ�������ȡ����Lua��Ȼ�����Ԥ���أ�ResourceManager����Lua�ļ��Ž���
    public List<string> LuaNames = new List<string>();

    //����Lua�ű�����
    private Dictionary<string, byte[]> m_LuaScripts;

    //����һ��Lua����������ıȽϴ�ȫ��ֻ��Ҫһ������Ҫusing XLua��
    public LuaEnv LuaEnv;


    //�����Editorģʽ�£�ֱ�Ӵ�Luapath�Ͱ�����Lua��ȡ���ֵ����ˣ�Ȼ���ٵ��ã�
    //����ͬ�����غ�ͬ��ʹ�õ����������������ģʽ�£���Ҫ��bundle�첽����Lua
    //����ȴ�ʱStart�͵����ˣ������첽����ͬ��ʹ�õ��������ҪԤ����
    //��Ҫ����һ���ص�֪ͨ
    public void Init()
    {
        //��ʼ�������
        LuaEnv = new LuaEnv();
        //�ⲿ����requireʱ�����Զ�����Loader����ȡ�ļ�
        LuaEnv.AddLoader(Loader);

        m_LuaScripts = new Dictionary<string, byte[]>();

#if UNITY_EDITOR
        if (AppConst.GameMode == GameMode.EditorMode)
            EditorLoadLuaScript();
        else
#endif
            LoadLuaScript();

    }

    /// <summary>
    /// ������Ӧ�ű���Lua�ļ���ʵ�����ǰ������ļ���string����Loaderȥ����
    /// Ȼ��Loaderͨ��GetLuaScript���س���Lua
    /// </summary>
    /// <param name="name"></param>
    public void StartLua(string name)
    {
        LuaEnv.DoString(string.Format("require'{0}'", name));
    }

    /// <summary>
    /// Lua�������require����Ĳ����ᴫ��name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    byte[] Loader(ref string name)
    {
        return GetLuaScript(name);
    }

    /// <summary>
    /// ��Loader���ʹ���ҵ�Ҫ���õ�ָ��Ŀ¼Lua�ļ�
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public byte[] GetLuaScript(string name)
    {
        //ΪʲôҪ�滻��.����/,��Ϊһ��ʹ��require ui.Login.register
        name = name.Replace(".", "/");
        //�Զ����Lua��׺����.bytes
        string fileName = PathUtil.GetLuaPath(name);

        byte[] luaScript = null;
        //�Ӽ������õ������Lua����
        if (!m_LuaScripts.TryGetValue(fileName, out luaScript))
        {
            Debug.LogError("lua script is not exist:" + fileName);
        }
        return luaScript;

    }

    /// <summary>
    /// �Ǳ༭��ģʽ�£���·���л�ȡab������ab���õ��ļ�
    /// </summary>
    void LoadLuaScript()
    {
        foreach (var name in LuaNames)
        {
            //�첽����Ҫһ���ص�(=>������һ�磬��LoadLuaִ����ʱ��ִ�лص���invoke��
            //�������)��obj���Ƿ��ص�Lua�Ķ���
            Manager.Resource.LoadLua(name, (UnityEngine.Object obj) =>
            {
                //LoadLua��������bundle���غõ�bundleRequest,asset��������obj����
                //�����Lua����������ӵ�m_LuaScripts��
                AddLuaScript(name, (obj as TextAsset).bytes);
                //��ResourceManager�н����汾�ļ�ʱ��������Lua�ļ���LuaNames
                //���LuaNamesȫ�������ص�m_LuaScripts�����У������LuaNames���˳�ѭ��
                if (m_LuaScripts.Count >= LuaNames.Count)
                {
                    //����Lua�ļ���������ˡ��Ϳ���ִ��ʹ��Lua�����ķ�����
                    Manager.Event.Fire(10000);
                    LuaNames.Clear();
                    LuaNames = null;
                }

            });
        }
    }

    /// <summary>
    /// ��LuaNames������ֶ�Ӧlua�ļ�������������ݣ��ŵ�������
    /// </summary>
    /// <param name="assetsName"></param>
    /// <param name="luaScript"></param>
    public void AddLuaScript(string assetsName, byte[] luaScript)
    {
        //Ϊ�˷�ֹ�ظ���ӣ������ַ�ʽ����ֱ�Ӹ���
        m_LuaScripts[assetsName] = luaScript;
    }
#if UNITY_EDITOR
    //�༭��ģʽ��ֱ�Ӽ���Lua�ļ�������Lua���ֺ����ݷŵ�������
    void EditorLoadLuaScript()
    {
        //��������Lua�ļ�
        string[] luaFiles = Directory.GetFiles(PathUtil.LuaPath, "*.bytes", 
            SearchOption.AllDirectories);
        for (int i = 0; i < luaFiles.Length; i++)
        {
            string fileName = PathUtil.GetStandardPath(luaFiles[i]);
            //��ȡLua�ļ�
            byte[] file = File.ReadAllBytes(fileName);
            //�Ѷ�ȡ��Lua�ļ���ӽ�ȥ
            AddLuaScript(PathUtil.GetUnityPath(fileName), file);
        }
        Manager.Event.Fire(10000);
    }

#endif
    private void Update()
    {
        //�ͷ�Lua�ڴ�
        if (LuaEnv != null)
        {
            LuaEnv.Tick();
        }
    }

    private void OnDestroy()
    {
        //�������Ҫ���ٵ�
        if (LuaEnv != null)
        {
            LuaEnv.Dispose();
            LuaEnv = null;
        }
    }
}
