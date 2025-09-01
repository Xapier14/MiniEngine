namespace MiniEngine;

public static class GameContext
{
    private static GameEngine? _gameEngine;

    public static GameEngine? GameEngine => _gameEngine;

    public static GameEngine GetGameEngine()
    {
        return _gameEngine ??= new GameEngine();
    }
}