
using UnityEngine;

public class LighthouseGate : MonoBehaviour
{
    [SerializeField] private Collider gateCollider;

    private bool unlocked;
    private bool transitioning;

    private void Awake()
    {
        if (gateCollider == null)
            gateCollider = GetComponent<Collider>();

        if (gateCollider != null)
            gateCollider.enabled = false;
    }

    private void Update()
    {
        if (unlocked || GameProgress.Instance == null)
            return;

        if (GameProgress.Instance.radio_sintonizada)
        {
            unlocked = true;
            gateCollider.enabled = true;

            Debug.Log("Acceso al Faro desbloqueado.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!unlocked || transitioning)
            return;

        if (other.GetComponentInParent<PlayerMovement>() == null)
            return;

        if (SceneTransition.Instance == null)
        {
            Debug.LogError("No se encontró SceneTransition.");
            return;
        }

        transitioning = true;
        SceneTransition.Instance.ChangeScene("Escena3_Faro");
    }
}
