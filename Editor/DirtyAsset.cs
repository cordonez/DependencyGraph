namespace Cordonez.DependencyGraph.Editor
{
	using System.Collections.Generic;

	internal class DirtyAsset : BaseAsset
	{
		private readonly string m_path;

		internal DirtyAsset(string _path)
		{
			m_path = _path;
		}

		internal override void Execute()
		{
			List<string> removedReferences = new List<string>();
			//Remove old references and dependencies
			if (DependencyGraphManager.AssetCollection.ContainsKey(m_path))
			{
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
						removedReferences.Add(reference);
					}
				}
			}

			//Add new dependencies and references
			DependencyGraphManager.CreateNewItemIfNecessary(m_path);
			DependencyGraphManager.AssetCollection[m_path].UpdateDependencies();
			foreach (string dependency in DependencyGraphManager.AssetCollection[m_path].DependenciesPaths)
			{
				DependencyGraphManager.CreateNewItemIfNecessary(dependency);
				DependencyGraphManager.AssetCollection[dependency].AddReference(m_path);
			}

			foreach (string removedReference in removedReferences)
			{
				DependencyGraphManager.AssetCollection[removedReference].UpdateDependencies();
				if (DependencyGraphManager.AssetCollection[removedReference].DependenciesPaths.Contains(m_path))
				{
					DependencyGraphManager.AssetCollection[m_path].AddReference(removedReference);
				}
			}
		}
	}
}