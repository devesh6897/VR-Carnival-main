using UnityEngine;

public class CabinController : MonoBehaviour
{
    public Animator cabinAnimator; // Assign this in the Inspector

    private bool isPlayerInCabin = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("PlayerController"))
        {
            other.transform.SetParent(transform);
            isPlayerInCabin = true;
            StartCabinAnimation();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("PlayerController"))
        {
            other.transform.SetParent(null);
            isPlayerInCabin = false;
        }
    }

    public void StartCabinAnimation()
    {
        if (isPlayerInCabin && cabinAnimator != null)
        {
            cabinAnimator.SetTrigger("start"); // Make sure "start" Trigger exists in Animator
        }
    }
}
