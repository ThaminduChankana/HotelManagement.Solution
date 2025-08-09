using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using HotelTool.Web.Pages.Reports;

public class DailyRequestsDocument : IDocument
{
    private readonly List<DailySpecialRequestsModel.RequestItem> _requests;
    private readonly DateTime _selectedDate;

    public DailyRequestsDocument(List<DailySpecialRequestsModel.RequestItem> requests, DateTime selectedDate)
    {
        _requests = requests;
        _selectedDate = selectedDate;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Margin(30);
            page.Size(PageSizes.A4);
            page.DefaultTextStyle(x => x.FontSize(10));
            page.Header()
                .Text($"Daily Special Requests - {_selectedDate:MMMM dd, yyyy}")
                .FontSize(16).Bold().AlignCenter();

            page.Content().Table(table =>
            {
                // Columns
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2); // Guest Name
                    columns.RelativeColumn(2); // Phone
                    columns.RelativeColumn(2); // Room Type
                    columns.RelativeColumn(1); // Room No
                    columns.RelativeColumn(3); // Special Request
                });

                // Header row
                table.Header(header =>
                {
                    header.Cell().Element(CellStyle).Text("Guest Name").Bold();
                    header.Cell().Element(CellStyle).Text("Phone").Bold();
                    header.Cell().Element(CellStyle).Text("Room Type").Bold();
                    header.Cell().Element(CellStyle).Text("Room #").Bold();
                    header.Cell().Element(CellStyle).Text("Special Request").Bold();

                    static IContainer CellStyle(IContainer container) => container
                        .PaddingVertical(5)
                        .PaddingHorizontal(5)
                        .Background(Colors.Grey.Lighten2)
                        .BorderBottom(1)
                        .ShowOnce();
                });

                // Data rows
                foreach (var item in _requests)
                {
                    table.Cell().Element(CellStyle).Text(item.GuestName);
                    table.Cell().Element(CellStyle).Text(item.Phone);
                    table.Cell().Element(CellStyle).Text(item.RoomType);
                    table.Cell().Element(CellStyle).Text(string.IsNullOrEmpty(item.AllocatedRoomNumber) ? "Not Assigned" : item.AllocatedRoomNumber);
                    table.Cell().Element(CellStyle).Text(item.SpecialRequest);

                    static IContainer CellStyle(IContainer container) => container
                        .PaddingVertical(5)
                        .PaddingHorizontal(5)
                        .BorderBottom(1);
                }
            });

            page.Footer().AlignCenter().Text(text =>
            {
                text.Span("Generated on: ").SemiBold();
                text.Span($"{DateTime.Now:yyyy-MM-dd HH:mm}");
            });
        });
    }
}
