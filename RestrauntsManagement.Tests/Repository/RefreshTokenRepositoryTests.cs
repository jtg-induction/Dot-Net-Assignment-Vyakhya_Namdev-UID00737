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
                Token = "refresh-token-123",
                UserId = userId,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };
        }

        [TestMethod]
        public async Task AddAsync_WhenRefreshTokenIsValid_ShouldInsertToken()
        {
            User user = CreateUser();

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            RefreshToken refreshToken = CreateRefreshToken(user.Id);

            await _repository.AddAsync(refreshToken);
            _context.RefreshTokens.Count().Should().Be(1);
            _context.RefreshTokens.First().Token.Should().Be("refresh-token-123");
            _context.RefreshTokens.First().UserId.Should().Be(user.Id);
        }

        [TestMethod]
        public async Task GetByTokenAsync_WhenTokenExists_ShouldReturnToken()
        {
            User user = CreateUser();
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            RefreshToken refreshToken = CreateRefreshToken(user.Id);

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            RefreshToken result = await _repository.GetByTokenAsync("refresh-token-123");

            result.Should().NotBeNull();
            result.Token.Should().Be("refresh-token-123");
            result.UserId.Should().Be(user.Id);
        }

        [TestMethod]
        public async Task GetByTokenAsync_WhenTokenDoesNotExist_ShouldReturnNull()
        {
            RefreshToken result = await _repository.GetByTokenAsync("invalid-token");
            result.Should().BeNull();
        }

        [TestMethod]
        public async Task GetByTokenAsync_WithMultipleTokens_ShouldReturnCorrectToken()
        {
            User user1 = CreateUser();

            User user2 = new User
            {
                Name = "Rahul",
                Email = "rahul@test.com",
                Password = "HASH",
                PhoneNumber = "9999999999"
            };

            _context.Users.Add(user1);
            _context.Users.Add(user2);

            await _context.SaveChangesAsync();

            RefreshToken token1 = CreateRefreshToken(user1.Id);
            token1.Token = "token-1";

            RefreshToken token2 = CreateRefreshToken(user2.Id);
            token2.Token = "token-2";

            _context.RefreshTokens.Add(token1);
            _context.RefreshTokens.Add(token2);

            await _context.SaveChangesAsync();

            RefreshToken result = await _repository.GetByTokenAsync("token-2");
            result.Should().NotBeNull();
            result.Token.Should().Be("token-2");
            result.UserId.Should().Be(user2.Id);
        }

        [TestMethod]
        public async Task GetByTokenAsync_WhenTokenIsRevoked_ShouldReturnRevokedToken()
        {
            User user = CreateUser();
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            RefreshToken refreshToken = CreateRefreshToken(user.Id);
            refreshToken.IsRevoked = true;
            refreshToken.RevokedAt = DateTime.UtcNow;

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            RefreshToken result = await _repository.GetByTokenAsync("refresh-token-123");
            result.Should().NotBeNull();
            result.IsRevoked.Should().BeTrue();
            result.RevokedAt.Should().NotBeNull();
        }

        [TestMethod]
        public async Task GetByTokenAsync_WhenTokenIsExpired_ShouldReturnToken()
        {
            User user = CreateUser();
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            RefreshToken refreshToken = CreateRefreshToken(user.Id);
            refreshToken.ExpiresAt = DateTime.UtcNow.AddMinutes(-10);
            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            RefreshToken result = await _repository.GetByTokenAsync("refresh-token-123");
            result.Should().NotBeNull();
            result.ExpiresAt.Should().BeBefore(DateTime.UtcNow);
        }

        [TestMethod]
        public async Task AddMultipleTokens_ShouldSaveAllTokens()
        {
            User user = CreateUser();

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            RefreshToken token1 = CreateRefreshToken(user.Id);
            token1.Token = "token-1";

            RefreshToken token2 = CreateRefreshToken(user.Id);
            token2.Token = "token-2";

            await _repository.AddAsync(token1);
            await _repository.AddAsync(token2);
            _context.RefreshTokens.Count().Should().Be(2);
        }

        [TestMethod]
        public async Task GetByTokenAsync_WhenNoTokensExist_ShouldReturnNull()
        {
            RefreshToken result = await _repository.GetByTokenAsync("refresh-token-123");
            result.Should().BeNull();
        }

        [TestMethod]
        public async Task GetByTokenAsync_WhenTokenIsNull_ShouldReturnNull()
        {
            RefreshToken result = await _repository.GetByTokenAsync(null);
            result.Should().BeNull();
        }

        [TestMethod]
        public async Task GetByTokenAsync_WhenTokenIsEmpty_ShouldReturnNull()
        {
            RefreshToken result = await _repository.GetByTokenAsync("");
            result.Should().BeNull();
        }

        [TestMethod]
        public async Task UpdateAsync_WhenRefreshTokenIsUpdated_ShouldPersistChanges()
        {
            User user = CreateUser();

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            RefreshToken refreshToken = CreateRefreshToken(user.Id);
            await _repository.AddAsync(refreshToken);

            refreshToken.IsRevoked = true;
            refreshToken.RevokedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(refreshToken);
            RefreshToken result = await _repository.GetByTokenAsync("refresh-token-123");
            result.Should().NotBeNull();
            result.IsRevoked.Should().BeTrue();
            result.RevokedAt.Should().NotBeNull();
        }

        [TestMethod]
        public async Task RevokeAsync_WhenTokenIsValid_ShouldRevokeToken()
        {
            User user = CreateUser();

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            RefreshToken refreshToken = CreateRefreshToken(user.Id);

            await _repository.AddAsync(refreshToken);
            await _repository.RevokeAsync(refreshToken);
            RefreshToken result = await _repository.GetByTokenAsync("refresh-token-123");

            result.Should().NotBeNull();
            result.IsRevoked.Should().BeTrue();
            result.RevokedAt.Should().NotBeNull();
        }

        [TestMethod]
        public async Task RevokeAsync_WhenTokenIsAlreadyRevoked_ShouldRemainRevoked()
        {
            User user = CreateUser();

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            RefreshToken refreshToken = CreateRefreshToken(user.Id);

            await _repository.AddAsync(refreshToken);
            await _repository.RevokeAsync(refreshToken);
            await _repository.RevokeAsync(refreshToken);

            refreshToken.IsRevoked.Should().BeTrue();
            refreshToken.RevokedAt.Should().NotBeNull();
        }

        [TestMethod]
        public async Task RevokeAsync_ShouldSetRevokedAt()
        {
            User user = CreateUser();
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            RefreshToken refreshToken = CreateRefreshToken(user.Id);

            await _repository.AddAsync(refreshToken);
            await _repository.RevokeAsync(refreshToken);
            refreshToken.RevokedAt.Should().NotBeNull();
            refreshToken.RevokedAt.Value
                .Should()
                .BeOnOrBefore(DateTime.UtcNow);
        }

        [TestMethod]
        public async Task AddAsync_NewToken_ShouldNotBeRevoked()
        {
            User user = CreateUser();

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            RefreshToken refreshToken = CreateRefreshToken(user.Id);
            await _repository.AddAsync(refreshToken);
            refreshToken.IsRevoked.Should().BeFalse();
        }

        [TestMethod]
        public async Task AddAsync_ShouldStoreExpiryDate()
        {
            User user = CreateUser();
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            DateTime expiry = DateTime.UtcNow.AddDays(7);
            RefreshToken refreshToken = new RefreshToken
            {
                Token = "refresh-token-123",
                UserId = user.Id,
                ExpiresAt = expiry,
                IsRevoked = false
            };

            await _repository.AddAsync(refreshToken);
            RefreshToken result = await _repository.GetByTokenAsync("refresh-token-123");
            result.Should().NotBeNull();
            result.ExpiresAt.Should().Be(expiry);
        }
    }
}
