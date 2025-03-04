namespace ClusteringPlugin.StackClustering
{
    internal class OccurrenceAPClusterer : PythonStackClusterer
    {
        static readonly string ScriptPath = @"C:\Users\benjaming\source\repos\Clustering\ClusteringPlugin\Parsing\StackClustering\stack_clusterer.py";
        static readonly string Algorithm = "occurrence_ap";

        public OccurrenceAPClusterer() : base(ScriptPath, Algorithm)
        {
        }
    }
}
