using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestBuild : MonoBehaviour
{
    private IEnumerator Start()
    {
        //��Դ�첽��������
        //AssetBundleCreateRequest���ڼ���bundle�ļ����صĶ���  ����bundleֻ��Ҫ.ab�ļ�
        AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync
            (Application.streamingAssetsPath + "/ui/prefabs/testui.prefab.ab");
        yield return request;

        //�ֶ�����prefab�õ�������ͼƬ��ab��  �������Զ���������ab���ķ���
        AssetBundleCreateRequest request1 = AssetBundle.LoadFromFileAsync
            (Application.streamingAssetsPath + "/ui/res/menu-background-image.png.ab");
    }
}
