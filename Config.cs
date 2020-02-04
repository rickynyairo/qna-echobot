namespace MyEchoBot.Config{

    static class Envars{
        public static string StorageName = System.Environment.GetEnvironmentVariable("STORAGE_NAME");
        public static string StorageUrlString = System.Environment.GetEnvironmentVariable("STORAGE_STRING");
        public static string LuisAppId = System.Environment.GetEnvironmentVariable("LUIS_APP_ID");
        public static string LuisAPIKey = System.Environment.GetEnvironmentVariable("LUIS_API_KEY");
        public static string LuisAPIHostName = System.Environment.GetEnvironmentVariable("LUIS_API_HOSTNAME");
    }
}