using System.Text.Json;
using CapitalGains.Application;
using CapitalGains.Domain;

var service = new CapitalGainsService();
var jsonOptions = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.KebabCaseLower,
    WriteIndented = false
};

string? line;
while (!string.IsNullOrEmpty(line = Console.ReadLine()))
{
    try
    {
        var operations = JsonSerializer.Deserialize<List<Operation>>(line, jsonOptions);

        if (operations != null)
        {
            var results = service.ProcessOperations(operations);
            var output = JsonSerializer.Serialize(results, jsonOptions);
            Console.WriteLine(output);
        }
    }
    catch (JsonException ex)
    {
        continue;
    }
}
