using System;
using System.Text;
using UnityEngine;

//class to get the name of the folder from JSON
[Serializable]
public class MeshClass
{
    public int[] faces;
    public float[] verts;
    public float[] cols;
    public float[] uvs;
    //public string imgdata;

    public Vector3[] vertices;
    public Color[] colours;
    public Vector2[] uv;
    //public byte[] texture;

    public void initialize()
    {
        int size = verts.Length;
        vertices = new Vector3[size / 3];
        colours = new Color[size / 3];
        
        for(int i = 0; i < size / 3; i++)
        {
            int index = 3 * i;
            vertices[i] = new Vector3(verts[index], verts[index + 1], verts[index + 2]);
            colours[i] = new Color(cols[index] / 255.0f, cols[index + 1] / 255.0f, cols[index + 2] / 255.0f);
        }

        size = uvs.Length;
        uv = new Vector2[size / 2];

        for(int i = 0; i < size / 2; i++)
        {
            int index = 2 * i;
            uv[i] = new Vector2(uvs[index], uvs[index + 1]);
        }

        //texture = Encoding.ASCII.GetBytes(imgdata);
    }
}
