using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// �ȸ����ࣺ
/// ������������ϢDownFileInfo������������ֻ��Ŀ¼filelist�ͷ�����filelist�ı���
/// ͨ��UnityWebRequest���ȿ��Դӱ������أ��ͷţ��ļ�Ҳ���Դӷ����������ļ���
/// ��DownLoadFileʵ���˵��������ļ�����װ�˵����ļ����أ�����
/// GetFileList���Դ�filelist��ȡ��Ҫ���µ��ļ���Ϣ
/// ���̣�
/// IsFirstInstall����ǳ��ΰ�װ���ʹӿɶ�Ŀ¼�ͷ���ԴReleaseResources���ɶ�дĿ¼
/// ��Ȼ���ȸ���CheckUpdate��������ɶ�Ŀ¼û����Դ�ʹӷ�����������ԴCheckUpdate
/// ���ɶ�дĿ¼���ͷ��������߼�һ��,ReleaseResources�ͷſɶ�Ŀ¼filelist��
/// �ͷ���OnReleaseReadPathFileListComplete�������а����ص������ļ����غ�
/// OnReleaseFileCompleteд��ɶ�дĿ¼�������ļ�������OnReleaseAllFileComplete
/// �ѿɶ�Ŀ¼filelistд��ɶ�дĿ¼��CheckUpdateheckUpdate���ط�����filelist��
/// ������OnDownLoadServerFileListComplete�������а���
/// �ص������ļ����غ�OnUpdateFileCompleteд��ɶ�дĿ¼��
/// �����ļ�������OnUpdateAllFileComplete�ѷ�����filelistд��ɶ�дĿ¼��������Ϸ
/// </summary>
public class HotUpdate : MonoBehaviour
{
    byte[] m_ReadPathFileListData;
    byte[] m_ServerFileListData;

    internal class DownFileInfo
    {
        public string url;
        //bundle��
        public string fileName;
        public DownloadHandler fileData;
    }

    /// <summary>
    /// Э�̷��������ļ������ص����ļ�
    /// </summary>
    /// <param name="info"></param>
    /// <param name="Complete">����һ�����ؾ����Action</param>
    /// <returns></returns>
    IEnumerator DownLoadFile(DownFileInfo info, Action<DownFileInfo> Complete)
    {
        //�ϰ汾��WWW,�����Ѿ�����
        //�°���Ҫʹ��UnityWebRequest ����UnityEngine.Networking
        UnityWebRequest webRequest = UnityWebRequest.Get(info.url);
        //����һ���ļ����ȴ������������ִ��
        yield return webRequest.SendWebRequest();

        //if(webRequest.isHttpError||webRequest.isNetworkError)
        if (webRequest.result == UnityWebRequest.Result.ProtocolError ||
            webRequest.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log("�����ļ�����" + info.url);
            yield break;
            //����ʧ�ܣ����ԣ��д�������
        }

        //������ɺ󣬸�info��ֵ
        info.fileData = webRequest.downloadHandler;
        //������ص���filelist,ֱ�ӽ���,��webRequest,downloadHandle.text
        //�����bundle,����д��,��webRequest,downloadHandler.data
        Complete?.Invoke(info);
        //������ɺ��ͷŵ�
        webRequest.Dispose();
    }

    /// <summary>
    /// ���ض���ļ��Ľӿ�
    /// </summary>
    /// <param name="infos">����ļ��б�</param>
    /// <param name="Complete">����һ���ļ���ɵĻص���Ȼ��д��</param>
    /// <param name="DownLoadAllComplete">�����ļ�������ɻص���֪ͨ�û��ͷ���Դ������</param>
    /// <returns></returns>
    IEnumerator DownLoadFile(List<DownFileInfo> infos, Action<DownFileInfo> Complete,
        Action DownLoadAllComplete)
    {
        foreach (DownFileInfo info in infos)
        {
            //���õ����ļ����ص�Э��
            yield return DownLoadFile(info, Complete);
        }
        DownLoadAllComplete?.Invoke();
    }

    /// <summary>
    /// ��ȡ�ļ���Ϣ
    /// </summary>
    /// <param name="fileData"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    private List<DownFileInfo> GetFileList(string fileData, string path)
    {
        //��string�淶������Ϊ��Щ���ţ�winд��txt���ж���ķ���
        string content = fileData.Trim().Replace("\r", "");
        string[] files = content.Split("\n");
        List<DownFileInfo> downFileInfos = new List<DownFileInfo>(files.Length);
        for (int i = 0; i < files.Length; i++)
        {
            //�õ��ļ���Ϣ
            string[] info = files[i].Split('|');
            DownFileInfo fileInfo = new DownFileInfo();
            fileInfo.fileName = info[1];
            //�ļ������ص�����ĵ�ַ
            fileInfo.url = Path.Combine(path, info[1]);
            downFileInfos.Add(fileInfo);
        }
        return downFileInfos;

    }

