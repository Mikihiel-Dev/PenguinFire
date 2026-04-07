using UnityEngine;
using UnityEngine.InputSystem;

public class Hotbar : MonoBehaviour
{
    public int slotCount = 2;
    int selectedSlot;

    public int pebbleCount = 20;

    readonly float slotSize = 128f;
    readonly float slotGap = 12f;
    readonly float bottomMargin = 40f;
    readonly float rightMargin = 40f;

    Texture2D pebbleIcon;

    void Start()
    {
        // Generate a small pebble icon procedurally
        int sz = 48;
        pebbleIcon = new Texture2D(sz, sz);
        Color clear = new Color(0, 0, 0, 0);
        for (int px = 0; px < sz; px++)
            for (int py = 0; py < sz; py++)
                pebbleIcon.SetPixel(px, py, clear);

        // Draw an oval pebble shape
        float cx = sz / 2f, cy = sz / 2f;
        float rx = sz * 0.42f, ry = sz * 0.30f;
        Color stone = new Color(0.5f, 0.48f, 0.45f);
        Color highlight = new Color(0.65f, 0.63f, 0.60f);
        for (int px = 0; px < sz; px++)
        {
            for (int py = 0; py < sz; py++)
            {
                float dx = (px - cx) / rx;
                float dy = (py - cy) / ry;
                float d = dx * dx + dy * dy;
                if (d <= 1f)
                {
                    // Simple shading: lighter toward top-left
                    float shade = 1f - d * 0.3f + (-dx + dy) * 0.15f;
                    Color c = Color.Lerp(stone, highlight, Mathf.Clamp01(shade));
                    c.a = 1f;
                    pebbleIcon.SetPixel(px, py, c);
                }
            }
        }
        pebbleIcon.Apply();
    }

    void Update()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null) return;

        float scroll = mouse.scroll.ReadValue().y;
        if (scroll > 0f)
            selectedSlot = (selectedSlot - 1 + slotCount) % slotCount;
        else if (scroll < 0f)
            selectedSlot = (selectedSlot + 1) % slotCount;
    }

    void OnGUI()
    {
        float totalW = slotCount * slotSize + (slotCount - 1) * slotGap;
        float x = Screen.width - rightMargin - totalW;
        float y = Screen.height - bottomMargin - slotSize;

        for (int i = 0; i < slotCount; i++)
        {
            Rect r = new Rect(x + i * (slotSize + slotGap), y, slotSize, slotSize);

            // Background
            Color bg = (i == selectedSlot) ? new Color(1f, 1f, 1f, 0.35f) : new Color(0f, 0f, 0f, 0.45f);
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, bg);
            tex.Apply();
            GUI.DrawTexture(r, tex);

            // Border
            Color border = (i == selectedSlot) ? Color.white : new Color(0.6f, 0.6f, 0.6f, 0.7f);
            float bw = (i == selectedSlot) ? 3f : 1f;
            DrawBorder(r, border, bw);

            // Slot number
            GUIStyle label = new GUIStyle(GUI.skin.label);
            label.alignment = TextAnchor.LowerRight;
            label.fontSize = 36;
            label.normal.textColor = new Color(1f, 1f, 1f, 0.5f);
            label.padding = new RectOffset(0, 4, 0, 2);
            GUI.Label(r, (i + 1).ToString(), label);

            // Draw pebble icon + count in slot 1 (when nothing else occupies it)
            if (i == 0 && pebbleCount > 0)
            {
                float iconSz = slotSize * 0.85f;
                float iconX = r.x + (slotSize - iconSz) / 2f;
                float iconY = r.y + (slotSize - iconSz) / 2f;
                GUI.DrawTexture(new Rect(iconX, iconY, iconSz, iconSz), pebbleIcon);

                GUIStyle countStyle = new GUIStyle(GUI.skin.label);
                countStyle.alignment = TextAnchor.LowerRight;
                countStyle.fontSize = 28;
                countStyle.fontStyle = FontStyle.Bold;
                countStyle.normal.textColor = Color.white;
                countStyle.padding = new RectOffset(0, 4, 0, 2);
                GUI.Label(r, pebbleCount.ToString(), countStyle);
            }
        }

        // Pebble count above hotbar: number on left, icon on right
        float pebIconSize = 96f;
        float pebFontSize = 48f;
        float pebTextW = 80f;
        float pebGap = 10f;
        float pebY = y - pebGap - pebIconSize;
        float pebIconX = Screen.width - rightMargin - pebIconSize;
        float pebTextX = pebIconX - pebTextW;

        GUI.DrawTexture(new Rect(pebIconX, pebY, pebIconSize, pebIconSize), pebbleIcon);

        GUIStyle aboveStyle = new GUIStyle(GUI.skin.label);
        aboveStyle.alignment = TextAnchor.MiddleRight;
        aboveStyle.fontSize = (int)pebFontSize;
        aboveStyle.fontStyle = FontStyle.Bold;
        aboveStyle.normal.textColor = Color.white;
        GUI.Label(new Rect(pebTextX, pebY, pebTextW, pebIconSize), pebbleCount.ToString(), aboveStyle);
    }

    void DrawBorder(Rect r, Color c, float w)
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, c);
        tex.Apply();
        GUI.DrawTexture(new Rect(r.x, r.y, r.width, w), tex);                  // top
        GUI.DrawTexture(new Rect(r.x, r.yMax - w, r.width, w), tex);            // bottom
        GUI.DrawTexture(new Rect(r.x, r.y, w, r.height), tex);                  // left
        GUI.DrawTexture(new Rect(r.xMax - w, r.y, w, r.height), tex);           // right
    }

    public int GetSelectedSlot() => selectedSlot;
}
