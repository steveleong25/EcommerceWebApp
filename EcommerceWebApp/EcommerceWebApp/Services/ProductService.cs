using EcommerceWebApp.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class ProductService
{
    private readonly HttpClient _httpClient;
    private readonly ApiSettings _apiSettings;

    public ProductService(HttpClient httpClient, IOptions<ApiSettings> apiSettings)
    {
        _httpClient = httpClient;
        _apiSettings = apiSettings.Value;
    }

    public async Task<List<Product>> GetProductsAsync()
    {
        // Add Basic Auth header
        var byteArray = Encoding.ASCII.GetBytes($"{_apiSettings.Username}:{_apiSettings.Password}");
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

        // Call API
        var response = await _httpClient.GetAsync(_apiSettings.GetProductsUrl);
        response.EnsureSuccessStatusCode();

        var jsonString = await response.Content.ReadAsStringAsync();

        // Deserialize to the wrapper object first
        var productResponse = JsonConvert.DeserializeObject<ProductResponse>(jsonString);

        // Return the products list
        return productResponse?.Products ?? new List<Product>();
    }
}
