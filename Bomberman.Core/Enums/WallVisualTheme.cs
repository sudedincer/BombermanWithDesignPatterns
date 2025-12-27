namespace Bomberman.Core.Enums
{
    /// <summary>
    /// Visual theme identifier for walls.
    /// Used by UI layer to select appropriate textures.
    /// Core layer stays UI-independent by using enum instead of texture paths.
    /// </summary>
    public enum WallVisualTheme
    {
        Generic,           // Fallback/Unbreakable walls (theme-independent)
        
        // Desert Theme
        DesertBreakable,   // Kumtaşı
        DesertHard,        // Sertleştirilmiş Kil
        
        // City Theme
        CityBreakable,     // Tuğla Duvar
        CityHard,          // Çelik Duvar
        
        // Forest Theme
        ForestBreakable,   // Ahşap Sandık
        ForestHard         // Kalın Kütük
    }
}
