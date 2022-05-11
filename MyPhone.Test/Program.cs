// See https://aka.ms/new-console-template for more information
using log4net;
using log4net.Config;
using MyPhone.IntegrationTest;
using MyPhone.IntegrationTest.Attributes;
using System.Reflection;
using System.Xml;

ILog logger = LogManager.GetLogger("TestRunner");
//Get log4net config in assembly
XmlDocument xml = new XmlDocument();
xml.LoadXml(MyPhone.Test.Properties.Resources.log4net);
XmlConfigurator.Configure(xml.DocumentElement);

logger.Info("   MyPhone Test Utility  ");
logger.Info("=========================");

var testTypes = Assembly.GetExecutingAssembly()
    .GetTypes()
    .Where(t => t.GetCustomAttributes(typeof(TestAttribute), true).Length > 0)
    .ToList();

List<Type> tests = new List<Type>();
foreach (Type t in testTypes)
{
    tests.Add(t);
}

while (true)
{
    Console.WriteLine();
    logger.Info("*************************");
    logger.Info("Available tests:");
    for (int i = 0; i < tests.Count; i++)
    {
        logger.Info(" " + i + ":  " + tests[i].Name);
    }
    Console.WriteLine();
    int index = ReadTestTaskIndex();

    Type testType = tests[index];
    ConstructorInfo? constructorInfo = testType.GetConstructor(Type.EmptyTypes);
    if (constructorInfo == null)
    {
        logger.ErrorFormat("[{0}] TestRunner does not support constrcutor that has parameters, your constrcutor must not contains any parameters", testType.Name);
        logger.Info("ℹ️⌨️ Press enter to continue");
        Console.ReadLine();
        continue;
    }
    List<MethodInfo> testMethods = testType.GetMethods()
        .Where(m => m.GetCustomAttributes(typeof(TestMethodAttribute), false).Length > 0)
        .ToList();
    if (testMethods.Count == 0)
    {
        logger.Error("This test class does not contains any test method. Abort!");
        continue;
    }

    await RunTestTask(testType, constructorInfo, testMethods);

    logger.Info("ℹ️⌨️ Press enter to select other test task");
    Console.ReadLine();
    Console.Clear();
}

async Task RunTestTask(Type testType, ConstructorInfo constructorInfo, IEnumerable<MethodInfo> testMethodEnum)
{
    Dictionary<MethodInfo, bool> testMethods = testMethodEnum.ToDictionary(m => m, m => false);

    logger.Info("-------------------------");
    logger.InfoFormat("Running test [{0}]", testType.Name);
    logger.InfoFormat("There are {0} test cases in this test: ", testMethods.Count);
    int i = 0;
    foreach (MethodInfo info in testMethods.Keys)
    {
        logger.InfoFormat("  {0}. {1}", i, info.Name);
        i++;
    }
    Console.WriteLine();

    object instance = constructorInfo.Invoke(null);

    i = 0;
    foreach (MethodInfo info in testMethods.Keys)
    {
        testMethods[info] = await RunTestCase(info, instance, i);
    }
    logger.Info("===========================================================================");
    logger.InfoFormat("Summary of [{0}]: ", testType.Name);
    logger.Info("===========================================================================");
    logger.InfoFormat("|{0,-40}|{1,-15}|", "Test case", "Passed");
    foreach ((MethodInfo method, bool success) in testMethods)
    {
        logger.InfoFormat("|{0,-40}|{1,-15}|", method.Name, success ? "Passed ✅" : "Failed ❌");
    }
    logger.Info("===========================================================================");
    int successfulCases = testMethods.Values.Where(s => s).Count();
    if (successfulCases == testMethods.Count)
    {
        logger.InfoFormat("Success            ✅ [{0}/{1}] {2}", successfulCases, testMethods.Count, testType.Name);
    }
    else if (successfulCases == 0)
    {
        logger.InfoFormat("Failed       ❌ [{0}/{1}] {2}", successfulCases, testMethods.Count, testType.Name);
    }
    else
    {
        logger.InfoFormat("Failed some cases  ⚠️ [{0}/{1}] {2}", successfulCases, testMethods.Count, testType.Name);
    }
}

async Task<bool> RunTestCase(MethodInfo method, object instance, int caseId)
{
    void NextPage()
    {
        for (int i = 0; i < 5; i++)
        {
            Console.WriteLine();
        }
    }

    logger.InfoFormat("Running test case #{0}. {1}", caseId, method.Name);
    logger.Info("ℹ️⌨️ Press enter to continue");
    Console.ReadLine();
    logger.Info(">>>>>>>>>>>>>>>>>>>>>>>>>");
    NextPage();

    // Run test case on a new thread.
    bool success = await Task.Run<bool>(async () =>
    {
        try
        {
            object? ret = method.Invoke(instance, null);
            if (method.ReturnType.IsAssignableTo(typeof(Task)))
            {
                await (Task)ret!;
            }
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            logger.ErrorFormat("Unhandled exception occured in #{0}. {1}", caseId, method.Name);
            if (ex.InnerException != null)
            {
                logger.Error(ex.InnerException.ToString());
            }
            else
            {
                logger.Error(ex.ToString());
            }
            return false;
        }
        
    });

    NextPage();
    logger.Info("<<<<<<<<<<<<<<<<<<<<<<<<<");
    if (success)
    {
        logger.InfoFormat("Test case #{0}.{1} passed ✅", caseId, method.Name);
    }
    else
    {
        logger.InfoFormat("Test case #{0}.{1} failed ❌", caseId, method.Name);
    }
    return success;
}

int ReadTestTaskIndex()
{
    int index = ConsoleHelper.ReadNumberIndexFromConsole(logger);
    if (index < 0 || index >= tests.Count)
    {
        logger.Warn("Does not exists a test corespond to this index, please type again");
        return ReadTestTaskIndex();
    }
    return index;
}

