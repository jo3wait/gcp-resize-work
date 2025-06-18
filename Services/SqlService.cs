using Microsoft.Data.SqlClient;

namespace ResizeWork.Services;

public sealed class SqlService
{
    private readonly string _cs = ConnectionHelper.Build();

    public async Task MarkDoneAsync(string imageId, string thumbKey, CancellationToken ct)
    {
        const string sql = """
            UPDATE FILES
            SET    STATUS      = 'done',
                   THUMB_PATH  = @thumb,
                   RESIZE_DT   = SYSUTCDATETIME()
            WHERE  ID    = @id;
            """;

        await using var conn = new SqlConnection(_cs);
        await conn.OpenAsync(ct);

        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@thumb", thumbKey);
        cmd.Parameters.AddWithValue("@id", imageId);
        await cmd.ExecuteNonQueryAsync(ct);
    }
}
