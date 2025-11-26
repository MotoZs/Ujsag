using Common;
using System.Collections.ObjectModel;
using System.Net.Http.Json;

namespace Client;

public partial class AuthorsPage : ContentPage
{
    private readonly HttpClient _httpClient;
    private ObservableCollection<AuthorDto> _authors;

    public AuthorsPage()
    {
        InitializeComponent();
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://localhost:7072/api") // Update with your API URL
        };
        _authors = new ObservableCollection<AuthorDto>();
        AuthorsCollectionView.ItemsSource = _authors;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAuthors();
    }

    private async Task LoadAuthors()
    {
        try
        {
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            var authors = await _httpClient.GetFromJsonAsync<List<AuthorDto>>("api/authors");

            _authors.Clear();
            if (authors != null)
            {
                foreach (var author in authors)
                {
                    _authors.Add(author);
                }
            }

            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
        catch (Exception ex)
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
            ShowMessage($"Error loading authors: {ex.Message}", true);
        }
    }

    private async void OnAddAuthorClicked(object sender, EventArgs e)
    {
        // Validate inputs
        if (string.IsNullOrWhiteSpace(NameEntry.Text))
        {
            ShowMessage("Please enter author name", true);
            return;
        }

        try
        {
            var newAuthor = new AuthorDto
            {
                Name = NameEntry.Text.Trim(),
            };

            var response = await _httpClient.PostAsJsonAsync("api/authors", newAuthor);

            if (response.IsSuccessStatusCode)
            {
                ShowMessage("Author added successfully!", false);

                // Clear inputs
                NameEntry.Text = string.Empty;

                // Reload authors list
                await LoadAuthors();
            }
            else
            {
                ShowMessage($"Error adding author: {response.StatusCode}", true);
            }
        }
        catch (Exception ex)
        {
            ShowMessage($"Error adding author: {ex.Message}", true);
        }
    }

    private void ShowMessage(string message, bool isError)
    {
        MessageLabel.Text = message;
        MessageLabel.TextColor = isError ? Colors.Red : Colors.Green;
        MessageLabel.IsVisible = true;

        // Hide message after 3 seconds
        Task.Run(async () =>
        {
            await Task.Delay(3000);
            MainThread.BeginInvokeOnMainThread(() =>
            {
                MessageLabel.IsVisible = false;
            });
        });
    }
}