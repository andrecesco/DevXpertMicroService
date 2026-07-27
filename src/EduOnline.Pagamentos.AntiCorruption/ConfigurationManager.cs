using System.Security.Cryptography;

namespace EduOnline.Pagamentos.AntiCorruption;

public class ConfigurationManager : IConfigurationManager
{
    public string GetValue(string node)
    {
        return new string([.. Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", 10).Select(s => s[RandomNumberGenerator.GetInt32(s.Length)])]);
    }
}
