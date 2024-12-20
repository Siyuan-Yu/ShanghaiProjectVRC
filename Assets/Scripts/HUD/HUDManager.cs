
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using Sirenix.OdinInspector;
using TMPro;

namespace HUD
{
    public class HUDManager : UdonSharpBehaviour
    {
        [Title("Points earned")] 
        [Required]
        public PointSystem pointSystem;
        [ChildGameObjectsOnly,Required]
        public TextMeshProUGUI earnedPoints;

        [InfoBox("Use {0} in the string to show earned points and \\n for a new line.")]
        public string earnedPointsFormat = "Earned: {0} points";

        [Title("Day Count")] [Required, ChildGameObjectsOnly,InfoBox("TODO")]
        public TextMeshProUGUI dayCount;

        [Title("Inventory")] [Required, ChildGameObjectsOnly,InfoBox("TODO")]
        public Transform inventory;
        private void Update()
        {
            earnedPoints.text = string.Format(earnedPointsFormat, (int)pointSystem.GetProgramVariable("points")); //TODO: Need further test.
        }
    }

}
