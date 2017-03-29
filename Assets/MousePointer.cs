using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VR;

public class MousePointer : MonoBehaviour {

    public Text text;
    public GameObject objectsGroup;
    public GameObject snapPointObject;
    private GameObject snapsGroup;
    private Camera camera;
    private Material material;
    private bool dragging = false;
    //private Vector3 objectToHitpointLocalDelta;
    private float hitPointZ;
    //private Vector3 mouseStartPosition;
    private GameObject hitObject;
    private List<Vector3> snappingPoints;
    
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
    
    void Update() {

        if (dragging)
        {
            var adjustedMousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, hitPointZ);
            foreach (var vertex in snappingPoints)
            {
                var screenVertex = camera.WorldToScreenPoint(vertex);
                if (Mathf.Pow(Input.mousePosition.x-screenVertex.x, 2) + Mathf.Pow(Input.mousePosition.y - screenVertex.y, 2) < 10*10)  // 10px snapping radius
                {
                    adjustedMousePosition = screenVertex;
                    hitPointZ = screenVertex.z;
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
                var hitPoint = rayCastHit.point;
                var hitNormal = rayCastHit.normal;
                transform.position = hitPoint;
                if (Input.GetMouseButtonDown(0))
                {
                    hitObject = rayCastHit.transform.gameObject;
                    rayCastHit.transform.parent = transform;
                    //objectToHitpointLocalDelta = transform.InverseTransformVector(rayCastHit.transform.position-hitPoint);
                    snappingPoints = GetSnappingVertices();
                    hitPointZ = camera.WorldToScreenPoint(hitPoint).z;
                    //Debug.Log(hitPointZ);
                    //mouseStartPosition = Input.mousePosition;
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
                transform.position = camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y,10));
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

    List<Vector3> GetSnappingVertices()
    {
        MeshFilter[] meshFilters = objectsGroup.GetComponentsInChildren<MeshFilter>();
        var vertices = new List<Vector3>();
        foreach (var meshFilter in meshFilters)
        {
            var tmpTransform = meshFilter.transform;
            foreach (var vertex in meshFilter.mesh.vertices)
            {
                vertices.Add(tmpTransform.TransformPoint(vertex));
            }
        }

        if (snapsGroup != null)
        {
            GameObject.DestroyImmediate(snapsGroup);
        }
        snapsGroup = new GameObject();
        foreach (var snap in vertices)
        {
            var snapObject = GameObject.Instantiate(snapPointObject) as GameObject;
            snapObject.transform.position = snap;
            snapObject.transform.parent = snapsGroup.transform;
        }

        return vertices;
    }
}
