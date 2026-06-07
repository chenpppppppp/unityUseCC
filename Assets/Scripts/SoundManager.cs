using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 音效 + 背景音乐管理器
/// 音效文件放 Resources/Sounds/ 文件夹
/// </summary>
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("音量")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 1f)] public float bgmVolume = 0.5f;

    // 音效
    private AudioSource sfxSource;
    private Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();

    // 背景音乐
    private AudioSource bgmSource;
    private AudioClip currentBGM;
    private float fadeSpeed = 0.5f;
    private Coroutine fadeCoroutine;

    // BGM 文件名列表
    private readonly string[] menuBGM = { "bgm_menu" };
    private readonly string[] battleBGM = { "bgm_battle" };
    private readonly string[] bossBGM = { "bgm_boss" };

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        // SFX 音源
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
        sfxSource.spatialBlend = 0f;

        // BGM 音源
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.playOnAwake = false;
        bgmSource.loop = true;
        bgmSource.spatialBlend = 0f;
        bgmSource.volume = bgmVolume * masterVolume;

        // 预加载所有音效
        string[] names = {
            "marble_launch", "marble_bounce", "marble_hit",
            "enemy_death", "enemy_spawn",
            "pickup_exp", "pickup_gold", "pickup_health",
            "level_up", "boss_phase", "boss_death",
            "game_win", "game_over", "button_click"
        };

        foreach (string name in names)
        {
            AudioClip clip = Resources.Load<AudioClip>($"Sounds/{name}");
            if (clip != null) clips[name] = clip;
        }

        // 预加载 BGM
        foreach (string name in menuBGM) LoadBGM(name);
        foreach (string name in battleBGM) LoadBGM(name);
        foreach (string name in bossBGM) LoadBGM(name);

        if (clips.Count == 0)
            Debug.LogWarning("[SoundManager] 未找到音效文件！游戏将静音运行。");
        else
            Debug.Log($"[SoundManager] 已加载 {clips.Count} 个音效");
    }

    void LoadBGM(string name)
    {
        if (!clips.ContainsKey(name))
        {
            AudioClip clip = Resources.Load<AudioClip>($"Sounds/{name}");
            if (clip != null) clips[name] = clip;
        }
    }

    // ========== 音效 ==========

    public void Play(string name)
    {
        if (!clips.ContainsKey(name)) return;
        sfxSource.PlayOneShot(clips[name], sfxVolume * masterVolume);
    }

    public void PlayRandom(params string[] names)
    {
        if (names == null || names.Length == 0) return;
        string name = names[Random.Range(0, names.Length)];
        Play(name);
    }

    // ========== 背景音乐 ==========

    public void PlayBGM(string name)
    {
        if (!clips.ContainsKey(name))
        {
            AudioClip clip = Resources.Load<AudioClip>($"Sounds/{name}");
            if (clip != null) clips[name] = clip;
        }

        if (!clips.ContainsKey(name) || clips[name] == null)
        {
            Debug.LogWarning($"[SoundManager] BGM '{name}' 未找到，请将文件放入 Resources/Sounds/");
            return;
        }

        StartCoroutine(FadeToBGM(clips[name]));
    }

    public void PlayRandomBGM(string[] choices)
    {
        if (choices == null || choices.Length == 0) return;
        string name = choices[Random.Range(0, choices.Length)];
        PlayBGM(name);
    }

    /// <summary>播放菜单背景音乐</summary>
    public void PlayMenuBGM()
    {
        PlayRandomBGM(menuBGM);
    }

    /// <summary>播放战斗背景音乐</summary>
    public void PlayBattleBGM()
    {
        PlayRandomBGM(battleBGM);
    }

    /// <summary>播放 Boss 战背景音乐</summary>
    public void PlayBossBGM()
    {
        PlayRandomBGM(bossBGM);
    }

    public void StopBGM()
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        bgmSource.Stop();
        currentBGM = null;
    }

    public void PauseBGM()
    {
        bgmSource.Pause();
    }

    public void ResumeBGM()
    {
        bgmSource.UnPause();
    }

    /// <summary>淡入淡出切换 BGM</summary>
    IEnumerator FadeToBGM(AudioClip targetClip)
    {
        // 淡出当前 BGM
        if (currentBGM != null && bgmSource.isPlaying)
        {
            float startVol = bgmSource.volume;
            for (float t = 0; t < 1f; t += Time.unscaledDeltaTime * fadeSpeed)
            {
                bgmSource.volume = Mathf.Lerp(startVol, 0f, t);
                yield return null;
            }
            bgmSource.Stop();
        }

        // 切换并淡入
        currentBGM = targetClip;
        bgmSource.clip = targetClip;
        bgmSource.volume = 0f;
        bgmSource.Play();

        float targetVol = bgmVolume * masterVolume;
        for (float t = 0; t < 1f; t += Time.unscaledDeltaTime * fadeSpeed)
        {
            bgmSource.volume = Mathf.Lerp(0f, targetVol, t);
            yield return null;
        }
        bgmSource.volume = targetVol;
    }

    /// <summary>设置 BGM 音量（不中断播放）</summary>
    public void SetBGMVolume(float vol)
    {
        bgmVolume = Mathf.Clamp01(vol);
        if (bgmSource.isPlaying)
            bgmSource.volume = bgmVolume * masterVolume;
    }

    /// <summary>设置 SFX 音量</summary>
    public void SetSFXVolume(float vol)
    {
        sfxVolume = Mathf.Clamp01(vol);
    }

    /// <summary>设置主音量（同时影响 SFX 和 BGM）</summary>
    public void SetMasterVolume(float vol)
    {
        masterVolume = Mathf.Clamp01(vol);
        if (bgmSource.isPlaying)
            bgmSource.volume = bgmVolume * masterVolume;
    }
}
