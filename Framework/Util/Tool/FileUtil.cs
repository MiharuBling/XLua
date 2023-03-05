using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// 文件工具类：
/// 1查看指定文件是否存在
/// 2往指定目录写文件
/// </summary>
public class FileUtil
{
    //使用类都是用静态方法
    //检测文件是否存在
    public static bool IsExits(string path)
    {
        FileInfo file = new FileInfo(path);
        return file.Exists;
    }

    /// <summary>
    /// 写入文件
    /// </summary>
    /// <param name="path">路径</param>
    /// <param name="data">数据</param>
    public static void WriteFile(string path, byte[] data)
    {
        //获取标准路径
        path = PathUtil.GetStandardPath(path);
        //文件夹的路径
        string dir = path.Substring(0, path.LastIndexOf("/"));
        //判断文件夹存不存在，不存在就创建
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        FileInfo file = new FileInfo(path);
        //并非win的覆盖写入,必须删除再重新创建文件，否则会报错。
        if (file.Exists)
        {
            file.Delete();
        }
        try
        {
            //创建文件流,FileMode.Create 如果文件不存在，则重新创建，否则覆盖它，
            //FileAccess.Write写入的方式
            using (FileStream fs = new FileStream(path, FileMode.Create, 
                FileAccess.Write))
            {
                fs.Write(data, 0, data.Length);
                //写完文件关闭文件流
                fs.Close();
            }
        }
        catch (IOException e)
        {
            Debug.Log(e.Message);
        }
    }

}
