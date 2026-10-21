
using UnityEngine;
using UnityEngine.Events;

public class StoryManager : MonoBehaviour
{
    public static StoryManager Instance { get; private set; }

    public UnityEvent OnRadioPuzzleCompleted = new UnityEvent();
    public UnityEvent OnPhotoCompleted = new UnityEvent();
    public UnityEvent OnLensCleaned = new UnityEvent();
    public UnityEvent OnObservatoryFound = new UnityEvent();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void StartRadioPuzzle()
    {
        if (GameProgress.Instance == null)
        {
            Debug.LogError("No se encontró GameProgress.");
            return;
        }

        if (GameProgress.Instance.radio_sintonizada)
        {
            Debug.Log("La radio ya está sintonizada.");
            return;
        }

        if (MobileBridge.Instance == null)
        {
            Debug.LogError("No se encontró MobileBridge.");
            return;
        }

        MobileBridge.Instance.StartPuzzle("radio", CompleteRadioPuzzle);
    }

    private void CompleteRadioPuzzle()
    {
        GameProgress.Instance.radio_sintonizada = true;

        Debug.Log("Historia: radio sintonizada.");

        OnRadioPuzzleCompleted.Invoke();
    }

    public void AddPhotoFragment()
    {
        if (GameProgress.Instance == null)
        {
            Debug.LogError("No se encontró GameProgress.");
            return;
        }

        if (GameProgress.Instance.photo_quantity >= 3)
        {
            Debug.Log("Ya se recogieron los tres fragmentos.");
            return;
        }

        GameProgress.Instance.photo_quantity++;

        Debug.Log(
            "Fragmentos: " +
            GameProgress.Instance.photo_quantity +
            "/3"
        );

        if (GameProgress.Instance.photo_quantity == 3)
        {
            Debug.Log("Historia: fotografía completa.");
            OnPhotoCompleted.Invoke();
        }
    }

    public void CleanLens()
    {
        if (GameProgress.Instance == null)
        {
            Debug.LogError("No se encontró GameProgress.");
            return;
        }

        if (GameProgress.Instance.lente_limpio)
        {
            Debug.Log("La lente ya está limpia.");
            return;
        }

        GameProgress.Instance.lente_limpio = true;

        Debug.Log("Historia: lente limpia.");

        OnLensCleaned.Invoke();
    }

    public void FindObservatory()
    {
        if (GameProgress.Instance == null)
        {
            Debug.LogError("No se encontró GameProgress.");
            return;
        }

        if (!GameProgress.Instance.lente_limpio)
        {
            Debug.Log("Primero debes limpiar la lente.");
            return;
        }

        if (GameProgress.Instance.obs_encontrado)
        {
            Debug.Log("El observatorio ya fue encontrado.");
            return;
        }

        GameProgress.Instance.obs_encontrado = true;

        Debug.Log("Historia: observatorio encontrado.");

        OnObservatoryFound.Invoke();
    }


    [ContextMenu("PRUEBA: Completar puzzle de radio")]
    private void TestRadioPuzzle()
    {
        StartRadioPuzzle();
    }



public void EnterLighthouse()
{
    if (GameProgress.Instance == null)
    {
        Debug.LogError("No se encontró GameProgress.");
        return;
    }

    if (!GameProgress.Instance.radio_sintonizada)
    {
        Debug.Log("La radio todavía no está sintonizada.");
        return;
    }

    if (SceneTransition.Instance == null)
        {
            Debug.LogError("No se encontró SceneTransition.");
            return;
        }

        SceneTransition.Instance.ChangeScene("Escena3_Faro");
    }

}
