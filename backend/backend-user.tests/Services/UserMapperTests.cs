using Xunit;
using backend_user.Services.Mappers;
using backend_user.DTO.User.Request;
using backend_user.DTO.User.Response;
using backend_user.Models;
using System;

public class UserMapperTests
{
    [Fact]
    public void ToResponseDto_NullUser_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => UserMapper.ToResponseDto(null));
    }

    [Fact]
    public void ToResponseDto_ValidUser_MapsCorrectly()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "a@b.com", Username = "user", FirstName = "A", LastName = "B", ProfessionalTitle = "Dev", Bio = "bio", Location = "loc", AvatarUrl = "url", IsActive = true, IsAdmin = false, LastLoginAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var dto = UserMapper.ToResponseDto(user);
        Assert.Equal(user.Id, dto.Id);
        Assert.Equal(user.Email, dto.Email);
        Assert.Equal(user.Username, dto.Username);
        Assert.Equal(user.FirstName, dto.FirstName);
        Assert.Equal(user.LastName, dto.LastName);
        Assert.Equal(user.ProfessionalTitle, dto.ProfessionalTitle);
        Assert.Equal(user.Bio, dto.Bio);
        Assert.Equal(user.Location, dto.Location);
        Assert.Equal(user.AvatarUrl, dto.AvatarUrl);
        Assert.Equal(user.IsActive, dto.IsActive);
        Assert.Equal(user.IsAdmin, dto.IsAdmin);
        Assert.Equal(user.LastLoginAt, dto.LastLoginAt);
        Assert.Equal(user.CreatedAt, dto.CreatedAt);
        Assert.Equal(user.UpdatedAt, dto.UpdatedAt);
    }

    [Fact]
    public void ToEntity_RegisterUserRequest_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => UserMapper.ToEntity((RegisterUserRequest)null));
    }

    [Fact]
    public void ToEntity_RegisterUserRequest_Valid_MapsCorrectly()
    {
        var req = new RegisterUserRequest { Email = "a@b.com", FirstName = "A", LastName = "B", ProfessionalTitle = "Dev", Bio = "bio", Location = "loc", ProfileImage = "url" };
        var user = UserMapper.ToEntity(req);
        Assert.Equal(req.Email, user.Email);
        Assert.Equal("a", user.Username);
        Assert.Equal(req.FirstName, user.FirstName);
        Assert.Equal(req.LastName, user.LastName);
        Assert.Equal(req.ProfessionalTitle, user.ProfessionalTitle);
        Assert.Equal(req.Bio, user.Bio);
        Assert.Equal(req.Location, user.Location);
        Assert.Equal(req.ProfileImage, user.AvatarUrl);
    }

    [Fact]
    public void ToEntity_RegisterOAuthUserRequest_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => UserMapper.ToEntity((RegisterOAuthUserRequest)null));
    }

    [Fact]
    public void ToEntity_RegisterOAuthUserRequest_Valid_MapsCorrectly()
    {
        var req = new RegisterOAuthUserRequest { Email = "a@b.com", FirstName = "A", LastName = "B", ProfessionalTitle = "Dev", Bio = "bio", Location = "loc", ProfileImage = "url" };
        var user = UserMapper.ToEntity(req);
        Assert.Equal(req.Email, user.Email);
        Assert.Equal("a", user.Username);
        Assert.Equal(req.FirstName, user.FirstName);
        Assert.Equal(req.LastName, user.LastName);
        Assert.Equal(req.ProfessionalTitle, user.ProfessionalTitle);
        Assert.Equal(req.Bio, user.Bio);
        Assert.Equal(req.Location, user.Location);
        Assert.Equal(req.ProfileImage, user.AvatarUrl);
    }

    [Fact]
    public void ToSummaryDto_NullUser_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => UserMapper.ToSummaryDto(null));
    }

    [Fact]
    public void ToSummaryDto_ValidUser_MapsCorrectly()
    {
        var user = new User { Id = Guid.NewGuid(), Username = "user", FirstName = "A", LastName = "B", ProfessionalTitle = "Dev", Location = "loc", AvatarUrl = "url", IsActive = true, CreatedAt = DateTime.UtcNow };
        var dto = UserMapper.ToSummaryDto(user);
        Assert.Equal(user.Id, dto.Id);
        Assert.Equal(user.Username, dto.Username);
        Assert.Equal(user.FirstName, dto.FirstName);
        Assert.Equal(user.LastName, dto.LastName);
        Assert.Equal(user.ProfessionalTitle, dto.ProfessionalTitle);
        Assert.Equal(user.Location, dto.Location);
        Assert.Equal(user.AvatarUrl, dto.AvatarUrl);
        Assert.Equal(user.IsActive, dto.IsActive);
        Assert.Equal(user.CreatedAt, dto.CreatedAt);
    }

    [Fact]
    public void ToPortfolioInfo_NullUser_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => UserMapper.ToPortfolioInfo(null));
    }

    [Fact]
    public void ToPortfolioInfo_ValidUser_MapsCorrectly()
    {
        var user = new User { Id = Guid.NewGuid(), Username = "user", FirstName = "A", LastName = "B", ProfessionalTitle = "Dev", Location = "loc", AvatarUrl = "url", Email = "a@b.com" };
        var info = UserMapper.ToPortfolioInfo(user);
        var type = info.GetType();
        Assert.Equal(user.Id, (Guid)type.GetProperty("userId")!.GetValue(info));
        Assert.Equal(user.Username, (string)type.GetProperty("username")!.GetValue(info));
        Assert.Equal($"{user.FirstName} {user.LastName}".Trim(), (string)type.GetProperty("name")!.GetValue(info));
        Assert.Equal(user.ProfessionalTitle, (string)type.GetProperty("professionalTitle")!.GetValue(info));
        Assert.Equal(user.Location, (string)type.GetProperty("location")!.GetValue(info));
        Assert.Equal(user.AvatarUrl, (string)type.GetProperty("avatarUrl")!.GetValue(info));
        Assert.Equal(user.Email, (string)type.GetProperty("email")!.GetValue(info));
    }

    [Fact]
    public void ToRegisterRequest_NullUser_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => UserMapper.ToRegisterRequest(null));
    }

    [Fact]
    public void ToRegisterRequest_ValidUser_MapsCorrectly()
    {
        var user = new User { Email = "a@b.com", FirstName = "A", LastName = "B", ProfessionalTitle = "Dev", Bio = "bio", Location = "loc", AvatarUrl = "url" };
        var req = UserMapper.ToRegisterRequest(user);
        Assert.Equal(user.Email, req.Email);
        Assert.Equal(user.FirstName, req.FirstName);
        Assert.Equal(user.LastName, req.LastName);
        Assert.Equal(user.ProfessionalTitle, req.ProfessionalTitle);
        Assert.Equal(user.Bio, req.Bio);
        Assert.Equal(user.Location, req.Location);
        Assert.Equal(user.AvatarUrl, req.ProfileImage);
    }
}