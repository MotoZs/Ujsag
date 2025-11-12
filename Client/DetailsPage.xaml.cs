using System.Net.Http.Json;

namespace Client;

public partial class DetailsPage : ContentPage, IQueryAttributable
{
    private readonly IHttpClientFactory httpClientFactory;
    private ToDoDto toDo;
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
            toDo = new ToDoDto { Title = "Ez egy új todo" };
            return;
        }
        else
        {
            var httpClient = httpClientFactory.CreateClient();
            toDo = await httpClient.GetFromJsonAsync<ToDoDto>($"https://localhost:7241/get/{id}");
        }

        TodoId.Text = toDo.Id.ToString();
    }
}