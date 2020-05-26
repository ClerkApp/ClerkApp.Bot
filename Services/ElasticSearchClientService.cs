using System;
using System.Diagnostics;
using System.Text;
using ClerkBot.Config;
using Elasticsearch.Net;
using Microsoft.Extensions.Options;
using Nest;
using Nest.JsonNetSerializer;

namespace ClerkBot.Services
{
    public interface IElasticSearchClientService
    {
        ElasticClient GetClient();
    }

    public class ElasticSearchClientService : IElasticSearchClientService
    {
        private static ElasticConfig _elasticConfig;
        private readonly ElasticClient _client;

        public ElasticSearchClientService(IOptions<ElasticConfig> elasticConfig)
        {
            _elasticConfig = elasticConfig.Value;
            _client = InitClient();
        }

        private static ElasticClient InitClient()
        {
            var connectionPool = new SingleNodeConnectionPool(new Uri(_elasticConfig.Host));
            var settings = new ConnectionSettings(connectionPool, JsonNetSerializer.Default)
                .DefaultFieldNameInferrer(p => p)
                .DisableDirectStreaming()
                .OnRequestCompleted(call =>
                {
                    Debug.Write(Encoding.UTF8.GetString(call.RequestBodyInBytes));
                });
            if (_elasticConfig.UseAuthentication)
            {
                settings.BasicAuthentication(_elasticConfig.Username, _elasticConfig.Password);
            }
            return new ElasticClient(settings);
        }

        public ElasticClient GetClient()
        {
            return _client;
        }
    }
}
