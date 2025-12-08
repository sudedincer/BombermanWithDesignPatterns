namespace Bomberman.UI.Scenes
{
    public static class SceneManager
    {
        public static Scene CurrentScene;

        public static void ChangeScene(Scene newScene)
        {
            CurrentScene = newScene;
        }
    }
}