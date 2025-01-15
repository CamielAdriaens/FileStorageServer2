using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MODELS;
using Xunit;

namespace FileStorage.Tests.ValidationTests
{
    public class ValidationTests
    {
        [Fact]
        public void User_Should_ValidateRequiredFields()
        {
            // Arrange
            var user = new User();
            var validationResults = new List<ValidationResult>();
            var context = new ValidationContext(user);

            // Act
            var isValid = Validator.TryValidateObject(user, context, validationResults, true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(validationResults, v => v.ErrorMessage.Contains("The Name field is required."));
            Assert.Contains(validationResults, v => v.ErrorMessage.Contains("The Email field is required."));
        }

        [Fact]
        public void User_Email_Should_BeValid()
        {
            // Arrange
            var user = new User
            {
                Email = "invalid-email",
                Name = "Test User",
                GoogleId = "12345"
            };
            var validationResults = new List<ValidationResult>();
            var context = new ValidationContext(user);

            // Act
            var isValid = Validator.TryValidateObject(user, context, validationResults, true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(validationResults, v => v.ErrorMessage.Contains("Invalid Email Address format."));
        }

        [Fact]
        public void User_Name_Should_Not_Exceed_MaxLength()
        {
            // Arrange
            var user = new User
            {
                Name = new string('a', 101),
                Email = "test@example.com",
                GoogleId = "12345"
            };
            var validationResults = new List<ValidationResult>();
            var context = new ValidationContext(user);

            // Act
            var isValid = Validator.TryValidateObject(user, context, validationResults, true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(validationResults, v => v.ErrorMessage.Contains("Name cannot exceed 100 characters."));
        }
    }

    public class UserFileTests
    {
        [Fact]
        public void UserFile_Should_ValidateRequiredFields()
        {
            // Arrange
            var userFile = new UserFile();
            var validationResults = new List<ValidationResult>();
            var context = new ValidationContext(userFile);

            // Act
            var isValid = Validator.TryValidateObject(userFile, context, validationResults, true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(validationResults, v => v.ErrorMessage.Contains("The MongoFileId field is required."));
            Assert.Contains(validationResults, v => v.ErrorMessage.Contains("The FileName field is required."));
        }

        [Fact]
        public void UserFile_FileName_Should_Not_Exceed_MaxLength()
        {
            // Arrange
            var userFile = new UserFile
            {
                MongoFileId = "1234567890",
                FileName = new string('a', 256),
                UploadDate = DateTime.Now,
                UserId = 1
            };
            var validationResults = new List<ValidationResult>();
            var context = new ValidationContext(userFile);

            // Act
            var isValid = Validator.TryValidateObject(userFile, context, validationResults, true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(validationResults, v => v.ErrorMessage.Contains("FileName cannot exceed 255 characters."));
        }
    }
}
