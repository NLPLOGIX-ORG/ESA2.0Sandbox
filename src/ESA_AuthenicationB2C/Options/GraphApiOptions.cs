namespace ESA_AuthenicationB2C.Options
{
    public class GraphApiOptions
    {
        public const string MicrosoftGraph = "MicrosoftGraph";

        public string TenantId { get; set; }
        public string AppId { get; set; }
        public string ClientSecret { get; set; }
        public string B2cExtensionAppClientId { get; set; }
    }
}