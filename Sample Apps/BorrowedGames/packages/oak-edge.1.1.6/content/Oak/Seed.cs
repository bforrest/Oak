﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;
using System.Text.RegularExpressions;
using Oak.Extensions;
using System.Diagnostics;
using System.IO;

namespace Oak
{
    [DebuggerNonUserCode]
    public class Seed
    {
        public virtual ConnectionProfile ConnectionProfile { get; set; }

        public Seed()
            : this(null)
        {
        }

        public Seed(ConnectionProfile connectionProfile)
        {
            if (connectionProfile == null) connectionProfile = new ConnectionProfile();
            ConnectionProfile = connectionProfile;
        }

        /// <summary>
        /// Generates script for creating a table.  Use ExecuteNonQuery() 
        /// to execute script.  ExecuteNonQuery is an 
        /// extension method on string (if you dont see the extension method, 
        /// make sure you have using Oak; in your cs file.
        /// </summary>
        /// <returns>Returns the sql command generated by seed.</returns>
        public string CreateTable(string table, dynamic[] columns)
        {
            string columnString = "";

            var primaryKeyColumn = null as string;

            foreach (var entry in columns)
            {
                object column = entry;

                if (column.IsPrimaryKey()) primaryKeyColumn = column.PrimaryKeyColumn();

                columnString += ColumnStringFor(column) + ",";
            }

            return CreateTableCommand(table, columnString, primaryKeyColumn);
        }

        /// <summary>
        /// Generates script to create columns.  Use ExecuteNonQuery()
        /// to execute script.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public string AddColumns(string table, dynamic[] columns)
        {
            string columnString = "";

            for (int i = 0; i < columns.Length; i++)
            {
                object column = columns[i];

                columnString += ColumnStringFor(column);

                if (i != columns.Length - 1) columnString += ", ";
            }

            return "ALTER TABLE [dbo].[{0}] ADD {1}"
                .With(table, columnString);
        }

        string ColumnStringFor(object column)
        {
            var name = column.Name();
            var type = column.SqlType();
            var defaultValue = column.DefaultValue();
            var isIdentity = column.IsIdentityColumn();
            var isPrimaryKey = column.IsPrimaryKey();
            var isForeignKey = column.IsForeignKeyColumn();

            string identityAsString = isIdentity ? " IDENTITY(1,1)" : "";
            string foreignKeyString = isForeignKey ? " FOREIGN KEY REFERENCES " + column.ForeignKey() : "";

            return "[{0}] {1} {2} {3}{4}{5}"
                        .With(name,
                            type,
                            column.NullDefinition(),
                            column.DefaultValueDefinition(),
                            identityAsString,
                            foreignKeyString)
                        .ReplaceSequentialSpacesWithSingleSpace()
                        .Trim();
        }

        string CreateTableCommand(string table, string columns, string primaryKeyColumn)
        {
            var primaryKeyScript =
                " CONSTRAINT [PK_{0}] PRIMARY KEY CLUSTERED ([{1}] ASC)".With(table, primaryKeyColumn ?? string.Empty);

            if (primaryKeyColumn == null) primaryKeyScript = "";

            return "CREATE TABLE [dbo].[{0}]({1}{2})".With(table, columns, primaryKeyScript);
        }

        public void PurgeDb()
        {
            DropAllForeignKeys();
            DropAllPrimaryKeys();
            DropAllTables();
        }

        void DropAllForeignKeys()
        {
            var reader = "select name as constraint_name, object_name(parent_obj) as table_name from sysobjects where xtype = 'f'".ExecuteReader(ConnectionProfile);

            while (reader.Read())
            {
                "alter table {0} drop constraint {1} ".With(reader["table_name"], reader["constraint_name"]).ExecuteNonQuery(ConnectionProfile);
            }
        }

        void DropAllPrimaryKeys()
        {
            var reader = "select name as constraint_name, object_name(parent_obj) as table_name from sysobjects where xtype = 'pk'".ExecuteReader(ConnectionProfile);

            while (reader.Read())
            {
                "alter table {0} drop constraint {1} ".With(reader["table_name"], reader["constraint_name"]).ExecuteNonQuery(ConnectionProfile);
            }
        }

        void DropAllTables()
        {
            var reader = "select name as table_name from sysobjects where xtype = 'u'".ExecuteReader(ConnectionProfile);

            while (reader.Read())
            {
                "drop table {0} ".With(reader["table_name"]).ExecuteNonQuery(ConnectionProfile);
            }
        }

        public void Export(string exportPath, IEnumerable<dynamic> scripts)
        {
            int order = 1;

            scripts.ForEach<dynamic>(s =>
            {
                var result = s();

                string name = s.Method.Name;

                if (result is string) WriteSqlFile(exportPath, "{0} - {1}".With(order, name), result);

                else
                {
                    int parts = 1;

                    foreach (var r in result)
                    {
                        WriteSqlFile(exportPath, "{0} - {1} - {2}".With(order, parts, name), r);

                        parts++;
                    }
                }

                order++;
            });
        }

        void WriteSqlFile(string path, string name, string content)
        {
            File.WriteAllText(Path.Combine(path, name + ".sql"), content);
        }

        public object Id()
        {
            return new { Id = "int", Identity = true, PrimaryKey = true };
        }

        public object GuidId()
        {
            return new { Id = "uniqueidentifier", PrimaryKey = true };
        }

        public void ExecuteNonQuery(dynamic script)
        {
            if (script is Delegate) ExecuteNonQuery(script());

            else if(script is string) (script as string).ExecuteNonQuery(ConnectionProfile);

            else foreach (var s in script) (s as string).ExecuteNonQuery(ConnectionProfile);
        }