    private void Start()
    {
        if (IsFirstInstall())
        {
            //����ǳ��ΰ�װ���Ȱ��տɶ�Ŀ¼��filelist�ͷ���Դ���ɶ�Ŀ¼���ٸ���
            ReleaseResources();
        }
        else
        {
            CheckUpdate();
        }
    }

    /// <summary>
    /// �Ƿ���ΰ�װ
    /// </summary>
    /// <returns></returns>
    private bool IsFirstInstall()
    {
        //�ж�ֻ��Ŀ¼�Ƿ���ڰ汾�ļ�
        bool isExistsReadPath = FileUtil.IsExits
            (Path.Combine(PathUtil.ReadPath, AppConst.FileListName));
        //�жϿɶ�дĿ¼�Ƿ���ڰ汾�ļ�
        bool isExistsReadWritePath = FileUtil.IsExits
            (Path.Combine(PathUtil.ReadWritePath, AppConst.FileListName));

        return isExistsReadPath && !isExistsReadWritePath;

    }

    /// <summary>
    /// ���ݿɶ�Ŀ¼��filelist�ͷ���Դ
    /// </summary>
    private void ReleaseResources()
    {
        string url = Path.Combine(PathUtil.ReadPath, AppConst.FileListName);
        DownFileInfo info = new DownFileInfo();
        info.url = url;
        //UnityWebRequest���Դӱ�������
        //�ȶ�ȡFilelist������Ҫ�ͷŵ��ļ�
        StartCoroutine(DownLoadFile(info, OnDownLoadReadPathFileListComplete));
    }

    /// <summary>
    /// �ͷ���filelist�Ļص��������ͷ�filelist����������ļ�
    /// </summary>
    /// <param name="file"></param>
    private void OnDownLoadReadPathFileListComplete(DownFileInfo file)
    {
        //��ֻ��Ŀ¼������filelist�󣬱���filelist������
        m_ReadPathFileListData = file.fileData.data;
        //��ȡ��filelist�������Ҫ�ͷŵ��ļ����ļ���Ϣ
        List<DownFileInfo> fileInfos = GetFileList(file.fileData.text, PathUtil.ReadPath);
        StartCoroutine(DownLoadFile(fileInfos, OnReleaseFileComplete, OnReleaseAllFileComplete));

    }

    /// <summary>
    /// �ͷŵ��ɶ�дĿ¼һ���ļ�
    /// </summary>
    /// <param name="fileinfo"></param>
    private void OnReleaseFileComplete(DownFileInfo fileinfo)
    {
        //�ɶ�дĿ¼��bundleĿ¼
        string writeFile = Path.Combine(PathUtil.ReadWritePath, fileinfo.fileName);
        FileUtil.WriteFile(writeFile, fileinfo.fileData.data);
    }

    /// <summary>
    /// �����ļ��ͷŵ��ɶ�дĿ¼��ɺ�д��filelist
    /// </summary>
    private void OnReleaseAllFileComplete()
    {
        //�����ļ����ͷ���ɺ��ٰ�filelistд��ɶ�дĿ¼
        FileUtil.WriteFile(Path.Combine(PathUtil.ReadWritePath, AppConst.FileListName),
            m_ReadPathFileListData);
        //�ͷ���ɺ󣬼�����
        CheckUpdate();
    }

    /// <summary>
    /// ������
    /// </summary>
    private void CheckUpdate()
    {
        //��ȡfilelist���ͷŷ������ϵĵ�ַ
        string url = Path.Combine(AppConst.ResourcesUrl, AppConst.FileListName);
        DownFileInfo info = new DownFileInfo();
        info.url = url;
        StartCoroutine(DownLoadFile(info, OnDownLoadServerFileListComplete));
    }

    private void OnDownLoadServerFileListComplete(DownFileInfo file)
    {
        //�������µ�filelist
        m_ServerFileListData = file.fileData.data;
        //��ȡ��Դ���������ļ���ϢĿ¼
        List<DownFileInfo> fileInfos = GetFileList(file.fileData.text, AppConst.ResourcesUrl);
        //������Ҫ���ص��ļ�����
        List<DownFileInfo> downListFiles = new List<DownFileInfo>();

        //������Դ���������ļ���Ϣ
        for (int i = 0; i < fileInfos.Count; i++)
        {
            string localFile = Path.Combine(PathUtil.ReadWritePath, fileInfos[i].fileName);
            //�жϱ����Ƿ���ڣ���������ھ�����
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
        //�������ļ�
        string writeFile = Path.Combine(PathUtil.ReadWritePath, file.fileName);
        FileUtil.WriteFile(writeFile, file.fileData.data);
    }

    private void OnUpdateAllFileComplete()
    {
        //�����ļ�������ɣ�д�����µ�filelist
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
