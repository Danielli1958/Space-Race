using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Music Clips")]
    public AudioClip menuMusic;
    public AudioClip gameMusic;

    private AudioSource audioSource;

    private void Awake()
    {
        // Destroy duplicate MusicManagers
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            Debug.LogError("MusicManager requires an AudioSource component!");
            return;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        PlayMenuMusic();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "GameScene")
        {
            PlayGameMusic();
        }
        else
        {
            PlayMenuMusic();
        }
    }

    private void PlayMenuMusic()
    {
        if (audioSource.clip == menuMusic)
            return;

        audioSource.clip = menuMusic;
        audioSource.loop = true;
        audioSource.Play();
    }

    private void PlayGameMusic()
    {
        if (audioSource.clip == gameMusic)
            return;

        audioSource.clip = gameMusic;
        audioSource.loop = true;
        audioSource.Play();
    }
}