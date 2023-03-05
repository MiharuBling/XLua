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
    //为什么不写成const？因为要在inspector中手动修改状态，而不能每次进来修改代码
    public static GameMode GameMode = GameMode.EditorMode;
    //热更新资源链接地址
    public const string ResourcesUrl = "http://127.0.0.1/AssetBundles/";

    
}
