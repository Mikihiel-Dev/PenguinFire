using UnityEngine;
using System.Collections.Generic;

public class NarwhalMesh : MonoBehaviour
{
    const int SIDES = 8;
    const float CY = 0.60f;

    List<Vector3> verts;
    List<Vector3> norms;
    List<int> bodyTris, bellyTris, hornTris;

    Transform leftFlipper, rightFlipper, flukeObj;
    Rigidbody parentRb;
    float swimTimer;

    void Start()
    {
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one;

        verts = new List<Vector3>();
        norms = new List<Vector3>();
        bodyTris = new List<int>();
        bellyTris = new List<int>();
        hornTris = new List<int>();

        BuildBody();
        BuildHorn();

        Mesh mesh = new Mesh();
        mesh.SetVertices(verts);
        mesh.SetNormals(norms);
        mesh.subMeshCount = 3;
        mesh.SetTriangles(bodyTris, 0);
        mesh.SetTriangles(bellyTris, 1);
        mesh.SetTriangles(hornTris, 2);
        mesh.RecalculateBounds();

        GetComponent<MeshFilter>().mesh = mesh;
        Shader s = Shader.Find("Universal Render Pipeline/Lit");
        Material bodyMat = MakeMat(s, new Color(0.45f, 0.50f, 0.55f), 0.5f);
        Material bellyMat = MakeMat(s, new Color(0.80f, 0.82f, 0.80f), 0.4f);
        Material hornMat = MakeMat(s, new Color(0.92f, 0.88f, 0.75f), 0.6f);
        GetComponent<MeshRenderer>().materials = new Material[] { bodyMat, bellyMat, hornMat };

        // Build flippers and flukes as separate animated children
        leftFlipper = BuildFlipperObj("LeftFlipper", -1f, bodyMat);
        rightFlipper = BuildFlipperObj("RightFlipper", 1f, bodyMat);
        flukeObj = BuildFlukeObj("Flukes", bodyMat);

        // Sphere eyes
        Material eyeWhiteMat = MakeMat(s, new Color(0.95f, 0.95f, 0.95f), 0.9f);
        Material pupilMat = MakeMat(s, new Color(0.05f, 0.05f, 0.05f), 0.9f);
        CreateEye("LeftEye", new Vector3(-0.30f, CY + 0.12f, 0.20f),
            new Vector3(0.12f, 0.12f, 0.06f), eyeWhiteMat, pupilMat, true);
        CreateEye("RightEye", new Vector3(0.30f, CY + 0.12f, 0.20f),
            new Vector3(0.12f, 0.12f, 0.06f), eyeWhiteMat, pupilMat, true);

        // Find parent rigidbody for speed detection
        Transform root = transform.parent;
        if (root) parentRb = root.GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Animate fins/tail when moving
        float speed = 0f;
        if (parentRb)
        {
            Vector3 hVel = parentRb.linearVelocity;
            hVel.y = 0f;
            speed = hVel.magnitude;
        }

        if (speed > 0.3f)
        {
            swimTimer += Time.deltaTime * 4f;
            float flipAngle = Mathf.Sin(swimTimer) * 20f;
            float tailAngle = Mathf.Sin(swimTimer * 1.2f) * 15f;

            if (leftFlipper) leftFlipper.localRotation = Quaternion.Euler(0f, 0f, flipAngle);
            if (rightFlipper) rightFlipper.localRotation = Quaternion.Euler(0f, 0f, -flipAngle);
            if (flukeObj) flukeObj.localRotation = Quaternion.Euler(tailAngle, 0f, 0f);
        }
        else
        {
            // Idle — gently return to rest
            if (leftFlipper) leftFlipper.localRotation = Quaternion.Lerp(leftFlipper.localRotation, Quaternion.identity, Time.deltaTime * 3f);
            if (rightFlipper) rightFlipper.localRotation = Quaternion.Lerp(rightFlipper.localRotation, Quaternion.identity, Time.deltaTime * 3f);
            if (flukeObj) flukeObj.localRotation = Quaternion.Lerp(flukeObj.localRotation, Quaternion.identity, Time.deltaTime * 3f);
        }
    }

