﻿using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Gov.CitizenApi.Features.Lookups
{
    public interface ILookupsService
    {
        IEnumerable<Statute> GetStatutes();
        Statute GetCountStatute(string code);
        Task<LookupsAll> GetAllLookUpsAsync();
    }

    public class LookupsService : ILookupsService
    {
        private ILogger<LookupsService> _logger;
        private static Lazy<LookupsAll> _lazyLookups = new Lazy<LookupsAll>(ReadLookupsAll);
        private Lazy<Dictionary<int, Statute>> _lazyStatutes = new Lazy<Dictionary<int, Statute>>(ReadStatutes);

        public LookupsService(ILogger<LookupsService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private static LookupsAll ReadLookupsAll()
        {
            var provider = new ManifestEmbeddedFileProvider(typeof(LookupsService).Assembly);
            var fileInfo = provider.GetFileInfo("Features/Lookups/TempData/LookupData.json");
            if (!fileInfo.Exists)
            {
                throw new Exception("Cannot find lookup file");
            }

            Stream fileStream = fileInfo.CreateReadStream();
            using StreamReader reader = new StreamReader(fileStream);
            var lookups = JsonSerializer.Deserialize<LookupsAll>(reader.ReadToEnd());

            return lookups;
        }

        private static Dictionary<int, Statute> ReadStatutes()
        {
            Dictionary<int, Statute> statutes = _lazyLookups.Value?.Statutes.ToDictionary(m => m.Code);
            return statutes;
        }

        public IEnumerable<Statute> GetStatutes()
        {
            LookupsAll lookups = _lazyLookups.Value;
            return _lazyLookups.Value?.Statutes;
        }

        public Statute GetCountStatute(string countCode)
        {
            Dictionary<int,Statute> statutes = _lazyStatutes.Value;
            int code;

            if(Int32.TryParse(countCode, out code) && statutes != null) 
            {
                Statute statute = statutes[code];
                return statute;
            }
            else
            {
                return null;
            }
        }

        public Task<LookupsAll>  GetAllLookUpsAsync()
        {
            if (_lazyLookups.Value != null)
                return Task.FromResult<LookupsAll>(_lazyLookups.Value);
            else
                return null;
        }
    }
}
