namespace Cordonez.DependencyGraph.Editor
{
	using System;
	using System.Collections.Generic;
	using UnityEditor;
	using UnityEngine;

	[Serializable]
	internal class DependencyGraphAsset
	{
		[SerializeField]
		internal string Path;
		[SerializeField]
		internal List<string> DependenciesPaths = new List<string>();
		[SerializeField]
		internal List<string> ReferencesPaths = new List<string>();

		internal DependencyGraphAsset(string _path)
		{
			Path = _path;
		}

		internal void UpdateDependencies()
		{
			DependenciesPaths.Clear();
			DependenciesPaths.AddRange(AssetDatabase.GetDependencies(Path, false));
		}

		internal void AddReference(string _graphAssetPath)
		{
			if (!ReferencesPaths.Contains(_graphAssetPath))
			{
				ReferencesPaths.Add(_graphAssetPath);
			}
		}

		internal void RemoveReference(string _graphAssetPath)
		{
			if (ReferencesPaths.Contains(_graphAssetPath))
			{
				ReferencesPaths.Remove(_graphAssetPath);
			}
		}
	}
}