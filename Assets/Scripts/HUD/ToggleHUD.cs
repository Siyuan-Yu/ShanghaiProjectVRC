using System;
using Sirenix.OdinInspector;
using UdonSharp;
using UnityEngine;
using UnityEngine.Rendering;
using VRC.Udon.Common;
using VRC.SDKBase;

namespace HUD
{
    public class ToggleHUD : UdonSharpBehaviour
    {
        [Title("HUD Canvas", "Probably the child Canvas, would be disabled initially"), Required]//, ChildGameObjectsOnly]
        public GameObject hudCanvas;
        private void Start()
        {
            //if(!hudCanvas) hudCanvas = transform.Find("HUD Canvas").gameObject;
            /*if (hudCanvas) 
                hudCanvas.SetActive(false);
            else
            {
            
                Debug.LogWarning("The HUD Canvas is not assigned on HUD MANAGER!");
            }*/
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
                hudCanvas.SetActive(!hudCanvas.activeSelf);
            }
        }

        public override void InputJump(bool value, UdonInputEventArgs args) // doesn't work???
        {
            Debug.Log("Jumped");
            if (value)
            {
                hudCanvas.SetActive(!hudCanvas.activeSelf);
            }
        }
    }
}
