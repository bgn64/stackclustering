import sys
import json
from sklearn.feature_extraction.text import TfidfVectorizer
from sklearn.cluster import AffinityPropagation
from sklearn import metrics

def cluster_texts(documents, preference=0):
    # Convert the text data into TF-IDF features
    vectorizer = TfidfVectorizer(stop_words='english')
    X = vectorizer.fit_transform(documents)

    # Perform Affinity Propagation clustering
    af = AffinityPropagation(preference=preference, random_state=0).fit(X)
    cluster_centers_indices = af.cluster_centers_indices_
    labels = af.labels_

    n_clusters_ = len(cluster_centers_indices)
    silhouette_score = metrics.silhouette_score(X, labels, metric="euclidean")

    return {
        "n_clusters": n_clusters_,
        "silhouette_score": silhouette_score,
        "labels": labels.tolist()
    }

if __name__ == "__main__":
    # Read file path from command line
    file_path = sys.argv[1]

    # Attempt to read preference from argv if available, otherwise default to 0
    try:
        preference = float(sys.argv[2]) if len(sys.argv) > 2 else 0
    except ValueError:
        preference = 0

    # Read json from file
    with open(file_path, 'r') as file:
        json_content = file.read()
    
    # Parse the input JSON data
    documents = json.loads(json_content)
    
    # Perform clustering
    result = cluster_texts(documents, preference)
    
    # Output the result as JSON
    print(json.dumps(result))