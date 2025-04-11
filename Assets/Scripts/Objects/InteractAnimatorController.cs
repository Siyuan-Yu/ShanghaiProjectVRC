
using System;
using Sirenix.OdinInspector;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Objects
{
    [RequireComponent(typeof(Collider))]
    public class InteractAnimatorController : UdonSharpBehaviour
    {
        [Required]
        [SerializeField] protected Animator animator;
        
        [Title("Animator Control Type")]
        [SerializeField] protected ConditionType conditionType = ConditionType.Trigger;
        
        [SerializeField,ShowIf("@conditionType==ConditionType.Trigger")] [Space]
        protected string triggerName = "Interact";
        
        [SerializeField,ShowIf("@conditionType==ConditionType.Trigger")] 
        protected string interactionText = "Use";
        
        [SerializeField,ShowIf("@conditionType==ConditionType.Bool")]
        protected string boolName = "Open";
        protected bool _boolValue = false;
    
        [SerializeField, ShowIf("@conditionType==ConditionType.Bool")] [Space]
        protected string interactionText1 = "Open";
        
        [SerializeField, ShowIf("@conditionType==ConditionType.Bool")]
        protected string interactionText2 = "Close";

        protected virtual void Start()
        {
            if (!animator)
            {
                Debug.LogWarning($"animator is not set on {name}");
            }
            if (conditionType == ConditionType.Trigger)
            {
                animator.ResetTrigger(triggerName);
            }
        }

        public override void Interact()
        {
            switch (conditionType)
            {
                case ConditionType.Trigger:
                    InteractionText = interactionText;
                    animator.SetTrigger(triggerName);
                    break;
                case ConditionType.Bool:
                    _boolValue = !_boolValue;
                    if (!_boolValue)
                        InteractionText = interactionText1;
                    else
                        InteractionText = interactionText2;
                    animator.SetBool(boolName, _boolValue);
                    break;
                default:
                    Debug.LogWarning("Unknown condition type");
                    break;
            }
        }
    
        public virtual void SwitchBoolState()
        {
            if (conditionType == ConditionType.Trigger)
            {
                Debug.LogWarning($"{name}'s animator control type is set to Trigger. SwitchBoolState() will not have any effect.");
                return;
            }
            
            _boolValue = !_boolValue;
            animator.SetBool(boolName, _boolValue);
        }
      
    }
      public enum ConditionType
        {
            Trigger,
            Bool
        }
}