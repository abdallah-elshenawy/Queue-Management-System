# 🏢 Queue Management System — Backend Integration Guide

> **Version:** 1.0 · **Last Updated:** June 2026  
> **Audience:** React.js frontend developers  
> **This file is the single source of truth for consuming the QMS API.**

---

## Table of Contents

1. [Project Overview](#1-project-overview)
2. [Authentication & Authorization](#2-authentication--authorization)
3. [Global Conventions](#3-global-conventions)
4. [Database Schema Overview](#4-database-schema-overview)
5. [API Endpoints — Full Reference](#5-api-endpoints--full-reference)
6. [Feature Modules (Chunks)](#6-feature-modules-chunks)
7. [File Upload Endpoints](#7-file-upload-endpoints)
8. [Real-Time / SignalR](#8-real-time--signalr)
9. [Background Jobs & Scheduled Tasks](#9-background-jobs--scheduled-tasks)
10. [Validation Rules](#10-validation-rules)
11. [CORS & Environment Configuration](#11-cors--environment-configuration)
12. [Third-Party Integrations](#12-third-party-integrations)
13. [Quick Reference Cheat Sheet](#13-quick-reference-cheat-sheet)
14. [Frontend Integration Checklist](#14-frontend-integration-checklist)
15. [Application Workflow — Complete User Journeys](#15-application-workflow--complete-user-journeys)

---

## 1. PROJECT OVERVIEW

The **Queue Management System (QMS)** is a backend API that enables businesses to manage customer queues across multiple branches. Customers book tickets for services at a specific branch, a door verifier scans QR codes to verify arrivals, counter employees call and serve the next ticket in line, and display screens show live queue information via real-time SignalR updates. The Admin role manages branches, services, employees, and the overall system.

### Technology Stack

| Component | Technology |
|-----------|-----------|
| **Framework** | ASP.NET Core 8.0 (net8.0) |
| **Language** | C# |
| **Database** | Microsoft SQL Server |
| **ORM** | Entity Framework Core 8.0 |
| **Authentication** | JWT Bearer Tokens |
| **Real-Time** | SignalR |
| **Object Mapping** | AutoMapper |
| **Password Hashing** | BCrypt.Net (work factor 12) |
| **API Docs** | Swagger / OpenAPI (dev only) |

### Base URLs

| Environment | URL |
|-------------|-----|
| **Local Development** | `https://localhost:7XXX` (check `launchSettings.json` for exact port) |
| **Swagger UI (dev only)** | `https://localhost:7XXX/swagger` |
| **SignalR Hub** | `https://localhost:7XXX/hubs/queue` |

### Environment Variables for Frontend

```env
REACT_APP_API_BASE_URL=https://localhost:7XXX
REACT_APP_SIGNALR_HUB_URL=https://localhost:7XXX/hubs/queue
```

---

## 2. AUTHENTICATION & AUTHORIZATION

### Auth Method: JWT Bearer Token

The API uses **JWT Bearer tokens** for authentication. Tokens are signed with HMAC-SHA256 and carry user claims including `NameIdentifier` (user ID), `Email`, and `Role`.

### 2.1 How to Obtain a Token

**Endpoint:** `POST /api/auth/login`

**Request:**
```json
{
  "email": "user@example.com",
  "password": "MyP@ssw0rd"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": null,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "accessTokenExpirationInSeconds": 3000,
    "refreshToken": "dGhpcyBpcyBhIHJlZnJlc2ggdG9rZW4...",
    "refreshTokenExpiration": "2026-06-20T16:00:00Z"
  },
  "errors": null
}
```

**Key fields to extract and store:**
- `data.accessToken` → store in `localStorage` or `sessionStorage`
- `data.refreshToken` → store securely (localStorage or httpOnly cookie if possible)
- `data.accessTokenExpirationInSeconds` → **3000 seconds (50 minutes)** — use to set a timer for refresh

### 2.2 How to Attach the Token

Add this header to **every authenticated request:**

```
Authorization: Bearer <accessToken>
```

**React.js Axios interceptor example:**

```javascript
import axios from 'axios';

const api = axios.create({
  baseURL: process.env.REACT_APP_API_BASE_URL,
});

// Request interceptor — attach token to every request
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('accessToken');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Response interceptor — handle 401 errors
api.interceptors.response.use(
  (response) => response,
  async (error) => {
    if (error.response?.status === 401) {
      const refreshToken = localStorage.getItem('refreshToken');
      if (refreshToken) {
        try {
          const res = await axios.post(
            `${process.env.REACT_APP_API_BASE_URL}/api/auth/refresh`,
            { token: refreshToken }
          );
          localStorage.setItem('accessToken', res.data.data.accessToken);
          localStorage.setItem('refreshToken', res.data.data.refreshToken);
          error.config.headers.Authorization = `Bearer ${res.data.data.accessToken}`;
          return axios(error.config); // Retry original request
        } catch {
          localStorage.clear();
          window.location.href = '/login';
        }
      } else {
        localStorage.clear();
        window.location.href = '/login';
      }
    }
    return Promise.reject(error);
  }
);

export default api;
```

### 2.3 Token Expiry & Refresh

| Token Type | Expiry Duration |
|-----------|----------------|
| **Access Token** | **50 minutes** (3000 seconds) |
| **Refresh Token** | **15 days** |

**Refresh endpoint:** `POST /api/auth/refresh`

```json
// Request
{ "token": "<refreshToken>" }

// Success Response (200)
{
  "success": true,
  "data": {
    "accessToken": "new-access-token...",
    "refreshToken": "new-refresh-token..."
  }
}
```

> ⚠️ **IMPORTANT:** Each refresh token is single-use. After using it, you receive a **new** refresh token. Always save the new one. If a revoked refresh token is reused (replay attack), **ALL** refresh tokens for that user are revoked and the user must log in again.

### 2.4 Roles & Permissions

| Role | Enum Value | Description | Access Level |
|------|-----------|-------------|-------------|
| **Admin** | `0` | Company administrator | Full system management: branches, services, employees, users |
| **Counter** | `1` | Counter employee | Call next ticket, complete tickets for their assigned service |
| **DoorVerifier** | `2` | Door verification staff | Verify customer QR codes at the branch entrance |
| **Customer** | `3` | End user | Register, book tickets, view profile, update profile |

**Role-based access matrix:**

| Endpoint Area | Admin | Counter | DoorVerifier | Customer | Public |
|-------------|-------|---------|-------------|----------|--------|
| Register Customer | — | — | — | — | ✅ |
| Login / Refresh / Logout | ✅ | ✅ | ✅ | ✅ | ✅ (login/refresh) |
| Register Admin/Counter/DoorVerifier | ✅ | — | — | — | — |
| Branch CRUD | ✅ | — | — | — | — |
| Service CRUD | ✅ | — | — | — | — |
| Get all users / by role / by id | ✅ | — | — | — | — |
| Update employee assignment | ✅ | — | — | — | — |
| View own profile | ✅ | ✅ | ✅ | ✅ | — |
| Update own profile / password / image | ✅ | ✅ | ✅ | ✅ | — |
| Delete own account | ✅ | ✅ | ✅ | ✅ | — |
| Create / update / delete ticket | ✅ | ✅ | ✅ | ✅ | — |
| Get all tickets / get ticket by id | ✅ | ✅ | ✅ | ✅ | — |
| Door verifying (QR scan) | — | — | ✅ | — | — |
| Call next ticket | — | ✅ | — | — | — |
| Complete ticket | — | ✅ | — | — | — |
| Display screen data | ✅ | ✅ | ✅ | ✅ | — |

### 2.5 Expired / Invalid Token Response

When a token is missing, expired, or invalid, the API returns:

```
HTTP 401 Unauthorized
```

The response body may be empty or contain:
```json
{
  "success": false,
  "message": null,
  "data": null,
  "errors": "You don't have access."
}
```

When a valid token lacks the required role:
```
HTTP 403 Forbidden
```

---

## 3. GLOBAL CONVENTIONS

### 3.1 Base URL Prefix

All API endpoints are prefixed with:
```
/api/{controllerName}
```

Controller name mapping:
| Controller | Base Path |
|-----------|----------|
| AuthController | `/api/auth` |
| BranchController | `/api/branch` |
| TicketController | `/api/ticket` |
| ServiceController | `/api/service` |
| DisplayScreenController | `/api/displayscreen` |

### 3.2 Date/Time Format

- All dates are in **UTC** and serialized as **ISO 8601** format.
- Example: `"2026-06-05T16:21:39.000Z"`
- The frontend should convert to the user's local timezone for display.

### 3.3 Pagination

> ⚠️ **No pagination is implemented.** All list endpoints return the full collection. For large datasets, handle client-side pagination.

### 3.4 Sorting & Filtering

> ⚠️ **No server-side sorting or filtering is implemented.** All list endpoints return unfiltered, unsorted results. Implement sorting/filtering client-side.

### 3.5 Standard Success Response Envelope

Every API response follows this exact JSON shape:

```json
{
  "success": true,
  "message": "Optional success message or null",
  "data": { /* payload — can be object, array, or string */ },
  "errors": null
}
```

### 3.6 Standard Error Response Envelope

**Business logic errors (returned by the service layer):**
```json
{
  "success": false,
  "message": null,
  "data": null,
  "errors": "Human-readable error message string"
}
```

**Validation errors (returned by ASP.NET model binding):**
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "FieldName": ["Error message 1", "Error message 2"],
    "AnotherField": ["Error message"]
  }
}
```

### 3.7 HTTP Status Codes

| Status Code | Meaning in This Project |
|------------|------------------------|
| **200 OK** | Request succeeded. Data in `data` field. |
| **400 Bad Request** | Validation failed OR business rule violated. Check `errors`. |
| **401 Unauthorized** | No token, expired token, or invalid token. |
| **403 Forbidden** | Valid token but insufficient role. |
| **404 Not Found** | Resource not found (branch, user, ticket). |

---

## 4. DATABASE SCHEMA OVERVIEW

### 4.1 Entity List

| Entity | Table Name | Description |
|--------|-----------|-------------|
| User | Users | All users (admins, counters, door verifiers, customers) |
| EmployeeInfo | Employees | Extended info for Counter and DoorVerifier users |
| CustomerInfo | Customers | Extended info for Customer users |
| Branch | Branches | Physical branch/office locations |
| Service | Services | Queue service types (e.g., "Account Opening", "Cash Deposit") |
| Ticket | Tickets | Customer queue tickets |
| DisplayScreen | DisplayScreens | Branch display screen (one per branch) |
| DisplayTicket | DisplayTickets | Called tickets shown on the display screen |
| RefreshToken | RefreshTokens | JWT refresh tokens for auth |

### 4.2 Entity Fields

#### User
| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| **Id** (PK) | int | No | Auto-generated primary key |
| Username | string(30) | No | Unique username |
| FirstName | string(30) | No | First name |
| SecondName | string(30) | No | Second name |
| ThirdName | string(30) | Yes | Third name (optional) |
| FourthName | string(30) | Yes | Fourth name or remainder (optional) |
| ImageUrl | string | Yes | Profile image path (e.g., `/images/users/guid.jpg`) |
| Email | string(60) | No | Unique email address |
| NationalNumber | string | No | Unique national ID number (exactly 14 digits) |
| PasswordHash | string | No | BCrypt hashed password |
| PhoneNumber | string | No | Unique Egyptian phone number |
| City | string | No | User's city |
| IsActive | bool? | Yes | `true` = active, `false` = soft-deleted. Default: `true` |
| Role | string (enum) | No | `Admin` / `Counter` / `DoorVerifier` / `Customer` |
| CreatedAt | DateTime? | Yes | Account creation timestamp (UTC). Default: `DateTime.UtcNow` |

**Indexes:** `Username` (unique), `Email` (unique), `PhoneNumber` (unique), `NationalNumber` (unique)

#### EmployeeInfo
| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| **UserId** (PK, FK → User) | int | No | Primary key = User.Id |
| CounterNumber | int? | Yes | Counter desk number (null for DoorVerifiers) |
| ServiceId (FK → Service) | int? | Yes | Assigned service (null for DoorVerifiers) |
| BranchId (FK → Branch) | int | No | Assigned branch |

**Indexes:** `ServiceId` (unique — each service can only be assigned to one employee)

#### CustomerInfo
| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| **UserId** (PK, FK → User) | int | No | Primary key = User.Id |

#### Branch
| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| **Id** (PK) | int | No | Auto-generated primary key |
| Name | string(50) | No | Unique branch name |
| Address | string(50) | No | Branch physical address |
| Contact | string(20) | No | Unique contact phone/info |
| CreatedAt | DateTime? | Yes | Creation timestamp. Default: `DateTime.UtcNow` |
| IsActive | bool? | Yes | Default: `true` |

**Indexes:** `Name` (unique), `Contact` (unique)

#### Service
| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| **Id** (PK) | int | No | Auto-generated primary key |
| Name | string(50) | No | Unique service name |
| Description | string(200) | No | Service description |
| IsActive | bool? | Yes | Default: `true` |

**Indexes:** `Name` (unique)

#### Ticket
| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| **Id** (PK) | int | No | Auto-generated primary key |
| TicketNumber | string | No | 8-char uppercase hex (auto-generated from GUID) |
| CustomerId (FK → CustomerInfo) | int | No | The customer who booked |
| ServiceId (FK → Service) | int | No | The requested service |
| BranchId (FK → Branch) | int | No | The branch location |
| EmployeeId (FK → EmployeeInfo) | int? | Yes | Set when a counter calls this ticket |
| QRCodeData | string | No | 32-char GUID hex (auto-generated, unique QR data) |
| Status | string (enum) | No | `Waiting`/`Serving`/`Completed`/`Skipped`/`Requeued` |
| ReservedAt | DateTime? | Yes | When the ticket was created. Default: `DateTime.UtcNow` |
| CalledAt | DateTime? | Yes | When the counter called this ticket |
| CompletedAt | DateTime? | Yes | When the service was completed |
| IsVerifiedAtDoor | bool? | Yes | Whether the door verifier scanned the QR. Default: `false` |
| VerificationTime | DateTime? | Yes | When the QR was scanned |

**Indexes:** `(TicketNumber, BranchId)` (composite unique)

#### DisplayScreen
| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| **Id** (PK) | int | No | Auto-generated primary key |
| BranchId (FK → Branch) | int | No | One display per branch |
| LastUpdated | DateTime? | Yes | Last update timestamp |
| IsActive | bool? | Yes | Default: `true` |

#### DisplayTicket
| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| **Id** (PK) | int | No | Auto-generated primary key |
| DisplayId (FK → DisplayScreen) | int | No | The display screen |
| TicketId (FK → Ticket) | int | No | The ticket being displayed |
| CounterNumber | int? | Yes | Which counter desk |
| CalledAt | DateTime | No | When the ticket was called |

#### RefreshToken
| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| **Id** (PK) | int | No | Auto-generated primary key |
| Token | string | No | The refresh token string (indexed) |
| ExpiresAt | DateTime | No | Expiry timestamp. Default: `UtcNow + 15 days` |
| UserId (FK → User) | int | No | Owner user |
| IsRevoked | bool | No | Whether revoked. Default: `false` |
| CreatedAt | DateTime | No | Creation timestamp. Default: `DateTime.UtcNow` |

**Indexes:** `Token` (non-unique index for fast lookup)

### 4.3 Entity Relationships

```
User (1) ←→ (0..1) EmployeeInfo          [1-to-1, FK: EmployeeInfo.UserId]
User (1) ←→ (0..1) CustomerInfo           [1-to-1, FK: CustomerInfo.UserId]
User (1) ←→ (0..*) RefreshToken           [1-to-many, FK: RefreshToken.UserId]

Branch (1) ←→ (0..*) EmployeeInfo         [1-to-many, FK: EmployeeInfo.BranchId]
Branch (1) ←→ (0..*) Ticket               [1-to-many, FK: Ticket.BranchId]
Branch (1) ←→ (0..1) DisplayScreen        [1-to-1, FK: DisplayScreen.BranchId, CASCADE delete]

Service (1) ←→ (0..1) EmployeeInfo        [1-to-1, FK: EmployeeInfo.ServiceId]
Service (1) ←→ (0..*) Ticket              [1-to-many, FK: Ticket.ServiceId]

CustomerInfo (1) ←→ (0..*) Ticket         [1-to-many, FK: Ticket.CustomerId]
EmployeeInfo (1) ←→ (0..*) Ticket         [1-to-many, FK: Ticket.EmployeeId]

DisplayScreen (1) ←→ (0..*) DisplayTicket [1-to-many, FK: DisplayTicket.DisplayId]
Ticket (1) ←→ (0..*) DisplayTicket        [1-to-many, FK: DisplayTicket.TicketId]
```

---

## 5. API ENDPOINTS — FULL REFERENCE

---

### 5.1 AUTH CONTROLLER (`/api/auth`)

---

#### [POST] /api/auth/register-customer
**Purpose:** Register a new customer account (public — no auth required).  
**Auth required:** No

**Request Headers:**
| Header | Value | Required |
|--------|-------|----------|
| Content-Type | application/json | Yes |

**Request Body (JSON):**
```json
{
  "fullName*": "string — Required. Must contain at least 3 space-separated names (e.g., 'Ahmed Mohamed Ali')",
  "username*": "string — Required. 8–30 characters",
  "email*": "string — Required. Valid email, 10–50 characters",
  "phoneNumber*": "string — Required. Egyptian phone: starts with 010/011/012/015, 11 digits total",
  "nationalNumber*": "string — Required. Exactly 14 characters",
  "password*": "string — Required. Min 8 chars, 1 uppercase, 1 lowercase, 1 digit, 1 special char (@$!%*?&_)",
  "confirmPassword*": "string — Required. Must match password",
  "city*": "string — Required. 2–30 characters"
}
```

**Success Response (200):**
```json
{
  "success": true,
  "message": null,
  "data": "Customer registered successfully.",
  "errors": null
}
```

**Error Responses:**
| Status | When it occurs | Response body |
|--------|---------------|---------------|
| 400 | Validation fails | `{ "errors": { "FullName": ["Full name must contain at least 3 names."] } }` |
| 400 | Duplicate email | `{ "success": false, "errors": "The email 'x@x.com' is already in use." }` |
| 400 | Duplicate username | `{ "success": false, "errors": "The username 'x' is already in use." }` |
| 400 | Duplicate national number | `{ "success": false, "errors": "The national number 'x' is already in use." }` |
| 400 | Weak password | `{ "success": false, "errors": "Password must contain at least one uppercase letter." }` |

**Frontend Usage Example (axios):**
```javascript
import api from './api'; // your configured axios instance

const registerCustomer = async (formData) => {
  try {
    const response = await api.post('/api/auth/register-customer', {
      fullName: formData.fullName,
      username: formData.username,
      email: formData.email,
      phoneNumber: formData.phoneNumber,
      nationalNumber: formData.nationalNumber,
      password: formData.password,
      confirmPassword: formData.confirmPassword,
      city: formData.city
    });
    return response.data;
  } catch (error) {
    return error.response.data;
  }
};
```

---

#### [POST] /api/auth/login
**Purpose:** Authenticate a user and obtain access + refresh tokens.  
**Auth required:** No

**Request Headers:**
| Header | Value | Required |
|--------|-------|----------|
| Content-Type | application/json | Yes |

**Request Body (JSON):**
```json
{
  "email*": "string — Required. Valid email address",
  "password*": "string — Required"
}
```

**Success Response (200):**
```json
{
  "success": true,
  "message": null,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "accessTokenExpirationInSeconds": 3000,
    "refreshToken": "dGhpcyBpcyBhIHJlZnJlc2ggdG9rZW4=",
    "refreshTokenExpiration": "2026-06-20T16:21:39Z"
  },
  "errors": null
}
```

**Error Responses:**
| Status | When it occurs | Response body |
|--------|---------------|---------------|
| 400 | Invalid credentials | `{ "success": false, "errors": "Invalid email or password." }` |
| 400 | Account deactivated | `{ "success": false, "errors": "Invalid email or password." }` |
| 400 | Validation fails | `{ "errors": { "Email": ["The Email field is not a valid e-mail address."] } }` |

**Frontend Usage Example (axios):**
```javascript
const login = async (email, password) => {
  try {
    const response = await api.post('/api/auth/login', { email, password });
    const { accessToken, refreshToken, accessTokenExpirationInSeconds } = response.data.data;
    localStorage.setItem('accessToken', accessToken);
    localStorage.setItem('refreshToken', refreshToken);
    setTimeout(() => refreshAccessToken(), (accessTokenExpirationInSeconds - 60) * 1000);
    return response.data;
  } catch (error) {
    return error.response.data;
  }
};
```

---

#### [POST] /api/auth/refresh
**Purpose:** Exchange a valid refresh token for a new access token + new refresh token.  
**Auth required:** No

**Request Body (JSON):**
```json
{
  "token*": "string — Required. The current refresh token"
}
```

**Success Response (200):**
```json
{
  "success": true,
  "data": {
    "accessToken": "new-access-token...",
    "refreshToken": "new-refresh-token..."
  },
  "errors": null
}
```

**Error Responses:**
| Status | When it occurs | Response body |
|--------|---------------|---------------|
| 400 | Token expired | `{ "success": false, "errors": "Invalid or expired refresh token." }` |
| 400 | Token already used (replay attack) | `{ "success": false, "errors": "Refresh token already used. Please log in again." }` |

**Frontend Usage Example (axios):**
```javascript
const refreshAccessToken = async () => {
  const refreshToken = localStorage.getItem('refreshToken');
  try {
    const response = await api.post('/api/auth/refresh', { token: refreshToken });
    localStorage.setItem('accessToken', response.data.data.accessToken);
    localStorage.setItem('refreshToken', response.data.data.refreshToken);
    return response.data.data;
  } catch (error) {
    localStorage.clear();
    window.location.href = '/login';
  }
};
```

---

#### [POST] /api/auth/logout
**Purpose:** Revoke a refresh token to log the user out server-side.  
**Auth required:** Yes — Any authenticated user

**Query Parameters:**
| Name | Type | Default | Description |
|------|------|---------|-------------|
| token | string | — | The refresh token to revoke |

**Success Response (200):**
```json
{ "success": true, "data": "Logged out successfully.", "errors": null }
```

**Frontend Usage Example (axios):**
```javascript
const logout = async () => {
  const refreshToken = localStorage.getItem('refreshToken');
  try {
    await api.post(`/api/auth/logout?token=${encodeURIComponent(refreshToken)}`);
  } finally {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    window.location.href = '/login';
  }
};
```

---

#### [POST] /api/auth/register-admin
**Purpose:** Register a new admin user (only existing admins can do this).  
**Auth required:** Yes — `Admin` role

**Request Body (JSON):** Same as register-customer (fullName, username, email, phoneNumber, nationalNumber, password, confirmPassword, city)

**Success Response (200):**
```json
{ "success": true, "data": "Admin registered successfully.", "errors": null }
```

**Frontend Usage Example (axios):**
```javascript
const registerAdmin = async (formData) => {
  const response = await api.post('/api/auth/register-admin', formData);
  return response.data;
};
```

---

#### [POST] /api/auth/register-counter
**Purpose:** Register a new counter employee (Admin only).  
**Auth required:** Yes — `Admin` role

**Request Body (JSON):**
```json
{
  "fullName*": "string — At least 3 names",
  "username*": "string — 8–30 characters",
  "email*": "string — Valid email, 10–50 characters",
  "phoneNumber*": "string — Egyptian phone: 010/011/012/015 + 8 digits",
  "nationalNumber*": "string — Exactly 14 characters",
  "password*": "string — Strong password",
  "confirmPassword*": "string — Must match password",
  "city*": "string — 2–30 characters",
  "counterNumber*": "int — Required. The desk/counter number",
  "serviceId*": "int — Required. FK to Service table",
  "branchId*": "int — Required. FK to Branch table"
}
```

**Success Response (200):**
```json
{ "success": true, "data": "Counter employee registered successfully.", "errors": null }
```

**Frontend Usage Example (axios):**
```javascript
const registerCounter = async (data) => {
  const response = await api.post('/api/auth/register-counter', {
    fullName: data.fullName,
    username: data.username,
    email: data.email,
    phoneNumber: data.phoneNumber,
    nationalNumber: data.nationalNumber,
    password: data.password,
    confirmPassword: data.confirmPassword,
    city: data.city,
    counterNumber: data.counterNumber,
    serviceId: data.serviceId,
    branchId: data.branchId
  });
  return response.data;
};
```

---

#### [POST] /api/auth/register-door-verifier
**Purpose:** Register a new door verifier employee (Admin only).  
**Auth required:** Yes — `Admin` role

**Request Body (JSON):**
```json
{
  "fullName*": "string — At least 3 names",
  "username*": "string — 8–30 characters",
  "email*": "string — Valid email, 10–50 characters",
  "phoneNumber*": "string — Egyptian phone",
  "nationalNumber*": "string — Exactly 14 characters",
  "password*": "string — Strong password",
  "confirmPassword*": "string — Must match password",
  "city*": "string — 2–30 characters",
  "branchId*": "int — Required. FK to Branch table"
}
```

> **Note:** Door verifiers do NOT have `counterNumber` or `serviceId` — they only have a `branchId`.

**Success Response (200):**
```json
{ "success": true, "data": "Door verifier registered successfully.", "errors": null }
```

**Frontend Usage Example (axios):**
```javascript
const registerDoorVerifier = async (data) => {
  const response = await api.post('/api/auth/register-door-verifier', {
    fullName: data.fullName,
    username: data.username,
    email: data.email,
    phoneNumber: data.phoneNumber,
    nationalNumber: data.nationalNumber,
    password: data.password,
    confirmPassword: data.confirmPassword,
    city: data.city,
    branchId: data.branchId
  });
  return response.data;
};
```

---

#### [POST] /api/auth/door-verifying
**Purpose:** Verify a customer's ticket QR code at the branch door.  
**Auth required:** Yes — `DoorVerifier` role

**Request Body (JSON):**
```json
{
  "qrCode*": "string — Required. The QR code data from the customer's ticket"
}
```

**Success Response (200):**
```json
{ "success": true, "data": "Ticket verified successfully.", "errors": null }
```

**Side Effects:** 
- Sets `IsVerifiedAtDoor = true` and `VerificationTime` on the ticket
- **Broadcasts a `TicketVerified` SignalR event** to all clients in the branch group

**Error Responses:**
| Status | When it occurs | Response body |
|--------|---------------|---------------|
| 400 | Invalid QR code | `{ "success": false, "errors": "Invalid QR code." }` |
| 400 | Ticket already used | `{ "success": false, "errors": "This ticket has already been used." }` |
| 401 | Not authenticated | — |
| 403 | Wrong role | — |

**Frontend Usage Example (axios):**
```javascript
const verifyTicketAtDoor = async (qrCode) => {
  const response = await api.post('/api/auth/door-verifying', { qrCode });
  return response.data;
};
```

---

#### [POST] /api/auth/call-next
**Purpose:** Counter employee calls the next waiting ticket in their service queue.  
**Auth required:** Yes — `Counter` role

**Request Body:** None

**Success Response (200):**
```json
{
  "success": true,
  "data": "Ticket 'AB12CD34' is now being served at counter 3.",
  "errors": null
}
```

**Side Effects:**
- Changes ticket status from `Waiting` → `Serving`
- Sets `CalledAt` timestamp and assigns `EmployeeId`
- Creates a `DisplayTicket` record for the branch display screen
- **Broadcasts a `TicketCalled` SignalR event** to all clients in the branch group
- If the customer has NOT been verified at the door, the ticket status changes to `Requeued`

**Error Responses:**
| Status | When it occurs | Response body |
|--------|---------------|---------------|
| 400 | No waiting tickets | `{ "success": false, "errors": "No waiting tickets for this service." }` |
| 400 | Employee not assigned to service | `{ "success": false, "errors": "This employee is not assigned to a service." }` |
| 400 | Customer not verified at door | `{ "success": false, "errors": "Customer has not been verified at the door. Ticket requeued." }` |
| 400 | No display screen for branch | `{ "success": false, "errors": "No display screen found for this branch." }` |

**Frontend Usage Example (axios):**
```javascript
const callNextTicket = async () => {
  try {
    const response = await api.post('/api/auth/call-next');
    return response.data;
  } catch (error) {
    return error.response.data;
  }
};
```

---

#### [POST] /api/auth/complete/{id}
**Purpose:** Mark a ticket as completed after serving the customer.  
**Auth required:** Yes — `Counter` role

**Path Parameters:**
| Name | Type | Description |
|------|------|-------------|
| id | int | The ticket ID to complete |

**Request Body:** None

**Success Response (200):**
```json
{ "success": true, "data": "Ticket 'AB12CD34' completed.", "errors": null }
```

**Business Rules:**
- Only the employee who called the ticket can complete it
- Only tickets with `Serving` status can be completed

**Error Responses:**
| Status | When it occurs | Response body |
|--------|---------------|---------------|
| 400 | Ticket not found | `{ "success": false, "errors": "Ticket not found." }` |
| 400 | Not assigned to you | `{ "success": false, "errors": "You can only complete tickets assigned to you." }` |
| 400 | Not in Serving status | `{ "success": false, "errors": "Only a ticket currently being served can be completed." }` |

**Frontend Usage Example (axios):**
```javascript
const completeTicket = async (ticketId) => {
  const response = await api.post(`/api/auth/complete/${ticketId}`);
  return response.data;
};
```

---

#### [GET] /api/auth/profile
**Purpose:** Get the current authenticated user's profile.  
**Auth required:** Yes — Any authenticated user

**Success Response (200):**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "username": "ahmed_ali",
    "fullName": "Ahmed Mohamed Ali",
    "imageUrl": "/images/users/abc123.jpg",
    "email": "ahmed@example.com",
    "role": "Customer",
    "phoneNumber": "01012345678",
    "nationalNumber": "12345678901234",
    "city": "Cairo",
    "empInfo": null
  },
  "errors": null
}
```

For employees (Counter/DoorVerifier/Admin), `empInfo` will be:
```json
{
  "empInfo": {
    "counterNumber": 3,
    "serviceId": 5,
    "branchId": 2
  }
}
```

**Frontend Usage Example (axios):**
```javascript
const getProfile = async () => {
  const response = await api.get('/api/auth/profile');
  return response.data.data;
};
```

---

#### [GET] /api/auth/{id}
**Purpose:** Get a specific user by ID (Admin only).  
**Auth required:** Yes — `Admin` role

**Path Parameters:**
| Name | Type | Description |
|------|------|-------------|
| id | int | The user ID |

**Query Parameters:**
| Name | Type | Default | Description |
|------|------|---------|-------------|
| includeInfo | bool | false | If true, includes EmployeeInfo and CustomerInfo |

**Success Response (200):**
```json
{
  "success": true,
  "data": {
    "id": 5,
    "username": "counter_user",
    "fullName": "Mohamed Ahmed Ibrahim",
    "imageUrl": null,
    "email": "counter@example.com",
    "role": "Counter",
    "phoneNumber": "01112345678",
    "nationalNumber": "12345678901234",
    "city": "Alexandria",
    "empInfo": { "counterNumber": 1, "serviceId": 3, "branchId": 1 }
  }
}
```

**Frontend Usage Example (axios):**
```javascript
const getUserById = async (userId, includeInfo = true) => {
  const response = await api.get(`/api/auth/${userId}?includeInfo=${includeInfo}`);
  return response.data.data;
};
```

---

#### [GET] /api/auth/get-all
**Purpose:** Get all users in the system (Admin only).  
**Auth required:** Yes — `Admin` role

**Query Parameters:**
| Name | Type | Default | Description |
|------|------|---------|-------------|
| includeInfo | bool | false | Include related employee/customer info |

**Success Response (200):**
```json
{
  "success": true,
  "data": [
    {
      "id": 1, "username": "admin_user", "fullName": "Admin Full Name",
      "email": "admin@example.com", "role": "Admin", "empInfo": null
    }
  ]
}
```

**Frontend Usage Example (axios):**
```javascript
const getAllUsers = async (includeInfo = false) => {
  const response = await api.get(`/api/auth/get-all?includeInfo=${includeInfo}`);
  return response.data.data;
};
```

---

#### [GET] /api/auth/get-by-role
**Purpose:** Get all users filtered by role (Admin only).  
**Auth required:** Yes — `Admin` role

**Query Parameters:**
| Name | Type | Default | Description |
|------|------|---------|-------------|
| role | int/enum | — | `0` = Admin, `1` = Counter, `2` = DoorVerifier, `3` = Customer |
| includeInfo | bool | false | Include related info |

**Frontend Usage Example (axios):**
```javascript
const getUsersByRole = async (role, includeInfo = false) => {
  const response = await api.get(`/api/auth/get-by-role?role=${role}&includeInfo=${includeInfo}`);
  return response.data.data;
};
```

---

#### [PATCH] /api/auth/change-password
**Purpose:** Change the current user's password.  
**Auth required:** Yes — Any authenticated user

**Request Body (JSON):**
```json
{
  "oldPassword*": "string — Required. Current password",
  "newPassword*": "string — Required. Must pass strength validation",
  "confirmNewPassword*": "string — Required. Must match newPassword"
}
```

**Success Response (200):**
```json
{ "success": true, "data": "Password changed successfully.", "errors": null }
```

**Error Responses:**
| Status | When it occurs | Response body |
|--------|---------------|---------------|
| 400 | Old password wrong | `{ "success": false, "errors": "Old password is incorrect." }` |
| 400 | Weak new password | `{ "success": false, "errors": "Password must contain at least one digit." }` |
| 400 | Passwords don't match | `{ "errors": { "ConfirmNewPassword": ["Passwords do not match."] } }` |

**Frontend Usage Example (axios):**
```javascript
const changePassword = async (oldPassword, newPassword, confirmNewPassword) => {
  const response = await api.patch('/api/auth/change-password', {
    oldPassword, newPassword, confirmNewPassword
  });
  return response.data;
};
```

---

#### [PATCH] /api/auth/upload-image
**Purpose:** Upload a profile image for the current user.  
**Auth required:** Yes — Any authenticated user

**Form Data:**
| Field Name | Type | Description |
|-----------|------|-------------|
| imageData | File | The image file to upload |

**Success Response (200):**
```json
{ "success": true, "data": "Image uploaded successfully.", "errors": null }
```

**Frontend Usage Example (axios):**
```javascript
const uploadProfileImage = async (file) => {
  const formData = new FormData();
  formData.append('imageData', file);
  const response = await api.patch('/api/auth/upload-image', formData, {
    headers: { 'Content-Type': 'multipart/form-data' }
  });
  return response.data;
};
```

---

#### [PATCH] /api/auth/update-user
**Purpose:** Update the current user's profile fields.  
**Auth required:** Yes — Any authenticated user

**Request Body (JSON):**
```json
{
  "username": "string — New username",
  "email": "string — New email",
  "phone": "string — New phone number"
}
```

**Success Response (200):**
```json
{ "success": true, "data": "Profile updated successfully.", "errors": null }
```

**Frontend Usage Example (axios):**
```javascript
const updateProfile = async (updates) => {
  const response = await api.patch('/api/auth/update-user', updates);
  return response.data;
};
```

---

#### [PATCH] /api/auth/update-employee/{id}
**Purpose:** Update an employee's assignment (counter number, service, branch). Admin only.  
**Auth required:** Yes — `Admin` role

**Path Parameters:**
| Name | Type | Description |
|------|------|-------------|
| id | int | The employee's user ID |

**Request Body (JSON):**
```json
{
  "counterNumber*": "int — The counter desk number",
  "serviceId*": "int — FK to Service",
  "branchId*": "int — FK to Branch"
}
```

**Success Response (200):**
```json
{ "success": true, "data": "Employee updated successfully.", "errors": null }
```

**Frontend Usage Example (axios):**
```javascript
const updateEmployee = async (employeeUserId, data) => {
  const response = await api.patch(`/api/auth/update-employee/${employeeUserId}`, data);
  return response.data;
};
```

---

#### [DELETE] /api/auth/delete-account
**Purpose:** Soft-delete (deactivate) the current user's account.  
**Auth required:** Yes — Any authenticated user

**Request Body:** None

**Success Response (200):**
```json
{
  "success": true,
  "data": "Your account has been deactivated. It will be permanently deleted after 30 days.",
  "errors": null
}
```

**Frontend Usage Example (axios):**
```javascript
const deleteAccount = async () => {
  const response = await api.delete('/api/auth/delete-account');
  localStorage.clear();
  window.location.href = '/login';
  return response.data;
};
```

---

### 5.2 BRANCH CONTROLLER (`/api/branch`)

---

#### [GET] /api/branch/get-all
**Purpose:** Get all branches.  
**Auth required:** Yes — `Admin` role

**Query Parameters:**
| Name | Type | Default | Description |
|------|------|---------|-------------|
| includeInfo | bool | false | If true, includes employees and tickets for each branch |

**Success Response (200):**
```json
{
  "success": true,
  "data": [
    {
      "id": 1, "name": "Main Branch", "address": "123 Main St, Cairo",
      "contact": "01012345678", "createdAt": "2026-06-01T10:00:00Z",
      "isActive": true, "employees": null, "tickets": null
    }
  ]
}
```

**Frontend Usage Example (axios):**
```javascript
const getAllBranches = async (includeInfo = false) => {
  const response = await api.get(`/api/branch/get-all?includeInfo=${includeInfo}`);
  return response.data.data;
};
```

---

#### [GET] /api/branch/{id}
**Purpose:** Get a specific branch by ID.  
**Auth required:** Yes — `Admin` role

**Frontend Usage Example (axios):**
```javascript
const getBranchById = async (id, includeInfo = true) => {
  const response = await api.get(`/api/branch/${id}?includeInfo=${includeInfo}`);
  return response.data.data;
};
```

---

#### [POST] /api/branch
**Purpose:** Create a new branch. Also creates a DisplayScreen for it automatically.  
**Auth required:** Yes — `Admin` role

**Request Body (JSON):**
```json
{
  "name*": "string — Required. 2–50 characters. Must be unique.",
  "address*": "string — Required. 2–50 characters.",
  "contact*": "string — Required. 2–20 characters. Must be unique."
}
```

**Side Effects:** A `DisplayScreen` is automatically created for the new branch.

**Frontend Usage Example (axios):**
```javascript
const createBranch = async (data) => {
  const response = await api.post('/api/branch', data);
  return response.data;
};
```

---

#### [PATCH] /api/branch/{id}
**Purpose:** Update an existing branch.  
**Auth required:** Yes — `Admin` role

**Frontend Usage Example (axios):**
```javascript
const updateBranch = async (id, data) => {
  const response = await api.patch(`/api/branch/${id}`, data);
  return response.data;
};
```

---

#### [PATCH] /api/branch/toggle-branch/{id}
**Purpose:** Activate or deactivate a branch (toggle IsActive).  
**Auth required:** Yes — `Admin` role

**Request Body:** None

**Frontend Usage Example (axios):**
```javascript
const toggleBranch = async (id) => {
  const response = await api.patch(`/api/branch/toggle-branch/${id}`);
  return response.data;
};
```

---

#### [DELETE] /api/branch/{id}
**Purpose:** Permanently (hard) delete a branch.  
**Auth required:** Yes — `Admin` role

**Side Effects:** The associated DisplayScreen is CASCADE-deleted.

**Frontend Usage Example (axios):**
```javascript
const deleteBranch = async (id) => {
  const response = await api.delete(`/api/branch/${id}`);
  return response.data;
};
```

---

### 5.3 SERVICE CONTROLLER (`/api/service`)

---

#### [GET] /api/service
**Purpose:** Get all services (queue types).  
**Auth required:** Yes — `Admin` role

**Success Response (200):**
```json
{
  "success": true,
  "data": [
    { "name": "Account Opening", "description": "Open a new bank account", "isActive": true }
  ]
}
```

**Frontend Usage Example (axios):**
```javascript
const getAllServices = async () => {
  const response = await api.get('/api/service');
  return response.data.data;
};
```

---

#### [GET] /api/service/{id}
**Purpose:** Get a specific service by ID.  
**Auth required:** Yes — `Admin` role

**Frontend Usage Example (axios):**
```javascript
const getServiceById = async (id) => {
  const response = await api.get(`/api/service/${id}`);
  return response.data.data;
};
```

---

#### [POST] /api/service
**Purpose:** Create a new service type.  
**Auth required:** Yes — `Admin` role

**Request Body (JSON):**
```json
{
  "name*": "string — Required. Service name (unique)",
  "description*": "string — Required. Max 200 characters"
}
```

**Frontend Usage Example (axios):**
```javascript
const createService = async (name, description) => {
  const response = await api.post('/api/service', { name, description });
  return response.data;
};
```

---

#### [PATCH] /api/service/{id}
**Purpose:** Update an existing service.  
**Auth required:** Yes — `Admin` role

**Frontend Usage Example (axios):**
```javascript
const updateService = async (id, data) => {
  const response = await api.patch(`/api/service/${id}`, data);
  return response.data;
};
```

---

#### [DELETE] /api/service/{id}
**Purpose:** Delete a service.  
**Auth required:** Yes — `Admin` role

**Frontend Usage Example (axios):**
```javascript
const deleteService = async (id) => {
  const response = await api.delete(`/api/service/${id}`);
  return response.data;
};
```

---

### 5.4 TICKET CONTROLLER (`/api/ticket`)

---

#### [GET] /api/ticket
**Purpose:** Get all tickets (with service and branch info).  
**Auth required:** Yes — Any authenticated user

**Success Response (200):**
```json
{
  "success": true,
  "data": [
    {
      "id": 1, "customerId": 10, "customerName": null,
      "ticketNumber": "AB12CD34",
      "qrCodeData": "a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6",
      "status": "Waiting", "reservedAt": "2026-06-05T10:00:00Z",
      "calledAt": null, "completedAt": null,
      "isVerifiedAtDoor": false, "verificationTime": null,
      "branchName": "Main Branch", "serviceName": "Account Opening",
      "employeeName": null
    }
  ]
}
```

**Frontend Usage Example (axios):**
```javascript
const getAllTickets = async () => {
  const response = await api.get('/api/ticket');
  return response.data.data;
};
```

---

#### [GET] /api/ticket/{id}
**Purpose:** Get a specific ticket by ID.  
**Auth required:** Yes — Any authenticated user

**Frontend Usage Example (axios):**
```javascript
const getTicketById = async (id) => {
  const response = await api.get(`/api/ticket/${id}`);
  return response.data.data;
};
```

---

#### [POST] /api/ticket
**Purpose:** Create a new queue ticket (book a spot in line).  
**Auth required:** Yes — Any authenticated user

**Request Body (JSON):**
```json
{
  "serviceId*": "int — Required. FK to the service the customer needs",
  "branchId*": "int — Required. FK to the branch the customer will visit"
}
```

**Success Response (200):**
```json
{ "success": true, "data": "Ticket 'AB12CD34' created successfully.", "errors": null }
```

**Frontend Usage Example (axios):**
```javascript
const createTicket = async (serviceId, branchId) => {
  const response = await api.post('/api/ticket', { serviceId, branchId });
  return response.data;
};
```

---

#### [PATCH] /api/ticket/{id}
**Purpose:** Update a ticket (change service or branch). Only the ticket owner can update.  
**Auth required:** Yes — Any authenticated user (must be ticket owner)

**Request Body (JSON):**
```json
{
  "serviceId": "int — New service ID",
  "branchId": "int — New branch ID"
}
```

**Frontend Usage Example (axios):**
```javascript
const updateTicket = async (ticketId, serviceId, branchId) => {
  const response = await api.patch(`/api/ticket/${ticketId}`, { serviceId, branchId });
  return response.data;
};
```

---

#### [DELETE] /api/ticket/{id}
**Purpose:** Delete a ticket (hard delete).  
**Auth required:** Yes — Any authenticated user

**Frontend Usage Example (axios):**
```javascript
const deleteTicket = async (ticketId) => {
  const response = await api.delete(`/api/ticket/${ticketId}`);
  return response.data;
};
```

---

### 5.5 DISPLAY SCREEN CONTROLLER (`/api/displayscreen`)

---

#### [GET] /api/displayscreen/{branchId}
**Purpose:** Get the current display screen data for a branch.  
**Auth required:** Yes — Any authenticated user

**Path Parameters:**
| Name | Type | Description |
|------|------|-------------|
| branchId | int | The branch ID |

**Success Response (200):**
```json
{
  "success": true,
  "data": {
    "id": 0, "branchName": "Main Branch", "lastUpdated": null,
    "displayTickets": [
      { "id": 1, "ticketNumber": "AB12CD34", "counterNumber": 3, "calledAt": "2026-06-05T10:30:00Z" }
    ]
  }
}
```

**Frontend Usage Example (axios):**
```javascript
const getDisplayScreenData = async (branchId) => {
  const response = await api.get(`/api/displayscreen/${branchId}`);
  return response.data.data;
};
```

---

## 6. FEATURE MODULES (CHUNKS)

### 6.1 Authentication Module
Handles user registration, login, token management, and logout.

| Method | Path | Description |
|--------|------|-------------|
| POST | /api/auth/register-customer | Public customer registration |
| POST | /api/auth/register-admin | Register new admin (Admin only) |
| POST | /api/auth/register-counter | Register counter employee (Admin only) |
| POST | /api/auth/register-door-verifier | Register door verifier (Admin only) |
| POST | /api/auth/login | Authenticate and get tokens |
| POST | /api/auth/refresh | Refresh access token |
| POST | /api/auth/logout | Revoke refresh token |

**Recommended call order:** register → login → store tokens → refresh before expiry → logout

**Business Rules:** Email, username, national number must be unique. Refresh tokens are single-use. Reuse triggers full revocation.

### 6.2 User / Profile Module
Manages user profiles, password changes, image uploads, and account deactivation.

| Method | Path | Description |
|--------|------|-------------|
| GET | /api/auth/profile | Get current user's profile |
| GET | /api/auth/{id} | Get user by ID (Admin) |
| GET | /api/auth/get-all | Get all users (Admin) |
| GET | /api/auth/get-by-role | Get users by role (Admin) |
| PATCH | /api/auth/update-user | Update own profile |
| PATCH | /api/auth/change-password | Change own password |
| PATCH | /api/auth/upload-image | Upload profile image |
| PATCH | /api/auth/update-employee/{id} | Update employee assignment (Admin) |
| DELETE | /api/auth/delete-account | Soft-delete own account |

**Side Effects:** delete-account performs soft-delete. upload-image saves to wwwroot/images/users/.

### 6.3 Branch Management Module (Admin only)

| Method | Path | Description |
|--------|------|-------------|
| GET | /api/branch/get-all | List all branches |
| GET | /api/branch/{id} | Get branch details |
| POST | /api/branch | Create new branch (+auto DisplayScreen) |
| PATCH | /api/branch/{id} | Update branch |
| PATCH | /api/branch/toggle-branch/{id} | Toggle active/inactive |
| DELETE | /api/branch/{id} | Hard delete branch (+CASCADE DisplayScreen) |

### 6.4 Service Management Module (Admin only)

| Method | Path | Description |
|--------|------|-------------|
| GET | /api/service | List all services |
| GET | /api/service/{id} | Get service details |
| POST | /api/service | Create new service |
| PATCH | /api/service/{id} | Update service |
| DELETE | /api/service/{id} | Delete service |

**Constraint:** Each service can only be assigned to ONE counter employee.

### 6.5 Queue / Ticket Module
The core queue flow — customers book, door verifiers verify, counters serve.

| Method | Path | Description |
|--------|------|-------------|
| GET | /api/ticket | List all tickets |
| GET | /api/ticket/{id} | Get ticket details |
| POST | /api/ticket | Book a new ticket |
| PATCH | /api/ticket/{id} | Update ticket |
| DELETE | /api/ticket/{id} | Delete ticket |
| POST | /api/auth/door-verifying | Verify ticket QR at door (DoorVerifier) |
| POST | /api/auth/call-next | Call next ticket (Counter) |
| POST | /api/auth/complete/{id} | Complete ticket (Counter) |

**Flow:** Book → Verify at door → Call next → Complete. Unverified tickets get requeued.

### 6.6 Display Screen Module

| Method | Path | Description |
|--------|------|-------------|
| GET | /api/displayscreen/{branchId} | Get display screen data |

Combine with SignalR `TicketCalled` events for real-time updates.

---

## 7. FILE UPLOAD ENDPOINTS

| Property | Value |
|----------|-------|
| **Endpoint** | `PATCH /api/auth/upload-image` |
| **Method** | `multipart/form-data` |
| **Field Name** | `imageData` |
| **Accepted MIME Types** | Any image file (no server-side restriction) |
| **Max File Size** | No explicit server limit configured |
| **Saved Location** | `wwwroot/images/users/{guid}.{extension}` |
| **Image URL Format** | `/images/users/{guid}.{extension}` |

> The API does NOT return the image URL in the upload response. Fetch the profile again after uploading to get the updated `imageUrl`.

---

## 8. REAL-TIME / SIGNALR

### 8.1 Hub Configuration

| Property | Value |
|----------|-------|
| **Hub URL** | `/hubs/queue` |
| **Full URL** | `https://localhost:7XXX/hubs/queue` |
| **Authentication** | JWT token via query string `?access_token=<token>` |

### 8.2 Client → Server Methods

| Method | Parameters | Description |
|--------|-----------|-------------|
| `JoinBranch` | `branchId: int` | Join a branch group to receive its events |
| `LeaveBranch` | `branchId: int` | Leave a branch group |

### 8.3 Server → Client Events

#### `TicketCalled`
**Triggered when:** A counter employee calls `POST /api/auth/call-next`

```json
{
  "ticketNumber": "AB12CD34",
  "counterNumber": 3,
  "serviceName": "Account Opening",
  "calledAt": "2026-06-05T10:30:00Z"
}
```

#### `TicketVerified`
**Triggered when:** A door verifier scans a QR code via `POST /api/auth/door-verifying`

```json
{
  "ticketNumber": "AB12CD34",
  "verifiedAt": "2026-06-05T10:25:00Z"
}
```

### 8.4 Complete React.js Connection Example

```bash
npm install @microsoft/signalr
```

```javascript
import * as signalR from '@microsoft/signalr';

class QueueHubService {
  constructor() {
    this.connection = null;
  }

  async start(branchId) {
    const token = localStorage.getItem('accessToken');

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(`${process.env.REACT_APP_SIGNALR_HUB_URL}?access_token=${token}`, {
        skipNegotiation: true,
        transport: signalR.HttpTransportType.WebSockets,
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(signalR.LogLevel.Information)
      .build();

    this.connection.on('TicketCalled', (notification) => {
      console.log('Ticket Called:', notification);
    });

    this.connection.on('TicketVerified', (notification) => {
      console.log('Ticket Verified:', notification);
    });

    this.connection.onreconnected(() => {
      this.connection.invoke('JoinBranch', branchId);
    });

    try {
      await this.connection.start();
      await this.connection.invoke('JoinBranch', branchId);
    } catch (error) {
      console.error('SignalR connection failed:', error);
      setTimeout(() => this.start(branchId), 5000);
    }
  }

  async stop(branchId) {
    if (this.connection) {
      try { await this.connection.invoke('LeaveBranch', branchId); } catch {}
      await this.connection.stop();
    }
  }
}

export const queueHub = new QueueHubService();
```

---

## 9. BACKGROUND JOBS & SCHEDULED TASKS

> **N/A — No background jobs or scheduled tasks are implemented in this project.**

Note: The code mentions that soft-deleted accounts should be hard-deleted after 30 days, but this is not yet automated.

---

## 10. VALIDATION RULES

### CreateUser / RegisterCustomer / RegisterAdmin DTO
| Field | Type | Rules |
|-------|------|-------|
| fullName | string | Required. Custom `[FullName]`: must have >= 3 space-separated names |
| username | string | Required. Min 8, Max 30 chars |
| email | string | Required. `[EmailAddress]`. Min 10, Max 50 chars |
| phoneNumber | string | Required. Regex: `^(010|011|012|015)\d{8}$` |
| nationalNumber | string | Required. Exactly 14 chars |
| password | string | Required. Server: Min 8, 1 upper, 1 lower, 1 digit, 1 of `@$!%*?&_` |
| confirmPassword | string | Required. `[Compare("Password")]` |
| city | string | Required. Min 2, Max 30 chars |

### CreateCounter DTO
Same as CreateUser plus: `counterNumber` (int, required), `serviceId` (int, required), `branchId` (int, required)

### CreateDoorVerifier DTO
Same as CreateUser plus: `branchId` (int, required)

### ChangePassword DTO
| Field | Rules |
|-------|-------|
| oldPassword | Required |
| newPassword | Required. Same password rules |
| confirmNewPassword | Required. `[Compare("NewPassword")]` |

### CreateBranch / UpdateBranch DTO
| Field | Rules |
|-------|-------|
| name | Required. Min 2, Max 50 chars |
| address | Required. Min 2, Max 50 chars |
| contact | Required. Min 2, Max 20 chars |

### CreateService / UpdateService DTO
| Field | Rules |
|-------|-------|
| name | Required |
| description | Required. Max 200 chars |

### CreateTicket / UpdateTicket DTO
| Field | Rules |
|-------|-------|
| serviceId | Required (int) |
| branchId | Required (int) |

### Validation Error Format
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "FullName": ["Full name must contain at least 3 names."],
    "Email": ["The Email field is not a valid e-mail address."]
  }
}
```

---

## 11. CORS & ENVIRONMENT CONFIGURATION

| Setting | Value |
|---------|-------|
| **Allowed Origins** | All origins (`SetIsOriginAllowed(_ => true)`) |
| **Allowed Headers** | All (`AllowAnyHeader()`) |
| **Allowed Methods** | All (`AllowAnyMethod()`) |
| **Allow Credentials** | Yes (`AllowCredentials()`) |

| Setting | Development | Production |
|---------|------------|------------|
| Swagger UI | Available at `/swagger` | Not available |
| CORS | All origins | Should be restricted |
| HTTPS | Required | Required |

---

## 12. THIRD-PARTY INTEGRATIONS

> **N/A — No third-party services.** All functionality (password hashing, JWT, file storage, real-time) is self-contained.

---

## 13. QUICK REFERENCE CHEAT SHEET

| Module | Method | Path | Auth | Role | Description |
|--------|--------|------|------|------|-------------|
| Auth | POST | /api/auth/register-customer | No | — | Register a customer |
| Auth | POST | /api/auth/login | No | — | Login and get tokens |
| Auth | POST | /api/auth/refresh | No | — | Refresh access token |
| Auth | POST | /api/auth/logout | Yes | Any | Revoke refresh token |
| Auth | POST | /api/auth/register-admin | Yes | Admin | Register an admin |
| Auth | POST | /api/auth/register-counter | Yes | Admin | Register counter employee |
| Auth | POST | /api/auth/register-door-verifier | Yes | Admin | Register door verifier |
| Users | GET | /api/auth/profile | Yes | Any | Get own profile |
| Users | GET | /api/auth/{id} | Yes | Admin | Get user by ID |
| Users | GET | /api/auth/get-all | Yes | Admin | Get all users |
| Users | GET | /api/auth/get-by-role | Yes | Admin | Get users by role |
| Users | PATCH | /api/auth/update-user | Yes | Any | Update own profile |
| Users | PATCH | /api/auth/change-password | Yes | Any | Change own password |
| Users | PATCH | /api/auth/upload-image | Yes | Any | Upload profile image |
| Users | PATCH | /api/auth/update-employee/{id} | Yes | Admin | Update employee assignment |
| Users | DELETE | /api/auth/delete-account | Yes | Any | Soft-delete own account |
| Queue | POST | /api/auth/door-verifying | Yes | DoorVerifier | Verify ticket QR at door |
| Queue | POST | /api/auth/call-next | Yes | Counter | Call next waiting ticket |
| Queue | POST | /api/auth/complete/{id} | Yes | Counter | Mark ticket as completed |
| Branch | GET | /api/branch/get-all | Yes | Admin | List all branches |
| Branch | GET | /api/branch/{id} | Yes | Admin | Get branch details |
| Branch | POST | /api/branch | Yes | Admin | Create branch |
| Branch | PATCH | /api/branch/{id} | Yes | Admin | Update branch |
| Branch | PATCH | /api/branch/toggle-branch/{id} | Yes | Admin | Toggle branch active status |
| Branch | DELETE | /api/branch/{id} | Yes | Admin | Delete branch |
| Service | GET | /api/service | Yes | Admin | List all services |
| Service | GET | /api/service/{id} | Yes | Admin | Get service details |
| Service | POST | /api/service | Yes | Admin | Create service |
| Service | PATCH | /api/service/{id} | Yes | Admin | Update service |
| Service | DELETE | /api/service/{id} | Yes | Admin | Delete service |
| Ticket | GET | /api/ticket | Yes | Any | List all tickets |
| Ticket | GET | /api/ticket/{id} | Yes | Any | Get ticket details |
| Ticket | POST | /api/ticket | Yes | Any | Book a new ticket |
| Ticket | PATCH | /api/ticket/{id} | Yes | Any | Update ticket |
| Ticket | DELETE | /api/ticket/{id} | Yes | Any | Delete ticket |
| Display | GET | /api/displayscreen/{branchId} | Yes | Any | Get display screen data |

---

## 14. FRONTEND INTEGRATION CHECKLIST

- [ ] Set `REACT_APP_API_BASE_URL` in `.env` file
- [ ] Set `REACT_APP_SIGNALR_HUB_URL` in `.env` file
- [ ] Install `@microsoft/signalr` package
- [ ] Install `axios` package
- [ ] Create Axios instance with `baseURL`
- [ ] Implement token storage in `localStorage` (`accessToken`, `refreshToken`)
- [ ] Implement Axios request interceptor to attach `Authorization: Bearer <token>`
- [ ] Implement Axios response interceptor (401 → refresh → retry → login redirect)
- [ ] Implement auto-refresh timer after login
- [ ] Create SignalR service connecting to `/hubs/queue` with token via query string
- [ ] Handle SignalR reconnection (re-join branch group)
- [ ] Handle file uploads using `FormData` and `multipart/form-data`
- [ ] Parse two error formats (service layer errors vs validation errors)
- [ ] Install `jwt-decode` and decode JWT to extract role for route guards
- [ ] Build role-based navigation (Admin / Counter / DoorVerifier / Customer)
- [ ] Build display screen page combining REST + SignalR events
- [ ] Install `qrcode.react` to generate QR codes from `ticket.qrCodeData`
- [ ] Handle soft-delete UX (deactivation message → redirect to login)

---

## 15. APPLICATION WORKFLOW — COMPLETE USER JOURNEYS

---

### 15.1 Customer Journey

1. **Register:** `POST /api/auth/register-customer` with full name, email, phone, etc.
2. **Login:** `POST /api/auth/login` → store `accessToken` + `refreshToken`
3. **Browse:** View available branches and services (needs admin to share or API update)
4. **Book ticket:** `POST /api/ticket` with `{ serviceId, branchId }` → receive ticket with QR data
5. **View ticket:** `GET /api/ticket` → find your ticket → generate QR code client-side from `qrCodeData`
6. **Go to branch:** Show QR code at door → door verifier scans it
7. **Wait:** Listen for `TicketCalled` SignalR event for your ticket number
8. **Get served:** Go to the designated counter
9. **Done:** Counter marks ticket as completed

### 15.2 Counter Employee Journey

1. **Login:** `POST /api/auth/login`
2. **Get profile:** `GET /api/auth/profile` → get `empInfo.branchId`, `counterNumber`, `serviceId`
3. **Connect SignalR:** Join branch group to receive real-time updates
4. **Call next:** `POST /api/auth/call-next` → auto-finds next verified waiting ticket for your service
5. **Serve customer:** Customer comes to your counter
6. **Complete:** `POST /api/auth/complete/{ticketId}`
7. **Repeat** step 4

**Key behavior:** The employee does NOT select which service to serve — they are pre-assigned by the admin. The system automatically pulls the next ticket for their assigned service.

### 15.3 Door Verifier Journey

1. **Login:** `POST /api/auth/login`
2. **Get profile:** `GET /api/auth/profile` → get `empInfo.branchId`
3. **Open QR scanner** (use `react-qr-reader` or `html5-qrcode` library)
4. **Scan customer's QR:** Extract QR data string
5. **Verify:** `POST /api/auth/door-verifying` with `{ qrCode: scannedData }`
6. **Show result:** Success or error message
7. **Repeat** step 4

### 15.4 Admin Journey

**Setting up a new branch:**
1. `POST /api/branch` → create branch (auto-creates display screen)
2. `POST /api/service` → create services (e.g., "Account Opening", "Cash Deposit")
3. `POST /api/auth/register-counter` → create counter employees (assign to service + branch + counter number)
4. `POST /api/auth/register-door-verifier` → create door verifier (assign to branch)

**Managing employees:**
1. `GET /api/auth/get-by-role?role=1` → list all counter employees
2. `PATCH /api/auth/update-employee/{id}` → reassign counter/service/branch

**Monitoring:**
1. `GET /api/auth/get-all?includeInfo=true` → view all users
2. `GET /api/ticket` → view all tickets across system
3. `GET /api/displayscreen/{branchId}` → view branch display

### 15.5 Ticket Status Lifecycle

```
[Customer Books] → Waiting
[Counter Calls + Verified] → Serving
[Counter Calls + NOT Verified] → Requeued → (re-enters queue as Waiting)
[Counter Completes] → Completed
```

| Status | Description | Set By |
|--------|-------------|--------|
| `Waiting` | Initial — ticket in queue | System (on create) |
| `Serving` | Counter is serving this customer | System (on call-next, if verified) |
| `Completed` | Service finished | Counter (on complete) |
| `Requeued` | Customer not verified, pushed back | System (on call-next, if not verified) |
| `Skipped` | Reserved for future use | — |

### 15.6 Recommended React.js App Structure

```
src/
├── api/
│   ├── axiosInstance.js       # Configured Axios with interceptors
│   ├── authApi.js             # Login, register, refresh, logout
│   ├── branchApi.js           # Branch CRUD
│   ├── serviceApi.js          # Service CRUD
│   ├── ticketApi.js           # Ticket CRUD + queue operations
│   ├── userApi.js             # User management
│   └── displayApi.js          # Display screen data
├── services/
│   └── signalrService.js      # SignalR hub connection
├── context/
│   └── AuthContext.js         # Auth state (token, user, role)
├── hooks/
│   ├── useAuth.js             # Auth hook
│   └── useSignalR.js          # SignalR connection hook
├── pages/
│   ├── auth/
│   │   ├── LoginPage.jsx
│   │   └── RegisterPage.jsx
│   ├── customer/
│   │   ├── BookTicketPage.jsx
│   │   ├── MyTicketsPage.jsx
│   │   └── TicketQRPage.jsx
│   ├── counter/
│   │   └── CounterDashboard.jsx
│   ├── doorVerifier/
│   │   └── QRScannerPage.jsx
│   ├── admin/
│   │   ├── DashboardPage.jsx
│   │   ├── BranchesPage.jsx
│   │   ├── ServicesPage.jsx
│   │   ├── EmployeesPage.jsx
│   │   └── UsersPage.jsx
│   ├── display/
│   │   └── DisplayScreenPage.jsx
│   └── shared/
│       └── ProfilePage.jsx
├── components/
│   ├── ProtectedRoute.jsx     # Role-based route guard
│   ├── QRCodeGenerator.jsx    # Generate QR from qrCodeData
│   └── QRScanner.jsx          # Camera-based QR reader
└── App.jsx                    # Routing setup
```

---

> **This document covers every endpoint, every DTO, every validation rule, every SignalR event, and every workflow in the Queue Management System backend. A React.js developer should be able to build the complete frontend using ONLY this reference.**
