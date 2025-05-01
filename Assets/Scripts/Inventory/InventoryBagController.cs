using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;
using HUD;
using Sirenix.OdinInspector;

namespace Inventory
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class InventoryBagController : UdonSharpBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject inventoryBag;
        
        [Header("Positioning")]
        [Tooltip("Offset from player position when summoned")]
        [SerializeField] private Vector3 bagOffset = new Vector3(0, 0, 0.5f);

        [Header("Settings")] 
        [SerializeField,InfoBox("TODO")] private float distToHideBag = 10f;
        [Tooltip("Enable/disable the inventory toggle functionality")]
        [SerializeField] private bool isEnabled = false; // Disabled by default until HUD is open
        [SerializeField] private bool debugMode = false;
        
        private bool _isBagVisible = false;
        private VRCPlayerApi _localPlayer;
        
        private void Start()
        {
            _localPlayer = Networking.LocalPlayer;
            
            if (!inventoryBag)
            {
                Debug.LogError("[InventoryBagController] Inventory bag GameObject not assigned!");
                return;
            }
            
            // Initialize bag as hidden
            inventoryBag.SetActive(false);
        }
        
        public override void InputUse(bool value, UdonInputEventArgs args)
        {
            // Only respond on button down, not release, and only when enabled 
            if (value && isEnabled)
            {
                ToggleBag();
            }
        }
        
        private void Update()
        {
            // Alternative keyboard input - press B key to toggle bag when enabled
            if (Input.GetKeyDown(KeyCode.B) && isEnabled)
            {
                ToggleBag();
            }

            if (!isEnabled)
            {
                
            }
        }
        
        private void ToggleBag()
        {
            if (debugMode)
            {
                Debug.Log($"[InventoryBagController] Toggling bag visibility: {!_isBagVisible}");
            }
            
            if (!_isBagVisible)
            {
                // Show bag at offset from player
                var playerPos = _localPlayer.GetPosition();
                var playerRot = _localPlayer.GetRotation();
                
                // Calculate position with offset relative to player's forward direction
                var offsetPosition = playerPos + (playerRot * bagOffset);
                
                // Move bag to position
                inventoryBag.transform.position = offsetPosition;
                
                // Make bag face the same direction as player
                inventoryBag.transform.rotation = playerRot;
                
                // Show bag
                inventoryBag.SetActive(true);
            }
            else
            {
                // Hide bag
                inventoryBag.SetActive(false);
            }
            
            // Toggle state
            _isBagVisible = !_isBagVisible;
        }
        
        // Public method to enable/disable bag toggling functionality
        public void SetEnabled(bool pEnabled)
        {
            isEnabled = pEnabled;
            
            /*// If disabling and bag is visible, hide it
            if (!pEnabled && _isBagVisible)
            {
                inventoryBag.SetActive(false);
                _isBagVisible = false;
                
                if (debugMode)
                {
                    Debug.Log("[InventoryBagController] Functionality disabled - hiding bag");
                }
            }*/
        }
    }
}