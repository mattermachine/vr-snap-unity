using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VR;

public class SnapEngine : MonoBehaviour
{
    public Text text;
    public GameObject objectsGroup;
    public GameObject libraryGroup;
    public GameObject snapGameobject;
    public GameObject pointerGameobject;
    private Transform pointerTransform;
    private GameObject snapsGroup;
    private Camera mainCamera;
    private Material pointerMaterial;
    private bool dragging = false;
    private float pointerZ;
    private float defaultPointerZ = 5;
    private List<GameObject> draggedObjects;
    private List<Snap> snaps;
    private Vector3 hitPoint;
    private Vector3 hitNormal;
    private Transform hitTransform;
    private bool flipNormal = false;
    public bool doVertexSnaps;
    public bool dontRayWhenDragging;
    public Color defaultPointerColor = new Color(0, 0, 0, .3f);
    public Color hoverPointerColor = new Color(0.89f, 0.82f, 0.02f, 0.58f);
    public Color draggingPointerColor = new Color(0.88f, 0.38f, 0f, 0.62f);
    public Color snappedPointerColor = new Color(0.93f, 0.22f, 0f, 0.8f);
    public bool pointerIsSnapped = false;
    public float librarySwitchAngle = 50;

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
        mainCamera = pointerGameobject.transform.parent.GetComponent<Camera>();
        pointerMaterial = pointerGameobject.GetComponent<Renderer>().sharedMaterial;
        VRSettings.showDeviceView = true;
        pointerTransform = pointerGameobject.transform;
        snaps = GetUserSnaps();
        pointerZ = defaultPointerZ;
        //Screen.SetResolution(VRSettings.eyeTextureWidth, VRSettings.eyeTextureHeight, false);
        //Debug.Log((" width: " + VRSettings.eyeTextureWidth + "  height: " + VRSettings.eyeTextureHeight));
    }

    void Update()
    {

        // Zoom using mousewheel.
        if (Input.GetAxis("Mouse ScrollWheel") > 0f ) // forward
        {
            mainCamera.transform.parent.Translate(mainCamera.transform.forward);
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0f ) // backwards
        {
            mainCamera.transform.parent.position = new Vector3(0,2,0);  // Snap back to overview.
        }

        // Show library when looking up.
//        Debug.Log(mainCamera.transform.eulerAngles.x);
        libraryGroup.SetActive((mainCamera.transform.eulerAngles.x - 360) < -librarySwitchAngle && mainCamera.transform.eulerAngles.x > 180);

        bool rayDidHit = false;
        if (!(dragging && dontRayWhenDragging))
        {
            // Ray against objects in the scene.
            RaycastHit rayCastHit;
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            rayDidHit = Physics.Raycast(ray, out rayCastHit, 1000);
            if (rayDidHit)
            {
                hitPoint = rayCastHit.point;
                hitNormal = rayCastHit.normal;
                hitTransform = rayCastHit.transform;
                pointerZ = mainCamera.WorldToScreenPoint(hitPoint).z;
                pointerTransform.position = hitPoint;
                pointerTransform.up = flipNormal ? -hitNormal : hitNormal;
            }
        }

        var pointerIsSnapped = SnapPointer();

        if (dragging)
        {
            pointerMaterial.color = pointerIsSnapped ? snappedPointerColor : draggingPointerColor;

            // When dragging, RMB flips the dragged object along the pointer normal.
            if (Input.GetMouseButtonDown(1))
            {
                flipNormal = !flipNormal;
            }

            // Dragged object released.
            if (Input.GetMouseButtonUp(0))
            {
                dragging = false;
                flipNormal = false;
                foreach (var draggedObject in draggedObjects)
                {
                    if (libraryGroup.activeSelf)
                    {
                        draggedObject.transform.parent = libraryGroup.transform;
                    }
                    else
                    {
                        draggedObject.transform.parent = objectsGroup.transform;
                    }
                    draggedObject.GetComponent<Collider>().enabled = true;  // re-enable collider
                }
                DestroyVertexSnapsGroup();
                snaps = GetUserSnaps();
            }
        }
        else  // not dragging
        {
            if (rayDidHit)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    dragging = true;
                    draggedObjects = new List<GameObject>();
                    GetConnectedObjects(hitTransform.gameObject);
                    foreach (var draggedObject in draggedObjects)
                    {
                        draggedObject.GetComponent<Collider>().enabled = false;  // disable collider -> disables raycasting against this object
                        draggedObject.transform.parent = pointerTransform; // parent object to pointer
                    }

                    snaps = new List<Snap>();
                    if (doVertexSnaps)
                    {
                        snaps.AddRange(GetVertexSnaps());
                    }
                    snaps.AddRange(GetUserSnaps());

                    pointerMaterial.color = draggingPointerColor;
                }
                else
                {
                    pointerMaterial.color = pointerIsSnapped ? snappedPointerColor : hoverPointerColor;
                }

                // Create snap point at pointer.
                if (Input.GetMouseButtonUp(1))
                {
                    var snap = new Snap();
                    snap.position = hitPoint;
                    snap.normal = hitNormal;
                    var snapObject = InstantiateSnapObject(snap);
                    snapObject.transform.parent = hitTransform;
                    snaps = GetUserSnaps();
                }
            }
            else  // no ray hit, not dragging
            {
                pointerZ = defaultPointerZ;
                pointerTransform.position = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, pointerZ));
                pointerTransform.forward = Vector3.up;
                pointerMaterial.color = defaultPointerColor;
            }
        }

        //text.text = (" width: " + VRSettings.eyeTextureWidth + "  height: " + VRSettings.eyeTextureHeight);
        text.text = ("x: " + Input.mousePosition.x + " y: " + Input.mousePosition.y);
    }

    private bool SnapPointer()
    {
        bool snapped = false;

        // Snap to closest snap in screenspace, if any within radius.
        var adjustedMousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, pointerZ);
        foreach (var snap in snaps)
        {
            var screenPosition = mainCamera.WorldToScreenPoint(snap.position);
            if (!(Mathf.Pow(Input.mousePosition.x - screenPosition.x, 2) +
                  Mathf.Pow(Input.mousePosition.y - screenPosition.y, 2) < snapDistance * snapDistance)) continue;
            adjustedMousePosition = screenPosition;
            pointerZ = screenPosition.z;
            pointerTransform.up = dragging ? snap.normal : -snap.normal;   // simulate male-female
            pointerTransform.up = flipNormal ? -pointerTransform.up : pointerTransform.up;
            snapped = true;
            break;
        }
        pointerTransform.position = mainCamera.ScreenToWorldPoint(adjustedMousePosition);

        return snapped;
    }
    // Generates snaps at vertices, on-the-fly.
    private List<Snap> GetVertexSnaps()
    {
        var meshFilters = new List<MeshFilter>();
        foreach (Transform child in objectsGroup.transform)
        {
            var meshFilter = child.GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                meshFilters.Add(meshFilter);
            }
        }
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

    // Finds all objects connected to the dragged one.
    private void GetConnectedObjects(GameObject draggedObject)
    {
        if (draggedObjects.Contains(draggedObject))
        {
            return;
        }
        draggedObjects.Add(draggedObject);
        var draggedSnapObjects = draggedObject.GetComponentsInChildren<SnapObject>().ToList();
        var otherSnapObjects = objectsGroup.GetComponentsInChildren<SnapObject>().ToList();
        // First remove dragged snap objects from the other snap objects.
        foreach (var draggedSnapObject in draggedSnapObjects)
        {
            if (otherSnapObjects.Contains(draggedSnapObject))
            {
                otherSnapObjects.Remove(draggedSnapObject);
            }
        }

        foreach (var draggedSnapObject in draggedSnapObjects)
        {
            foreach (var otherSnapObject in otherSnapObjects)
            {
                if (draggedSnapObject.transform.position == otherSnapObject.transform.position)
                {
                    var otherObject = otherSnapObject.transform.parent.gameObject;
                    // Recurse to get all connected objects
                    // FIXME avoid infinite loops
                    GetConnectedObjects(otherObject);
                }
            }
        }
    }

    // Gathers all valid user-created snaps from object in the scene.
    private List<Snap> GetUserSnaps()
    {
        var userSnaps = new List<Snap>();
        List<SnapObject> snapObjects = objectsGroup.GetComponentsInChildren<SnapObject>().ToList();
        snapObjects.AddRange(libraryGroup.GetComponentsInChildren<SnapObject>().ToList());
        foreach (var snapObject in snapObjects)
        {
            var snap = new Snap();
            snap.position = snapObject.transform.position;
            snap.normal = snapObject.transform.up;
            userSnaps.Add(snap);
        }

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
        var snapObject = Instantiate(this.snapGameobject) as GameObject;
        snapObject.transform.position = snap.position;
        snapObject.transform.up = snap.normal;
        return snapObject;
    }
}
