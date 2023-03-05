using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//专门用来存放ObjectPool里放置的不同对象的数据类
public class PoolObject
{
    //具体对象
    public Object Object;

    //对象名字
    public string Name;

    //最后一次使用时间，针对不同类型的对象有不同的有效时长，超过有效时长就销毁
    public System.DateTime LastUseTime;

    //生成实例，放入池子时，会记录一下上一次的使用时间
    public PoolObject(string name, Object obj)
    {
        Name = name;
        Object = obj;
        LastUseTime = System.DateTime.Now;
    }
}
