namespace Test_Taste_Console_Application.Constants
{
    public static class HttpClientSettings
    {
        public const string JsonType = "application/json";

        //API bearer token: appsettings.json first, then this env var as a fallback.
        public const string ApiKeyConfigurationKey = "SolarSystemApi:ApiKey";
        public const string ApiKeyEnvironmentVariable = "SOLAR_SYSTEM_API_KEY";
    }
}
