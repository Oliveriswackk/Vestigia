
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class MobileBridge : MonoBehaviour
{
    public static MobileBridge Instance { get; private set; }

    [Header("Simulación de Móvil")]
    [SerializeField] private float simulationDelay = 3f;

    public bool IsPuzzleRunning { get; private set; }

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

    public void StartPuzzle(string puzzleId, UnityAction onCompleted)
    {
        if (IsPuzzleRunning)
        {
            Debug.LogWarning("Ya hay un puzzle en ejecución.");
            return;
        }

        StartCoroutine(SimulatePuzzle(puzzleId, onCompleted));
    }

    private IEnumerator SimulatePuzzle(
        string puzzleId,
        UnityAction onCompleted)
    {
        IsPuzzleRunning = true;

        Debug.Log("Móvil: iniciando puzzle " + puzzleId);

        yield return new WaitForSeconds(simulationDelay);

        Debug.Log("Móvil: puzzle completado " + puzzleId);

        IsPuzzleRunning = false;

        onCompleted?.Invoke();
    }
}
