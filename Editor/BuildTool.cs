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

    //Ϊ���ܹ�������ƽ̨����Ҫ��Ŀ��ƽ̨��Ϊ��������
    static void Build(BuildTarget target)
    {
        //��ҪĿ�����ռ����build��Ϣ����Ҫ����Щ�ļ�����Ҫ��bundle����һ��ʲô��������
        List<AssetBundleBuild> assetBundleBuilds = new List<AssetBundleBuild>();
        //�ļ���Ϣ�б�
        List<string> bundleInfos = new List<string>();
        //��һ����������������ļ����ļ���Directory.GetDirectories��
        //Directory.GetFiles��Ӧ���ִ������һ����ȡ�ļ���һ����ȡ�ļ�,GetFiles�Ƚϼ�
        string[] files = Directory.GetFiles(PathUtil.BuildResourcesPath, "*", SearchOption.AllDirectories);
        //�����ļ����ҳ����ˣ���Ҫ�ų���meta�ļ���json�ļ�
        for (int i = 0; i < files.Length; i++)
        {
            if (files[i].EndsWith(".meta") || files[i].EndsWith(".json"))
            {
                continue;
            }
            //����һ����Ҫbuild��Bundle
            AssetBundleBuild assetBundle = new AssetBundleBuild();

            //���������·��б�ܿ��ܲ�ͬ����Ҫ�淶һ��
            string fileName = PathUtil.GetStandardPath(files[i]);

            string assetName = PathUtil.GetUnityPath(fileName);//��ȡ���·��

            //һ��assetBundle���Դ������ļ�������ֻ��һ���ļ�
            assetBundle.assetNames = new string[] { assetName };

            //��������
            string bundleName = fileName.Replace(PathUtil.BuildResourcesPath, "").ToLower();
            assetBundle.assetBundleName = bundleName + ".ab";

            assetBundleBuilds.Add(assetBundle);

            //����ļ���������Ϣ
            List<string> dependenceInfo = GetDependence(assetName);
            //�汾��Ϣ�����ļ�·������bundle���������ļ��б�
            string bundleInfo = assetName + "|" + bundleName + ".ab";

            if (dependenceInfo.Count > 0)
                bundleInfo = bundleInfo + "|" + string.Join("|", dependenceInfo);

            bundleInfos.Add(bundleInfo);
            
        }

        //Ϊʲô������һ������BuildAssetBundles(string outputPath, BuildAssetBundleOptions assetBundleOptions, BuildTarget targetPlatform)������Ϊ��Ҫ�Լ�ȥ��Դ����bundle�����ǩ�����鷳
        //�ڶ���������listתΪarray����
        //������������ѹ����ʽ��ѡ��Ĭ��
        //���ĸ�������Ŀ��ƽ̨����ѡ��win
        if (Directory.Exists(PathUtil.BundleOutPath))
        {
            //�ж��Ƿ���·�������������ļ��У���ɾ���ļ����ݹ�recursiveɾ�������ļ������ļ�
            Directory.Delete(PathUtil.BundleOutPath, true);
        }
        Directory.CreateDirectory(PathUtil.BundleOutPath);//ɾ��·���󣬴���·��

        BuildPipeline.BuildAssetBundles(PathUtil.BundleOutPath, assetBundleBuilds.ToArray(),
            BuildAssetBundleOptions.None, target);

        //дbundle��Ϣ�ļ�
        File.WriteAllLines(PathUtil.BundleOutPath + "/" + AppConst.FileListName, bundleInfos);
        //�������ļ�����unity��Դ����ˢ��һ��
        AssetDatabase.Refresh();
        

      
    }

    static List<string> GetDependence(string curFile)
    {
        List<string> dependence = new List<string>();
        //������ļ��������ļ�ȫ����ȡ���������ȡ�ű��ļ��������ļ��������Ҫȥ��
        string[] files = AssetDatabase.GetDependencies(curFile);
        dependence = files.Where(file => !file.EndsWith(".cs") && !file.Equals(curFile)).ToList();
        return dependence;
    }
}
