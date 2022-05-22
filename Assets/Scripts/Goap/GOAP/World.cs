#if UNITY_EDITOR
#endif

public sealed class World
{
    private static readonly World instance = new();
    private static WorldStates world;
    static World()
    {
        world = new();
    }

    private World()
    {

    }

    public static World Instance { get => instance; }

    public WorldStates GetWorld() => world;
}