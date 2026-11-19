
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class RandomSoundEmitter : MonoBehaviour
{
    [Header("Sonidos disponibles")]
    [SerializeField] private AudioClip[] sounds;

    [Header("Configuración")]
    [SerializeField] private float interval = 120f;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    private void Start()
    {
        StartCoroutine(PlayRandomSound());
    }

    private IEnumerator PlayRandomSound()
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);

            if (sounds == null || sounds.Length == 0)
                continue;

            AudioClip randomSound = sounds[Random.Range(0, sounds.Length)];

            audioSource.PlayOneShot(randomSound);
        }
    }
}
