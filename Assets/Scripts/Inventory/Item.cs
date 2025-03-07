
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Inventory
{
    [RequireComponent(typeof(Collider)),RequireComponent(typeof(Rigidbody))]
    public class Item : UdonSharpBehaviour
    {
        public string useHintText = "Use";
        public ItemCategory itemCategory;
        
        public Sprite icon;

        public override void Interact()
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            
            //TODO: FInd the local player and add it to the player's inventory
        }
    }
}

