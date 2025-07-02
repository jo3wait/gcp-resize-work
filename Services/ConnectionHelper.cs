using Microsoft.Data.SqlClient;

namespace ResizeWork.Services;

/// <summary>
/// 依環境變數自動組出連線字串：
/// * privat IP 直連，連線字串設定於 Secret Manager
/// * 本機 Docker Desktop，LOCAL_SQL_HOST 存在
/// * Cloud Run，無 LOCAL_SQL_HOST (走 Cloud SQL 整合/Proxy)，預設 127.0.0.1:1433
/// </summary>
internal static class ConnectionHelper
{
    public static string Build()
    {
        // (1) Cloud Run <-> Cloud SQL for MSSQL (privat IP 直連)（設定於 Secret Manager）
        var full = Environment.GetEnvironmentVariable("DB_CONN");
        if (!string.IsNullOrWhiteSpace(full))
            return full;

        // (2) Local Docker Desktop MSSQL
        var local = Environment.GetEnvironmentVariable("LOCAL_SQL_HOST");
        if (!string.IsNullOrWhiteSpace(local))
        {
            return new SqlConnectionStringBuilder
            {
                DataSource = $"{local},1433",
                UserID = Environment.GetEnvironmentVariable("DB_USER"),
                Password = Environment.GetEnvironmentVariable("DB_PASS"),
                InitialCatalog = Environment.GetEnvironmentVariable("DB_NAME"),
                Encrypt = false,
                TrustServerCertificate = true
            }.ConnectionString;
        }

        // (3) 預設
        return new SqlConnectionStringBuilder
        {
            DataSource = Environment.GetEnvironmentVariable("DB_HOST") ?? "127.0.0.1,1433",
            UserID = Environment.GetEnvironmentVariable("DB_USER"),
            Password = Environment.GetEnvironmentVariable("DB_PASS"),
            InitialCatalog = Environment.GetEnvironmentVariable("DB_NAME"),
            Encrypt = true,
            TrustServerCertificate = true
        }.ConnectionString;
    }
}
