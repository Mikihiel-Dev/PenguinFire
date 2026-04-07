using UnityEngine;
using System.Collections;

public class NPCPenguinSpawner : MonoBehaviour
{
    public int count = 10;
    public float iceHalfSize = 96f;

    void Start()
    {
        // Ice formations first
        int formationCount = 12;
        float blockSize = 2f;
        for (int i = 0; i < formationCount; i++)
        {
            float fx = Random.Range(-iceHalfSize + 10f, iceHalfSize - 10f);
            float fz = Random.Range(-iceHalfSize + 10f, iceHalfSize - 10f);
            int height = Random.Range(1, 6);
            IceFormation.Spawn(new Vector3(fx, 0f, fz), height, blockSize);
        }

        // Wait for physics to register colliders before spawning on top
        StartCoroutine(SpawnAfterPhysics());
    }

    IEnumerator SpawnAfterPhysics()
    {
        yield return new WaitForFixedUpdate();

        float spawnRange = iceHalfSize - 10f;

        for (int i = 0; i < count; i++)
        {
            float x = Random.Range(-spawnRange, spawnRange);
            float z = Random.Range(-spawnRange, spawnRange);
            SpawnNPC(new Vector3(x, GetSurfaceY(x, z) + 0.5f, z));
        }

        for (int i = 0; i < 2; i++)
        {
            float nx = Random.Range(-spawnRange, spawnRange), nz = Random.Range(-spawnRange, spawnRange);
            SpawnAnimal<NarwhalMesh>("NPCNarwhal", new Vector3(nx, GetSurfaceY(nx, nz) + 0.5f, nz),
                new Vector3(1.2f, 1.2f, 5.5f), new Vector3(0f, 0.6f, 2.5f), 2f, 180f,
                hColor: new Color(0.45f, 0.5f, 0.55f), hRadius: 0.35f, hOffset: new Vector3(0f, 1.0f, 0f));
            float bx = Random.Range(-spawnRange, spawnRange), bz = Random.Range(-spawnRange, spawnRange);
            SpawnAnimal<PolarBearMesh>("NPCPolarBear", new Vector3(bx, GetSurfaceY(bx, bz) + 0.5f, bz),
                new Vector3(1.0f, 1.8f, 3.0f), new Vector3(0f, 0.9f, 0.5f), 2.5f, 0f,
                hColor: new Color(0.95f, 0.93f, 0.88f), hRadius: 0.4f, hOffset: new Vector3(0f, 1.8f, 0.5f));
            float sx = Random.Range(-spawnRange, spawnRange), sz = Random.Range(-spawnRange, spawnRange);
            SpawnAnimal<SealMesh>("NPCSeal", new Vector3(sx, GetSurfaceY(sx, sz) + 0.5f, sz),
                new Vector3(0.9f, 1.2f, 2.6f), new Vector3(0f, 0.5f, 0f), 1.5f, 0f,
                hColor: new Color(0.55f, 0.5f, 0.45f), hRadius: 0.3f, hOffset: new Vector3(0f, 1.0f, 0f));
        }

        for (int i = 0; i < 100; i++)
        {
            float px = Random.Range(-iceHalfSize + 2f, iceHalfSize - 2f);
            float pz = Random.Range(-iceHalfSize + 2f, iceHalfSize - 2f);
            SpawnPebble(new Vector3(px, GetSurfaceY(px, pz) + 0.05f, pz), i % 5);
        }
    }

    float GetSurfaceY(float x, float z)
    {
        RaycastHit hit;
        if (Physics.Raycast(new Vector3(x, 200f, z), Vector3.down, out hit, 400f))
            return hit.point.y;
        return 0f;
    }

    void SpawnNPC(Vector3 pos)
    {
        // Root
        GameObject npc = new GameObject("NPCPenguin");
        npc.transform.position = pos;

        Rigidbody rb = npc.AddComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        BoxCollider col = npc.AddComponent<BoxCollider>();
        col.size = new Vector3(0.8f, 1.5f, 0.6f);
        col.center = new Vector3(0f, 0.75f, 0f);

        NPCPenguinAI ai = npc.AddComponent<NPCPenguinAI>();
        ai.iceHalfSize = iceHalfSize;

        EnemyHealth eh = npc.AddComponent<EnemyHealth>();
        eh.headColor = Color.black;
        eh.headRadius = 0.25f;
        eh.headOffset = new Vector3(0f, 1.4f, 0f);

        // Body with penguin mesh
        GameObject body = new GameObject("Body");
        body.transform.SetParent(npc.transform, false);
        body.AddComponent<MeshFilter>();
        body.AddComponent<MeshRenderer>();
        body.AddComponent<Penguin1>();

        // Eyes (visible, children of body)
        // Penguin1 will handle creating visible eyes
        // We still need invisible eye anchors for consistency
        CreateEyeAnchor("LeftEye", npc.transform);
        CreateEyeAnchor("RightEye", npc.transform);

        // Head (will be hidden by Penguin1)
        GameObject head = new GameObject("Head");
        head.transform.SetParent(npc.transform, false);
    }

    void SpawnAnimal<T>(string name, Vector3 pos, Vector3 colliderSize, Vector3 colliderCenter, float moveSpeed, float bodyYRot = 0f, Color? hColor = null, float hRadius = 0.3f, Vector3? hOffset = null) where T : MonoBehaviour
    {
        GameObject npc = new GameObject(name);
        npc.transform.position = pos;

        Rigidbody rb = npc.AddComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        BoxCollider col = npc.AddComponent<BoxCollider>();
        col.size = colliderSize;
        col.center = colliderCenter;

        NPCPenguinAI ai = npc.AddComponent<NPCPenguinAI>();
        ai.iceHalfSize = iceHalfSize;
        ai.moveSpeed = moveSpeed;

        EnemyHealth eh = npc.AddComponent<EnemyHealth>();
        eh.headColor = hColor ?? Color.gray;
        eh.headRadius = hRadius;
        eh.headOffset = hOffset ?? new Vector3(0f, 1.5f, 0f);

        GameObject body = new GameObject("Body");
        body.transform.SetParent(npc.transform, false);
        if (bodyYRot != 0f)
            body.transform.localRotation = Quaternion.Euler(0f, bodyYRot, 0f);
        body.AddComponent<MeshFilter>();
        body.AddComponent<MeshRenderer>();
        body.AddComponent<T>();
    }

    void SpawnPebble(Vector3 pos, int type)
    {
        GameObject pebble = new GameObject("Pebble");
        pebble.transform.position = pos;
        pebble.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        pebble.transform.localScale = Vector3.one * 0.3f;
        pebble.AddComponent<MeshFilter>();
        pebble.AddComponent<MeshRenderer>();
        PebbleMesh pm = pebble.AddComponent<PebbleMesh>();
        pm.pebbleType = type;
    }

    void CreateEyeAnchor(string name, Transform parent)
    {
        GameObject eye = new GameObject(name);
        eye.transform.SetParent(parent, false);
        // Add a dummy child so Penguin1 can find a "pupil"
        GameObject pupil = new GameObject("Pupil");
        pupil.transform.SetParent(eye.transform, false);
        pupil.transform.localPosition = new Vector3(0f, 0f, 0.35f);
    }
}
