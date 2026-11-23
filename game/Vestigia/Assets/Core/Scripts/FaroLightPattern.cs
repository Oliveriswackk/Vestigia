
using UnityEngine;
using System.Collections;

public class FaroLightPattern : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private Light faroLight;

    [Header("Patrón en segundos")]
    [SerializeField] private float[] pattern =
    {
        1f, 1f,
        1f, 1f,
        1f, 3f,
        1f, 3f
    };

    private void Start()
    {
        if (faroLight == null)
            faroLight = GetComponent<Light>();

        StartCoroutine(PlayPattern());
    }

    private IEnumerator PlayPattern()
    {
        while (true)
        {
            for (int i = 0; i < pattern.Length; i++)
            {
                // Índices pares: encender.
                // Índices impares: apagar.
                faroLight.enabled = (i % 2 == 0);

                yield return new WaitForSeconds(pattern[i]);
            }
        }
    }
}
