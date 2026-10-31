using UnityEditor;
using UnityEngine;

// Muestra el nivel de RedCircle en la ventana Scene fuera de Play.
// Los objetos son temporales (DontSave): no se guardan en la escena y se rehacen al salir de Play.
[InitializeOnLoad]
static class LevelPreview
{
    const string RootName = "Nivel (vista previa)";

    static LevelPreview()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
        EditorApplication.delayCall += Rebuild;
    }

    static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode) Clear();
        else if (state == PlayModeStateChange.EnteredEditMode) Rebuild();
    }

    [MenuItem("Tools/Actualizar vista previa del nivel")]
    static void Rebuild()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        Clear();
        var root = RedCircle.BuildLevel(RootName);
        foreach (var t in root.GetComponentsInChildren<Transform>(true))
            t.gameObject.hideFlags = HideFlags.DontSave;
        SceneView.RepaintAll();
    }

    static void Clear()
    {
        var old = GameObject.Find(RootName);
        if (old != null) Object.DestroyImmediate(old);
    }
}
