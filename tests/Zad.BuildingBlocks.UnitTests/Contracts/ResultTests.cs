using AwesomeAssertions;
using Xunit;
using Zad.BuildingBlocks.Contracts.Paging;
using Zad.BuildingBlocks.Contracts.Results;

namespace Zad.BuildingBlocks.UnitTests.Contracts;

public sealed class ResultTests
{
    [Fact]
    public void Success_Result_Exposes_Success_State_And_Throws_On_Error()
    {
        var result = Result.Success();

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();

        var act = () => result.Error;
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Failure_Result_Exposes_Error_And_Rejects_Null()
    {
        var error = Error.Validation("test.invalid", "Invalid input");
        var result = Result.Failure(error);

        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);

        var nullAct = () => Result.Failure(null!);
        nullAct.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Generic_Success_Exposes_Value_And_Throws_On_Error()
    {
        var result = Result<int>.Success(42);

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be(42);

        var act = () => result.Error;
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Generic_Failure_Throws_On_Value_With_Error_Code()
    {
        var error = Error.NotFound("test.missing", "Missing item");
        var result = Result<string>.Failure(error);

        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);

        var act = () => result.Value;
        act.Should().Throw<InvalidOperationException>()
            .Which.Message.Should().Contain("test.missing");
    }

    [Fact]
    public void Error_Factory_Methods_Assign_Expected_Error_Types()
    {
        var details = new Dictionary<string, object?>(StringComparer.Ordinal) { ["field"] = "Name" };

        var validation = Error.Validation("v", "msg", details);
        validation.Type.Should().Be(ErrorType.Validation);
        validation.Details.Should().ContainKey("field");

        Error.NotFound("nf", "msg").Type.Should().Be(ErrorType.NotFound);
        Error.Conflict("c", "msg").Type.Should().Be(ErrorType.Conflict);
        Error.Unauthorized("u", "msg").Type.Should().Be(ErrorType.Unauthorized);
        Error.Forbidden("f", "msg").Type.Should().Be(ErrorType.Forbidden);
        Error.Unprocessable("up", "msg").Type.Should().Be(ErrorType.Unprocessable);
        Error.Failure("fl", "msg").Type.Should().Be(ErrorType.Failure);
    }

    [Fact]
    public void PagedList_Calculates_Page_Navigation_Properties()
    {
        var first = new PagedList<string>(["a", "b"], Page: 1, PageSize: 2, TotalCount: 5);
        first.TotalPages.Should().Be(3);
        first.HasPreviousPage.Should().BeFalse();
        first.HasNextPage.Should().BeTrue();

        var middle = new PagedList<string>(["c", "d"], Page: 2, PageSize: 2, TotalCount: 5);
        middle.HasPreviousPage.Should().BeTrue();
        middle.HasNextPage.Should().BeTrue();

        var last = new PagedList<string>(["e"], Page: 3, PageSize: 2, TotalCount: 5);
        last.HasPreviousPage.Should().BeTrue();
        last.HasNextPage.Should().BeFalse();

        var zeroSize = new PagedList<string>([], Page: 1, PageSize: 0, TotalCount: 5);
        zeroSize.TotalPages.Should().Be(0);
    }
}
