using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 热更新类：
/// 定义了下载信息DownFileInfo，和用来保存只读目录filelist和服务器filelist的变量
/// 通用UnityWebRequest，既可以从本地下载（释放）文件也可以从服务器下载文件，
/// 用DownLoadFile实现了单个或多个文件（封装了单个文件下载）下载
/// GetFileList可以从filelist获取需要更新的文件信息
/// 流程：
/// IsFirstInstall如果是初次安装，就从可读目录释放资源ReleaseResources到可读写目录
/// （然后热更新CheckUpdate），如果可读目录没有资源就从服务器下载资源CheckUpdate
/// 到可读写目录。释放与下载逻辑一致,ReleaseResources释放可读目录filelist，
/// 释放完OnReleaseReadPathFileListComplete下载所有包，回调单个文件下载好
/// OnReleaseFileComplete写入可读写目录，所有文件下载完OnReleaseAllFileComplete
/// 把可读目录filelist写入可读写目录，CheckUpdateheckUpdate下载服务器filelist，
/// 下载完OnDownLoadServerFileListComplete下载所有包，
/// 回调单个文件下载好OnUpdateFileComplete写入可读写目录，
/// 所有文件下载完OnUpdateAllFileComplete把服务器filelist写入可读写目录，进入游戏
/// </summary>
public class HotUpdate : MonoBehaviour
{
    byte[] m_ReadPathFileListData;
    byte[] m_ServerFileListData;

    internal class DownFileInfo
    {
        public string url;
        //bundle名
        public string fileName;
        public DownloadHandler fileData;
    }

    /// <summary>
    /// 协程方法下载文件，下载单个文件
    /// </summary>
    /// <param name="info"></param>
    /// <param name="Complete">返回一个下载句柄的Action</param>
    /// <returns></returns>
    IEnumerator DownLoadFile(DownFileInfo info, Action<DownFileInfo> Complete)
    {
        //老版本用WWW,现在已经弃用
        //新版需要使用UnityWebRequest 引入UnityEngine.Networking
        UnityWebRequest webRequest = UnityWebRequest.Get(info.url);
        //下载一个文件，等待，下载完继续执行
        yield return webRequest.SendWebRequest();

        //if(webRequest.isHttpError||webRequest.isNetworkError)
        if (webRequest.result == UnityWebRequest.Result.ProtocolError ||
            webRequest.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log("下载文件出错：" + info.url);
            yield break;
            //下载失败，重试，有次数限制
        }

        //下载完成后，给info赋值
        info.fileData = webRequest.downloadHandler;
        //如果下载的是filelist,直接解析,用webRequest,downloadHandle.text
        //如果是bundle,可以写入,用webRequest,downloadHandler.data
        Complete?.Invoke(info);
        //下载完成后释放掉
        webRequest.Dispose();
    }

    /// <summary>
    /// 下载多个文件的接口
    /// </summary>
    /// <param name="infos">多个文件列表</param>
    /// <param name="Complete">下载一个文件完成的回调，然后写入</param>
    /// <param name="DownLoadAllComplete">所有文件下载完成回调，通知用户释放资源、更新</param>
    /// <returns></returns>
    IEnumerator DownLoadFile(List<DownFileInfo> infos, Action<DownFileInfo> Complete,
        Action DownLoadAllComplete)
    {
        foreach (DownFileInfo info in infos)
        {
            //调用单个文件下载的协程
            yield return DownLoadFile(info, Complete);
        }
        DownLoadAllComplete?.Invoke();
    }

    /// <summary>
    /// 获取文件信息
    /// </summary>
    /// <param name="fileData"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    private List<DownFileInfo> GetFileList(string fileData, string path)
    {
        //对string规范化，因为有些符号，win写入txt会有多余的符号
        string content = fileData.Trim().Replace("\r", "");
        string[] files = content.Split("\n");
        List<DownFileInfo> downFileInfos = new List<DownFileInfo>(files.Length);
        for (int i = 0; i < files.Length; i++)
        {
            //拿到文件信息
            string[] info = files[i].Split('|');
            DownFileInfo fileInfo = new DownFileInfo();
            fileInfo.fileName = info[1];
            //文件的下载到哪里的地址
            fileInfo.url = Path.Combine(path, info[1]);
            downFileInfos.Add(fileInfo);
        }
        return downFileInfos;

    }

    private void Start()
    {
        if (IsFirstInstall())
        {
            //如果是初次安装，先按照可读目录的filelist释放资源到可读目录，再更新
            ReleaseResources();
        }
        else
        {
            CheckUpdate();
        }
    }

