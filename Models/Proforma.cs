using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ProformaLogytronic.Models
{
    public class Proforma
    {
        public int Id { get; set; }
        public int NumeroSecuencia { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public string ClienteNombre { get; set; } = string.Empty;
        public string ClienteDocumento { get; set; } = string.Empty;
        public string ClienteTelefono { get; set; } = string.Empty;
        public string ClienteCorreo { get; set; } = string.Empty;
        public string ClienteDireccion { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
        public decimal Total { get; set; }
        public string RutaPdf { get; set; } = string.Empty;
        public string Estado { get; set; } = "Generada";
        public List<ProformaItem> Items { get; set; } = new();
    }
}
