using DotNetRestaurantManagement.Services.Implementations;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DotNetRestaurantManagement.Tests.Services
{
    [TestClass]
    public class PasswordHasherTests
    {
        private PasswordHasher _passwordHasher;

        [TestInitialize]
        public void Setup()
        {
            _passwordHasher = new PasswordHasher();
        }

        [TestMethod]
        public void Hash_ValidPassword_ReturnsHashedPassword()
        {
            string password = "Vyakhya@123";
            string hash = _passwordHasher.Hash(password);
            hash.Should().NotBeNullOrWhiteSpace();
            hash.Should().NotBe(password);
        }

        [TestMethod]
        public void Verify_CorrectPassword_ReturnsTrue()
        {
            string password = "Vyakhya@123";
            string hash = _passwordHasher.Hash(password);
            bool result = _passwordHasher.Verify(password, hash);
            result.Should().BeTrue();
        }

        [TestMethod]
        public void Verify_WrongPassword_ReturnsFalse()
        {
            string password = "Vyakhya@123";
            string hash = _passwordHasher.Hash(password);
            bool result = _passwordHasher.Verify("WrongPassword", hash);
            result.Should().BeFalse();
        }

        [TestMethod]
        public void Hash_NullPassword_ThrowsArgumentException()
        {
            Action action = () => _passwordHasher.Hash(null);
            action.Should().Throw<ArgumentException>().WithMessage("Password cannot be empty!*");
        }

        [TestMethod]
        public void Hash_EmptyPassword_ThrowsArgumentException()
        {
            Action action = () => _passwordHasher.Hash(string.Empty);
            action.Should().Throw<ArgumentException>().WithMessage("Password cannot be empty!*");
        }

        [TestMethod]
        public void Hash_WhiteSpacePassword_ThrowsArgumentException()
        {
            Action action = () => _passwordHasher.Hash("   ");
            action.Should().Throw<ArgumentException>().WithMessage("Password cannot be empty!*");
        }

        [TestMethod]
        public void Verify_NullPassword_ReturnsFalse()
        {
            bool result = _passwordHasher.Verify(null, "HASH");
            result.Should().BeFalse();
        }

        [TestMethod]
        public void Verify_NullHash_ReturnsFalse()
        {
            bool result = _passwordHasher.Verify("Vyakhya@123", null);
            result.Should().BeFalse();
        }

        [TestMethod]
        public void Verify_BothNull_ReturnsFalse()
        {
            bool result = _passwordHasher.Verify(null, null);
            result.Should().BeFalse();
        }

        [TestMethod]
        public void Verify_EmptyPassword_ReturnsFalse()
        {
            bool result = _passwordHasher.Verify("", "HASH");
            result.Should().BeFalse();
        }

        [TestMethod]
        public void Verify_EmptyHash_ReturnsFalse()
        {
            bool result = _passwordHasher.Verify("Vyakhya@123", "");
            result.Should().BeFalse();
        }

        [TestMethod]
        public void Verify_HashedPassword_ReturnsTrue()
        {            
            string password = "Vyakhya@123";
            string hash = _passwordHasher.Hash(password);
            _passwordHasher.Verify(password, hash).Should().BeTrue();
        }

        [TestMethod]
        public void Hash_SamePassword_GeneratesDifferentHashes()
        {
            string password = "Vyakhya@123";
            string hash1 = _passwordHasher.Hash(password);
            string hash2 = _passwordHasher.Hash(password);

            hash1.Should().NotBe(hash2);
            _passwordHasher.Verify(password, hash1).Should().BeTrue();
            _passwordHasher.Verify(password, hash2).Should().BeTrue();
        }

    }
}
