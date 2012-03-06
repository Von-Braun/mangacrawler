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
using NHibernate.Linq;
using MangaCrawlerLib.Crawlers;
using System.IO;

namespace MangaCrawlerLib
{
    public class NH
    {
        private static Configuration Configuration;
        private static ISessionFactory SessionFactory;
        private static bool s_log = false;
        private static bool s_in_memory = false;

        private static string s_database_dir;
        public readonly static string s_database_name = "manga.db";

        public static void SetupFromFile(string a_database_dir)
        {
            s_database_dir = a_database_dir;

            Prepare();
            //if (!CheckDatabaseSchemaAndVersion()) // TODO:
              RecreateDatabase();
            ResetStates();
        }

        public static void SetupFromMemory()
        {
            s_in_memory = true;

            Prepare();
            RecreateDatabase();
        }

        private static void Prepare()
        {
            CreateConfiguration();
            AddMappings();
            SchemaMetadataUpdater.QuoteTableAndColumns(Configuration);
            SessionFactory = Configuration.BuildSessionFactory();
        }

        private static void CreateConfiguration()
        {
            Configuration = new Configuration();

            Configuration.DataBaseIntegration(db =>
            {
                db.Driver<NHibernate.Driver.SQLite20Driver>();
                db.Dialect<NHibernate.Dialect.SQLiteDialect>();
                db.KeywordsAutoImport = Hbm2DDLKeyWords.AutoQuote;
                db.IsolationLevel = IsolationLevel.Serializable;
                db.HqlToSqlSubstitutions = "true=1;false=0";

                if (s_in_memory)
                {
                    db.ConnectionString = "Data Source=:memory:;Version=3;New=True;Pooling=True;Max Pool Size=1";
                }
                else
                {
                    db.ConnectionString = String.Format(
                        "Data Source=\"{0}\\{1}\";Version=3",
                        s_database_dir, s_database_name);
                }

                db.LogFormattedSql = s_log;
                db.LogSqlInConsole = s_log;
                db.AutoCommentSql = s_log;
            });
        }

        private static ISession OpenSession()
        {
            return SessionFactory.OpenSession();
        }

        public static void Transaction(Action<ISession> a_action)
        {
            using (var session = OpenSession())
            {

                ITransaction transaction = session.BeginTransaction();

                try
                {
                    a_action(session);
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    Loggers.NH.Fatal("Exception", ex);
                    transaction.Rollback();
                }
                finally
                {
                    session.Dispose();
                }
            }
        }

        public static void TransactionLockUpdate<T>(T a_obj, Action a_action)
        {
            using (var session = OpenSession())
            {
                ITransaction transaction = session.BeginTransaction();

                session.Lock(a_obj, LockMode.None);

                try
                {
                    a_action();
                    session.SaveOrUpdate(a_obj);
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    Loggers.NH.Fatal("Exception", ex);
                    transaction.Rollback();
                }
                finally
                {
                    session.Dispose();
                }
            }
        }

        public static void TransactionLock<T>(T a_obj, Action a_action) 
        {
            using (var session = OpenSession())
            {
                ITransaction transaction = session.BeginTransaction();

                session.Lock(a_obj, LockMode.None);

                try
                {
                    a_action();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    Loggers.NH.Fatal("Exception", ex);
                    transaction.Rollback();
                }
                finally
                {
                    session.Dispose();
                }
            }
        }

        public static R TransactionLockWithResult<T, R>(T a_obj, Func<R> a_func)
        {
            using (var session = OpenSession())
            {
                ITransaction transaction = session.BeginTransaction();

                session.Lock(a_obj, LockMode.None);

                try
                {
                    R result = a_func();
                    transaction.Commit();
                    return result;
                }
                catch (Exception ex)
                {
                    Loggers.NH.Fatal("Exception", ex);
                    transaction.Rollback();
                    return default(R);
                }
                finally
                {
                    session.Dispose();
                }
            }
        }

