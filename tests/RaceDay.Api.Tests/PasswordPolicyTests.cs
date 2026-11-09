using RaceDay.Contracts;

namespace RaceDay.Api.Tests;

public sealed class PasswordPolicyTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("password123!")]
    [InlineData("RACEDAY2026!")]
    [InlineData("RaceDay2026")]
    [InlineData("RaceDay!")]
    public void Rejects_passwords_that_do_not_meet_the_course_project_policy(string? password)
    {
        Assert.False(PasswordPolicy.IsStrong(password));
    }

    [Theory]
    [InlineData("RaceDay!Run2026")]
    [InlineData("Durban#Runner2030")]
    public void Accepts_strong_passwords(string password)
    {
        Assert.True(PasswordPolicy.IsStrong(password));
    }
}
