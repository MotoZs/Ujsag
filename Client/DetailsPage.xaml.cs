using Common;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Client;

public partial class DetailsPage : ContentPage, IQueryAttributable
{
    private readonly HttpClient _httpClient;
    public string BACKEND_URL = "https://localhost:7072";
    private readonly IHttpClientFactory httpClientFactory;
    private ArticleDto article;
    private int id;

    public DetailsPage(IHttpClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;
        InitializeComponent();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        query.TryGetValue("Id", out var idObject);
        id = (int)(idObject ?? 0);
    }

    protected override async void OnAppearing()
    {

        await LoadTokenAsync(_httpClient);

        base.OnAppearing();

        await LoadDataAsync();
    }

    private async Task LoadTokenAsync(HttpClient client)
    {
        try
        {
            string? token = await SecureStorage.GetAsync("auth_token");
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }
        catch
        {
            await DisplayAlert("Error", "Token load failed.", "OK");
        }
    }


    private async ValueTask LoadDataAsync()
    {
        if (id != 0)
        {
            var httpClient = httpClientFactory.CreateClient();
            article = await httpClient.GetFromJsonAsync<ArticleDto>($"{BACKEND_URL}/get/{id}");
        }

        ArticleTitle.Text = article?.Title;
        ArticleAuthorId.Text = article?.Author?.Id.ToString() ?? "add meg az Id-t";
        ArticleCreateDate.Text = article?.CreatedDate.ToString("yyyy-MM-dd") ?? "1999-09-09";
        ArticleContent.Text = article?.Description ?? "description";
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}