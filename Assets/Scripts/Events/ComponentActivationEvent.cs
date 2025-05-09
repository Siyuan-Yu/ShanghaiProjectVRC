using System;
using Sirenix.OdinInspector;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Video;
using VRC.Udon;

namespace Events
{
    public enum ComponentType
    {
        UdonBehaviour,
        Renderer,
        Collider,
        Light,
        Canvas,
        ParticleSystem,
        AudioSource,
        Animator,
        //VideoPlayer //not supported
    }

    public class ComponentActivationEvent : UdonSharpBehaviour
    //"is" is not supported by Udon, Behaviour.enabled is not supported by Udon. So here we have this:
    {
        [SerializeField, Required] private GameObject targetObject;
        [SerializeField, Required] private ComponentType componentType;
        [SerializeField, ShowIf("@componentType == ComponentType.UdonBehaviour")]
        private string udonBehaviourTypeName; // For UdonBehaviour, specify the script name
        [SerializeField] private bool debugMode = false;
        [SerializeField, ReadOnly] private string[] methodsNames = { "Activate", "Deactivate" };
        
        // Cache components for better performance
        private UdonBehaviour cachedUdonBehaviour;
        private Renderer cachedRenderer;
        private Collider cachedCollider;
        private Light cachedLight;
        private Canvas cachedCanvas;
        private ParticleSystem cachedParticleSystem;
        private AudioSource cachedAudioSource;
        private Animator cachedAnimator;
        //private VideoPlayer cachedVideoPlayer;
        
        private bool initialized = false;
        
        private void Start()
        {
            if (!targetObject)
            {
                Debug.LogError($"No target GameObject set for Activation Event on {name}");
                this.enabled = false;
                return;
            }
            
            InitializeComponent();
        }
        
        private void InitializeComponent()
        {
            if (initialized) return;
            
            switch (componentType)
            {
                case ComponentType.UdonBehaviour:
                    if (string.IsNullOrEmpty(udonBehaviourTypeName))
                    {
                        Debug.LogError($"UdonBehaviour type name not specified on {name}");
                        break;
                    }
                    
                    var components = targetObject.GetComponents<Component>();
                    foreach (var comp in components)
                    {
                        if (comp.GetType().Name == udonBehaviourTypeName)
                        {
                            cachedUdonBehaviour = (UdonBehaviour)comp;
                            break;
                        }
                    }
                    
                    if (!cachedUdonBehaviour)
                    {
                        Debug.LogError($"UdonBehaviour of type {udonBehaviourTypeName} not found on {targetObject.name}");
                    }
                    break;
                    
                case ComponentType.Renderer:
                    cachedRenderer = targetObject.GetComponent<Renderer>();
                    if (!cachedRenderer) Debug.LogError($"Renderer not found on {targetObject.name}");
                    break;
                    
                case ComponentType.Collider:
                    cachedCollider = targetObject.GetComponent<Collider>();
                    if (!cachedCollider) Debug.LogError($"Collider not found on {targetObject.name}");
                    break;
                    
                case ComponentType.Light:
                    cachedLight = targetObject.GetComponent<Light>();
                    if (!cachedLight) Debug.LogError($"Light not found on {targetObject.name}");
                    break;
                    
                case ComponentType.Canvas:
                    cachedCanvas = targetObject.GetComponent<Canvas>();
                    if (!cachedCanvas) Debug.LogError($"Canvas not found on {targetObject.name}");
                    break;
                    
                case ComponentType.ParticleSystem:
                    cachedParticleSystem = targetObject.GetComponent<ParticleSystem>();
                    if (!cachedParticleSystem) Debug.LogError($"ParticleSystem not found on {targetObject.name}");
                    break;
                    
                case ComponentType.AudioSource:
                    cachedAudioSource = targetObject.GetComponent<AudioSource>();
                    if (!cachedAudioSource) Debug.LogError($"AudioSource not found on {targetObject.name}");
                    break;
                    
                case ComponentType.Animator:
                    cachedAnimator = targetObject.GetComponent<Animator>();
                    if (!cachedAnimator) Debug.LogError($"Animator not found on {targetObject.name}");
                    break;
                /*case ComponentType.VideoPlayer:
                    cachedVideoPlayer = targetObject.GetComponent<VideoPlayer>();
                    if (!cachedVideoPlayer) Debug.LogError($"VideoPlayer not found on {targetObject.name}");
                    break;*/
                default:
                    Debug.LogError($"Unknown component type {componentType} on {name}");
                    break;
            }
            
            initialized = true;
        }

        public void Activate()
        {
            if (!initialized) InitializeComponent();
            
            switch (componentType)
            {
                case ComponentType.UdonBehaviour:
                    if (cachedUdonBehaviour)
                    {
                        cachedUdonBehaviour.enabled = true;
                        if (debugMode) Debug.Log($"Activated UdonBehaviour: {udonBehaviourTypeName} on {targetObject.name}");
                    }
                    break;
                    
                case ComponentType.Renderer:
                    if (cachedRenderer)
                    {
                        cachedRenderer.enabled = true;
                        if (debugMode) Debug.Log($"Activated Renderer on {targetObject.name}");
                    }
                    break;
                    
                case ComponentType.Collider:
                    if (cachedCollider)
                    {
                        cachedCollider.enabled = true;
                        if (debugMode) Debug.Log($"Activated Collider on {targetObject.name}");
                    }
                    break;
                    
                case ComponentType.Light:
                    if (cachedLight)
                    {
                        cachedLight.enabled = true;
                        if (debugMode) Debug.Log($"Activated Light on {targetObject.name}");
                    }
                    break;
                    
                case ComponentType.Canvas:
                    if (cachedCanvas)
                    {
                        cachedCanvas.enabled = true;
                        if (debugMode) Debug.Log($"Activated Canvas on {targetObject.name}");
                    }
                    break;
                    
                case ComponentType.ParticleSystem:
                    if (cachedParticleSystem)
                    {
                        cachedParticleSystem.Play();
                        if (debugMode) Debug.Log($"Started ParticleSystem on {targetObject.name}");
                    }
                    break;
                    
                case ComponentType.AudioSource:
                    if (cachedAudioSource)
                    {
                        cachedAudioSource.enabled = true;
                        if (debugMode) Debug.Log($"Activated AudioSource on {targetObject.name}");
                    }
                    break;
                    
                case ComponentType.Animator:
                    if (cachedAnimator)
                    {
                        cachedAnimator.enabled = true;
                        if (debugMode) Debug.Log($"Activated Animator on {targetObject.name}");
                    }
                    break;
                /*case ComponentType.VideoPlayer:
                    if (cachedVideoPlayer)
                    {
                        cachedVideoPlayer.enabled = true;
                        if (debugMode) Debug.Log($"Activated VideoPlayer on {targetObject.name}");
                    }
                    break;*/
                default:
                    Debug.LogError($"Unknown component type {componentType} on {name}");
                    break;
            }
        }

        public void Deactivate()
        {
            if (!initialized) InitializeComponent();
            
            switch (componentType)
            {
                case ComponentType.UdonBehaviour:
                    if (cachedUdonBehaviour)
                    {
                        cachedUdonBehaviour.enabled = false;
                        if (debugMode) Debug.Log($"Deactivated UdonBehaviour: {udonBehaviourTypeName} on {targetObject.name}");
                    }
                    break;
                    
                case ComponentType.Renderer:
                    if (cachedRenderer)
                    {
                        cachedRenderer.enabled = false;
                        if (debugMode) Debug.Log($"Deactivated Renderer on {targetObject.name}");
                    }
                    break;
                    
                case ComponentType.Collider:
                    if (cachedCollider)
                    {
                        cachedCollider.enabled = false;
                        if (debugMode) Debug.Log($"Deactivated Collider on {targetObject.name}");
                    }
                    break;
                    
                case ComponentType.Light:
                    if (cachedLight)
                    {
                        cachedLight.enabled = false;
                        if (debugMode) Debug.Log($"Deactivated Light on {targetObject.name}");
                    }
                    break;
                    
                case ComponentType.Canvas:
                    if (cachedCanvas)
                    {
                        cachedCanvas.enabled = false;
                        if (debugMode) Debug.Log($"Deactivated Canvas on {targetObject.name}");
                    }
                    break;
                    
                case ComponentType.ParticleSystem:
                    if (cachedParticleSystem)
                    {
                        cachedParticleSystem.Stop();
                        if (debugMode) Debug.Log($"Stopped ParticleSystem on {targetObject.name}");
                    }
                    break;
                    
                case ComponentType.AudioSource:
                    if (cachedAudioSource)
                    {
                        cachedAudioSource.enabled = false;
                        if (debugMode) Debug.Log($"Deactivated AudioSource on {targetObject.name}");
                    }
                    break;
                    
                case ComponentType.Animator:
                    if (cachedAnimator)
                    {
                        cachedAnimator.enabled = false;
                        if (debugMode) Debug.Log($"Deactivated Animator on {targetObject.name}");
                    }
                    break;
                /*case ComponentType.VideoPlayer:
                    if (cachedVideoPlayer)
                    {
                        cachedVideoPlayer.enabled = false;
                        if (debugMode) Debug.Log($"Deactivated VideoPlayer on {targetObject.name}");
                    }
                    break;*/
                default:
                    Debug.LogError($"Unknown component type {componentType} on {name}");
                    break;
            }
        }
    }
}