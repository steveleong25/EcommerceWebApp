using EcommerceWebApp.Data;
using EcommerceWebApp.Models;
using Microsoft.AspNetCore.WebUtilities;
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
        var byteArray = Encoding.ASCII.GetBytes($"{_apiSettings.Username}:{_apiSettings.Password}");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var productsApiUrl = QueryHelpers.AddQueryString(_apiSettings.GetProductsUrl, "page", "1");

        // Call API HEAD to get headers
        var headResponse = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, _apiSettings.GetProductsUrl));
        headResponse.EnsureSuccessStatusCode();
        headResponse.Headers.TryGetValues("X-WC-Total", out var totalProducts);
        headResponse.Headers.TryGetValues("X-WC-TotalPages", out var totalPages);

        // Check if database has the same records as API data
        var products = await _productRepository.GetProducts();
        if (products.Count == int.Parse(totalProducts.First()))
        {
            return products.Where(p => p.ProductType.Equals("variable")).ToList();
        }

        // Else, send a GET to get products
        var getResponse = await _httpClient.GetAsync(productsApiUrl);
        getResponse.EnsureSuccessStatusCode();

        var jsonString = await getResponse.Content.ReadAsStringAsync();

        dynamic jsonData = JsonConvert.DeserializeObject(jsonString);

        _productRepository.AddProducts(jsonData);

        // Deserialize to the wrapper object first
        var productResponse = JsonConvert.DeserializeObject<ProductResponse>(jsonString);

        var totalNumberOfPages = int.Parse(totalPages.First());

        for (int i = 2; i <= totalNumberOfPages; i++)
        {
            productsApiUrl = QueryHelpers.AddQueryString(_apiSettings.GetProductsUrl, "page", (i).ToString());

            getResponse = await _httpClient.GetAsync(productsApiUrl);
            getResponse.EnsureSuccessStatusCode();

            jsonString = await getResponse.Content.ReadAsStringAsync();
            jsonData = JsonConvert.DeserializeObject(jsonString);

            _productRepository.AddProducts(jsonData);

            var nextPageProducts = JsonConvert.DeserializeObject<ProductResponse>(jsonString);
            productResponse.Products.AddRange(nextPageProducts.Products);
        }

        // Return the products list
        return productResponse?.Products ?? new List<Product>();
    }
}
