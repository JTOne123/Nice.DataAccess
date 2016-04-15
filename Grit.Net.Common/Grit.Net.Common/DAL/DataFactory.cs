﻿using Grit.Net.Common.Attributes;
using Grit.Net.Common.Exceptions;
using Grit.Net.Common.Models;
using Grit.Net.Common.TypeEx;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;

namespace Grit.Net.Common.DAL
{
    public class DataFactory
    {
        internal static Mapping<string, string> EntityAndTables;
        internal static Mapping<string, string> PropertyAndColumns;
        public static void Create()
        {
            string assemblyName = BaseConfig.DataFactoryEntityAssembly;
            string namespaceName = BaseConfig.DataFactoryEntityNamespace;
            if (string.IsNullOrEmpty(assemblyName) )
                throw new ConfigNotImplementException();
            Create(assemblyName, namespaceName);
        }
        public static void Create(string assemblyName, string namespaceName)
        { 
            if (!string.IsNullOrEmpty(assemblyName))
            {
                IEnumerable<Type> types = null;
                if (string.IsNullOrEmpty(namespaceName))
                    types = Assembly.Load(assemblyName).GetTypes().Where(t => t.IsClass);
                else
                    types = Assembly.Load(assemblyName).GetTypes().Where(t => t.IsClass && t.Namespace == namespaceName);

                EntityAndTables = new Mapping<string, string>(types.Count());
                PropertyAndColumns = new Mapping<string, string>();
                string typeName = null;
                string tableName = null;
                PropertyInfo[] properties = null;
                string propertyName = null;
                string columnName = null;

                TableAttribute attr = null;
                foreach (Type type in types)
                {
                    attr = type.GetCustomAttribute<TableAttribute>();
                    if (attr != null)
                    {
                        typeName = type.Name;
                        tableName = attr.Name;

                        EntityAndTables.Add(typeName.ToUpper(), attr.Name == null ? typeName : attr.Name);
                        properties = type.GetProperties();
                        foreach (PropertyInfo pi in properties)
                        {
                            ColumnAttribute attri = pi.GetCustomAttribute<ColumnAttribute>();
                            propertyName = pi.Name;
                            if (attri == null)
                                columnName = propertyName;
                            else
                                columnName = attri.Name;
                            PropertyAndColumns.Add(string.Format("{0}.{1}", typeName, propertyName.ToUpper()), string.Format("{0}.{1}", typeName, columnName));
                        }
                    }
                }
                EntityAndTables.TrimEmpty();
                PropertyAndColumns.TrimEmpty();
            }
        }
    

    }

    public class DataFactory<T> where T : TEntity
    {
        private static string DataProviderFullName = ConfigurationManager.AppSettings["DataProviderFullName"];
        private static Assembly CurrentAssembly = Assembly.GetExecutingAssembly();
        public static IGeneralDAL<T> Create()
        {
            #region DELETE
            //Type genericType = CurrentAssembly.GetType(string.Format("{0}`1", GeneralDALClassName));
            //genericType = genericType.MakeGenericType(typeof(T));
            //return Activator.CreateInstance(genericType) as IGeneralDAL<T>;
            #endregion
            return CurrentAssembly.CreateInstance(string.Format("{0}`1[[{1}]]", DataProviderFullName, typeof(T).AssemblyQualifiedName)) as IGeneralDAL<T>;
        }
    }
}
