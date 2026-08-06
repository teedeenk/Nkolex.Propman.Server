namespace Nkolex.Propman.Server.Startup
{
    public static class ConsoleBanner
    {
        private static readonly string[] Circle =
        {
            "      .=+*##*+=.      ",
            "    :+%@@@@@@@@@%+:   ",
            "   =%@@@@@@@@@@@@@%=  ",
            "  #@@@@@ █ █ █ @@@@@# ",
            "  #@@@@@ ██ ██ @@@@@# ",
            "  #@@@@@ █ █ █ @@@@@# ",
            "  #@@@@@ █   █ @@@@@# ",
            "  #@@@@@ █   █ @@@@@# ",
            "   =%@@@@@@@@@@@@@%=  ",
            "    :+%@@@@@@@@@%+:   ",
            "      .=+*##*+=.      "
        };

        private static readonly (string[] Rows, ConsoleColor Color)[] Word =
        {
            (new[]
            {
                "█████",
                "█    ",
                "████ ",
                "█    ",
                "█████"
            }, ConsoleColor.Red),
            (new[]
            {
                "█   █",
                "█   █",
                "█████",
                "█   █",
                "█   █"
            }, ConsoleColor.Blue),
            (new[]
            {
                " ███ ",
                "█   █",
                "█████",
                "█   █",
                "█   █"
            }, ConsoleColor.Green),
            (new[]
            {
                "█   █",
                "█   █",
                "█████",
                "█   █",
                "█   █"
            }, ConsoleColor.Yellow),
            (new[]
            {
                " ███ ",
                "█   █",
                "█   █",
                "█   █",
                " ███ "
            }, ConsoleColor.Blue)
        };

        private const int WordStartRow = 3;

        public static void Print()
        {
            var originalColor = Console.ForegroundColor;

            Console.WriteLine();
            for (var i = 0; i < Circle.Length; i++)
            {
                Console.Write("  ");
                WriteCircleRow(Circle[i]);
                Console.Write("   ");

                var wordRow = i - WordStartRow;
                if (wordRow >= 0 && wordRow < Word[0].Rows.Length)
                {
                    WriteWordRow(wordRow, wordRow == 0);
                }

                Console.ForegroundColor = originalColor;
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        private static void WriteWordRow(int rowIndex, bool appendTrademark)
        {
            foreach (var (rows, color) in Word)
            {
                Console.ForegroundColor = color;
                Console.Write(rows[rowIndex]);
                Console.Write(' ');
            }

            if (appendTrademark)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("®");
            }
        }

        private static void WriteCircleRow(string row)
        {
            var mStart = row.IndexOf('█');
            var mEnd = row.LastIndexOf('█');
            if (mStart >= 0)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write(row[..mStart]);
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(row[mStart..(mEnd + 1)]);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write(row[(mEnd + 1)..]);
            }
            else
            {
                var mid = row.Length / 2;
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write(row[..mid]);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write(row[mid..]);
            }
        }
    }
}
