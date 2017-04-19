using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DraggableObject : MonoBehaviour
{
    public static List<DraggableObject> draggableObjects = new List<DraggableObject>();

    public bool isOnConstructionPlane = false;
    private Mesh mesh;
    private Mesh wireframeMesh;
    private Material material;
    private Material wireframeMaterial;

    // Use this for initialization
    void Start ()
    {
//	    material = Resources.Load ( "ToonLitOutline", typeof ( Material ) ) as Material;
        wireframeMaterial = Resources.Load ( "WireframeMat", typeof ( Material ) ) as Material;
        mesh = gameObject.GetComponent<MeshFilter>().mesh;
        wireframeMesh = new Mesh();
        wireframeMesh.vertices = mesh.vertices;
        wireframeMesh.SetIndices (BuildEdgeLists(mesh), MeshTopology.Lines, 0, false);
        draggableObjects.Add(this);
    }

//    public void DrawWireframe()
//    {
//	    Graphics.DrawMesh ( wireframeMesh, transform.localToWorldMatrix, wireframeMaterial, 0, null, 0, null, false, false );
//    }
    private void OnRenderObject()
    {
//	    Graphics.DrawMesh ( mesh, transform.localToWorldMatrix, material, 0, null, 0, null, false, false );
        wireframeMaterial.SetPass(0);
        Graphics.DrawMeshNow ( wireframeMesh, transform.localToWorldMatrix, 0 );
    }


    private int[] BuildEdgeLists (Mesh mesh)
    {
        var tmpEdges = new List<int> ();
        var tmpBoundaryEdges = new List<int> ();
        var connectedVertices = new Dictionary<int, Dictionary<int, bool>> (); // <vertex index, <connected vertex index, boundary edge?>>

        // build a list of non-duplicate edges
        var triangles = mesh.triangles;
        for ( int t = 0; t < triangles.Length; t+=3 ) {
            var triangle = new int[] {triangles[t], triangles[t + 1], triangles[t + 2]};

            for ( int i = 0; i < 3; i++ ) {
                int v1 = triangle[ i ];
                int v2 = triangle[ ( i + 1 ) % 3 ];

                if ( v2 < v1 ) {
                    int tmp = v1;
                    v1 = v2;
                    v2 = tmp;
                }

                if ( connectedVertices.ContainsKey ( v1 ) ) {
                    if ( !connectedVertices[ v1 ].ContainsKey ( v2 ) ) {
                        connectedVertices[ v1 ][ v2 ] = true;  //  boundary edge, until a second triangle is found to share these two vertices (ie, edge)
                        tmpEdges.Add ( v1 );
                        tmpEdges.Add ( v2 );
                    }
                    else      // this is a shared edge - ie: not a boundary edge
                        connectedVertices[ v1 ][ v2 ] = false;
                }
                else {
                    connectedVertices[ v1 ] = new Dictionary<int, bool> ();

                    if ( !connectedVertices.ContainsKey ( v2 ) )
                        connectedVertices[ v2 ] = new Dictionary<int, bool> ();

                    connectedVertices[ v1 ][ v2 ] = true;  //  boundary edge, until a second triangle is found to share these two vertices (ie, edge)
                    tmpEdges.Add ( v1 );
                    tmpEdges.Add ( v2 );
                }
            }
        }

        foreach ( int v1 in connectedVertices.Keys ) {
            foreach ( int v2 in connectedVertices[ v1 ].Keys ) {
                if ( connectedVertices[ v1 ][ v2 ] ) {
                    tmpBoundaryEdges.Add ( v1 );
                    tmpBoundaryEdges.Add ( v2 );
                }
            }
        }

//        edges = tmpEdges.ToArray ();
        return tmpBoundaryEdges.ToArray ();
    }

}
