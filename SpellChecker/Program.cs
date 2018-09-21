using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using SpellChecker;
using System.Runtime;

namespace SpellChecker
{
   class Program
   {
      private static DictionaryTree dictionary;
      private const String finishingString = "===";

      private static void Main(string[] args)
      {
         dictionary = new DictionaryTree(2);

         Console.WriteLine("Добро пожаловать в программу Spell Checker!/n" +
                           "Вы можете загрузить словарь с помощью ключа -d путь_к_словарю");
         Console.WriteLine("Либо вводите слова через пробел или с новой строки." +
                           $"Введите {finishingString} для завершения словаря");


/*         int previousWordsCount = dictionary.WordsCount;
         while (ReadLineToDictionary(finishingString))
         {
            Console.WriteLine($"Добавлено {dictionary.WordsCount - previousWordsCount} слов");
            Console.WriteLine($"Всего в словаре {dictionary.WordsCount} слов");
            previousWordsCount = dictionary.WordsCount;
         }*/
         ReadDictionatyFromFile(@"C:/dict.txt");

         Console.WriteLine($"Всего в словаре {dictionary.WordsCount} слов");
         Console.WriteLine($"Введите текст для проверки. Для завершения, введите {finishingString}");

         while (true)
         {
            String correctedLine = ReadLineToCorrection(finishingString);
            if (correctedLine == null)
            {
               break;
            }
            Console.WriteLine(correctedLine);
         }
      }

      static void ReadDictionatyFromFile(String filePath)
      {
         StreamReader sr = new StreamReader(filePath);
         while (true)
         {
            String line = sr.ReadLine();
            if (line == null)
            {
               break;
            }

            AddLineToDictionary(line);
         }
      }

      // return false if user enters endLine as separate line
      static bool ReadLineToDictionary(String endLine)
      {
         String input = Console.ReadLine();
         if (input == endLine)
         {
            return false;
         }

         AddLineToDictionary(input);

         return true;
      }

      static void AddLineToDictionary(String inputLine)
      {
         String[] splitted = inputLine.Split(' ');

         for (int i = 0; i < splitted.Length; ++i)
         {
            if (String.IsNullOrWhiteSpace(splitted[i]))
            {
               continue;
            }

            dictionary.AddWord(splitted[i].Trim());
         }
      }

      // return null if user enters endLine as separate line
      static String ReadLineToCorrection(String endLine)
      {
         String input = Console.ReadLine();
         if (input == endLine)
         {
            return null;
         }

         Regex wordRegex = new Regex(@"\w+");
         return wordRegex.Replace(input, CorrectWord);
      }

      static string CorrectWord(Match m)
      {
         return dictionary.GetCorrectedWord(m.Value);
      }
   }
}
