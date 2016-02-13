public static class ComponentIds {
    public const int Destroy = 0;
    public const int GameBoardCache = 1;
    public const int GameBoard = 2;
    public const int GameBoardElement = 3;
    public const int Input = 4;
    public const int Interactive = 5;
    public const int Movable = 6;
    public const int Position = 7;
    public const int Resource = 8;
    public const int Score = 9;
    public const int ScoreValue = 10;
    public const int View = 11;

    public const int TotalComponents = 12;

    public static readonly string[] componentNames = {
        "Destroy",
        "GameBoardCache",
        "GameBoard",
        "GameBoardElement",
        "Input",
        "Interactive",
        "Movable",
        "Position",
        "Resource",
        "Score",
        "ScoreValue",
        "View"
    };

    public static readonly System.Type[] componentTypes = {
        typeof(DestroyComponent),
        typeof(GameBoardCacheComponent),
        typeof(GameBoardComponent),
        typeof(GameBoardElementComponent),
        typeof(InputComponent),
        typeof(InteractiveComponent),
        typeof(MovableComponent),
        typeof(PositionComponent),
        typeof(ResourceComponent),
        typeof(ScoreComponent),
        typeof(ScoreValueComponent),
        typeof(ViewComponent)
    };
}