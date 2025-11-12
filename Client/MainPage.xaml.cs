using System.Collections.ObjectModel;
using System.Net.Http.Json;

namespace Client;

public partial class MainPage : ContentPage
{
    private readonly IHttpClientFactory httpClientFactory;
    private ObservableCollection<ToDoDto> toDoCollection = [];

    public MainPage(IHttpClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;

        InitializeComponent();

        ToDosView.ItemsSource = toDoCollection;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        var httpClient = httpClientFactory.CreateClient();
        var toDos = await httpClient.GetFromJsonAsync<List<ToDoDto>>("https://localhost:7241/list");

        toDoCollection.Clear();

        foreach (var toDo in toDos)
            toDoCollection.Add(toDo);
    }

    private async void OnAddNewClickedAsync(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("details");
    }

    private async void OnDeleteClickedAsync(object? sender, EventArgs e)
    {
        var toDo = (ToDoDto)((Button)sender).BindingContext;
        var httpClient = httpClientFactory.CreateClient();
        var toDos = await httpClient.DeleteAsync($"https://localhost:7241/delete/{toDo.Id}");

        await LoadDataAsync();
    }

    private async void OnEditClickedAsync(object? sender, EventArgs e)
    {
        var toDo = (ToDoDto)((Button)sender).BindingContext;

        await Shell.Current.GoToAsync("details", new ShellNavigationQueryParameters { { "Id", toDo.Id } });
    }
}