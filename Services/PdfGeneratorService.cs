using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ProformaLogytronic.ViewModels;

namespace ProformaLogytronic.Services
{
    public static class PdfGeneratorService
    {
        public static void Generate(MainViewModel vm, string filePath)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily(Fonts.Arial));

                    page.Header().Element(x => ComposeHeader(x, vm));
                    page.Content().Element(x => ComposeContent(x, vm));
                    page.Footer().Element(ComposeFooter);
                });
            });

            document.GeneratePdf(filePath);
        }

        private static void ComposeHeader(IContainer container, MainViewModel vm)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("LOGYTRONIC").FontSize(24).SemiBold().FontColor(Colors.Blue.Darken2);
                    column.Item().Text("Soluciones Tecnológicas").FontSize(12).FontColor(Colors.Grey.Medium);
                });

                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("PROFORMA DE VENTA").FontSize(20).Bold().FontColor(Colors.Black).AlignRight();
                    column.Item().Text($"Fecha: {vm.Proforma.Fecha:dd/MM/yyyy}").AlignRight();
                    if (!string.IsNullOrWhiteSpace(vm.Proforma.NumeroProforma))
                    {
                        column.Item().Text($"Nº: {vm.Proforma.NumeroProforma}").AlignRight();
                    }
                });
            });
        }

        private static void ComposeContent(IContainer container, MainViewModel vm)
        {
            container.PaddingVertical(1, Unit.Centimetre).Column(column =>
            {
                column.Item().Element(x => ComposeClientDetails(x, vm));
                column.Item().PaddingTop(1, Unit.Centimetre).Element(x => ComposeTable(x, vm));
                column.Item().PaddingTop(0.5f, Unit.Centimetre).Element(x => ComposeTotals(x, vm));
            });
        }

        private static void ComposeClientDetails(IContainer container, MainViewModel vm)
        {
            container.Background(Colors.Grey.Lighten4).Padding(10).Column(column =>
            {
                column.Item().PaddingBottom(5).Text("Datos del Cliente").FontSize(14).SemiBold();
                
                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text($"Nombre/Razón Social: {vm.Cliente.Nombre}");
                        col.Item().Text($"NIT/CI: {vm.Cliente.NitCi}");
                    });
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text($"Teléfono: {vm.Cliente.Telefono}");
                        col.Item().Text($"Correo: {vm.Cliente.Correo}");
                    });
                });
                if (!string.IsNullOrWhiteSpace(vm.Cliente.Direccion))
                {
                    column.Item().PaddingTop(2).Text($"Dirección: {vm.Cliente.Direccion}");
                }
            });
        }

        private static void ComposeTable(IContainer container, MainViewModel vm)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(80); // Codigo
                    columns.RelativeColumn();   // Descripcion
                    columns.ConstantColumn(80); // Cantidad
                    columns.ConstantColumn(100); // Precio U.
                    columns.ConstantColumn(100); // Total
                });

                table.Header(header =>
                {
                    header.Cell().Element(CellStyle).Text("Código").SemiBold();
                    header.Cell().Element(CellStyle).Text("Descripción").SemiBold();
                    header.Cell().Element(CellStyle).AlignRight().Text("Cantidad").SemiBold();
                    header.Cell().Element(CellStyle).AlignRight().Text("P. Unitario").SemiBold();
                    header.Cell().Element(CellStyle).AlignRight().Text("Total").SemiBold();

                    static IContainer CellStyle(IContainer container)
                    {
                        return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                    }
                });

                foreach (var item in vm.Productos)
                {
                    table.Cell().Element(CellStyle).Text(item.Codigo ?? "");
                    table.Cell().Element(CellStyle).Text(item.Descripcion ?? "");
                    table.Cell().Element(CellStyle).AlignRight().Text(item.Cantidad.ToString());
                    table.Cell().Element(CellStyle).AlignRight().Text($"{item.PrecioUnitario:N2}");
                    table.Cell().Element(CellStyle).AlignRight().Text($"{item.Total:N2}");

                    static IContainer CellStyle(IContainer container)
                    {
                        return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                    }
                }
            });
        }

        private static void ComposeTotals(IContainer container, MainViewModel vm)
        {
            container.Row(row =>
            {
                row.RelativeItem(); 
                row.ConstantItem(250).Column(column =>
                {
                    column.Item().BorderTop(1).BorderColor(Colors.Black).PaddingTop(5).Row(r =>
                    {
                        r.RelativeItem().Text("Total General:").FontSize(14).SemiBold().AlignRight();
                        r.ConstantItem(100).Text($"{vm.TotalFinal:N2}").FontSize(14).Bold().AlignRight();
                    });
                });
            });
        }

        private static void ComposeFooter(IContainer container)
        {
            container.AlignCenter().Text(x =>
            {
                x.Span("Gracias por su preferencia. Documento sin validez fiscal.").FontSize(10).FontColor(Colors.Grey.Darken1);
            });
        }
    }
}
