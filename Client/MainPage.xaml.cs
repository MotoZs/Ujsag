using Common;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using Microsoft.Maui.Storage;

namespace Client;


public partial class MainPage : ContentPage
{
    public string BACKEND_URL = "https://localhost:7072/api";

    private readonly IHttpClientFactory httpClientFactory;
    private ObservableCollection<ArticleDto> articleCollection = new ObservableCollection<ArticleDto>();
    private ObservableCollection<ArticleDto> publicArticleCollection = new ObservableCollection<ArticleDto>();

    // local id generation for client-only articles
    private int _nextLocalId = -1;
    private int _editingId = 0; // 0 = creating new

    // persisted local articles
    private readonly List<ArticleDto> _localAdded = new();
    private readonly string _localFileName;

    public MainPage(IHttpClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;

        _localFileName = Path.Combine(FileSystem.AppDataDirectory, "local_articles.json");

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
        List<ArticleDto>? articles = null;

        try
        {
            articles = await httpClient.GetFromJsonAsync<List<ArticleDto>>($"{BACKEND_URL}/articles");
        }
        catch
        {
            articles = new List<ArticleDto>();
        }

        articleCollection.Clear();
        publicArticleCollection.Clear();

        if (articles != null)
        {
            foreach (var article in articles)
            {
                articleCollection.Add(article);
                publicArticleCollection.Add(article);
            }
        }

        // load persisted local articles
        await LoadLocalArticlesAsync();

        // merge local-only articles (those with negative ids)
        foreach (var local in _localAdded)
        {
            if (!articleCollection.Any(a => a.Id == local.Id))
            {
                articleCollection.Insert(0, local);
                publicArticleCollection.Insert(0, local);
            }
        }
    }

