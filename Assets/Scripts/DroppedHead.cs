using UnityEngine;

public class DroppedHead : MonoBehaviour
{
    public Color headColor = Color.gray;
    public float headRadius = 0.3f;

    void Start()
    {
        // Head sphere
        MeshFilter mf = gameObject.AddComponent<MeshFilter>();
        MeshRenderer mr = gameObject.AddComponent<MeshRenderer>();
        GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        mf.mesh = temp.GetComponent<MeshFilter>().mesh;
        Destroy(temp);

        Shader s = Shader.Find("Universal Render Pipeline/Lit");
        Material mat = new Material(s);
        mat.SetColor("_BaseColor", headColor);
        mr.material = mat;

        transform.localScale = Vector3.one * headRadius * 2f;

        // Eyes
        CreateEye(new Vector3(-0.25f, 0.15f, 0.35f));
        CreateEye(new Vector3(0.25f, 0.15f, 0.35f));

        // Physics
        SphereCollider col = gameObject.AddComponent<SphereCollider>();
        col.radius = 0.5f;

        Rigidbody rb = gameObject.AddComponent<Rigidbody>();
        rb.mass = 0.5f;
        // Small random tumble
        rb.angularVelocity = new Vector3(
            Random.Range(-3f, 3f), Random.Range(-2f, 2f), Random.Range(-3f, 3f));
    }

    void CreateEye(Vector3 localPos)
    {
        GameObject eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(eye.GetComponent<Collider>());
        eye.transform.SetParent(transform, false);
        eye.transform.localPosition = localPos;
        eye.transform.localScale = Vector3.one * 0.3f;
        Shader s = Shader.Find("Universal Render Pipeline/Lit");
        Material mat = new Material(s);
        mat.SetColor("_BaseColor", Color.white);
        eye.GetComponent<MeshRenderer>().material = mat;

        // Pupil
        GameObject pupil = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(pupil.GetComponent<Collider>());
        pupil.transform.SetParent(eye.transform, false);
        pupil.transform.localPosition = new Vector3(0f, 0f, 0.4f);
        pupil.transform.localScale = Vector3.one * 0.45f;
        Material pMat = new Material(s);
        pMat.SetColor("_BaseColor", Color.black);
        pupil.GetComponent<MeshRenderer>().material = pMat;
    }
}