    Transform BuildFlipperObj(string name, float side, Material mat)
    {
        // Flipper pivot at body attachment point
        GameObject go = new GameObject(name);
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(side * 0.52f, CY, 1.05f);

        // Build flipper mesh relative to pivot
        List<Vector3> fv = new List<Vector3>();
        List<Vector3> fn = new List<Vector3>();
        List<int> ft = new List<int>();

        Vector3 nSide = side < 0 ? Vector3.left : Vector3.right;
        Vector3 nOpp = side < 0 ? Vector3.right : Vector3.left;

        // Triangle points relative to pivot (pivot is at body edge, mid-flipper z)
        Vector3 a = new Vector3(0f, 0f, -0.55f);
        Vector3 b = new Vector3(side * 0.28f, -0.14f, 0.35f);
        Vector3 c = new Vector3(0f, 0f, 0.55f);

        // Top face
        int i0 = FV(fv, fn, a, nSide);
        int i1 = FV(fv, fn, b, nSide);
        int i2 = FV(fv, fn, c, nSide);
        if (side < 0) { ft.Add(i0); ft.Add(i1); ft.Add(i2); }
        else { ft.Add(i0); ft.Add(i2); ft.Add(i1); }

        // Bottom face
        int i3 = FV(fv, fn, a, nOpp);
        int i4 = FV(fv, fn, b, nOpp);
        int i5 = FV(fv, fn, c, nOpp);
        if (side < 0) { ft.Add(i3); ft.Add(i5); ft.Add(i4); }
        else { ft.Add(i3); ft.Add(i4); ft.Add(i5); }

        Mesh m = new Mesh();
        m.SetVertices(fv);
        m.SetNormals(fn);
        m.SetTriangles(ft, 0);
        m.RecalculateBounds();

        go.AddComponent<MeshFilter>().mesh = m;
        go.AddComponent<MeshRenderer>().material = mat;
        return go.transform;
    }

