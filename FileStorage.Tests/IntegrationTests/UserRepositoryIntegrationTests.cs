using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using DAL;
using MODELS;

public class UserRepositoryTests
{
    private readonly UserRepository _repository;
    private readonly AppDbContext _context;

    public UserRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _repository = new UserRepository(_context);
    }

    private async Task SeedDatabaseAsync()
    {
        var user = new User
        {
            UserId = 1,
            GoogleId = "google123",
            Email = "test@example.com",
            Name = "Test User",
            UserFiles = new List<UserFile>
            {
                new UserFile { FileId = 1, MongoFileId = "file123", FileName = "file1.txt", UploadDate = DateTime.UtcNow },
                new UserFile { FileId = 2, MongoFileId = "file456", FileName = "file2.txt", UploadDate = DateTime.UtcNow }
            }
        };

        var shareRequest = new PendingFileShare
        {
            ShareId = 1,
            MongoFileId = "file123",
            FileName = "file1.txt",
            SenderUserId = 1,
            RecipientUserId = 2,
            IsAccepted = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        _context.PendingFileShares.Add(shareRequest);
        await _context.SaveChangesAsync();
    }

    #region GetUserByUserId
    [Fact]
    public async Task GetUserByUserId_ShouldReturnUser_WhenUserExists()
    {
        await SeedDatabaseAsync();
        var user = await _repository.GetUserByUserId(1);

        Assert.NotNull(user);
        Assert.Equal(1, user.UserId);
        Assert.Equal("test@example.com", user.Email);
    }

    [Fact]
    public async Task GetUserByUserId_ShouldReturnNull_WhenUserDoesNotExist()
    {
        var user = await _repository.GetUserByUserId(999);

        Assert.Null(user);
    }
    #endregion

    #region GetUserByGoogleId
    [Fact]
    public async Task GetUserByGoogleId_ShouldReturnUser_WhenUserExists()
    {
        await SeedDatabaseAsync();
        var user = await _repository.GetUserByGoogleId("google123");

        Assert.NotNull(user);
        Assert.Equal("google123", user.GoogleId);
    }

    [Fact]
    public async Task GetUserByGoogleId_ShouldReturnNull_WhenUserDoesNotExist()
    {
        var user = await _repository.GetUserByGoogleId("nonexistent");

        Assert.Null(user);
    }
    #endregion

    #region GetUserByEmail
    [Fact]
    public async Task GetUserByEmail_ShouldReturnUser_WhenUserExists()
    {
        await SeedDatabaseAsync();
        var user = await _repository.GetUserByEmail("test@example.com");

        Assert.NotNull(user);
        Assert.Equal("test@example.com", user.Email);
    }

    [Fact]
    public async Task GetUserByEmail_ShouldReturnNull_WhenUserDoesNotExist()
    {
        var user = await _repository.GetUserByEmail("nonexistent@example.com");

        Assert.Null(user);
    }
    #endregion

    #region GetFileByMongoFileId
    [Fact]
    public async Task GetFileByMongoFileId_ShouldReturnFile_WhenFileExists()
    {
        await SeedDatabaseAsync();
        var file = await _repository.GetFileByMongoFileId("file123");

        Assert.NotNull(file);
        Assert.Equal("file123", file.MongoFileId);
    }

    [Fact]
    public async Task GetFileByMongoFileId_ShouldReturnNull_WhenFileDoesNotExist()
    {
        var file = await _repository.GetFileByMongoFileId("nonexistent");

        Assert.Null(file);
    }
    #endregion

    #region CreateUser
    [Fact]
    public async Task CreateUser_ShouldAddUser_WhenValidUserProvided()
    {
        var newUser = new User { GoogleId = "newGoogleId", Email = "new@example.com", Name = "New User" };

        var createdUser = await _repository.CreateUser(newUser);

        Assert.NotNull(createdUser);
        Assert.Equal("newGoogleId", createdUser.GoogleId);
        Assert.NotEqual(0, createdUser.UserId);
    }

    [Fact]
    public async Task CreateUser_ShouldThrowException_WhenUserIsNull()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.CreateUser(null));
    }
    #endregion

    #region AddUserFile
    [Fact]
    public async Task AddUserFile_ShouldAddFile_WhenValidFileProvided()
    {
        var newFile = new UserFile { MongoFileId = "file789", FileName = "file3.txt", UserId = 1, UploadDate = DateTime.UtcNow };

        await _repository.AddUserFile(newFile);

        var file = await _context.UserFiles.FirstOrDefaultAsync(f => f.MongoFileId == "file789");
        Assert.NotNull(file);
    }

    [Fact]
    public async Task AddUserFile_ShouldThrowException_WhenFileIsNull()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.AddUserFile(null));
    }
    #endregion

    #region GetUserFiles
    [Fact]
    public async Task GetUserFiles_ShouldReturnFiles_WhenFilesExist()
    {
        await SeedDatabaseAsync();
        var files = await _repository.GetUserFiles("google123");

        Assert.NotEmpty(files);
        Assert.Equal(2, files.Count);
    }

    [Fact]
    public async Task GetUserFiles_ShouldReturnEmptyList_WhenNoFilesExist()
    {
        var files = await _repository.GetUserFiles("nonexistent");

        Assert.Empty(files);
    }
    #endregion

    #region RemoveUserFile
    [Fact]
    public async Task RemoveUserFile_ShouldRemoveFile_WhenFileExists()
    {
        await SeedDatabaseAsync();
        await _repository.RemoveUserFile(1, "file123");

        var file = await _context.UserFiles.FirstOrDefaultAsync(f => f.MongoFileId == "file123");
        Assert.Null(file);
    }

    [Fact]
    public async Task RemoveUserFile_ShouldDoNothing_WhenFileDoesNotExist()
    {
        await _repository.RemoveUserFile(1, "nonexistent");

        var files = await _context.UserFiles.ToListAsync();
        Assert.Empty(files);
    }
    #endregion

    #region File Share Methods
    [Fact]
    public async Task AddFileShare_ShouldAddShareRequest_WhenValidShareProvided()
    {
        var newShare = new PendingFileShare
        {
            MongoFileId = "file789",
            FileName = "file4.txt",
            SenderUserId = 1,
            RecipientUserId = 2,
            IsAccepted = false,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddFileShare(newShare);

        var share = await _context.PendingFileShares.FirstOrDefaultAsync(s => s.MongoFileId == "file789");
        Assert.NotNull(share);
    }

    [Fact]
    public async Task AcceptFileShare_ShouldMarkShareAsAccepted_WhenShareExists()
    {
        await SeedDatabaseAsync();
        var share = await _context.PendingFileShares.FirstAsync();

        await _repository.AcceptFileShare(share);

        Assert.True(share.IsAccepted);
    }

    [Fact]
    public async Task RemoveFileShareAsync_ShouldRemoveShare_WhenShareExists()
    {
        await SeedDatabaseAsync();
        var share = await _context.PendingFileShares.FirstAsync();

        await _repository.RemoveFileShareAsync(share);

        var deletedShare = await _context.PendingFileShares.FindAsync(share.ShareId);
        Assert.Null(deletedShare);
    }
    #endregion
}
