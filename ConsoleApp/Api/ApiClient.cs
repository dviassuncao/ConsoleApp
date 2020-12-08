using ConsoleApp.Config;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using ConsoleApp.Models;

namespace ConsoleApp.Api
{
    class ApiClient : IApiClient
    {
        private readonly CustomHostedServiceOptions _options;
        private readonly HttpClient _client;
        public ApiClient(HttpClient httpClient, IOptions<CustomHostedServiceOptions> options)
        {
            _options = options.Value;
            _client = httpClient;
            _client.BaseAddress = new Uri(_options.EnderecoApi);
        }

        public async Task<List<Item>> GetItemFila()
        {
            string resultado = await _client.GetStringAsync("Items");
            var items = !string.IsNullOrEmpty(resultado) ? JsonSerializer.Deserialize<List<Item>>(resultado) : null;
            return items;
        }
    }
}
