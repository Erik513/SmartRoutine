using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace SmartRoutine.Logic.Services
{
    public static class ChangeDetector
    {
        public static bool HasChanges<T>(
            T original,
            T current,
            params string[] ignoredProperties)
        {
            return !AreEqual(original, current, ignoredProperties);
        }

        public static bool AreEqual<T>(
            T first,
            T second,
            params string[] ignoredProperties)
        {
            var ignored = new HashSet<string>(
                ignoredProperties ?? Array.Empty<string>());

            return AreObjectsEqual(first, second, ignored);
        }

        private static bool AreObjectsEqual(
            object first,
            object second,
            HashSet<string> ignoredProperties)
        {
            if (ReferenceEquals(first, second))
                return true;

            if (first == null || second == null)
                return false;

            if (first.GetType() != second.GetType())
                return false;

            var type = first.GetType();

            if (IsSimpleType(type))
                return first.Equals(second);

            if (typeof(IEnumerable).IsAssignableFrom(type) &&
                type != typeof(string))
            {
                return AreEnumerablesEqual(
                    (IEnumerable)first,
                    (IEnumerable)second,
                    ignoredProperties);
            }

            foreach (var property in type.GetProperties(
                BindingFlags.Public | BindingFlags.Instance))
            {
                if (!property.CanRead)
                    continue;

                if (ignoredProperties.Contains(property.Name))
                    continue;

                var firstValue = property.GetValue(first);
                var secondValue = property.GetValue(second);

                if (!AreObjectsEqual(firstValue, secondValue, ignoredProperties))
                    return false;
            }

            return true;
        }

        private static bool AreEnumerablesEqual(
            IEnumerable first,
            IEnumerable second,
            HashSet<string> ignoredProperties)
        {
            var firstEnumerator = first.GetEnumerator();
            var secondEnumerator = second.GetEnumerator();

            while (true)
            {
                bool firstHasNext = firstEnumerator.MoveNext();
                bool secondHasNext = secondEnumerator.MoveNext();

                if (firstHasNext != secondHasNext)
                    return false;

                if (!firstHasNext)
                    return true;

                if (!AreObjectsEqual(
                    firstEnumerator.Current,
                    secondEnumerator.Current,
                    ignoredProperties))
                {
                    return false;
                }
            }
        }

        private static bool IsSimpleType(Type type)
        {
            return type.IsPrimitive ||
                   type.IsEnum ||
                   type == typeof(string) ||
                   type == typeof(decimal) ||
                   type == typeof(DateTime) ||
                   type == typeof(DateTime?) ||
                   type == typeof(Guid) ||
                   type == typeof(Guid?);
        }
    }
}