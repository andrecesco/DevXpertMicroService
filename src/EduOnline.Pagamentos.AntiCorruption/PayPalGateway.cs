namespace EduOnline.Pagamentos.AntiCorruption;

public class PayPalGateway : IPayPalGateway
{
    public bool CommitTransaction(string cardHashKey, string orderId, decimal amount)
    {
        return new Random().Next(2) == 0;
        //return false;
    }

    public string GetCardHashKey(string serviceKey, string cartaoCredito)
    {
        return new string([.. Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", 10).Select(s => s[new Random().Next(s.Length)])]);
    }

    public string GetPayPalServiceKey(string apiKey, string encriptionKey)
    {
        return new string([.. Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", 10).Select(s => s[new Random().Next(s.Length)])]);
    }
}
