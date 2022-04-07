using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer),(typeof(MeshFilter)))]
public class MeshGenerator : MonoBehaviour
{
    public Mesh mesh;
    public MeshFilter filter;
    public Renderer render;
    List<int> triangles=new();
    List<Vector3> vertices=new();
    int[,] carte=null;
    Material mat;
    GameManager manager;
    public void Init(int[,] carte,GameManager manager){
        this.carte=carte;
        this.manager=manager;
        StartGeneration();
    }

    public void StartGeneration(){
        render=GetComponent<MeshRenderer>();
        filter=GetComponent<MeshFilter>();
        GenerateMesh();
    }

    public void GenerateMesh(){
        mesh=new Mesh{name="dsfhsd"};
        mesh.vertices=TriangleGen();
        mesh.triangles=new int[]{0,1,2,3,4,5};
        filter.mesh=mesh;
    }

    public Vector3[] TriangleGen(){
        Vector3[] v3=new Vector3[6];
        v3[0]=new(0,0);
        v3[1]=new(1,0);
        v3[2]=new(0,1);
        v3[3]=new(1,0);
        v3[4]=new(0,1);
        v3[5]=new(1,1);
        return v3;
    }
}
