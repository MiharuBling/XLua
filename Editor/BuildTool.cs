using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class BuildTool : Editor
{
    [MenuItem("Tools/Build Windows Bundle")]
    static void BundleWindowsBuild()
    {
        Build(BuildTarget.StandaloneWindows);
    }

    [MenuItem("Tools/Build Android Bundle")]
    static void BundleAndroidBuild()
    {
        Build(BuildTarget.Android);
    }

    [MenuItem("Tools/Build IOS Bundle")]
    static void BundleIOSBuild()
    {
        Build(BuildTarget.iOS);
    }

    //为了能够构建多平台，需要把目标平台作为参数传入
    static void Build(BuildTarget target)
    {
        //主要目的是收集这个build信息，需要打哪些文件，需要给bundle包用一个什么样的名字
        List<AssetBundleBuild> assetBundleBuilds = new List<AssetBundleBuild>();
        //文件信息列表
        List<string> bundleInfos = new List<string>();
        //第一步搜索出这个所有文件的文件名Directory.GetDirectories和
        //Directory.GetFiles对应两种打包策略一个获取文件夹一个获取文件,GetFiles比较简单
        string[] files = Directory.GetFiles(PathUtil.BuildResourcesPath, "*", SearchOption.AllDirectories);
        //所有文件都找出来了，需要排除掉meta文件和json文件
        for (int i = 0; i < files.Length; i++)
        {
            if (files[i].EndsWith(".meta") || files[i].EndsWith(".json"))
            {
                continue;
            }
            //创建一个需要build的Bundle
            AssetBundleBuild assetBundle = new AssetBundleBuild();

            //处理出来的路径斜杠可能不同，需要规范一下
            string fileName = PathUtil.GetStandardPath(files[i]);

            string assetName = PathUtil.GetUnityPath(fileName);//获取相对路径

            //一个assetBundle可以打包多个文件，这里只放一个文件
            assetBundle.assetNames = new string[] { assetName };

            //创建包名
            string bundleName = fileName.Replace(PathUtil.BuildResourcesPath, "").ToLower();
            assetBundle.assetBundleName = bundleName + ".ab";

            assetBundleBuilds.Add(assetBundle);

            //添加文件和依赖信息
            List<string> dependenceInfo = GetDependence(assetName);
            //版本信息包括文件路径名，bundle名、依赖文件列表
            string bundleInfo = assetName + "|" + bundleName + ".ab";

            if (dependenceInfo.Count > 0)
                bundleInfo = bundleInfo + "|" + string.Join("|", dependenceInfo);

            bundleInfos.Add(bundleInfo);
            
        }

        //为什么不用另一个重载BuildAssetBundles(string outputPath, BuildAssetBundleOptions assetBundleOptions, BuildTarget targetPlatform)，是因为需要自己去资源设置bundle名打标签，很麻烦
        //第二个参数把list转为array数组
        //第三个参数是压缩格式，选择默认
        //第四个参数是目标平台，先选择win
        if (Directory.Exists(PathUtil.BundleOutPath))
        {
            //判断是否有路径，如果有这个文件夹，就删掉文件，递归recursive删掉所有文件和子文件
            Directory.Delete(PathUtil.BundleOutPath, true);
        }
        Directory.CreateDirectory(PathUtil.BundleOutPath);//删除路径后，创建路径

        BuildPipeline.BuildAssetBundles(PathUtil.BundleOutPath, assetBundleBuilds.ToArray(),
            BuildAssetBundleOptions.None, target);

        //写bundle信息文件
        File.WriteAllLines(PathUtil.BundleOutPath + "/" + AppConst.FileListName, bundleInfos);
        //创建好文件后，在unity资源库中刷新一下
        AssetDatabase.Refresh();
        

      
    }

    static List<string> GetDependence(string curFile)
    {
        List<string> dependence = new List<string>();
        //把这个文件的依赖文件全部获取出来，会获取脚本文件和自身文件，因此需要去掉
        string[] files = AssetDatabase.GetDependencies(curFile);
        dependence = files.Where(file => !file.EndsWith(".cs") && !file.Equals(curFile)).ToList();
        return dependence;
    }
}
