using System.Data;
using System.Diagnostics;
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
int moveSpeed = 2;
int level = 0;
float gravityValue;
float gravityBase = 2;
float secondsInAir = 0;
bool coyoteTiming = true;
bool gravity = true;
bool isJumping = false;
bool IsWalled = false;
Vector2 movement;

Rectangle spawnPoint = new(300, 300, 150, 20);

List<Rectangle> walls = new()
{
    new(300, 300, 150, 20),
    new(300, 400, 150, 20),
    new(500, 300, 15, 200),
    new(100, 300, 75, 80),
    new(600, 300, 15, 80),
    new(800, 450, 80, 15),
    new(1000, 450, 15, 15),
    new(1200, 450, 15, 15)
};

List<Rectangle> wallsForLevel2 = new()
{
    
};

walls.Add(spawnPoint);

List<Rectangle> boosts = new(){
    new(1100, 400, 10, 10)
};

Raylib.InitWindow(windowWidth, windowHeight, "Hi!");
Raylib.SetTargetFPS(60);

Rectangle player = new Rectangle(0, 0, playerWidth, playerHeight);
Rectangle groundCheck = new(0, playerHeight/2, playerWidth, 1);
Rectangle headCheck = new(0, playerHeight, playerWidth, 10);
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

    if (scene != "start")
    {
        Raylib.ClearBackground(Color.PINK);
        Raylib.DrawTexturePro(character, new Rectangle(0,0,character.width, character.height), player, new Vector2(0, 0), 0, Color.WHITE);
        Raylib.DrawRectangleRec(player, Color.DARKBLUE);
        Raylib.DrawRectangleRec(groundCheck, Color.ORANGE);
        Raylib.DrawRectangleRec(headCheck, Color.BLUE);

        movement = Vector2.Zero;

        if (Raylib.IsKeyDown(KeyboardKey.KEY_RIGHT)) { movement.X = 1 * moveSpeed; }
        if (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT)) { movement.X = -1 * moveSpeed; }
        if (Raylib.IsKeyDown(KeyboardKey.KEY_UP) && coyoteTiming == true) {
            jump = 1;
            coyoteTiming = false;
            isJumping = true;
        }
        if (jump > 0 && isJumping == true){ // deleted "|| gravity == true && isJumping == true" (unclear purpose)
            if (notWallCheck(headCheck, walls) == true){
                jump = 0;
                gravity = true;
            }
            else{
                player.y -= jumpHeight;
            }
        }
        else{
            isJumping = false;
        }

        player.x += movement.X;
        groundCheck.x = player.x;
        headCheck.x = player.x;

        if (WallCheck(player, walls) == true)
        {
            if (notWallCheck(headCheck, walls) == false){
                IsWalled = true;
            }
            player.x -= movement.X;
        }


        bool isGrounded = grounded(groundCheck, walls);
        // if (isGrounded == true && WallCheck(player, walls) == false){
        //     gravity = false;
        // }

        if (isGrounded == false){
            gravity = true;
        }

        if (isGrounded == true){
            jump = 0;
            gravity = false;
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
                (player.x, player.y, framesInAir, secondsInAir, coyoteTiming) = ResetValues(player.x, player.y, framesInAir, secondsInAir, coyoteTiming, spawnPoint, player);
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
        headCheck.y = player.y-5;
        if (WallCheck(player, walls) == true){
            if (notWallCheck(headCheck, walls) == false){
                IsWalled = true;
            }
        }
        
        if (IsWalled == true){
            if (gravity == true){
                gravity = false;
            }
            Rectangle collisionRectangle;
            foreach (Rectangle wall in walls){
                collisionRectangle = Raylib.GetCollisionRec(player, wall);
                if (collisionRectangle.y > 0){
                    player.y -= collisionRectangle.height;
                    headCheck.y -= collisionRectangle.height;
                    groundCheck.y -= collisionRectangle.height;
                }
            }
        }

        if (player.x > 1280 && level == 1){
            level = 2;
            walls.Clear();
            walls = walls.Concat(wallsForLevel2).ToList();
            spawnPoint = new(300, 500, 150, 20);
            walls.Add(spawnPoint);
            player.y = spawnPoint.y - player.height;
            player.x = spawnPoint.x + spawnPoint.width/2;
            gravity = false;
        }


        foreach (Rectangle wall in walls)
        {
            Raylib.DrawRectangleRec(wall, Color.BLACK);
        }

        foreach (Rectangle boost in boosts){
            Raylib.DrawRectangleRec(boost, Color.WHITE);
        }

        Raylib.DrawRectangleRec(spawnPoint, Color.GREEN);
    }

    if (level == 2){
        foreach (Rectangle wall in wallsForLevel2){
            Raylib.DrawRectangleRec(wall, Color.GRAY);
        }
    }

    else if (scene == "start")
    {
        if (Raylib.IsKeyPressed(KeyboardKey.KEY_SPACE))
        {
            level = 1;
            scene = "game";
        }
    }

    Raylib.BeginDrawing();

    Raylib.EndDrawing();
}
static (float, float, int, float, bool) ResetValues(float X, float Y, int frames, float seconds, bool coyote, Rectangle spawnPoint, Rectangle player)
{
    X = spawnPoint.x + spawnPoint.width/2 - player.width/2;
    Y = spawnPoint.y - player.height;
    frames = 0;
    seconds = 0;
    coyote = true;

    return(X, Y, frames, seconds, coyote);
}

static bool PowerUp(Rectangle player, List<Rectangle> boosts){
    foreach (Rectangle boost in boosts){
        if (Raylib.CheckCollisionRecs(player, boost)){
            return true;
        }
    }
    return false;
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

static bool notWallCheck(Rectangle headCheck, List<Rectangle> walls){
    foreach (Rectangle wall in walls){
        if (Raylib.CheckCollisionRecs(headCheck, wall)){
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