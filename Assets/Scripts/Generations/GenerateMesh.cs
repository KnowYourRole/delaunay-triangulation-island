using UnityEngine;
using System.Collections.Generic;

public class GenerateMesh
{
    public List<Vector3> vertices;
    public Dictionary<string, List<int>> submeshes;
    public List<int> triangles;

    int cellIndex;

    public GenerateMesh()
    {
        cellIndex = 0;
        vertices = new List<Vector3>();
        triangles = new List<int>();
        submeshes = new Dictionary<string, List<int>>();
    }

    public void AddTriangleIndices(int a, int b, int c, string submesh)
    {

        List<int> triangle = new List<int>
        {
            a + cellIndex,
            b + cellIndex,
            c + cellIndex
        };

        triangles.AddRange(triangle);

        if (!submeshes.ContainsKey(submesh))
            submeshes.Add(submesh, new List<int>());

        submeshes[submesh].AddRange(triangle);
    }

    public List<int> AddPolygon(int vertexCount, string submesh)
    {
        switch (vertexCount)
        {
            case 3:
                AddTriangleIndices(0, 2, 1, submesh);
                break;
            case 4:
                AddTriangleIndices(0, 2, 1, submesh);
                AddTriangleIndices(0, 3, 2, submesh);
                break;
            default:
                break;
        }

        if (vertexCount > 4)
        {
            AddTriangleIndices(0, 4, 3, submesh);
            AddTriangleIndices(0, 3, 1, submesh);
            AddTriangleIndices(1, 3, 2, submesh);

            for (int i = 0; i < vertexCount - 5; i++)
                AddTriangleIndices(0, 5 + i, 4 + i, submesh);

        }
        cellIndex += vertexCount;

        return triangles;
    }


    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray(); //stores vertices to array
        mesh.subMeshCount = submeshes.Count;

        int i = 0;
        foreach (var submesh in submeshes)
        {
            mesh.SetTriangles(submesh.Value, i);
            i++;
        }
        mesh.RecalculateNormals();
        return mesh;
    }

}