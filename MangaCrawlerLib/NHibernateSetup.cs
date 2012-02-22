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

                if (a_log)
                {
                    db.LogFormattedSql = true;
                    db.LogSqlInConsole = true;
                    db.AutoCommentSql = true;
                }
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
                (type.GetConstructor().Invoke() as IClassMapping).Map(mapper);
               // (FormatterServices.GetUninitializedObject(type) as IClassMapping).Map(mapper);

            HbmMapping mapping = mapper.CompileMappingForAllExplicitlyAddedEntities();

            Loggers.NHibernate.Info(mapping.AsString());

            Configuration.AddDeserializedMapping(mapping, "MangaCrawler"); 
        }

        public static void CreateDatabaseSchema()
        {
            new SchemaExport(Configuration).Drop(false, true);
            new SchemaExport(Configuration).Create(false, true);
        }

        // TODO: po co jest ValidateSchema
        protected static void ValidateSchema()
        {
            SchemaValidator schemaValidator = new SchemaValidator(Configuration);
            schemaValidator.Validate();
        }
    }
}
