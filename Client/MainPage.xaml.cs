using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Json;

namespace Client;

// ArticleDto defined in MainPage for UI use
public class ArticleDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Expired { get; set; }
}

public partial class MainPage : ContentPage
{
    private readonly IHttpClientFactory httpClientFactory;
    private ObservableCollection<ArticleDto> articleCollection = new ObservableCollection<ArticleDto>();
    private ObservableCollection<ArticleDto> publicArticleCollection = new ObservableCollection<ArticleDto>();

    public MainPage(IHttpClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;

        InitializeComponent();

        ArticlesView.ItemsSource = articleCollection;
        PublicArticlesView.ItemsSource = publicArticleCollection;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        var httpClient = httpClientFactory.CreateClient();
        var articles = await httpClient.GetFromJsonAsync<List<ArticleDto>>("https://localhost:7241/list");

        articleCollection.Clear();
        publicArticleCollection.Clear();

        foreach (var article in articles)
        {
            articleCollection.Add(article);
            publicArticleCollection.Add(article);
        }
    }

    private async void OnLoginClickedAsync(object? sender, EventArgs e)
    {
        // Simplified admin check - teammates will implement real auth later
        if (UsernameEntry.Text == "admin" && PasswordEntry.Text == "password")
        {
            AdminPanel.IsVisible = true;
            LoginPanel.IsVisible = false;
            MessageLabel.Text = string.Empty;
        }
        else
        {
            MessageLabel.Text = "Invalid credentials";
        }
    }

    private void OnLogoutClickedAsync(object? sender, EventArgs e)
    {
        AdminPanel.IsVisible = false;
        LoginPanel.IsVisible = true;
    }

    private async void OnAddNewClickedAsync(object? sender, EventArgs e)
    {
        // Navigate to article page to create a new Article (admins only)
        await Shell.Current.GoToAsync("article");
    }

    private async void OnDeleteClickedAsync(object? sender, EventArgs e)
    {
        var article = (ArticleDto)((Button)sender).BindingContext;
        var httpClient = httpClientFactory.CreateClient();
        var response = await httpClient.DeleteAsync($"https://localhost:7241/delete/{article.Id}");

        await LoadDataAsync();
    }

    private async void OnEditClickedAsync(object? sender, EventArgs e)
    {
        var article = (ArticleDto)((Button)sender).BindingContext;

        // Navigate to article page for editing Article
        await Shell.Current.GoToAsync("article", new ShellNavigationQueryParameters { { "Id", article.Id } });
    }
}