    /// <summary>
    /// 是否初次安装
    /// </summary>
    /// <returns></returns>
    private bool IsFirstInstall()
    {
        //判断只读目录是否存在版本文件
        bool isExistsReadPath = FileUtil.IsExits
            (Path.Combine(PathUtil.ReadPath, AppConst.FileListName));
        //判断可读写目录是否存在版本文件
        bool isExistsReadWritePath = FileUtil.IsExits
            (Path.Combine(PathUtil.ReadWritePath, AppConst.FileListName));

        return isExistsReadPath && !isExistsReadWritePath;

    }

    /// <summary>
    /// 根据可读目录的filelist释放资源
    /// </summary>
    private void ReleaseResources()
    {
        string url = Path.Combine(PathUtil.ReadPath, AppConst.FileListName);
        DownFileInfo info = new DownFileInfo();
        info.url = url;
        //UnityWebRequest可以从本地下载
        //先读取Filelist里面需要释放的文件
        StartCoroutine(DownLoadFile(info, OnDownLoadReadPathFileListComplete));
    }

    /// <summary>
    /// 释放完filelist的回调，用于释放filelist里面的所有文件
    /// </summary>
    /// <param name="file"></param>
    private void OnDownLoadReadPathFileListComplete(DownFileInfo file)
    {
        //从只读目录加载完filelist后，保存filelist的内容
        m_ReadPathFileListData = file.fileData.data;
        //获取到filelist里的所有要释放的文件的文件信息
        List<DownFileInfo> fileInfos = GetFileList(file.fileData.text, PathUtil.ReadPath);
        StartCoroutine(DownLoadFile(fileInfos, OnReleaseFileComplete, OnReleaseAllFileComplete));

    }

    /// <summary>
    /// 释放到可读写目录一个文件
    /// </summary>
    /// <param name="fileinfo"></param>
    private void OnReleaseFileComplete(DownFileInfo fileinfo)
    {
        //可读写目录加bundle目录
        string writeFile = Path.Combine(PathUtil.ReadWritePath, fileinfo.fileName);
        FileUtil.WriteFile(writeFile, fileinfo.fileData.data);
    }

    /// <summary>
    /// 所有文件释放到可读写目录完成后，写入filelist
    /// </summary>
    private void OnReleaseAllFileComplete()
    {
        //所有文件都释放完成后，再把filelist写入可读写目录
        FileUtil.WriteFile(Path.Combine(PathUtil.ReadWritePath, AppConst.FileListName),
            m_ReadPathFileListData);
        //释放完成后，检查更新
        CheckUpdate();
    }

    /// <summary>
    /// 检查更新
    /// </summary>
    private void CheckUpdate()
    {
        //获取filelist再释放服务器上的地址
        string url = Path.Combine(AppConst.ResourcesUrl, AppConst.FileListName);
        DownFileInfo info = new DownFileInfo();
        info.url = url;
        StartCoroutine(DownLoadFile(info, OnDownLoadServerFileListComplete));
    }

    private void OnDownLoadServerFileListComplete(DownFileInfo file)
    {
        //保存最新的filelist
        m_ServerFileListData = file.fileData.data;
        //获取资源服务器的文件信息目录
        List<DownFileInfo> fileInfos = GetFileList(file.fileData.text, AppConst.ResourcesUrl);
        //定义需要下载的文件集合
        List<DownFileInfo> downListFiles = new List<DownFileInfo>();

        //遍历资源服务器的文件信息
        for (int i = 0; i < fileInfos.Count; i++)
        {
            string localFile = Path.Combine(PathUtil.ReadWritePath, fileInfos[i].fileName);
            //判断本地是否存在，如果不存在就下载
            if (!FileUtil.IsExits(localFile))
            {
                fileInfos[i].url = Path.Combine(AppConst.ResourcesUrl, fileInfos[i].fileName);
                downListFiles.Add(fileInfos[i]);
            }
        }
        if (downListFiles.Count > 0)
        {
            StartCoroutine(DownLoadFile(fileInfos, OnUpdateFileComplete, OnUpdateAllFileComplete));

        }
        else
        {
            EnterGame();
        }
    }

    private void OnUpdateFileComplete(DownFileInfo file)
    {
        //下载新文件
        string writeFile = Path.Combine(PathUtil.ReadWritePath, file.fileName);
        FileUtil.WriteFile(writeFile, file.fileData.data);
    }

    private void OnUpdateAllFileComplete()
    {
        //所有文件下载完成，写入最新的filelist
        FileUtil.WriteFile(Path.Combine(PathUtil.ReadWritePath, AppConst.FileListName),
            m_ServerFileListData);
        EnterGame();
    }

    private void EnterGame()
    {
        Manager.Resource.ParseVersionFile();
        Manager.Resource.LoadUI("Login/LoginUI", OnComplete);
    }

    private void OnComplete(UnityEngine.Object obj)
    {
        GameObject go = Instantiate(obj) as GameObject;
        go.transform.SetParent(this.transform);
        go.SetActive(true);
        go.transform.localPosition = Vector3.zero;
    }
}
