using UnityEngine;

public class HealthBar : MonoBehaviour
{
    public float maxHealth = 100f;
    public float maxEnergy = 50f;
    float currentHealth;
    float currentEnergy;

    readonly float widthPerPoint = 6f;
    readonly float barHeight = 56f;
    readonly float barGap = 8f;
    readonly float bottomMargin = 40f;
    readonly float leftMargin = 40f;

    Texture2D bgTex, healthTex, energyTex, borderTex;

    void Start()
    {
        currentHealth = maxHealth;
        currentEnergy = maxEnergy;

        bgTex = MakeTex(new Color(0f, 0f, 0f, 0.55f));
        healthTex = MakeTex(new Color(0.15f, 0.75f, 0.2f, 0.9f));
        energyTex = MakeTex(new Color(0.9f, 0.8f, 0.1f, 0.9f));
        borderTex = MakeTex(new Color(0.7f, 0.7f, 0.7f, 0.7f));
    }

    Texture2D MakeTex(Color c)
    {
        Texture2D t = new Texture2D(1, 1);
        t.SetPixel(0, 0, c);
        t.Apply();
        return t;
    }

    void OnGUI()
    {
        float x = leftMargin;

        // Health bar (bottom)
        float healthW = maxHealth * widthPerPoint;
        float healthY = Screen.height - bottomMargin - barHeight;
        DrawBar(x, healthY, healthW, currentHealth, maxHealth, healthTex, "HP");

        // Energy bar (above health)
        float energyW = maxEnergy * widthPerPoint;
        float energyY = healthY - barGap - barHeight;
        DrawBar(x, energyY, energyW, currentEnergy, maxEnergy, energyTex, "Energy");
    }

    void DrawBar(float x, float y, float width, float current, float max, Texture2D fillTex, string label)
    {
        Rect bg = new Rect(x, y, width, barHeight);
        GUI.DrawTexture(bg, bgTex);

        float frac = Mathf.Clamp01(current / max);
        GUI.DrawTexture(new Rect(x, y, width * frac, barHeight), fillTex);

        DrawBorder(bg, borderTex);

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.alignment = TextAnchor.MiddleCenter;
        style.fontSize = 28;
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = Color.white;
        GUI.Label(bg, label + "  " + Mathf.CeilToInt(current) + " / " + Mathf.CeilToInt(max), style);
    }

    void DrawBorder(Rect r, Texture2D tex)
    {
        float w = 2f;
        GUI.DrawTexture(new Rect(r.x, r.y, r.width, w), tex);
        GUI.DrawTexture(new Rect(r.x, r.yMax - w, r.width, w), tex);
        GUI.DrawTexture(new Rect(r.x, r.y, w, r.height), tex);
        GUI.DrawTexture(new Rect(r.xMax - w, r.y, w, r.height), tex);
    }

    public void TakeDamage(float amount) { currentHealth = Mathf.Max(0f, currentHealth - amount); }
    public void Heal(float amount) { currentHealth = Mathf.Min(maxHealth, currentHealth + amount); }
    public void IncreaseMaxHealth(float amount) { maxHealth += amount; currentHealth += amount; }
    public float GetHealth() => currentHealth;

    public void UseEnergy(float amount) { currentEnergy = Mathf.Max(0f, currentEnergy - amount); }
    public void RestoreEnergy(float amount) { currentEnergy = Mathf.Min(maxEnergy, currentEnergy + amount); }
    public void IncreaseMaxEnergy(float amount) { maxEnergy += amount; currentEnergy += amount; }
    public float GetEnergy() => currentEnergy;
}
