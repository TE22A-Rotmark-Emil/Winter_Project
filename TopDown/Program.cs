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
int powerUpTimer = 0;
float gravityValue = 0;
float gravityBase = 2;
float secondsInAir = 0;
bool dead = true;
bool coyoteTiming = true;
bool gravity = true;
bool isJumping = false;
bool IsWalled = false;
// bool canMove = true;
Vector2 movement;

//todo: add proper support for powerups (turns out they don't work if you hit a wall without your feet colliding with the wall)

Rectangle spawnPoint = new(0, 0, 0, 0);

List<Rectangle> deathSquares = new(){
    new(100, 300, 80, 80),
};

List<Rectangle> winSquares = new(){
    new(800, 300, 80, 80)
};

List<Rectangle> wallsForLevel1 = new()
{
    new(300, 300, 150, 20),
    new(300, 400, 150, 20),
    new(500, 300, 15, 200),
    new(600, 300, 15, 80),
    new(800, 450, 80, 15),
    new(1000, 450, 15, 15),
    new(1200, 450, 15, 15)
};

List<Rectangle> wallsForLevel2 = new()
{
    new(600, 400, 150, 20)
};

List<Rectangle> walls = new(){

};

List<Rectangle> boostsForLevel1 = new(){
    new(1100, 400, 20, 20)
};

