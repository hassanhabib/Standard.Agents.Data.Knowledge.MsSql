// ---------------------------------------------------------------
// Copyright (c) Hassan Habib All rights reserved.
// Licensed under the The Standard Software License (TSSL)
// ---------------------------------------------------------------

namespace Standard.Agents.Data.Knowledge.MsSql;

public static class StandardAgentMsSqlKnowledgeExtensions
{
    /// <summary>
    /// Grounds the agent on a SQL Server table using full-text search — the SQL Server
    /// equivalent of <c>.Knowledge(folder)</c>. Each turn, rows whose content matches the
    /// prompt (via <c>FREETEXT</c>) are returned and seeded into the agent's observations.
    /// Requires a full-text index on the content column.
    /// </summary>
    /// <param name="agent">The agent to configure.</param>
    /// <param name="connectionString">SQL Server connection string.</param>
    /// <param name="table">Table (optionally schema-qualified) holding the documents.</param>
    /// <param name="contentColumn">Text column to search and return.</param>
    /// <param name="maxResults">Maximum documents injected per turn.</param>
    /// <returns>The same agent, so calls can be chained.</returns>
    public static StandardAgent UseKnowledgeMsSql(
        this StandardAgent agent,
        string connectionString,
        string table = "KnowledgeDocuments",
        string contentColumn = "Content",
        int maxResults = 3) =>
        agent.UseKnowledge(new MsSqlKnowledgeBroker(
            connectionString, table, contentColumn, maxResults));
}
