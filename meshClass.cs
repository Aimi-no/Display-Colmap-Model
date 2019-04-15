using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

//class to get the name of the folder from JSON
[Serializable]
public class meshClass
{
    public int[] faces;
    public float[] verts;
    public int[] cols;
    public Vector3[] vertices;
    public Color[] colours;

    public void initialize()
    {
        int size = verts.Length;
        vertices = new Vector3[size / 3];
        colours = new Color[size / 3];
        for (int i = 0; i < size/3; i++)
        {
            int index = 3 * i;
            vertices[i] = new Vector3(verts[index], verts[index + 1], verts[index + 2]);
            colours[i] = new Color(verts[index], verts[index + 1], verts[index + 2]);
        }
    }
}