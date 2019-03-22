namespace Cordonez.DependencyGraph.Editor
{
	using System.Collections.Generic;
	using UnityEditor;

	[InitializeOnLoad]
	internal class DependencyGraphHooks : AssetPostprocessor
	{
		private static bool m_initialized;

		static DependencyGraphHooks()
		{
			if (m_initialized)
			{
				return;
			}

			m_initialized = true;
			List<DependencyGraphAsset> collection = DependencyGraphIOUtility.Load();
			DependencyGraphManager.Init(collection);
			EditorApplication.quitting += OnQuit;
			EditorApplication.update += DependencyGraphManager.Update;
		}

		private static void OnQuit()
		{
			DependencyGraphIOUtility.Save(DependencyGraphManager.AssetCollection);
			DependencyGraphManager.ClearCollection();
			EditorApplication.quitting -= OnQuit;
			EditorApplication.update -= DependencyGraphManager.Update;
			m_initialized = false;
		}

		private static void OnPostprocessAllAssets(string[] _importedAssets, string[] _deletedAssets, string[] _movedAssets, string[] _movedFromAssetPaths)
		{
			DependencyGraphManager.AddDirtyAsset(_importedAssets);
			DependencyGraphManager.AddDeletedAsset(_deletedAssets);
			for (int i = 0; i < _movedAssets.Length; i++)
			{
				DependencyGraphManager.MovedAsset(_movedFromAssetPaths[i], _movedAssets[i]);
			}
		}
	}
}