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

namespace MangaCrawlerLib
{
    public class NHibernateSetup
    {
        private static Configuration Configuration;
        private static ISessionFactory SessionFactory;

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
                    "Data Source=\"{0}\\MangaCrawler\\manga.db\";Version=3",
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData));

                // TODO: skonfigurowac logowanie, powiazac je jakos z nlog, albo z niego zrezygnowac
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

            // TODO: czy da sie to zautomatyzowac
            var types = from type in Assembly.GetAssembly(typeof(NHibernateSetup)).GetTypes()
                        where !type.IsInterface
                        where type.IsImplementInterface(typeof(IClassMapping))
                        where !type.IsAbstract
                        select type;

            foreach (var type in types)
                (Activator.CreateInstance(type) as IClassMapping).Map(mapper);

            HbmMapping mapping = mapper.CompileMappingForAllExplicitlyAddedEntities();
            Configuration.AddDeserializedMapping(mapping, "MangaCrawler"); 
        }

        public static void CreateDatabaseSchema()
        {
            new SchemaExport(Configuration).Drop(false, true);
            new SchemaExport(Configuration).Create(false, true);
        }

        // TODO: jakie zastosowanie
        protected static bool ValidateSchema()
        {
            try
            {
                SchemaValidator schemaValidator = new SchemaValidator(Configuration);
                schemaValidator.Validate();
                return true;
            }
            catch (HibernateException e)
            {
                Console.WriteLine(e.Message); // TODO: log
                return false;
            }
        }
    }
}
