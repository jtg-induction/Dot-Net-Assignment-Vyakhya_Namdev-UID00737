using DotNetRestaurantManagement.Data;
using DotNetRestaurantManagement.Models.Entities;
using DotNetRestaurantManagement.Repositories;
using Effort;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetRestaurantManagement.Tests.Repositories
{
    [TestClass]
    public class RefreshTokenRepositoryTests
    {
        private RestaurantDbContext _context;
        private RefreshTokenRepository _repository;

        [TestInitialize]
        public void Setup()
        {
            var connection = DbConnectionFactory.CreateTransient();
            _context = new RestaurantDbContext(connection);
            _repository = new RefreshTokenRepository(_context);
        }

        private User CreateUser()
        {
            return new User
            {
                Name = "Vyakhya",
                Email = "vyakhya@test.com",
                Password = "HASH",
                PhoneNumber = "9876543210"
            };
        }

        private RefreshToken CreateRefreshToken(long userId)
        {
            return new RefreshToken
            {
                UserId = userId,
                Token = "refresh-token-hash"
            };
        }

        [TestMethod]
        [Description("Verifies that a valid refresh token is successfully added to the database.")]
        public async Task AddAsync_WhenRefreshTokenIsValid_ShouldInsertToken()
        {
            User user = CreateUser();
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            RefreshToken refreshToken = CreateRefreshToken(user.Id);
            await _repository.Add(refreshToken);
            await _repository.SaveChangesAsync();
            _context.RefreshTokens.Count().Should().Be(1);
            RefreshToken result = _context.RefreshTokens.First();
            result.Id.Should().BeGreaterThan(0);
            result.UserId.Should().Be(user.Id);
            result.Token.Should().Be("refresh-token-hash");
        }

        [TestMethod]
        [Description("Verifies that GetByTokenAsync returns the refresh token when the token hash exists.")]
        public async Task GetByTokenAsync_WhenTokenExists_ShouldReturnToken()
        {
            User user = CreateUser();
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            RefreshToken refreshToken = CreateRefreshToken(user.Id);
            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            RefreshToken result = await _repository.GetByTokenAsync("refresh-token-hash");

            result.Should().NotBeNull();
            result.Token.Should().Be("refresh-token-hash");
            result.UserId.Should().Be(user.Id);
        }

        [TestMethod]
        [Description("Verifies that GetByTokenAsync includes the user associated with the refresh token.")]
        public async Task GetByTokenAsync_WhenTokenExists_ShouldReturnAssociatedUser()
        {
            User user = CreateUser();
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            RefreshToken refreshToken = CreateRefreshToken(user.Id);
            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            RefreshToken result = await _repository.GetByTokenAsync("refresh-token-hash");

            result.Should().NotBeNull();
            result.User.Should().NotBeNull();
            result.User.Id.Should().Be(user.Id);
            result.User.Email.Should().Be("vyakhya@test.com");
        }

        [TestMethod]
        [Description("Verifies that GetByTokenAsync returns null when the token hash does not exist.")]
        public async Task GetByTokenAsync_WhenTokenDoesNotExist_ShouldReturnNull()
        {
            RefreshToken result = await _repository.GetByTokenAsync("invalid-token-hash");
            result.Should().BeNull();
        }

        [TestMethod]
        [Description("Verifies that GetByTokenAsync returns the correct refresh token when multiple tokens exist.")]
        public async Task GetByTokenAsync_WithMultipleTokens_ShouldReturnCorrectToken()
        {
            User user = CreateUser();
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            RefreshToken token1 = CreateRefreshToken(user.Id);
            token1.Token = "token-hash-1";

            RefreshToken token2 = CreateRefreshToken(user.Id);
            token2.Token = "token-hash-2";

            _context.RefreshTokens.Add(token1);
            _context.RefreshTokens.Add(token2);
            await _context.SaveChangesAsync();

            RefreshToken result = await _repository.GetByTokenAsync("token-hash-2");
            result.Should().NotBeNull();
            result.Token.Should().Be("token-hash-2");
            result.UserId.Should().Be(user.Id);
        }

        [TestMethod]
        [Description("Verifies that GetByIdAsync returns the refresh token when the ID exists.")]
        public async Task GetByIdAsync_WhenIdExists_ShouldReturnToken()
        {
            User user = CreateUser();
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            RefreshToken refreshToken = CreateRefreshToken(user.Id);
            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            RefreshToken result = await _repository.GetByIdAsync(refreshToken.Id);

            result.Should().NotBeNull();
            result.Id.Should().Be(refreshToken.Id);
            result.UserId.Should().Be(user.Id);
            result.Token.Should().Be("refresh-token-hash");
        }

        [TestMethod]
        [Description("Verifies that GetByIdAsync returns null when the refresh token ID does not exist.")]
        public async Task GetByIdAsync_WhenIdDoesNotExist_ShouldReturnNull()
        {
            RefreshToken result = await _repository.GetByIdAsync(999999);

            result.Should().BeNull();
        }

        [TestMethod]
        [Description("Verifies that multiple refresh tokens can be added for the same user.")]
        public async Task AddMultipleTokens_ShouldSaveAllTokens()
        {
            User user = CreateUser();
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            RefreshToken token1 = CreateRefreshToken(user.Id);
            token1.Token = "token-hash-1";

            RefreshToken token2 = CreateRefreshToken(user.Id);
            token2.Token = "token-hash-2";

            await _repository.Add(token1);
            await _repository.Add(token2);
            await _context.SaveChangesAsync();
            _context.RefreshTokens.Count().Should().Be(2);
        }

        [TestMethod]
        [Description("Verifies that deleting an existing refresh token removes it from the database.")]
        public async Task DeleteAsync_WhenRefreshTokenExists_ShouldDeleteToken()
        {
            User user = CreateUser();
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            RefreshToken refreshToken = CreateRefreshToken(user.Id);
            await _repository.Add(refreshToken);
            await _context.SaveChangesAsync();

            await _repository.Delete(refreshToken);
            await _context.SaveChangesAsync();

            RefreshToken result = await _repository.GetByIdAsync(refreshToken.Id);

            result.Should().BeNull();
            _context.RefreshTokens.Count().Should().Be(0);
        }
    }
}
