namespace ConsoleRpg.Helpers;

public class MenuManager
{
    private readonly OutputManager _outputManager;

    public MenuManager(OutputManager outputManager)
    {
        _outputManager = outputManager;
    }

    public bool ShowMainMenu()
    {
        _outputManager.WriteLine("Welcome to the RPG Game!", ConsoleColor.Yellow);
        _outputManager.WriteLine("1. Start Game", ConsoleColor.Cyan);
        _outputManager.WriteLine("2. Exit", ConsoleColor.Cyan);
        _outputManager.Display();

        return HandleMainMenuInput();
    }

    private bool HandleMainMenuInput()
    {
        while (true)
        {
            var input = Console.ReadLine();
            switch (input)
            {
                case "1":
                    _outputManager.WriteLine("Starting game...", ConsoleColor.Green);
                    _outputManager.Display();
                    return true;
                case "2":
                    _outputManager.WriteLine("Exiting game...", ConsoleColor.Red);
                    _outputManager.Display();
                    Environment.Exit(0);
                    return false;
                default:
                    _outputManager.WriteLine("Invalid selection. Please choose 1 or 2.", ConsoleColor.Red);
                    _outputManager.Display();
                    break;
            }
        }
    }

    public int ShowInventoryMenu()
    {
        _outputManager.WriteLine("\nInventory Management:", ConsoleColor.Yellow);
        _outputManager.WriteLine("1. Search for item by name", ConsoleColor.Cyan);
        _outputManager.WriteLine("2. List items by type", ConsoleColor.Cyan);
        _outputManager.WriteLine("3. Sort items", ConsoleColor.Cyan);
        _outputManager.WriteLine("0. Back", ConsoleColor.Cyan);
        _outputManager.Display();

        while (true)
        {
            _outputManager.Write("Select an option: ", ConsoleColor.White);
            _outputManager.Display();
            var input = Console.ReadLine();
            if (int.TryParse(input, out var choice) && choice is >= 0 and <= 3)
                return choice;

            _outputManager.WriteLine("Invalid selection. Try again.", ConsoleColor.Red);
            _outputManager.Display();
        }
    }

    public int ShowSortSubmenu()
    {
        _outputManager.WriteLine("\nSort Options:", ConsoleColor.Yellow);
        _outputManager.WriteLine("1. Sort by Name", ConsoleColor.Cyan);
        _outputManager.WriteLine("2. Sort by Attack Value", ConsoleColor.Cyan);
        _outputManager.WriteLine("3. Sort by Defense Value", ConsoleColor.Cyan);
        _outputManager.Display();

        while (true)
        {
            _outputManager.Write("Choose 1-3: ", ConsoleColor.White);
            var input = Console.ReadLine();
            if (int.TryParse(input, out var choice) && choice is >= 1 and <= 3)
                return choice;

            _outputManager.WriteLine("Invalid selection. Try again.", ConsoleColor.Red);
            _outputManager.Display();
        }
    }
}
