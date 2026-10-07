# 🎟️ Smart Event, Venue & Parking Management Platform

A full-stack web application for managing events, venues, seat reservations, parking, bookings, payments, notifications and related services.

The system is built using **Angular, ASP.NET Core Web API, Entity Framework Core, SQL Server and SignalR**.

## 🚀 Key Features

- 🔐 User authentication and authorization
- 🎫 Event management and booking
- 💺 Seat reservation and seat management
- 🅿️ Parking reservation
- 🏢 Venue management
- 💳 Payment and receipt management
- 🔔 Notifications
- 🎟️ Digital tickets and QR check-in
- 📡 Real-time communication using SignalR
- 👨‍💼 Admin and organizer management

## 🛠️ Tech Stack

### Frontend
- Angular
- TypeScript
- HTML
- CSS

### Backend
- ASP.NET Core Web API
- C#
- Entity Framework Core
- SignalR

### Database
- Microsoft SQL Server

### Development Tools
- Git
- GitHub
- Swagger
- Visual Studio / VS Code

## 👨‍💻 My Contribution

As a team member and team leader, I contributed to the development of the project.

### Frontend Development
- Developed and worked on the application's frontend pages.
- Worked with Angular components and user interfaces.
- Integrated frontend functionality with backend APIs.

### Backend Testing
- Participated in backend API testing.
- Tested API functionality and application workflows.
- Helped identify and verify issues during development.

### Team Contribution
- Contributed to overall project development and coordination.
- Worked with the team to integrate different parts of the system.

## 📂 Repository Structure

```text
frontend/
└── sevpms-web/
    └── Angular application

backend/
├── src/
│   ├── SEVPMS.Api/
│   ├── SEVPMS.Application/
│   ├── SEVPMS.Domain/
│   ├── SEVPMS.Infrastructure/
│   └── SEVPMS.Realtime/
│
└── tests/
    └── Backend tests
```

## 🔄 System Architecture

```text
Angular Frontend
       ↓
ASP.NET Core Web API
       ↓
Entity Framework Core
       ↓
SQL Server

        ↘
       SignalR
        ↘
 Real-time Communication
```

## ▶️ Running the Project

### Backend

The backend is built with ASP.NET Core and uses SQL Server for data storage.

### Frontend

The frontend is built with Angular and normally runs on:

```text
http://localhost:4200
```

The development API is configured around:

```text
http://localhost:5090
```

## 🔒 Configuration & Security

Production credentials and sensitive configuration should not be committed to source control.

Environment-specific configuration should be used for:

- SQL Server connection strings
- JWT signing keys
- SMTP configuration
- Payment provider credentials
- QR/ticket signing configuration

## 🎯 Project Goal

The goal of this project is to provide a centralized platform for managing **events, venues, seat reservations, parking and related services** through a modern web application.

---

⭐ Developed as a team project with a focus on full-stack web application development.
