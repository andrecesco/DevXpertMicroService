using System.Reflection;
using Xunit.v3;

namespace EduOnline.IntegrationTest;

public class PriorityOrderer : ITestCaseOrderer
{
    public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases) where TTestCase : Xunit.Sdk.ITestCase
    {
        var sortedMethods = new SortedDictionary<int, List<TTestCase>>();

        foreach (var testCase in testCases)
        {
            var priority = 0;

            if (testCase is IXunitTestCase xunitTestCase)
            {
                var attr = xunitTestCase.TestMethod.Method.GetCustomAttribute<TestPriorityAttribute>();
                if (attr != null)
                    priority = attr.Priority;
            }

            GetOrCreate(sortedMethods, priority).Add(testCase);
        }
        var prioritizedSortedMethods = sortedMethods.Keys.Select(priority => sortedMethods[priority]);
        foreach (var list in prioritizedSortedMethods)
        {
            list.Sort((x, y) => StringComparer.OrdinalIgnoreCase.Compare(GetTestMethodName(x), GetTestMethodName(y)));
            foreach (var testCase in list)
                yield return testCase;
        }
    }

    private static string GetTestMethodName<TTestCase>(TTestCase testCase) where TTestCase : Xunit.Sdk.ITestCase
    {
        return testCase is IXunitTestCase xunitTestCase ? xunitTestCase.TestMethod.Method.Name : string.Empty;
    }

    private static TValue GetOrCreate<TKey, TValue>(IDictionary<TKey, TValue> dictionary, TKey key) where TValue : new()
    {
        if (dictionary.TryGetValue(key, out var result)) return result;

        result = new TValue();
        dictionary[key] = result;

        return result;
    }

    public IReadOnlyCollection<TTestCase> OrderTestCases<TTestCase>(IReadOnlyCollection<TTestCase> testCases) where TTestCase : notnull, Xunit.Sdk.ITestCase
    {
        return OrderTestCases(testCases.AsEnumerable()).ToArray();
    }
}
