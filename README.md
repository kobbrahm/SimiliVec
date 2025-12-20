# SimiliVec: Custom Vector Database

[![Status Badge](https://img.shields.io/badge/Status-In%20Development-blue)](https://github.com/your-username/SimiliVec) 
[![Language Badge](https://img.shields.io/badge/Built%20With-C%23-darkgreen)](https://docs.microsoft.com/en-us/dotnet/csharp/)

> ** STUDENT PROJECT WARNING ** > This project is a proof-of-concept and learning exercise. It is **NOT** production-ready and lacks critical features like persistence and concurrency.

---

## Project Overview

**SimiliVec** is a custom vector database designed to explore the mechanics of high-performance similarity search. The project focuses on a native C# implementation of modern vector database components:

* **Embedding Model:** Uses the **E5 transformer model** for generating high-quality vector representations.
* **Indexing Structure:** Implements the **Hierarchical Navigable Small World (HNSW)** graph for efficient approximate nearest neighbor search.

---

## Key Features (Planned)

*  High-performance **HNSW** index implementation.
*  Native integration with the **E5** embedding model.
*  Simple API for vector insertion and similarity search.

---

## Getting Started

### Prerequisites

- .NET 9.0 SDK or later
- PowerShell (Windows) or Bash (Linux/Mac)

### Setup Instructions

1. **Clone the repository:**
   ```bash
   git clone <repository-url>
   cd SimiliVec
   ```

2. **Download the E5 model and tokenizer files:**

   **On Windows (PowerShell):**
   ```powershell
   .\setup-models.ps1
   ```

   **On Linux/Mac (Bash):**
   ```bash
   chmod +x setup-models.sh
   ./setup-models.sh
   ```

   This script will download:
   - E5-Small-V2 model (133 MB)
   - Tokenizer files (vocab.txt, tokenizer.json, tokenizer_config.json)

3. **Build and run the project:**
   ```bash
   cd SimiliVec.Api
   dotnet build
   dotnet run
   ```

4. **Access the API:**
   - Swagger UI: http://localhost:5202/swagger
   - API Endpoint: http://localhost:5202/api
