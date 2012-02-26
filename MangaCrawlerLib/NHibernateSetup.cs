using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Cfg.Loquacious;
using NHibernate.Cfg;
using NHibernate;
using System.Data;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NHibernate.Tool.hbm2ddl;
using System.Reflection;
using TomanuExtensions;
using System.Runtime.Serialization;

namespace MangaCrawlerLib
{
    public class NHibernateSetup
    {
        private static Configuration Configuration;
        private static ISessionFactory SessionFactory;

        public static string DatabaseDir;
        public readonly static string DatabaseName = "manga.db";

        public static void Setup(bool a_log)
        {
            CreateConfiguration(a_log);
            AddMappings();
            SchemaMetadataUpdater.QuoteTableAndColumns(Configuration);
            SessionFactory = Configuration.BuildSessionFactory();
        }

        public static ISession CreateSession()
        {
            return SessionFactory.OpenSession();
        }

        private static void CreateConfiguration(bool a_log)
        {
            Configuration = new Configuration();

            Configuration.DataBaseIntegration(db =>
            {
                db.Driver<NHibernate.Driver.SQLite20Driver>();
                db.Dialect<NHibernate.Dialect.SQLiteDialect>();
                db.KeywordsAutoImport = Hbm2DDLKeyWords.AutoQuote;
                db.IsolationLevel = IsolationLevel.ReadCommitted;
                db.HqlToSqlSubstitutions = "true=1;false=0";
                db.ConnectionString = String.Format(
                    "Data Source=\"{0}\\{1}\";Version=3",
                    DatabaseDir, DatabaseName);

                db.LogFormattedSql = a_log;
                db.LogSqlInConsole = a_log;
                db.AutoCommentSql = a_log;
            });
        }

        private static void AddMappings()
        {
            
            ModelMapper mapper = new ModelMapper();

            var types = from type in Assembly.GetAssembly(typeof(NHibernateSetup)).GetTypes()
                        where !type.IsInterface
                        where type.IsImplementInterface(typeof(IClassMapping))
                        where !type.IsAbstract
                        select type;

            foreach (var type in types)
            {
                ConstructorInfo ci = type.GetConstructor(
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);
                IClassMapping cm = ci.Invoke() as IClassMapping;
                cm.GetType().InvokeMember(
                    "Map", 
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod,
                    null, 
                    cm, 
                    mapper);
            }

            HbmMapping mapping = mapper.CompileMappingForAllExplicitlyAddedEntities();
            
            Loggers.NH.Info(mapping.AsString());

            Configuration.AddDeserializedMapping(mapping, "MangaCrawler"); 
        }

        public static void CreateDatabaseSchema()
        {
            new SchemaExport(Configuration).Drop(false, true);
            new SchemaExport(Configuration).Create(false, true);
        }
    }
}
