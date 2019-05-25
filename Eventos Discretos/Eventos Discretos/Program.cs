using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eventos_Discretos;

namespace Eventos_Discretos
{
    class Program
    {
        static KojosKitchen kojo;
        static void Main(string[] args)
        {
            kojo = new KojosKitchen(3, 2, false);//Aproximadamente 30 personas por hora en horario normal y 50 en horario pico

            Print(5, 1, 2, 0);
            Print(5, 2, 2, 1);
            Print(5, 3, 3, 2);
            Print(5, 4, 4, 3);
            Print(5, 5, 5, 4);
        }

        public static void Print(int days, int _case, int employers, int plus)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Caso {0}: {1} empleados, +{2} en horas pico", _case, employers, plus);
            Console.ForegroundColor = ConsoleColor.Gray;

            for (int i = 0; i < days; i++)
            {
                kojo.Simulation(employers, plus);
                Console.WriteLine(kojo.fiveMinutes * 100 / kojo.total_clients + "%");
                Console.WriteLine("Total: {0}, Inconformes: {1}", kojo.total_clients, kojo.fiveMinutes);
                Console.WriteLine("Inconformes Sandwich: {0}, Inconfirmes Sushi: {1}", kojo.five_minutes_sandwich, kojo.five_minutes_sushi);
                Console.WriteLine();
            }
        }
    }
}