    private async Task SaveLocalArticlesAsync()
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                PropertyNameCaseInsensitive = true
            };

            // avoid serializing nested Articles list on Author to reduce size
            var clone = _localAdded.Select(a => new ArticleDto
            {
                Id = a.Id,
                Title = a.Title,
                Description = a.Description,
                AuthorId = a.AuthorId,
                Author = a.Author == null ? null : new AuthorDto { Id = a.Author.Id, Name = a.Author.Name, Articles = new List<ArticleDto>() },
                CreatedDate = a.CreatedDate
            }).ToList();

            var json = JsonSerializer.Serialize(clone, options);
            await File.WriteAllTextAsync(_localFileName, json);
        }
        catch
        {
            // ignore persistence errors
        }
    }

    private async Task LoadLocalArticlesAsync()
    {
        try
        {
            if (!File.Exists(_localFileName)) return;
            var json = await File.ReadAllTextAsync(_localFileName);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var list = JsonSerializer.Deserialize<List<ArticleDto>>(json, options);
            if (list == null) return;

            _localAdded.Clear();
            _localAdded.AddRange(list);

            // ensure _nextLocalId remains negative and less than any existing local id
            if (_localAdded.Any())
            {
                var min = _localAdded.Min(a => a.Id);
                if (min <= _nextLocalId) _nextLocalId = min - 1;
            }
        }
        catch
        {
            // ignore
        }
    }

    private async void OnLoginClickedAsync(object? sender, EventArgs e)
    {
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

    private void OnAddNewClickedAsync(object? sender, EventArgs e)
    {
        // show in-page editor to create a new article
        _editingId = 0;
        AdminTitleEntry.Text = string.Empty;
        AdminAuthorEntry.Text = string.Empty;
        AdminPublishedPicker.Date = DateTime.Today;
        AdminDescriptionEntry.Text = string.Empty;
        AdminContentEditor.Text = string.Empty;

        ArticleEditorPanel.IsVisible = true;
    }

    private void OnBackFromEditorClicked(object? sender, EventArgs e)
    {
        // hide editor without saving
        ArticleEditorPanel.IsVisible = false;
        _editingId = 0;
    }

    private async void OnSaveArticleClicked(object? sender, EventArgs e)
    {
        var title = AdminTitleEntry.Text ?? string.Empty;
        if (string.IsNullOrWhiteSpace(title))
        {
            await DisplayAlert("Validation", "Title is required", "OK");
            return;
        }

        var authorName = AdminAuthorEntry.Text ?? string.Empty;
        var created = AdminPublishedPicker.Date;
        var desc = AdminDescriptionEntry.Text ?? string.Empty;
        var content = AdminContentEditor.Text ?? string.Empty;

        if (_editingId == 0)
        {
            var newArticle = new ArticleDto()
            {
                Id = _nextLocalId--,
                Title = title,
                Description = desc,
                AuthorId = 0,
                Author = new AuthorDto { Id = 0, Name = authorName, Articles = new List<ArticleDto>() },
                CreatedDate = created
            };

            _localAdded.Add(newArticle);
            articleCollection.Insert(0, newArticle);
            publicArticleCollection.Insert(0, newArticle);

            await SaveLocalArticlesAsync();

            // try to POST to backend (best effort)
            try
            {
                var httpClient = httpClientFactory.CreateClient();
                await httpClient.PostAsJsonAsync($"{BACKEND_URL}/articles", newArticle);
            }
            catch
            {
                // ignore
            }
        }
        else
        {
            var existing = articleCollection.FirstOrDefault(a => a.Id == _editingId);
            if (existing != null)
            {
                // create a new instance with updated values and replace in collections so UI updates immediately
                var updated = new ArticleDto
                {
                    Id = existing.Id,
                    Title = title,
                    Description = desc,
                    AuthorId = existing.AuthorId,
                    Author = existing.Author == null ? new AuthorDto { Id = 0, Name = authorName, Articles = new List<ArticleDto>() } : new AuthorDto { Id = existing.Author.Id, Name = authorName, Articles = new List<ArticleDto>() },
                    CreatedDate = created
                };

                // replace in articleCollection
                for (int i = 0; i < articleCollection.Count; i++)
                {
                    if (articleCollection[i].Id == updated.Id)
                    {
                        articleCollection[i] = updated;
                        break;
                    }
                }

                // replace in publicArticleCollection
                for (int i = 0; i < publicArticleCollection.Count; i++)
                {
                    if (publicArticleCollection[i].Id == updated.Id)
                    {
                        publicArticleCollection[i] = updated;
                        break;
                    }
                }

                // update persisted local list if needed
                if (updated.Id < 0)
                {
                    var local = _localAdded.FirstOrDefault(a => a.Id == updated.Id);
                    if (local != null)
                    {
                        local.Title = updated.Title;
                        local.Description = updated.Description;
                        local.CreatedDate = updated.CreatedDate;
                        local.Author = updated.Author;
                        await SaveLocalArticlesAsync();
                    }
                }

                // try to PUT to backend
                if (updated.Id > 0)
                {
                    try
                    {
                        var httpClient = httpClientFactory.CreateClient();
                        await httpClient.PutAsJsonAsync($"{BACKEND_URL}/articles/{updated.Id}", updated);
                    }
                    catch { }
                }
            }
        }

        ArticleEditorPanel.IsVisible = false;
        _editingId = 0;
    }

    private async void OnDeleteClickedAsync(object? sender, EventArgs e)
    {
        var article = (ArticleDto)((Button)sender).BindingContext;

        articleCollection.Remove(article);
        publicArticleCollection.Remove(article);

        if (article.Id < 0)
        {
            _localAdded.RemoveAll(a => a.Id == article.Id);
            await SaveLocalArticlesAsync();
            return;
        }

        if (article.Id > 0)
        {
            try
            {
                var httpClient = httpClientFactory.CreateClient();
                await httpClient.DeleteAsync($"{BACKEND_URL}/articles/{article.Id}");
            }
            catch { }
        }
    }

    private void OnEditClickedAsync(object? sender, EventArgs e)
    {
        var article = (ArticleDto)((Button)sender).BindingContext;

        _editingId = article.Id;
        AdminTitleEntry.Text = article.Title;
        AdminAuthorEntry.Text = article.Author?.Name ?? string.Empty;
        AdminPublishedPicker.Date = article.CreatedDate == default ? DateTime.Today : article.CreatedDate;
        AdminDescriptionEntry.Text = article.Description;
        AdminContentEditor.Text = string.Empty; // ArticleDto in Common doesn't have content field

        ArticleEditorPanel.IsVisible = true;
    }
}