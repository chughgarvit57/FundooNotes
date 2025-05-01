# 📝 Fundoo Notes API

**Fundoo Notes** is a secure and scalable note-taking Web API built with **ASP.NET Core**. It enables users to register, log in, and perform operations like creating, updating, pinning, archiving, and deleting notes. The API uses **JWT authentication** and provides developer-friendly documentation via **Swagger**.

---

## 🚀 Features

- 🔐 User registration & login with JWT token
- 📝 Create, update, delete notes
- 📌 Pin/Unpin notes
- 📓 Archive/Unarchive notes
- 📇 Label management
- 🧪 API Testing with Swagger
- 🧱 Clean architecture (Controller ➔ Business ➔ Repository ➔ Model)
- 💡 Dependency Injection + Entity Framework Core
- 🤝 Collaborators can be added to share and manage notes together

---

## ⚙️ Tech Stack

- **Framework**: ASP.NET Core Web API
- **Database**: Microsoft SQL Server
- **ORM**: Entity Framework Core
- **Authentication**: JWT (JSON Web Token)
- **API Docs**: Swagger / Swashbuckle

---

## 🛠️ Getting Started

### ✅ Prerequisites

- [.NET 6 SDK or later](https://dotnet.microsoft.com/download)
- SQL Server (Local or Azure)
- Visual Studio / VS Code

### 🔧 Run Locally

```bash
# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run the project
dotnet run
```

Once the app is running, open:

```
https://localhost:5001/swagger
```

to access Swagger UI for testing your APIs.

---

## 📁 Project Structure

```
FundooNotes/
├── FundooNotes/Controllers/
├── BusinessLayer/
├── RepositoryLayer/
├── ModelLayer/
├── RepositoryLayer/Migrations/
├── appsettings.json (not committed)
└── Program.cs
```

---

## 🔐 Security

- Secrets like JWT keys and SMTP credentials are stored in `appsettings.json`, which is **excluded** from version control via `.gitignore`.
- Use **User Secrets** or **environment variables** for production.

---

## 📚 API Documentation

Swagger is auto-integrated and runs at:

```
https://localhost:5001/swagger
```

Explore and test all routes via a clean UI.

## 📄 License

This project is licensed under the [MIT License](LICENSE).

---

## 🙇‍♂️ Author

Made with ❤️ by **Garvit Chugh**  
[GitHub](https://github.com/chughgarvit57) | [LinkedIn](https://www.linkedin.com/in/chughgarvit09/)