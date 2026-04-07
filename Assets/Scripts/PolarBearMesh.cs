using UnityEngine;
using System.Collections.Generic;

public class PolarBearMesh : MonoBehaviour
{
    const int SIDES = 8;

    List<Vector3> verts;
    List<Vector3> norms;
    List<int> whiteTris, darkTris;

    // Legs: [0]=front-left, [1]=front-right, [2]=rear-left, [3]=rear-right
    Transform[] legs = new Transform[4];
    Rigidbody parentRb;
    float gaitTimer;

    void Start()
    {
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one;
        transform.localRotation = Quaternion.identity;

        verts = new List<Vector3>();
        norms = new List<Vector3>();
        whiteTris = new List<int>();
        darkTris = new List<int>();

        BuildAll();

        Mesh mesh = new Mesh();
        mesh.SetVertices(verts);
        mesh.SetNormals(norms);
        mesh.subMeshCount = 2;
        mesh.SetTriangles(whiteTris, 0);
        mesh.SetTriangles(darkTris, 1);
        mesh.RecalculateBounds();

        GetComponent<MeshFilter>().mesh = mesh;
        Shader s = Shader.Find("Universal Render Pipeline/Lit");
        Material whiteMat = MakeMat(s, new Color(0.93f, 0.91f, 0.85f), 0.25f);
        Material darkMat = MakeMat(s, new Color(0.08f, 0.08f, 0.08f), 0.3f);
        GetComponent<MeshRenderer>().materials = new Material[] { whiteMat, darkMat };

        // Animated legs as child objects
        legs[0] = BuildLegObj("FrontLeft", -0.36f, 0.60f, whiteMat);
        legs[1] = BuildLegObj("FrontRight", 0.36f, 0.60f, whiteMat);
        legs[2] = BuildLegObj("RearLeft", -0.36f, -0.70f, whiteMat);
        legs[3] = BuildLegObj("RearRight", 0.36f, -0.70f, whiteMat);

        // Sphere eyes like the penguin
        Material eyeWhiteMat = MakeMat(s, new Color(0.95f, 0.95f, 0.95f), 0.9f);
        Material pupilMat = MakeMat(s, new Color(0.05f, 0.05f, 0.05f), 0.9f);

        float headCy = 1.10f;
        CreateEye("LeftEye", new Vector3(-0.25f, headCy + 0.20f, 1.42f),
            new Vector3(0.16f, 0.16f, 0.16f), eyeWhiteMat, pupilMat);
        CreateEye("RightEye", new Vector3(0.25f, headCy + 0.20f, 1.42f),
            new Vector3(0.16f, 0.16f, 0.16f), eyeWhiteMat, pupilMat);

        // Find parent rigidbody for speed detection
        Transform root = transform.parent;
        if (root) parentRb = root.GetComponent<Rigidbody>();
    }

    void Update()
    {
        float speed = 0f;
        if (parentRb)
        {
            Vector3 hVel = parentRb.linearVelocity;
            hVel.y = 0f;
            speed = hVel.magnitude;
        }

        if (speed > 0.3f)
        {
            gaitTimer += Time.deltaTime * 6f;
            float swing = Mathf.Sin(gaitTimer) * 30f;

            // Diagonal gait: front-right + rear-left swing together,
            // front-left + rear-right swing opposite
            if (legs[0]) legs[0].localRotation = Quaternion.Euler(-swing, 0f, 0f);  // front-left
            if (legs[1]) legs[1].localRotation = Quaternion.Euler(swing, 0f, 0f);   // front-right
            if (legs[2]) legs[2].localRotation = Quaternion.Euler(swing, 0f, 0f);   // rear-left
            if (legs[3]) legs[3].localRotation = Quaternion.Euler(-swing, 0f, 0f);  // rear-right
        }
        else
        {
            for (int i = 0; i < 4; i++)
                if (legs[i]) legs[i].localRotation = Quaternion.Lerp(legs[i].localRotation, Quaternion.identity, Time.deltaTime * 4f);
        }
    }

