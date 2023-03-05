using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using XLua;

/// <summary>
/// LuaNames是ResourceManager中解析版本文件ParseVersionFile时赋值的，根据LuaNames，
/// 从Bundle（或Editor本地）加载LoadLua，加载好就放入m_LuaScripts，
/// 全部加载完清空LuaNames
/// 初始化Init：添加一个回调，创建new LuaEnv()虚拟机，虚拟机LuaEnv.AddLoader(loader)
/// (外部StartLua调用DoString找lua，loader调用GetLuaScript找到lua脚本)，
/// 按模式加载lua(bundle或者editor)调用的前提是全部加载完了，执行了回调通知加载完毕，才可以调用。
/// 即InitOk
/// 调用：1.Init；2.StartLua（内部是LuaEnv.DoStringloader调用GetLuaScript找到lua脚本）；
/// 3.调用函数XLua.LuaFunction func = Global.Get<xxx>("xxx");func.Call();
/// </summary>
public class LuaManager : MonoBehaviour
{
    //所有的Lua文件名，获取所有Lua，然后进行预加载，ResourceManager查找Lua文件放进来
    public List<string> LuaNames = new List<string>();

    //缓存Lua脚本内容
    private Dictionary<string, byte[]> m_LuaScripts;

    //定义一个Lua虚拟机，消耗比较大，全局只需要一个，需要using XLua；
    public LuaEnv LuaEnv;


    //如果是Editor模式下，直接从Luapath就把所有Lua读取到字典中了，然后再调用，
    //属于同步加载后同步使用的情况，但是在其他模式下，需要从bundle异步加载Lua
    //如果等待时Start就调用了，属于异步加载同步使用的情况，需要预加载
    //需要创建一个回调通知
    public void Init()
    {
        //初始化虚拟机
        LuaEnv = new LuaEnv();
        //外部调用require时，会自动调用Loader来获取文件
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
    /// 启动对应脚本的Lua文件，实际上是把启动文件的string传给Loader去加载
    /// 然后Loader通过GetLuaScript加载出来Lua
    /// </summary>
    /// <param name="name"></param>
    public void StartLua(string name)
    {
        LuaEnv.DoString(string.Format("require'{0}'", name));
    }

    /// <summary>
    /// Lua里面调用require后面的参数会传到name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    byte[] Loader(ref string name)
    {
        return GetLuaScript(name);
    }

    /// <summary>
    /// 和Loader配合使用找到要调用的指定目录Lua文件
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public byte[] GetLuaScript(string name)
    {
        //为什么要替换掉.换成/,因为一般使用require ui.Login.register
        name = name.Replace(".", "/");
        //自定义的Lua后缀名是.bytes
        string fileName = PathUtil.GetLuaPath(name);

        byte[] luaScript = null;
        //从集合中拿到缓存的Lua内容
        if (!m_LuaScripts.TryGetValue(fileName, out luaScript))
        {
            Debug.LogError("lua script is not exist:" + fileName);
        }
        return luaScript;

    }

    /// <summary>
    /// 非编辑器模式下，从路径中获取ab包，从ab包拿到文件
    /// </summary>
    void LoadLuaScript()
    {
        foreach (var name in LuaNames)
        {
            //异步的需要一个回调(=>后面那一坨，当LoadLua执行完时，执行回调并invoke把
            //结果返回)，obj就是返回的Lua的对象
            Manager.Resource.LoadLua(name, (UnityEngine.Object obj) =>
            {
                //LoadLua调用完会把bundle加载好的bundleRequest,asset传进来用obj接受
                //把这个Lua根据名称添加到m_LuaScripts中
                AddLuaScript(name, (obj as TextAsset).bytes);
                //在ResourceManager中解析版本文件时加载所有Lua文件到LuaNames
                //如果LuaNames全部都加载到m_LuaScripts集合中，就清空LuaNames，退出循环
                if (m_LuaScripts.Count >= LuaNames.Count)
                {
                    //所有Lua文件加载完成了。就可以执行使用Lua函数的方法了
                    Manager.Event.Fire(10000);
                    LuaNames.Clear();
                    LuaNames = null;
                }

            });
        }
    }

    /// <summary>
    /// 把LuaNames里的名字对应lua文件本身里面的内容，放到集合内
    /// </summary>
    /// <param name="assetsName"></param>
    /// <param name="luaScript"></param>
    public void AddLuaScript(string assetsName, byte[] luaScript)
    {
        //为了防止重复添加，用这种方式可以直接覆盖
        m_LuaScripts[assetsName] = luaScript;
    }
#if UNITY_EDITOR
    //编辑器模式下直接加载Lua文件，并把Lua名字和内容放到集合内
    void EditorLoadLuaScript()
    {
        //搜索所有Lua文件
        string[] luaFiles = Directory.GetFiles(PathUtil.LuaPath, "*.bytes", 
            SearchOption.AllDirectories);
        for (int i = 0; i < luaFiles.Length; i++)
        {
            string fileName = PathUtil.GetStandardPath(luaFiles[i]);
            //读取Lua文件
            byte[] file = File.ReadAllBytes(fileName);
            //把读取的Lua文件添加进去
            AddLuaScript(PathUtil.GetUnityPath(fileName), file);
        }
        Manager.Event.Fire(10000);
    }

#endif
    private void Update()
    {
        //释放Lua内存
        if (LuaEnv != null)
        {
            LuaEnv.Tick();
        }
    }

    private void OnDestroy()
    {
        //虚拟机需要销毁掉
        if (LuaEnv != null)
        {
            LuaEnv.Dispose();
            LuaEnv = null;
        }
    }
}
