namespace TruckeroApp.ServiceClients;

internal static class ApiRoutes
{
    internal static class Company
    {
        internal const string Base = "/api/Company";
        internal const string Metadata = "/api/Company/metadata";
        internal static string ById(Guid id) => $"{Base}/{id}";
    }

    internal static class Store
    {
        internal const string Base = "/api/Store";
        internal static string ById(Guid id) => $"{Base}/{id}";
        internal static string CompanyStores(Guid companyId) => $"{Base}/company/{companyId}";
        internal static string Disable(Guid id) => $"{Base}/{id}/disable";
    }
}
