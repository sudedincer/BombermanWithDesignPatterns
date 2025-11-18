using Xunit;
using Bomberman.Core;
using Assert = Xunit.Assert;

public class DecoratorTests
{
    [Fact]
    public void SpeedBoostDecorator_IncreasesSpeed_By20Percent()
    {
        // Arrange
        IPlayer p1 = new BasePlayer();
        double initialSpeed = p1.GetSpeed(); // 3.0
        
        // Act
        // Oyuncuyu SpeedBoostDecorator ile sarmala
        p1 = new SpeedBoostDecorator(p1);
        double boostedSpeed = p1.GetSpeed(); 
        
        // Assert
        
        // *******************************************************************
        // DEĞİŞİKLİK BURADA: Sapmaya izin vermek için hassasiyet (tolerance) ekliyoruz.
        // Dördüncü parametre (0.0001), 4 ondalık basamağa kadar tolerans tanır.
        Assert.Equal(3.6, boostedSpeed, 4); 
        // *******************************************************************
        
        // Maksimum bomba sayısının değişmediğini kontrol et (bu testte hala 1 olmalı)
        Assert.Equal(1, p1.GetMaxBombs());
    }
}