
using System;
using System.IO;
using UnityEngine;

public class GameProgress : MonoBehaviour
{
    public static GameProgress Instance { get; private set; }

    private string SavePath =>
        Path.Combine(Application.persistentDataPath, "vestigia_save.json");

    [Header("Pueblo - Radio")]
    public bool radio_sintonizada { get; private set; }

    [Header("Faro - Fragmentos de fotografía")]
    public bool fragmento_planta1 { get; private set; }
    public bool fragmento_planta2 { get; private set; }
    public bool fragmento_planta3 { get; private set; }

    // Se calcula automáticamente. No se modifica directamente.
    public int photo_quantity =>
        (fragmento_planta1 ? 1 : 0) +
        (fragmento_planta2 ? 1 : 0) +
        (fragmento_planta3 ? 1 : 0);

    [Header("Faro - Segundo piso")]
    public bool palanca_recogida { get; private set; }

    [Header("Faro - Puzzle")]
    public bool puzzle_completado { get; private set; }

    [Header("Faro - Mirador")]
    public bool lente_limpio { get; private set; }
    public bool obs_encontrado { get; private set; }

    [Serializable]
    private class SaveData
    {
        public bool radio_sintonizada;

        public bool fragmento_planta1;
        public bool fragmento_planta2;
        public bool fragmento_planta3;

        public bool palanca_recogida;
        public bool puzzle_completado;

        public bool lente_limpio;
        public bool obs_encontrado;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Asegurar que GameSystems sea un objeto raíz.
        transform.SetParent(null);

        DontDestroyOnLoad(gameObject);

        LoadProgress();
    }

    // ==========================================
    // PUEBLO - RADIO
    // ==========================================

    public void SetRadioSintonizada()
    {
        if (radio_sintonizada)
            return;

        radio_sintonizada = true;
        SaveProgress();
    }

    // ==========================================
    // FARO - FRAGMENTOS
    // ==========================================

    public void RegisterPhotoFragment(int planta)
    {
        bool registrado = false;

        switch (planta)
        {
            case 1:
                if (!fragmento_planta1)
                {
                    fragmento_planta1 = true;
                    registrado = true;
                }
                break;

            case 2:
                if (!fragmento_planta2)
                {
                    fragmento_planta2 = true;
                    registrado = true;
                }
                break;

            case 3:
                if (!fragmento_planta3)
                {
                    fragmento_planta3 = true;
                    registrado = true;
                }
                break;

            default:
                Debug.LogError(
                    "GameProgress: Número de planta inválido: " + planta
                );
                return;
        }

        if (registrado)
        {
            SaveProgress();

            Debug.Log(
                "Fragmento de planta " + planta +
                " registrado. Total: " + photo_quantity
            );
        }
    }

    // ==========================================
    // FARO - SEGUNDO PISO
    // ==========================================

    public void SetPalancaRecogida()
    {
        if (palanca_recogida)
            return;

        palanca_recogida = true;
        SaveProgress();
    }

    // ==========================================
    // FARO - PUZZLE
    // ==========================================

    public void SetPuzzleCompletado()
    {
        if (puzzle_completado)
            return;

        puzzle_completado = true;
        SaveProgress();
    }

    // ==========================================
    // FARO - MIRADOR
    // ==========================================

    public void SetLenteLimpio()
    {
        if (lente_limpio)
            return;

        lente_limpio = true;
        SaveProgress();
    }

    public void SetObservatorioEncontrado()
    {
        if (obs_encontrado)
            return;

        obs_encontrado = true;
        SaveProgress();
    }

    // ==========================================
    // GUARDADO
    // ==========================================

    public void SaveProgress()
    {
        SaveData data = new SaveData
        {
            radio_sintonizada = radio_sintonizada,

            fragmento_planta1 = fragmento_planta1,
            fragmento_planta2 = fragmento_planta2,
            fragmento_planta3 = fragmento_planta3,

            palanca_recogida = palanca_recogida,
            puzzle_completado = puzzle_completado,

            lente_limpio = lente_limpio,
            obs_encontrado = obs_encontrado
        };

        try
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SavePath, json);

            Debug.Log("Partida guardada correctamente.");
        }
        catch (Exception exception)
        {
            Debug.LogError(
                "GameProgress: Error al guardar: " + exception.Message
            );
        }
    }

    // ==========================================
    // CARGA
    // ==========================================

    private void LoadProgress()
    {
        if (!File.Exists(SavePath))
        {
            Debug.Log("No existe una partida guardada. Nueva partida.");
            return;
        }

        try
        {
            string json = File.ReadAllText(SavePath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            if (data == null)
            {
                Debug.LogError("GameProgress: Archivo de guardado inválido.");
                return;
            }

            radio_sintonizada = data.radio_sintonizada;

            fragmento_planta1 = data.fragmento_planta1;
            fragmento_planta2 = data.fragmento_planta2;
            fragmento_planta3 = data.fragmento_planta3;

            palanca_recogida = data.palanca_recogida;
            puzzle_completado = data.puzzle_completado;

            lente_limpio = data.lente_limpio;
            obs_encontrado = data.obs_encontrado;

            Debug.Log(
                "Partida cargada. Fragmentos: " + photo_quantity
            );
        }
        catch (Exception exception)
        {
            Debug.LogError(
                "GameProgress: Error al cargar: " + exception.Message
            );
        }
    }


    
#if UNITY_EDITOR
    [ContextMenu("DESARROLLO: REINICIAR PARTIDA")]
    public void ResetProgress()
    {
        radio_sintonizada = false;

        fragmento_planta1 = false;
        fragmento_planta2 = false;
        fragmento_planta3 = false;

        palanca_recogida = false;
        puzzle_completado = false;

        lente_limpio = false;
        obs_encontrado = false;

        // Sobrescribir el archivo JSON con el progreso reiniciado.
        SaveProgress();

        Debug.Log(
            "====================================\n" +
            "PARTIDA REINICIADA CORRECTAMENTE\n" +
            "Fragmentos: " + photo_quantity + "/3\n" +
            "Archivo: " + SavePath + "\n" +
            "===================================="
        );
    }
#endif


}
