using NetArchTest.Rules;
using SupportDesk.Api.Domain;

namespace SupportDesk.Api.Tests;

// The layers live in one project, so the compiler cannot stop a reference in the wrong direction.
// These tests inspect the compiled IL (method bodies included) and fail when a layer depends on
// something it must not know about.
public sealed class ArchitectureTests
{
    private const string Domain = "SupportDesk.Api.Domain";
    private const string Services = "SupportDesk.Api.Services";
    private const string Controllers = "SupportDesk.Api.Controllers";
    private const string Contracts = "SupportDesk.Api.Contracts";
    private const string Data = "SupportDesk.Api.Data";
    private const string Repositories = "SupportDesk.Api.Repositories";
    private const string ErrorHandling = "SupportDesk.Api.ErrorHandling";
    private const string EntityFrameworkCore = "Microsoft.EntityFrameworkCore";
    private const string AspNetCore = "Microsoft.AspNetCore";

    private static readonly Types ApiTypes = Types.InAssembly(typeof(Ticket).Assembly);

    [Fact]
    public void Domain_DependsOnNoOtherLayerOrFramework()
    {
        AssertNoDependency(Domain, Services, Controllers, Contracts, Data, Repositories, ErrorHandling, EntityFrameworkCore, AspNetCore);
    }

    [Fact]
    public void Services_DependOnlyOnDomainAndTheirOwnPorts()
    {
        AssertNoDependency(Services, Controllers, Contracts, Data, Repositories, ErrorHandling, EntityFrameworkCore, AspNetCore);
    }

    [Fact]
    public void ControllersAndContracts_DoNotReachPersistence()
    {
        AssertNoDependency(Controllers, Data, Repositories, EntityFrameworkCore);
        AssertNoDependency(Contracts, Data, Repositories, Services, Controllers, EntityFrameworkCore);
    }

    private static void AssertNoDependency(string layer, params string[] forbidden)
    {
        var result = ApiTypes
            .That().ResideInNamespace(layer)
            .ShouldNot().HaveDependencyOnAny(forbidden)
            .GetResult();

        Assert.True(
            result.IsSuccessful,
            $"{layer} must not depend on [{string.Join(", ", forbidden)}]. Violations: "
            + string.Join(", ", result.FailingTypeNames ?? []));
    }
}
