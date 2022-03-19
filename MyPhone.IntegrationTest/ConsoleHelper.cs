using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPhone.IntegrationTest
{
    public class ConsoleHelper
    {
        public static int ReadNumberIndexFromConsole(ILog? logger = null)
        {
            if (logger == null)
            {
                logger = LogManager.GetLogger(typeof(ConsoleHelper));
            }

            logger.Info("ℹ️⌨️ Please type the number index to run the test");
            string? str = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(str))
            {
                logger.Warn("Invalid input, please type again");
                return ReadNumberIndexFromConsole(logger);
            }

            if (int.TryParse(str, out int index))
            {
                return index;
            }
            else
            {
                logger.Warn("Invalid input, please re-type again");
                return ReadNumberIndexFromConsole(logger);
            }
        }
    }
}
