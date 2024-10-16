using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Lucene.Net.Search;
using Lucene.Net.Search.Similarities;
using System.Reflection;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

namespace RealEstateApplication.Services.Helpers
{
    public class LuceneEngine<T> : ILuceneEngine<T> where T : class, new()
    {
        public LuceneEngine(string indexPath, ILogger<LuceneEngine<T>> logger)
        {
            _indexPath = indexPath ?? throw new ArgumentNullException(nameof(indexPath));
            _logger = logger;
        }

        private IndexWriter CreateIndexWriter()
        {
            var indexDirInfo = new DirectoryInfo(_indexPath);

            // Ensure the directory exists
            if (!indexDirInfo.Exists)
            {
                indexDirInfo.Create();  // Create the directory if it doesn't exist
            }

            var indexDir = FSDirectory.Open(indexDirInfo);
            var analyzer = new StandardAnalyzer(LUCENE_VERSION);
            var indexConfig = new IndexWriterConfig(LUCENE_VERSION, analyzer);
            return new IndexWriter(indexDir, indexConfig);
        }

        public T GetIndex(int id, string idField)
        {
            var indexDirInfo = new DirectoryInfo(_indexPath);

            // Ensure the directory exists
            if (!indexDirInfo.Exists)
            {
                indexDirInfo.Create();  // Create the directory if it doesn't exist
            }

            using var indexDir = FSDirectory.Open(indexDirInfo);
            using var reader = DirectoryReader.Open(indexDir);
            var searcher = new IndexSearcher(reader);
            var term = new Term(idField, id.ToString());
            var query = new TermQuery(term);
            var topDocs = searcher.Search(query, 1);

            if (topDocs.TotalHits > 0)
            {
                var doc = searcher.Doc(topDocs.ScoreDocs[0].Doc);
                return DocumentToEntity(doc);
            }
            return null;
        }

        public void AddIndex(T entity)
        {
            using var writer = CreateIndexWriter();
            var doc = EntityToDocument(entity);
            writer.AddDocument(doc);
            writer.Commit();
        }

        public void UpdateIndex(T entity, string idField)
        {
            var indexDirInfo = new DirectoryInfo(_indexPath);

            // Ensure the directory exists
            if (!indexDirInfo.Exists)
            {
                indexDirInfo.Create();  // Create the directory if it doesn't exist
            }

            using var writer = CreateIndexWriter();
            var doc = EntityToDocument(entity);
            var idValue = GetFieldValue(entity, idField)?.ToString();

            if (idValue == null)
                throw new ArgumentException($"Field '{idField}' cannot be null.", nameof(idField));

            var term = new Term(idField, idValue);
            writer.UpdateDocument(term, doc);
            writer.Commit();
        }

        public void DeleteIndex(int id, string idField)
        {
            var indexDirInfo = new DirectoryInfo(_indexPath);

            // Ensure the directory exists
            if (!indexDirInfo.Exists)
            {
                indexDirInfo.Create();  // Create the directory if it doesn't exist
            }

            using var writer = CreateIndexWriter();
            var term = new Term(idField, id.ToString());
            writer.DeleteDocuments(term);
            writer.Commit();
        }