    void CreateEye(string name, Vector3 localPos, Vector3 scale, Material eyeMat, Material pupilMat)
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
    }

    Material MakeMat(Shader s, Color c, float sm)
    {
        Material m = new Material(s);
        m.SetColor("_BaseColor", c);
        m.SetFloat("_Smoothness", sm);
        return m;
    }

    int V(Vector3 p, Vector3 n) { int i = verts.Count; verts.Add(p); norms.Add(n.normalized); return i; }
    void Tri(List<int> t, int a, int b, int c) { t.Add(a); t.Add(b); t.Add(c); }

    int[] AddRing(float z, float r, float cy)
    {
        int[] idx = new int[SIDES];
        for (int i = 0; i < SIDES; i++)
        {
            float a = i * Mathf.PI * 2f / SIDES;
            idx[i] = V(new Vector3(Mathf.Sin(a) * r, cy + Mathf.Cos(a) * r, z),
                       new Vector3(Mathf.Sin(a), Mathf.Cos(a), 0f));
        }
        return idx;
    }

    void ConnectRings(int[] r1, int[] r2, List<int> tris)
    {
        for (int i = 0; i < SIDES; i++)
        {
            int ni = (i + 1) % SIDES;
            Tri(tris, r1[i], r2[i], r2[ni]);
            Tri(tris, r1[i], r2[ni], r1[ni]);
        }
    }

    void Cap(int center, int[] ring, List<int> tris, bool front)
    {
        for (int i = 0; i < SIDES; i++)
        {
            int ni = (i + 1) % SIDES;
            if (front) Tri(tris, center, ring[ni], ring[i]);
            else Tri(tris, center, ring[i], ring[ni]);
        }
    }

    void BuildAll()
    {
        float bodyCy = 0.95f;
        float headCy = 1.10f;

        // Continuous tube: body → neck → head
        // Fat body (radii boosted beyond 2x for chubbiness)
        float[][] allRings = {
            // Body
            new[] {-1.20f, 0.40f, bodyCy},
            new[] {-0.60f, 0.68f, bodyCy},
            new[] { 0.00f, 0.75f, bodyCy},
            new[] { 0.60f, 0.68f, bodyCy},
            new[] { 1.00f, 0.50f, bodyCy},
            // Neck
            new[] { 1.05f, 0.42f, 1.02f},
            // Head (0.625x length)
            new[] { 1.15f, 0.46f, headCy},
            new[] { 1.34f, 0.40f, headCy},
            new[] { 1.53f, 0.28f, headCy},
            new[] { 1.65f, 0.16f, headCy},
        };

        // Butt cap
        int butt = V(new Vector3(0, bodyCy, -1.40f), Vector3.back);
        int[][] rs = new int[allRings.Length][];
        for (int r = 0; r < allRings.Length; r++)
            rs[r] = AddRing(allRings[r][0], allRings[r][1], allRings[r][2]);

        int[] buttRing = new int[SIDES];
        for (int i = 0; i < SIDES; i++)
            buttRing[i] = V(verts[rs[0][i]], Vector3.back);
        Cap(butt, buttRing, whiteTris, false);

        // Connect all rings
        for (int r = 0; r < allRings.Length - 1; r++)
            ConnectRings(rs[r], rs[r + 1], whiteTris);

        // Nose cap (dark)
        int last = allRings.Length - 1;
        int nose = V(new Vector3(0, headCy, 1.72f), Vector3.forward);
        int[] noseRing = new int[SIDES];
        for (int i = 0; i < SIDES; i++)
            noseRing[i] = V(verts[rs[last][i]], Vector3.forward);
        Cap(nose, noseRing, darkTris, true);

        // Ears
        BuildEars(headCy);

        // Legs are now separate animated child objects

        // Tail
        BuildTail(bodyCy);
    }

    void BuildEars(float headCy)
    {
        // Head at z≈1.3 has r=0.46, surface at x=0.22 → y_off=sqrt(0.46²-0.22²)=0.40
        float earBaseY = headCy + 0.40f;

        // Left ear
        int la = V(new Vector3(-0.22f, earBaseY, 1.16f), Vector3.up);
        int lb = V(new Vector3(-0.17f, earBaseY + 0.10f, 1.21f), Vector3.up);
        int lc = V(new Vector3(-0.17f, earBaseY, 1.26f), Vector3.up);
        Tri(whiteTris, la, lc, lb);
        int la2 = V(new Vector3(-0.22f, earBaseY, 1.16f), Vector3.down);
        int lb2 = V(new Vector3(-0.17f, earBaseY + 0.10f, 1.21f), Vector3.down);
        int lc2 = V(new Vector3(-0.17f, earBaseY, 1.26f), Vector3.down);
        Tri(whiteTris, la2, lb2, lc2);

        // Right ear
        int ra = V(new Vector3(0.22f, earBaseY, 1.16f), Vector3.up);
        int rb = V(new Vector3(0.17f, earBaseY + 0.10f, 1.21f), Vector3.up);
        int rc = V(new Vector3(0.17f, earBaseY, 1.26f), Vector3.up);
        Tri(whiteTris, ra, rb, rc);
        int ra2 = V(new Vector3(0.22f, earBaseY, 1.16f), Vector3.down);
        int rb2 = V(new Vector3(0.17f, earBaseY + 0.10f, 1.21f), Vector3.down);
        int rc2 = V(new Vector3(0.17f, earBaseY, 1.26f), Vector3.down);
        Tri(whiteTris, ra2, rc2, rb2);
    }

    Transform BuildLegObj(string name, float x, float z, Material mat)
    {
        // Pivot at top of leg (hip/shoulder joint)
        float top = 0.38f;
        float hw = 0.14f;

        GameObject go = new GameObject(name);
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(x, top, z);

        List<Vector3> lv = new List<Vector3>();
        List<Vector3> ln = new List<Vector3>();
        List<int> lt = new List<int>();

        // Box from 0 (top, at pivot) down to -top (ground)
        float h = -top;
        int t0 = LV(lv, ln, new Vector3(-hw, 0, -hw), new Vector3(-1, 0, -1));
        int t1 = LV(lv, ln, new Vector3(hw, 0, -hw), new Vector3(1, 0, -1));
        int t2 = LV(lv, ln, new Vector3(hw, 0, hw), new Vector3(1, 0, 1));
        int t3 = LV(lv, ln, new Vector3(-hw, 0, hw), new Vector3(-1, 0, 1));
        int b0 = LV(lv, ln, new Vector3(-hw, h, -hw), new Vector3(-1, 0, -1));
        int b1 = LV(lv, ln, new Vector3(hw, h, -hw), new Vector3(1, 0, -1));
        int b2 = LV(lv, ln, new Vector3(hw, h, hw), new Vector3(1, 0, 1));
        int b3 = LV(lv, ln, new Vector3(-hw, h, hw), new Vector3(-1, 0, 1));

        // Sides
        LT(lt, t0, b1, b0); LT(lt, t0, t1, b1);
        LT(lt, t1, b2, b1); LT(lt, t1, t2, b2);
        LT(lt, t2, b3, b2); LT(lt, t2, t3, b3);
        LT(lt, t3, b0, b3); LT(lt, t3, t0, b0);
        // Bottom
        LT(lt, b0, b2, b3); LT(lt, b0, b1, b2);

        Mesh m = new Mesh();
        m.SetVertices(lv);
        m.SetNormals(ln);
        m.SetTriangles(lt, 0);
        m.RecalculateBounds();

        go.AddComponent<MeshFilter>().mesh = m;
        go.AddComponent<MeshRenderer>().material = mat;
        return go.transform;
    }

    int LV(List<Vector3> v, List<Vector3> n, Vector3 p, Vector3 norm)
    {
        int i = v.Count; v.Add(p); n.Add(norm.normalized); return i;
    }

    void LT(List<int> t, int a, int b, int c) { t.Add(a); t.Add(b); t.Add(c); }

    void BuildTail(float bodyCy)
    {
        int a = V(new Vector3(0, bodyCy + 0.30f, -1.20f), Vector3.up);
        int b = V(new Vector3(0, bodyCy + 0.44f, -1.50f), Vector3.up);
        int c = V(new Vector3(-0.08f, bodyCy + 0.30f, -1.30f), Vector3.left);
        int d = V(new Vector3(0.08f, bodyCy + 0.30f, -1.30f), Vector3.right);
        Tri(whiteTris, a, c, b);
        Tri(whiteTris, a, b, d);
        int a2 = V(new Vector3(0, bodyCy + 0.30f, -1.20f), Vector3.down);
        int b2 = V(new Vector3(0, bodyCy + 0.44f, -1.50f), Vector3.down);
        int c2 = V(new Vector3(-0.08f, bodyCy + 0.30f, -1.30f), Vector3.down);
        int d2 = V(new Vector3(0.08f, bodyCy + 0.30f, -1.30f), Vector3.down);
        Tri(whiteTris, a2, b2, c2);
        Tri(whiteTris, a2, d2, b2);
    }
}
