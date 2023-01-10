namespace threads._01._2023;

static class Program
{
    public static async Task Main()
    {
        // 1е задание
        Console.WriteLine("Задание 1");
        Task1_CreateChildThread.Execute();
        Console.WriteLine("\nЗадание 1 - завершено. Ожидается ввод для продолжения\n");
        Console.ReadLine();
        Console.Clear();
        
        // 8e задание
        Console.WriteLine("Задание 8\n");
        await Task8_PiParallelCalc.Execute();
        Console.WriteLine("Задание 8 - завершено.");

    }
    
}