        public List<T> SearchWithSimilarity(string searchTerm, string fieldName, int pageNumber, int pageSize, double minimumSimilarity = 0.5)
        {
            try
            {
                var indexDirInfo = new DirectoryInfo(_indexPath);

                // Ensure the directory exists
                if (!indexDirInfo.Exists)
                {
                    indexDirInfo.Create();  // Create the directory if it doesn't exist
                }

                // Get the properties of the generic type T
                var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

                // Check if the fieldName exists in the properties of T
                var fieldExists = props.Any(p => p.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase));

                if (!fieldExists)
                {
                    _logger.LogError($"Field '{fieldName}' does not exist in type '{typeof(T).Name}'.");
                    throw new ArgumentException($"Field '{fieldName}' does not exist in type '{typeof(T).Name}'.");
                }

                var analyzer = new StandardAnalyzer(LUCENE_VERSION);
                using var indexDir = FSDirectory.Open(new DirectoryInfo(_indexPath));

                if (!DirectoryReader.IndexExists(indexDir))
                {
                    _logger.LogError("No Index Found.");
                    return new List<T>();
                }

                using var reader = DirectoryReader.Open(indexDir);
                var searcher = new IndexSearcher(reader);
                searcher.Similarity = new BM25Similarity();

                var booleanQuery = new BooleanQuery();

                var fuzzyTerm = new Term(fieldName, searchTerm);
                booleanQuery.Add(new FuzzyQuery(fuzzyTerm, 2), Occur.SHOULD);

                var prefixTerm = new Term(fieldName, searchTerm);
                booleanQuery.Add(new PrefixQuery(prefixTerm), Occur.SHOULD);

                var wildcardQuery = new WildcardQuery(new Term(fieldName, searchTerm + "*"));
                booleanQuery.Add(wildcardQuery, Occur.SHOULD);

                var topDocs = searcher.Search(booleanQuery, pageNumber * pageSize);
                var results = new List<T>();

                foreach (var scoreDoc in topDocs.ScoreDocs)
                {
                    if (scoreDoc.Score >= minimumSimilarity)
                    {
                        var doc = searcher.Doc(scoreDoc.Doc);
                        var entity = DocumentToEntity(doc);
                        results.Add(entity);
                    }
                }

                return results;
            }
            catch (Exception ex)
            {
                throw new Exception("Internal server error.", ex);
            }
        }

        private Document EntityToDocument(T entity)
        {
            var doc = new Document();
            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                var value = prop.GetValue(entity);

                if (value != null)
                {
                    if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        // Serialize the list to a JSON string
                        var serializedValue = JsonConvert.SerializeObject(value);
                        doc.Add(new TextField(prop.Name, serializedValue, Field.Store.YES));
                    }
                    else
                    {
                        // Handle regular properties
                        doc.Add(new StringField(prop.Name, value.ToString(), Field.Store.YES));
                    }
                }
            }

            return doc;
        }

        private T DocumentToEntity(Document doc)
        {
            var entity = new T();
            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                var value = doc.Get(prop.Name);
                if (value != null && prop.CanWrite)
                {
                    try
                    {
                        // Check if the property type is a collection (List<T>)
                        if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                        {
                            // Get the type of the elements in the list
                            var itemType = prop.PropertyType.GetGenericArguments()[0];

                            // Deserialize the string to a List of the specified item type
                            var list = JsonConvert.DeserializeObject(value, typeof(List<>).MakeGenericType(itemType));
                            prop.SetValue(entity, list);
                        }
                        else if (prop.PropertyType.IsEnum)
                        {
                            // Handle enum properties
                            var enumValue = Enum.Parse(prop.PropertyType, value.ToString());
                            prop.SetValue(entity, enumValue);
                        }
                        else
                        {
                            // For non-collection and non-enum types, use Convert.ChangeType
                            var convertedValue = Convert.ChangeType(value, prop.PropertyType);
                            prop.SetValue(entity, convertedValue);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Handle any conversion exceptions, if needed
                        throw new InvalidOperationException($"Failed to convert property '{prop.Name}' with value '{value}' to type '{prop.PropertyType.Name}'.", ex);
                    }
                }
            }

            return entity;
        }

        private object GetFieldValue(T entity, string fieldName)
        {
            var prop = typeof(T).GetProperty(fieldName);
            return prop?.GetValue(entity);
        }


        private const LuceneVersion LUCENE_VERSION = LuceneVersion.LUCENE_48;
        private readonly string _indexPath;
        public ILogger<LuceneEngine<T>> _logger { get; }
    }

    public interface ILuceneEngine<T> where T : class, new()
    {
        T GetIndex(int id, string idField);
        void AddIndex(T entity);
        void UpdateIndex(T entity, string idField);
        void DeleteIndex(int id, string idField);
        List<T> SearchWithSimilarity(string searchTerm, string fieldName, int pageNumber, int pageSize, double minimumSimilarity = 0.5);
    }
}

