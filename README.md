# TicketFlow API — Secure Event Ticketing & Seat Reservation System

A Clean Architecture ASP.NET Core Web API built with PostgreSQL, Entity Framework Core, JWT Authentication, Role-Based Access Control (RBAC), and SignalR real-time seat locking.

---

## Security & Vulnerability Summary

### 1. Vulnerabilities Identified & Resolved
* **SQL Injection (SQLi):** Raw string concatenation in database lookups can allow attackers to bypass filters or manipulate tables.
  * **Fix Applied:** Enforced **Entity Framework Core LINQ & Npgsql parameterized queries** across all repositories (`ReservationRepository`, `EventRepository`, `OrderRepository`) so all user inputs are strictly bound as parameters.
* **Cross-Site Scripting (XSS):** Unfiltered HTML or `<script>` tags in event titles, descriptions, or user registration names could execute malicious JavaScript in client browsers.
  * **Fix Applied:** Implemented `InputSanitizer` (using `HtmlEncoder.Default.Encode` and regex tag/event-handler stripping) alongside a custom `[NoXss]` Data Annotation validation attribute on input DTOs.
* **Broken Access Control / Unauthorized Seat Tampering:** Public endpoints or weak session checks could allow regular users to create/delete events or hijack another user's pending reservation.
  * **Fix Applied:** Configured **ASP.NET Core Identity + JWT Bearer Authentication** with strict **Role-Based Access Control (`[Authorize(Roles = "Admin,Organizer")]`)** on administrative endpoints, ownership checks (`reservation.UserId == userId`) on checkout/cancellation, and centralized `GlobalExceptionHandlerMiddleware` returning RFC 7807 `ProblemDetails` without leaking stack traces.

### 2. How Microsoft Copilot Assisted
* **Secure Validation & Sanitization:** Assisted in generating the regex patterns in `InputSanitizer` and the custom `[NoXss]` `ValidationAttribute` to block inline event handlers (`onerror=`) and script tags.
* **Authentication & RBAC Configuration:** Helped scaffold JWT Bearer token validation parameters, Swagger security definitions, and role-restricted controller policies.
* **Security Unit Testing:** Accelerated writing parameterized xUnit `[Theory]` test cases in `TicketFlow.Tests` to verify XSS payload neutralization, SQLi literal handling, and reflection-based RBAC attribute checks.

---

## Running the Project & Security Tests

```bash
# Run the API
dotnet run --project TicketFlow.API

# Execute Security Verification Tests
dotnet test