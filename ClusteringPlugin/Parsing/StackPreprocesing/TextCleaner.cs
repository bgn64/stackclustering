using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ClusteringPlugin.StackPreprocessing
{
    internal class TextCleaner : ITextCleaner
    {
        static readonly HashSet<string> StopWords = new HashSet<string>
        {
            "a", "an", "and", "are", "as", "at", "be", "but", "by", "for", "if", "in", "into", "is", "it", "no", "not", "of", "on", "or", "such", "that", "the", "their", "then", "there", "these", "they", "this", "to", "was", "will", "with"
        };

        public string CleanText(string text)
        {
            // Convert to lowercase
            string cleaned = text.ToLower();

            // Remove punctuation
            cleaned = Regex.Replace(cleaned, @"[^\w\s]", "");

            // Trim extra spaces
            cleaned = Regex.Replace(cleaned, @"\s+", " ").Trim();

            // Remove stop words
            cleaned = string.Join(" ", cleaned
                .Split(' ')
                .Where(word => !StopWords.Contains(word)));

            return cleaned;
        }
    }
}
