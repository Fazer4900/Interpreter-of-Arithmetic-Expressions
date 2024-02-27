using System.Data;

namespace ArithmeticInterpretter
{
    internal class Program
    {

       

        static void Main(string[] args)
        {
            int numberOfExpresions = -10;
            Console.WriteLine("kolik budete zadavat expresions ?");
            if(!int.TryParse(Console.ReadLine(), out numberOfExpresions) || numberOfExpresions < 0)
            {
                throw new Exception();
            }


            ExpressionEvaluator evaluator = new ExpressionEvaluator();

         

            for(int i = 0; i < numberOfExpresions;i++)
            {
                string expression = Console.ReadLine();
                if(expression == string.Empty)
                {
                    Console.WriteLine("Error empty"); 
                    continue;
                }

                Console.WriteLine(evaluator.evaluate(expression));
            }


            Console.ReadKey();
  
        }
    }
}
