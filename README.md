# 🧠 Ollama Chat API (.NET + ADO.NET)

Simple ASP.NET Core Web API that:

- Sends prompts to Ollama (local LLM)
- Receives generated response
- Saves request & response to SQL Server
- Returns AI response to client

---

## 🚀 Tech Stack

- ASP.NET Core Web API
- HttpClient (Typed DI)
- ADO.NET
- SQL Server
- Ollama (Local LLM)

---

## 📂 Database

Database: `Ollama_msgs`

Table:

```sql
CREATE TABLE msgs
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    request_msg NVARCHAR(MAX),
    response_msg NVARCHAR(MAX)
);
