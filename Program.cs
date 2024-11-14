using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

class MazeGame
{
    static Stopwatch stopwatch = new Stopwatch();
    static int playerX = 0, playerY = 0; // Player's position
    static int score = 0; // Player's score
    static string[] mazeRows;
    static char[][] mazeChar;

    // Number of bad guys and coins
    const int numberOfBadGuys = 20;
    const int numberOfCoins = 20;

    // Bad guy positions
    static int[] badGuy_X = new int[numberOfBadGuys];
    static int[] badGuy_Y = new int[numberOfBadGuys];

    // Coin positions
    static int[] coin_X = new int[numberOfCoins];
    static int[] coin_Y = new int[numberOfCoins];

    static void Main()
    {
    
        // Load maze layout from maze.txt
        mazeRows = File.ReadAllLines("maze.txt");
        mazeChar = mazeRows.Select(item => item.ToArray()).ToArray();

        Console.Clear();
        DisplayInstructions();

        stopwatch.Start(); // Start the timer

        // Place bad guys and coins in the maze
        PlaceBadGuysAndCoins();

        // Display maze
        PrintMaze();

        do
        {
            var key = Console.ReadKey(true).Key;
            if (key == ConsoleKey.Escape) // Exit game on Escape key
            {
                Environment.Exit(0);
            }
            ProcessPlayerMove(key);
            UpdateBadGuys();
            CheckCoinCollection();
            CheckWinCondition();
            PrintMaze();
        } while (true);
    }

    static void DisplayInstructions()
    {
        Console.WriteLine("Welcome to the Maze Game!");
        Console.WriteLine("Navigate the maze using the arrow keys.");
        Console.WriteLine("Collect all the '^' coins to unlock the treasure.");
        Console.WriteLine("Avoid the bad guys '%'.");
        Console.WriteLine("Press Escape to exit the game.");
        Console.WriteLine("Press any key to start.");
        Console.ReadKey(true);
        Console.Clear();
    }

    static void PrintMaze()
    {
        Console.Clear();
        for (int i = 0; i < mazeRows.Length; i++)
        {
            Console.WriteLine(new string(mazeChar[i]));
        }

        // Display player position
        if (playerY >= 0 && playerY < mazeChar.Length && playerX >= 0 && playerX < mazeChar[playerY].Length)
        {
            Console.SetCursorPosition(playerX, playerY);
            Console.Write("P"); // Mark the player's position with 'P'
        }

        // Display the score below the maze
        Console.SetCursorPosition(0, mazeRows.Length + 1);
        Console.WriteLine($"Score: {score}");
    }

    static void ProcessPlayerMove(ConsoleKey key)
    {
        int newX = playerX, newY = playerY;

        switch (key)
        {
            case ConsoleKey.UpArrow:
                newY--;
                break;
            case ConsoleKey.DownArrow:
                newY++;
                break;
            case ConsoleKey.LeftArrow:
                newX--;
                break;
            case ConsoleKey.RightArrow:
                newX++;
                break;
        }

        // Ensure player stays inside the maze and doesn't walk through walls
        if (IsValidMove(newX, newY))
        {
            playerX = newX;
            playerY = newY;
        }
    }

    static bool IsValidMove(int newX, int newY)
    {
        if (newX < 0 || newY < 0 || newY >= mazeChar.Length || newX >= mazeChar[newY].Length)
            return false; // Out of bounds
        if (mazeChar[newY][newX] == '*' || mazeChar[newY][newX] == '#')
            return false; // Can't move through walls or the treasure
        return true;
    }

    static void UpdateBadGuys()
    {
        // Move the bad guys (represented by '%')
        for (int i = 0; i < numberOfBadGuys; i++)
        {
            if (badGuy_X[i] >= 0 && badGuy_Y[i] >= 0 && badGuy_Y[i] < mazeChar.Length && badGuy_X[i] < mazeChar[badGuy_Y[i]].Length)
            {
                // Remove the bad guy ('%')
                mazeChar[badGuy_Y[i]][badGuy_X[i]] = ' ';

                // Update the bad guy's position (e.g., move randomly or in a predefined direction)
                Random rand = new Random();
                int moveDirection = rand.Next(4); // 0=up, 1=down, 2=left, 3=right

                int newX = badGuy_X[i], newY = badGuy_Y[i];
                switch (moveDirection)
                {
                    case 0: // Move up
                        newY--;
                        break;
                    case 1: // Move down
                        newY++;
                        break;
                    case 2: // Move left
                        newX--;
                        break;
                    case 3: // Move right
                        newX++;
                        break;
                }

                // Ensure the bad guy stays within bounds and doesn't collide with walls
                if (IsValidMove(newX, newY))
                {
                    badGuy_X[i] = newX;
                    badGuy_Y[i] = newY;
                    mazeChar[badGuy_Y[i]][badGuy_X[i]] = '%'; // Place the bad guy ('%') at the new position
                }
            }
        }
    }

    static void CheckCoinCollection()
    {
        // If player moves over a coin, collect it
        for (int i = 0; i < numberOfCoins; i++)
        {
            if (playerX == coin_X[i] && playerY == coin_Y[i])
            {
                score += 100;
                mazeChar[coin_Y[i]][coin_X[i]] = ' '; // Remove the coin
                coin_X[i] = coin_Y[i] = -1; // Mark coin as collected
            }
        }

        // Open the door if all coins are collected
        if (score == numberOfCoins * 100)
        {
            for (int i = 0; i < mazeChar.Length; i++)
            {
                for (int j = 0; j < mazeChar[i].Length; j++)
                {
                    if (mazeChar[i][j] == '#')
                    {
                        mazeChar[i][j] = ' '; // Open the gate (remove '#')
                    }
                }
            }
        }
    }

    static void CheckWinCondition()
    {
        // Check if the player reached the treasure
        if (mazeChar[playerY][playerX] == '#')
        {
            stopwatch.Stop();
            Console.Clear();
            Console.WriteLine("Congratulations, you won!");
            Console.WriteLine($"Your time: {stopwatch.Elapsed}");
            Console.WriteLine($"Your score: {score}");
            Environment.Exit(0);
        }
    }

    // Place bad guys and coins in random positions in the maze
    static void PlaceBadGuysAndCoins()
    {
        Random rand = new Random();

        // Place bad guys
        for (int i = 0; i < numberOfBadGuys; i++)
        {
            bool validBadGuy = false;
            while (!validBadGuy)
            {
                int tempX = rand.Next(mazeChar[0].Length);
                int tempY = rand.Next(mazeChar.Length);

                if (mazeChar[tempY][tempX] == ' ' || mazeChar[tempY][tempX] == '^') // Bad guys can only be placed in empty spaces or where coins are
                {
                    badGuy_X[i] = tempX;
                    badGuy_Y[i] = tempY;
                    mazeChar[tempY][tempX] = '%'; // Place bad guy
                    validBadGuy = true;
                }
            }
        }

        // Place coins
        for (int i = 0; i < numberOfCoins; i++)
        {
            bool validCoin = false;
            while (!validCoin)
            {
                int tempX = rand.Next(mazeChar[0].Length);
                int tempY = rand.Next(mazeChar.Length);

                if (mazeChar[tempY][tempX] == ' ') // Coins can only be placed in empty spaces
                {
                    coin_X[i] = tempX;
                    coin_Y[i] = tempY;
                    mazeChar[tempY][tempX] = '^'; // Place coin
                    validCoin = true;
                }
            }
        }
    }
}
