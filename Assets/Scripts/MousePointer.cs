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
    private Camera mainCamera;
    private Material material;
    private bool dragging = false;
    private float hitPointZ;
    private GameObject draggedObject;
    private List<Snap> snaps;
    private Vector3 hitPoint;
    private Vector3 hitNormal;
    private bool flipNormal = false;

    public float snapDistance = 5;  // snap radius in pixels

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
        mainCamera = transform.parent.GetComponent<Camera>();
        material = gameObject.GetComponent<Renderer>().sharedMaterial;
        VRSettings.showDeviceView = true;
        //Screen.SetResolution(VRSettings.eyeTextureWidth, VRSettings.eyeTextureHeight, false);
        //Debug.Log((" width: " + VRSettings.eyeTextureWidth + "  height: " + VRSettings.eyeTextureHeight));
    }

    void Update()
    {
        if (dragging)
        {
            // When dragging, RMB flips the dragged object along the pointer normal.
            if (Input.GetMouseButtonDown(1))
            {
                flipNormal = !flipNormal;
            }

            var adjustedMousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, hitPointZ);
            transform.up = hitNormal;

            // Snap to closest snap in screenspace, if any within radius.
            foreach (var snap in snaps)
            {
                var screenPosition = mainCamera.WorldToScreenPoint(snap.position);
                if (!(Mathf.Pow(Input.mousePosition.x - screenPosition.x, 2) +
                      Mathf.Pow(Input.mousePosition.y - screenPosition.y, 2) < snapDistance * snapDistance)) continue;
                adjustedMousePosition = screenPosition;
                hitPointZ = screenPosition.z;
                transform.up = snap.normal;
                break;
            }

            transform.position = mainCamera.ScreenToWorldPoint(adjustedMousePosition);
            if (flipNormal)
            {
                transform.up = -transform.up;
            }

            if (Input.GetMouseButtonUp(0))
            {
                dragging = false;
                flipNormal = false;
                draggedObject.transform.parent = objectsGroup.transform;
                DestroyVertexSnapsGroup();
            }
        }
        else
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
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
                    draggedObject = rayCastHit.transform.gameObject;
                    rayCastHit.transform.parent = transform;
                    snaps = GetVertexSnaps();
                    snaps.AddRange(GetUserSnaps());
                    hitPointZ = mainCamera.WorldToScreenPoint(hitPoint).z;
                    //Debug.Log(hitPointZ);
                    dragging = true;
                    material.color = new Color(0, 1, 0, .3f);
                }
                else
                {
                    material.color = new Color(1,0,0,.3f);
                }
                if (Input.GetMouseButtonUp(1))
                {
                    // create snap point at pointer
                }
            }
            else
            {
                transform.position = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 5));   // FIXME make Z distance more general purpose
                transform.forward = Vector3.up;
                material.color = new Color(0, 0, 0, .3f);
            }
        }

        //text.text = (" width: " + VRSettings.eyeTextureWidth + "  height: " + VRSettings.eyeTextureHeight);
        text.text = ("x: " + Input.mousePosition.x + " y: " + Input.mousePosition.y);
    }

    // Creates snaps at vertices, on-the-fly.
    private List<Snap> GetVertexSnaps()
    {
        MeshFilter[] meshFilters = objectsGroup.GetComponentsInChildren<MeshFilter>();
        var vertexSnaps = new List<Snap>();
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
                vertexSnaps.Add(snap);
            }
        }
        DestroyVertexSnapsGroup();
        snapsGroup = new GameObject();
        foreach (var snap in vertexSnaps)
        {
            var snapObject = InstantiateSnapObject(snap);
            snapObject.transform.parent = snapsGroup.transform;
        }

        return vertexSnaps;
    }

    // Gathers all valid user-created snaps from object in the scene.
    private List<Snap> GetUserSnaps()
    {
        var userSnaps = new List<Snap>();
        return userSnaps;
    }

    private void DestroyVertexSnapsGroup()
    {
        if (snapsGroup != null)
        {
            DestroyImmediate(snapsGroup);
        }
    }

    private GameObject InstantiateSnapObject(Snap snap)
    {
        var snapObject = Instantiate(snapPointObject) as GameObject;
        snapObject.transform.position = snap.position;
        snapObject.transform.up = snap.normal;
        return snapObject;
    }
}
