using UnityEngine;
using System.Collections.Generic;

public class Penguin1 : MonoBehaviour
{
    const int SIDES = 8;

    public float waddleSpeed = 10f;
    public float waddleAngle = 8f;
    public float bobAmount = 0.03f;

    List<Vector3> verts;
    List<Vector3> normals;
    List<int> blackTris, whiteTris, orangeTris;

    private Rigidbody parentRb;
    private float waddleTimer;
    private Transform origLeftPupil, origRightPupil;
    private Transform visLeftPupil, visRightPupil;

    void Start()
    {
        parentRb = GetComponentInParent<Rigidbody>();
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one;
        transform.localRotation = Quaternion.identity;

        verts = new List<Vector3>();
        normals = new List<Vector3>();
        blackTris = new List<int>();
        whiteTris = new List<int>();
        orangeTris = new List<int>();

        BuildBody();
        BuildBeak();
        BuildFlippers();
        BuildFeet();

        Mesh mesh = new Mesh();
        mesh.SetVertices(verts);
        mesh.SetNormals(normals);
        mesh.subMeshCount = 3;
        mesh.SetTriangles(blackTris, 0);
        mesh.SetTriangles(whiteTris, 1);
        mesh.SetTriangles(orangeTris, 2);
        mesh.RecalculateBounds();

        GetComponent<MeshFilter>().mesh = mesh;

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        GetComponent<MeshRenderer>().materials = new Material[]
        {
            MakeMat(shader, new Color(0.12f, 0.12f, 0.15f), 0.3f),
            MakeMat(shader, new Color(0.93f, 0.93f, 0.90f), 0.3f),
            MakeMat(shader, new Color(0.95f, 0.55f, 0.1f), 0.3f),
        };

        // Hide the head cube
        Transform head = transform.parent.Find("Head");
        if (head) head.gameObject.SetActive(false);

        // Hide original eyes but keep them for EyeController tracking
        Transform le = transform.parent.Find("LeftEye");
        Transform re = transform.parent.Find("RightEye");
        if (le)
        {
            le.localPosition = new Vector3(-0.10f, 1.17f, 0.28f);
            HideRenderers(le);
            origLeftPupil = le.childCount > 0 ? le.GetChild(0) : null;
        }
        if (re)
        {
            re.localPosition = new Vector3(0.10f, 1.17f, 0.28f);
            HideRenderers(re);
            origRightPupil = re.childCount > 0 ? re.GetChild(0) : null;
        }

        // Create visible eyes on the Body so they waddle with it
        Shader shader2 = Shader.Find("Universal Render Pipeline/Lit");
        Material eyeWhiteMat = MakeMat(shader2, new Color(0.95f, 0.95f, 0.95f), 0.9f);
        Material pupilMat = MakeMat(shader2, new Color(0.05f, 0.05f, 0.05f), 0.9f);

        visLeftPupil = CreateEye("VisLeftEye", new Vector3(-0.10f, 1.17f, 0.28f),
            new Vector3(0.18f, 0.18f, 0.09f), eyeWhiteMat, pupilMat);
        visRightPupil = CreateEye("VisRightEye", new Vector3(0.10f, 1.17f, 0.28f),
            new Vector3(0.18f, 0.18f, 0.09f), eyeWhiteMat, pupilMat);
    }

    void HideRenderers(Transform t)
    {
        var r = t.GetComponent<MeshRenderer>();
        if (r) r.enabled = false;
        for (int i = 0; i < t.childCount; i++)
            HideRenderers(t.GetChild(i));
    }

    Transform CreateEye(string name, Vector3 localPos, Vector3 scale, Material eyeMat, Material pupilMat)
    {
        GameObject eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        eye.name = name;
        Object.Destroy(eye.GetComponent<Collider>());
        eye.transform.SetParent(transform, false);
        eye.transform.localPosition = localPos;
        eye.transform.localScale = scale;
        eye.GetComponent<MeshRenderer>().material = eyeMat;

        GameObject pupil = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pupil.name = name + "Pupil";
        Object.Destroy(pupil.GetComponent<Collider>());
        pupil.transform.SetParent(eye.transform, false);
        pupil.transform.localPosition = new Vector3(0f, 0f, 0.35f);
        pupil.transform.localScale = new Vector3(0.45f, 0.45f, 0.5f);
        pupil.GetComponent<MeshRenderer>().material = pupilMat;

        return pupil.transform;
    }

