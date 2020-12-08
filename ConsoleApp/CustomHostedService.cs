using ConsoleApp.Api;
using ConsoleApp.Config;
using ConsoleApp.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp
{
    public class CustomHostedService : IHostedService, IDisposable
    {
        private readonly CustomHostedServiceOptions _options;
        private readonly ILogger _logger;
        private Timer _timer;
        private readonly IApiClient _apiClient;

        public CustomHostedService(ILogger<CustomHostedService> logger, IOptions<CustomHostedServiceOptions> options, IApiClient apiClient)
        {
            _logger = logger;
            _options = options.Value;
            _apiClient = apiClient;
        }

        private async void Processar(object state)
        {
            _logger.LogInformation($"Inicio de processamento");
            var watch = System.Diagnostics.Stopwatch.StartNew();

            List<Item> items = null;
            try
            {
                items = await _apiClient.GetItemFila();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao consultar a API.");
            }

            if (items != null && items.Count > 0)
            {
                _logger.LogInformation($"{items.Count} item a serem processados.");
                var dePara = File.ReadAllLines(_options.DePara);
                var cotacoes = Util.GetCotacoes(dePara, _options.DadosCotacao).ToList();
                var moedas = Util.GetMoedas(_options.DadosMoeda).ToList();

                List<string> valores = new List<string>();
                valores.Add(_options.Colunas);
                foreach (Item item in items)
                {
                    var moedasDoItem = moedas.Where(m => m.DataReferencia >= item.data_inicio && m.DataReferencia <= item.data_fim).ToList();
                    valores.AddRange((from x in moedasDoItem
                               join y in cotacoes on (id: x.Id, dataReferencia: x.DataReferencia)
                               equals (id: y.Id, dataReferencia: y.DataReferencia)
                               select string.Join(";",x.Id, x.DataReferencia.ToShortDateString(), y.Valor)).ToList());
                }
                if (valores != null && valores.Count > 0)
                {
                    if (!Directory.Exists(_options.Saida))
                        Directory.CreateDirectory(_options.Saida);
                    string caminhoArquivo = $"{_options.Saida}\\Resultado_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.csv";
                    File.WriteAllLines(caminhoArquivo, valores);
                    _logger.LogInformation($"Arquivo gerado: {Directory.GetCurrentDirectory()}{caminhoArquivo}");
                }
            }
            else
                _logger.LogInformation($"Não existem items a serem processados.");

            watch.Stop();
            _logger.LogInformation($"Fim de processamento ===========================> Tempo decorrido: {watch.ElapsedMilliseconds}ms");
            _timer?.Change(_options.IntervaloDeExecucao * 1000, 0);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("O serviço está sendo iniciado.");
            _timer = new Timer(Processar, null, 0, -1);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("O serviço está sendo parado.");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
