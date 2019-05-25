using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eventos_Discretos
{


    public class KojosKitchen
    {
        private enum timeZone { hard, light, closed };
        private enum Hours { ten_AM = 0, eleven_half_AM = 90, one_half_PM = 210, five_PM = 420, seven_PM = 540, nine_PM = 660 }

        private double t;
        public int fiveMinutes;
        private Queue<Consumer> queue;
        public double light_lambda;
        public double hard_lambda;

        private double lambda
        {
            get {
                if (t < (int)Hours.eleven_half_AM) return light_lambda;//90=11:30
                if (t < (int)Hours.one_half_PM) return hard_lambda;//210=1:30
                if (t < (int)Hours.five_PM) return light_lambda;//420=5:00
                if (t < (int)Hours.seven_PM) return hard_lambda;//540=7:00
                if (t < (int)Hours.nine_PM) return light_lambda;//660=9:00

                return light_lambda;
            }
        }
        public int total_clients;
        public int clients_in_system;
        SortedList<double, List<Consumer>> system;
        int all_day_cookers;
        int hard_hours_cookers;

        public int five_minutes_sandwich;
        public int five_minutes_sushi;

        private bool debug;

        public int cookers_number
        {
            get
            {
                if (t < (int)Hours.eleven_half_AM)
                    return all_day_cookers;
                if (t < (int)Hours.one_half_PM)
                    return all_day_cookers + hard_hours_cookers;
                if (t < (int)Hours.five_PM)
                    return all_day_cookers;
                if (t < (int)Hours.seven_PM)
                    return all_day_cookers + hard_hours_cookers;

                return all_day_cookers;
            }
        }


        public KojosKitchen(double _light_lambda, double _hard_lambda,bool _debug=true)
        {
            light_lambda = _light_lambda;
            hard_lambda = _hard_lambda;
            debug = _debug;
            Init();
        }


        private timeZone dayTime(double t)
        {
            if (t < (int)Hours.eleven_half_AM) return timeZone.light;//90=11:30
            if (t < (int)Hours.one_half_PM) return timeZone.hard;//210=1:30
            if (t < (int)Hours.five_PM) return timeZone.light;//420=5:00
            if (t < (int)Hours.seven_PM) return timeZone.hard;//540=7:00
            if (t < (int)Hours.nine_PM) return timeZone.light;//660=9:00

            return timeZone.closed;
        }

        static double GetExponential(double lambda)
        {
            Random r = new Random();
            return -(lambda) * Math.Log(r.NextDouble());
        }

        static double GetUniform()
        {
            Random r = new Random();
            return r.NextDouble();
        }

        static double GetUniform(double min, double max)
        {
            Random r = new Random();
            return min + (max - min) * r.NextDouble();
        }

        public void Simulation(int _allDayCookers = 2, int _hardHourCookers = 1)
        {
            all_day_cookers = _allDayCookers;
            hard_hours_cookers = _hardHourCookers;
            Init();

            while (t < (int)Hours.nine_PM || system.Count > 0)
            {
                System.Threading.Thread.Sleep(1);
                int nextArrive = 0;
                while (nextArrive == 0)
                    nextArrive = (int)GetExponential(lambda);

                t += nextArrive;

                //Verificar si al llegar al tiempo t se vaciaron algunos depedientes
                //De existir algun cliente que termino incrementar los contadores correspondientes
                while (system.Count >= 1 && system.First().Value[0].exit <= t)
                {
                    if (debug)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Client exits: {0}", system.First().Value[0].exit);
                    }

                    if (system.First().Value[0].exit - system.First().Value[0].arrive >= 5)
                    {
                        fiveMinutes++;
                        try {
                            var x = (SandwichConsumer)system.First().Value[0];
                            five_minutes_sandwich++;
                        }
                        catch
                        {
                            five_minutes_sushi++;
                        }                        
                    }

                    total_clients++;
                    if (system.First().Value.Count <= 1)
                        system.RemoveAt(0);
                    else
                        system.First().Value.RemoveAt(0);

                    clients_in_system--;
                }

                //Si hay dependientes libres poner los que estan en cola a que sean atendidos
                while (queue.Count > 0 && clients_in_system < cookers_number)
                {
                    Consumer current = queue.Dequeue();
                    double x = current.GetExit();
                    current.exit = t + x;

                    if (system.ContainsKey(t + x))
                        system[t + x].Add(current);
                    else
                    {
                        var aux = new List<Consumer>();
                        aux.Add(current);
                        system.Add(t + x, aux);
                    }
                    clients_in_system++;
                    if (debug)
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine("Client left the queue");
                    }
                }


                if (t > (int)Hours.nine_PM)
                    continue;

                double r = GetUniform();
                Consumer c = null;
                if (r > 0.5)
                    c = new SushiConsumer();
                else
                    c = new SandwichConsumer();

                c.arrive = t;
                if (debug)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Client arrives: {0}", c.arrive);
                }

                if (system.Count < cookers_number)
                {//Si quedan dependientes disponibles poner el cliente actual en uno de ellos
                    
                    double x = c.GetExit();
                    c.exit = t + x;

                    if (system.ContainsKey(t + x))
                        system[t + x].Add(c);
                    else
                    {
                        var aux = new List<Consumer>();
                        aux.Add(c);
                        system.Add(t + x, aux);
                        clients_in_system++;
                    }
                }
                else
                {
                    queue.Enqueue(c);
                    if (debug)
                    {
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine("Client enqueue");
                    }
                }
            }
        }

        private void Init()
        {
            t = 0;
            fiveMinutes = 0;
            five_minutes_sandwich = 0;
            five_minutes_sushi = 0;
            queue = new Queue<Consumer>();
            total_clients = 0;
            clients_in_system = 0;
            system = new SortedList<double, List<Consumer>>();
        }
        
    }

    public class Consumer
    {
        public enum Type { sandwich, sushi };
        public double arrive;
        public double exit;
        protected int min, max;

        public Type caracteristic;
        public double GetExit()
        {
            Random r = new Random();
            return r.Next(min, max);
        }
    }

    public class SandwichConsumer : Consumer
    {
        public SandwichConsumer()
        {
            min = 3;
            max = 5;
        }

        public new Type caracteristic
        {
            get
            {
                return Type.sandwich;
            }
        }
        
    }

    public class SushiConsumer : Consumer
    {
        public SushiConsumer()
        {
            min = 5;
            max = 8;
        }

        public new Type caracteristic
        {
            get
            {
                return Type.sushi;
            }
        }

    }
}
