using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Imago.Support;
using Imago.Support.Numerics;

namespace Imago.ScenarioRunner;

/// <summary>
/// Discovers scenarios by scanning an assembly for methods marked with <see cref="ScenarioAttribute"/>.
/// </summary>
internal static class ScenarioRegistry
{
    /// <summary>
    /// Gets every scenario defined in the given assembly, ordered by name.
    /// </summary>
    /// <param name="assembly">The assembly to scan for scenario methods.</param>
    /// <returns>A read-only list of scenario descriptors.</returns>
    /// <exception cref="InvalidOperationException">Thrown when a marked method is invalid or two names collide.</exception>
    public static IReadOnlyList<ScenarioDescriptor> All(Assembly assembly)
    {
        var descriptors = new List<ScenarioDescriptor>();
        var namesSeen = new Dictionary<string, MethodInfo>(StringComparer.OrdinalIgnoreCase);

        foreach (var type in assembly.GetTypes())
        {
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                var attribute = method.GetCustomAttribute<ScenarioAttribute>();
                if (attribute == null)
                {
                    continue;
                }

                Validate(type, method);

                string name = attribute.Name ?? method.Name.ToKebabCase();
                if (namesSeen.TryGetValue(name, out var existing))
                {
                    throw new InvalidOperationException(
                        $"Two scenarios resolve to the name '{name}': {Describe(existing)} and {Describe(method)}.");
                }

                namesSeen.Add(name, method);

                var windowSizeAttribute = method.GetCustomAttribute<WindowSizeAttribute>();
                Vector2Int? windowSize = windowSizeAttribute != null
                    ? new Vector2Int(windowSizeAttribute.Width, windowSizeAttribute.Height)
                    : null;
                var backend = method.GetCustomAttribute<BackendAttribute>()?.Backend;
                bool isolated = method.GetCustomAttribute<IsolatedAttribute>() != null;

                descriptors.Add(new ScenarioDescriptor(
                    name,
                    attribute.Description ?? string.Empty,
                    Invoker(type, method),
                    windowSize,
                    backend,
                    isolated));
            }
        }

        return descriptors.OrderBy(descriptor => descriptor.Name, StringComparer.Ordinal).ToList();
    }

    private static Func<ScenarioContext, Task> Invoker(Type type, MethodInfo method)
    {
        return context =>
        {
            var instance = (ScenarioBase)Activator.CreateInstance(type)!;
            instance.Bind(context);
            return (Task)method.Invoke(instance, null)!;
        };
    }

    private static void Validate(Type type, MethodInfo method)
    {
        if (!typeof(ScenarioBase).IsAssignableFrom(type) || type.IsAbstract)
        {
            throw new InvalidOperationException(
                $"{Describe(method)} is marked [Scenario] but must belong to a non-abstract {nameof(ScenarioBase)} subclass.");
        }

        if (method.GetParameters().Length != 0 || !typeof(Task).IsAssignableFrom(method.ReturnType))
        {
            throw new InvalidOperationException(
                $"{Describe(method)} is marked [Scenario] but must take no parameters and return a Task.");
        }
    }

    private static string Describe(MethodInfo method)
    {
        return $"{method.DeclaringType!.Name}.{method.Name}";
    }
}
