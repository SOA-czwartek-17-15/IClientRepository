/*  SOA - Serwis zarządzania klientami dla banku
    created by Bartłomiej Hebda & Tomasz Bąba | 2014  */

using System;
using System.ServiceModel;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel.Description;

namespace ClientRepository {

    [ServiceContract]
    public interface IClientRepository {

        /// <summary>
        /// Tworzy klienta w serwisie ClientRepository z danymi osobistymi podanymi jako parametr
        /// oraz nadaje mu unikalny identyfikator.
        /// </summary>
        /// <param name="clientInfo">Dane personalne klienta</param>
        /// <returns>Unikalny identyfikator klienta</returns>
        [OperationContract]
        Guid CreateClient(ClientInformation clientInfo);

        /// <summary>
        /// Pobiera informacje personalne klienta o wyspecyfikowanym identyfikatorze.
        /// </summary>
        /// <param name="clientID">Unikalny identyfikator klienta</param>
        /// <returns>Informacje personalne o kliencie lub null gdy nie znaleziono</returns>
        [OperationContract]
        ClientInformation GetClientInformation(Guid clientID);

        /// <summary>
        /// Przeszukuje bazę serwisu ClientRepository w poszukiwaniu klientów, którzy pasują
        /// do wzorca (niepuste pola ClientInformation). Następnie zwraca wszystkich znalezionych klientów.
        /// </summary>
        /// <param name="someClientInfo">Częściowo wypełnione informacje o kliencie</param>
        /// <returns>Znalezione identyfikatory, które spełniają warunek</returns>
        [OperationContract]
        IEnumerable<Guid> SearchForClientsBy(ClientInformation someClientInfo);

    }

    [DataContract(Namespace = "ClientRepository")]
    public enum GenderType { MALE, FEMALE }

    [DataContract(Namespace = "ClientRepository")]
    public class ClientInformation {

        [DataMember]
        public string FirstName { get; set; }

        [DataMember]
        public string LastName { get; set; }

        [DataMember]
        public string Address { get; set; }

        [DataMember]
        public string BirthPlace { get; set; }

        [DataMember]
        public DateTime BirthDate { get; set; }

        [DataMember]
        public GenderType Gender { get; set; }
		
    } 
}