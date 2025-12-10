using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Pr7
{
    class Warehouse
    {
        public static string PrintAll()
        {
            return "\tСклад\nНумер\tНазвание\tЦена\tКол-во\tДо доставки\n" +
            string.Join("\n", Core.Context.Part.ToList().Select(x => $"{x.id}\t{x.name}\t{x.price}\t{x.count}")) + "\n" +
            string.Join("\n", (
                from x in Core.Context.PartPending
                join part in Core.Context.Part
                on x.id_part equals part.id
                where x.time_remains != 0
                select new { part, x }
            ).ToList().Select(xx => $"{xx.part.id}\t{xx.part.name}\t{xx.part.price}\t{xx.x.count}\t{xx.x.time_remains}"));
        }
        public static Part RandomPart()
        {
            var id = new Random().Next(0, Core.Context.Part.Count()) + 1;
            return (from x in Core.Context.Part where x.id == id select x).First();
        }
        public static bool OrderPart(int id, int cc)
        {
            var price = (from x in Core.Context.Part where x.id == id select x.price).First();
            if (price > Core.Context.Account.First().balance)
                return false;
            Core.Context.PartPending.Add(new PartPending { id = 0, id_part = id, count = cc, time_remains = 2 });
            Core.Context.Account.First().balance -= price * cc;
            Core.Context.SaveChanges();
            return true;
        }
        public static void PendingOrdersStep()
        {
            (from x in Core.Context.PartPending where x.time_remains == 1 select x).ToList().ForEach(x => (
                from y in Core.Context.Part where y.id == x.id_part select y
            ).First().count += x.count);
            (from x in Core.Context.PartPending where x.time_remains != 0 select x).ToList().ForEach(x => x.time_remains--);
            Core.Context.SaveChanges();
        }
    };
    internal class Program
    {
        static bool OrderInt()
        {
            Console.WriteLine("Введите номер части для заказа (или 0):");
            int ch = Convert.ToInt32(Console.ReadLine());
            if (ch == 0)
                return false;
            Console.WriteLine("Введите количество:");
            int cc = Convert.ToInt32(Console.ReadLine());
            if (Warehouse.OrderPart(ch, cc))
                Console.WriteLine("Деталь заказана.");
            else
                Console.WriteLine("Деталь НЕ заказана.");
            return true;
        }
        static void Main(string[] args)
        {
            while (true)
            {
                if (Core.Context.Account.First().balance < 0)
                {
                    Console.WriteLine("Вы обанкротились.");
                    return;
                }
                Warehouse.PendingOrdersStep();
                Console.WriteLine($"На счету: {Core.Context.Account.First().balance}");
                Console.WriteLine(Warehouse.PrintAll());
                bool canceled = false;
                while (!canceled)
                    canceled = !OrderInt();
                var part = Warehouse.RandomPart();
                Console.WriteLine($"К вам приехал клиент, у которого отвалился: {part.name} (№{part.id})");
                var tbp = part.price + 200;
                Console.WriteLine($"К оплате: {tbp}");
                Console.WriteLine("Принять? y/n");
                if (Console.ReadLine() == "n")
                {
                    Console.WriteLine("Zа отказ штраф: 100 денег");
                    Core.Context.Account.First().balance -= 100;
                    Core.Context.SaveChanges();
                    continue;
                }
                if (part.count == 0)
                {
                    Console.WriteLine("Внимание! Запчасти у вас нету, вы поставили случайную!");
                    var part2 = Warehouse.RandomPart();
                    int i = 0;
                    for (; (part2.count == 0 || part2.id == part.id) && i < 50; i++)
                        part2 = Warehouse.RandomPart();
                    if (i == 50)
                    {
                        Console.WriteLine("Деталей не осталось. Ваша коротка жизнь подошла к концу.");
                        return;
                    }
                    part2.count--;
                    var lost = part.price * 2;
                    Console.WriteLine($"Клиент вернулся недовольный и вытянул из вас {lost} денег.");
                    Core.Context.Account.First().balance -= lost;
                    Core.Context.SaveChanges();
                    continue;
                }
                part.count--;
                Core.Context.Account.First().balance += tbp;
                Core.Context.SaveChanges();
                Console.WriteLine("Всё идёт по плану.");
            }
        }
    }
}
