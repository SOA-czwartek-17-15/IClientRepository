/*  SOA - Serwis zarządzania klientami dla banku
    created by Bartłomiej Hebda & Tomasz Bąba | 2014  */

using NLog;
using Npgsql;

using System;
using System.Reflection;
using System.ServiceModel;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel.Description;
using System.Configuration;

using Contracts;

namespace ClientRepository {

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ClientRepositoryService : IClientRepository {

        public ClientRepositoryService(int portNumber, string serviceRepo, ISqlConnector provider) {
            Logger.Info("Przypisanie połączenia z bazą danych");
            SqlConnector = provider;

            string hostUri = string.Format("net.tcp://localhost:{0}/IClientRepository", portNumber);

            Logger.Info("Tworzenie CRServiceHost pod adresem {0}...", hostUri);

            try {
                CRServiceHost = new ServiceHost(this, new[] { new Uri(hostUri) });
            } catch {
                Logger.Error("Nie udało się utworzyć CRServiceHost.");
                //Environment.Exit(1);
                throw new Exception("Critical");
            }

            Logger.Info("CRServiceHost został poprawie utworzony.");
            Logger.Info("Tworzenie obiektu CRServiceMetaBehavior...");

            try {
                CRServiceMetaBehavior = CRServiceHost.Description.Behaviors.Find<ServiceMetadataBehavior>();

                if (CRServiceMetaBehavior == null) {
                    Logger.Info("ServiceMetaDataBehavior nie istnieje dla CRServiceHost pod adresem {0}.", hostUri);
                    Logger.Info("Tworzenie nowego CRServiceMetadataBehavior");

                    CRServiceMetaBehavior = new ServiceMetadataBehavior();

                    Logger.Info("Dodawanie CRServiceMetaBehavior do opisu CRServiceHost.");
                    CRServiceHost.Description.Behaviors.Add(CRServiceMetaBehavior);

                    CRServiceMetaBehavior.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
                    Logger.Info("Zmiana PolicyVersion na {0}", CRServiceMetaBehavior.MetadataExporter.PolicyVersion);
                } else {
                    Logger.Warn("ServiceMetadataBehaviour już istnieje");
                }
            } catch {
                Logger.Error("Nie można utworzyć i dodać CRServiceMetaBehavior do CRServiceHost.");
                //Environment.Exit(1);
                throw new Exception("Critical");
            }

            Logger.Info("CRServiceMetaBehaviour poprawnie utworzony i ustawiony");
            Logger.Info("Tworzenie Endpointów dla CRServiceHost");

            try {
                Logger.Info("Dodawanie MetadataExchange jako endpoint dla CRServiceHost...");
                CRServiceHost.AddServiceEndpoint(ServiceMetadataBehavior.MexContractName, MetadataExchangeBindings.CreateMexTcpBinding(), "MEX");
                Logger.Info("Wystawianie interfejsu IClientRepository jako endpoint dla CRServiceHost...");
                CRServiceHost.AddServiceEndpoint(typeof(IClientRepository), new NetTcpBinding(SecurityMode.None), new Uri(hostUri));
            } catch {
                Logger.Error("Nie można wystawić endpointu.");
                //Environment.Exit(1);
                throw new Exception("Critical");
            }

            Logger.Info("Poprawnie dodano endpoint'y do CRServiceHost");
            Logger.Info("Otwieranie CRServiceHost...");

            try {
                CRServiceHost.Open();
            } catch {
                Logger.Error("Nie można otworzyć CRServiceHost!");
                //Environment.Exit(1);
                throw new Exception("Critical");
            }

            /*try {
                // string serviceRepositoryAddress = ConfigurationManager.AppSettings["serviceRepositoryServer"];
                NetTcpBinding binding = new NetTcpBinding(SecurityMode.None);
                ChannelFactory<IServiceRepository> cf = new ChannelFactory<IServiceRepository>(binding, new EndpointAddress(serviceRepo));
                ServiceRepo = cf.CreateChannel();
                ServiceRepo.RegisterService("IClientRepository", ConfigurationManager.AppSettings["localAddress"]);
            } catch {
                Logger.Error("Nie można znaleźć IServiceRepository");
                throw new Exception("Critical");
            }*/

            Logger.Warn("Ustanowiono połączenie! :)");
            // TODO: rejestrowanie w ServiceRepository po poprawnym utworzeniu połączenia
        }

        #region Interface Implentation

        public Guid CreateClient(ClientInformation clientInfo) {
            Guid newClient = Guid.NewGuid();

            string command = string.Format("INSERT INTO customers VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}');",
                    clientInfo.FirstName,
                    clientInfo.LastName,
                    clientInfo.Country,
                    clientInfo.City,
                    clientInfo.Street,
                    clientInfo.PostCode,
                    clientInfo.BirthPlace,
                    clientInfo.BirthDate.ToShortDateString(),
                    newClient.ToString("N"));

            try {
                SqlConnector.Insert(command);
                Logger.Info("Zapytanie {0} zostało wykonane prawidłowo!", command);
                return newClient;
            } catch {
                Logger.Error("Zapytanie {0} nie zostało wykonane poprawnie!", command);
                return Guid.Empty;
            }

        }

        public ClientInformation GetClientInformation(Guid clientID) {
            string command = string.Format("SELECT * FROM customers WHERE guid = '{0}';", clientID.ToString("N"));
            ClientInformation clientInfo = new ClientInformation();

            try {
                string[] data = SqlConnector.GetEntireRow(command).Split(';');
                clientInfo.FirstName = data[0];
                clientInfo.LastName = data[1];
                clientInfo.Country = data[2];
                clientInfo.City = data[3];
                clientInfo.Street = data[4];
                clientInfo.PostCode = data[5];
                clientInfo.BirthPlace = data[6];
                clientInfo.BirthDate = DateTime.ParseExact(data[7], "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);

                Logger.Info("Wyszukano klienta o guid = {0}!", clientID);

                return clientInfo;
            } catch {
                Logger.Warn("Klient o guid = {0} nie istnieje w bazie!", clientID);
                return null;
            }
        }

        public IEnumerable<Guid> SearchForClientsBy(ClientInformation someClientInfo) {
            string command = "SELECT guid FROM customers WHERE ";

            command += "first_name LIKE ";
            if (someClientInfo.FirstName != null)
                command += "'" + someClientInfo.FirstName + "'";
            else
                command += "'%'";
            command += " AND ";

            command += "last_name LIKE ";
            if (someClientInfo.LastName != null)
                command += "'" + someClientInfo.LastName+ "'";
            else
                command += "'%'";
            command += " AND ";

            command += "country LIKE ";
            if (someClientInfo.Country != null)
                command += "'" + someClientInfo.Country + "'";
            else
                command += "'%'";
            command += " AND ";

            command += "city LIKE ";
            if (someClientInfo.City!= null)
                command += "'" + someClientInfo.City + "'";
            else
                command += "'%'";
            command += " AND ";

            command += "street LIKE ";
            if (someClientInfo.Street != null)
                command += "'" + someClientInfo.Street + "'";
            else
                command += "'%'";
            command += " AND ";

            command += "post_code LIKE ";
            if (someClientInfo.PostCode != null)
                command += "'" + someClientInfo.PostCode+ "'";
            else
                command += "'%'";
            command += " AND ";

            command += "birth_place LIKE ";
            if (someClientInfo.BirthPlace != null)
                command += "'" + someClientInfo.BirthPlace + "'";
            else
                command += "'%'";
            command += " AND ";

            command += "birth_date LIKE ";
            if (someClientInfo.BirthDate != null && !someClientInfo.BirthDate.ToShortDateString().Equals("0001-01-01"))
                command += "'" + someClientInfo.BirthDate+ "'";
            else
                command += "'%'";

            command += ";";
            List<Guid> result = new List<Guid>();

            try {
                string[] data = SqlConnector.GetGuid(command).Split(';');
                foreach (var x in data) {
                    if (x != string.Empty)
                    result.Add(Guid.ParseExact(x, "N"));
                }

                Logger.Info("Znaleziono {0} rekordów!", data.Length);
                return result;

            } catch {
                Logger.Warn("Wystąpił problem przy pobieraniu danych klienta lub nie znaleziono klienta o danych {0}.", command);
                return null;
            }
                
        }

        public ISqlConnector GetConnector() { return SqlConnector; }

        #endregion
        #region Methods And Fileds

        public void Close() {
            // TODO: wyrejestrowanie z serwisu
            CRServiceHost.Close();
        }

        private ServiceHost CRServiceHost;
        private ISqlConnector SqlConnector;
        private ServiceMetadataBehavior CRServiceMetaBehavior;
        private IServiceRepository ServiceRepo;
        /// <summary>
        ///  Istnieje tylko jeden logger na klasę, bo:
        ///  - jest thread-safe 
        ///  - call stack można rozdzielić na call tylko z klasy, a nie ze wszystkich klas
        ///  - istnieje metoda GetCurrentClassLogger, ale nie jest supportowana przez compact framework
        /// </summary>
        private static Logger Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #endregion

    }


    public class Program {

        public static void Main(string[] args) {
            ClientRepositoryService service = new ClientRepositoryService(4321, "", new PSqlConnector("Server=127.0.0.1;Port=5432;User Id=postgres;Password=toor;Database=ClientRepository;"));
            var x = service.CreateClient(new ClientInformation { FirstName = "xxx", LastName = "yyy", Country = "zzz", Street = "ccc", City = "yyy", PostCode = "32-020", BirthPlace = "sss", BirthDate = DateTime.Now });
            Console.WriteLine(x);
            var result = service.GetClientInformation(x);
            Console.WriteLine(result.FirstName + " >> " + result.LastName);

            ClientInformation info = new ClientInformation();
            info.FirstName = "xxx";
            IEnumerable<Guid> y = service.SearchForClientsBy(info);
            foreach (var z in y)
                Console.WriteLine(z);

            service.Close();
            Console.WriteLine(Guid.NewGuid());
            Console.ReadLine();

        }
    }
}
