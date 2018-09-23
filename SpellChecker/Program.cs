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
      private static Corrector dictionary;
      private const String finishingString = "===";
      private const String dictionaryFile = @"dictionary.txt";
      private const bool silentMode = false;

      private static void Main(string[] args)
      {
         dictionary = new Corrector(2);

         var defaultConsoleOut = Console.Out;

         if (silentMode)
         {
            Console.SetOut(TextWriter.Null);
         }

         if (ReadDictionatyFromFile(@"dictionary.txt") && !silentMode)
         {
            Console.WriteLine("Словарь загружен!");
         }
         else
         {
            Console.WriteLine("Добро пожаловать в программу Spell Checker!\n" +
                              $"Можно загрузить словарь из файла, разместив {dictionaryFile} в папке программы\n" +
                              "Словарь должен быть в UTF-8. Слова разделяются пробелами или переносами строк\n\n" +
                              "Либо вводите слова через пробел или с новой строки.\n" +
                              $"Введите {finishingString} отдельной строкой для завершения словаря");

            int previousWordsCount = dictionary.WordsCount;

            while (ReadLineToDictionary(finishingString))
            {
               Console.WriteLine($"Добавлено {dictionary.WordsCount - previousWordsCount} слов" +
                                 $"Всего в словаре {dictionary.WordsCount} слов");

               previousWordsCount = dictionary.WordsCount;
            }
         }

         Console.WriteLine($"Всего в словаре {dictionary.WordsCount} слов\n" +
                           "Вводите текст для проверки.\n" +
                           $"Для завершения, введите {finishingString} отдельной строкой");

         Console.SetOut(defaultConsoleOut);

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

      static bool ReadDictionatyFromFile(String filePath)
      {
         StreamReader sr;
         try
         {
            sr = new StreamReader(filePath);
         }
         catch (Exception)
         {
            return false;
         }

         Console.WriteLine("Загружается словарь...");

         while (true)
         {
            String line = sr.ReadLine();
            if (line == null)
            {
               break;
            }
            AddLineToDictionary(line);
         }

         return true;
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
