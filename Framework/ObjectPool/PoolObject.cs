using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//ר���������ObjectPool����õĲ�ͬ�����������
public class PoolObject
{
    //�������
    public Object Object;

    //��������
    public string Name;

    //���һ��ʹ��ʱ�䣬��Բ�ͬ���͵Ķ����в�ͬ����Чʱ����������Чʱ��������
    public System.DateTime LastUseTime;

    //����ʵ�����������ʱ�����¼һ����һ�ε�ʹ��ʱ��
    public PoolObject(string name, Object obj)
    {
        Name = name;
        Object = obj;
        LastUseTime = System.DateTime.Now;
    }
}
