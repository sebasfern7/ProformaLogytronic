namespace ProformaLogytronic.Models
{
    public class AppSettings
    {
        /// <summary>Ruta absoluta donde se crean las carpetas Proformas/Año/NombreMes. Vacío = carpeta de la aplicación.</summary>
        public string? PdfOutputDirectory { get; set; }
    }
}
