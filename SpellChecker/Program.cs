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
      private const String dictionaryFile = "dictionary.txt";

      private static bool quietMode = false;
      private static bool disableTrim = false;

      private static TextWriter defaultConsoleOut;

      private static void Main(string[] args)
      {
         ReadCommandLine(args);

         if (quietMode)
         {
            defaultConsoleOut = Console.Out;
            Console.SetOut(TextWriter.Null);
         }

         dictionary = new Corrector(2);

         if (!ReadDictionatyFromFile(dictionaryFile))
         {
            Console.WriteLine("Добро пожаловать в программу Spell Checker!\n" +
                              "Запустите программу с ключом -q, чтобы включить тихий режим\n" +
                              "Ключ -u отключит сжатие словаря (ускорит загрузку и поиск, но увеличит портребление памяти)\n" +
                              $"Можно загрузить словарь из файла, разместив {dictionaryFile} в папке программы\n" +
                              "Словарь должен быть в UTF-8. Слова разделяются пробелами или переносами строк\n\n" +
                              "Либо вводите слова через пробел или с новой строки.\n" +
                              $"Введите {finishingString} отдельной строкой для завершения словаря");

            int previousWordsCount = dictionary.WordsCount;
            while (ReadLineToDictionary(finishingString))
            {
               Console.WriteLine($"Добавлено {dictionary.WordsCount - previousWordsCount} слов\n" +
                                 $"Всего в словаре {dictionary.WordsCount} слов");
               previousWordsCount = dictionary.WordsCount;
            }
         }

         if (!disableTrim) { 
            Console.WriteLine("Словарь сжимается...");
            dictionary.TrimExcess();
         }

         Console.WriteLine($"Всего в словаре {dictionary.WordsCount} слов\n" +
                           "Вводите текст для проверки.\n" +
                           $"Ведите {finishingString} отдельной строкой для выхода из программы");

         if (quietMode)
         {
            Console.SetOut(defaultConsoleOut);
         }
         
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

         Console.WriteLine("Cловарь загружается...");

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
         return wordRegex.Replace(input, m => { return dictionary.GetCorrectedWord(m.Value); });
      }

      static string CorrectWord(Match m)
      {
         return dictionary.GetCorrectedWord(m.Value);
      }

      static void ReadCommandLine(string[] args)
      {
         if (args == null || args.Length < 1 || args.Length > 2)
         {
            return;
         }

         if (args.Contains("-q", StringComparer.OrdinalIgnoreCase))
         {
            quietMode = true;
         }

         if (args.Contains("-u", StringComparer.OrdinalIgnoreCase))
         {
            disableTrim = true;
         }

      }
   }
}
