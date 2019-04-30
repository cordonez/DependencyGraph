namespace Cordonez.DependencyGraph.Editor
{
	internal class RemovedAsset : BaseAsset
	{
		private readonly string m_path;

		public RemovedAsset(string _path)
		{
			m_path = _path;
		}

		internal override void Execute()
		{
			if (!DependencyGraphManager.AssetCollection.ContainsKey(m_path))
			{
				return;
			}

			foreach (string dependency in DependencyGraphManager.AssetCollection[m_path].DependenciesPaths)
			{
				if (DependencyGraphManager.AssetCollection.ContainsKey(dependency))
				{
					DependencyGraphManager.AssetCollection[dependency].RemoveReference(m_path);
				}
			}

			foreach (string reference in DependencyGraphManager.AssetCollection[m_path].ReferencesPaths)
			{
				if (DependencyGraphManager.AssetCollection.ContainsKey(reference))
				{
					DependencyGraphManager.AssetCollection[reference].RemoveDependency(m_path);
				}
			}

			DependencyGraphManager.AssetCollection.Remove(m_path);
		}
	}
}