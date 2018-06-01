using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(ComputeTextureCreator))]
public class TextureCreatorInspector : Editor {

	private ComputeTextureCreator creator;

	private void OnEnable () {
		creator = target as ComputeTextureCreator;
        Undo.undoRedoPerformed += RefreshCreator;
	}

	private void OnDisable () {
		Undo.undoRedoPerformed -= RefreshCreator;
	}

	private void RefreshCreator () {
		//if (Application.isPlaying) {
			creator.FillTexture();
		//}
	}

	public override void OnInspectorGUI () {
		EditorGUI.BeginChangeCheck();
		DrawDefaultInspector();
		if (EditorGUI.EndChangeCheck()) {
			RefreshCreator();
		}
	}
}
