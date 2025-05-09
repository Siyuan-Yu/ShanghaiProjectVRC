using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

// Make sure to place this script in an Editor folder
namespace Editor
{
    public class ObjectRenamer : OdinEditorWindow
    {
        [MenuItem("Tools/Object Renamer")]
        private static void OpenWindow()
        {
            GetWindow<ObjectRenamer>("VRChat Object Renamer").Show();
        }

        [TitleGroup("Find and Rename Objects")]
        [InfoBox("Find objects by name and add numbered suffixes to make them unique for VRChat networking")]
        [SerializeField, LabelText("Search Name")]
        private string searchName = "";

        [TitleGroup("Find and Rename Objects")]
        [LabelText("Starting Index")]
        [SerializeField] 
        private int startIndex = 1;

        [TitleGroup("Find and Rename Objects")]
        [SerializeField, LabelText("Suffix Separator")] 
        private string separator = "_";

        [TitleGroup("Find and Rename Objects")]
        [LabelText("Search In Selection Only")]
        [SerializeField] 
        private bool searchInSelectionOnly = false;

        [TitleGroup("Find and Rename Objects")]
        [LabelText("Preview Only (No Renaming)")]
        [SerializeField] 
        private bool previewOnly = true;

        [TitleGroup("Results")]
        [ReadOnly]
        [ListDrawerSettings(ShowIndexLabels = true)]
        [SerializeField]
        private List<RenamePreview> renameResults = new List<RenamePreview>();

        [System.Serializable]
        public class RenamePreview
        {
            [ReadOnly]
            public string originalName;
            [ReadOnly]
            public string originalPath;
            [ReadOnly]
            public string newName;
            [ReadOnly]
            public GameObject gameObject;
        }

        [TitleGroup("Find and Rename Objects")]
        [Button("Find Objects")]
        private void FindObjects()
        {
            renameResults.Clear();

            if (string.IsNullOrEmpty(searchName))
            {
                EditorUtility.DisplayDialog("Error", "Please enter a search name", "OK");
                return;
            }

            GameObject[] rootObjects;
            if (searchInSelectionOnly)
            {
                rootObjects = Selection.gameObjects;
            }
            else
            {
                rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene()
                    .GetRootGameObjects();
            }

            List<GameObject> foundObjects = new List<GameObject>();

            foreach (var root in rootObjects)
            {
                // First search in the root objects
                if (root.name.Contains(searchName))
                {
                    foundObjects.Add(root);
                }

                // Then search in all children
                foundObjects.AddRange(
                    root.GetComponentsInChildren<Transform>(true)
                        .Where(t => t.gameObject != root && t.name.Contains(searchName))
                        .Select(t => t.gameObject)
                );
            }

            if (foundObjects.Count == 0)
            {
                EditorUtility.DisplayDialog("No Objects Found", 
                    "No objects matching the search name were found.", "OK");
                return;
            }

            // Create rename previews
            int currentIndex = startIndex;
            foreach (var obj in foundObjects)
            {
                RenamePreview preview = new RenamePreview
                {
                    originalName = obj.name,
                    originalPath = GetGameObjectPath(obj),
                    newName = $"{searchName}{separator}{currentIndex}",
                    gameObject = obj
                };

                renameResults.Add(preview);
                currentIndex++;
            }
        }

        [TitleGroup("Find and Rename Objects")]
        [Button("Apply Renaming"), GUIColor(0.4f, 1f, 0.4f)]
        private void ApplyRenaming()
        {
            if (renameResults.Count == 0)
            {
                EditorUtility.DisplayDialog("No Objects to Rename", 
                    "Please find objects first using the 'Find Objects' button.", "OK");
                return;
            }

            if (previewOnly)
            {
                EditorUtility.DisplayDialog("Preview Mode", 
                    "Preview mode is active. Uncheck 'Preview Only' to apply changes.", "OK");
                return;
            }

            Undo.RecordObjects(renameResults.Select(r => r.gameObject).ToArray(), "Rename VRChat Objects");

            foreach (var result in renameResults)
            {
                if (result.gameObject != null)
                {
                    result.gameObject.name = result.newName;
                }
            }

            EditorUtility.DisplayDialog("Renaming Complete", 
                $"Successfully renamed {renameResults.Count} objects.", "OK");
        }

        private string GetGameObjectPath(GameObject obj)
        {
            string path = obj.name;
            Transform parent = obj.transform.parent;
        
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }
        
            return path;
        }

        [TitleGroup("Find and Rename Selected Objects")]
        [InfoBox("Directly rename the currently selected objects with sequential suffixes")]
        [SerializeField, LabelText("Suffix Separator")] 
        private string selectedSeparator = "_";

        [TitleGroup("Find and Rename Selected Objects")]
        [LabelText("Starting Index")]
        [SerializeField] 
        private int selectedStartIndex = 1;

        [TitleGroup("Find and Rename Selected Objects")]
        [Button("Rename Selected Objects")]
        private void RenameSelectedObjects()
        {
            GameObject[] selectedObjects = Selection.gameObjects;
        
            if (selectedObjects.Length == 0)
            {
                EditorUtility.DisplayDialog("No Selection", 
                    "Please select one or more objects in the hierarchy first.", "OK");
                return;
            }

            Undo.RecordObjects(selectedObjects, "Rename Selected VRChat Objects");
        
            int currentIndex = selectedStartIndex;
            foreach (var obj in selectedObjects)
            {
                string baseName = obj.name;
            
                // Remove any existing numeric suffix pattern
                int lastSeparatorIndex = baseName.LastIndexOf(selectedSeparator);
                if (lastSeparatorIndex > 0)
                {
                    string potentialSuffix = baseName.Substring(lastSeparatorIndex + 1);
                    if (int.TryParse(potentialSuffix, out _))
                    {
                        baseName = baseName.Substring(0, lastSeparatorIndex);
                    }
                }
            
                obj.name = $"{baseName}{selectedSeparator}{currentIndex}";
                currentIndex++;
            }
        
            EditorUtility.DisplayDialog("Renaming Complete", 
                $"Successfully renamed {selectedObjects.Length} selected objects.", "OK");
        }
    }
}