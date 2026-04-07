using UnityEngine;
using System.Collections.Generic;

public class PebbleMesh : MonoBehaviour
{
    public int pebbleType;

    List<Vector3> verts;
    List<Vector3> norms;
    List<int> tris;

    void Start()
    {
        verts = new List<Vector3>();
        norms = new List<Vector3>();
        tris = new List<int>();

        BuildPebble();

        Mesh mesh = new Mesh();
        mesh.SetVertices(verts);
        mesh.SetNormals(norms);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateBounds();

        GetComponent<MeshFilter>().mesh = mesh;
        Shader s = Shader.Find("Universal Render Pipeline/Lit");
        float g = Random.Range(0.35f, 0.60f);
        float r = g + Random.Range(-0.04f, 0.06f);
        float b = g + Random.Range(-0.06f, 0.02f);
        Material mat = new Material(s);
        mat.SetColor("_BaseColor", new Color(r, g, b));
        mat.SetFloat("_Smoothness", Random.Range(0.15f, 0.45f));
        GetComponent<MeshRenderer>().material = mat;

        if (!GetComponent<Collider>())
        {
            SphereCollider col = gameObject.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = 1.5f;
        }
    }

    void BuildPebble()
    {
        // 5 pebble shapes — all same overall size, different proportions
        int sides, rings;
        float rx, ry, rz, deform;

        switch (pebbleType)
        {
            case 0: // Round flat disc
                sides = 8; rings = 3;
                rx = 0.50f; ry = 0.12f; rz = 0.50f; deform = 0.03f;
                break;
            case 1: // Elongated oval
                sides = 6; rings = 4;
                rx = 0.30f; ry = 0.18f; rz = 0.55f; deform = 0.04f;
                break;
            case 2: // Chunky square-ish
                sides = 5; rings = 3;
                rx = 0.42f; ry = 0.22f; rz = 0.40f; deform = 0.06f;
                break;
            case 3: // Thin slab
                sides = 6; rings = 3;
                rx = 0.48f; ry = 0.08f; rz = 0.38f; deform = 0.03f;
                break;
            default: // Egg shaped
                sides = 7; rings = 4;
                rx = 0.36f; ry = 0.20f; rz = 0.44f; deform = 0.05f;
                break;
        }

        // Top cap vertex
        int top = V(new Vector3(0, ry, 0), Vector3.up);

        // Ring vertices
        int[][] ringIdx = new int[rings][];
        for (int r = 0; r < rings; r++)
        {
            float phi = Mathf.PI * (r + 1) / (rings + 1);
            float sinP = Mathf.Sin(phi);
            float cosP = Mathf.Cos(phi);
            ringIdx[r] = new int[sides];
            for (int i = 0; i < sides; i++)
            {
                float theta = i * Mathf.PI * 2f / sides;
                float x = Mathf.Sin(theta) * sinP * rx + Random.Range(-deform, deform);
                float y = cosP * ry + Random.Range(-deform, deform);
                float z = Mathf.Cos(theta) * sinP * rz + Random.Range(-deform, deform);
                Vector3 n = new Vector3(Mathf.Sin(theta) * sinP, cosP, Mathf.Cos(theta) * sinP);
                ringIdx[r][i] = V(new Vector3(x, y, z), n);
            }
        }

        // Bottom cap vertex
        int bot = V(new Vector3(0, -ry, 0), Vector3.down);

        // Top cap triangles
        for (int i = 0; i < sides; i++)
        {
            int ni = (i + 1) % sides;
            Tri(top, ringIdx[0][i], ringIdx[0][ni]);
        }

        // Ring connections
        for (int r = 0; r < rings - 1; r++)
        {
            for (int i = 0; i < sides; i++)
            {
                int ni = (i + 1) % sides;
                Tri(ringIdx[r][i], ringIdx[r + 1][i], ringIdx[r + 1][ni]);
                Tri(ringIdx[r][i], ringIdx[r + 1][ni], ringIdx[r][ni]);
            }
        }

        // Bottom cap triangles
        int lastR = rings - 1;
        for (int i = 0; i < sides; i++)
        {
            int ni = (i + 1) % sides;
            Tri(bot, ringIdx[lastR][ni], ringIdx[lastR][i]);
        }
    }

    int V(Vector3 p, Vector3 n) { int i = verts.Count; verts.Add(p); norms.Add(n.normalized); return i; }
    void Tri(int a, int b, int c) { tris.Add(a); tris.Add(b); tris.Add(c); }
}
