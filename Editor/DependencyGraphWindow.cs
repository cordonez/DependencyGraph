namespace Cordonez.DependencyGraph.Editor
{
	using System.Linq;
	using UnityEditor;
	using UnityEngine;

	internal class DependencyGraphWindow : EditorWindow
	{
		[MenuItem("Assets/Show dependency graph", false, 999)]
		private static void Init()
		{
			DependencyGraphWindow window = (DependencyGraphWindow) GetWindow(typeof(DependencyGraphWindow), false, "Dependency Graph", false);
			window.Show();
		}

		private void OnGUI()
		{
			if (GUILayout.Button("Scan the project"))
			{
				DependencyGraphManager.ScanProject();
				DependencyGraphIOUtility.Save(DependencyGraphManager.AssetCollection.Values.ToList());
			}

			DependencyGraphAssetDrawer.DrawGraphForActiveObject();
		}

		private void OnProjectChange()
		{
			RepaintWindow();
		}

		private void OnSelectionChange()
		{
			RepaintWindow();
			EditorUtility.FocusProjectWindow();
		}

		private static void RepaintWindow()
		{
			DependencyGraphWindow window = GetWindow<DependencyGraphWindow>();
			if (window != null)
			{
				window.Repaint();
			}
		}
	}
}