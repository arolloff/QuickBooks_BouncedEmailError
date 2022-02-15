Console.WriteLine($"Enter invoice number to query:");

var input = Console.ReadLine();
if (string.IsNullOrWhiteSpace(input))
    return 1;

var service = new BouncedEmailError.QuickBooksService(new HttpClient());
return await service.GetInvoiceByNumberAsync(input);
