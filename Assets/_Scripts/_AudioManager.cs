using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [System.Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;

        [Range(0f, 1f)] public float volume;
        [Range(0.1f, 3f)] public float pitch;
        [Range(0f, 1f)]public float spatialBlend = 1f;

        public bool loop;

        [HideInInspector] public AudioSource AudioSource;
    }

    public Sound[] sounds;
    private Dictionary<GameObject, Dictionary<string, AudioSource>> gameObjectAudioSources = new Dictionary<GameObject, Dictionary<string, AudioSource>>();
    private AudioSource OneshotAudioSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            IntializeAudioSource();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        FadeIn("Main BGM", 1.5f);
        FadeIn("Main Ambience", 1.5f);
    }

    private void IntializeAudioSource()
    {
        foreach (Sound s in sounds)
        {
            s.AudioSource = gameObject.AddComponent<AudioSource>();
            s.AudioSource.clip = s.clip;
            s.AudioSource.volume = s.volume;
            s.AudioSource.pitch = s.pitch;
            s.AudioSource.loop = s.loop;
            s.AudioSource.spatialBlend = s.spatialBlend;
        }
    }

    public void Play(string name, GameObject target = null)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + "not found");
            return;
        }

        AudioSource sourceToUse = target != null ? GetAudioSource(target, name) : s.AudioSource;
        sourceToUse.clip = s.clip;
        sourceToUse.volume = s.volume;
        sourceToUse.pitch = s.pitch;
        sourceToUse.loop = s.loop;
        sourceToUse.spatialBlend = s.spatialBlend;
        sourceToUse.Play();
    }

    public void Stop(string name, GameObject target = null)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }

        AudioSource sourceToUse = target != null ? GetAudioSource(target, name) : s.AudioSource;
        sourceToUse.Stop();
    }

    public void Pause(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        s.AudioSource.Pause();
    }

    public void Resume(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        s.AudioSource.UnPause();
    }

    public void SetVolume(string name, float volume)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        s.AudioSource.volume = Mathf.Clamp01(volume);
    }

    public void SetPitch(string name, float pitch)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        s.AudioSource.pitch = Mathf.Clamp(pitch, 0.1f, 3f);
    }

    public void FadeIn(string name, float duration)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        StartCoroutine(FadeCoroutine(s, 0f, s.volume, duration, true));
    }

    public void FadeOut(string name, float duration)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        StartCoroutine(FadeCoroutine(s, s.AudioSource.volume, 0f, duration, false));
    }

    private IEnumerator FadeCoroutine(Sound sound, float startVolume, float endVolume, float duration, bool playOnStart)
    {
        float elapsedTime = 0f;
        sound.AudioSource.volume = startVolume;

        if (playOnStart)
            sound.AudioSource.Play();

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            sound.AudioSource.volume = Mathf.Lerp(startVolume, endVolume, elapsedTime / duration);
            yield return null;
        }

        sound.AudioSource.volume = endVolume;

        if (!playOnStart && endVolume == 0f)
            sound.AudioSource.Stop();
    }

    public bool IsPlaying(string name, GameObject target = null)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return false;
        }

        return s.AudioSource.isPlaying;
    }

    public void PlayOneShot(string name, GameObject target = null)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }

        AudioSource sourceToUse = target != null ? GetAudioSource(target, name) : OneshotAudioSource;
        sourceToUse.PlayOneShot(s.clip, s.volume);
    }

    private AudioSource GetAudioSource(GameObject target, string soundName)
    {
        if (!gameObjectAudioSources.ContainsKey(target))
        {
            gameObjectAudioSources[target] = new Dictionary<string, AudioSource>();
        }

        if (!gameObjectAudioSources[target].ContainsKey(soundName))
        {
            AudioSource newSource = target.AddComponent<AudioSource>();
            gameObjectAudioSources[target][soundName] = newSource;
        }

        return gameObjectAudioSources[target][soundName];
    }
}
