using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using ProformaLogytronic.Models;

namespace ProformaLogytronic.Repositories
{
    public class JsonProformaRepository : IProformaRepository
    {
        private readonly string _filePath;

        public JsonProformaRepository()
        {
            string dataDir = Path.Combine(AppContext.BaseDirectory, "Data");
            if (!Directory.Exists(dataDir))
            {
                Directory.CreateDirectory(dataDir);
            }
            _filePath = Path.Combine(dataDir, "proformas.json");
        }

        public IEnumerable<Proforma> ObtenerTodas()
        {
            if (!File.Exists(_filePath)) return Enumerable.Empty<Proforma>();

            try
            {
                string json = File.ReadAllText(_filePath);
                if (string.IsNullOrWhiteSpace(json)) return Enumerable.Empty<Proforma>();
                
                return JsonSerializer.Deserialize<List<Proforma>>(json) ?? new List<Proforma>();
            }
            catch
            {
                return Enumerable.Empty<Proforma>();
            }
        }

        public void Guardar(Proforma proforma)
        {
            var proformas = ObtenerTodas().ToList();
            
            // Reemplazar si ya existe el mismo número de secuencia para evitar duplicados en re-intentos
            var existente = proformas.FirstOrDefault(p => p.NumeroSecuencia == proforma.NumeroSecuencia);
            if (existente != null)
            {
                proformas.Remove(existente);
            }

            proformas.Add(proforma);

            string json = JsonSerializer.Serialize(proformas, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            
            File.WriteAllText(_filePath, json);
        }

        public int GenerarSiguienteNumero()
        {
            var proformas = ObtenerTodas();
            if (!proformas.Any()) return 1;
            
            return proformas.Max(p => p.NumeroSecuencia) + 1;
        }
    }
}