    void Update()
    {
        if (!parentRb) return;

        Vector3 vel = parentRb.linearVelocity;
        float speed = new Vector2(vel.x, vel.z).magnitude;

        if (speed > 0.1f)
        {
            waddleTimer += Time.deltaTime * waddleSpeed;
            float rock = Mathf.Sin(waddleTimer) * waddleAngle;
            float bob = Mathf.Abs(Mathf.Cos(waddleTimer)) * bobAmount;
            transform.localRotation = Quaternion.Euler(0f, 0f, rock);
            transform.localPosition = new Vector3(0f, bob, 0f);
        }
        else
        {
            waddleTimer = 0f;
            transform.localRotation = Quaternion.identity;
            transform.localPosition = Vector3.zero;
        }

        // Mirror pupil positions from the invisible originals to the visible ones
        if (origLeftPupil && visLeftPupil)
            visLeftPupil.localPosition = origLeftPupil.localPosition;
        if (origRightPupil && visRightPupil)
            visRightPupil.localPosition = origRightPupil.localPosition;
    }

    Material MakeMat(Shader shader, Color color, float smoothness)
    {
        Material mat = new Material(shader);
        mat.SetColor("_BaseColor", color);
        mat.SetFloat("_Smoothness", smoothness);
        return mat;
    }

    int V(float x, float y, float z, float nx, float ny, float nz)
    {
        int i = verts.Count;
        verts.Add(new Vector3(x, y, z));
        normals.Add(new Vector3(nx, ny, nz).normalized);
        return i;
    }

    int V(Vector3 pos, Vector3 normal)
    {
        int i = verts.Count;
        verts.Add(pos);
        normals.Add(normal.normalized);
        return i;
    }

    void Tri(List<int> t, int a, int b, int c)
    {
        t.Add(a); t.Add(b); t.Add(c);
    }

    Vector3 RingPos(float y, float r, int i)
    {
        float a = i * Mathf.PI * 2f / SIDES;
        return new Vector3(Mathf.Sin(a) * r, y, Mathf.Cos(a) * r);
    }

    Vector3 RingNormal(int i)
    {
        float a = i * Mathf.PI * 2f / SIDES;
        return new Vector3(Mathf.Sin(a), 0f, Mathf.Cos(a)).normalized;
    }

    void BuildBody()
    {
        float[][] rings =
        {
            new[] { 0.15f, 0.20f },
            new[] { 0.35f, 0.35f },
            new[] { 0.55f, 0.38f },
            new[] { 0.75f, 0.33f },
            new[] { 0.90f, 0.25f },
            new[] { 1.00f, 0.27f },
            new[] { 1.15f, 0.30f },
            new[] { 1.28f, 0.25f },
            new[] { 1.38f, 0.15f },
        };

        // Bottom cap
        int botCenter = V(0, 0.05f, 0, 0, -1, 0);
        int[] botRing = new int[SIDES];
        for (int i = 0; i < SIDES; i++)
        {
            Vector3 pos = RingPos(rings[0][0], rings[0][1], i);
            botRing[i] = V(pos, Vector3.down);
        }
        for (int i = 0; i < SIDES; i++)
        {
            int ni = (i + 1) % SIDES;
            Tri(blackTris, botCenter, botRing[ni], botRing[i]);
        }

        // Ring vertices with outward normals
        int[][] rs = new int[rings.Length][];
        for (int r = 0; r < rings.Length; r++)
        {
            rs[r] = new int[SIDES];
            for (int i = 0; i < SIDES; i++)
            {
                Vector3 pos = RingPos(rings[r][0], rings[r][1], i);
                Vector3 n = RingNormal(i);
                rs[r][i] = V(pos, n);
            }
        }

        // Ring connections
        HashSet<int> bellyFaces = new HashSet<int> { 6, 7, 0, 1 };
        for (int r = 0; r < rings.Length - 1; r++)
        {
            bool hasBelly = r < 5;
            for (int i = 0; i < SIDES; i++)
            {
                int ni = (i + 1) % SIDES;
                List<int> t = (hasBelly && bellyFaces.Contains(i)) ? whiteTris : blackTris;
                Tri(t, rs[r][i], rs[r + 1][ni], rs[r + 1][i]);
                Tri(t, rs[r][i], rs[r][ni], rs[r + 1][ni]);
            }
        }

        // Top cap
        int topCenter = V(0, 1.42f, 0, 0, 1, 0);
        int last = rings.Length - 1;
        int[] topRing = new int[SIDES];
        for (int i = 0; i < SIDES; i++)
        {
            Vector3 pos = RingPos(rings[last][0], rings[last][1], i);
            topRing[i] = V(pos, Vector3.up);
        }
        for (int i = 0; i < SIDES; i++)
        {
            int ni = (i + 1) % SIDES;
            Tri(blackTris, topCenter, topRing[i], topRing[ni]);
        }
    }

