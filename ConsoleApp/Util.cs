using ConsoleApp.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ConsoleApp
{
    public static class Util
    {
        public static IEnumerable<Moeda> GetMoedas(string caminho)
        {
            using (var fs = new FileStream(caminho, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var streamReader = new StreamReader(fs))
                {
                    string linha = string.Empty;
                    while ((linha = streamReader.ReadLine()) != null)
                    {
                        var valores = linha.Split(";");
                        DateTime.TryParse(valores[1], out DateTime dataReferencia);
                        if (dataReferencia.Date != DateTime.MinValue)
                            yield return new Moeda() { Id = valores[0], DataReferencia = dataReferencia };
                    }
                }
            }
        }
        public static IEnumerable<Moeda> GetCotacoes(string[] dePara, string caminho)
        {
            using (var fs = new FileStream(caminho, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var streamReader = new StreamReader(fs))
                {
                    string linha = string.Empty;
                    while ((linha = streamReader.ReadLine()) != null)
                    {
                        var valores = linha.Split(";");
                        DateTime.TryParse(valores[2], out DateTime dataReferencia);
                        decimal.TryParse(valores[0], out decimal valorCotacao);
                        if (valorCotacao > 0 && dataReferencia != DateTime.MinValue)
                            yield return new Moeda() { Valor = valorCotacao, Id = GetIdMoeda(dePara, valores[1]), DataReferencia = dataReferencia };
                    }
                }
            }
        }

        private static string GetIdMoeda(string[] dePara, string codigoCotacao)
        {
            foreach (var item in dePara)
            {
                var valores = item.Split(";");
                if (valores[1] == codigoCotacao)
                    return valores[0];
            }
            return string.Empty;
        }
    }
}
