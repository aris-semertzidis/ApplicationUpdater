namespace ApplicationUpdater.Core.Updater;

public class Updater
{
    private readonly string applicationUrl;

    public Updater(string applicationUrl)
    {
        this.applicationUrl = applicationUrl;
    }

    public async Task UpdateApplication()
    {
        using (HttpClient client = new HttpClient())
        {
            HttpResponseMessage response = await client.GetAsync(applicationUrl);
            response.EnsureSuccessStatusCode();
            string content = await response.Content.ReadAsStringAsync();

            // Process the file content here

            // Example: Print the file content
            Console.WriteLine(content);
        }
    }
}
