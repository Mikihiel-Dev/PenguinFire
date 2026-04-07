using UnityEngine;

public class NPCPenguinAI : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float iceHalfSize = 48f;
    public float turnMargin = 5f;

    private Rigidbody rb;
    private float yaw;
    private float targetYaw;
    private float changeTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        yaw = Random.Range(0f, 360f);
        targetYaw = yaw;
        changeTimer = Random.Range(2f, 5f);
    }

    void Update()
    {
        changeTimer -= Time.deltaTime;
        if (changeTimer <= 0f)
        {
            targetYaw = yaw + Random.Range(-90f, 90f);
            changeTimer = Random.Range(2f, 6f);
        }

        // Turn away from edge
        Vector3 pos = transform.position;
        float limit = iceHalfSize - turnMargin;

        if (pos.x > limit) targetYaw = 180f + Random.Range(-45f, 45f);
        else if (pos.x < -limit) targetYaw = 0f + Random.Range(-45f, 45f);
        if (pos.z > limit) targetYaw = 270f + Random.Range(-45f, 45f);
        else if (pos.z < -limit) targetYaw = 90f + Random.Range(-45f, 45f);

        // Clamp position hard so they never leave
        pos.x = Mathf.Clamp(pos.x, -iceHalfSize, iceHalfSize);
        pos.z = Mathf.Clamp(pos.z, -iceHalfSize, iceHalfSize);
        transform.position = pos;

        // Smooth turn
        yaw = Mathf.LerpAngle(yaw, targetYaw, Time.deltaTime * 2f);
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);

        // Move forward
        Vector3 forward = transform.forward * moveSpeed;
        Vector3 vel = rb.linearVelocity;
        rb.linearVelocity = new Vector3(forward.x, vel.y, forward.z);
    }
}
