namespace Cordonez.DependencyGraph.Editor
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using UnityEditor;

	[InitializeOnLoad]
	internal class DependencyGraphHooks : AssetPostprocessor
	{
		static DependencyGraphHooks()
		{
			EditorApplication.update += Update;
			EditorApplication.playModeStateChanged += PlayModeStateChangeHandler;
			PlayModeStateChangeHandler(PlayModeStateChange.EnteredEditMode);
		}

		private static void Update()
		{
			if (!DependencyGraphManager.IsDirty)
			{
				return;
			}

			AssetDatabase.SaveAssets();
			DependencyGraphManager.Update();
			DependencyGraphIOUtility.Save(DependencyGraphManager.AssetCollection.Values.ToList());
		}

		private static void PlayModeStateChangeHandler(PlayModeStateChange _playModeState)
		{
			switch (_playModeState)
			{
				case PlayModeStateChange.EnteredEditMode:
				case PlayModeStateChange.EnteredPlayMode:
					List<DependencyGraphAsset> collection = DependencyGraphIOUtility.Load();
					DependencyGraphManager.Init(collection);
					break;
				case PlayModeStateChange.ExitingEditMode:
				case PlayModeStateChange.ExitingPlayMode:
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(_playModeState), _playModeState, null);
			}
		}

		private static void OnPostprocessAllAssets(string[] _importedAssets, string[] _deletedAssets, string[] _movedAssets, string[] _movedFromAssetPaths)
		{
			DependencyGraphManager.AddDirtyAsset(_importedAssets);
			DependencyGraphManager.AddDeletedAsset(_deletedAssets);
			for (int i = 0; i < _movedAssets.Length; i++)
			{
				DependencyGraphManager.AddMovedAsset(_movedFromAssetPaths[i], _movedAssets[i]);
			}
		}
	}
}