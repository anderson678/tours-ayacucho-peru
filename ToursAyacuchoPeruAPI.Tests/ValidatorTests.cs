using ToursAyacuchoPeruAPI.Application.DTOs.Auth;
using ToursAyacuchoPeruAPI.Application.DTOs.Reservations;
using ToursAyacuchoPeruAPI.Application.Validators;
using ToursAyacuchoPeruAPI.Domain.Enums;

namespace ToursAyacuchoPeruAPI.Tests;

public class ValidatorTests
{
    [Fact]
    public void RegisterRequestValidator_accepts_valid_client_data()
    {
        var validator = new RegisterRequestValidator();

        var result = validator.Validate(new RegisterRequestDto
        {
            Nombre = "Cliente Demo",
            Correo = "cliente@email.com",
            Password = "Seguro@2026",
            Telefono = "987654321"
        });

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("corta1@")]
    [InlineData("sinmayuscula@2026")]
    [InlineData("SINNUMERO@")]
    [InlineData("SinEspecial2026")]
    public void RegisterRequestValidator_rejects_weak_passwords(string password)
    {
        var validator = new RegisterRequestValidator();

        var result = validator.Validate(new RegisterRequestDto
        {
            Nombre = "Cliente Demo",
            Correo = "cliente@email.com",
            Password = password,
            Telefono = "987654321"
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterRequestDto.Password));
    }

    [Theory]
    [InlineData("12345678")]
    [InlineData("1234567890123456")]
    [InlineData("987ABC321")]
    public void RegisterRequestValidator_rejects_invalid_phone(string phone)
    {
        var validator = new RegisterRequestValidator();

        var result = validator.Validate(new RegisterRequestDto
        {
            Nombre = "Cliente Demo",
            Correo = "cliente@email.com",
            Password = "Seguro@2026",
            Telefono = phone
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterRequestDto.Telefono));
    }

    [Fact]
    public void UpdateProfileRequestValidator_accepts_https_photo_url()
    {
        var validator = new UpdateProfileRequestValidator();

        var result = validator.Validate(new UpdateProfileDto
        {
            Nombre = "Cliente",
            Telefono = "987654321",
            FotoUrl = "https://example.com/foto.jpg"
        });

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("ftp://example.com/foto.jpg")]
    [InlineData("solo-texto")]
    public void UpdateProfileRequestValidator_rejects_invalid_photo_url(string photoUrl)
    {
        var validator = new UpdateProfileRequestValidator();

        var result = validator.Validate(new UpdateProfileDto
        {
            FotoUrl = photoUrl
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateProfileDto.FotoUrl));
    }

    [Fact]
    public void LoginRequestValidator_rejects_empty_credentials()
    {
        var validator = new LoginRequestValidator();

        var result = validator.Validate(new LoginRequestDto
        {
            Correo = "",
            Password = ""
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(LoginRequestDto.Correo));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(LoginRequestDto.Password));
    }

    [Fact]
    public void RegisterPaymentValidator_accepts_valid_payment()
    {
        var validator = new RegisterPaymentValidator();

        var result = validator.Validate(new RegisterPaymentDto
        {
            ReservaId = Guid.NewGuid(),
            Monto = 150m,
            MetodoPago = MetodoPago.Yape,
            NumReferencia = "YAPE-123"
        });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void RegisterPaymentValidator_rejects_invalid_payment_data()
    {
        var validator = new RegisterPaymentValidator();

        var result = validator.Validate(new RegisterPaymentDto
        {
            ReservaId = Guid.Empty,
            Monto = 0m,
            MetodoPago = (MetodoPago)999,
            NumReferencia = ""
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterPaymentDto.ReservaId));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterPaymentDto.Monto));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterPaymentDto.MetodoPago));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterPaymentDto.NumReferencia));
    }
}
