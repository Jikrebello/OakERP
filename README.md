# 🌳 Oak ERP

[![.NET](https://img.shields.io/badge/.NET%207+-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![MAUI](https://img.shields.io/badge/MAUI-Blazor-blueviolet?logo=microsoft&logoColor=white)](https://learn.microsoft.com/en-us/dotnet/maui/)
[![PostgreSQL](https://img.shields.io/badge/Database-PostgreSQL-336791?logo=postgresql&logoColor=white)](https://www.postgresql.org/)

> **Modern, modular ERP system built with .NET MAUI, Blazor, and PostgreSQL**  
> Cross-platform. Offline-ready. Razor-powered UI. Cloud and private hosting supported.

---

## ✨ Features

- ⚙️ **Unified UI**: Razor components shared across desktop and web
- 🖥️ **Offline Desktop App**: Native .NET MAUI app for Windows and MacOS
- 🌐 **Online Web Client**: Always-connected Blazor WebAssembly client
- 📱 **Mobile Companion App**: Android/iOS support for POS, warehouse, and CRM
- 🔄 **Sync Capability**: Offline-first model with eventual online sync
- 🐳 **Easy Dev Setup**: Docker-based PostgreSQL and pgAdmin services

---

## 🧱 Initial Modules

OakERP is being built module-by-module. Initial release will include:

- 📘 General Ledger (GL)
- 🧾 Accounts Payable (AP)
- 💰 Accounts Receivable (AR)
- 📦 Inventory Management
- 👥 Customer Relationship Management (CRM)

> Future modules will include HR, Payroll, Fixed Assets, Projects, and Manufacturing.

---

## 📦 Solution Overview

```plaintext
/OakERP.Solution
├── Apps/              → Desktop, Web, Mobile (.NET MAUI Blazor)
├── Core/              → Domain, Application Logic, Shared DTOs
├── Infrastructure/    → EF Core, Auth, Notifications
├── UI/                → Shared Razor Components
├── Reports/           → Printable/exportable Razor reports
├── Tools/             → CLI Migration Tool
├── Tests/             → Unit + Integration tests
├── Docs/              → Auto-generated and dev documentation
├── docker-compose.yml
├── README.md
└── README-dev.md      → Developer setup instructions
```

---

## 🧑‍💻 Developer Setup

To get the database environment running locally using Docker:

> 📖 Follow the full setup in [README-dev.md](OakERP.Docs\README-dev.md)

---

## 📣 Project Vision

OakERP is designed for businesses who want a modern ERP system that:

- Runs anywhere — offline or online
- Feels the same across all platforms
- Can be self-hosted or deployed to the cloud
- Supports disconnected workflows and automatic sync

---

## 🚧 Project Status

| Component               | Status         |
| ----------------------- | -------------- |
| Docker DB Setup         | ✅ Complete     |
| MAUI Blazor Shell       | ✅ Bootstrapped |
| Core Modules (GL/AP/AR) | 🚧 In Progress  |
| Mobile POS App          | 🛠️ Pending      |
| EF Migration Tool       | 🛠️ Scaffolded   |

