using ConsoleApp.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp.Api
{
    public interface IApiClient
    {
        Task<List<Item>> GetItemFila();
    }
}
