using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestBuild : MonoBehaviour
{
    private IEnumerator Start()
    {
        //资源异步创建请求
        //AssetBundleCreateRequest用于加载bundle文件返回的东西  加载bundle只需要.ab文件
        AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync
            (Application.streamingAssetsPath + "/ui/prefabs/testui.prefab.ab");
        yield return request;

        //手动加载prefab用到的两个图片的ab包  后面有自动加载依赖ab包的方法
        AssetBundleCreateRequest request1 = AssetBundle.LoadFromFileAsync
            (Application.streamingAssetsPath + "/ui/res/menu-background-image.png.ab");
    }
}
