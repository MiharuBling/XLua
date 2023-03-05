using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MySceneManager : MonoBehaviour
{
    private string m_LogicName = "[SceneLogic]";
    private void Awake()
    {
        //对应下面设置激活场景时候的监听
        SceneManager.activeSceneChanged += OnActiveSceneChanged;

    }

    /// <summary>
    /// 切换场景的回调，从前面s1切换激活到s2场景
    /// </summary>
    /// <param name="s1">之前激活，现在被取消激活隐藏的场景</param>
    /// <param name="s2">现在新激活的场景</param>
    private void OnActiveSceneChanged(Scene s1, Scene s2)
    {
        if (!s1.isLoaded || !s2.isLoaded)
            return;
        SceneLogic logic1 = GetSceneLogic(s1);
        SceneLogic logic2 = GetSceneLogic(s2);

        logic1?.OnInActive();
        logic2?.OnActive();
    }

    public void SetActive(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        SceneManager.SetActiveScene(scene);
        //激活一个场景的时候，之前的场景会被隐藏,因此需要创建一个事件,用来监听哪个场景被取消激活了
    }

    /// <summary>
    /// 叠加加载场景的接口
    /// </summary>
    /// <param name="sceneName"></param>
    /// <param name="luaName"></param>
    public void LoadScene(string sceneName, string luaName)
    {
        //场景是unity,ab包解包完成后异步加载场景
        Manager.Resource.LoadScene(sceneName, (UnityEngine.Object obj) =>
        {
            StartCoroutine(StartLoadScene(sceneName, luaName, LoadSceneMode.Additive));
        });
    }

    /// <summary>
    /// 切换场景的接口
    /// </summary>
    /// <param name="sceneName"></param>
    /// <param name="luaName"></param>
    public void ChangeScene(string sceneName, string luaName)
    {
        //场景是.unity，ab包解包完成后异步加载场景
        Manager.Resource.LoadScene(sceneName, (UnityEngine.Object obj) =>
        {
            //只加载单一的场景
            StartCoroutine(StartLoadScene(sceneName, luaName, LoadSceneMode.Single));
        });
    }

    /// <summary>
    /// 卸载场景
    /// </summary>
    /// <param name="sceneName"></param>
    public void UnLoadSceneAsync(string sceneName)
    {
        StartCoroutine(UnLoadScene(sceneName));
    }

    /// <summary>
    /// 检测场景是否已经加载
    /// </summary>
    /// <param name="sceneName">场景名</param>
    /// <returns></returns>
    private bool IsLoadedScene(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        return scene.isLoaded;
    }

    IEnumerator StartLoadScene(string sceneName, string luaName, LoadSceneMode mode)
    {
        //检测场景是否加载
        if(IsLoadedScene(sceneName))
            yield break;

        //异步操作协同程序
        AsyncOperation async = SceneManager.LoadSceneAsync(sceneName, mode);
        //allowSceneActivation允许场景一旦准备好就被激活，设置false的话，场景只加载90%，
        //留下10%等待
        async.allowSceneActivation = true;
        yield return async;

        //获取加载成功的场景
        Scene scene = SceneManager.GetSceneByName(sceneName);
        //创建挂载场景逻辑脚本的物体，每一个场景加载好，都会创建个物体挂载SceneLogic脚本
        //并移动到该场景中
        GameObject go = new GameObject(m_LogicName);
        //有可能加载了多个场景，把这个挂载场景逻辑脚本的物体移动到激活的场景中
        SceneManager.MoveGameObjectToScene(go, scene);

        SceneLogic logic = go.AddComponent<SceneLogic>();
        logic.SceneName = sceneName;
        logic.Init(luaName);
        logic.OnEnter();

    }

    private IEnumerator UnLoadScene(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (!scene.isLoaded)
        {
            Debug.LogError("scene not isloaded");
            yield break;
        }
        //因为由多个场景，要找个挂载SceneLogic脚本的物体到底在哪个物体身上
        SceneLogic logic = GetSceneLogic(scene);
        logic?.OnQuit();//场景卸载调用lua的quit方法，先调用lua的退出场景函数，再异步卸载场景
        AsyncOperation async = SceneManager.UnloadSceneAsync(scene);
        yield return async;

    }
    private SceneLogic GetSceneLogic(Scene scene)
    {
        GameObject[] gameObjects = scene.GetRootGameObjects();
        foreach (GameObject go in gameObjects)
        {
            if (go.name.CompareTo(m_LogicName) == 0)
            {
                SceneLogic logic = go.GetComponent<SceneLogic>();
                return logic;
            }
        }
        return null;
    }
}
