using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

public class SoundManager : MonoBehaviour
{
    AudioSource m_MusicAudio;//背景音乐
    AudioSource m_SoundAudio;//音效

    private float SoundVolume
    {
        get { return PlayerPrefs.GetFloat("SoundVolume", 1.0f); }
        set
        {
            m_SoundAudio.volume = value;
            PlayerPrefs.SetFloat("SoundVolume", value);
        }
    }
    private float MusicVolume
    {
        get { return PlayerPrefs.GetFloat("MusicVolume", 1.0f); }
        set
        {
            m_MusicAudio.volume = value;
            PlayerPrefs.SetFloat("MusicVolume", value);
        }
    }
    private void Awake()
    {
        //背景音乐需要循环，并且不能直接播放
        m_SoundAudio = this.gameObject.AddComponent<AudioSource>();
        m_SoundAudio.playOnAwake = false;
        m_SoundAudio.loop = true;

        //音效不需要循环，音效不一定有，不用设置playOnAwake
        m_SoundAudio = this.gameObject.AddComponent<AudioSource>();
        m_SoundAudio.loop = false;
    }
    public void PlayMusic(string name)
    {
        //音量小于0.1f因为听不见了，同时节省开销，直接不播放
        if (this.MusicVolume < 0.1f)
        {
            return;
        }
        //如果背景音乐是正在播放的，也跳过
        string oldName = "";
        if (m_MusicAudio.clip != null)
        {
            oldName = m_MusicAudio.clip.name;
        }
        if (oldName == name.Substring(0, name.IndexOf(".")))
        {
            //m_SoundAudio.Play();
            return;
        }
        //否则就去播放背景音乐
        Manager.Resource.LoadMusic(name, (UnityEngine.Object obj) =>
        {
            m_SoundAudio.clip = obj as AudioClip;
            m_SoundAudio.Play();
        });
    }
    //暂停播放背景音乐
    public void PauseMusic()
    {
        m_SoundAudio.Pause();
    }
    //继续播放背景音乐
    public void OnUnPauseMusic()
    {
        m_SoundAudio.UnPause();
    }
    //停止播放背景音乐
    public void StopMusic()
    {
        m_SoundAudio.clip = null;
        m_SoundAudio.Stop();
    }
    //设置背景音乐音量
    public void SetMusicVolume(float value)
    {
        this.MusicVolume = value;
    }
    //设置音效音量
    public void SetSoundVolume(float value)
    {
        this.SoundVolume = value;
    }

    [CSharpCallLua]
    public static List<Type> mymodule_cs_call_lua_list = new List<Type>()
    {
        typeof(UnityEngine.Events.UnityAction<float>),
    };
}
