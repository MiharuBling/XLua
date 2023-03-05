using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameMode
{
    EditorMode,
    PackageBundle,
    UpdateMode,
}

public class AppConst
{
    public const string BundleExtension = ".ab";
    public const string FileListName = "filelist.txt";
    //Ϊʲô��д��const����ΪҪ��inspector���ֶ��޸�״̬��������ÿ�ν����޸Ĵ���
    public static GameMode GameMode = GameMode.EditorMode;
    //�ȸ�����Դ���ӵ�ַ
    public const string ResourcesUrl = "http://127.0.0.1/AssetBundles/";

    
}
