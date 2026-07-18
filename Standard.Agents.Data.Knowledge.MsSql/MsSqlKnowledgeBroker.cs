// ---------------------------------------------------------------
// Copyright (c) Hassan Habib All rights reserved.
// Licensed under the The Standard Software License (TSSL)
// ---------------------------------------------------------------

using Microsoft.Data.SqlClient;
using Standard.Agents.Brokers.Knowledges;

namespace Standard.Agents.Data.Knowledge.MsSql;

public sealed class MsSqlKnowledgeBroker : IKnowledgeBroker
{
    private readonly string connectionString;
    private readonly string table;
    private readonly string contentColumn;
    private readonly int maxResults;

    // The table and content column are developer configuration, resolved once at
    // construction — that is where a store's identity lives, so the IKnowledgeBroker
    // interface never has to grow a "which table" parameter. The prompt (query) is always
    // a parameter, never interpolated.
    public MsSqlKnowledgeBroker(
        string connectionString,
        string table = "KnowledgeDocuments",
        string contentColumn = "Content",
        int maxResults = 3)
    {
        this.connectionString = connectionString;
        this.table = table;
        this.contentColumn = contentColumn;
        this.maxResults = maxResults;
    }

    public async ValueTask<IReadOnlyList<string>> SelectKnowledgeAsync(string query)
    {
        var documents = new List<string>();

        string column = $"[{this.contentColumn.Replace("]", "]]")}]";

        string sql =
            $"SELECT TOP (@maxResults) {column} FROM {this.table} "
                + $"WHERE FREETEXT({column}, @query)";

        await using var connection = new SqlConnection(this.connectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@maxResults", this.maxResults);
        command.Parameters.AddWithValue("@query", query);

        await using SqlDataReader reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            documents.Add(reader.GetString(0));
        }

        return documents;
    }
}
