using System.Data;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Xml;
using Raylib_cs;
int windowWidth = 1280;
int windowHeight = 720;
int playerWidth = 40;
int playerHeight = 64;
bool gravity = true;
Vector2 movement;
int moveSpeed = 2;

int space = 2;

Random generator = new();

int rand = Random.Shared.Next(1, 2);

int[,] map = new int[15, 15];

int xEnemy = map.GetLength(0) / 4;
int yEnemy = map.GetLength(1) / 4;

for (int y = 3; y < 15; y += 4)
{
    for (int x = 3; x < 15; x += 4)
    {
        if (Random.Shared.Next(2) == 1)
        {
            map[x, y] = 1;
        }
    }
}

for (int y = 0; y < map.GetLength(1); y++)
{
    for (int x = 0; x < map.GetLength(0); x++)
    {
        Console.SetCursorPosition(x * space, y * (space - 1));
        Console.Write(map[x, y]);
    }
}

List<Rectangle> walls = new();



walls.Add(new Rectangle(300, 300, 150, 20));
walls.Add(new Rectangle(500, 300, 15, 200));
walls.Add(new Rectangle(100, 300, 75, 80));

Raylib.InitWindow(windowWidth, windowHeight, "Hi!");
Raylib.SetTargetFPS(60);

Rectangle player = new Rectangle(0, 0, playerWidth, playerHeight);
Rectangle groundCheck = new(0, playerHeight/2, playerWidth, 1);
Vector2 playerCentre = new(playerHeight/2, playerWidth/2);
Texture2D character = Raylib.LoadTexture("TheSillyBlue.png");

string scene = "start";

while (!Raylib.WindowShouldClose())
{

    foreach (Rectangle wall in walls)
    {
        if (Raylib.CheckCollisionRecs(player, wall))
        {
            scene = "start";
        }
    }

    if (scene == "game")
    {
        Raylib.ClearBackground(Color.PINK);
        Raylib.DrawTexturePro(character, new Rectangle(0,0,character.width, character.height), player, new Vector2(0, 0), 0, Color.WHITE);
        Raylib.DrawRectangleRec(groundCheck, Color.ORANGE);

        movement = Vector2.Zero;

        if (Raylib.IsKeyDown(KeyboardKey.KEY_RIGHT)) { movement.X = 1; }
        if (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT)) { movement.X = -1; }
        if (Raylib.IsKeyDown(KeyboardKey.KEY_UP)) { movement.Y = -1; }
        if (Raylib.IsKeyDown(KeyboardKey.KEY_DOWN)) { movement.Y = 1; }

        if (movement.Length() > 0)
        {
            movement = Vector2.Normalize(movement) * moveSpeed;
        }

        player.x += movement.X;
        groundCheck.x = player.x;

        bool IsWalled = WallCheck(player, walls);
        if (IsWalled == true)
        {
            player.x -= movement.X;
        }

        player.y += movement.Y;
        groundCheck.y = player.y+playerHeight;
        IsWalled = WallCheck(player, walls);
        if (IsWalled == true)
        {
            player.y -= movement.Y;
        }

        bool isGrounded = grounded(groundCheck, walls);
        if (isGrounded == true){
            gravity = false;
        }

        if (gravity == true){
            player.y++;
        }

        foreach (Rectangle wall in walls)
        {
            if (Raylib.CheckCollisionRecs(player, wall))
            {

            }

        }

        foreach (Rectangle wall in walls)
        {
            Raylib.DrawRectangleRec(wall, Color.BLACK);
        }
    }

    else if (scene == "start")
    {
        if (Raylib.IsKeyPressed(KeyboardKey.KEY_SPACE))
        {
            scene = "game";
        }
    }

    Raylib.BeginDrawing();



    Raylib.EndDrawing();
}

static bool WallCheck(Rectangle player, List<Rectangle> walls)
{
    foreach (Rectangle wall in walls)
    {
        if (Raylib.CheckCollisionRecs(player, wall))
        {
            return true;
        }
    }
    return false;
}

static bool grounded(Rectangle groundCheck, List<Rectangle> walls){
    foreach (Rectangle wall in walls){
        if (Raylib.CheckCollisionRecs(groundCheck, wall)){
            return true;
        }
    }
    return false;
}