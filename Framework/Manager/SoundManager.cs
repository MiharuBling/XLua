using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

public class SoundManager : MonoBehaviour
{
    AudioSource m_MusicAudio;//��������
    AudioSource m_SoundAudio;//��Ч

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
        //����������Ҫѭ�������Ҳ���ֱ�Ӳ���
        m_SoundAudio = this.gameObject.AddComponent<AudioSource>();
        m_SoundAudio.playOnAwake = false;
        m_SoundAudio.loop = true;

        //��Ч����Ҫѭ������Ч��һ���У���������playOnAwake
        m_SoundAudio = this.gameObject.AddComponent<AudioSource>();
        m_SoundAudio.loop = false;
    }
    public void PlayMusic(string name)
    {
        //����С��0.1f��Ϊ�������ˣ�ͬʱ��ʡ������ֱ�Ӳ�����
        if (this.MusicVolume < 0.1f)
        {
            return;
        }
        //����������������ڲ��ŵģ�Ҳ����
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
        //�����ȥ���ű�������
        Manager.Resource.LoadMusic(name, (UnityEngine.Object obj) =>
        {
            m_SoundAudio.clip = obj as AudioClip;
            m_SoundAudio.Play();
        });
    }
    //��ͣ���ű�������
    public void PauseMusic()
    {
        m_SoundAudio.Pause();
    }
    //�������ű�������
    public void OnUnPauseMusic()
    {
        m_SoundAudio.UnPause();
    }
    //ֹͣ���ű�������
    public void StopMusic()
    {
        m_SoundAudio.clip = null;
        m_SoundAudio.Stop();
    }
    //���ñ�����������
    public void SetMusicVolume(float value)
    {
        this.MusicVolume = value;
    }
    //������Ч����
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
