using System;
using System.Configuration;
using System.Threading;
using EasyNetQ;

namespace TestConsole
{
    public class Program
    {
        static void Main(string[] args)
        {
            var calculationsCount = int.Parse(args[0]);

            var apiUrl = ConfigurationManager.AppSettings["apiUrl"];
            var rabbitMqConnectionString = ConfigurationManager.AppSettings["rabbitMqConnectionString"];

            Console.WriteLine("threadId " + Thread.CurrentThread.ManagedThreadId + "initializes calculations");

            Console.WriteLine("Press enter to stop calculations");

            using (var bus = RabbitHutch.CreateBus(rabbitMqConnectionString).Advanced)
            {
                for (var i = 0; i < calculationsCount; i++)
                {
                    var calculator = new FibonacciDistributedCalculator(apiUrl, bus);
                    calculator.StartCalculation();
                }

                Console.ReadLine();
            }
        }
    }
}
