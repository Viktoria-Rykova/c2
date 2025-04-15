using System;
using System.Collections.Generic;
using System.Linq;

namespace компилятор_2
{
    internal class LexicalAnalyzer
    {
        public List<(string Problem, int Position)> AnalyzeText(string input)
        {
            var results = new List<(string, int)>();
            int i = 0;

            // Массив недопустимых символов
            char[] invalidChars = { '@', '№', '#', '$', '%', '^', '&', '*', '!', '"', '<', '>', '/', '\\', '|', '.', '?', '~',
                             'А', 'Б', 'В', 'Г', 'Д', 'Е', 'Ё', 'Ж', 'З', 'И', 'Й', 'К', 'Л', 'М', 'Н', 'О', 'П', 'Р', 'С',
                             'Т', 'У', 'Ф', 'Х', 'Ц', 'Ч', 'Ш', 'Щ', 'Ы', 'Э', 'Ю', 'Я', 'а', 'б', 'в', 'г', 'д', 'е', 'ё',
                             'ж', 'з', 'и', 'й', 'к', 'л', 'м', 'н', 'о', 'п', 'р', 'с', 'т', 'у', 'ф', 'х', 'ц', 'ч', 'ш',
                             'щ', 'ы', 'э', 'ю', 'я' };

            // Символы, которые допустимы в словарях (включая одинарную кавычку для строк)
            char[] allowedSymbols = { '{', '}', ':', ',', ';', '\'', '"', '=' };

            while (i < input.Length && char.IsWhiteSpace(input[i])) i++; // Пропускаем пробелы в начале

            // Основной цикл по символам
            while (i < input.Length)
            {
                char c = input[i];

                // Проверка на недопустимые символы
                if (Array.Exists(invalidChars, element => element == c))
                {
                    // Записываем ошибку с позициями и завершаем проверку
                    results.Add(("Ошибка (недопустимый символ '" + c + "')", i + 1));
                    return results; // Завершаем проверку после первой ошибки
                }

                // Если символ является допустимым для синтаксиса словаря (пропускаем их)
                if (Array.Exists(allowedSymbols, element => element == c))
                {
                    i++; // Пропускаем символ
                    continue;
                }

                // Если символ является буквой, цифрой или '_', то продолжаем разбор
                if (char.IsLetterOrDigit(c) || c == '_')
                {
                    i++;
                    continue; // Идем дальше, так как это допустимый символ
                }

                // Если символ является пробелом, пропускаем его
                if (char.IsWhiteSpace(c))
                {
                    i++;
                    continue;
                }

                // Недопустимый символ, который не был найден в массиве invalidChars и allowedSymbols
                results.Add(("Ошибка (недопустимый символ '" + c + "')", i + 1));
                return results; // Завершаем проверку после первой ошибки
            }

            return results;
        }
    }
}
