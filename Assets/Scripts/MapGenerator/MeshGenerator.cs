using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer),typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour
{
    public Mesh mesh;
    public MeshFilter filter;
    public Renderer render;

    public void Start(){
        render=GetComponent<Renderer>();
        filter=GetComponent<MeshFilter>();
        GenerateMesh();
    }

    public void GenerateMesh(){
        mesh=new Mesh{name="dsfhsd"};
        mesh.vertices=TriangleGen();
        mesh.triangles=new int[]{0,1,2};
        filter.mesh=mesh;
    }

    public Vector3[] TriangleGen(){
        Vector3[] v3=new Vector3[3];
        v3[0]=new(0,0,0);
        v3[1]=Vector3.right;
        v3[2]=Vector3.up;
        return v3;
    }
}
