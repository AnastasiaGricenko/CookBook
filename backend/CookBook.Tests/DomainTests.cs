using System.Security.Claims;
using System.Text;
using CookBook.Api.Domain;
using CookBook.Api.Domain.ValueObjects;
using CookBook.Api.Infrastructure.Options;
using CookBook.Api.Infrastructure.Security;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace CookBook.Tests;

/// <summary>Юнит-тесты домена и инфраструктурной безопасности (без HTTP).</summary>
public sealed class DomainTests
{
    [Fact]
    public void Email_ValidValue_IsTrimmedAndLowered()
    {
        var email = Email.Create("  Demo@Kulibook.RU  ");

        Assert.Equal("demo@kulibook.ru", email.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Email_Empty_ThrowsRequired(string value)
    {
        var exception = Assert.Throws<DomainValidationException>(() => Email.Create(value));

        Assert.Equal(ValidationTexts.Required, exception.Errors["email"].Single());
    }

    [Fact]
    public void Email_WithoutAt_ThrowsFormat()
    {
        var exception = Assert.Throws<DomainValidationException>(() => Email.Create("not-an-email"));

        Assert.Equal(ValidationTexts.EmailFormat, exception.Errors["email"].Single());
    }

    [Fact]
    public void Email_TooLong_ThrowsMaxLength()
    {
        var value = $"{new string('a', 250)}@mail.ru";

        var exception = Assert.Throws<DomainValidationException>(() => Email.Create(value));

        Assert.Equal(ValidationTexts.EmailMax, exception.Errors["email"].Single());
    }

    [Fact]
    public void PersonName_IsTrimmed()
    {
        Assert.Equal("Мария", PersonName.Create("  Мария  ").Value);
    }

    [Theory]
    [InlineData("A")]
    [InlineData("")]
    public void PersonName_TooShort_Throws(string value)
    {
        Assert.Throws<DomainValidationException>(() => PersonName.Create(value));
    }

    [Fact]
    public void PersonName_TooLong_Throws()
    {
        var exception = Assert.Throws<DomainValidationException>(
            () => PersonName.Create(new string('Я', 101)));

        Assert.Equal(ValidationTexts.NameMax, exception.Errors["name"].Single());
    }

    [Fact]
    public void RecipeTitle_IsTrimmed()
    {
        Assert.Equal("Борщ", RecipeTitle.Create("  Борщ  ").Value);
    }

    [Fact]
    public void RecipeTitle_Empty_Throws()
    {
        Assert.Throws<DomainValidationException>(() => RecipeTitle.Create("   "));
    }

    [Fact]
    public void NutritionFacts_NegativeValue_ThrowsWithFieldError()
    {
        var exception = Assert.Throws<DomainValidationException>(
            () => NutritionFacts.Create(-1, 10, 10, 10));

        Assert.Equal(ValidationTexts.NutritionOutOfRange, exception.Errors["kcal"].Single());
    }

    [Fact]
    public void NutritionFacts_Boundaries_AreAllowed()
    {
        var facts = NutritionFacts.Create(0, 10_000, 0, 10_000);

        Assert.Equal(0, facts.Kcal);
        Assert.Equal(10_000, facts.Protein);
    }

    [Theory]
    [InlineData(Difficulty.Easy, "Легкая")]
    [InlineData(Difficulty.Medium, "Средняя")]
    [InlineData(Difficulty.Hard, "Сложная")]
    public void DifficultyMapper_MapsBothDirections(Difficulty difficulty, string apiValue)
    {
        Assert.Equal(apiValue, DifficultyMapper.ToApiString(difficulty));
        Assert.Equal(difficulty, DifficultyMapper.Parse(apiValue));
    }

    [Fact]
    public void DifficultyMapper_UnknownValue_ThrowsValidation()
    {
        var exception = Assert.Throws<DomainValidationException>(
            () => DifficultyMapper.Parse("Очень сложная"));

        Assert.Equal(ValidationTexts.InvalidForm, exception.Message);
        Assert.True(exception.Errors.ContainsKey("difficulty"));
    }

    [Fact]
    public void Pbkdf2PasswordHasher_SamePassword_ProducesDifferentHashes()
    {
        var hasher = new Pbkdf2PasswordHasher();

        var first = hasher.Hash("demo123");
        var second = hasher.Hash("demo123");

        Assert.NotEqual(first, second);
        Assert.Equal(3, first.Split('.').Length);
    }

    [Fact]
    public void Pbkdf2PasswordHasher_Verify_AcceptsOnlyCorrectPassword()
    {
        var hasher = new Pbkdf2PasswordHasher();
        var hash = hasher.Hash("demo123");

        Assert.True(hasher.Verify("demo123", hash));
        Assert.False(hasher.Verify("demo124", hash));
        Assert.False(hasher.Verify("demo123", "испорченный-хеш"));
    }

    [Fact]
    public void JwtTokenService_TokenContainsSubAndIssuer()
    {
        var userId = Guid.NewGuid();
        var service = CreateTokenService();

        var (token, expiresAt) = service.CreateAccessToken(userId);

        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token);
        Assert.Equal(userId.ToString(), jwt.Claims.Single(claim => claim.Type == "sub").Value);
        Assert.Equal("CookBook.Api", jwt.Issuer);
        Assert.True(expiresAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task JwtTokenService_ExpiredToken_DoesNotValidate()
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(CookBookFactory.TestJwtKey));

        var expired = new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = "CookBook.Api",
            Subject = new ClaimsIdentity([new Claim("sub", Guid.NewGuid().ToString())]),
            Expires = DateTime.UtcNow.AddMinutes(-10),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256),
        });

        var result = await new JsonWebTokenHandler().ValidateTokenAsync(expired, new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "CookBook.Api",
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ClockSkew = TimeSpan.Zero,
        });

        Assert.False(result.IsValid);
    }

    private static JwtTokenService CreateTokenService() => new(Options.Create(new AuthOptions
    {
        JwtKey = CookBookFactory.TestJwtKey,
        Issuer = "CookBook.Api",
        AccessTtlMinutes = 60,
        CookieName = "access_token",
    }));
}
