namespace Cordonez.DependencyGraph.Editor
{
	using System.Collections.Generic;
	using UnityEditor;
	using UnityEngine;

	internal static class DependencyGraphManager
	{
		internal static bool IsDirty => m_deletedAsset.Count != 0 || m_dirtyAssets.Count != 0;
		internal static List<DependencyGraphAsset> AssetCollection => new List<DependencyGraphAsset>(m_assetCollection.Values);

		private static readonly Dictionary<string, DependencyGraphAsset> m_assetCollection = new Dictionary<string, DependencyGraphAsset>();
		private static readonly List<string> m_deletedAsset = new List<string>();
		private static readonly List<string> m_dirtyAssets = new List<string>();

		internal static void ScanProject()
		{
			ClearCollection();
			UpdateDependencies();
			UpdateReferences();
			EditorUtility.ClearProgressBar();
		}

		internal static DependencyGraphAsset GetSelectedAsset(Object _asset)
		{
			if (_asset == null)
			{
				return null;
			}

			string assetPath = AssetDatabase.GetAssetPath(_asset);
			return m_assetCollection.ContainsKey(assetPath) ? m_assetCollection[assetPath] : null;
		}

		internal static DependencyGraphAsset GetSelectedAsset(string _assetPath)
		{
			if (string.IsNullOrEmpty(_assetPath))
			{
				return null;
			}

			return m_assetCollection.ContainsKey(_assetPath) ? m_assetCollection[_assetPath] : null;
		}

		private static void UpdateDependencies()
		{
			string[] assetGUIDS = AssetDatabase.FindAssets("");
			for (int i = 0; i < assetGUIDS.Length; i++)
			{
				EditorUtility.DisplayProgressBar("Scaning the project", "Updating dependencies " + i + "/" + assetGUIDS.Length, ((float) i / assetGUIDS.Length) / 2f);
				string assetPath = AssetDatabase.GUIDToAssetPath(assetGUIDS[i]);
				CreateNewItemIfNecessary(assetPath);
				m_assetCollection[assetPath].UpdateDependencies();
			}
		}

		private static void UpdateReferences()
		{
			int amount = m_assetCollection.Values.Count;
			int index = 0;
			foreach (DependencyGraphAsset graphItem in m_assetCollection.Values)
			{
				EditorUtility.DisplayProgressBar("Scaning the project", "Updating references " + index + "/" + amount, 0.5f + ((float) index / amount) / 2f);
				index++;
				if (graphItem.DependenciesPaths.Count == 0)
				{
					continue;
				}

				foreach (string dependencyPath in graphItem.DependenciesPaths)
				{
					CreateNewItemIfNecessary(dependencyPath);
					m_assetCollection[dependencyPath].AddReference(graphItem.Path);
				}
			}
		}

		private static void CreateNewItemIfNecessary(string _path)
		{
			if (!m_assetCollection.ContainsKey(_path))
			{
				m_assetCollection[_path] = new DependencyGraphAsset(_path);
			}
		}

		internal static void AddDeletedAsset(string _path)
		{
			m_deletedAsset.Add(_path);
		}

		internal static void AddDeletedAsset(string[] _paths)
		{
			m_deletedAsset.AddRange(_paths);
		}

		internal static void AddDirtyAsset(string[] _paths)
		{
			m_dirtyAssets.AddRange(_paths);
		}

		internal static void AddDirtyAsset(string _asset)
		{
			m_dirtyAssets.Add(_asset);
		}

		internal static void Init(List<DependencyGraphAsset> _collection)
		{
			if (_collection == null)
			{
				return;
			}

			foreach (DependencyGraphAsset dependencyGraphAsset in _collection)
			{
				m_assetCollection.Add(dependencyGraphAsset.Path, dependencyGraphAsset);
			}
		}

		internal static void Update()
		{
			if (m_deletedAsset.Count != 0 || m_dirtyAssets.Count != 0)
			{
				AssetDatabase.SaveAssets();
			}

			if (m_deletedAsset.Count != 0)
			{
				foreach (string deletedAsset in m_deletedAsset)
				{
					if (!m_assetCollection.ContainsKey(deletedAsset))
					{
						continue;
					}

					foreach (string dependency in m_assetCollection[deletedAsset].DependenciesPaths)
					{
						if (m_assetCollection.ContainsKey(dependency))
						{
							m_assetCollection[dependency].RemoveReference(deletedAsset);
						}
					}

					m_assetCollection.Remove(deletedAsset);
				}
			}

			if (m_dirtyAssets.Count != 0)
			{
				foreach (string dirtyAsset in m_dirtyAssets)
				{
					//Remove old references (dependencies will be removed when generating the new ones)
					if (m_assetCollection.ContainsKey(dirtyAsset))
					{
						foreach (string dependency in m_assetCollection[dirtyAsset].DependenciesPaths)
						{
							if (m_assetCollection.ContainsKey(dependency))
							{
								m_assetCollection[dependency].RemoveReference(dirtyAsset);
							}
						}
					}

					//Add new dependencies and references
					CreateNewItemIfNecessary(dirtyAsset);
					m_assetCollection[dirtyAsset].UpdateDependencies();
					foreach (string dependency in m_assetCollection[dirtyAsset].DependenciesPaths)
					{
						CreateNewItemIfNecessary(dependency);
						m_assetCollection[dependency].AddReference(dirtyAsset);
					}
				}
			}

			m_deletedAsset.Clear();
			m_dirtyAssets.Clear();
		}

		internal static void ClearCollection()
		{
			m_assetCollection.Clear();
			m_deletedAsset.Clear();
			m_dirtyAssets.Clear();
		}

		internal static void MovedAsset(string _sourcePath, string _destinationPath)
		{
			if (m_assetCollection.ContainsKey(_sourcePath))
			{
				AddDirtyAsset(_destinationPath);
				AddDeletedAsset(_sourcePath);
			}
		}
	}
}