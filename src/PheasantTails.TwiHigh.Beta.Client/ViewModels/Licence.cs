namespace PheasantTails.TwiHigh.Beta.Client.ViewModels
{
    public class Licence
    {
        public string PackageName { get; set; } = string.Empty;
        public string PackageVersion { get; set; } = string.Empty;
        public string PackageUrl { get; set; } = string.Empty;
        public string Copyright { get; set; } = string.Empty;
        public string[] Authors { get; set; } = Array.Empty<string>();
        public string Description { get; set; } = string.Empty;
        public string LicenseUrl { get; set; } = string.Empty;
        public string LicenseType { get; set; } = string.Empty;
    }
}
