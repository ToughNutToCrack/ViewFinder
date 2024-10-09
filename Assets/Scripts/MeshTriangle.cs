using System.Collections.Generic;
using UnityEngine;

public class MeshTriangle
{
    private int submeshIndex;

    public List<Vector3> Vertices { get; set; } = new();
    public List<Vector3> Normals { get; set; } = new();
    public List<Vector2> UVs { get; set; } = new();
    public int SubmeshIndex { get => submeshIndex; private set => SubmeshIndex = value; }

    public MeshTriangle(Vector3[] _vertices, Vector3[] _normals, Vector2[] _uvs, int _submeshIndex) 
    {
        Clear();

        Vertices.AddRange(_vertices);
        Normals.AddRange(_normals);
        UVs.AddRange(_uvs);

        submeshIndex = _submeshIndex;
    }

    private void Clear()
    {
        Vertices.Clear();
        Normals.Clear();
        UVs.Clear();
        
        submeshIndex = 0;
    }
}
