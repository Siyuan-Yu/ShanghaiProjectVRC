using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using Sirenix.OdinInspector;
using System;

namespace Events.Triggers
{
    [RequireComponent(typeof(Collider))]
    public class TriggerEventTrigger : UdonSharpBehaviour
    {
        [TitleGroup("Trigger Settings")]
        [Tooltip("If disabled, this trigger won't fire events")]
        public bool triggerEnabled = true;
        
        [Tooltip("Which player types can activate this trigger")]
        public TriggerTarget targetType = TriggerTarget.All;
        
        [Tooltip("Events to fire when a player enters the trigger")]
        [SerializeField] private bool useOnTriggerEnter = false;

        [ShowIf("useOnTriggerEnter")] [SerializeField,HorizontalGroup("OnTriggerEnter"),Tooltip("Target UdonBehaviour to receive the event"),Required]
        private UdonSharpBehaviour[] onEnterEventsTarget;
        
        [ShowIf("useOnTriggerEnter")] [SerializeField,HorizontalGroup("OnTriggerEnter"),Tooltip("Name of the event method to call")]
        private string[] onEnterEventsName;
        
        [ShowIf("useOnTriggerEnter")] [SerializeField,HorizontalGroup("OnTriggerEnter"),Tooltip("Optional delay in seconds"),Min(0f)]
        private float[] onEnterEventsDelay;
        
        
        [Tooltip("Events to fire when a player exits the trigger")]
        [SerializeField] private bool useOnTriggerExit = false;
        
        [ShowIf("useOnTriggerExit")] [SerializeField,HorizontalGroup("OnTriggerExit"),Tooltip("Target UdonBehaviour to receive the event"),Required]
        private UdonSharpBehaviour[] onExitEventsTarget;
        
        [ShowIf("useOnTriggerExit")] [SerializeField,HorizontalGroup("OnTriggerExit"),Tooltip("Name of the event method to call")]
        private string[] onExitEventsName;
        
        [ShowIf("useOnTriggerExit")] [SerializeField,HorizontalGroup("OnTriggerExit"),Tooltip("Optional delay in seconds"),Min(0f)]
        private float[] onExitEventsDelay;
        
        
        
        [Tooltip("Events to fire while a player stays in the trigger")]
        [SerializeField] private bool useOnTriggerStay = false;
        
        [ShowIf("useOnTriggerStay")] [SerializeField,HorizontalGroup("OnTriggerStay"),Tooltip("Target UdonBehaviour to receive the event"),Required]
        private UdonSharpBehaviour[] onStayEventsTarget;
        
        [ShowIf("useOnTriggerStay")] [SerializeField,HorizontalGroup("OnTriggerStay"),Tooltip("Name of the event method to call")]
        private string[] onStayEventsName;
        
        [ShowIf("useOnTriggerStay")] [SerializeField,HorizontalGroup("OnTriggerStay"),Tooltip("Optional delay in seconds"),Min(0f)]
        private float[] onStayEventsDelay;
        
        [Tooltip("How often to fire stay events (seconds)")]
        [ShowIf("useOnTriggerStay"), Min(0.1f)]
        [SerializeField] private float stayEventInterval = 1.0f;
        
        private float lastStayEventTime = 0f;
        private bool playerInTrigger = false;
        
        /*[Serializable]
        public class TriggerEvent
        {
            [Tooltip("Target UdonBehaviour to receive the event"),Required]
            public UdonSharpBehaviour target;
            
            [Tooltip("Name of the event method to call")]
            public string eventName;
            
            [Tooltip("Optional delay in seconds")]
            [Min(0f)] public float delay;
        }*/
        
        
        
        private void Start()
        {
            // Make sure this has a trigger collider
            var col = GetComponent<Collider>();
            if (col.isTrigger) return;
            
            Debug.LogWarning($"Collider on {name} should be set as a trigger!");
            col.isTrigger = true;
        }
        
        private void Update()
        {
            // Handle OnTriggerStay events
            if (useOnTriggerStay && playerInTrigger && triggerEnabled)
            {
                if (Time.time >= lastStayEventTime + stayEventInterval)
                {
                    lastStayEventTime = Time.time;
                    FireTriggerEvents(onStayEventsTarget, onStayEventsName, onStayEventsDelay);
                }
            }
        }

        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            if (!triggerEnabled) return;

            // Check if this player should trigger events
            if (!ShouldProcessPlayer(player)) return;

            playerInTrigger = true;

            if (useOnTriggerEnter)
            {
                FireTriggerEvents(onEnterEventsTarget, onEnterEventsName, onEnterEventsDelay);
            }
        }

        public override void OnPlayerTriggerExit(VRCPlayerApi player)
        {
            if (!triggerEnabled) return;

            // Check if this player should trigger events
            if (!ShouldProcessPlayer(player)) return;

            playerInTrigger = false;

            if (useOnTriggerExit)
            {
                FireTriggerEvents(onExitEventsTarget, onExitEventsName, onEnterEventsDelay);
            }
        }
        
        private bool ShouldProcessPlayer(VRCPlayerApi player)
        {
            switch (targetType)
            {
                case TriggerTarget.LocalPlayerOnly:
                    return player.isLocal;
                case TriggerTarget.HostOnly:
                    return player.isMaster;
                case TriggerTarget.RemotePlayersOnly:
                    return !player.isLocal;
                case TriggerTarget.All:
                default:
                    return true;
            }
        }
        
        private void FireTriggerEvents(UdonSharpBehaviour[] eventTargets, string[] eventNames, float[] eventDelays)
        {
            for(var i=0; i<eventTargets.Length; i++)
            {
                if (!eventTargets[i]) continue;
                if (eventNames.Length <= i) continue;
                
                if (eventDelays.Length > i || eventDelays[i] <= 0)
                {
                    eventTargets[i].SendCustomEvent(eventNames[i]);
                }
                else
                {
                    eventTargets[i].SendCustomEventDelayedSeconds(eventNames[i], eventDelays[i]);
                }
            }
        }
        /*private void FireTriggerEvents(TriggerEvent[] events)
        {
            foreach (var evt in events)
            {
                if (evt.target) continue; 
                
                if (evt.delay <= 0)
                {
                    evt.target.SendCustomEvent(evt.eventName);
                }
                else
                {
                    evt.target.SendCustomEventDelayedSeconds(evt.eventName, evt.delay);
                }
            }
        }*/
    }
    public enum TriggerTarget //enum cannot be in a class
    {
        All,
        HostOnly,
        LocalPlayerOnly,
        RemotePlayersOnly
    }
}