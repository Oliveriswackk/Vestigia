
using UnityEngine;

public class GameProgress : MonoBehaviour
{
    public static GameProgress Instance { get; private set; }

    [Header("Pueblo - Radio")]
    public bool radio_sintonizada = false;

    [Header("Faro - Fotografía")]
    [Range(0, 3)]
    public int photo_quantity = 0;

    [Header("Faro - Mirador")]
    public bool lente_limpio = false;
    public bool obs_encontrado = false;

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
}
