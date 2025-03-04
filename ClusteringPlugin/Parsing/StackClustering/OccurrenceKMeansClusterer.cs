namespace ClusteringPlugin.StackClustering
{
    internal class OccurrenceKMeansClusterer : PythonStackClusterer
    {
        static readonly string ScriptPath = @"C:\Users\benjaming\source\repos\Clustering\ClusteringPlugin\Parsing\StackClustering\stack_clusterer.py";
        static readonly string Algorithm = "occurrence_kmeans";

        public OccurrenceKMeansClusterer() : base(ScriptPath, Algorithm)
        {
        }
    }
}
