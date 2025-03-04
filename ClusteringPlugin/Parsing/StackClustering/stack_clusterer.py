import json
import sys
from sklearn.cluster import KMeans
from sklearn import metrics
import numpy as np

# Define the custom classes based on the provided structure
class StackFrame:
    def __init__(self, Module, Function):
        self.Module = Module
        self.Function = Function

class Stack:
    def __init__(self, Frames):
        self.Frames = [StackFrame(**frame) for frame in Frames]

# Define the custom class for clustering result
class ClusterResult:
    def __init__(self, n_clusters, silhouette_score, labels):
        self.n_clusters = n_clusters
        self.silhouette_score = silhouette_score
        self.labels = labels

    def to_dict(self):
        return {
            "n_clusters": self.n_clusters,
            "silhouette_score": self.silhouette_score,
            "labels": self.labels
        }

# Function to deserialize the JSON content from the file
def deserialize_stacks(file_path):
    with open(file_path, 'r') as file:
        data = json.load(file)
    stacks = [Stack(**stack) for stack in data]
    return stacks

# Function to vectorize the stacks
def vectorize_stacks(stacks):
    # Create a set of all distinct stack frames
    distinct_frames = set()
    for stack in stacks:
        for frame in stack.Frames:
            distinct_frames.add((frame.Module, frame.Function))
    
    # Create a list of distinct frames to assign unique indices
    distinct_frames = list(distinct_frames)
    frame_index = {frame: idx for idx, frame in enumerate(distinct_frames)}
    
    # Create vectors for each stack
    vectors = []
    for stack in stacks:
        vector = [0] * len(distinct_frames)
        for frame in stack.Frames:
            idx = frame_index[(frame.Module, frame.Function)]
            vector[idx] += 1
        vectors.append(vector)
    
    return np.array(vectors), distinct_frames

# Constants for clustering implementations
OCCURRENCE_KMEANS = "occurrence_kmeans"

# Function to perform K-means clustering
def occurrence_kmeans_clustering(stacks):
    vectors, distinct_frames = vectorize_stacks(stacks)
    
    # Perform K-means clustering
    kmeans = KMeans(n_clusters=10, random_state=0).fit(vectors)
    
    return ClusterResult(
        n_clusters=kmeans.n_clusters,
        silhouette_score=metrics.silhouette_score(vectors, kmeans.labels_),
        labels=kmeans.labels_.tolist()
    )

# Function to perform clustering based on the selected implementation
def perform_clustering(stacks, method):
    if method == OCCURRENCE_KMEANS:
        return occurrence_kmeans_clustering(stacks)
    elif method == "other_method_1":
        # Placeholder for another clustering method implementation
        return ClusterResult(
            n_clusters=0,
            silhouette_score=0.0,
            labels=[]
        )
    elif method == "other_method_2":
        # Placeholder for another clustering method implementation
        return ClusterResult(
            n_clusters=0,
            silhouette_score=0.0,
            labels=[]
        )
    else:
        raise ValueError(f"Unknown clustering method: {method}")

if __name__ == "__main__":
    # Read file path and clustering method from command line arguments
    #input_arg = sys.argv[1]
    #file_path, clustering_method = input_arg.rsplit(' ', 1)

    # Read file path and clustering method from command line arguments
    file_path = sys.argv[1]
    clustering_method = sys.argv[2]
    
    # Deserialize the stacks from the provided file
    stacks = deserialize_stacks(file_path)
    
    # Perform clustering on the deserialized stacks using the selected method
    result = perform_clustering(stacks, clustering_method)
    
    # Output the result as JSON
    print(json.dumps(result.to_dict()))