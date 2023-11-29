using System.Data;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Xml;
using Raylib_cs;
int windowWidth = 1280;
int windowHeight = 720;
int playerWidth = 40;
int playerHeight = 64;
int framesInAir = 0;
int jump = 0;
int jumpHeight = 3;
float gravityValue;
float gravityBase = 2;
float secondsInAir = 0;
bool coyoteTiming = true;
bool gravity = true;
bool isJumping = false;
Vector2 movement;
int moveSpeed = 2;

Character hero = new Character();

hero.Reset();

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
Rectangle groundCheck = new(0, playerHeight/2, playerWidth/(float)1.06, 1);
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
        if (Raylib.IsKeyDown(KeyboardKey.KEY_UP) && coyoteTiming == true) {
            jump = 90;
            coyoteTiming = false;
            isJumping = true;
        }
        if (jump > 0 && isJumping == true || gravity == true && isJumping == true){
            player.y -= jumpHeight;
            jump--;
        }
        else{
            isJumping = false;
        }

        player.x += movement.X;
        groundCheck.x = player.x + 1;

        bool IsWalled = WallCheck(player, walls);
        if (IsWalled == true)
        {
            player.x -= movement.X;
        }


        bool isGrounded = grounded(groundCheck, walls);
        if (isGrounded == true){
            gravity = false;
        }


        else if (isGrounded == false){
            gravity = true;
        }

        if (gravity == true){
            framesInAir++;
            if (framesInAir > 1 && framesInAir < 61){
                secondsInAir += (float)1/60;
            }
            if (framesInAir > 60){
                framesInAir = 0;
            }
            if (coyoteTiming == true && framesInAir > 30){
                coyoteTiming = false;
            }
            gravityValue = (float)gravityBase*secondsInAir*secondsInAir;
            movement.Y += gravityValue;
            if (player.y > 700){
                (player.x, player.y, framesInAir, secondsInAir, coyoteTiming) = ResetValues(player.x, player.y, framesInAir, secondsInAir, coyoteTiming);
            }
        }
        else if (gravity == false){
            if (coyoteTiming == false){
                coyoteTiming = true;
            }
            if (secondsInAir > 0 || framesInAir > 0){
                secondsInAir = 0;
                framesInAir = 0;
            }
        }

        player.y += movement.Y;
        groundCheck.y = player.y+playerHeight;
        IsWalled = WallCheck(player, walls);
        
        while (IsWalled == true){
            if (gravity == true){
                gravity = false;
            }
            player.y--;
            IsWalled = WallCheck(player, walls);
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
static (float, float, int, float, bool) ResetValues(float X, float Y, int frames, float seconds, bool coyote){
    X = 0;
    Y = 0;
    frames = 0;
    seconds = 0;
    coyote = true;

    return(X, Y, frames, seconds, coyote);
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