    Transform BuildFlukeObj(string name, Material mat)
    {
        // Fluke pivot at tail base
        GameObject go = new GameObject(name);
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(0f, CY, 5.00f);

        List<Vector3> fv = new List<Vector3>();
        List<Vector3> fn = new List<Vector3>();
        List<int> ft = new List<int>();

        // Left fluke (relative to pivot at tail base)
        Vector3 lb = Vector3.zero;
        Vector3 lm = new Vector3(0f, 0f, 0.30f);
        Vector3 lt = new Vector3(-0.50f, 0f, 0.70f);

        int l0 = FV(fv, fn, lb, Vector3.up);
        int l1 = FV(fv, fn, lm, Vector3.up);
        int l2 = FV(fv, fn, lt, Vector3.up);
        ft.Add(l0); ft.Add(l1); ft.Add(l2);
        int l3 = FV(fv, fn, lb, Vector3.down);
        int l4 = FV(fv, fn, lm, Vector3.down);
        int l5 = FV(fv, fn, lt, Vector3.down);
        ft.Add(l3); ft.Add(l5); ft.Add(l4);

        // Right fluke
        Vector3 rt = new Vector3(0.50f, 0f, 0.70f);
        int r0 = FV(fv, fn, lb, Vector3.up);
        int r1 = FV(fv, fn, lm, Vector3.up);
        int r2 = FV(fv, fn, rt, Vector3.up);
        ft.Add(r0); ft.Add(r2); ft.Add(r1);
        int r3 = FV(fv, fn, lb, Vector3.down);
        int r4 = FV(fv, fn, lm, Vector3.down);
        int r5 = FV(fv, fn, rt, Vector3.down);
        ft.Add(r3); ft.Add(r4); ft.Add(r5);

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

    void CreateEye(string name, Vector3 localPos, Vector3 scale, Material eyeMat, Material pupilMat, bool faceBack = false)
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
        float pz = faceBack ? -0.35f : 0.35f;
        pupil.transform.localPosition = new Vector3(0f, 0f, pz);
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

    int[] AddRing(float z, float r)
    {
        int[] idx = new int[SIDES];
        for (int i = 0; i < SIDES; i++)
        {
            float a = i * Mathf.PI * 2f / SIDES;
            float x = Mathf.Sin(a) * r;
            float y = CY + Mathf.Cos(a) * r;
            Vector3 n = new Vector3(Mathf.Sin(a), Mathf.Cos(a), 0f);
            idx[i] = V(new Vector3(x, y, z), n);
        }
        return idx;
    }

    void BuildBody()
    {
        float[][] rings = {
            new[] {0.0f, 0.12f},
            new[] {0.30f, 0.36f},
            new[] {0.80f, 0.50f},
            new[] {1.60f, 0.60f},
            new[] {2.60f, 0.56f},
            new[] {3.60f, 0.36f},
            new[] {4.40f, 0.20f},
            new[] {5.00f, 0.08f},
        };

        // Nose cap
        int nose = V(new Vector3(0, CY, -0.10f), Vector3.back);
        int[][] rs = new int[rings.Length][];
        for (int r = 0; r < rings.Length; r++)
            rs[r] = AddRing(rings[r][0], rings[r][1]);

        // Tail cap
        int tail = V(new Vector3(0, CY, 5.20f), Vector3.forward);

        // Nose cap faces
        int[] noseRing = new int[SIDES];
        for (int i = 0; i < SIDES; i++)
            noseRing[i] = V(verts[rs[0][i]], Vector3.back);
        for (int i = 0; i < SIDES; i++)
        {
            int ni = (i + 1) % SIDES;
            Tri(bodyTris, nose, noseRing[i], noseRing[ni]);
        }

        // Ring connections (fixed winding)
        HashSet<int> belly = new HashSet<int> { 3, 4, 5 };
        for (int r = 0; r < rings.Length - 1; r++)
        {
            for (int i = 0; i < SIDES; i++)
            {
                int ni = (i + 1) % SIDES;
                var t = belly.Contains(i) ? bellyTris : bodyTris;
                Tri(t, rs[r][i], rs[r + 1][i], rs[r + 1][ni]);
                Tri(t, rs[r][i], rs[r + 1][ni], rs[r][ni]);
            }
        }

        // Tail cap faces
        int last = rings.Length - 1;
        int[] tailRing = new int[SIDES];
        for (int i = 0; i < SIDES; i++)
            tailRing[i] = V(verts[rs[last][i]], Vector3.forward);
        for (int i = 0; i < SIDES; i++)
        {
            int ni = (i + 1) % SIDES;
            Tri(bodyTris, tail, tailRing[ni], tailRing[i]);
        }
    }

    void BuildHorn()
    {
        Vector3 tip = new Vector3(0, CY + 0.04f, -1.60f);
        float br = 0.06f;
        float bz = -0.04f;
        int tipV = V(tip, Vector3.forward);

        int[] baseV = new int[4];
        Vector3[] offsets = { new Vector3(0, br, 0), new Vector3(br, 0, 0), new Vector3(0, -br, 0), new Vector3(-br, 0, 0) };
        Vector3[] normDir = { Vector3.up, Vector3.right, Vector3.down, Vector3.left };
        for (int i = 0; i < 4; i++)
            baseV[i] = V(new Vector3(0, CY + 0.04f, bz) + offsets[i], normDir[i]);

        for (int i = 0; i < 4; i++)
        {
            int ni = (i + 1) % 4;
            Tri(hornTris, tipV, baseV[ni], baseV[i]);
        }
    }
}
