
public enum State
{
    Run,
    Slide,
    Jump
}

public enum Layer
{
    //built in layers - unity provides these
    Default = 0,
    TransparentFX,
    IgnoreRaycast,
    Water = 4,
    UI,
    //User Layers - We define these
    Palyer = 8,
    Obstacle
}
