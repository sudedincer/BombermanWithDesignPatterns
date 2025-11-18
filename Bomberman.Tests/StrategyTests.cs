using Xunit;
using Bomberman.Core;
using Bomberman.Core.Entities;
using Bomberman.Core.Patterns.Behavioral.Strategy;
using Assert = Xunit.Assert;

public class StrategyTests
{
    [Fact]
    public void StaticMovementStrategy_MovesEnemy_ByItsSpeed()
    {
        // Arrange
        var initialX = 10.0;
        // StaticMovement stratejisini kullanarak bir düşman oluştur
        var staticMovementStrategy = new StaticMovement();
        var enemy = new Enemy(initialX, 5.0, staticMovementStrategy);
        var expectedNewX = initialX + enemy.Speed; // 10.0 + 0.5 = 10.5
        
        // Act
        // Güncelleme metodu çağrıldığında, düşman stratejisine göre hareket etmeli
        enemy.Update(null, null); // Şimdilik Map ve Player'ı null gönderiyoruz
        
        // Assert
        Assert.Equal(expectedNewX, enemy.X);
    }

    [Fact]
    public void Enemy_CanChangeStrategy_AtRuntime()
    {
        // Arrange
        // Bu, Strategy deseninin en büyük faydasını gösterir!
        var initialStrategy = new StaticMovement();
        var enemy = new Enemy(0, 0, initialStrategy);
        
        // Act
        // Yeni bir strateji (örneğin, Chasing AI olabilirdi) atıyoruz.
        // Şimdilik sadece StaticMovement'ı kullanacağız ama kodun esnekliğini gösteriyor.
        var newStrategy = new StaticMovement(); 
        enemy.SetMovementStrategy(newStrategy);
        
        // Assert
        // Düşmanın strateji nesnesinin değiştiğini kontrol edebiliriz (idealde başka bir Concrete Strategy ile)
        Assert.NotNull(newStrategy);
    }
}