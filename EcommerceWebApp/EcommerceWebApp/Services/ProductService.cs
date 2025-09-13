using EcommerceWebApp.Data;
using EcommerceWebApp.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

public class ProductService
{
    private readonly HttpClient _httpClient;
    private readonly ApiSettings _apiSettings;
    private readonly ProductRepository _productRepository;

    public ProductService(HttpClient httpClient, IOptions<ApiSettings> apiSettings, ProductRepository productRepository)
    {
        _httpClient = httpClient;
        _apiSettings = apiSettings.Value;
        _productRepository = productRepository;
    }

    public async Task<List<Product>> GetProductsAsync()
    {
        if (_productRepository.GetProducts().Result.Any())
        {
            return _productRepository.GetProducts().Result.Where(p => p.ProductType.Equals("variable")).ToList();
        }

        var byteArray = Encoding.ASCII.GetBytes($"{_apiSettings.Username}:{_apiSettings.Password}");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Call API
        var response = await _httpClient.GetAsync(_apiSettings.GetProductsUrl);
        response.EnsureSuccessStatusCode();

        var jsonString = await response.Content.ReadAsStringAsync();

        dynamic jsonData = JsonConvert.DeserializeObject(jsonString);

        _productRepository.AddProducts(jsonData);

        // Deserialize to the wrapper object first
        var productResponse = JsonConvert.DeserializeObject<ProductResponse>(jsonString);

        // Return the products list
        return productResponse?.Products ?? new List<Product>();
    }
}
