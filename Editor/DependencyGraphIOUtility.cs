namespace Cordonez.DependencyGraph.Editor
{
	using System;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Windows;

	internal static class DependencyGraphIOUtility
	{
		private static readonly string m_jsonPath = Application.dataPath.Remove(Application.dataPath.LastIndexOf('/')) + "/Library/DependencyGraph.json";

		internal static List<DependencyGraphAsset> Load()
		{
			if (!File.Exists(m_jsonPath))
			{
				return null;
			}

			string json = System.IO.File.ReadAllText(m_jsonPath);
			return JsonUtility.FromJson<AssetCollection>(json).Collection;
		}

		internal static void Save(List<DependencyGraphAsset> _assetCollection)
		{
			string json = JsonUtility.ToJson(new AssetCollection(_assetCollection), true);
			System.IO.File.WriteAllText(m_jsonPath, json);
		}

		[Serializable]
		private class AssetCollection
		{
			internal List<DependencyGraphAsset> Collection => m_collection;

			[SerializeField]
			private List<DependencyGraphAsset> m_collection;

			internal AssetCollection(List<DependencyGraphAsset> _collection)
			{
				m_collection = _collection;
			}
		}
	}
}