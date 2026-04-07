using UnityEngine;
using System.Collections.Generic;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    float currentHealth;

    static Texture2D bgTex, fillTex, borderTex;

    struct DmgPopup
    {
        public float amount;
        public float timer;
        public Vector3 offset;
    }
    List<DmgPopup> popups = new List<DmgPopup>();
    const float popupDuration = 0.8f;

    void Start()
    {
        currentHealth = maxHealth;

        if (bgTex == null)
        {
            bgTex = MakeTex(new Color(0f, 0f, 0f, 0.6f));
            fillTex = MakeTex(new Color(0.8f, 0.15f, 0.15f, 0.9f));
            borderTex = MakeTex(new Color(0.3f, 0.3f, 0.3f, 0.7f));
        }
    }

    static Texture2D MakeTex(Color c)
    {
        Texture2D t = new Texture2D(1, 1);
        t.SetPixel(0, 0, c);
        t.Apply();
        return t;
    }

    public Color headColor = new Color(0.3f, 0.3f, 0.3f);
    public float headRadius = 0.3f;
    public Vector3 headOffset = new Vector3(0f, 1.5f, 0f);

    public void TakeDamage(float amount)
    {
        currentHealth = Mathf.Max(0f, currentHealth - amount);
        popups.Add(new DmgPopup
        {
            amount = amount,
            timer = popupDuration,
            offset = new Vector3(Random.Range(-0.3f, 0.3f), 0f, 0f)
        });
        if (currentHealth <= 0f)
            Die();
    }

    void Die()
    {
        GameObject head = new GameObject("DroppedHead");
        head.transform.position = transform.position + headOffset;
        DroppedHead dh = head.AddComponent<DroppedHead>();
        dh.headColor = headColor;
        dh.headRadius = headRadius;
        Destroy(gameObject);
    }

    void Update()
    {
        for (int i = popups.Count - 1; i >= 0; i--)
        {
            var p = popups[i];
            p.timer -= Time.deltaTime;
            if (p.timer <= 0f)
                popups.RemoveAt(i);
            else
                popups[i] = p;
        }
    }

    void OnGUI()
    {
        if (bgTex == null)
        {
            bgTex = MakeTex(new Color(0f, 0f, 0f, 0.6f));
            fillTex = MakeTex(new Color(0.8f, 0.15f, 0.15f, 0.9f));
            borderTex = MakeTex(new Color(0.3f, 0.3f, 0.3f, 0.7f));
        }

        Camera cam = Camera.main;
        if (!cam) return;

        Vector3 worldPos = transform.position + Vector3.up * 1.6f;
        Vector3 screenPos = cam.WorldToScreenPoint(worldPos);

        // Behind camera
        if (screenPos.z < 0f) return;

        // Too far away
        float dist = Vector3.Distance(cam.transform.position, transform.position);
        if (dist > 30f) return;

        float barW = 60f;
        float barH = 8f;
        float x = screenPos.x - barW / 2f;
        float y = Screen.height - screenPos.y - barH / 2f;
        Rect bg = new Rect(x, y, barW, barH);

        GUI.DrawTexture(bg, bgTex);

        float frac = Mathf.Clamp01(currentHealth / maxHealth);
        GUI.DrawTexture(new Rect(x, y, barW * frac, barH), fillTex);

        // Border
        float bw = 1f;
        GUI.DrawTexture(new Rect(bg.x, bg.y, bg.width, bw), borderTex);
        GUI.DrawTexture(new Rect(bg.x, bg.yMax - bw, bg.width, bw), borderTex);
        GUI.DrawTexture(new Rect(bg.x, bg.y, bw, bg.height), borderTex);
        GUI.DrawTexture(new Rect(bg.xMax - bw, bg.y, bw, bg.height), borderTex);

        // Damage popups
        GUIStyle dmgStyle = new GUIStyle(GUI.skin.label);
        dmgStyle.fontSize = 40;
        dmgStyle.fontStyle = FontStyle.Bold;
        dmgStyle.alignment = TextAnchor.MiddleCenter;

        for (int i = 0; i < popups.Count; i++)
        {
            var p = popups[i];
            float t = 1f - p.timer / popupDuration;
            float rise = t * 40f;
            float alpha = Mathf.Lerp(1f, 0f, t);

            Vector3 popWorld = transform.position + Vector3.up * 2f + p.offset;
            Vector3 popScreen = cam.WorldToScreenPoint(popWorld);
            if (popScreen.z < 0f) continue;

            float px = popScreen.x - 30f;
            float py = Screen.height - popScreen.y - rise;

            dmgStyle.normal.textColor = new Color(1f, 0.15f, 0.1f, alpha);
            GUI.Label(new Rect(px, py, 60f, 30f), "-" + p.amount.ToString("0"), dmgStyle);
        }
    }

    public float GetHealth() => currentHealth;
}
