using Common;
using System.Collections.ObjectModel;
using System.Net.Http.Json;
using System.Net.Http.Headers;

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
        await SetAuthToken();
        await LoadAuthors();
    }

    private async Task SetAuthToken()
    {
        try
        {
            // Get the stored auth token
            var token = await SecureStorage.GetAsync("auth_token");

            if (!string.IsNullOrEmpty(token))
            {
                // Set the authorization header for all requests
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                ShowMessage("Please log in first", true);
            }
        }
        catch (Exception ex)
        {
            ShowMessage($"Auth error: {ex.Message}", true);
        }
    }

    private async Task LoadAuthors()
    {
        try
        {
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            var response = await _httpClient.GetAsync("api/authors");

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                ShowMessage("Session expired. Please log in again.", true);
                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;
                return;
            }

            response.EnsureSuccessStatusCode();
            var authors = await response.Content.ReadFromJsonAsync<List<AuthorDto>>();

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
            // Make sure token is set
            await SetAuthToken();

            var newAuthor = new AuthorDto
            {
                Name = NameEntry.Text.Trim(),
            };

            var response = await _httpClient.PostAsJsonAsync("api/authors", newAuthor);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                ShowMessage("Session expired. Please log in again.", true);
                return;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                ShowMessage("Access denied. Admin privileges required.", true);
                return;
            }

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
                var errorContent = await response.Content.ReadAsStringAsync();
                ShowMessage($"Error adding author: {response.StatusCode} - {errorContent}", true);
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