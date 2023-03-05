using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStart : MonoBehaviour
{
    public GameMode GameMode;
    private void Start()
    {
        AppConst.GameMode = GameMode;
        DontDestroyOnLoad(this);

        Manager.Resource.ParseVersionFile();
        Manager.Lua.Init(() =>
        {
            //初始化完成之后(Lua都加载完),在执行回调
            Manager.Lua.StartLua("main");
            //输入的是函数名
            XLua.LuaFunction func = Manager.Lua.LuaEnv.Global.Get<XLua.LuaFunction>("Main");
            func.Call();
        });
    }
}
