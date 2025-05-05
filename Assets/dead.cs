using UnityEngine;

public class TeleportOnTrigger : MonoBehaviour
{
    public Transform teleportTarget; // Assign the destination transform in Inspector

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("PlayerController"))
        {
            if (teleportTarget != null)
            {
                other.transform.position = teleportTarget.position;
            }
            else
            {
                Debug.LogWarning("Teleport target is not assigned.");
            }
        }
    }
}
