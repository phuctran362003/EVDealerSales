# 🚗 EVDealerSales - Electric Vehicle Dealer Management System

[![.NET](https://img.shields.io/badge/.NET-8.0-purple)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-8.0-blue)](https://asp.net/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-2022-red)](https://www.microsoft.com/sql-server)
[![Docker](https://img.shields.io/badge/Docker-Enabled-2496ED)](https://www.docker.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

A comprehensive **Electric Vehicle (EV) Dealership Management System** built with **ASP.NET Core 8.0**, **Razor Pages**, **Entity Framework Core**, and **SignalR**. This system provides complete workflow management for customers, dealer staff, and managers.

## 📋 Table of Contents

- [Features](#-features)
- [Technology Stack](#-technology-stack)
- [Architecture](#-architecture)
- [Getting Started](#-getting-started)
- [User Roles & Workflows](#-user-roles--workflows)
- [Project Structure](#-project-structure)
- [Configuration](#-configuration)
- [API Integration](#-api-integration)
- [Database Schema](#-database-schema)
- [Contributing](#-contributing)
- [License](#-license)

## ✨ Features

### 🛒 Customer Features
- **Vehicle Browsing**: Browse, search, filter, and compare electric vehicles
- **Test Drive Management**: Schedule and manage test drive appointments
- **Order Processing**: Create orders with complete vehicle customization
- **Stripe Payment Integration**: Secure online payment processing
- **Delivery Tracking**: Request and track vehicle delivery status
- **Feedback System**: Submit feedback and reviews for purchased vehicles
- **Profile Management**: Update personal information and view order history

### 👔 Dealer Staff Features
- **Test Drive Management**: Approve, schedule, and manage customer test drives
- **Order Management**: View and process customer orders
- **Delivery Coordination**: Confirm and manage vehicle deliveries
- **Customer Management**: View customer information and purchase history
- **Real-time Chat**: Communicate with customers via SignalR

### 👨‍💼 Manager Features
- **Vehicle Inventory**: Full CRUD operations for vehicle catalog
- **Staff Management**: Manage dealer staff accounts and permissions
- **Feedback Management**: Review and resolve customer feedback
- **Analytics Dashboard**: View sales reports and business analytics
- **All Staff Permissions**: Access to all staff-level features

### 🤖 AI Features
- **Gemini AI Chatbot**: Intelligent customer support chatbot
- **Data Analytics**: AI-powered sales and trend analysis

## 🛠 Technology Stack

### Backend
- **Framework**: ASP.NET Core 8.0
- **UI**: Razor Pages
- **ORM**: Entity Framework Core 8.0
- **Database**: SQL Server 2022
- **Authentication**: JWT Bearer Authentication
- **Real-time**: SignalR
- **Payment**: Stripe.NET SDK

### Frontend
- **HTML5/CSS3/JavaScript**
- **Bootstrap** (assumed from Razor Pages)
- **SignalR Client** for real-time features

### DevOps
- **Containerization**: Docker & Docker Compose
- **Database Migrations**: EF Core Migrations

### External APIs
- **Google Gemini AI**: Chatbot integration
- **Stripe**: Payment processing

## 🏗 Architecture

This project follows a **Clean Architecture** pattern with clear separation of concerns:

```
┌─────────────────────────────────────────────┐
│         Presentation Layer                  │
│    (Razor Pages, SignalR Hubs)             │
└────────────────┬────────────────────────────┘
                 │
┌────────────────▼────────────────────────────┐
│         Business Logic Layer                │
│    (Services, Interfaces, Utils)           │
└────────────────┬────────────────────────────┘
                 │
┌────────────────▼────────────────────────────┐
│         Data Access Layer                   │
│    (Repository, UnitOfWork, DbContext)     │
└────────────────┬────────────────────────────┘
                 │
┌────────────────▼────────────────────────────┐
│         Business Objects                    │
│    (Entities, DTOs, Enums)                 │
└─────────────────────────────────────────────┘
```

## 🚀 Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (for containerized deployment)
- [SQL Server 2022](https://www.microsoft.com/sql-server) (if running locally)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [Rider](https://www.jetbrains.com/rider/)

### Installation

#### Option 1: Docker Deployment (Recommended)

1. **Clone the repository**
   ```bash
   git clone https://github.com/Dessmin/EVDealerSales.git
   cd EVDealerSales
   ```

2. **Build and run with Docker Compose**
   ```bash
   docker-compose up -d
   ```

3. **Access the application**
   ```
   http://localhost:5000
   ```

The database will be automatically created and seeded with initial data.

#### Option 2: Local Development

1. **Clone the repository**
   ```bash
   git clone https://github.com/Dessmin/EVDealerSales.git
   cd EVDealerSales
   ```

2. **Update Connection String**
   
   Edit `appsettings.Development.json` in the `EVDealerSales.Presentation` folder:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=EVDealerSalesDb;Trusted_Connection=True;TrustServerCertificate=True"
     }
   }
   ```

3. **Apply Migrations**
   ```bash
   cd EVDealerSales.Presentation
   dotnet ef database update
   ```

4. **Run the application**
   ```bash
   dotnet run
   ```

5. **Access the application**
   ```
   http://localhost:5000
   ```

### Default Accounts

After seeding, use these credentials to login:

| Role | Email | Password |
|------|-------|----------|
| **Manager** | manager@evdealer.com | Manager@123 |
| **Staff** | staff@evdealer.com | Staff@123 |
| **Customer** | customer@evdealer.com | Customer@123 |

## 👥 User Roles & Workflows

### Customer Workflows (9 Main Flows)

1. **Authentication & Profile**
   - Register/Login
   - Profile management

2. **Vehicle Browsing**
   - Browse vehicles catalog
   - View vehicle details
   - Compare vehicles

3. **Test Drive**
   - Register test drive
   - View test drive history

4. **Order & Payment**
   - Create order
   - Checkout with Stripe
   - Payment confirmation
   - View order history
   - Request delivery
   - Submit feedback

### Dealer Staff Workflows (6 Main Flows)

1. **Test Drive Management**
   - View test drive requests
   - Confirm/Cancel test drives
   - Register test drive for walk-in customers

2. **Order Management**
   - View all orders
   - Filter and search orders
   - Process delivery requests

3. **Delivery Management**
   - Confirm deliveries
   - Update delivery status
   - Track delivery progress

4. **Customer Management**
   - View customer list
   - View customer details

5. **Reports**
   - View sales reports
   - Analytics dashboard

### Manager Workflows (7 Main Flows)

1. **Vehicle Management**
   - Create new vehicles
   - Update vehicle information
   - Manage inventory

2. **Staff Management**
   - Create staff accounts
   - Update staff information
   - Deactivate staff

3. **Feedback Management**
   - View all feedback
   - Resolve feedback
   - Delete feedback

4. **Reports & Analytics**
   - Sales reports
   - Data analytics

5. **All Staff Permissions**
   - Access to all staff features

## 🔌 API Integration

### Stripe Payment Integration

The system integrates Stripe for secure payment processing:

- **Checkout Session**: Creates payment sessions for orders
- **Payment Intent**: Handles payment confirmation
- **Webhook Support**: Processes payment events

### Google Gemini AI Integration

Intelligent chatbot powered by Google's Gemini AI:

- **Natural Language Processing**: Understands customer queries
- **Product Recommendations**: Suggests vehicles based on preferences
- **Real-time Responses**: Instant customer support

## 🗄 Database Schema

### Core Entities

- **User**: Customer, staff, and manager accounts
- **Vehicle**: Electric vehicle inventory
- **TestDrive**: Test drive appointments
- **Order**: Customer orders
- **Payment**: Payment transactions
- **Delivery**: Delivery tracking
- **Feedback**: Customer feedback and reviews
- **Chat**: Real-time chat messages

### Enumerations

- **RoleType**: Customer, DealerStaff, DealerManager
- **OrderStatus**: Pending, Confirmed, Processing, Completed, Cancelled
- **PaymentStatus**: Pending, Paid, Failed, Refunded
- **DeliveryStatus**: Pending, Confirmed, InTransit, Delivered, Failed, Cancelled
- **TestDriveStatus**: Pending, Confirmed, Completed, Cancelled
- **InvoiceStatus**: Unpaid, Paid, Overdue, Cancelled

## 🔐 Security Features

- **JWT Authentication**: Secure token-based authentication
- **Role-based Authorization**: Granular permission control
- **Password Hashing**: Secure password storage
- **Data Protection**: ASP.NET Core Data Protection for sensitive data
- **HTTPS Support**: Production-ready SSL/TLS configuration

## 📊 Key Features Implementation

### Real-time Chat (SignalR)
- **ChatHub**: Customer-to-staff real-time messaging
- **ChatbotHub**: AI-powered customer support

### Payment Processing (Stripe)
- Secure checkout flow
- Payment confirmation
- Order status updates

### Data Analytics
- Sales reports
- Customer insights
- Inventory tracking


## 🙏 Acknowledgments

- FPT University - PRN232 Course
- ASP.NET Core Team
- Stripe API
- Google Gemini AI
- SignalR Team