    void BuildBeak()
    {
        Vector3 tipPos = new Vector3(0, 1.12f, 0.45f);
        Vector3 topPos = new Vector3(0, 1.20f, 0.31f);
        Vector3 botPos = new Vector3(0, 1.06f, 0.31f);
        Vector3 leftPos = new Vector3(-0.09f, 1.13f, 0.31f);
        Vector3 rightPos = new Vector3(0.09f, 1.13f, 0.31f);

        Vector3 nTopRight = Vector3.Cross(topPos - tipPos, rightPos - tipPos).normalized;
        if (nTopRight == Vector3.zero) nTopRight = Vector3.forward;
        Vector3 nBotRight = Vector3.Cross(rightPos - tipPos, botPos - tipPos).normalized;
        if (nBotRight == Vector3.zero) nBotRight = Vector3.forward;
        Vector3 nBotLeft = Vector3.Cross(botPos - tipPos, leftPos - tipPos).normalized;
        if (nBotLeft == Vector3.zero) nBotLeft = Vector3.forward;
        Vector3 nTopLeft = Vector3.Cross(leftPos - tipPos, topPos - tipPos).normalized;
        if (nTopLeft == Vector3.zero) nTopLeft = Vector3.forward;

        // Each face gets its own vertices
        int tr0 = V(tipPos, nTopRight); int tr1 = V(rightPos, nTopRight); int tr2 = V(topPos, nTopRight);
        Tri(orangeTris, tr0, tr1, tr2);

        int br0 = V(tipPos, nBotRight); int br1 = V(botPos, nBotRight); int br2 = V(rightPos, nBotRight);
        Tri(orangeTris, br0, br1, br2);

        int bl0 = V(tipPos, nBotLeft); int bl1 = V(leftPos, nBotLeft); int bl2 = V(botPos, nBotLeft);
        Tri(orangeTris, bl0, bl1, bl2);

        int tl0 = V(tipPos, nTopLeft); int tl1 = V(topPos, nTopLeft); int tl2 = V(leftPos, nTopLeft);
        Tri(orangeTris, tl0, tl1, tl2);
    }

    void BuildWing(float side)
    {
        float s = side;
        Vector3 shoulderPos = new Vector3(s * 0.27f, 0.85f, 0.02f * s);
        Vector3 upperOutPos = new Vector3(s * 0.52f, 0.68f, 0.04f * s);
        Vector3 tipPos = new Vector3(s * 0.60f, 0.38f, 0f);
        Vector3 lowerOutPos = new Vector3(s * 0.48f, 0.22f, -0.03f * s);
        Vector3 armpitPos = new Vector3(s * 0.28f, 0.20f, -0.02f * s);

        // Compute face normal
        Vector3 edge1 = tipPos - shoulderPos;
        Vector3 edge2 = armpitPos - shoulderPos;
        Vector3 frontNormal = Vector3.Cross(edge1, edge2).normalized;
        if (frontNormal == Vector3.zero) frontNormal = Vector3.forward;
        Vector3 backNormal = -frontNormal;

        // Front face - separate vertices
        int fs = V(shoulderPos, frontNormal);
        int fu = V(upperOutPos, frontNormal);
        int ft = V(tipPos, frontNormal);
        int fl = V(lowerOutPos, frontNormal);
        int fa = V(armpitPos, frontNormal);
        Tri(blackTris, fs, fu, ft);
        Tri(blackTris, fs, ft, fl);
        Tri(blackTris, fs, fl, fa);

        // Back face - separate vertices
        int bs = V(shoulderPos, backNormal);
        int bu = V(upperOutPos, backNormal);
        int bt = V(tipPos, backNormal);
        int bl = V(lowerOutPos, backNormal);
        int ba = V(armpitPos, backNormal);
        Tri(blackTris, bs, bt, bu);
        Tri(blackTris, bs, bl, bt);
        Tri(blackTris, bs, ba, bl);
    }

    void BuildFlippers()
    {
        BuildWing(-1f);
        BuildWing(1f);
    }

    void BuildFoot(float side)
    {
        float s = side;
        Vector3 heelPos = new Vector3(s * 0.10f, 0.01f, 0.02f);
        Vector3 toeLPos = new Vector3(s * 0.22f, 0.01f, 0.35f);
        Vector3 toeMPos = new Vector3(s * 0.10f, 0.01f, 0.38f);
        Vector3 toeRPos = new Vector3(s * 0.01f, 0.01f, 0.35f);

        // Top face
        int th = V(heelPos, Vector3.up);
        int ttl = V(toeLPos, Vector3.up);
        int ttm = V(toeMPos, Vector3.up);
        int ttr = V(toeRPos, Vector3.up);
        Tri(orangeTris, th, ttl, ttm);
        Tri(orangeTris, th, ttm, ttr);

        // Bottom face
        int bh = V(heelPos, Vector3.down);
        int btl = V(toeLPos, Vector3.down);
        int btm = V(toeMPos, Vector3.down);
        int btr = V(toeRPos, Vector3.down);
        Tri(orangeTris, bh, btm, btl);
        Tri(orangeTris, bh, btr, btm);
    }

    void BuildFeet()
    {
        BuildFoot(-1f);
        BuildFoot(1f);
    }
}
