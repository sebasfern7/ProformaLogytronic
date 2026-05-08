using System;
using System.IO;
using System.Linq;
using ProformaLogytronic.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Previewer;

namespace ProformaLogytronic.Services
{
    public class PdfService : IPdfService
    {
        public PdfService()
        {
            // QuestPDF requiere configurar el modo de licencia
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public string Generar(Proforma proforma, string directorioBase)
        {
            // Estructura de carpetas: /Proformas/AAAA/MM/
            string year = proforma.FechaCreacion.Year.ToString();
            string month = proforma.FechaCreacion.Month.ToString("D2");
            string relativeFolder = Path.Combine("Proformas", year, month);
            string targetDir = Path.Combine(directorioBase, relativeFolder);

            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            string safeName = string.Join("_", proforma.ClienteNombre.Split(Path.GetInvalidFileNameChars())).Replace(" ", "_");
            string fileName = $"Proforma_{proforma.NumeroSecuencia:D6}_{safeName}.pdf";
            string fullPath = Path.Combine(targetDir, fileName);

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Segoe UI"));

                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("LOGYTRONIC").FontSize(24).SemiBold().FontColor("#005A9E");
                            col.Item().Text("Soluciones Tecnológicas").FontSize(12).Italic();
                        });

                        row.RelativeItem().Column(col =>
                        {
                            col.Item().AlignRight().Text("PROFORMA DE VENTA").FontSize(18).Bold();
                            col.Item().AlignRight().Text($"Nº {proforma.NumeroSecuencia:D6}").FontSize(14).SemiBold();
                            col.Item().AlignRight().Text($"Fecha: {proforma.FechaCreacion:dd/MM/yyyy}");
                        });
                    });

                    page.Content().PaddingVertical(20).Column(col =>
                    {
                        // Datos del Cliente
                        col.Item().BorderBottom(1).PaddingBottom(5).Text("DATOS DEL CLIENTE").SemiBold();
                        col.Item().PaddingTop(5).Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text(t => { t.Span("Señor(es): ").SemiBold(); t.Span(proforma.ClienteNombre); });
                                c.Item().Text(t => { t.Span("NIT/CI: ").SemiBold(); t.Span(proforma.ClienteDocumento); });
                            });
                        });

                        col.Item().PaddingTop(20);

                        // Tabla de Productos
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(80);  // Código
                                columns.RelativeColumn();    // Descripción
                                columns.ConstantColumn(60);  // Cantidad
                                columns.ConstantColumn(100); // P. Unitario
                                columns.ConstantColumn(100); // Total
                            });

                            // Encabezado Tabla
                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Código");
                                header.Cell().Element(CellStyle).Text("Descripción");
                                header.Cell().Element(CellStyle).AlignCenter().Text("Cant.");
                                header.Cell().Element(CellStyle).AlignRight().Text("P. Unitario");
                                header.Cell().Element(CellStyle).AlignRight().Text("Total");

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                                }
                            });

                            // Filas
                            foreach (var item in proforma.Items)
                            {
                                table.Cell().Element(RowStyle).Text(item.CodigoProducto);
                                table.Cell().Element(RowStyle).Text(item.Descripcion);
                                table.Cell().Element(RowStyle).AlignCenter().Text(item.Cantidad.ToString());
                                table.Cell().Element(RowStyle).AlignRight().Text(item.PrecioUnitario.ToString("N2"));
                                table.Cell().Element(RowStyle).AlignRight().Text(item.Subtotal.ToString("N2"));

                                static IContainer RowStyle(IContainer container)
                                {
                                    return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                                }
                            }
                        });

                        col.Item().PaddingTop(15).AlignRight().Width(250).Column(c =>
                        {
                            c.Item().Row(row =>
                            {
                                row.RelativeItem().Text("SUBTOTAL:").AlignRight();
                                row.RelativeItem().Text(proforma.Subtotal.ToString("N2")).AlignRight();
                            });
                            
                            c.Item().PaddingTop(5).Background("#F0F0F0").Padding(5).Row(row =>
                            {
                                row.RelativeItem().Text("TOTAL:").AlignRight().FontSize(14).Bold();
                                row.RelativeItem().Text(proforma.Total.ToString("N2")).AlignRight().FontSize(14).Bold();
                            });
                        });
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Página ");
                        x.CurrentPageNumber();
                    });
                });
            }).GeneratePdf(fullPath);

            return fullPath;
        }
    }
}
