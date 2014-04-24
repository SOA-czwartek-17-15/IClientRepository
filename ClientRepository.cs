using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace ClientRepository
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ClientRepository : IClientRepository
    {

        public Guid CreateClient(ClientInformation clientInfo)
        {
            throw new NotImplementedException();
        }

        public ClientInformation GetClientInformation(Guid clientID)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Guid> SearchForClientsBy(ClientInformation someClientInfo)
        {
            throw new NotImplementedException();
        }

    }


    public class ClientRepositoryService
    {
        public static void ShowInvocation()
        {
            ConsoleColor prev = Console.ForegroundColor;

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("+-------------------+");
            Console.WriteLine("| Client Repository |");
            Console.WriteLine("+-------------------+");
            Console.WriteLine();

            Console.ForegroundColor = prev;
        }

        public static void Main(string[] args)
        {
            ShowInvocation();
            //Logger.Test();
            Logger.LogInformation("Starting ClientRepository service...");

            try
            {
                ClientRepository clientRepository = new ClientRepository();
                Logger.LogSuccess("Client repository was created!");

                Logger.LogInformation("Trying to create ServiceHost...");
                var sh = new ServiceHost(clientRepository, new[] { new Uri("net.tcp://localhost:41234/IClientRepository") });
                Logger.LogSuccess("New referece to ServiceHost created!");

                Logger.LogInformation("Trying to establish metadata behaviour...");
                ServiceMetadataBehavior metadata = sh.Description.Behaviors.Find<ServiceMetadataBehavior>();
                if (metadata == null)
                {
                    metadata = new ServiceMetadataBehavior();
                    sh.Description.Behaviors.Add(metadata);
                }
                else
                {
                    Logger.LogWarrning("Metadata already exists");
                }
                metadata.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
                Logger.LogSuccess("PolicyVersion is now Policy15!");

                Logger.LogInformation("Adding new endpoints to the service");
                sh.AddServiceEndpoint(ServiceMetadataBehavior.MexContractName, MetadataExchangeBindings.CreateMexTcpBinding(), "MEX");
                sh.AddServiceEndpoint(typeof(IClientRepository), new NetTcpBinding(SecurityMode.None), new Uri("net.tcp://localhost:41234/IClientRepository"));

                Logger.LogInformation("Trying to start ClientRepository service...");

                sh.Open();

                Logger.LogSuccess("ClientRepository up & ready!");

            }
            catch (Exception e)
            {
                Logger.LogError(e.Message);
                Logger.LogWarrning("Service will be closed! :C");
                Console.ReadLine();
                return;
            }

            Console.ReadLine();

        }
    }
}
