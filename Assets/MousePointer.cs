using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VR;

public class MousePointer : MonoBehaviour
{

    public Text text;
    public GameObject objectsGroup;
    public GameObject snapPointObject;
    private GameObject snapsGroup;
    private Camera camera;
    private Material material;
    private bool dragging = false;
    private float hitPointZ;
    private GameObject hitObject;
    private List<Snap> snaps;
    Vector3 hitPoint;
    Vector3 hitNormal;

    struct Snap
    {
        public Vector3 position;
        public Vector3 normal;
    }

    void Awake()
    {
    }

    void Start()
    {
        camera = transform.parent.GetComponent<Camera>();
        material = gameObject.GetComponent<Renderer>().material;
        VRSettings.showDeviceView = true;
        //Screen.SetResolution(VRSettings.eyeTextureWidth, VRSettings.eyeTextureHeight, false);
        //Debug.Log((" width: " + VRSettings.eyeTextureWidth + "  height: " + VRSettings.eyeTextureHeight));
    }

    void Update()
    {

        if (dragging)
        {
            var adjustedMousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, hitPointZ);
            transform.up = hitNormal;
            foreach (var snap in snaps)
            {
                var screenPosition = camera.WorldToScreenPoint(snap.position);
                if (Mathf.Pow(Input.mousePosition.x - screenPosition.x, 2) + Mathf.Pow(Input.mousePosition.y - screenPosition.y, 2) < 10 * 10)  // 10px snapping radius
                {
                    adjustedMousePosition = screenPosition;
                    hitPointZ = screenPosition.z;
                    transform.up = snap.normal;
                    break;
                }
            }
            transform.position = camera.ScreenToWorldPoint(adjustedMousePosition);
        }
        else
        {
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayCastHit;
            if (Physics.Raycast(ray, out rayCastHit, 1000))
            {
                hitPoint = rayCastHit.point;
                hitNormal = rayCastHit.normal;
                transform.position = hitPoint;
                transform.up = hitNormal;
                if (Input.GetMouseButtonDown(0))
                {
                    // parent object to pointer
                    hitObject = rayCastHit.transform.gameObject;
                    rayCastHit.transform.parent = transform;
                    snaps = GetSnaps();
                    hitPointZ = camera.WorldToScreenPoint(hitPoint).z;
                    //Debug.Log(hitPointZ);
                    dragging = true;
                    material.color = Color.green;
                }
                else
                {
                    material.color = Color.red;
                }
            }
            else
            {
                transform.position = camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 5));   // FIXME make Z distance more general purpose
                transform.forward = Vector3.up;
                material.color = Color.black;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            dragging = false;
            hitObject.transform.parent = objectsGroup.transform;
        }

        //text.text = (" width: " + VRSettings.eyeTextureWidth + "  height: " + VRSettings.eyeTextureHeight);
        text.text = ("x: " + Input.mousePosition.x + " y: " + Input.mousePosition.y);
    }

    List<Snap> GetSnaps()
    {
        MeshFilter[] meshFilters = objectsGroup.GetComponentsInChildren<MeshFilter>();
        var snaps = new List<Snap>();
        foreach (var meshFilter in meshFilters)
        {
            var tmpTransform = meshFilter.transform;
            var vertices = meshFilter.mesh.vertices;
            var normals = meshFilter.mesh.normals;
            for (int i = 0; i < vertices.Length; i++)
            {
                var snap = new Snap();
                snap.position = tmpTransform.TransformPoint(vertices[i]);
                snap.normal = tmpTransform.TransformVector(normals[i]);
                snaps.Add(snap);
            }
        }

        if (snapsGroup != null)
        {
            GameObject.DestroyImmediate(snapsGroup);
        }
        snapsGroup = new GameObject();
        foreach (var snap in snaps)
        {
            var snapObject = GameObject.Instantiate(snapPointObject) as GameObject;
            snapObject.transform.position = snap.position;
            snapObject.transform.up = snap.normal;
            snapObject.transform.parent = snapsGroup.transform;
        }

        return snaps;
    }
}
