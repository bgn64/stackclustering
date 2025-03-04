using System.Text.Json.Serialization;

namespace ClusteringPlugin.StackClustering
{
    internal class ClusterResult
    {
        [JsonPropertyName("labels")]
        public int[]? Labels { get; set; }
        [JsonPropertyName("n_clusters")]
        public int NumberOfClusters { get; set; }
        [JsonPropertyName("silhouette_score")]
        public double SilhouetteScore { get; set; }
    }
}
