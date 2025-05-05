using UnityEngine;
using System.Collections;

public class CabinController : MonoBehaviour
{
    public Animator cabinAnimator; // Assign in Inspector
    public GameObject firecracker; // Assign firecracker GameObject
    public float firecrackerDelay = 2f; // Set delay time in Inspector

    private bool isPlayerInCabin = false;
    private Coroutine firecrackerCoroutine;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("PlayerController"))
        {
            other.transform.SetParent(transform);
            isPlayerInCabin = true;
            StartCabinAnimation();

            if (firecracker != null)
            {
                if (firecrackerCoroutine != null)
                    StopCoroutine(firecrackerCoroutine);

                firecrackerCoroutine = StartCoroutine(ActivateFirecrackerWithDelay());
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("PlayerController"))
        {
            other.transform.SetParent(null);
            isPlayerInCabin = false;

            if (firecrackerCoroutine != null)
            {
                StopCoroutine(firecrackerCoroutine);
                firecrackerCoroutine = null;
            }

            if (firecracker != null)
                firecracker.SetActive(false); // Immediately deactivate
            StopCabinAnimation();

        }
    }

    private IEnumerator ActivateFirecrackerWithDelay()
    {
        yield return new WaitForSeconds(firecrackerDelay);
        if (isPlayerInCabin && firecracker != null)
            firecracker.SetActive(true);
    }

    public void StartCabinAnimation()
    {
        if (isPlayerInCabin && cabinAnimator != null)
        {
            cabinAnimator.SetTrigger("start");
        }
    }
    public void StopCabinAnimation()
    {
        if (cabinAnimator != null)
        {
            cabinAnimator.ResetTrigger("start"); // optional: cancel the trigger
            cabinAnimator.SetTrigger("stop");    // you can use a "stop" trigger, or use SetBool("isRunning", false)
        }
    }
}
