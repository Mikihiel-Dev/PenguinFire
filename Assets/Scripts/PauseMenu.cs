using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    public PlayerMovement playerMovement;
    public CameraFollow cameraFollow;

    private bool isPaused;
    private bool isThirdPerson;

    void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        if (kb.escapeKey.wasPressedThisFrame)
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (playerMovement) playerMovement.inputEnabled = false;
    }

    void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (playerMovement) playerMovement.inputEnabled = true;
    }

    void ToggleCamera()
    {
        isThirdPerson = !isThirdPerson;
        if (cameraFollow)
        {
            cameraFollow.orbitAngle = 0f;
            if (isThirdPerson)
                cameraFollow.offset = new Vector3(0f, 2f, -4f);
            else
                cameraFollow.offset = new Vector3(0f, 1.3f, 0.3f);
        }
    }

    void OnGUI()
    {
        if (!isPaused) return;

        float w = 220f;
        float h = 50f;
        float gap = 15f;
        float totalH = h * 3 + gap * 2;
        float x = (Screen.width - w) / 2f;
        float y = (Screen.height - totalH) / 2f;

        // Darken background
        GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "");

        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 22;

        if (GUI.Button(new Rect(x, y, w, h), "Resume", buttonStyle))
            Resume();

        y += h + gap;
        string camLabel = isThirdPerson ? "First Person" : "Third Person";
        if (GUI.Button(new Rect(x, y, w, h), camLabel, buttonStyle))
            ToggleCamera();

        y += h + gap;
        if (GUI.Button(new Rect(x, y, w, h), "Quit", buttonStyle))
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
