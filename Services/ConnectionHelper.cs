using Microsoft.Data.SqlClient;

namespace ResizeWork.Services;

/// <summary>
/// 依環境變數自動組出連線字串：
/// * 本機 Docker Desktop → LOCAL_SQL_HOST 存在
/// * Cloud Run → 無 LOCAL_SQL_HOST (走 Cloud SQL 整合/Proxy)，預設 127.0.0.1:1433
/// </summary>
internal static class ConnectionHelper
{
    public static string Build()
    {
        // ── A. Local Docker Desktop MSSQL ─────────────────────────────
        var localHost = Environment.GetEnvironmentVariable("LOCAL_SQL_HOST");
        if (!string.IsNullOrWhiteSpace(localHost))
        {
            return new SqlConnectionStringBuilder
            {
                DataSource = $"{localHost},1433",
                UserID = Environment.GetEnvironmentVariable("DB_USER"),
                Password = Environment.GetEnvironmentVariable("DB_PASS"),
                InitialCatalog = Environment.GetEnvironmentVariable("DB_NAME"),
                Encrypt = false,
                TrustServerCertificate = true
            }.ConnectionString;
        }

        // ── B. Cloud Run ↔ Cloud SQL for MSSQL (Require SSL=True) ─────
        return new SqlConnectionStringBuilder
        {
            DataSource = "127.0.0.1,1433",   // Cloud Run 連線整合或 Cloud SQL Proxy
            UserID = Environment.GetEnvironmentVariable("DB_USER"),
            Password = Environment.GetEnvironmentVariable("DB_PASS"),
            InitialCatalog = Environment.GetEnvironmentVariable("DB_NAME"),
            Encrypt = true,
            TrustServerCertificate = true   // 若要 CA 驗證請設 false + 配 PFX/PEM
        }.ConnectionString;
    }
}
