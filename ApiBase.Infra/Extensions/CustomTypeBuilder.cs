using System.Collections.Concurrent;
using System.Reflection;
using System.Reflection.Emit;

namespace ApiBase.Infra.Extensions
{
    /// <summary>
    /// Builds dynamic types at runtime using Reflection.Emit.
    /// Used for field projection — creates anonymous-like types with only the requested properties.
    /// Results are cached by property signature to avoid redundant type generation.
    /// 
    /// This class replaces both the former CustomTypeBuilder (no cache) and DynamicTypeBuilder (cached),
    /// which were duplicates with minor differences.
    /// </summary>
    public static class CustomTypeBuilder
    {
        private static readonly AssemblyName AssemblyName = new("DynamicTypesAssembly");
        private static readonly ModuleBuilder ModuleBuilder;
        private static readonly ConcurrentDictionary<string, Lazy<Type>> TypeCache = new();

        // ModuleBuilder is not thread-safe: different keys may be built at the same time.
        private static readonly Lock BuildLock = new();

        static CustomTypeBuilder()
        {
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(AssemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
        }

        /// <summary>
        /// Returns a cached dynamic type for the given property dictionary,
        /// or creates and caches a new one if not found.
        /// </summary>
        /// <param name="properties">Dictionary of property name to property type.</param>
        public static Type CreateType(Dictionary<string, Type> properties)
        {
            var cacheKey = BuildCacheKey(properties);

            // GetOrAdd alone can run the factory twice under concurrency (two types for the same key).
            // Lazy<T> guarantees a single BuildType per key; the losing Lazy is discarded unevaluated.
            return TypeCache.GetOrAdd(cacheKey, _ => new Lazy<Type>(() =>
            {
                lock (BuildLock)
                {
                    return BuildType(properties);
                }
            })).Value;
        }

        /// <summary>
        /// Returns a cached dynamic type for the given field dictionary (IDictionary overload).
        /// Used internally by query infrastructure.
        /// </summary>
        public static Type GetOrCreate(IDictionary<string, Type> fields)
        {
            return CreateType(new Dictionary<string, Type>(fields));
        }

        private static string BuildCacheKey(Dictionary<string, Type> properties)
        {
            // Ordered by name: {Name, Price} and {Price, Name} share the same type.
            return string.Join(";", properties.OrderBy(p => p.Key, StringComparer.Ordinal).Select(p => $"{p.Key}:{p.Value.FullName}"));
        }

        private static Type BuildType(Dictionary<string, Type> properties)
        {
            string typeName = $"DynamicType_{Guid.NewGuid():N}";

            var typeBuilder = ModuleBuilder.DefineType(typeName, TypeAttributes.Public | TypeAttributes.Class
            );

            foreach (var prop in properties)
            {
                CreateProperty(typeBuilder, prop.Key, prop.Value);
            }

            return typeBuilder.CreateTypeInfo()!;
        }

        private static void CreateProperty(TypeBuilder typeBuilder, string propertyName, Type propertyType)
        {
            var fieldBuilder = typeBuilder.DefineField($"_{propertyName}", propertyType, FieldAttributes.Private);
            var propertyBuilder = typeBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);
            var getMethod = typeBuilder.DefineMethod($"get_{propertyName}", MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, propertyType, Type.EmptyTypes);
            var getIL = getMethod.GetILGenerator();

            getIL.Emit(OpCodes.Ldarg_0);
            getIL.Emit(OpCodes.Ldfld, fieldBuilder);
            getIL.Emit(OpCodes.Ret);

            var setMethod = typeBuilder.DefineMethod($"set_{propertyName}", MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, null, new[] { propertyType });
            var setIL = setMethod.GetILGenerator();

            setIL.Emit(OpCodes.Ldarg_0);
            setIL.Emit(OpCodes.Ldarg_1);
            setIL.Emit(OpCodes.Stfld, fieldBuilder);
            setIL.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getMethod);
            propertyBuilder.SetSetMethod(setMethod);
        }
    }
}
