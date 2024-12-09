namespace TrafficCourts.Staff.Service.Caching;

public class Cache
{
    /// <summary>
    /// The prefix of items cached by the staff api
    /// </summary>
    public const string Prefix = "staff:";

    /// <summary>
    /// Keycloak named cache
    /// </summary>
    public static class Keycloak
    {
        /// <summary>
        /// Stores the ICollection&lt;GroupRepresentation&gt; by group name.
        /// </summary>
        /// <param name="group">The group</param>
        /// <param name="version">The version of the data structure. If new data structure is used, defined a new version. Defaults to 1.</param>
        /// <returns></returns>
        public static string UsersByGroup(string group, int version = 1) => $"keycloak:v{version}:groups:{group}";

        /// <summary>
        /// Stores the ICollection&lt;GroupRepresentation&gt; by user name.
        /// </summary>
        /// <param name="user">The user</param>
        /// <param name="version">The version of the data structure. If new data structure is used, defined a new version. Defaults to 1.</param>
        /// <returns></returns>
        public static string UsersByUsername(string user, int version = 1) => $"keycloak:v{version}:users:{user}";
    }

    /// <summary>
    /// Oracle data named cache. Stores data cached queried from Oracle.
    /// </summary>
    /// <remarks>
    /// Stored in the shared library because we may need to clear cached data on
    /// changes to disputes.
    /// </remarks>
    public static class OracleData
    {
        /// <summary>
        /// Stores the ICollection&lt;DisputeListItem&gt; items fetched from GetAllDisputes
        /// </summary>
        /// <param name="version">The version of the data structure. If new data structure is used, defined a new version. Defaults to 1.</param>
        public static string DisputeListItems(int version = 1) => $"oracle:v{version}:all:dispute-list-items";
    }

    /// <summary>
    /// Api named cache. Stores data cached queried returned fro the API, primarily code tables.
    /// </summary>
    public static class Api
    {
        public static string Agencies(int version = 1) => $"staff-api:v{version}:agencies";
        public static string Agencies(string type, int version = 1) => $"staff-api:v{version}:agencies:{type}";

        public static string Countries(int version = 1) => $"staff-api:v{version}:countries";

        public static string Languages(int version = 1) => $"staff-api:v{version}:languages";
        public static string Provinces(int version = 1) => $"staff-api:v{version}:provinces";

        public static string DisputeCaseFileStatusTypes(int version = 1) => $"staff-api:v{version}:tco:dispute-status_types";

        /// <summary>
        /// Stores the list of statutues
        /// </summary>
        /// <param name="version">The version of the data structure. If new data structure is used, defined a new version. Defaults to 1.</param>
        public static string Statutes(int version = 1) => $"staff-api:v{version}:statutes";

    }
}
