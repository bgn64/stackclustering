namespace ClusteringPlugin.StackClustering
{
    internal class KeywordAPClusterer : PythonStackClusterer
    {
        static readonly string ScriptPath = @"C:\Users\benjaming\source\repos\Clustering\ClusteringPlugin\Parsing\StackClustering\stack_clusterer.py";
        static readonly string Algorithm = "keyword_ap";

        public KeywordAPClusterer() : base(ScriptPath, Algorithm)
        {
        }
    }
}
