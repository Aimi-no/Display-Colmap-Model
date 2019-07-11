using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class DrawPly : MonoBehaviour
{
    private string BASEURL = "http://10.35.100.210:9099";
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

        string url = BASEURL + "/api/cv/get_textured_model/";

        UnityWebRequest request = UnityWebRequest.Get(url);


        UnityWebRequestAsyncOperation op = request.SendWebRequest();

        op.completed += HandleReconstructionRun;

    }

    //handler for http request
    void HandleReconstructionRun(AsyncOperation op)
    {
        UnityWebRequestAsyncOperation aop = (UnityWebRequestAsyncOperation)op;

        MeshClass result = JsonUtility.FromJson<MeshClass>(aop.webRequest.downloadHandler.text);
        result.initialize();

        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        /*Debug.Log(result.vertices[0][0]);
        Debug.Log(result.vertices[0][1]);
        Debug.Log(result.vertices[0][2]);*/
        mesh.vertices = result.vertices;
        //mesh.uv = newUV;
        /*Debug.Log(result.faces[0]);
        Debug.Log(result.faces[1]);
        Debug.Log(result.faces[2]);*/
        mesh.triangles = result.faces;

        mesh.uv = result.uv;

        // mesh.colors = result.colours;
        GameObject model = GameObject.Find("GameObject");
        MeshFilter mf = model.GetComponent<MeshFilter>();
        mf.mesh = mesh;


        DownloadTexture();
        //Debug.Log(aop.webRequest.downloadHandler.text);
    }

    void DownloadTexture()
    {
        Debug.Log("Downloading texture");

        string url = BASEURL + "/api/cv/download_texture/";

        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);


        UnityWebRequestAsyncOperation op = request.SendWebRequest();

        op.completed += HandleTextureDownload;
    }

    void HandleTextureDownload(AsyncOperation op)
    {
        UnityWebRequestAsyncOperation aop = (UnityWebRequestAsyncOperation)op;

        Texture result =((DownloadHandlerTexture)aop.webRequest.downloadHandler).texture;
        

        Renderer m_Renderer;
        m_Renderer = GetComponent<MeshRenderer>();
        m_Renderer.material.SetTexture("_MainTex", result);
    }


}