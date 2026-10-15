using System;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Singleton that plays the music of the active scene. Created at startup from the MusicManager prefab in Resources,
/// where each track lists the scenes, or folders of scenes, it plays in.
/// Crossfades when the player enters a scene with different music, and keeps the music going between scenes that share it.
/// </summary>
public class MusicManager : MonoBehaviour
{
    /// <summary>
    /// Name of the prefab in a Resources folder that holds the music setup. Without it, no music plays.
    /// </summary>
    private const string ResourcesPrefabName = "MusicManager";

    private static MusicManager m_instance;

    /// <summary>
    /// Instance of MusicManager. Null if there is no MusicManager prefab in Resources.
    /// </summary>
    public static MusicManager Instance => m_instance;

    [Serializable]
    private class MusicTrack
    {
        [Tooltip("Only labels the track in this list.")]
        public string Name;
        public AudioClip Music;
#if UNITY_EDITOR
        [Tooltip("Scenes that play this music. Drop in a folder to cover every scene inside it.")]
        public UnityEngine.Object[] Scenes;
#endif
        [HideInInspector] public string[] ScenePaths;
    }

    [Header("Music")]
    [Tooltip("Plays in every scene that no track lists.")]
    [SerializeField] AudioClip _defaultMusic;
    [Tooltip("Music for specific scenes. A scene listed directly wins over a folder it is in.")]
    [SerializeField] MusicTrack[] _tracks;

    [Header("Playback")]
    [SerializeField, Range(0f, 1f)] float _volume = 0.7f;
    [Tooltip("Seconds the old music takes to fade out while the new music fades in.")]
    [SerializeField] float _fadeDuration = 1.5f;

    // Two sources, so the music of the previous scene can fade out while the new music fades in
    private AudioSource _current;
    private AudioSource _previous;
    private float _currentFade;
    private float _previousFade;

    #region SingletonHandling
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateOnStartup()
    {
        if (m_instance == null)
        {
            MusicManager prefab = Resources.Load<MusicManager>(ResourcesPrefabName);
            if (prefab == null)
            {
                Debug.LogWarning($"[MusicManager] No '{ResourcesPrefabName}' prefab in a Resources folder, so no music plays.");
                return;
            }
            Instantiate(prefab).name = ResourcesPrefabName;
        }

        // The first scene is already active, so no scene change announces it
        m_instance.PlaySceneMusic(SceneManager.GetActiveScene());
    }

    private void Awake()
    {
        if (m_instance != null && m_instance != this)
        {
            // Destroy only happens at the end of the frame, keep Update from running until then
            enabled = false;
            Destroy(gameObject);
            return;
        }
        m_instance = this;

        // DontDestroyOnLoad only works on root objects
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        _current = CreateSource();
        _previous = CreateSource();
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    private void OnDestroy()
    {
        if (m_instance != this)
            return;

        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
        m_instance = null;
    }

    private AudioSource CreateSource()
    {
        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = true;
        source.volume = 0f;
        // Highest priority, so music is never cut off to free a voice for sound effects
        source.priority = 0;
        return source;
    }
    #endregion

    #region MusicHandling
    /// <summary>
    /// Fades over to new music. Does nothing if that music is already playing.
    /// </summary>
    /// <param name="music">Music to loop, or null to fade to silence</param>
    public void Play(AudioClip music)
    {
        if (_current.clip == music)
            return;

        // The music that was playing becomes the one fading out
        (_current, _previous) = (_previous, _current);
        (_currentFade, _previousFade) = (_previousFade, _currentFade);

        // Coming back before the music finished fading out fades it back in from where it is
        if (_current.clip == music && _current.isPlaying)
            return;

        _current.Stop();
        _current.clip = music;
        _current.volume = 0f;
        _currentFade = 0f;
        if (music != null)
            _current.Play();
    }

    private void Update()
    {
        // Unscaled time, so fades keep going while the game is paused
        float step = _fadeDuration > 0f ? Time.unscaledDeltaTime / _fadeDuration : 1f;
        _currentFade = Mathf.MoveTowards(_currentFade, 1f, step);
        _previousFade = Mathf.MoveTowards(_previousFade, 0f, step);

        _current.volume = _currentFade * _volume;
        _previous.volume = _previousFade * _volume;

        if (_previousFade <= 0f && _previous.isPlaying)
            _previous.Stop();
    }

    private void OnActiveSceneChanged(Scene previousScene, Scene newScene)
    {
        PlaySceneMusic(newScene);
    }

    private void PlaySceneMusic(Scene scene)
    {
        Play(GetMusic(scene.path));
    }

    /// <summary>
    /// Music of the track that lists the scene most directly, or the default music if no track lists it.
    /// </summary>
    private AudioClip GetMusic(string scenePath)
    {
        AudioClip music = _defaultMusic;
        int bestMatch = -1;

        foreach (MusicTrack track in _tracks)
        {
            foreach (string trackPath in track.ScenePaths)
            {
                int match = GetMatchStrength(trackPath, scenePath);
                if (match > bestMatch)
                {
                    bestMatch = match;
                    music = track.Music;
                }
            }
        }
        return music;
    }

    /// <summary>
    /// How directly a track's scene or folder path covers a scene: the scene itself beats any folder,
    /// and a folder beats the folders around it. -1 if it doesn't cover the scene.
    /// </summary>
    private static int GetMatchStrength(string trackPath, string scenePath)
    {
        if (string.IsNullOrEmpty(trackPath))
            return -1;

        if (trackPath == scenePath)
            return int.MaxValue;

        return scenePath.StartsWith(trackPath + "/", StringComparison.Ordinal) ? trackPath.Length : -1;
    }
    #endregion

#if UNITY_EDITOR
    #region EditorHandling
    /// <summary>
    /// Scene and folder assets only exist in the editor, so their paths are stored for builds.
    /// </summary>
    private void OnValidate()
    {
        foreach (MusicTrack track in _tracks)
        {
            track.ScenePaths = new string[track.Scenes.Length];
            for (int i = 0; i < track.Scenes.Length; i++)
            {
                UnityEngine.Object scene = track.Scenes[i];
                string path = scene != null ? AssetDatabase.GetAssetPath(scene) : string.Empty;

                if (scene != null && scene is not SceneAsset && !AssetDatabase.IsValidFolder(path))
                {
                    Debug.LogWarning($"[MusicManager] '{scene.name}' in track '{track.Name}' is not a scene or a folder, so it was removed.", this);
                    track.Scenes[i] = null;
                    path = string.Empty;
                }
                track.ScenePaths[i] = path;
            }
        }
    }
    #endregion
#endif
}
