using UnityEngine;

public class ThrownPebble : MonoBehaviour
{
    public float damage = 5f;

    void OnCollisionEnter(Collision collision)
    {
        EnemyHealth eh = collision.gameObject.GetComponent<EnemyHealth>();
        if (eh)
        {
            eh.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
