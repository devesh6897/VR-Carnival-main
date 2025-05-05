using UnityEngine;

public class TeleportOnDeadCollision : MonoBehaviour
{
    [SerializeField] private Transform targetObject; // Assign this in the Inspector

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Dead") && targetObject != null)
        {
            transform.position = targetObject.position;
        }
    }
}
