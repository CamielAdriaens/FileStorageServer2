using DAL;
using Microsoft.EntityFrameworkCore;
using MODELS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileStorage.Tests.IntegrationTests
{
    public class UserRepositoryIntegrationTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly UserRepository _userRepository;

        public UserRepositoryIntegrationTests()
        {
            // Setup in-memory database options for SQL Server
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new AppDbContext(options);
            _userRepository = new UserRepository(_context);
        }

        [Fact]
        public async Task CreateUser_ShouldAddUserToDatabase()
        {
            // Arrange
            var user = new User { GoogleId = "google-id", Email = "test@example.com", Name = "Test User" };

            // Act
            var result = await _userRepository.CreateUser(user);

            // Assert
            var retrievedUser = await _context.Users.FindAsync(result.UserId);
            Assert.Equal("google-id", retrievedUser.GoogleId);
        }

        [Fact]
        public async Task GetUserByGoogleId_ShouldReturnUser()
        {
            // Arrange
            var user = new User { GoogleId = "google-id", Email = "test@example.com", Name = "Test User" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userRepository.GetUserByGoogleId("google-id");

            // Assert
            Assert.Equal(user, result);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }

}
