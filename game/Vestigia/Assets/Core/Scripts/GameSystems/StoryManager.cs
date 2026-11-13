
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
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }

    // ==========================================
    // RADIO
    // ==========================================

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
        if (GameProgress.Instance == null)
        {
            Debug.LogError("No se encontró GameProgress.");
            return;
        }

        GameProgress.Instance.SetRadioSintonizada();

        Debug.Log("Historia: radio sintonizada.");

        OnRadioPuzzleCompleted.Invoke();
    }

    // ==========================================
    // FOTOGRAFÍA
    // ==========================================

    public void AddPhotoFragment(int planta)
    {
        if (GameProgress.Instance == null)
        {
            Debug.LogError("No se encontró GameProgress.");
            return;
        }

        int cantidadAnterior = GameProgress.Instance.photo_quantity;

        GameProgress.Instance.RegisterPhotoFragment(planta);

        int cantidadActual = GameProgress.Instance.photo_quantity;

        // Evitar eventos duplicados si el fragmento ya estaba recogido.
        if (cantidadActual == cantidadAnterior)
            return;

        Debug.Log(
            "Fragmentos: " +
            cantidadActual +
            "/3"
        );

        if (cantidadActual == 3)
        {
            Debug.Log("Historia: fotografía completa.");
            OnPhotoCompleted.Invoke();
        }
    }

    // ==========================================
    // LENTE
    // ==========================================

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

        GameProgress.Instance.SetLenteLimpio();

        Debug.Log("Historia: lente limpia.");

        OnLensCleaned.Invoke();
    }

    // ==========================================
    // OBSERVATORIO
    // ==========================================

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

        GameProgress.Instance.SetObservatorioEncontrado();

        Debug.Log("Historia: observatorio encontrado.");

        OnObservatoryFound.Invoke();
    }

    // ==========================================
    // TRANSICIONES DE ESCENA
    // ==========================================

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

    // ==========================================
    // PRUEBAS
    // ==========================================

    [ContextMenu("PRUEBA: Completar puzzle de radio")]
    private void TestRadioPuzzle()
    {
        StartRadioPuzzle();
    }
}
