using System.Drawing.Printing;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using Sirenix.OdinInspector;
using TimeRelated;
using TMPro;
using VRC.Udon.Common;

namespace HUD
{
    public class HUDManager : PlayerFollower
    {
        [Title("HUD Canvas", "Probably the child Canvas, would be disabled initially"), Required, ChildGameObjectsOnly]
        public GameObject hudCanvas;

        private bool _isHUDEnabled;
        [Title("Points earned")] [Required] public PointSystem pointSystem;
        [ChildGameObjectsOnly, Required] public TextMeshProUGUI earnedPoints;

        [InfoBox("Use {0} in the string to show earned points and \\n for a new line.")]
        public string earnedPointsFormat = "Earned: {0} points";

        [Title("Day & Time")]
        [Required,SceneObjectsOnly] public Clock clock;
        [Required, ChildGameObjectsOnly]
        public TextMeshProUGUI dayTimeText;

        [Title("Inventory")] [Required, ChildGameObjectsOnly] //,InfoBox("TODO")]
        public Transform inventory;

        [Title("Setting")] [SerializeField] private bool debugMode = false;

        public override void Start()
        {
            base.Start();
            
            if (!hudCanvas) hudCanvas = transform.Find("HUD Canvas").gameObject;
            if (hudCanvas)
                hudCanvas.SetActive(false);
            else
            {
                Debug.LogWarning("The HUD Canvas is not assigned on HUD MANAGER!");
            }
        }

        private void Update()
        {
            if (earnedPoints)
                earnedPoints.text =
                    string.Format(earnedPointsFormat,
                        (int)pointSystem.GetProgramVariable("points")); //TODO: Need further test.

            if (clock && dayTimeText)
            {
                dayTimeText.text = $"Day {clock.dayCount} \n {clock.curGameVirtualTimeString}";
            }
            if (debugMode)
                Debug.Log("points1: " + (int)pointSystem.GetProgramVariable("points") +
                          $"\n points2: {pointSystem.points}");
            /*if (Input.GetKeyDown(KeyCode.H))
            {
                hudCanvas.SetActive(!hudCanvas.activeSelf);
            }*/
        }
        
        public override void InputJump(bool value, UdonInputEventArgs args)
        {
            //Override jump to show or hide the hud and inventory
            //  This uses the "Jump" input (A button or Space key by default)
            if (value && hudCanvas) //Button pressed
            {
                //_isHUDEnabled = !_isHUDEnabled;
                hudCanvas.SetActive(!hudCanvas.activeSelf);
            }
        }
    }
}