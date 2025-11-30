using ConsoleRpgEntities.Models.Abilities.PlayerAbilities;
using ConsoleRpgEntities.Models.Attributes;
using ConsoleRpgEntities.Models.Equipments;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConsoleRpgEntities.Models.Characters
{
    public class Player : ITargetable, IPlayer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Experience { get; set; }
        public int Health { get; set; }

        // Foreign key
        public int? EquipmentId { get; set; }

        // Navigation properties
        public virtual Inventory Inventory { get; set; } = new Inventory { Items = new List<Item>() };
        public virtual Equipment Equipment { get; set; } = new Equipment();
        public virtual ICollection<Ability> Abilities { get; set; } = new List<Ability>();

        [NotMapped]
        public List<Item> InventoryItems
        {
            get
            {
                EnsureInventoryReady();
                if (Inventory.Items is not List<Item> list)
                {
                    list = Inventory.Items.ToList();
                    Inventory.Items = list;
                }
                return list;
            }
        }

        public void Attack(ITargetable target)
        {
            // Player-specific attack logic
            Console.WriteLine($"{Name} attacks {target.Name} with a {Equipment.Weapon.Name} dealing {Equipment.Weapon.Attack} damage!");
            target.Health -= Equipment.Weapon.Attack;
            System.Console.WriteLine($"{target.Name} has {target.Health} health remaining.");

        }

        public void UseAbility(IAbility ability, ITargetable target)
        {
            if (Abilities.Contains(ability))
            {
                ability.Activate(this, target);
            }
            else
            {
                Console.WriteLine($"{Name} does not have the ability {ability.Name}!");
            }
        }

        private void EnsureInventoryReady()
        {
            if (Inventory == null)
                Inventory = new Inventory { Items = new List<Item>() };
            if (Inventory.Items == null)
                Inventory.Items = new List<Item>();
            Equipment ??= new Equipment();
        }

        public void AddItem(Item item)
        {
            EnsureInventoryReady();
            if (item is null) return;
            Inventory.Items.Add(item);
        }

        public bool RemoveItem(Item item)
        {
            EnsureInventoryReady();
            if (item is null) return false;
            return Inventory.Items.Remove(item);
        }

        public (bool success, string message) EquipItem(Item item)
        {
            EnsureInventoryReady();
            if (item is null) return (false, "Item is null.");
            if (!Inventory.Items.Contains(item)) return (false, "Item is not in your inventory.");

            if (string.Equals(item.Type, "Weapon", StringComparison.OrdinalIgnoreCase))
            {
                if (item.Attack <= 0) return (false, "This weapon has no attack power.");
                Equipment ??= new Equipment();
                Equipment.Weapon = item;
                return (true, $"Equipped weapon: {item.Name} (+{item.Attack} ATK).");
            }

            if (string.Equals(item.Type, "Armor", StringComparison.OrdinalIgnoreCase))
            {
                if (item.Defense <= 0) return (false, "This armor has no defense.");
                Equipment ??= new Equipment();
                Equipment.Armor = item;
                return (true, $"Equipped armor: {item.Name} (+{item.Defense} DEF).");
            }

            return (false, "Only weapons or armor can be equipped.");
        }

        public (bool success, string message) UseItem(Item item)
        {
            EnsureInventoryReady();
            if (item is null) return (false, "Item is null.");
            if (!Inventory.Items.Contains(item)) return (false, "Item is not in your inventory.");

            if (string.Equals(item.Type, "Potion", StringComparison.OrdinalIgnoreCase))
            {
                var healed = Math.Max(1, item.Value / 10);
                Health += healed;
                Inventory.Items.Remove(item);
                return (true, $"Used potion '{item.Name}'. Restored {healed} HP.");
            }

            return (false, "Only consumables (e.g., Potion) can be used.");
        }
    }
}
