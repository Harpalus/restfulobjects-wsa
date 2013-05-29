// Copyright � Naked Objects Group Ltd ( http://www.nakedobjects.net). 
// All Rights Reserved. This code released under the terms of the 
// Microsoft Public License (MS-PL) ( http://opensource.org/licenses/ms-pl.html) 
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using NakedObjects;
using NakedObjects.Services;

namespace AdventureWorksModel {
    [DisplayName("Customers")]
    public class CustomerRepository : AbstractFactoryAndRepository {
        #region Injected Services

        #region Injected: ContactRepository

        public ContactRepository ContactRepository { set; protected get; }

        #endregion

        #endregion

        #region FindCustomerByAccountNumber

        [MemberOrder(10)]
        public Customer FindCustomerByAccountNumber(string accountNumber) {
            IQueryable<Customer> query = from obj in Instances<Customer>()
                                         where obj.AccountNumber == accountNumber
                                         orderby obj.AccountNumber
                                         select obj;

            return SingleObjectWarnIfNoMatch(query);
        }

        #endregion

        #region Stores Menu

        [MemberOrder(20, Name="Stores")]
        [PageSize(2)]
        public IQueryable<Store> FindStoreByName(string name) {
            return from obj in Instances<Store>()
                   where obj.Name.ToUpper().Contains(name.ToUpper())
                   select obj;
        }

        [MemberOrder(40, Name = "Stores")]
        public Store CreateNewStoreCustomer()
        {
            var store = NewTransientInstance<Store>();
            store.CustomerType = "S";
            return store;
        }

        [MemberOrder(60, Name = "Stores")]
        public Store RandomStore()
        {
            return Random<Store>();
        }


        [MemberOrder(80, Name = "Stores")]
        public IQueryable<Store> QueryStores([Optionally, TypicalLength(40)] string whereClause,
                                        [Optionally, TypicalLength(40)] string orderByClause,
                                        bool descending)
        {
            return DynamicQuery<Store>(whereClause, orderByClause, descending);
        }

        public virtual string ValidateQueryStores(string whereClause, string orderByClause, bool descending)
        {
            return ValidateDynamicQuery<Store>(whereClause, orderByClause, descending);
        }

        #endregion

        #region Individuals Menu

        [MemberOrder(30, Name="Individuals")]
        public IQueryable<Individual> FindIndividualCustomerByName([Optionally] string firstName, string lastName)
        {
            IQueryable<Contact> matchingContacts = ContactRepository.FindContactByName(firstName, lastName);

            return from indv in Instances<Individual>()
                                           from contact in matchingContacts
                                           where indv.Contact.ContactID == contact.ContactID
                                           orderby indv.Contact.LastName, indv.Contact.LastName                                          
                                           select indv;
        }

        [MemberOrder(50, Name = "Individuals")]
        public Individual CreateNewIndividualCustomer(string firstName, string lastName, [DataType(DataType.Password)] string initialPassword) {
            var indv = NewTransientInstance<Individual>();
            indv.CustomerType = "I";
            Contact contact = NewTransientInstance<Contact>();
            contact.FirstName = firstName;
            contact.LastName = lastName;
            contact.EmailPromotion = 0;
            contact.NameStyle = false;
            contact.ChangePassword(null, initialPassword, null);
            indv.Contact = contact;
            Persist(ref indv);
            return indv;
        }

        [QueryOnly]
        [MemberOrder(70, Name = "Individuals")]
        public Individual RandomIndividual() {
            return Random<Individual>();
        }

         [MemberOrder(90, Name = "Individuals")]
        [PageSize(10)]
        public IQueryable<Individual> QueryIndividuals([Optionally, TypicalLength(40)] string whereClause,
                                                  [Optionally, TypicalLength(40)] string orderByClause,
                                                  bool descending) {
            return DynamicQuery<Individual>(whereClause, orderByClause, descending);
        }


        public virtual string ValidateQueryIndividuals(string whereClause, string orderByClause, bool descending) {
            return ValidateDynamicQuery<Individual>(whereClause, orderByClause, descending);
        }

        #endregion

        public void ThrowDomainException()
        {
            throw new DomainException("Foo");
        }
    }
}