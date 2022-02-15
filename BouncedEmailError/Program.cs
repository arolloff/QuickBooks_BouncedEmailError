Console.WriteLine($"Enter invoice number to query:");

var input = Console.ReadLine();
if (string.IsNullOrWhiteSpace(input))
    return 1;

using (var httpClient = new HttpClient())
{
    var service = new BouncedEmailError.QuickBooksService(httpClient);
    return await service.GetInvoiceByNumberAsync(input);
}
