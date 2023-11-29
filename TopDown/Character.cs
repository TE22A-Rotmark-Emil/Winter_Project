
using Raylib_cs;

public class Character
{
    public Rectangle rectangle;
    public float framesInAir;

    public void Reset()
    {
        rectangle.x = 0;
        rectangle.y = 0;
        framesInAir = 0;
    }
}
