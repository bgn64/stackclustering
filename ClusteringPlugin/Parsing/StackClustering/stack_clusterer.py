import json
import sys
from sklearn.cluster import KMeans, AffinityPropagation
from sklearn.feature_extraction.text import TfidfVectorizer
from sklearn import metrics
import numpy as np
import os
import openai
import re

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

# TextCleaner class based on the provided C# code
class TextCleaner:
    StopWords = {
        "a", "an", "and", "are", "as", "at", "be", "but", "by",
        "for", "if", "in", "into", "is", "it", "no", "not", "of",
        "on", "or", "such", "that", "the", "their", "then", "there",
        "these", "they", "this", "to", "was", "will", "with"
    }

    @staticmethod
    def clean_text(text):
        # Convert to lowercase
        cleaned = text.lower()
        # Remove punctuation
        cleaned = re.sub(r'[^\\w\\s]', '', cleaned)
        # Trim extra spaces
        cleaned = re.sub(r'\\s+', ' ', cleaned).strip()
        # Remove stop words
        cleaned = ' '.join(word for word in cleaned.split() if word not in TextCleaner.StopWords)
        return cleaned

# Function to get keywords from Azure LLM and cache results
def get_keywords_from_llm(stack_frame, cache):
    if not stack_frame.Module or not stack_frame.Function:
        return ""
    
    key = f"{stack_frame.Module}!{stack_frame.Function}"
    
    if key in cache:
        return cache[key]
    
    # Throw an exception indicating which stack_frame was not found in cache
    raise KeyError(f"Stack frame not found in cache: Module='{stack_frame.Module}', Function='{stack_frame.Function}', Key='{key}', Cache='{cache}'") 
    
    # Call Azure LLM to get keywords (using OpenAI as an example)
    response = openai.Completion.create(
        engine="text-davinci-003",
        prompt=f"Provide a response consisting of 5 or less key words that describe the purpose of the function '{stack_frame.Function}' from the module '{stack_frame.Module}'.",
        max_tokens=50,
        n=1,
        stop=None,
        temperature=0.5,
    )
    
    keywords = response.choices[0].text.strip()
    
    # Cache the result
    cache[key] = keywords
    
    return keywords

# Function to preprocess stacks using keywords from LLM and cache results
def preprocess_stacks_with_keywords(stacks, cache):
    preprocessed_stacks = []
    
    for stack in stacks:
        keywords_list = []
        
        for frame in stack.Frames:
            keywords = get_keywords_from_llm(frame, cache)
            keywords_list.append(keywords)
        
        preprocessed_stack_text = TextCleaner.clean_text(" ".join(keywords_list))
        preprocessed_stacks.append(preprocessed_stack_text)
    
    return preprocessed_stacks

# Constants for clustering implementations
OCCURRENCE_KMEANS = "occurrence_kmeans"
OCCURRENCE_AP = "occurrence_ap"
KEYWORD_AP = "keyword_ap"

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

# Function to perform AP clustering
def occurrence_ap_clustering(stacks):
    vectors, distinct_frames = vectorize_stacks(stacks)
    
    # Perform Affinity Propagation clustering with preference value -10
    ap = AffinityPropagation(preference=-10, random_state=0).fit(vectors)
    
    return ClusterResult(
        n_clusters=len(ap.cluster_centers_indices_),
        silhouette_score=metrics.silhouette_score(vectors, ap.labels_, metric="euclidean"),
        labels=ap.labels_.tolist()
    )

# Function to perform keyword-based AP clustering
def keyword_ap_clustering(stacks):
    # Load cache from file if it exists
    cache_file_path = "keyword_cache.json"
    
    if os.path.exists(cache_file_path):
        with open(cache_file_path, 'r') as cache_file:
            cache = json.load(cache_file)
    else:
        cache = {}
    
    # Preprocess stacks using keywords from LLM and cache results
    preprocessed_stacks = preprocess_stacks_with_keywords(stacks, cache)
    
    # Save updated cache to file
    with open(cache_file_path, 'w') as cache_file:
        json.dump(cache, cache_file)
    
    # Convert the text data into TF-IDF features
    vectorizer = TfidfVectorizer(stop_words='english')
    X = vectorizer.fit_transform(preprocessed_stacks)
    
    # Perform Affinity Propagation clustering with preference value -10
    ap = AffinityPropagation(preference=-10, random_state=0).fit(X)
    
    return ClusterResult(
        n_clusters=len(ap.cluster_centers_indices_),
        silhouette_score=metrics.silhouette_score(X, ap.labels_, metric="euclidean"),
        labels=ap.labels_.tolist()
    )

# Function to perform clustering based on the selected implementation
def perform_clustering(stacks, method):
    if method == OCCURRENCE_KMEANS:
        return occurrence_kmeans_clustering(stacks)
    elif method == OCCURRENCE_AP:
        return occurrence_ap_clustering(stacks)
    elif method == KEYWORD_AP:
        return keyword_ap_clustering(stacks)
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
    #  C:\Users\benjaming\Documents\sample_input.txt keywords_ap
    # Read file path and clustering method from command line arguments
    #input_arg = sys.argv[1]
    
    # Split the input argument into file path and clustering method using rsplit(' ', 1)
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