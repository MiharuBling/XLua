using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XLua;

[XLua.LuaCallCSharp]
public static class UnityEx
{
    //�԰�ť���¼��ļ�����һ����չ
    //C#�������¼������C#������ί���ˣ�������Lua�ķ���������lua�ķ����������ʱ����
    public static void OnClickSet(this Button button, object callback)
    {
        XLua.LuaFunction func = callback as XLua.LuaFunction;
        //�����¼�ǰ�����Ƴ�����Ϊ��������������壬�������ÿ�δ�ui��������������������¼�
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(
            () =>
            {
                func?.Call();
            });
    }
    public static void OnValueChangedSet(this Slider slider, object callback)
    {
        XLua.LuaFunction func = callback as XLua.LuaFunction;
        //�����¼�ǰ�����Ƴ�
        slider.onValueChanged.RemoveAllListeners();
        slider.onValueChanged.AddListener(
            (float value) =>
            {
                func?.Call(value);
            });
    }
}


