# 🧪 OakERP Testing Guide 

This guide details the testing strategy for OakERP, ensuring code correctness, reliability, and maintainability. OakERP employs a layered testing approach, dividing tests into two main projects: **Unit Tests** and **Integration Tests**. This document covers the what, why, and how, enabling developers to write effective tests quickly.

---

## 📂 Project Overview

OakERP organizes its tests into two distinct projects:

| Project | Purpose | Dependencies |
|---------|---------|--------------|
| **OakERP.Tests.Unit** | Tests business logic and services in isolation, mocking all external dependencies (e.g., DB, network). | No DB or network access. |
| **OakERP.Tests.Integration** | Tests the full Web API stack, including real PostgreSQL database and cross-service behaviors. | Requires a running PostgreSQL DB (preferably via Docker). |

### 🧩 Project Layout
```
/OakERP.Tests.Unit           # Pure logic/service tests (no DB or network)
/OakERP.Tests.Integration    # Full API & DB tests (with real Postgres, Docker recommended)
```

---

## 1️⃣ Unit Tests

### 🧐 Purpose
- **Test business logic and services in isolation.**
- Mock all external dependencies (e.g., database, external services).
- Run quickly without requiring a database or HTTP calls.

### 📋 What Gets Tested?
- Service classes (e.g., `AuthService` for login, register).
- Pure logic functions and utilities.
- Any component with dependencies that can be mocked (e.g., `UserManager`, `SignInManager`, `JwtGenerator`, `DbContext`, repositories).

### 🛠 How Do They Work?
- **Libraries**: Use `xUnit` for test execution, `Moq` for mocking dependencies, and `Shouldly` for assertions.
- **Setup**: Configure mocks in test class constructors or setup methods.
- **Execution**: Call the service/logic and assert the results (e.g., Dtos, success/failure states).
- **No DB/network**: All external interactions are mocked to ensure speed and isolation.

### 🧑‍💻 Example
```csharp
[Fact]
public async Task RegisterAsync_Should_Fail_When_Passwords_Do_Not_Match()
{
    // Arrange
    var Dto = new RegisterDto { Username = "test", Password = "pass1", ConfirmPassword = "pass2" };
    var authService = new AuthService(/* mocked dependencies */);

    // Act
    var result = await authService.RegisterAsync(Dto);

    // Assert
    result.Success.ShouldBeFalse();
}
```

### 🚀 Run Unit Tests
Run it via inside Visual studio or via command line:

```bash
dotnet test OakERP.Tests.Unit
```

### 📜 Guidelines
- Mock **all** external dependencies (e.g., `DbContext`, `UserManager`).
- Use realistic inputs to test edge cases and typical scenarios.
- Avoid database reads/writes in unit tests.
- Update test constructors when new dependencies are injected into services.

---

## 2️⃣ Integration Tests

### 🧐 Purpose
- **Test the full API stack**: HTTP endpoints, real PostgreSQL database, controllers, middleware, and cross-service interactions.
- Validate end-to-end flows, including authorization, persistence, and side effects.
- Ensure the system behaves correctly under real-world conditions.

### 📋 What Gets Tested?
- API endpoints (e.g., `/api/auth/register`, `/api/auth/login`).
- End-to-end flows involving HTTP requests, database operations, and JWT tokens.
- Scenarios requiring real database behavior (e.g., EF Core queries, cascade deletes).
- Licensing, Identity framework, and token generation.

### 🛠 How Do They Work?
- **Test Server**: Use `WebApplicationFactory` to spin up an in-memory test server.
- **Database**: Connect to a real PostgreSQL database (preferably via Docker for consistency).
- **HTTP Client**: Simulate real client requests using `HttpClient`.
- **Cleanup**: Use custom helpers (e.g., `PostAndMarkAsync`, `MarkForCleanup`) to mark test data for removal, ensuring a clean database after each test.
- **Libraries**: Use `NUnit` for better lifecycle control (`SetUp`/`TearDown`) and `Shouldly` for assertions.

### 🧑‍💻 Example
```csharp
[Test]
public async Task Register_Endpoint_Should_Create_Tenant_And_License()
{
    // Arrange
    var Dto = new RegisterDto { TenantName = "TestTenant", Username = "test", Password = "pass" };

    // Act
    var result = await PostAndMarkAsync<RegisterDto, AuthResultDto, Tenant>(
        ApiRoutes.Auth.Register,
        Dto,
        (req, res) => DbContext.Tenants.FirstOrDefault(t => t.Name == req.TenantName)
    );

    // Assert
    result.Success.ShouldBeTrue();
    DbContext.Tenants.ShouldContain(t => t.Name == Dto.TenantName);
}
```

### 🧩 Test Utilities
- **`ApiTestException`**: Throws descriptive errors for failed HTTP/API calls.
- **`PostAndMarkAsync`**: Sends POST requests and marks created entities for cleanup.
- **`MarkForCleanup`**: Marks database entities for removal after tests.
- **`WebApiIntegrationTestBase`**: Base class for setting up HTTP client, DB context, and cleanup logic.
- **`PersistentDbFixture`**: Ensures reliable cleanup of test data.
- **`ApiRoutes`**: Centralized class for endpoint paths (e.g., `ApiRoutes.Auth.Login`).

