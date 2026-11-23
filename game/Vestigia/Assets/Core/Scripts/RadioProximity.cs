
using UnityEngine;

public class RadioProximity : MonoBehaviour
{
    [Header("Jugador")]
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Audios")]
    [SerializeField] private AudioSource radioAudio;
    [SerializeField] private AudioSource discoveryAudio;

    [Header("Distancia")]
    [SerializeField] private float radioDistance = 5f;
    [SerializeField] private float discoveryDistance = 3.5f;

    [Header("Campo de vision")]
    [Tooltip("Angulo total. 120 grados significa 60 a cada lado.")]
    [Range(10f, 360f)]
    [SerializeField] private float fieldOfViewAngle = 120f;

    // Evita repetir el sonido mientras el jugador
    // permanezca dentro de la distancia de descubrimiento.
    private bool hasPlayedDiscovery = false;

    private void Start()
    {
        if (playerMovement == null ||
            radioAudio == null ||
            discoveryAudio == null)
        {
            Debug.LogError(
                "RadioProximity: Asigna PlayerMovement y ambos Audio Sources.",
                this
            );

            enabled = false;
            return;
        }

        radioAudio.playOnAwake = false;
        radioAudio.loop = true;
        radioAudio.Stop();
        radioAudio.volume = 0f;

        discoveryAudio.playOnAwake = false;
        discoveryAudio.loop = false;
        discoveryAudio.Stop();

        hasPlayedDiscovery = false;
    }

    private void Update()
    {
        Vector3 playerPosition =
            playerMovement.transform.position;

        Vector3 radioPosition = transform.position;

        // Ignorar la altura para calcular la distancia.
        playerPosition.y = 0f;
        radioPosition.y = 0f;

        float distance = Vector3.Distance(
            playerPosition,
            radioPosition
        );

        // =========================================
        // 1. SONIDO AMBIENTAL DE LA RADIO
        // Funciona únicamente según la distancia.
        // =========================================

        bool canHearRadio = distance <= radioDistance;

        if (canHearRadio)
        {
            if (!radioAudio.isPlaying)
                radioAudio.Play();

            float volumeFactor = 1f - Mathf.Clamp01(
                distance / radioDistance
            );

            radioAudio.volume = volumeFactor;
        }
        else
        {
            if (radioAudio.isPlaying)
                radioAudio.Stop();

            radioAudio.volume = 0f;
        }

        // =========================================
        // 2. DETECCION POR CAMPO DE VISION
        // =========================================

        Vector3 facingDirection =
            playerMovement.LastFacingDirection;

        facingDirection.y = 0f;

        Vector3 directionToRadio =
            transform.position -
            playerMovement.transform.position;

        directionToRadio.y = 0f;

        bool hasFacingDirection =
            facingDirection.sqrMagnitude > 0.001f;

        bool hasDirectionToRadio =
            directionToRadio.sqrMagnitude > 0.001f;

        bool isWithinFieldOfView = false;

        if (hasFacingDirection && hasDirectionToRadio)
        {
            float angle = Vector3.Angle(
                facingDirection.normalized,
                directionToRadio.normalized
            );

            isWithinFieldOfView =
                angle <= fieldOfViewAngle * 0.5f;
        }

        // =========================================
        // 3. DESCUBRIMIENTO
        // Requiere distancia Y campo de vision.
        // Solo reproduce una vez por aproximacion.
        // =========================================

        bool isWithinDiscoveryDistance =
            distance <= discoveryDistance;

        bool canDiscoverRadio =
            isWithinDiscoveryDistance &&
            isWithinFieldOfView;

        if (canDiscoverRadio && !hasPlayedDiscovery)
        {
            if (discoveryAudio.clip != null)
            {
                discoveryAudio.PlayOneShot(
                    discoveryAudio.clip
                );

                hasPlayedDiscovery = true;
            }
        }

        // No reiniciar al dejar de mirar:
        // solo reiniciar cuando se aleje de la radio.
        if (distance > discoveryDistance)
        {
            hasPlayedDiscovery = false;
        }
    }

    private void OnDisable()
    {
        if (radioAudio != null)
            radioAudio.Stop();

        if (discoveryAudio != null)
            discoveryAudio.Stop();
    }
}
