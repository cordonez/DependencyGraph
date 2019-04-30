namespace Cordonez.DependencyGraph.Editor
{
	internal class MovedAsset : BaseAsset
	{
		private readonly string m_source;
		private readonly string m_destination;

		public MovedAsset(string _source, string _destination)
		{
			m_source = _source;
			m_destination = _destination;
		}

		internal override void Execute()
		{
			if (!DependencyGraphManager.AssetCollection.ContainsKey(m_source))
			{
				return;
			}

			DependencyGraphManager.AssetCollection.Add(m_destination, DependencyGraphManager.AssetCollection[m_source]);
			DependencyGraphManager.AssetCollection.Remove(m_source);
			DependencyGraphManager.AssetCollection[m_destination].Path = m_destination;

			//Update dependencies
			foreach (string dependency in DependencyGraphManager.AssetCollection[m_destination].DependenciesPaths)
			{
				DependencyGraphManager.AssetCollection[dependency].ReferencesPaths.Remove(m_source);
				DependencyGraphManager.AssetCollection[dependency].ReferencesPaths.Add(m_destination);
			}

			//Update references
			foreach (string reference in DependencyGraphManager.AssetCollection[m_destination].ReferencesPaths)
			{
				DependencyGraphManager.AssetCollection[reference].DependenciesPaths.Remove(m_source);
				DependencyGraphManager.AssetCollection[reference].DependenciesPaths.Add(m_destination);
			}
		}
	}
}