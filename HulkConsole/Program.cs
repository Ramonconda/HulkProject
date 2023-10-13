using Hulk;
namespace HulkConsole
{
     class Program
    {
        static void Main()
        {
            Compiler compiler = new(Console.WriteLine);
            while (true)
            {
                Console.Write(">");
                compiler.Compile(Console.ReadLine());
                
            }
        }
    }
}