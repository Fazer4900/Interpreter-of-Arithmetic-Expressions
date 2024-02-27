using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

/*
 
 pseudo code from https://www.geeksforgeeks.org/expression-evaluation/ used to write the Evaluator
 1. While there are still tokens to be read in,
   1.1 Get the next token.
   1.2 If the token is:
       1.2.1 A number: push it onto the value stack.
       1.2.2 A variable: get its value, and push onto the value stack.
       1.2.3 A left parenthesis: push it onto the operator stack.
       1.2.4 A right parenthesis:
         1 While the thing on top of the operator stack is not a 
           left parenthesis,
             1 Pop the operator from the operator stack.
             2 Pop the value stack twice, getting two operands.
             3 Apply the operator to the operands, in the correct order.
             4 Push the result onto the value stack.
         2 Pop the left parenthesis from the operator stack, and discard it.
       1.2.5 An operator (call it thisOp):
         1 While the operator stack is not empty, and the top thing on the
           operator stack has the same or greater precedence as thisOp,
           1 Pop the operator from the operator stack.
           2 Pop the value stack twice, getting two operands.
           3 Apply the operator to the operands, in the correct order.
           4 Push the result onto the value stack.
         2 Push thisOp onto the operator stack.
2. While the operator stack is not empty,
    1 Pop the operator from the operator stack.
    2 Pop the value stack twice, getting two operands.
    3 Apply the operator to the operands, in the correct order.
    4 Push the result onto the value stack.
3. At this point the operator stack should be empty, and the value
   stack should have only one value in it, which is the final result.
 
 */


public enum ArithmeticOperation
{    
    Add,
    Subtract,
    Multiply,
    Divide,
    OpenBracket,
    ClosedBracket    
}

namespace ArithmeticInterpretter
{
    internal class ExpressionEvaluator
    {
        Stack<int> values = new Stack<int>();
        Stack<ArithmeticOperation> operations = new Stack<ArithmeticOperation>();       
        private static int getPriority(ArithmeticOperation operation)
        {
            if(operation == ArithmeticOperation.Multiply || operation == ArithmeticOperation.Divide)
            {
                return 2;
            }
            if(operation == ArithmeticOperation.Add || operation == ArithmeticOperation.Subtract)
            {
                return 1;
            }
            return 0;
        }


        public string evaluate(string expression)
        {

            values.Clear();
            operations.Clear();

            if(!basicCharactersCheck(expression))
            {
                return "Error";
            }

            string formatedExpression = "";

            if(!removeSpaces(expression,out formatedExpression)) 
            {
                return "Error";
            }

            //Console.WriteLine("po uprave je " + formatedExpression);

            int expressionIndex = 0;
            while(expressionIndex < expression.Length)  
            {
                char character = expression[expressionIndex];

                if (char.IsNumber(character))  // pokud je to cislo ctu dokud nachazim cisla a ptidam je do ciselneho stacku
                {
                    string wholeNumber = "";
                    wholeNumber += character;

                    while (expressionIndex < expression.Length - 1 && char.IsNumber(expression[expressionIndex +1]))
                    {
                        expressionIndex++;
                        character = expression[expressionIndex];
                        wholeNumber += character;
                    }

                    int number = 0;
                    if (int.TryParse(wholeNumber, out number))
                    {
                        //Console.WriteLine("pridavam cislo " + number);
                        values.Push(number);
                    }
                    else
                    {
                        return "Error";
                        //throw new Exception("failed when loading number in Expression");
                    }

                    expressionIndex++;
                    continue;
                }


                if(character == '(') { operations.Push(ArithmeticOperation.OpenBracket);} // V připadě otevřené závorky pushni do operand stacku

                                                      
                if (character == ')')
                {
                                                  
                    while(operations.Peek() != ArithmeticOperation.OpenBracket) // damn to je celkem smart
                    {
                        // vyreseni operace v zavorkach
                                                       
                        int number1 = 0;
                        int number2 = 0;
                        ArithmeticOperation operation;
                            
                        if(!values.TryPop(out number1)) { return "Error";} // nacteme cisla ve stacku
                        if(!values.TryPop(out number2)) { return "Error";}
                        if(!operations.TryPop(out operation)) { return "Error";}

                        int result = aplyOperation(number1,number2 ,operation);
              
                        values.Push(result); // spočtu a pushnu back výsledek s kterým opětovně počítám dokud se nedostanu na začátek závorky
                    }

                    operations.Pop();          
                }

                if(character == '*' || character == '/' || character == '+' || character == '-') // pokud operator 
                {
                    ArithmeticOperation operation;
                    switch (character)
                    {
                        case '*':
                            operation = ArithmeticOperation.Multiply; break;
                        case '/':
                            operation = ArithmeticOperation.Divide; break;
                        case '+':
                            operation = ArithmeticOperation.Add; break;
                        case '-':
                            operation = ArithmeticOperation.Subtract; break;
                                
                        default: throw new Exception("nonvalid arithmeticOperation");
                    }

                    while(operations.Count() > 0 && getPriority(operation) <= getPriority(operations.Peek()))                         
                    {
                        int result = 0;
                        int number1 = 0;
                        int number2 = 0;

                        if (!values.TryPop(out number1)) { return "Error "; } // nacteme cisla ve stacku
                        if (!values.TryPop(out number2)) { return "Error"; }
                        if (!operations.TryPop(out operation)) { return "Error"; }

                        result = aplyOperation(number2,number1,operation);
                        values.Push(result);
                    }

                    operations.Push(operation);

                }                
                expressionIndex++;
            }

            while(operations.Count > 0)
            {
                int number1 = 0;
                int number2 = 0;
                ArithmeticOperation operation;



                if (!values.TryPop(out number1)) { return "Error"; } // nacteme cisla ve stacku
                if (!values.TryPop(out number2)) { return "Error"; }
                if (!operations.TryPop(out operation)) { return "Error"; }

                int result = aplyOperation(number2, number1, operation);
                values.Push(result);

            }

            if(values.Count != 1) {

                return "error";
            }
     
            int finalResult = values.Pop();
            return finalResult.ToString();

                
        }

        private bool basicCharactersCheck(string expression) 
        {
            foreach(char character in expression)
            {
                if(!char.IsNumber(character) && character != '*' && character != '/' && character != '+' && character != '-' && character != '(' && character != ')' && character  != ' ')
                {
                    return false;
                }
                
            }
            return true;        
        }

        private bool removeSpaces(string expression , out string formatedString)
        {
            formatedString = "";
            foreach (char character in expression)
            {
                if(character != ' ') 
                { 
                    formatedString += character;
                }
            }

            if(formatedString.Length == 0) { return false; }
            return true;
        }

        int aplyOperation(int number1, int number2, ArithmeticOperation operation)
        {

            //Console.WriteLine("aplikuji operaci " + number1 + " " + operation + " " + number2  );

            switch (operation)
            {
                case ArithmeticOperation.Add:
                    return number1 + number2;
                case ArithmeticOperation.Subtract:
                    return number1 - number2;
                case ArithmeticOperation.Multiply:
                    return number1 * number2;
                case ArithmeticOperation.Divide:
                    return number1 / number2;
            }
            return 0;
        }


    }
}
