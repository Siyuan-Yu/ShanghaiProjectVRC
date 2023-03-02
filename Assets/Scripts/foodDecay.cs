
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
public class foodDecay : UdonSharpBehaviour
{

    //NEED TO REPLACE THIS TIME TO THE SYSTEM TIME
    public float startTime;
    public float fadeTime;

    public GameObject food1;
    private bool _startDecay;

    public int playerID;
    public Text playerIDText;
    void Start()
    {
        _startDecay = false;
    }


    void OnTriggerEnter(Collider other)
    {
        // Collider otherCollider = collision.collider;
        // if (other.GetType() == typeof(VRCPlayerApi))
        // {
        //     Networking.SetOwner(other, gameObject);
        // }

        Debug.Log("collision happens w/o names!");
        if (GetComponent<Collider>().gameObject.name.Contains("fridge"))
        {

            Debug.Log("collision happens!");
            
        }
    }

    void OnTriggerExit(Collider other)
    {
        // Collider otherCollider = collision.collider;

        // if(otherCollider == collider)
        // {
        Debug.Log("stop collision!");

        _startDecay = true;
        
       
    }


    void Update()
    {

        //IF IT COLLIDES WITH FRIDGE THEN NOTHING HAPPENS

        //ELSE IF
        if (_startDecay) {

        float transparency = 1.0f - (Time.time - startTime) / fadeTime;
        transparency = Mathf.Clamp01(transparency);

        // Debug.Log(transparency);

        Material material = food1.GetComponent<Renderer>().material;

        material.color = new Color(material.color.r, material.color.g, material.color.b, transparency);
        }

        // playerID = gameObject.
        // playerIDText.text = 
   
    }
}
