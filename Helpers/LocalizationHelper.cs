using System.Text.Json;

namespace AceBackend.Helpers
{
    public class LocalizationHelper
    {
        private readonly Dictionary<string, JsonElement> _enMessages;
        private readonly Dictionary<string, JsonElement> _arMessages;

        public LocalizationHelper()
        {
            var enPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "en.json");
            var arPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "ar.json");

            var enJson = File.ReadAllText(enPath);
            var arJson = File.ReadAllText(arPath);

            _enMessages = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(enJson) ?? new Dictionary<string, JsonElement>();
            _arMessages = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(arJson) ?? new Dictionary<string, JsonElement>();
        }

        public string Get(string key, string language = "en")
        {
            var messages = language.ToLower() == "ar" ? _arMessages : _enMessages;
            
            var keys = key.Split('.');
            JsonElement current = messages.ContainsKey(keys[0]) ? messages[keys[0]] : new JsonElement();

            for (int i = 1; i < keys.Length; i++)
            {
                if (current.ValueKind == JsonValueKind.Object && current.TryGetProperty(keys[i], out JsonElement next))
                {
                    current = next;
                }
                else
                {
                    return key; // Return key if not found
                }
            }

            return current.ValueKind == JsonValueKind.String ? current.GetString() ?? key : key;
        }
    }
}
