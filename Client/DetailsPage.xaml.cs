using Common;
using System.Net.Http.Json;

namespace Client;

public partial class DetailsPage : ContentPage, IQueryAttributable
{
    public string BACKEND_URL = "https://localhost:7072/api";
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
        base.OnAppearing();

        await LoadDataAsync();
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