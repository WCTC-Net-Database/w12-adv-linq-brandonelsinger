using ConsoleRpg.Helpers;
using ConsoleRpgEntities.Data;
using ConsoleRpgEntities.Models.Attributes;
using ConsoleRpgEntities.Models.Characters;
using ConsoleRpgEntities.Models.Characters.Monsters;
using ConsoleRpgEntities.Models.Equipments;
using Microsoft.EntityFrameworkCore;

namespace ConsoleRpg.Services;

public class GameEngine
{
    private readonly GameContext _context;
    private readonly MenuManager _menuManager;
    private readonly OutputManager _outputManager;

    private IPlayer _player;
    private IMonster _goblin;

    public GameEngine(GameContext context, MenuManager menuManager, OutputManager outputManager)
    {
        _menuManager = menuManager;
        _outputManager = outputManager;
        _context = context;
    }

    public void Run()
    {
        if (_menuManager.ShowMainMenu())
        {
            SetupGame();
        }
    }

    private void GameLoop()
    {
        _outputManager.Clear();

        while (true)
        {
            _outputManager.WriteLine("Choose an action:", ConsoleColor.Cyan);
            _outputManager.WriteLine("1. Attack");
            _outputManager.WriteLine("2. Inventory");
            _outputManager.WriteLine("3. Quit");

            _outputManager.Display();

            var input = Console.ReadLine();

            switch (input)
            {
                case "1":
                    AttackCharacter();
                    break;
                case "2":
                    InventoryLoop();
                    break;
                case "3":
                    _outputManager.WriteLine("Exiting game...", ConsoleColor.Red);
                    _outputManager.Display();
                    Environment.Exit(0);
                    break;
                default:
                    _outputManager.WriteLine("Invalid selection. Please choose 1-3.", ConsoleColor.Red);
                    _outputManager.Display();
                    break;
            }
        }
    }

    private void InventoryLoop()
    {
        while (true)
        {
            var choice = _menuManager.ShowInventoryMenu();
            switch (choice)
            {
                case 1:
                    DoSearch();
                    break;
                case 2:
                    DoListByType();
                    break;
                case 3:
                    DoSort();
                    break;
                case 0:
                    _outputManager.Clear();
                    return;
            }
        }
    }

    private void DoSearch()
    {
        _outputManager.Write("Enter search term: ", ConsoleColor.White);
        _outputManager.Display();
        var term = Console.ReadLine() ?? string.Empty;

        var items = GetPlayerItems()
            .Where(i => i.Name.Contains(term, StringComparison.OrdinalIgnoreCase))
            .OrderBy(i => i.Name)
            .ThenBy(i => i.Id)
            .ToList();

        _outputManager.Clear();
        _outputManager.WriteLine("Search Results:", ConsoleColor.Yellow);
        if (items.Count == 0)
        {
            _outputManager.WriteLine("No matching items.", ConsoleColor.Yellow);
        }
        else
        {
            for (int i = 0; i < items.Count; i++)
                _outputManager.WriteLine($"{i + 1}. {FormatItem(items[i])}", ConsoleColor.Cyan);
        }
        _outputManager.Display();
        Pause();
    }

    private void DoListByType()
    {
        var groups = GetPlayerItems()
            .GroupBy(i => i.Type)
            .OrderBy(g => g.Key)
            .ToList();

        _outputManager.Clear();
        _outputManager.WriteLine("Items by Type:", ConsoleColor.Yellow);

        if (groups.Count == 0)
        {
            _outputManager.WriteLine("No items.", ConsoleColor.Yellow);
        }
        else
        {
            foreach (var g in groups)
            {
                _outputManager.WriteLine($"[{g.Key}] ({g.Count()})", ConsoleColor.Green);
                foreach (var it in g.OrderBy(i => i.Name))
                    _outputManager.WriteLine($" - {FormatItem(it)}", ConsoleColor.Cyan);
            }
        }

        _outputManager.Display();
        Pause();
    }

