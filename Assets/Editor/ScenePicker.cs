using UnityEditor.Toolbars;
using UnityEditor.Overlays;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using UnityEngine;

[EditorToolbarElement(id, typeof(SceneView))]
class ScenePickerDropdown : EditorToolbarDropdown
{
    public const string id = "Toolbar/ScenePicker";

    private static string _selectedScenePath;

    public ScenePickerDropdown()
    {
        text = "ScenePicker";
        clicked += ShowDropdown;
    }

    private void ShowDropdown()
    {
        var scenes = EditorBuildSettings.scenes;
        var menu = new GenericMenu();
        foreach (var scene in scenes)
        {
            if (string.IsNullOrEmpty(scene.path))
                continue;

            string scenePath = scene.path;
            string sceneName = Path.GetFileNameWithoutExtension(scenePath);
            menu.AddItem(
                new GUIContent(sceneName),
                _selectedScenePath == scenePath,
                () => OpenScene(scenePath));
        }

        menu.ShowAsContext();
    }

    private void OpenScene(string scenePath)
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        EditorSceneManager.OpenScene(scenePath);
        _selectedScenePath = scenePath;
        text = Path.GetFileNameWithoutExtension(scenePath);
    }

}
[Overlay(typeof(SceneView), "ScenePicker", defaultDisplay = true)]
public class EditorScenePicker : ToolbarOverlay
{
    EditorScenePicker() : base(
ScenePickerDropdown.id
)
    { }

}
