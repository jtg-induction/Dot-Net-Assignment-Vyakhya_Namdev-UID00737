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

        /// <summary>
        /// Verifies that Hash returns a non-empty hashed value for a valid password.
        /// </summary>
        [TestMethod]
        public void Hash_ValidPassword_ReturnsHashedPassword()
        {
            string password = "Vyakhya@123";
            string hash = _passwordHasher.Hash(password);
            hash.Should().NotBeNullOrWhiteSpace();
            hash.Should().NotBe(password);
        }

        /// <summary>
        /// Verifies that Verify returns true when the correct password is provided for a valid hash.
        /// </summary>
        [TestMethod]
        public void Verify_CorrectPassword_ReturnsTrue()
        {
            string password = "Vyakhya@123";
            string hash = _passwordHasher.Hash(password);
            bool result = _passwordHasher.Verify(password, hash);
            result.Should().BeTrue();
        }

        /// <summary>
        /// Verifies that Verify returns false when an incorrect password is provided.
        /// </summary>
        [TestMethod]
        public void Verify_WrongPassword_ReturnsFalse()
        {
            string password = "Vyakhya@123";
            string hash = _passwordHasher.Hash(password);
            bool result = _passwordHasher.Verify("WrongPassword", hash);
            result.Should().BeFalse();
        }

        /// <summary>
        /// Verifies that Hash throws an ArgumentException when the password is null.
        /// </summary>
        [TestMethod]
        public void Hash_NullPassword_ThrowsArgumentException()
        {
            Action action = () => _passwordHasher.Hash(null);
            action.Should().Throw<ArgumentException>().WithMessage("Password cannot be empty!*");
        }

        /// <summary>
        /// Verifies that Hash throws an ArgumentException when the password is empty.
        /// </summary>
        [TestMethod]
        public void Hash_EmptyPassword_ThrowsArgumentException()
        {
            Action action = () => _passwordHasher.Hash(string.Empty);
            action.Should().Throw<ArgumentException>().WithMessage("Password cannot be empty!*");
        }

        /// <summary>
        /// Verifies that Verify returns false when the password is null.
        /// </summary>
        [TestMethod]
        public void Verify_NullPassword_ReturnsFalse()
        {
            bool result = _passwordHasher.Verify(null, "HASH");
            result.Should().BeFalse();
        }

        /// <summary>
        /// Verifies that Verify returns false when the hash is null.
        /// </summary>
        [TestMethod]
        public void Verify_NullHash_ReturnsFalse()
        {
            bool result = _passwordHasher.Verify("Vyakhya@123", null);
            result.Should().BeFalse();
        }

        /// <summary>
        /// Verifies that Verify returns false when the hash is empty.
        /// </summary>
        [TestMethod]
        public void Verify_EmptyHash_ReturnsFalse()
        {
            bool result = _passwordHasher.Verify("Vyakhya@123", "");
            result.Should().BeFalse();
        }
    }
}
