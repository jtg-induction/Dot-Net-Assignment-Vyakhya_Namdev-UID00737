using DotNetRestaurantManagement.Data;
using DotNetRestaurantManagement.Models.Entities;
using DotNetRestaurantManagement.Repositories;
using Effort;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data.Entity;
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
                UserId = userId
            };
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context.Dispose();
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
            RefreshToken result = _context.RefreshTokens.First();
            result.Token.Should().Be("refresh-token-123");
            result.UserId.Should().Be(user.Id);
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
        public async Task DeleteAsync_WhenRefreshTokenExists_ShouldDeleteToken()
        {
            User user = CreateUser();
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            RefreshToken refreshToken = CreateRefreshToken(user.Id);
            await _repository.AddAsync(refreshToken);
            await _repository.DeleteAsync(refreshToken);
            RefreshToken result = await _repository.GetByTokenAsync("refresh-token-123");
            result.Should().BeNull();
            _context.RefreshTokens.Count().Should().Be(0);
        }

        [TestMethod]
        public async Task DeleteAsync_WhenMultipleTokensExist_ShouldDeleteOnlySpecifiedToken()
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
            await _repository.DeleteAsync(token1);
            _context.RefreshTokens.Count().Should().Be(1);
            RefreshToken remainingToken = await _repository.GetByTokenAsync("token-2");
            remainingToken.Should().NotBeNull();
            remainingToken.Token.Should().Be("token-2");
        }

        [TestMethod]
        public async Task DeleteByUserIdAsync_UserHasRefreshTokens_DeletesAllUserTokens()
        {
            var user = new User
            {
                Name = "Test User",
                Email = "test@example.com",
                Password = "hashed-password",
                PhoneNumber = "9876543210"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            var token1 = new RefreshToken
            {
                UserId = user.Id,
                Token = "refresh-token-1",
                CreatedAt = DateTime.UtcNow
            };

            var token2 = new RefreshToken
            {
                UserId = user.Id,
                Token = "refresh-token-2",
                CreatedAt = DateTime.UtcNow
            };

            _context.RefreshTokens.Add(token1);
            _context.RefreshTokens.Add(token2);
            await _context.SaveChangesAsync();
            await _repository.DeleteByUserIdAsync((int)user.Id);

            var remainingTokens = await _context.RefreshTokens
                .Where(x => x.UserId == user.Id)
                .ToListAsync();

            remainingTokens.Should().BeEmpty();
        }

        [TestMethod]
        public async Task DeleteByUserIdAsync_UserHasTokens_DoesNotDeleteOtherUsersTokens()
        {
            var user1 = new User
            {
                Name = "User One",
                Email = "user1@example.com",
                Password = "hashed-password",
                PhoneNumber = "9876543210"
            };

            var user2 = new User
            {
                Name = "User Two",
                Email = "user2@example.com",
                Password = "hashed-password",
                PhoneNumber = "9876543211"
            };

            _context.Users.Add(user1);
            _context.Users.Add(user2);
            await _context.SaveChangesAsync();
            _context.RefreshTokens.Add(new RefreshToken
            {
                UserId = user1.Id,
                Token = "user1-token",
                CreatedAt = DateTime.UtcNow
            });

            _context.RefreshTokens.Add(new RefreshToken
            {
                UserId = user2.Id,
                Token = "user2-token",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            await _repository.DeleteByUserIdAsync((int)user1.Id);
            var user1Tokens = await _context.RefreshTokens
                .Where(x => x.UserId == user1.Id)
                .ToListAsync();

            var user2Tokens = await _context.RefreshTokens
                .Where(x => x.UserId == user2.Id)
                .ToListAsync();

            user1Tokens.Should().BeEmpty();
            user2Tokens.Should().HaveCount(1);
            user2Tokens[0].Token.Should().Be("user2-token");
        }

        [TestMethod]
        public async Task DeleteByUserIdAsync_UserHasNoRefreshTokens_DoesNotThrow()
        {
            int userId = 999;
            Func<Task> act = () => _repository.DeleteByUserIdAsync(userId);
            await act.Should().NotThrowAsync();
            var tokens = await _context.RefreshTokens
                .Where(x => x.UserId == userId)
                .ToListAsync();

            tokens.Should().BeEmpty();
        }
    }
}
