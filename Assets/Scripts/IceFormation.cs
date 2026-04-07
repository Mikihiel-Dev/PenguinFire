using UnityEngine;

public class IceFormation : MonoBehaviour
{
    static Mesh rampMesh;

    public static void Spawn(Vector3 center, int peakHeight, float blockSize)
    {
        GameObject root = new GameObject("IceFormation");
        root.transform.position = center;

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");

        if (rampMesh == null)
            rampMesh = BuildRampMesh();

        for (int layer = 0; layer < peakHeight; layer++)
        {
            int radius = peakHeight - 1 - layer;
            float y = layer * blockSize + blockSize / 2f;

            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dz = -radius; dz <= radius; dz++)
                {
                    GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    block.transform.SetParent(root.transform, false);
                    block.transform.localPosition = new Vector3(dx * blockSize, y, dz * blockSize);
                    block.transform.localScale = Vector3.one * blockSize;
                    block.GetComponent<MeshRenderer>().material = MakeIceMat(shader);
                }
            }

            if (peakHeight > 1)
                PlaceRamps(root.transform, layer, radius, blockSize, shader);
        }
    }

    static void PlaceRamps(Transform root, int layer, int innerR, float bs, Shader shader)
    {
        float y = layer * bs;

        // Mesh: low end at -Z (y=0), high end at +Z (y=1)
        // Rotate so high end faces toward pyramid center
        for (int i = -innerR; i <= innerR; i++)
        {
            // +Z side: high end toward -Z (center)
            PlaceRamp(root, new Vector3(i * bs, y, (innerR + 1) * bs), 180f, bs, shader);
            // -Z side: high end toward +Z (center)
            PlaceRamp(root, new Vector3(i * bs, y, -(innerR + 1) * bs), 0f, bs, shader);
            // +X side: high end toward -X (center)
            PlaceRamp(root, new Vector3((innerR + 1) * bs, y, i * bs), -90f, bs, shader);
            // -X side: high end toward +X (center)
            PlaceRamp(root, new Vector3(-(innerR + 1) * bs, y, i * bs), 90f, bs, shader);
        }
    }

    static void PlaceRamp(Transform root, Vector3 pos, float yRot, float bs, Shader shader)
    {
        GameObject ramp = new GameObject("Ramp");
        ramp.transform.SetParent(root, false);
        ramp.transform.localPosition = pos;
        ramp.transform.localRotation = Quaternion.Euler(0f, yRot, 0f);
        ramp.transform.localScale = Vector3.one * bs;

        MeshFilter mf = ramp.AddComponent<MeshFilter>();
        mf.mesh = rampMesh;
        MeshRenderer mr = ramp.AddComponent<MeshRenderer>();
        mr.material = MakeIceMat(shader);
        MeshCollider mc = ramp.AddComponent<MeshCollider>();
        mc.sharedMesh = rampMesh;
        mc.convex = true;
    }

    static Mesh BuildRampMesh()
    {
        // Wedge: low at -Z (y=0), high at +Z (y=1)
        //
        //   TLF ---- TRF      (y=1, z=+0.5)
        //   /|        /|
        //  / |       / |
        // BLB------BRB |      (y=0, z=-0.5)
        //  \ |       \ |
        //   BLF ---- BRF      (y=0, z=+0.5)
        //
        Vector3 BLB = new Vector3(-0.5f, 0f, -0.5f);
        Vector3 BRB = new Vector3( 0.5f, 0f, -0.5f);
        Vector3 BLF = new Vector3(-0.5f, 0f,  0.5f);
        Vector3 BRF = new Vector3( 0.5f, 0f,  0.5f);
        Vector3 TLF = new Vector3(-0.5f, 1f,  0.5f);
        Vector3 TRF = new Vector3( 0.5f, 1f,  0.5f);

        // Compute normals from geometry
        // Slope normal: up and toward -Z
        Vector3 slopeN = Vector3.Cross(BRB - BLB, TLF - BLB).normalized; // (0, 1, -1) norm

        Vector3[] v = new Vector3[]
        {
            // Slope face - 0,1,2,3
            BLB, BRB, TRF, TLF,
            // Bottom face - 4,5,6,7
            BLB, BRB, BRF, BLF,
            // Left triangle - 8,9,10
            BLB, BLF, TLF,
            // Right triangle - 11,12,13
            BRB, BRF, TRF,
            // Front wall - 14,15,16,17
            BLF, BRF, TRF, TLF,
        };

        Vector3[] n = new Vector3[]
        {
            slopeN, slopeN, slopeN, slopeN,
            Vector3.down, Vector3.down, Vector3.down, Vector3.down,
            Vector3.left, Vector3.left, Vector3.left,
            Vector3.right, Vector3.right, Vector3.right,
            Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward,
        };

        // Winding: Unity front face = clockwise viewed from outside
        //
        // Slope (normal ~(0,1,-1), viewed from above-back):
        //   BLB(0) → TLF(3) → TRF(2), BLB(0) → TRF(2) → BRB(1)
        //   Verify: (TLF-BLB)×(TRF-BLB) = (0,1,1)×(1,1,1) = (0,-1,1)? No...
        //   Actually: (BRB-BLB)×(TLF-BLB) = (1,0,0)×(0,1,1) = (-1,0,1)? No...
        //
        //   Let me just compute directly:
        //   Tri (0,3,2): edge1=3-0=(0,1,1), edge2=2-0=(1,1,1)
        //     normal = e1×e2 = (1*1-1*1, 1*1-0*1, 0*1-1*1) = (0, 1, -1) ✓
        //   Tri (0,2,1): edge1=2-0=(1,1,1), edge2=1-0=(1,0,0)
        //     normal = e1×e2 = (1*0-1*0, 1*1-1*0, 1*0-1*1) = (0, 1, -1) ✓
        //
        // Bottom (normal (0,-1,0), viewed from below):
        //   Tri (4,5,6): e1=5-4=(1,0,0), e2=6-4=(1,0,1)
        //     normal = (0*1-0*0, 0*1-1*1, 1*0-0*1) = (0,-1,0) ✓
        //   Tri (4,6,7): e1=6-4=(1,0,1), e2=7-4=(0,0,1)
        //     normal = (0*1-1*0, 1*0-1*1, 1*0-0*0) = (0,-1,0) ✓
        //
        // Left (normal (-1,0,0)):
        //   Tri (8,9,10): e1=9-8=(0,0,1), e2=10-8=(0,1,1)
        //     normal = (0*1-1*1, 1*0-0*1, 0*1-0*0) = (-1,0,0) ✓
        //
        // Right (normal (1,0,0)):
        //   Tri (11,13,12): e1=13-11=(0,1,1), e2=12-11=(0,0,1)
        //     normal = (1*1-1*0, 1*0-0*1, 0*0-1*0) = (1,0,0) ✓
        //
        // Front (normal (0,0,1)):
        //   Tri (14,15,16): e1=15-14=(1,0,0), e2=16-14=(1,1,0)
        //     normal = (0*0-0*1, 0*1-1*0, 1*1-0*1) = (0,0,1) ✓
        //   Tri (14,16,17): e1=16-14=(1,1,0), e2=17-14=(0,1,0)
        //     normal = (1*0-0*1, 0*0-1*0, 1*1-1*0) = (0,0,1) ✓

        int[] t = new int[]
        {
            // Slope
            0, 3, 2,
            0, 2, 1,
            // Bottom
            4, 5, 6,
            4, 6, 7,
            // Left
            8, 9, 10,
            // Right
            11, 13, 12,
            // Front
            14, 15, 16,
            14, 16, 17,
        };

        Mesh m = new Mesh();
        m.vertices = v;
        m.normals = n;
        m.triangles = t;
        m.RecalculateBounds();
        return m;
    }

    static Material MakeIceMat(Shader shader)
    {
        Material mat = new Material(shader);
        float tint = Random.Range(0.75f, 0.95f);
        mat.SetColor("_BaseColor", new Color(tint * 0.85f, tint * 0.93f, tint, 0.92f));
        mat.SetFloat("_Smoothness", Random.Range(0.6f, 0.85f));
        mat.SetFloat("_Surface", 1f);
        mat.SetFloat("_Blend", 0f);
        mat.SetOverrideTag("RenderType", "Transparent");
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.renderQueue = 3000;
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        return mat;
    }
}
