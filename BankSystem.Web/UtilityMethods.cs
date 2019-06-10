using System.Data.SqlClient;

namespace BankSystem.Web
{
    public static class UtilityMethods
    {
        public static string GenerateConnectionString()
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder
            {
                DataSource = ".\\SQLEXPRESS",
                InitialCatalog = "master",
                UserID = "SA",
            };

            builder["Trusted_Connection"] = true;

            return builder.ConnectionString;
        }
    }
}
