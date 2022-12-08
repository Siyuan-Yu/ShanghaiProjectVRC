
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class foodDecay : UdonSharpBehaviour
{

    //NEED TO REPLACE THIS TIME TO THE SYSTEM TIME
    public float startTime;
    public float fadeTime;

    public GameObject food1;

    void Start()
    {
        
    }


    void Update()
    {

        //IF IT COLLIDES WITH FRIDGE THEN NOTHING HAPPENS

        //ELSE IF
        float transparency = 1.0f- ( Time.time - startTime ) / fadeTime;
        transparency = Mathf.Clamp01(transparency);

        Debug.Log(transparency);

        Material material = food1.GetComponent<Renderer>().material;

        material.color = new Color(material.color.r, material.color.g, material.color.b, transparency);
    }
}
