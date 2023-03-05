using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// �ļ������ࣺ
/// 1�鿴ָ���ļ��Ƿ����
/// 2��ָ��Ŀ¼д�ļ�
/// </summary>
public class FileUtil
{
    //ʹ���඼���þ�̬����
    //����ļ��Ƿ����
    public static bool IsExits(string path)
    {
        FileInfo file = new FileInfo(path);
        return file.Exists;
    }

    /// <summary>
    /// д���ļ�
    /// </summary>
    /// <param name="path">·��</param>
    /// <param name="data">����</param>
    public static void WriteFile(string path, byte[] data)
    {
        //��ȡ��׼·��
        path = PathUtil.GetStandardPath(path);
        //�ļ��е�·��
        string dir = path.Substring(0, path.LastIndexOf("/"));
        //�ж��ļ��д治���ڣ������ھʹ���
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        FileInfo file = new FileInfo(path);
        //����win�ĸ���д��,����ɾ�������´����ļ�������ᱨ��
        if (file.Exists)
        {
            file.Delete();
        }
        try
        {
            //�����ļ���,FileMode.Create ����ļ������ڣ������´��������򸲸�����
            //FileAccess.Writeд��ķ�ʽ
            using (FileStream fs = new FileStream(path, FileMode.Create, 
                FileAccess.Write))
            {
                fs.Write(data, 0, data.Length);
                //д���ļ��ر��ļ���
                fs.Close();
            }
        }
        catch (IOException e)
        {
            Debug.Log(e.Message);
        }
    }

}
