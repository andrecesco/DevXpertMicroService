using Xunit.Abstractions;
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

            // Use reflection to access TestMethod property since it may not be on ITestCase interface
            var testMethodProperty = testCase.GetType().GetProperty("TestMethod");
            if (testMethodProperty != null)
            {
                var testMethod = testMethodProperty.GetValue(testCase);
                var methodProperty = testMethod?.GetType().GetProperty("Method");
                if (methodProperty != null)
                {
                    var method = methodProperty.GetValue(testMethod) as IMethodInfo;
                    if (method != null)
                    {
                        foreach (var attr in method.GetCustomAttributes(typeof(TestPriorityAttribute).AssemblyQualifiedName))
                            priority = attr.GetNamedArgument<int>("Priority");
                    }
                }
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
        var testMethodProperty = testCase.GetType().GetProperty("TestMethod");
        if (testMethodProperty != null)
        {
            var testMethod = testMethodProperty.GetValue(testCase);
            var methodProperty = testMethod?.GetType().GetProperty("Method");
            if (methodProperty != null)
            {
                var method = methodProperty.GetValue(testMethod) as IMethodInfo;
                return method?.Name ?? string.Empty;
            }
        }
        return string.Empty;
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
