using UnityEngine;
using System.Collections.Generic;

public class SealMesh : MonoBehaviour
{
    const int SIDES = 10;

    List<Vector3> verts;
    List<Vector3> norms;
    List<int> bodyTris, bellyTris, darkTris;

    Transform leftFlipper, rightFlipper, tailFlippers;
    Rigidbody parentRb;
    float moveTimer;

    void Start()
    {
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one;
        transform.localRotation = Quaternion.identity;

        verts = new List<Vector3>();
        norms = new List<Vector3>();
        bodyTris = new List<int>();
        bellyTris = new List<int>();
        darkTris = new List<int>();

        BuildAll();
        BuildSnout();

        Mesh mesh = new Mesh();
        mesh.SetVertices(verts);
        mesh.SetNormals(norms);
        mesh.subMeshCount = 3;
        mesh.SetTriangles(bodyTris, 0);
        mesh.SetTriangles(bellyTris, 1);
        mesh.SetTriangles(darkTris, 2);
        mesh.RecalculateBounds();

        GetComponent<MeshFilter>().mesh = mesh;
        Shader s = Shader.Find("Universal Render Pipeline/Lit");
        Material bodyMat = MakeMat(s, new Color(0.52f, 0.56f, 0.54f), 0.4f);
        GetComponent<MeshRenderer>().materials = new Material[]
        {
            bodyMat,
            MakeMat(s, new Color(0.68f, 0.70f, 0.68f), 0.35f),
            MakeMat(s, new Color(0.02f, 0.02f, 0.02f), 0.3f),
        };

        // Animated flippers and tail as child objects
        leftFlipper = BuildFlipperObj("LeftFlipper", -1f, bodyMat);
        rightFlipper = BuildFlipperObj("RightFlipper", 1f, bodyMat);
        tailFlippers = BuildTailObj("TailFlippers", bodyMat);

        // Sphere eyes
        Material eyeWhiteMat = MakeMat(s, new Color(0.92f, 0.92f, 0.92f), 0.9f);
        Material pupilMat = MakeMat(s, new Color(0.05f, 0.05f, 0.05f), 0.9f);
        CreateEye("LeftEye", new Vector3(-0.24f, 1.06f, 1.38f),
            new Vector3(0.20f, 0.20f, 0.20f), eyeWhiteMat, pupilMat);
        CreateEye("RightEye", new Vector3(0.24f, 1.06f, 1.38f),
            new Vector3(0.20f, 0.20f, 0.20f), eyeWhiteMat, pupilMat);

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
            moveTimer += Time.deltaTime * 5f;
            float flipAngle = Mathf.Sin(moveTimer) * 25f;
            float tailAngle = Mathf.Sin(moveTimer * 0.8f) * 18f;

            // Front flippers paddle back and forth (rotate around Z)
            if (leftFlipper) leftFlipper.localRotation = Quaternion.Euler(0f, flipAngle, 0f);
            if (rightFlipper) rightFlipper.localRotation = Quaternion.Euler(0f, -flipAngle, 0f);
            // Tail wiggles side to side
            if (tailFlippers) tailFlippers.localRotation = Quaternion.Euler(0f, tailAngle, 0f);
        }
        else
        {
            if (leftFlipper) leftFlipper.localRotation = Quaternion.Lerp(leftFlipper.localRotation, Quaternion.identity, Time.deltaTime * 3f);
            if (rightFlipper) rightFlipper.localRotation = Quaternion.Lerp(rightFlipper.localRotation, Quaternion.identity, Time.deltaTime * 3f);
            if (tailFlippers) tailFlippers.localRotation = Quaternion.Lerp(tailFlippers.localRotation, Quaternion.identity, Time.deltaTime * 3f);
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

    // Ring in XY plane at z position. rx=width, ry=height, cy=center height
    int[] AddRing(float z, float rx, float ry, float cy)
    {
        int[] idx = new int[SIDES];
        for (int i = 0; i < SIDES; i++)
        {
            float a = i * Mathf.PI * 2f / SIDES;
            float x = Mathf.Sin(a) * rx;
            float y = cy + Mathf.Cos(a) * ry;
            Vector3 n = new Vector3(Mathf.Sin(a), Mathf.Cos(a), 0f);
            idx[i] = V(new Vector3(x, y, z), n);
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

    void BuildAll()
    {
        // Plump round body sitting low, neck curves up, round head
        // {z, rx, ry, cy}
        float[][] allRings = {
            // Rear (butt)
            new[] {-0.70f, 0.15f, 0.12f, 0.25f},
            new[] {-0.45f, 0.38f, 0.25f, 0.30f},
            // Fat round body - wide and low
            new[] {-0.15f, 0.55f, 0.35f, 0.32f},
            new[] { 0.15f, 0.60f, 0.38f, 0.34f},
            new[] { 0.45f, 0.58f, 0.36f, 0.34f},
            new[] { 0.70f, 0.50f, 0.32f, 0.36f},
            // Neck curving up — stays thick
            new[] { 0.90f, 0.42f, 0.30f, 0.42f},
            new[] { 1.05f, 0.38f, 0.28f, 0.52f},
            new[] { 1.15f, 0.35f, 0.26f, 0.65f},
            new[] { 1.22f, 0.34f, 0.25f, 0.78f},
            // Round head
            new[] { 1.30f, 0.34f, 0.26f, 0.90f},
            new[] { 1.40f, 0.32f, 0.25f, 0.96f},
            new[] { 1.48f, 0.28f, 0.22f, 0.98f},
            // Snout tapering forward
            new[] { 1.55f, 0.20f, 0.16f, 0.96f},
            new[] { 1.62f, 0.12f, 0.10f, 0.94f},
        };

        int[][] rs = new int[allRings.Length][];
        for (int r = 0; r < allRings.Length; r++)
            rs[r] = AddRing(allRings[r][0], allRings[r][1], allRings[r][2], allRings[r][3]);

        // Butt cap
        int butt = V(new Vector3(0, 0.25f, -0.80f), Vector3.back);
        int[] buttCap = new int[SIDES];
        for (int i = 0; i < SIDES; i++)
            buttCap[i] = V(verts[rs[0][i]], Vector3.back);
        for (int i = 0; i < SIDES; i++)
        {
            int ni = (i + 1) % SIDES;
            Tri(bodyTris, butt, buttCap[i], buttCap[ni]);
        }

        // Connect rings — belly on bottom faces for body section
        HashSet<int> bellyFaces = new HashSet<int> { 4, 5, 6 };
        for (int r = 0; r < allRings.Length - 1; r++)
        {
            bool hasBelly = r < 6;
            for (int i = 0; i < SIDES; i++)
            {
                int ni = (i + 1) % SIDES;
                var t = (hasBelly && bellyFaces.Contains(i)) ? bellyTris : bodyTris;
                Tri(t, rs[r][i], rs[r + 1][i], rs[r + 1][ni]);
                Tri(t, rs[r][i], rs[r + 1][ni], rs[r][ni]);
            }
        }

        // Nose cap (dark)
        int last = allRings.Length - 1;
        int nose = V(new Vector3(0, 0.94f, 1.68f), Vector3.forward);
        int[] noseCap = new int[SIDES];
        for (int i = 0; i < SIDES; i++)
            noseCap[i] = V(verts[rs[last][i]], Vector3.forward);
        for (int i = 0; i < SIDES; i++)
        {
            int ni = (i + 1) % SIDES;
            Tri(darkTris, nose, noseCap[ni], noseCap[i]);
        }
    }

    Transform BuildFlipperObj(string name, float side, Material mat)
    {
        // Pivot at body edge, mid-flipper z
        GameObject go = new GameObject(name);
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(side * 0.45f, 0.15f, 0.65f);

        List<Vector3> fv = new List<Vector3>();
        List<Vector3> fn = new List<Vector3>();
        List<int> ft = new List<int>();

        // Paddle shape relative to pivot
        float fy = -0.11f; // tip drops down from pivot
        Vector3 a = new Vector3(0f, 0f, -0.15f);
        Vector3 b = new Vector3(side * 0.40f, fy, -0.10f);
        Vector3 c = new Vector3(side * 0.35f, fy, 0.20f);
        Vector3 d = new Vector3(0f, 0f, 0.15f);

        // Top face
        int i0 = FV(fv, fn, a, Vector3.up);
        int i1 = FV(fv, fn, b, Vector3.up);
        int i2 = FV(fv, fn, c, Vector3.up);
        int i3 = FV(fv, fn, d, Vector3.up);
        if (side < 0) { ft.Add(i0); ft.Add(i1); ft.Add(i2); ft.Add(i0); ft.Add(i2); ft.Add(i3); }
        else { ft.Add(i0); ft.Add(i2); ft.Add(i1); ft.Add(i0); ft.Add(i3); ft.Add(i2); }

        // Bottom face
        int i4 = FV(fv, fn, a, Vector3.down);
        int i5 = FV(fv, fn, b, Vector3.down);
        int i6 = FV(fv, fn, c, Vector3.down);
        int i7 = FV(fv, fn, d, Vector3.down);
        if (side < 0) { ft.Add(i4); ft.Add(i6); ft.Add(i5); ft.Add(i4); ft.Add(i7); ft.Add(i6); }
        else { ft.Add(i4); ft.Add(i5); ft.Add(i6); ft.Add(i4); ft.Add(i6); ft.Add(i7); }

        Mesh m = new Mesh();
        m.SetVertices(fv);
        m.SetNormals(fn);
        m.SetTriangles(ft, 0);
        m.RecalculateBounds();

        go.AddComponent<MeshFilter>().mesh = m;
        go.AddComponent<MeshRenderer>().material = mat;
        return go.transform;
    }

    Transform BuildTailObj(string name, Material mat)
    {
        // Pivot at rear of body
        GameObject go = new GameObject(name);
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(0f, 0.20f, -0.65f);

        List<Vector3> fv = new List<Vector3>();
        List<Vector3> fn = new List<Vector3>();
        List<int> ft = new List<int>();

        float tipY = -0.16f;

        // Left tail paddle (relative to pivot)
        Vector3 l0 = new Vector3(0f, 0f, 0f);
        Vector3 l1 = new Vector3(-0.16f, 0f, -0.05f);
        Vector3 l2 = new Vector3(-0.34f, tipY, -0.25f);
        Vector3 l3 = new Vector3(-0.40f, tipY, -0.55f);
        Vector3 l4 = new Vector3(-0.21f, tipY, -0.70f);
        Vector3 l5 = new Vector3(0f, 0f, -0.20f);

        // Top face
        int a0 = FV(fv, fn, l0, Vector3.up); int a1 = FV(fv, fn, l1, Vector3.up);
        int a2 = FV(fv, fn, l2, Vector3.up); int a3 = FV(fv, fn, l3, Vector3.up);
        int a4 = FV(fv, fn, l4, Vector3.up); int a5 = FV(fv, fn, l5, Vector3.up);
        ft.Add(a0); ft.Add(a1); ft.Add(a5);
        ft.Add(a1); ft.Add(a2); ft.Add(a5);
        ft.Add(a5); ft.Add(a2); ft.Add(a4);
        ft.Add(a2); ft.Add(a3); ft.Add(a4);
        // Bottom
        int b0 = FV(fv, fn, l0, Vector3.down); int b1 = FV(fv, fn, l1, Vector3.down);
        int b2 = FV(fv, fn, l2, Vector3.down); int b3 = FV(fv, fn, l3, Vector3.down);
        int b4 = FV(fv, fn, l4, Vector3.down); int b5 = FV(fv, fn, l5, Vector3.down);
        ft.Add(b0); ft.Add(b5); ft.Add(b1);
        ft.Add(b1); ft.Add(b5); ft.Add(b2);
        ft.Add(b5); ft.Add(b4); ft.Add(b2);
        ft.Add(b2); ft.Add(b4); ft.Add(b3);

        // Right tail paddle (mirror)
        Vector3 r1 = new Vector3(0.16f, 0f, -0.05f);
        Vector3 r2 = new Vector3(0.34f, tipY, -0.25f);
        Vector3 r3 = new Vector3(0.40f, tipY, -0.55f);
        Vector3 r4 = new Vector3(0.21f, tipY, -0.70f);

        int c0 = FV(fv, fn, l0, Vector3.up); int c1 = FV(fv, fn, r1, Vector3.up);
        int c2 = FV(fv, fn, r2, Vector3.up); int c3 = FV(fv, fn, r3, Vector3.up);
        int c4 = FV(fv, fn, r4, Vector3.up); int c5 = FV(fv, fn, l5, Vector3.up);
        ft.Add(c0); ft.Add(c5); ft.Add(c1);
        ft.Add(c1); ft.Add(c5); ft.Add(c2);
        ft.Add(c5); ft.Add(c4); ft.Add(c2);
        ft.Add(c2); ft.Add(c4); ft.Add(c3);
        // Bottom
        int d0 = FV(fv, fn, l0, Vector3.down); int d1 = FV(fv, fn, r1, Vector3.down);
        int d2 = FV(fv, fn, r2, Vector3.down); int d3 = FV(fv, fn, r3, Vector3.down);
        int d4 = FV(fv, fn, r4, Vector3.down); int d5 = FV(fv, fn, l5, Vector3.down);
        ft.Add(d0); ft.Add(d1); ft.Add(d5);
        ft.Add(d1); ft.Add(d2); ft.Add(d5);
        ft.Add(d5); ft.Add(d2); ft.Add(d4);
        ft.Add(d2); ft.Add(d3); ft.Add(d4);

        Mesh m = new Mesh();
        m.SetVertices(fv);
        m.SetNormals(fn);
        m.SetTriangles(ft, 0);
        m.RecalculateBounds();

        go.AddComponent<MeshFilter>().mesh = m;
        go.AddComponent<MeshRenderer>().material = mat;
        return go.transform;
    }

    int FV(List<Vector3> v, List<Vector3> n, Vector3 p, Vector3 norm)
    {
        int i = v.Count; v.Add(p); n.Add(norm.normalized); return i;
    }

    void BuildSnout()
    {
        // Nose sits right on the snout tip (last ring at z=1.62, nose cap at z=1.68)
        // Keep it small and flush against the face
        float nz = 1.64f;  // just past the last ring
        float ny = 0.94f;  // matches nose cap cy
        float nr = 0.08f;

        // Nose bump — 4 triangle diamond, barely protruding
        int nc = V(new Vector3(0, ny, nz + 0.03f), Vector3.forward);
        int nt = V(new Vector3(0, ny + nr, nz), new Vector3(0, 1, 0.5f));
        int nb = V(new Vector3(0, ny - nr, nz), new Vector3(0, -1, 0.5f));
        int nl = V(new Vector3(-nr, ny, nz), new Vector3(-1, 0, 0.5f));
        int nrv = V(new Vector3(nr, ny, nz), new Vector3(1, 0, 0.5f));
        Tri(darkTris, nc, nrv, nt);
        Tri(darkTris, nc, nb, nrv);
        Tri(darkTris, nc, nl, nb);
        Tri(darkTris, nc, nt, nl);

        // Nostrils — two small dark dots on the nose surface
        float nostrilSpread = 0.04f;
        float nostrilZ = nz + 0.02f;
        float nostrilR = 0.02f;
        for (int side = -1; side <= 1; side += 2)
        {
            float nx = side * nostrilSpread;
            int n0 = V(new Vector3(nx, ny + nostrilR, nostrilZ), Vector3.forward);
            int n1 = V(new Vector3(nx + nostrilR, ny, nostrilZ), Vector3.forward);
            int n2 = V(new Vector3(nx, ny - nostrilR, nostrilZ), Vector3.forward);
            int n3 = V(new Vector3(nx - nostrilR, ny, nostrilZ), Vector3.forward);
            Tri(darkTris, n0, n1, n2);
            Tri(darkTris, n0, n2, n3);
        }

        // Mouth line — sits against the underside of the snout
        // Snout bottom at z=1.55: cy=0.96, ry=0.16 → bottom at y=0.80
        // Snout bottom at z=1.62: cy=0.94, ry=0.10 → bottom at y=0.84
        float mz1 = 1.58f;
        float mz2 = 1.46f;
        float my = 0.83f;
        float mw = 0.05f;

        // Down-facing (visible from below)
        int ma = V(new Vector3(-mw, my, mz1), Vector3.down);
        int mb = V(new Vector3(mw, my, mz1), Vector3.down);
        int mc = V(new Vector3(mw, my, mz2), Vector3.down);
        int md = V(new Vector3(-mw, my, mz2), Vector3.down);
        Tri(darkTris, ma, mc, mb);
        Tri(darkTris, ma, md, mc);
        // Front-facing (visible from front)
        int ma2 = V(new Vector3(-mw, my, mz1), Vector3.forward);
        int mb2 = V(new Vector3(mw, my, mz1), Vector3.forward);
        int mc2 = V(new Vector3(mw, my, mz2), Vector3.forward);
        int md2 = V(new Vector3(-mw, my, mz2), Vector3.forward);
        Tri(darkTris, ma2, mb2, mc2);
        Tri(darkTris, ma2, mc2, md2);
    }
}
