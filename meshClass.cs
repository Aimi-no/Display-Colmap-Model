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
    public Vector3[] vertices;
    //public Color[] colours;

    /*public meshClass(int num, List<CameraItem> cams)
    {
        numberOfCams = num;
        cameras = cams;
    }*/
}