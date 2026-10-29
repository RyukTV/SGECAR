using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SistemaGestionEmpresarial.Contracts;

namespace SistemaGestionEmpresarial.Api.Services;

public sealed class ReporteProductosPdfService
{
    private static readonly CultureInfo CulturaPrecio = CultureInfo.GetCultureInfo("en-US");
    private static readonly CultureInfo CulturaFecha = CultureInfo.GetCultureInfo("es-DO");

    public byte[] Generar(ReporteProductosPdfRequest reporte, string nombreUsuario)
    {
        var documento = Document.Create(contenedor =>
        {
            contenedor.Page(pagina =>
            {
                pagina.Size(PageSizes.A4);
                pagina.Margin(30);
                pagina.DefaultTextStyle(estilo => estilo.FontFamily("Lato").FontSize(9).FontColor(Colors.BlueGrey.Darken4));

                pagina.Content().Column(columna =>
                {
                    columna.Spacing(16);
                    columna.Item().Row(fila =>
                    {
                        fila.RelativeItem().Column(encabezado =>
                        {
                            encabezado.Item().Text("SGECAR · INVENTARIO").FontColor(Colors.Blue.Darken2).Bold().FontSize(8);
                            encabezado.Item().Text("Reporte de Productos").Bold().FontSize(18);
                            encabezado.Item().Text("Entidad: Productos");
                        });
                        fila.ConstantItem(150).AlignRight().Column(fecha =>
                        {
                            fecha.Item().Text("Fecha de generación").Bold();
                            fecha.Item().Text(reporte.FechaGeneracion.ToString("dd/MM/yyyy HH:mm", CulturaFecha));
                        });
                    });

                    columna.Item().BorderTop(1).BorderColor(Colors.Grey.Lighten1);
                    columna.Item().Background(Colors.Grey.Lighten4).Padding(10).Column(resumen =>
                    {
                        resumen.Item().Text(texto =>
                        {
                            texto.Span("Criterios: ").Bold();
                            texto.Span(reporte.Criterios);
                        });
                        resumen.Item().Text(texto =>
                        {
                            texto.Span("Registros encontrados: ").Bold();
                            texto.Span(reporte.Productos.Count.ToString(CulturaFecha));
                        });
                        resumen.Item().Text(texto =>
                        {
                            texto.Span("Generado por: ").Bold();
                            texto.Span(nombreUsuario);
                        });
                    });

                    if (reporte.Productos.Count == 0)
                    {
                        columna.Item().Text("No se encontraron productos para los filtros seleccionados.");
                        return;
                    }

                    columna.Item().Table(tabla =>
                    {
                        tabla.ColumnsDefinition(columnas =>
                        {
                            columnas.ConstantColumn(25);
                            columnas.RelativeColumn(1.1f);
                            columnas.RelativeColumn(2f);
                            columnas.RelativeColumn(1.1f);
                            columnas.ConstantColumn(35);
                            columnas.RelativeColumn(1.25f);
                        });

                        tabla.Header(encabezado =>
                        {
                            foreach (var titulo in new[] { "ID", "Código", "Nombre", "Precio", "Stock", "Categoría" })
                                encabezado.Cell().Element(CeldaEncabezado).Text(titulo).Bold();
                        });

                        foreach (var producto in reporte.Productos)
                        {
                            tabla.Cell().Element(Celda).Text(producto.Id.ToString(CulturaFecha));
                            tabla.Cell().Element(Celda).Text(producto.Codigo);
                            tabla.Cell().Element(Celda).Text(producto.Nombre);
                            tabla.Cell().Element(Celda).Text($"RD$ {producto.Precio.ToString("N2", CulturaPrecio)}");
                            tabla.Cell().Element(Celda).Text(producto.Stock.ToString(CulturaFecha));
                            tabla.Cell().Element(Celda).Text(producto.Categoria);
                        }
                    });
                });
            });
        });

        return documento.GeneratePdf();
    }

    private static IContainer CeldaEncabezado(IContainer celda) =>
        celda.Background(Colors.BlueGrey.Darken3).PaddingVertical(7).PaddingHorizontal(4)
            .DefaultTextStyle(estilo => estilo.FontColor(Colors.White).FontSize(8));

    private static IContainer Celda(IContainer celda) =>
        celda.BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2)
            .PaddingVertical(6).PaddingHorizontal(4).DefaultTextStyle(estilo => estilo.FontSize(8));
}
