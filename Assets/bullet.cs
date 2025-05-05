using UnityEngine;

public class DestroyBothOnBalloonCollision : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("balloon"))
        {
            Destroy(collision.gameObject); // Destroy the balloon
            Destroy(gameObject);           // Destroy this object
        }
    }
}