List<Rectangle> boosts = new(){

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

    if (scene == "game")
    {
        Raylib.ClearBackground(Color.PINK);
        Raylib.DrawTexturePro(character, new Rectangle(0,0, character.width, character.height), player, new Vector2(0, 0), 0, Color.WHITE);
        // Raylib.DrawRectangleRec(player, Color.DARKBLUE);
        // Raylib.DrawRectangleRec(groundCheck, Color.ORANGE);
        // Raylib.DrawRectangleRec(headCheck, Color.BLUE);

        movement = Vector2.Zero;

        if (powerUpTimer > 0){
            if (Raylib.IsKeyDown(KeyboardKey.KEY_C)){
                powerUpTimer = 0;
            }
            else{
                powerUpTimer--;
                player.x += 5;
                if (isJumping == true){
                    player.y -= gravityValue - jumpHeight;
                }
                else{
                    player.y -= gravityValue;
                }
                (groundCheck.x, groundCheck.y, headCheck.x, headCheck.y) = Resync(groundCheck.x, groundCheck.y, headCheck.x, headCheck.y, player.x, player.y, playerHeight);
            }
        }

        if (Raylib.IsKeyDown(KeyboardKey.KEY_RIGHT)) { movement.X = 1 * moveSpeed; }
        if (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT)) { movement.X = -1 * moveSpeed; }
        if (Raylib.IsKeyDown(KeyboardKey.KEY_UP) && coyoteTiming == true) {
            jump = 1;
            coyoteTiming = false;
            isJumping = true;
        }
        if (jump > 0 && isJumping == true){ // deleted "|| gravity == true && isJumping == true" (unclear purpose)
            if (WallCheck(headCheck, walls) == true){
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

        if (dead == true){
            (player.x, player.y, framesInAir, jump, secondsInAir, coyoteTiming) = ResetValues(player.x, player.y, framesInAir, jump, secondsInAir, coyoteTiming, spawnPoint, player);
            powerUpTimer = 0;
            dead = false;
        }


        if (WallCheck(player, walls) == true)
        {
            if (WallCheck(headCheck, walls) == false){
                IsWalled = true;
            }
            player.x -= movement.X;
            (groundCheck.x, groundCheck.y, headCheck.x, headCheck.y) = Resync(groundCheck.x, groundCheck.y, headCheck.x, headCheck.y, player.x, player.y, playerHeight);
        }
        else{
            IsWalled = false;
        }

        if (WallCheck(player, deathSquares) == true){
            dead = true;
        }

        bool isGrounded = grounded(groundCheck, walls);

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
                dead = true;
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
        (groundCheck.x, groundCheck.y, headCheck.x, headCheck.y) = Resync(groundCheck.x, groundCheck.y, headCheck.x, headCheck.y, player.x, player.y, playerHeight);
        if (WallCheck(player, walls) == true){
            if (WallCheck(headCheck, walls) == false){
                IsWalled = true;
            }
        }
        else{
            IsWalled = false;
        }
        
        if (IsWalled == true){
            if (gravity == true){
                gravity = false;
            }
            Rectangle collisionRectangle;
            foreach (Rectangle wall in walls){
                if (powerUpTimer > 0){
                    Console.WriteLine("Disabled powerup");
                    powerUpTimer = 0;
                }
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
            boosts.Clear();
            walls.Clear();
            walls = walls.Concat(wallsForLevel2).ToList();
            spawnPoint = new(300, 500, 150, 20);
            walls.Add(spawnPoint);
            dead = true;
            gravity = false;
        }

        if (PowerUp(player, boosts) == true){
            List<Rectangle> collidedBoosts = new();
            foreach (Rectangle boost in boosts){
                if (Raylib.CheckCollisionRecs(player, boost))
                {
                    collidedBoosts.Add(boost);
                }
            }
            foreach (Rectangle collidedBoost in collidedBoosts){
                boosts.Remove(collidedBoost);
            }
            powerUpTimer = 120;
        }

        foreach (Rectangle wall in walls)
        {
            Raylib.DrawRectangleRec(wall, Color.BLACK);
        }

        foreach (Rectangle boost in boosts){
            Raylib.DrawRectangleRec(boost, Color.WHITE);
        }

        Raylib.DrawRectangleRec(spawnPoint, Color.GREEN);

        foreach (Rectangle deathSquare in deathSquares){
            Raylib.DrawRectangleRec(deathSquare, Color.RED);
        }
    }

    if (level == 2){
        foreach (Rectangle wall in wallsForLevel2){
            Raylib.DrawRectangleRec(wall, Color.GRAY);
        }

        foreach (Rectangle winSquare in winSquares){
            Raylib.DrawRectangleRec(winSquare, Color.GOLD);
        }

        if (WallCheck(player, winSquares) == true){
            scene = "win";
        }
    }

    if (scene == "start")
    {
        Raylib.DrawText("Press Space to Begin", windowWidth/3+48, windowHeight/2+windowHeight/3, 32, Color.GRAY);
        if (Raylib.IsKeyPressed(KeyboardKey.KEY_SPACE))
        {
            level = 1;
            scene = "game";
            spawnPoint = new(300, 300, 150, 20);
            boosts = boosts.Concat(boostsForLevel1).ToList();
            walls.Add(spawnPoint);
            walls = walls.Concat(wallsForLevel1).ToList();
        }
    }

    if (scene == "win"){
        level = 0;
        Raylib.ClearBackground(Color.BLACK);
        Raylib.DrawText("Win Get! (space to restart)", windowWidth/4, windowHeight/2, 48, Color.GOLD);
        if (Raylib.IsKeyPressed(KeyboardKey.KEY_SPACE)){
            level = 1;
            scene = "game";
            walls.Clear();
            walls = walls.Concat(wallsForLevel1).ToList();
            boosts = boosts.Concat(boostsForLevel1).ToList();
            spawnPoint = new(300, 300, 150, 20);
            walls.Add(spawnPoint);
            dead = true;
        }
    }

    Raylib.BeginDrawing();

    Raylib.EndDrawing();
}

static (float, float, int, int, float, bool) ResetValues(float X, float Y, int frames, int jump, float seconds, bool coyote, Rectangle spawnPoint, Rectangle player)
{
    X = spawnPoint.x + spawnPoint.width/2 - player.width/2;
    Y = spawnPoint.y - player.height;
    frames = 0;
    jump = 0;
    seconds = 0;
    coyote = true;

    return(X, Y, frames, jump, seconds, coyote);
}

static (float, float, float, float) Resync(float groundCheckX, float groundCheckY, float headCheckX, float headCheckY, float playerX, float playerY, int playerHeight){
    groundCheckX = playerX;
    groundCheckY = playerY+playerHeight;
    headCheckX = playerX;
    headCheckY = playerY-5;

    return(groundCheckX, groundCheckY, headCheckX, headCheckY);
}

groundCheck.x = player.x;
headCheck.x = player.x;
groundCheck.y = player.y+playerHeight;
headCheck.y = player.y-5;

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

static bool grounded(Rectangle groundCheck, List<Rectangle> walls){
    foreach (Rectangle wall in walls){
        if (Raylib.CheckCollisionRecs(groundCheck, wall)){
            return true;
        }
    }
    return false;
}