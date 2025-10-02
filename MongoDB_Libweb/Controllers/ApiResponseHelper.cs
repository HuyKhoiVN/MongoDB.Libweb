using System.Text.Json;

namespace MongoDB_Libweb.Controllers
{
    public class ApiResponseHelper
    {
        public static bool IsSuccess(JsonElement response)
        {
            return response.ValueKind == JsonValueKind.Object && 
                   response.TryGetProperty("success", out var successProperty) && 
                   successProperty.GetBoolean();
        }

        public static string GetErrorMessage(JsonElement response, string defaultMessage = "Operation failed")
        {
            if (response.ValueKind == JsonValueKind.Object && 
                response.TryGetProperty("error", out var errorProperty))
            {
                return errorProperty.GetString() ?? defaultMessage;
            }
            return defaultMessage;
        }

        public static string GetStringProperty(JsonElement element, string propertyName, string defaultValue = "")
        {
            if (element.ValueKind == JsonValueKind.Object && 
                element.TryGetProperty(propertyName, out var property))
            {
                return property.GetString() ?? defaultValue;
            }
            return defaultValue;
        }

        public static JsonElement GetObjectProperty(JsonElement element, string propertyName)
        {
            if (element.ValueKind == JsonValueKind.Object && 
                element.TryGetProperty(propertyName, out var property) &&
                property.ValueKind == JsonValueKind.Object)
            {
                return property;
            }
            return JsonDocument.Parse("{}").RootElement;
        }
    }
}
