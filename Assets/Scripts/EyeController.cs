using UnityEngine;
using UnityEngine.InputSystem;

public class EyeController : MonoBehaviour
{
    public Transform leftPupil;
    public Transform rightPupil;
    public float maxOffset = 0.2f;

    private Camera cam;
    private Vector3 pupilRestPos = new Vector3(0f, 0f, 0.35f);

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null) return;

        Vector2 mousePos = mouse.position.ReadValue();
        Vector3 eyeCenter = transform.position + Vector3.up * 1.3f;
        Vector3 screenPos = cam.WorldToScreenPoint(eyeCenter);
        Vector2 dir = mousePos - (Vector2)screenPos;
        float maxScreenDist = Screen.height * 0.3f;
        dir = Vector2.ClampMagnitude(dir / maxScreenDist, 1f);

        Vector3 offset = new Vector3(dir.x * maxOffset, dir.y * maxOffset, 0f);

        if (leftPupil) leftPupil.localPosition = pupilRestPos + offset;
        if (rightPupil) rightPupil.localPosition = pupilRestPos + offset;
    }
}
