using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MySceneManager : MonoBehaviour
{
    private string m_LogicName = "[SceneLogic]";
    private void Awake()
    {
        //��Ӧ�������ü����ʱ��ļ���
        SceneManager.activeSceneChanged += OnActiveSceneChanged;

    }

    /// <summary>
    /// �л������Ļص�����ǰ��s1�л����s2����
    /// </summary>
    /// <param name="s1">֮ǰ������ڱ�ȡ���������صĳ���</param>
    /// <param name="s2">�����¼���ĳ���</param>
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
        //����һ��������ʱ��֮ǰ�ĳ����ᱻ����,�����Ҫ����һ���¼�,���������ĸ�������ȡ��������
    }

    /// <summary>
    /// ���Ӽ��س����Ľӿ�
    /// </summary>
    /// <param name="sceneName"></param>
    /// <param name="luaName"></param>
    public void LoadScene(string sceneName, string luaName)
    {
        //������unity,ab�������ɺ��첽���س���
        Manager.Resource.LoadScene(sceneName, (UnityEngine.Object obj) =>
        {
            StartCoroutine(StartLoadScene(sceneName, luaName, LoadSceneMode.Additive));
        });
    }

    /// <summary>
    /// �л������Ľӿ�
    /// </summary>
    /// <param name="sceneName"></param>
    /// <param name="luaName"></param>
    public void ChangeScene(string sceneName, string luaName)
    {
        //������.unity��ab�������ɺ��첽���س���
        Manager.Resource.LoadScene(sceneName, (UnityEngine.Object obj) =>
        {
            //ֻ���ص�һ�ĳ���
            StartCoroutine(StartLoadScene(sceneName, luaName, LoadSceneMode.Single));
        });
    }

    /// <summary>
    /// ж�س���
    /// </summary>
    /// <param name="sceneName"></param>
    public void UnLoadSceneAsync(string sceneName)
    {
        StartCoroutine(UnLoadScene(sceneName));
    }

    /// <summary>
    /// ��ⳡ���Ƿ��Ѿ�����
    /// </summary>
    /// <param name="sceneName">������</param>
    /// <returns></returns>
    private bool IsLoadedScene(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        return scene.isLoaded;
    }

    IEnumerator StartLoadScene(string sceneName, string luaName, LoadSceneMode mode)
    {
        //��ⳡ���Ƿ����
        if(IsLoadedScene(sceneName))
            yield break;

        //�첽����Эͬ����
        AsyncOperation async = SceneManager.LoadSceneAsync(sceneName, mode);
        //allowSceneActivation������һ��׼���þͱ��������false�Ļ�������ֻ����90%��
        //����10%�ȴ�
        async.allowSceneActivation = true;
        yield return async;

        //��ȡ���سɹ��ĳ���
        Scene scene = SceneManager.GetSceneByName(sceneName);
        //�������س����߼��ű������壬ÿһ���������غã����ᴴ�����������SceneLogic�ű�
        //���ƶ����ó�����
        GameObject go = new GameObject(m_LogicName);
        //�п��ܼ����˶����������������س����߼��ű��������ƶ�������ĳ�����
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
        //��Ϊ�ɶ��������Ҫ�Ҹ�����SceneLogic�ű������嵽�����ĸ���������
        SceneLogic logic = GetSceneLogic(scene);
        logic?.OnQuit();//����ж�ص���lua��quit�������ȵ���lua���˳��������������첽ж�س���
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
