namespace Cordonez.DependencyGraph.Editor
{
	using System.Collections.Generic;
	using UnityEditor;

	internal static class DependencyGraphManager
	{
		internal static bool IsDirty => m_dirtyAssets.Count != 0;

		internal static readonly Dictionary<string, DependencyGraphAsset> AssetCollection = new Dictionary<string, DependencyGraphAsset>();

		private static readonly List<BaseAsset> m_dirtyAssets = new List<BaseAsset>();

		internal static void Init(List<DependencyGraphAsset> _collection)
		{
			if (_collection == null)
			{
				return;
			}

			ClearCollection();
			foreach (DependencyGraphAsset dependencyGraphAsset in _collection)
			{
				AssetCollection.Add(dependencyGraphAsset.Path, dependencyGraphAsset);
			}
		}

		internal static void ScanProject()
		{
			ClearCollection();
			UpdateDependencies();
			UpdateReferences();
		}

		internal static DependencyGraphAsset GetSelectedAsset(string _assetPath)
		{
			if (string.IsNullOrEmpty(_assetPath))
			{
				return null;
			}

			return AssetCollection.ContainsKey(_assetPath) ? AssetCollection[_assetPath] : null;
		}

		internal static void AddDeletedAsset(string[] _paths)
		{
			foreach (string path in _paths)
			{
				m_dirtyAssets.Add(new RemovedAsset(path));
			}
		}

		internal static void AddDirtyAsset(string[] _paths)
		{
			foreach (string path in _paths)
			{
				m_dirtyAssets.Add(new DirtyAsset(path));
			}
		}

		internal static void AddMovedAsset(string _sourcePath, string _destinationPath)
		{
			m_dirtyAssets.Add(new MovedAsset(_sourcePath, _destinationPath));
		}

		internal static void Update()
		{
			foreach (BaseAsset baseAsset in m_dirtyAssets)
			{
				baseAsset.Execute();
			}

			m_dirtyAssets.Clear();
		}

		private static void UpdateDependencies()
		{
			string[] assetGuids = AssetDatabase.FindAssets("");
			for (int i = 0; i < assetGuids.Length; i++)
			{
				EditorUtility.DisplayProgressBar("Scaning the project", "Updating dependencies " + i + "/" + assetGuids.Length, (float) i / assetGuids.Length / 2f);
				string assetPath = AssetDatabase.GUIDToAssetPath(assetGuids[i]);
				CreateNewItemIfNecessary(assetPath);
				AssetCollection[assetPath].UpdateDependencies();
			}
		}

		private static void UpdateReferences()
		{
			int amount = AssetCollection.Values.Count;
			int index = 0;
			foreach (DependencyGraphAsset graphItem in AssetCollection.Values)
			{
				EditorUtility.DisplayProgressBar("Scaning the project", "Updating references " + index + "/" + amount, 0.5f + (float) index / amount / 2f);
				index++;
				if (graphItem.DependenciesPaths.Count == 0)
				{
					continue;
				}

				foreach (string dependencyPath in graphItem.DependenciesPaths)
				{
					CreateNewItemIfNecessary(dependencyPath);
					AssetCollection[dependencyPath].AddReference(graphItem.Path);
				}
			}

			EditorUtility.ClearProgressBar();
		}

		internal static void CreateNewItemIfNecessary(string _path)
		{
			if (!AssetCollection.ContainsKey(_path))
			{
				AssetCollection[_path] = new DependencyGraphAsset(_path);
			}
		}

		private static void ClearCollection()
		{
			AssetCollection.Clear();
			m_dirtyAssets.Clear();
		}
	}
}