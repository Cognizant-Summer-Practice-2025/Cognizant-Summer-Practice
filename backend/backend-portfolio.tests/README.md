# Backend Portfolio Tests

This project contains unit tests for the backend-portfolio service.

## Test Structure

- **Controllers**: Tests for API controllers
- **Services**: Tests for business logic services
- **Repositories**: Tests for data access repositories
- **Models**: Tests for entity models
- **DTO**: Tests for data transfer objects
- **Helpers**: Test utilities and helper classes

## Running Tests

To run all tests:
```bash
dotnet test
```

To run tests with coverage:
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## Test Dependencies

- xUnit for test framework
- Moq for mocking
- FluentAssertions for readable assertions
- AutoFixture for test data generation
- Microsoft.EntityFrameworkCore.InMemory for database testing 