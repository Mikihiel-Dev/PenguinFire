using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float mouseSensitivity = 2f;

    public float throwForce = 20f;

    private Rigidbody rb;
    private Hotbar hotbar;
    [HideInInspector] public float xRotation;
    [HideInInspector] public float yRotation;
    [HideInInspector] public bool inputEnabled = true;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        Cursor.lockState = CursorLockMode.Locked;
        yRotation = transform.eulerAngles.y;

        StartCoroutine(SnapToSurface());

        hotbar = GetComponent<Hotbar>();
        if (!hotbar) hotbar = gameObject.AddComponent<Hotbar>();
        if (!GetComponent<HealthBar>())
            gameObject.AddComponent<HealthBar>();
    }

    IEnumerator SnapToSurface()
    {
        // Teleport high up immediately so we're not stuck inside ice, then gravity does the rest
        transform.position = new Vector3(transform.position.x, 100f, transform.position.z);
        rb.linearVelocity = Vector3.zero;
        yield return null;
    }

    void Update()
    {
        if (!inputEnabled)
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            return;
        }

        Mouse mouse = Mouse.current;
        if (mouse != null && !mouse.middleButton.isPressed)
        {
            Vector2 mouseDelta = mouse.delta.ReadValue();
            // Clamp to prevent jumps on focus changes / alt-tab
            mouseDelta = Vector2.ClampMagnitude(mouseDelta, 50f);
            yRotation += mouseDelta.x * mouseSensitivity * Time.deltaTime * 10f;
            xRotation -= mouseDelta.y * mouseSensitivity * Time.deltaTime * 10f;
            xRotation = Mathf.Clamp(xRotation, -80f, 80f);
            transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
        }

        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        float h = 0f;
        float v = 0f;

        if (kb.wKey.isPressed) v += 1f;
        if (kb.sKey.isPressed) v -= 1f;
        if (kb.aKey.isPressed) h -= 1f;
        if (kb.dKey.isPressed) h += 1f;

        Vector3 move = (transform.forward * v + transform.right * h).normalized * moveSpeed;
        Vector3 vel = rb.linearVelocity;
        rb.linearVelocity = new Vector3(move.x, vel.y, move.z);

        // Throw pebble with left click when slot 0 (pebbles) is selected
        if (mouse != null && mouse.leftButton.wasPressedThisFrame
            && hotbar && hotbar.GetSelectedSlot() == 0 && hotbar.pebbleCount > 0)
        {
            hotbar.pebbleCount--;
            ThrowPebble();
        }
    }

    void ThrowPebble()
    {
        GameObject pebble = new GameObject("ThrownPebble");
        pebble.transform.localScale = Vector3.one * 0.3f;

        // Spawn in front of the player
        Vector3 aimDir = Quaternion.Euler(xRotation, yRotation, 0f) * Vector3.forward;
        pebble.transform.position = transform.position + Vector3.up * 1.2f + aimDir * 1f;

        pebble.AddComponent<MeshFilter>();
        pebble.AddComponent<MeshRenderer>();
        PebbleMesh pm = pebble.AddComponent<PebbleMesh>();
        pm.pebbleType = Random.Range(0, 5);

        SphereCollider col = pebble.AddComponent<SphereCollider>();
        col.radius = 0.5f;

        pebble.AddComponent<ThrownPebble>();

        Rigidbody prb = pebble.AddComponent<Rigidbody>();
        prb.mass = 0.3f;
        prb.linearVelocity = aimDir * throwForce;

        Destroy(pebble, 5f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PebbleMesh>())
        {
            Hotbar hotbar = GetComponent<Hotbar>();
            if (hotbar) hotbar.pebbleCount++;
            Destroy(other.gameObject);
        }
    }
}
