using System.Net.Http.Headers;

namespace EduOnline.IntegrationTest;

public static class TestsExtensions
{
    public static decimal ApenasNumeros(this string value)
    {
        return Convert.ToDecimal(new string(value.Where(char.IsDigit).ToArray()));
    }

    public static void AtribuirToken(this HttpClient client, string token)
    {
        client.AtribuirJsonMediaType();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public static void AtribuirJsonMediaType(this HttpClient client)
    {
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }
}
