using System.Collections.Generic;
using UnityEngine;

public class GeneratedMesh 
{
    List<Vector3> vertices          = new List<Vector3>();
    List<Vector3> normals           = new List<Vector3>();
    List<Vector2> uvs               = new List<Vector2>();
    List<List<int>> submeshIndices  = new List<List<int>>();

    public List<Vector3> Vertices           { get { return vertices; }          set { vertices = value; } }
    public List<Vector3> Normals            { get { return normals; }           set { normals = value; } }
    public List<Vector2> UVs                { get { return uvs; }               set { uvs = value; } }
    public List<List<int>> SubmeshIndices   { get { return submeshIndices; }    set { submeshIndices = value; } }

    public void AddTriangle(MeshTriangle _triangle)
    {
        int currentVerticeCount = vertices.Count;

        vertices.AddRange(_triangle.Vertices);
        normals.AddRange(_triangle.Normals);
        uvs.AddRange(_triangle.UVs);

        if(submeshIndices.Count < _triangle.SubmeshIndex + 1)
        {
            for (int i = submeshIndices.Count; i < _triangle.SubmeshIndex + 1; i++)
            {
                submeshIndices.Add(new List<int>());
            }
        }

        for (int i = 0; i < 3; i++)
        {
            submeshIndices[_triangle.SubmeshIndex].Add(currentVerticeCount + i);
        }
    }

    public void AddTriangle(Vector3[] _vertices, Vector3[] _normals, Vector2[] _uvs, int _submeshIndex, Vector4[] _tangents = null)
    {
        int currentVerticeCount = vertices.Count;

        vertices.AddRange(_vertices);
        normals.AddRange(_normals);
        uvs.AddRange(_uvs);

        if(submeshIndices.Count < _submeshIndex + 1)
        {
            for (int i = submeshIndices.Count; i < _submeshIndex + 1; i++)
            {
                submeshIndices.Add(new List<int>());
            }
        }

        for (int i = 0; i < 3; i++)
        {
            submeshIndices[_submeshIndex].Add(currentVerticeCount + i);
        }
    }



    public Mesh GetGeneratedMesh()
    {
        Mesh mesh = new Mesh();
        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetUVs(0, uvs);
        mesh.SetUVs(1, uvs);

        mesh.subMeshCount = submeshIndices.Count;
        for (int i = 0; i < submeshIndices.Count; i++)
        {
            mesh.SetTriangles(submeshIndices[i], i);
        }
            return mesh;
    }
    
}
