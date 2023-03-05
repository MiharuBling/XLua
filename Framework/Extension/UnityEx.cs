using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XLua;

[XLua.LuaCallCSharp]
public static class UnityEx
{
    //对按钮的事件的监听做一个扩展
    //C#监听的事件变成了C#的匿名委托了，而不是Lua的方法，所以lua的方法变成了临时变量
    public static void OnClickSet(this Button button, object callback)
    {
        XLua.LuaFunction func = callback as XLua.LuaFunction;
        //监听事件前，先移除，因为不能销毁这个物体，所以如果每次打开ui都监听，会监听无数个事件
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
        //监听事件前，先移除
        slider.onValueChanged.RemoveAllListeners();
        slider.onValueChanged.AddListener(
            (float value) =>
            {
                func?.Call(value);
            });
    }
}


