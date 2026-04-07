using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 1.3f, 0.3f);
    public float orbitSensitivity = 3f;

    private PlayerMovement playerMovement;
    [HideInInspector] public float orbitAngle;

    void Start()
    {
        if (target)
            playerMovement = target.GetComponent<PlayerMovement>();
    }

    void LateUpdate()
    {
        if (!target) return;

        bool isFirstPerson = offset.z > 0f;

        if (isFirstPerson)
        {
            transform.position = target.position + target.rotation * offset;
            float yAngle = playerMovement ? playerMovement.yRotation : target.eulerAngles.y;
            float xAngle = playerMovement ? playerMovement.xRotation : 0f;
            transform.rotation = Quaternion.Euler(xAngle, yAngle, 0f);
        }
        else
        {
            Mouse mouse = Mouse.current;
            bool orbiting = mouse != null && mouse.middleButton.isPressed && playerMovement && playerMovement.inputEnabled;

            if (orbiting)
            {
                float delta = Mathf.Clamp(mouse.delta.ReadValue().x, -50f, 50f);
                orbitAngle += delta * orbitSensitivity * Time.deltaTime * 10f;
            }
            else
            {
                // Snap to the player's facing direction
                float targetAngle = playerMovement ? playerMovement.yRotation : target.eulerAngles.y;
                orbitAngle = targetAngle;
            }

            Quaternion orbitRot = Quaternion.Euler(0f, orbitAngle, 0f);
            Vector3 desiredPos = target.position + orbitRot * offset;
            transform.position = desiredPos;

            Vector3 dir = (target.position + Vector3.up * 1.2f) - desiredPos;
            float yaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            float horizontalDist = Mathf.Sqrt(dir.x * dir.x + dir.z * dir.z);
            float pitch = -Mathf.Atan2(dir.y, horizontalDist) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }
    }
}
