![The Standard for Agents](https://raw.githubusercontent.com/hassanhabib/The-Standard-Agent/main/assets/the-standard-agent-cover.png)

# Standard.Agents.Data.Knowledge.MsSql

**A SQL Server knowledge source for [The Standard AI Agent Framework](https://github.com/hassanhabib/The-Standard-Agent).**

[![Nuget](https://img.shields.io/nuget/v/Standard.Agents.Data.Knowledge.MsSql?logo=nuget&style=default&color=blue)](https://www.nuget.org/packages/Standard.Agents.Data.Knowledge.MsSql)
![Nuget](https://img.shields.io/nuget/dt/Standard.Agents.Data.Knowledge.MsSql?color=blue&label=Downloads)
[![Core: Standard.Agents](https://img.shields.io/nuget/v/Standard.Agents?logo=nuget&style=default&color=blue&label=Standard.Agents)](https://www.nuget.org/packages/Standard.Agents)
[![License: TSSL](https://img.shields.io/badge/license-TSSL%20v1.1-blue.svg)](https://github.com/hassanhabib/Standard.Agents.Data.Knowledge.MsSql/blob/master/LICENSE.txt)

---

> **Part of [The Standard for Agents](https://github.com/hassanhabib/The-Standard-Agent).**
> In `Agent = Orchestration(Data, Decision, Direction)`, this package extends the **Data** nature —
> it moves *knowledge* from local files into a **SQL Server** table with full-text search. It is an
> optional adapter: it plugs into the core [`Standard.Agents`](https://www.nuget.org/packages/Standard.Agents)
> package through the same `IKnowledgeBroker` seam, so nothing else about your agent changes.

The core `Standard.Agents` package ships **file-based** knowledge and is deliberately
dependency-free. This package brings
[Microsoft.Data.SqlClient](https://github.com/dotnet/SqlClient) so you can ground an agent on a SQL
Server table instead — with **tokenized full-text search** (`FREETEXT`) rather than the file
default's substring match.

## Install

```bash
dotnet add package Standard.Agents                       # the core framework
dotnet add package Standard.Agents.Data.Knowledge.MsSql  # this knowledge source
```

## Use

```csharp
using Standard.Agents;
using Standard.Agents.Data.Knowledge.MsSql;

var agent = new StandardAgent(apiUrl, apiKey, "LLooMA2.0")
    .UseKnowledgeMsSql("Server=localhost;Database=agent;Trusted_Connection=True;Encrypt=False;");

string answer = await agent.ProcessPromptAsync("What is our refund window?");
```

One line — everything else (skills, tools, guardians, memory, streaming) is the core, unchanged.
Each turn the agent calls `SelectKnowledgeAsync` with the prompt; this broker runs a full-text
query and returns the top matches, seeded into the agent's observations.

### Options

```csharp
.UseKnowledgeMsSql(
    connectionString,
    table: "KnowledgeDocuments",   // optionally schema-qualified, e.g. "kb.Documents"
    contentColumn: "Content",      // the text column searched and returned
    maxResults: 3)                 // documents injected per turn
```

Or construct the broker directly and pass it to the core's `.UseKnowledge(...)`:

```csharp
.UseKnowledge(new MsSqlKnowledgeBroker(connectionString, table: "kb.Documents"))
```

## The table

This broker **reads** an existing table — populating it is your ETL, kept out of the agent.
`FREETEXT` requires a **full-text index** on the content column:

```sql
CREATE TABLE KnowledgeDocuments (
    Id      bigint IDENTITY(1,1) PRIMARY KEY,
    Content nvarchar(max) NOT NULL
);

-- full-text search prerequisites (once per database / table)
CREATE FULLTEXT CATALOG AgentKnowledgeCatalog;
CREATE FULLTEXT INDEX ON KnowledgeDocuments (Content)
    KEY INDEX PK__KnowledgeDocuments   -- your PK's index name
    ON AgentKnowledgeCatalog;
```

The query the broker runs is, in effect:

```sql
SELECT TOP (@maxResults) Content FROM KnowledgeDocuments
WHERE FREETEXT(Content, @prompt);
```

`FREETEXT` matches on meaning — stemming and stop-words included — so "refund window" finds
"windows for refunds." The prompt is always a bound parameter; the table and column names are your
configuration, resolved once at construction. (Results are the full-text matches capped at
`maxResults`; ranked ordering is a later enhancement.)

## Where this fits

| | |
|---|---|
| **Core framework** | [`Standard.Agents`](https://www.nuget.org/packages/Standard.Agents) · [repo](https://github.com/hassanhabib/The-Standard-Agent) |
| **This package** | the **Data** nature — knowledge, backed by SQL Server full-text search |
| **Sibling** | [`Standard.Agents.Data.Knowledge.Postgres`](https://www.nuget.org/packages/Standard.Agents.Data.Knowledge.Postgres) |
| **Getting started** | [The core how-to](https://github.com/hassanhabib/The-Standard-Agent/blob/main/docs/how-to.md) |

## License

Licensed under the [The Standard Software License (TSSL) v1.1](LICENSE.txt) — the same license as
`Standard.Agents`. [Microsoft.Data.SqlClient](https://github.com/dotnet/SqlClient) is MIT-licensed
and consumed as a NuGet dependency.
