using Common;
using System.Net.Http.Json;

namespace Client;

public partial class DetailsPage : ContentPage, IQueryAttributable
{
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
        if (id == 0)
        {
            article = new ArticleDto { Title = "New Article", Description = "", AuthorId = 0, Author = AuthorDto, CreatedDate = DateTime };
        }
        else
        {
            var httpClient = httpClientFactory.CreateClient();
            article = await httpClient.GetFromJsonAsync<ArticleDto>($"https://localhost:7241/get/{id}");
        }

        ArticleTitle.Text = article.Title;
        ArticleAuthor.Text = article.Author;
        ArticlePublished.Text = article.CreatedDate.ToString("yyyy-MM-dd");
        ArticleContent.Text = article.Description;
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}