    private void DoSort()
    {
        var sort = _menuManager.ShowSortSubmenu();

        IEnumerable<Item> ordered = sort switch
        {
            1 => GetPlayerItems().OrderBy(i => i.Name).ThenBy(i => i.Id),
            2 => GetPlayerItems().OrderByDescending(i => i.Attack).ThenBy(i => i.Name),
            3 => GetPlayerItems().OrderByDescending(i => i.Defense).ThenBy(i => i.Name),
            _ => GetPlayerItems()
        };

        var list = ordered.ToList();

        _outputManager.Clear();
        _outputManager.WriteLine("Sorted Items:", ConsoleColor.Yellow);
        if (list.Count == 0)
        {
            _outputManager.WriteLine("No items.", ConsoleColor.Yellow);
        }
        else
        {
            for (int i = 0; i < list.Count; i++)
                _outputManager.WriteLine($"{i + 1}. {FormatItem(list[i])}", ConsoleColor.Cyan);
        }
        _outputManager.Display();
        Pause();
    }

    private void AttackCharacter()
    {
        if (_goblin is ITargetable targetableGoblin)
        {
            _player.Attack(targetableGoblin);
            _player.UseAbility(_player.Abilities.First(), targetableGoblin);
        }
    }
    private void SetupGame()
    {
        // Load a player and ensure inventory/equipment are available for LINQ ops.
        _player = _context.Players
            .Include(p => p.Inventory)
            .ThenInclude(inv => inv.Items)
            .Include(p => p.Equipment)
            .ThenInclude(e => e.Weapon)
            .Include(p => p.Equipment)
            .ThenInclude(e => e.Armor)
            .FirstOrDefault();

        _outputManager.WriteLine($"{_player.Name} has entered the game.", ConsoleColor.Green);

        EnsureDemoItemsInInventory();

        // Load monsters into random rooms 
        LoadMonsters();

        // Pause before starting the game loop
        Thread.Sleep(500);
        GameLoop();
    }

    private void LoadMonsters()
    {
        _goblin = _context.Monsters.OfType<Goblin>().FirstOrDefault();
    }

    private IEnumerable<Item> GetPlayerItems()
    {
        var p = (ConsoleRpgEntities.Models.Characters.Player)_player;
        return p.Inventory?.Items ?? Enumerable.Empty<Item>();
    }

    private static string FormatItem(Item i) =>
        $"{i.Name} [Type={i.Type}, ATK={i.Attack}, DEF={i.Defense}, WGT={i.Weight}, VAL={i.Value}]";

    private static void Pause()
    {
        Console.Write("\nPress Enter to continue...");
        Console.ReadLine();
    }

    private void EnsureDemoItemsInInventory()
    {
        var concrete = (ConsoleRpgEntities.Models.Characters.Player)_player;

        if (concrete.Inventory == null)
            concrete.Inventory = new ConsoleRpgEntities.Models.Equipments.Inventory { Items = new List<Item>() };

        if (concrete.Inventory.Items == null)
            concrete.Inventory.Items = new List<Item>();

        if (concrete.Inventory.Items.Any())
            return;

        var all = _context.Items.AsNoTracking().ToList();

        var weapon = all.Where(i => i.Type.Equals("Weapon", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(i => i.Attack).FirstOrDefault();

        var armor = all.Where(i => i.Type.Equals("Armor", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(i => i.Defense).FirstOrDefault();

        var potion = all.FirstOrDefault(i => i.Type.Equals("Potion", StringComparison.OrdinalIgnoreCase));

        foreach (var pick in new[] { weapon, armor, potion })
        {
            if (pick != null)
            {
                var tracked = _context.Items.Find(pick.Id);
                if (tracked != null && !concrete.Inventory.Items.Contains(tracked))
                    concrete.Inventory.Items.Add(tracked);
            }
        }

        _context.SaveChanges();
    }

}
