﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class DrawPLY : MonoBehaviour
{
    private string BASEURL = "http://192.168.56.1:9099";
    // Start is called before the first frame update
    void Start()
    {
        DownloadFile();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void DownloadFile()
    {
        Debug.Log("Downloading model");

        string url = BASEURL + "/api/cv/download_model/";

        UnityWebRequest request = UnityWebRequest.Get(url);


        UnityWebRequestAsyncOperation op = request.SendWebRequest();

        op.completed += HandleReconstructionRun;

    }

    //handler for http request
    void HandleReconstructionRun(AsyncOperation op)
    {
        UnityWebRequestAsyncOperation aop = (UnityWebRequestAsyncOperation)op;

        meshClass result = JsonUtility.FromJson<meshClass>(aop.webRequest.downloadHandler.text);
        result.initialize();

        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        Debug.Log(result.vertices[0][0]);
        Debug.Log(result.vertices[0][1]);
        Debug.Log(result.vertices[0][2]);
        mesh.vertices = result.vertices;
        //mesh.uv = newUV;
        Debug.Log(result.faces[0]);
        Debug.Log(result.faces[1]);
        Debug.Log(result.faces[2]);
        mesh.triangles = result.faces;
       // mesh.colors = result.colours;
        GameObject model = GameObject.Find("GameObject");
        MeshFilter mf = model.GetComponent<MeshFilter>();
        mf.mesh = mesh;


        //Debug.Log(aop.webRequest.downloadHandler.text);
    }


   /* public Mesh LoadFile(string file)
    {
        Debug.Log("Loading file");
        int maximumVertex = 5000;



        //byte[] byteArray = Encoding.ASCII.GetBytes(contents);
        MemoryStream stream = new MemoryStream(file);
        int vertexCount = 0;
        Color[] colors = new Color[vertexCount];
        Vector3[] vertices = new Vector3[vertexCount];
        Vector3[] normals = new Vector3[vertexCount];
        int[] indecies = new int[vertexCount];


        Matrix4x4 Rs = transform.R;
        for (int k = 0; k < 4; k++)
        {
            for (int l = 0; l < 4; l++)
            {
                Rs[k, l] *= transform.s;
            }
        }
        // convert stream to string

        //MemoryStream mStrm = new MemoryStream(Encoding.UTF8.GetBytes(file));


        Mesh data = new Mesh();
        int levelOfDetails = 1;

        using (BinaryReader reader = new BinaryReader(stream))
        {
            int cursor = 0;
            int length = (int)reader.BaseStream.Length;
            string lineText = "";
            bool header = true;
            int colorDataCount = 3;
            int index = 0;
            int step = 0;
            Debug.Log("Length: " + length);
            while (cursor + step < length)
            {
                if (header)
                {
                    char v = reader.ReadChar();
                    if (v == '\n')
                    {
                        if (lineText.Contains("end_header"))
                        {
                            header = false;
                        }
                        else if (lineText.Contains("element vertex"))
                        {
                            string[] array = lineText.Split(' ');
                            if (array.Length > 0)
                            {
                                int subtractor = array.Length - 2;
                                vertexCount = Convert.ToInt32(array[array.Length - subtractor]);
                                if (vertexCount > maximumVertex)
                                {
                                    levelOfDetails = 1 + (int)Mathf.Floor(vertexCount / maximumVertex);
                                    vertexCount = maximumVertex;
                                }

                                vertices = new Vector3[vertexCount];
                                normals = new Vector3[vertexCount];
                                colors = new Color[vertexCount];
                                indecies = new int[vertexCount];
                            }
                        }
                        else if (lineText.Contains("property uchar alpha"))
                        {
                            colorDataCount = 4;
                        }
                        lineText = "";
                    }
                    else
                    {
                        lineText += v;
                    }
                    step = sizeof(char);
                    cursor += step;
                }
                else
                {
                    if (index < vertexCount)
                    {

                        vertices[index] = (/*Rs * *//*new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle())) /*+ new Vector4(transform.t.x, transform.t.y, transform.t.z,1)*//*;
                        normals[index] = new Vector3(-reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                        Color col = new Color(reader.ReadByte() / 255f, reader.ReadByte() / 255f, reader.ReadByte() / 255f, 1f);

                        if (col.r == 0f && col.b == 0f && col.g == 0f)
                            colors[index] = new Color(0.149f, 0.149f, 0.149f, 1f);
                        else
                            colors[index] = col;

                        indecies[index] = index;

                        step = sizeof(float) * 6 * levelOfDetails + sizeof(byte) * colorDataCount * levelOfDetails;
                        cursor += step;
                        if (colorDataCount > 3)
                        {
                            reader.ReadByte();
                        }

                        if (levelOfDetails > 1)
                        {
                            for (int l = 1; l < levelOfDetails; ++l)
                            {
                                for (int f = 0; f < 6; ++f)
                                {
                                    reader.ReadSingle();
                                }
                                for (int b = 0; b < colorDataCount; ++b)
                                {
                                    reader.ReadByte();
                                }
                            }
                        }
                        Debug.Log(cursor + step + ", " + index);
                        ++index;
                    }
                    else
                    {
                        break;
                    }
                }
            }

        }
        data.vertices = vertices;
        data.normals = normals;
        data.colors = colors;
        data.SetIndices(indecies, MeshTopology.Points, 0);


        return data;

    }
}
       /* Vector3 center = new Vector3();
        //file = Regex.Replace(file, @" +", " ");
        int numberOfVertices = 0;
        StringReader strReader = new StringReader(file);
        while (true)
        {

            string line = strReader.ReadLine();
            if (line == null || line == "end_header") break;
            if (line.Length == 0) continue;
            if (line.StartsWith("element vertex "))
            {
                string[] v = line.Substring(0).Split(' ');

                numberOfVertices = Int32.Parse(v[2]);

                //Debug.Log("\n" + numberOfVertices + " vertices to draw\n");
            }

        }

        Mesh model = new Mesh();
        //model.colors = new Color[numberOfVertices];
        // model.vertices = new Vector3[numberOfVertices];
        Vector3[] vertices = new Vector3[numberOfVertices];
        Color[] colors = new Color[numberOfVertices];
        int[] indecies = new int[numberOfVertices];

        for (int i = 0; i < numberOfVertices; i++)
        {
            string line = strReader.ReadLine();
            if (line == null || line.Length == 0)
            {
                Debug.Log("There's something wrong with the file");
                break;
            }


            string[] v = line.Substring(0).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);


            try
            {
                vertices[i] = new Vector3(float.Parse(v[0]), float.Parse(v[1]), float.Parse(v[2]));
            }
            catch(Exception e)
            {
                Debug.Log(v.Length);
                Debug.Log(float.Parse(v[0]) +", "+ float.Parse(v[1]) + ", " + float.Parse(v[2]));
            }

            center += vertices[i];

            CheckMinMax(vertices[i], i);

            if (dense)
            {
                colors[i] = new Color(float.Parse(v[6]), float.Parse(v[7]), float.Parse(v[8]));
            }
            else
            {

                colors[i] = new Color(float.Parse(v[3]), float.Parse(v[4]), float.Parse(v[5]));
                //Debug.Log(float.Parse(v[3]) + " " + float.Parse(v[4]) + " " + float.Parse(v[5]));
            }



            indecies[i] = i;

        }

        Debug.Log("All data read");

        model.vertices = vertices;
        model.colors = colors;

        model.SetIndices(indecies, MeshTopology.Points, 0);

        GameObject reconstruction = GameObject.Find("ReconstructionObject");

        BoxCollider collider = reconstruction.GetComponent<BoxCollider>();

        collider.size = new Vector3(Math.Abs(maxX - minX), Math.Abs(maxY - minY), Math.Abs(maxZ - minZ));

        collider.center = center / numberOfVertices;

        return numberOfVertices == 0 ? null : model;
    }

    void CheckMinMax(Vector3 coords, int i) {
        if (i == 0) {
            minX = coords.x;
            maxX = coords.x;
            minY = coords.y;
            maxY = coords.y;
            minZ = coords.z;
            maxZ = coords.z;
        }
        else
        {
            if (coords.x < minX)
                minX = coords.x;
            else
                maxX = coords.x;

            if (coords.y < minY)
                minY = coords.y;
            else
                maxY = coords.y;

            if (coords.z < minZ)
                minZ = coords.z;
            else
                maxZ = coords.z;
        }

    }*/

}