### 🚀 Run Integration Tests
1. Ensure the PostgreSQL database is running (e.g., via Docker Compose):
   ```bash
   docker compose up
   ```
2. Run tests:
   ```bash
   dotnet test OakERP.Tests.Integration
   ```

### 📜 Guidelines
- Always clean up test data using `MarkForCleanup` or `PostAndMarkAsync`.
- Use `ApiRoutes` for endpoint paths to avoid hardcoding.
- Prefer integration tests for scenarios involving EF Core, Identity, licensing, or tokens.
- Avoid mocking `DbSet` or database queries; use a real PostgreSQL database.
- Ensure the test database is running on `localhost` or configured correctly.

---

## ⚙️ Marking Entities for Cleanup
To prevent database pollution and ensure repeatable tests, use:
- **`MarkForCleanup(entity)`**: Marks a single entity for deletion after the test.
- **`PostAndMarkAsync`**: Combines POST requests with automatic cleanup of created entities.

This ensures tests are isolated and the database remains clean.

---

## 🎯 Guiding Principles
- **Separation**: Keep unit and integration tests distinct. Unit tests focus on logic; integration tests validate APIs and DB.
- **Repeatability**: Tests must leave no leftover data and always start with a clean state.
- **Mock Sparingly**: Mock dependencies in unit tests; use real components (e.g., DB, API) in integration tests.
- **Documentation**: Write tests that clearly demonstrate expected behavior, serving as living documentation.
- **Fail Fast, Fail Clear**: Assert critical behaviors and throw descriptive errors when tests fail.
- **Developer Experience**: Use test helpers to minimize boilerplate and improve maintainability.

---

## 🧭 When to Use What
| Use Case | Test Type | Notes |
|----------|-----------|-------|
| Test `AuthService.LoginAsync()` | Unit | Pure logic; mock `SignInManager`, `UserManager`. |
| Test `/api/auth/login` endpoint | Integration | Validates real API, DB, and JWT token. |
| Check DB cascade behavior | Integration | Requires real DB; avoid mocks. |
| Validate Dto validation rules | Unit or Integration | Unit for logic-only; Integration for API-returned Dtos. |
| Test EF Core queries | Integration | Use real PostgreSQL; avoid mocking `DbSet`. |

---

## 🏁 Getting Started
1. **Choose the Right Project**:
   - **Unit Tests**: For pure business logic or service methods.
   - **Integration Tests**: For API endpoints, database interactions, or end-to-end flows.
2. **Set Up Environment**:
   - For integration tests, ensure PostgreSQL is running via Docker:
     ```bash
     docker compose up
     ```
   - Verify connection strings and DB permissions.
3. **Use Test Helpers**:
   - Leverage `WebApiIntegrationTestBase` for integration test setup/teardown.
   - Use `PostAndMarkAsync`, `MarkForCleanup`, and other utilities to simplify testing.
4. **Run Tests**:
   - Unit tests:
     ```bash
     dotnet test OakERP.Tests.Unit
     ```
   - Integration tests:
     ```bash
     dotnet test OakERP.Tests.Integration
     ```
5. **Write New Tests**:
   - Follow the examples above.
   - Use base classes and helpers to reduce boilerplate.
   - Mark created entities for cleanup in integration tests.
6. **Run Tests Often**:
   - Tests should pass locally and in CI.
   - CI runs both `OakERP.Tests.Unit` and `OakERP.Tests.Integration`.

---

## 🆘 Troubleshooting
- **DB errors in integration tests?**
  - Ensure Docker is running and the test database is accessible.
  - Check connection strings and DB permissions.
- **Tests fail on CI but pass locally?**
  - Verify connection strings, DB permissions, and avoid hard-coded data.
  - Ensure Docker setup matches CI environment.
- **Need new test helpers?**
  - Add them to `OakERP.Tests.Integration.TestSetup` or relevant test utilities.
- **Mocking issues in unit tests?**
  - Ensure mocks reflect realistic behavior, including edge cases.
  - Update constructors if new dependencies are added.

---

## 🧼 Design Philosophy
- **Unit tests protect logic**: They ensure individual components work as expected.
- **Integration tests protect contracts**: They validate real-world behavior and system integration.
- **Isolated and ephemeral data**: Tests must not interfere with each other.
- **Realistic mocks**: Mocks should simulate real behavior, not just happy paths.
- **Developer-first**: Helpers and base classes reduce boilerplate and improve test maintainability.

---

## 📝 Contributing
- Add new tests to the appropriate project (`Unit` or `Integration`).
- Update this guide (`README-tests.md`) if the test setup evolves.
- Share new test helpers or utilities with the team to improve testing efficiency.
- Ensure all tests pass in CI and follow the guiding principles.

---

## 🚀 Final Notes
- Keep this updated as the test setup evolves.
- Run tests frequently to maintain confidence in the codebase.

Happy testing! 🎉
