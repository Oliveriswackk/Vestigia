
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class IntroManager : MonoBehaviour
{
    [Header("Configuración del vídeo")]
    [SerializeField] private VideoPlayer videoPlayer;

    [Header("Siguiente escena")]
    [SerializeField] private string nextScene = "Scene_02_Pueblo";

    private bool sceneChanging = false;

    private void Start()
    {
        videoPlayer.loopPointReached += OnVideoFinished;

        videoPlayer.Prepare();
        videoPlayer.prepareCompleted += OnVideoPrepared;
    }

    private void OnVideoPrepared(VideoPlayer vp)
    {
        videoPlayer.Play();
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        if (sceneChanging)
            return;

        sceneChanging = true;

        SceneManager.LoadScene(nextScene);
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
            videoPlayer.prepareCompleted -= OnVideoPrepared;
        }
    }
}
