using System;
using Sirenix.OdinInspector;
using UdonSharp;
using UnityEngine;

namespace Events
{
    public class GameObjectActivationEvent : UdonSharpBehaviour
    {
        [SerializeField,Required] private GameObject target;

        [SerializeField, ReadOnly] private string[] methodsNames = { "Activate", "Deactivate" };
        private void Start()
        {
            if (target) return;
            
            Debug.LogError($"No target of Activation Event on {name}");
            enabled = false;
        }

        public void Activate()
        {
            target.SetActive(true);
        }

        public void Deactivate()
        {
            target.SetActive(false);
        }
    }
}