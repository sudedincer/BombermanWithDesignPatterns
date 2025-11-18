using Xunit;
using Bomberman.Services.Data;
using System.Threading.Tasks;
using System.IO;
using Assert = Xunit.Assert;

public class RepositoryTests
{
    [Fact]
    public async Task AddUser_And_RetrieveUser_ShouldSucceed()
    {
        // Arrange
        // Test için geçici bir SQLite dosyası oluştur.
        var dbPath = Path.Combine(Path.GetTempPath(), $"test_bomberman_{Guid.NewGuid()}.db");
        var repository = new SqliteUserRepository(dbPath);
        
        var newUser = new User { Username = "TestUser", PasswordHash = "securehash" };
        
        // Act
        // Kullanıcıyı ekle
        var success = await repository.AddUserAsync(newUser);
        
        // Eklenen kullanıcıyı geri oku
        var retrievedUser = await repository.GetByUsernameAsync("TestUser");
        
        // Assert
        Assert.True(success);
        Assert.NotNull(retrievedUser);
        Assert.Equal("TestUser", retrievedUser.Username);
        
        // Cleanup (Temizlik)
        File.Delete(dbPath);
    }
}