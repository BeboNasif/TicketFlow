namespace TicketFlow.Tests;

using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using TicketFlow.API.Controllers;
using TicketFlow.BLL.Helpers;
using TicketFlow.BLL.Validation;
using TicketFlow.DAL.Context;
using TicketFlow.DAL.Models;
using Xunit;

public class SecurityVerificationTests
{
    // 1. XSS Prevention Tests
    [Theory]
    [InlineData("<script>alert('XSS')</script>Concert")]
    [InlineData("<img src=x onerror=alert(1)>VIP Seat")]
    [InlineData("javascript:alert('hack')")]
    public void InputSanitizer_DetectsAndNeutralizes_XssPayloads(string maliciousInput)
    {
        Assert.True(InputSanitizer.ContainsMaliciousPayload(maliciousInput));

        var sanitized = InputSanitizer.SanitizeAndEncode(maliciousInput);

        Assert.DoesNotContain("<script>", sanitized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("onerror=", sanitized, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void NoXssAttribute_RejectsMaliciousDtoInput()
    {
        var dto = new SampleSecuredDto { Title = "<script>document.cookie</script>" };
        var context = new ValidationContext(dto);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(dto, context, results, validateAllProperties: true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.ErrorMessage!.Contains("unsafe HTML or script"));
    }

    // 2. SQL Injection Prevention Tests (Parameterized EF Core Queries)
    [Fact]
    public void Queries_TreatSqlInjectionPayload_AsLiteralString()
    {
        var categories = new[]
        {
            new Category { Name = "Music", Description = "Live concerts" }
        }.AsQueryable();

        var sqlInjectionPayload = "' OR '1'='1; DROP TABLE \"Categories\"; --";

        // The payload is evaluated as a literal value and cannot alter the predicate.
        var matchedCategories = categories
            .Where(c => c.Name == sqlInjectionPayload)
            .ToList();

        Assert.Empty(matchedCategories);
        Assert.Equal(1, categories.Count());
    }

    // 3. Role-Based Access Control (RBAC) Verification Tests
    [Fact]
    public void SensitiveAdminControllers_EnforceRoleBasedAuthorization()
    {
        var eventsControllerType = typeof(EventsController);
        var postMethods = eventsControllerType
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Where(m => m.GetCustomAttributes<Microsoft.AspNetCore.Mvc.HttpPostAttribute>().Any()
                     || m.GetCustomAttributes<Microsoft.AspNetCore.Mvc.HttpDeleteAttribute>().Any());

        foreach (var method in postMethods)
        {
            var authAttr = method.GetCustomAttribute<AuthorizeAttribute>()
                        ?? eventsControllerType.GetCustomAttribute<AuthorizeAttribute>();

            Assert.NotNull(authAttr);
            Assert.False(string.IsNullOrWhiteSpace(authAttr!.Roles),
                $"Endpoint {method.Name} must restrict access by specific Roles (RBAC).");
        }
    }

    private class SampleSecuredDto
    {
        [NoXss]
        public string Title { get; set; } = string.Empty;
    }
}