using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Pr7
{
    class User
    {
        public static Users RegisterLogin(string username, string password, bool login)
        {
            if (!login)
            {
                Core.Context.Users.Add(new Users { username = username, passwd = password });
                Core.Context.SaveChanges();
            }
            return (from user in Core.Context.Users where user.username == username select user).First();
        }
    }
    internal class Program
    {
        static Users RegisterLoginInt(bool login)
        {
            Console.WriteLine("Введите имя пользователя: ");
            var username = Console.ReadLine();
            Console.WriteLine("Введите пароль: ");
            var password = Console.ReadLine();
            if (!login)
            {
                Console.WriteLine("Введите пароль опять: ");
                if (password != Console.ReadLine())
                {
                    Console.WriteLine("Пароли не совпадают.");
                    return RegisterLoginInt(login);
                }
            }
            Users u;
            try
            {
                u = User.RegisterLogin(username, password, login);
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine(login ? "Такого пользователя не существует" : "Такой пользователь уже существует");
                return RegisterLoginInt(login);
            }
            if (login && u.passwd != password)
            {
                Console.WriteLine("Пароль не подходит.");
                return RegisterLoginInt(login);
            }
            return u;
        }
        static int? LoggedInAs = null;
        static int AskInt()
        {
            try
            {
                return Convert.ToInt32(Console.ReadLine());
            }
            catch
            {
                return AskInt();
            }
        }
        static void PrintHistory(bool asc)
        {
            var b = (
                from x in Core.Context.Orders
                join y in Core.Context.OrdersTowarys on x.id equals y.order_id
                join z in Core.Context.Pvz on x.pvz equals z.id
                join a in Core.Context.Towary on y.towar_id equals a.id
                where x.user_id == LoggedInAs
                select new { x.id, z.location, y.price_fact, a.name }
            );
            Console.WriteLine("Заказ\tПВЗ\tЦена товара\tНазвание товар");
            Console.WriteLine(string.Join("\n",
                (asc ? b.OrderBy(x => x.id) : b.OrderByDescending(x => x.id))
                .ToList().Select(x => $"{x.id}\t{x.location}\t{x.price_fact}\t{x.name}")
            ));
        }
        static void PrintItems()
        {
            Console.WriteLine("ID\tЦена\tНазвание");
            Console.WriteLine(string.Join("\n", Core.Context.Towary.ToList().Select(x => $"{x.id}\t{x.price}\t{x.name}")));
        }
        static List<int> cart = new List<int>();
        static void PrintCart(List<int> list)
        {
            Console.WriteLine("Цена\tНазвание");
            Console.WriteLine(string.Join("\n",
                list.Select(y => (from x in Core.Context.Towary where x.id == y select x).First()).ToList().Select(x => $"{x.price}\t{x.name}")
            ));
        }
        static bool CheckItemExistance(int id)
        {
            return (from x in Core.Context.Towary where x.id == id select 1).Count() != 0;
        }
        static void CheckoutCart(ref List<int> items, int pvz)
        {
            var o = Core.Context.Orders.Add(new Orders { user_id = LoggedInAs.Value, datetime = DateTime.Now, pvz = pvz });
            foreach (var item in items)
                Core.Context.OrdersTowarys.Add(new OrdersTowarys
                {
                    order_id = o.id,
                    towar_id = item,
                    price_fact = (from x in Core.Context.Towary where x.id == item select x.price).First()
                });
            Core.Context.SaveChanges();
            Console.WriteLine("Заказ оформлен.");
            items.Clear();
        }

        static void Main(string[] args)
        {
            
        }
    }
}
