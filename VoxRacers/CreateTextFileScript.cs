using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.IO;
using System.Text;

public class CreateTextFileScript : MonoBehaviour
{
    public GameObject original;
    public string posTextFileName;
    public string colTextFileName;

    //text
    private StreamWriter posWrite;
    private StreamWriter colWrite;

    // Use this for initialization
    void Start ()
    {       
        string path = "Assets/Resources/Particle Text Files/" + posTextFileName + ".txt";
        posWrite = new StreamWriter(path, true);

        path = "Assets/Resources/Particle Text Files/" + colTextFileName + ".txt";
        colWrite = new StreamWriter(path, true);

        GenerateTxtFile();

        Debug.Log("GENERATED TEXT FILE");

        //write.WriteLine("Testing {0}", 1);
        //write.WriteLine("Testing {0}", 2);
        //write.Close();

        //read = new StreamReader(path);
        //Debug.Log(read.ReadLine());
        //Debug.Log(read.ReadLine());
        //read.Close();
    }
	
	// Update is called once per frame
	void Update ()
    {

    }

    void GenerateTxtFile()
    {
        //create builder
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < original.transform.childCount; i++)
        {
            //store position into string
            sb.Clear();
            sb.Append(original.transform.GetChild(i).localPosition.x).Append(" ").Append(original.transform.GetChild(i).localPosition.y).Append(" ").Append(original.transform.GetChild(i).localPosition.z).Append(" ");
            //store info of original into text file
            posWrite.WriteLine(sb.ToString());

            //store ifo of colour in text file
            sb.Clear();
            sb.Append(original.transform.GetChild(i).GetComponent<MeshFilter>().mesh.colors[0].r).Append(" ").Append(original.transform.GetChild(i).GetComponent<MeshFilter>().mesh.colors[0].g).Append(" ").Append(original.transform.GetChild(i).GetComponent<MeshFilter>().mesh.colors[0].b).Append(" ");
            colWrite.WriteLine(sb.ToString());

        }

        posWrite.Close();
        colWrite.Close();

    }

}
