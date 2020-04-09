using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.ElasticSearch
{
    public static class ElasticSearchExtensions
    {
        //https://www.elastic.co/guide/en/elasticsearch/client/net-api/6.x/nest-getting-started.html
        //https://miroslavpopovic.com/posts/2018/07/elasticsearch-with-aspnet-core-and-docker
        //Index == Table
        //The code will create an index per documentType

        [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
        public sealed class IndexAttribute : Attribute
        {
            public string Name { get; }
            public IndexAttribute(string name)
            {
                if(string.IsNullOrWhiteSpace(name))
                {
                    throw new Exception("Index name cannot be empty");
                }

                Name = name;
            }
        }

        private static string GetIndex<TDocument>(string indexSuffix = null)
        {
            var type = typeof(TDocument);
            var indexAttribute = type.GetCustomAttributes(typeof(IndexAttribute), false).Select(a => (IndexAttribute)a).FirstOrDefault();
            var index = (indexAttribute != null ? indexAttribute.Name : typeof(TDocument).Name.ToLower()) + (string.IsNullOrWhiteSpace(indexSuffix) ? "" : $"-{indexSuffix}");
            return index;
        }

        public static Task<IndexResponse> IndexAsync<TDocument>(
            this IElasticClient client, 
            TDocument document, 
            CancellationToken cancellationToken = default(CancellationToken), 
            string indexSuffix = null)
            where TDocument : class
        {
           var index = GetIndex<TDocument>(indexSuffix);
           return client.IndexAsync(
               document, 
               s => s.Index(index), 
               cancellationToken);
        }

        public static Task<UpdateResponse<TDocument>> UpsertAsync<TDocument>(
            this IElasticClient client, 
            TDocument document, 
            CancellationToken cancellationToken = default(CancellationToken), 
            string indexSuffix = null)
           where TDocument : class
        {
            var index = GetIndex<TDocument>(indexSuffix);
            return client.UpdateAsync<TDocument>(
                document, 
                u => u.Index(index).Doc(document).DocAsUpsert(true), 
                cancellationToken);
        }

        public static Task<ISearchResponse<TDocument>> SearchAsync<TDocument>(
            this IElasticClient client, 
            Func<SearchDescriptor<TDocument>, SearchDescriptor<TDocument>> selector, 
            CancellationToken cancellationToken = default(CancellationToken), 
            string indexSuffix = null)
           where TDocument : class
        {
            var index = GetIndex<TDocument>(indexSuffix);
            return client.SearchAsync<TDocument>(
                s => selector(s.Index(index)), 
                cancellationToken);
        }

        public static Task<DeleteByQueryResponse> DeleteByQueryAsync<TDocument>(
            this IElasticClient client, 
            Func<DeleteByQueryDescriptor<TDocument>, DeleteByQueryDescriptor<TDocument>> selector, 
            CancellationToken cancellationToken = default(CancellationToken), 
            string indexSuffix = null)
           where TDocument : class
        {
            var index = GetIndex<TDocument>(indexSuffix);
            return client.DeleteByQueryAsync<TDocument>(
                s => selector(s.Index(index)), 
                cancellationToken);
        }

        public static Task<DeleteIndexResponse> DeleteIndexAsync<TDocument>(
            this IElasticClient client, CancellationToken cancellationToken = default(CancellationToken), 
            string indexSuffix = null)
            where TDocument : class
        {
            var index = GetIndex<TDocument>(indexSuffix);
            return client.DeleteIndexAsync(
                index,  
                cancellationToken);
        }

        public static Task<DeleteIndexResponse> DeleteIndexAsync(
            this IElasticClient client, 
            string index, 
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return client.DeleteIndexAsync(
                index,
                cancellationToken
                );
        }

        //https://rimdev.io/bulk-import-documents-into-elasticsearch-using-nest/
        public static void BulkAll<TDocument>(
            this IElasticClient client, 
            IEnumerable<TDocument> documents, 
            CancellationToken cancellationToken = default(CancellationToken), 
            string indexSuffix = null)
             where TDocument : class
        {
            var index = GetIndex<TDocument>(indexSuffix);
            var waitHandle = new CountdownEvent(1);

            var bulkAll = client.BulkAll(documents, b => b
                .Index(index)
                .BackOffRetries(2)
                .BackOffTime("30s")
                .RefreshOnCompleted(true)
                .MaxDegreeOfParallelism(4)
                .Size(1000),
                cancellationToken
            );

            bulkAll.Subscribe(new BulkAllObserver(
                onNext: (b) => { Console.Write("."); },
                onError: (e) => { throw e; },
                onCompleted: () => waitHandle.Signal()
            ));

            waitHandle.Wait();
        }
    }
}
