using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

public class LuaBehaviour : MonoBehaviour
{
    //全局只能有一个LuaEnv
    private LuaEnv m_LuaEnv = Manager.Lua.LuaEnv;
    protected LuaTable m_ScriptEnv;
    private Action m_LuaInit;
    private Action m_LuaUpdate;
    private Action m_LuaOnDestroy;

    public string luaName;

    private void Awake()
    {
        m_ScriptEnv = m_LuaEnv.NewTable();

        //为每个脚本设置一个独立的环境，可一定程度上放置脚本间全局变量、函数冲突
        LuaTable meta = m_LuaEnv.NewTable();
        meta.Set("_index", m_LuaEnv.Global);
        m_ScriptEnv.SetMetaTable(meta);
        meta.Dispose();

        m_ScriptEnv.Set("self", this);

    }

    public virtual void Init(string luaName)
    {
        m_LuaEnv.DoString(Manager.Lua.GetLuaScript(luaName), luaName, m_ScriptEnv);
        m_ScriptEnv.Get("Update", out m_LuaUpdate);
        m_ScriptEnv.Get("OnInit", out m_LuaInit);

        m_LuaInit?.Invoke();
    }
   

    private void Update()
    {
        m_LuaUpdate?.Invoke();
    }

    protected virtual void Clear()
    {
        m_LuaOnDestroy = null;
        //运行环境释放掉
        m_ScriptEnv?.Dispose();
        m_ScriptEnv = null;
        m_LuaInit = null;
        m_LuaUpdate = null;
    }

    //两个不一定同时触发，退出的时候不会调用OnDestroy，所以都要写Clear
    private void OnDestroy()
    {
        m_LuaOnDestroy?.Invoke();
        Clear();
    }
    private void OnApplicationQuit()
    {
        Clear();
    }
}