        public static R TransactionLockUpdateWithResult<T, R>(T a_obj, Func<R> a_func)
        {
            using (var session = OpenSession())
            {
                ITransaction transaction = session.BeginTransaction();

                session.Lock(a_obj, LockMode.None);

                try
                {
                    R result = a_func();
                    session.SaveOrUpdate(a_obj);
                    transaction.Commit();
                    return result;
                }
                catch (Exception ex)
                {
                    Loggers.NH.Fatal("Exception", ex);
                    transaction.Rollback();
                    return default(R);
                }
                finally
                {
                    session.Dispose();
                }
            }
        }

        public static T TransactionWithResult<T>(Func<ISession, T> a_func)
        {
            using (var session = OpenSession())
            {
                ITransaction transaction = session.BeginTransaction();

                try
                {
                    T result = a_func(session);
                    transaction.Commit();
                    return result;
                }
                catch (Exception ex)
                {
                    Loggers.NH.Fatal("Exception", ex);
                    transaction.Rollback();
                    return default(T);
                }
                finally
                {
                    session.Dispose();
                }
            }
        }

        private static void AddMappings()
        {
            
            ModelMapper mapper = new ModelMapper();

            var types = from type in Assembly.GetAssembly(typeof(NH)).GetTypes()
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
            
            Loggers.NH.Debug(mapping.AsString());

            Configuration.AddDeserializedMapping(mapping, "MangaCrawler"); 
        }

        public static string Version
        {
            get
            {
                return String.Format("{0}.{1}",
                    Assembly.GetExecutingAssembly().GetName().Version.Major,
                    Assembly.GetExecutingAssembly().GetName().Version.Minor);
            }
        }

        public static void RecreateDatabase()
        {
            Loggers.NH.Info("Creating new database.");

            SessionFactory.Close();
            new FileInfo(String.Format("{0}\\{1}", s_database_dir, s_database_name)).Delete();
            Prepare();

            new SchemaExport(Configuration).Drop(false, true);
            new SchemaExport(Configuration).Create(false, true);

            Transaction((session) =>
            {
                session.Save(new DBVersion() { Version = Version });

                var servers = (from c in CrawlerList.Crawlers
                               select new Server(c.GetServerURL(), c.Name)).ToList();

                foreach (var server in servers)
                    session.Save(server);
            });
        }

        private static bool CheckDatabaseSchemaAndVersion()
        {
            DBVersion[] version_table;

            try
            {
                version_table = OpenSession().Query<DBVersion>().ToArray();
            }
            catch
            {
                Loggers.NH.Error("Probably empty db.");
                return false;
            }

            if (version_table.Count() != 1)
            {
                Loggers.NH.Error("Too many versions.");
                return false;
            }

            var db_version = version_table.First().Version;

            if (db_version != Version)
            {
                Loggers.NH.Error("Unknown database version.");
                return false;
            }
            
            //SchemaValidator validator = new SchemaValidator(Configuration);
            //try
            //{
            //    validator.Validate();
            //}
            //catch (Exception ex)
            //{
            //    Loggers.NH.Fatal("Exception", ex);
            //    return false;
            //}

            return true;
        }

        private static void ResetStates()
        {
            Transaction(session =>
            {
                foreach (var server in session.Query<Server>().Where(s => s.State != ServerState.Initial))
                {
                    server.SetState(ServerState.Initial);
                    session.SaveOrUpdate(server);
                }

                foreach (var serie in session.Query<Serie>().Where(s => s.State != SerieState.Initial))
                {
                    serie.SetState(SerieState.Initial);
                    session.Update(serie);
                }

                foreach (var chapter in session.Query<Chapter>().Where(s => s.State != ChapterState.Initial))
                {
                    chapter.SetState(ChapterState.Initial);
                    session.Update(chapter);
                }
            });
        }
    }
}