        public string DisableKeyConstaints()
        {
            return "EXEC sp_msforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT all';";
        }

        public string EnableKeyConstraints()
        {
            return "EXEC sp_msforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT all';";
        }

        public void DeleteAllRecords()
        {
            DisableKeyConstaints().ExecuteNonQuery(ConnectionProfile);

            "EXEC sp_msforeachtable 'delete ?';".ExecuteNonQuery(ConnectionProfile);

            EnableKeyConstraints().ExecuteNonQuery(ConnectionProfile);
        }

        public string RenameColumn(string table, string currentColumnName, string newColumnName)
        {
            return "sp_rename '{0}.{1}', '{2}', 'COLUMN'".With(table, currentColumnName, newColumnName);
        }

        public string DropColumn(string table, string column)
        {
            return "alter table {0} drop column {1}".With(table, column);
        }

        public string DropConstraint(string table, string forColumn)
        {
            var name = @"
            select CONSTRAINT_NAME
            from INFORMATION_SCHEMA.KEY_COLUMN_USAGE 
            where TABLE_NAME = '{0}' 
            and COLUMN_NAME = '{1}'".With(table, forColumn).ExecuteScalar(ConnectionProfile);

            return "alter table {0} drop constraint {1}".With(table, name);
        }

        public void ExecuteUpTo(IEnumerable<Func<dynamic>> scripts, Func<dynamic> method)
        {
            foreach (Func<dynamic> script in scripts)
            {
                if (script.Method == method.Method) break;

                ExecuteNonQuery(script());
            }
        }

        public void ExecuteTo(IEnumerable<Func<dynamic>> scripts, Func<dynamic> method)
        {
            foreach (Func<dynamic> script in scripts)
            {
                ExecuteNonQuery(script());

                if (script.Method == method.Method) break;
            }
        }
    }
}

namespace Oak.Extensions
{
    public static class StringExtensions
    {
        public static string With(this string s, params object[] args)
        {
            return string.Format(s, args);
        }

        public static string ReplaceSequentialSpacesWithSingleSpace(this string s)
        {
            var single = Regex.Replace(s, @"[ ]{2,}", " ");

            return single;
        }
    }

    public static class ColumnExtensions
    {
        public static bool AllowsNulls(this object columnDefinition)
        {
            return !columnDefinition.Properties().Has("Nullable", withValue: false, @in: columnDefinition);
        }

        public static bool IsIdentityColumn(this object columnDefinition)
        {
            return columnDefinition.Properties().Has("Identity", withValue: true, @in: columnDefinition);
        }

        public static bool IsForeignKeyColumn(this object columnDefinition)
        {
            return columnDefinition.Properties().Has("ForeignKey", @in: columnDefinition);
        }

        public static object ForeignKey(this object columnDefinition)
        {
            return columnDefinition.Properties().Get("ForeignKey", @in: columnDefinition);
        }

        public static bool IsPrimaryKey(this object columnDefinition)
        {
            return columnDefinition.Properties().Has("PrimaryKey", withValue: true, @in: columnDefinition);
        }

        public static object DefaultValue(this object columnDefinition)
        {
            return columnDefinition.Properties().Get("Default", @in: columnDefinition);
        }

        public static object DefaultValueDefinition(this object columnDefinition)
        {
            var defaultValue = columnDefinition.DefaultValue();

            string defaultAsString = "";

            if (defaultValue != null) defaultAsString = "DEFAULT('{0}')".With(defaultValue.ToString());
            else return "";

            var reservedStrings = new[] { "getdate()", "newid()" };

            if (reservedStrings.Contains(defaultValue.ToString().ToLower())) defaultAsString = "DEFAULT({0})".With(defaultValue.ToString());

            return defaultAsString;
        }

        public static string NullDefinition(this object columnDefinition)
        {
            var nullDefinition = columnDefinition.AllowsNulls() ? "NULL" : "NOT NULL";

            if (columnDefinition.IsPrimaryKey() || columnDefinition.IsIdentityColumn()) nullDefinition = "NOT NULL";

            if (columnDefinition.IsForeignKeyColumn()) nullDefinition = "";

            return nullDefinition;
        }

        public static string PrimaryKeyColumn(this object columnDefinition)
        {
            if (columnDefinition.IsPrimaryKey())
            {
                return columnDefinition.Name();
            }

            return null;
        }

        public static string Name(this object columnDefinition)
        {
            return columnDefinition.Properties().First().Name;
        }

        public static bool Has(this PropertyInfo[] properties, string name, bool withValue, object @in)
        {
            return properties.Any(s => s.Name == name && Convert.ToBoolean(s.GetValue(@in, null)) == withValue);
        }

        public static bool Has(this PropertyInfo[] properties, string name, object @in)
        {
            return properties.Any(s => s.Name == name);
        }

        public static object Get(this PropertyInfo[] properties, string name, object @in)
        {
            var property = properties.SingleOrDefault(s => s.Name == name);

            if (property == null) return null;

            return property.GetValue(@in, null);
        }

        public static string Value(this PropertyInfo propertyInfo, object o)
        {
            return propertyInfo.GetValue(o, null).ToString();
        }

        public static string SqlType(this object columnDefinition)
        {
            return columnDefinition.Properties().First().Value(columnDefinition);
        }

        private static PropertyInfo[] Properties(this object columnDefinition)
        {
            return columnDefinition.GetType().GetProperties();
        }
    }
}