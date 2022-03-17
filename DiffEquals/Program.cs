using System;
using System.IO;

namespace DiffEquals
{
    class Program
    {
        static void Main(string[] args)
        {
            DiffEqual diffEqual = new DiffEqual("input.txt", (double x, double y) => { return 5 + 5 * x - y; }, "output.txt");
        }
    }
}
