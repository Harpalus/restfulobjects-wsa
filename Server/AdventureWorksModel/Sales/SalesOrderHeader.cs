// Copyright � Naked Objects Group Ltd ( http://www.nakedobjects.net). 
// All Rights Reserved. This code released under the terms of the 
// Microsoft Public License (MS-PL) ( http://opensource.org/licenses/ms-pl.html) 
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using NakedObjects;

namespace AdventureWorksModel
{

    [DisplayName("Sales Order")]
    [IconName("trolley.png")]
    public class SalesOrderHeader : AWDomainObject, ICreditCardCreator
    {
        #region Injected Servives
        public ShoppingCartRepository ShoppingCartRepository { set; protected get; }
        #endregion

        #region Life Cycle Methods

        public void Created()
        {
            OrderDate = DateTime.Now.Date;
            DueDate = DateTime.Now.Date.AddDays(7);
            RevisionNumber = 1;
            Status = 1;
            SubTotal = 0;
            TaxAmt = 0;
            Freight = 0;
            TotalDue = 0;
        }

        public override void Updating()
        {
            base.Updating();
            const byte increment = 1;
            RevisionNumber += increment;

        }

        public override void Persisting()
        {
            if (Customer.IsStore())
            {
                contact = StoreContact.Contact;
            }
            base.Persisting();
        }

        public void Loaded()
        {
            UpdateStoreContact();
        }

        private void UpdateStoreContact()
        {
            if (Customer.IsStore() && StoreContact == null && Contact != null && Customer != null)
            {
                StoreContact = FindStoreContactForContact();
            }
        }

        public void Persisted()
        {
            UpdateStoreContact();
            if (AddItemsFromCart)
            {
                ShoppingCartRepository.AddAllItemsInCartToOrder(this);
                AddItemsFromCart = false;
            }
        }

        #endregion

        #region Properties

        #region ID

        [Hidden]
        public virtual int SalesOrderID { get; set; }

        #endregion

        #region SalesOrderNumber

        [Title]
        [Disabled]
        [MemberOrder(1)]
        public virtual string SalesOrderNumber { get; set; }

        #endregion

        #region Status

        //Properly, the Status property should be [Disabled], and modified only through
        //appropriate actions such as Approve.  It has been left modifiable here only
        //to demonstrate the behaviour of Enum properties.
        [MemberOrder(1.1), TypicalLength(12), EnumDataType(typeof(OrderStatus))]
        public virtual byte Status { get; set; }

        
        public OrderStatus DefaultStatus()
        {
            return OrderStatus.InProcess;
        }
      

        [Hidden]
        public virtual Boolean IsInProcess()
        {
            return Status.Equals((byte)OrderStatus.InProcess);
        }

        [Hidden]
        public virtual Boolean IsApproved()
        {
            return Status.Equals((byte)OrderStatus.Approved);
        }

        [Hidden]
        public virtual bool IsBackOrdered()
        {
            return Status.Equals((byte)OrderStatus.BackOrdered);
        }

        [Hidden]
        public virtual bool IsRejected()
        {
            return Status.Equals((byte)OrderStatus.Rejected);
        }

        [Hidden]
        public virtual bool IsShipped()
        {
            return Status.Equals((byte)OrderStatus.Shipped);
        }

        [Hidden]
        public virtual bool IsCancelled()
        {
            return Status.Equals((byte)OrderStatus.Cancelled);
        }

        #endregion

        #region Customer

        [Disabled, MemberOrder(2)]
        public virtual Customer Customer { get; set; }

        #endregion

        #region Contact

        private Contact contact;

        [Hidden]
        public virtual Contact Contact
        {
            get { return contact; }
            set { contact = value; }
        }

        internal void SetUpContact(Contact value)
        {
            Contact = value;
            storeContact = FindStoreContactForContact();
        }

        #region StoreContact Property

        private StoreContact storeContact;

        [MemberOrder(3)]
        public virtual StoreContact StoreContact
        {
            get { return storeContact; }
            set
            {
                if (value != null)
                {
                    storeContact = value;
                    Contact = value.Contact;
                }
            }
        }

        private StoreContact FindStoreContactForContact()
        {
            IQueryable<StoreContact> query = from obj in Container.Instances<StoreContact>()
                                             where obj.Contact.ContactID == Contact.ContactID && obj.Store.CustomerID == Customer.CustomerID
                                             select obj;

            return query.FirstOrDefault();
        }

        public virtual bool HideStoreContact()
        {
            return Customer != null && Customer.IsIndividual();
        }

        [Executed(Where.Remotely)]
        public List<StoreContact> ChoicesStoreContact()
        {
            if (Customer != null && Customer.IsStore())
            {
                return new List<StoreContact>(((Store)Customer).Contacts);
            }
            return new List<StoreContact>();
        }

        #endregion

        #endregion

        #region BillingAddress

        [MemberOrder(4)]
        public virtual Address BillingAddress { get; set; }

        public Address DefaultBillingAddress()
        {
            if (Customer == null)
            {
                return null;
            }
            IQueryable<Address> query = from obj in Container.Instances<CustomerAddress>()
                                        where obj.Customer.CustomerID == Customer.CustomerID &&
                                              obj.AddressType.Name == "Billing"
                                        select obj.Address;

            return query.FirstOrDefault();
        }

        [Executed(Where.Remotely)]
        public List<Address> ChoicesBillingAddress()
        {
            IQueryable<Address> query = from obj in Container.Instances<CustomerAddress>()
                                        where obj.Customer.CustomerID == Customer.CustomerID
                                        select obj.Address;

            return query.ToList();
        }

        #endregion

        #region PurchaseOrderNumber

        [Optionally, StringLength(25), MemberOrder(5)]
        public virtual string PurchaseOrderNumber { get; set; }

        #endregion

        #region ShippingAddress

        [MemberOrder(10)]
        public virtual Address ShippingAddress { get; set; }

        public Address DefaultShippingAddress()
        {
            if (Customer == null)
            {
                return null;
            }
            IQueryable<Address> query = from obj in Container.Instances<CustomerAddress>()
                                        where obj.Customer.CustomerID == Customer.CustomerID &&
                                              obj.AddressType.Name == "Shipping"
                                        select obj.Address;

            return query.FirstOrDefault();
        }

        [Executed(Where.Remotely)]
        public List<Address> ChoicesShippingAddress()
        {
            return ChoicesBillingAddress();
        }

        #endregion

        #region ShipMethod

        [MemberOrder(11)]
        public virtual ShipMethod ShipMethod { get; set; }

        
        public ShipMethod DefaultShipMethod()
        {
            return Container.Instances<ShipMethod>().FirstOrDefault();
        }
      

        #endregion

        #region AccountNumber

        [Optionally, StringLength(15), MemberOrder(12)]
        public virtual string AccountNumber { get; set; }

        #endregion

        #region Dates

        #region OrderDate

        [Disabled]
        [MemberOrder(20)]
        [Mask("d")]
        public virtual DateTime OrderDate { get; set; }

        #endregion

        #region DueDate

        [MemberOrder(21)]
        [Mask("d")]
        public virtual DateTime DueDate { get; set; }

        public string DisableDueDate()
        {
            return DisableIfShipped();
        }

        public string ValidateDueDate(DateTime dueDate) {

            if (dueDate.Date < OrderDate.Date) {
                return "Due date cannot be before order date";
            }

            return null; 
        }

        private string DisableIfShipped()
        {
            if (IsShipped())
            {
                return "Order has been shipped";
            }
            return null;
        }

        #endregion

        #region ShipDate

        [Optionally]
        [MemberOrder(22)]
        [Mask("d")]
        [Range(-30, 0)]
        public virtual DateTime? ShipDate { get; set; }

        public string DisableShipDate()
        {
            return DisableIfShipped();
        }

        public string ValidateShipDate(DateTime? shipDate) {

            if (shipDate.HasValue && shipDate.Value.Date < OrderDate.Date) {
                return "Ship date cannot be before order date";
            }

            return null;
        }


        #endregion

        #endregion

        #region Amounts

        [Disabled]
        [MemberOrder(31)]
        [Mask("C")]
        public virtual decimal SubTotal { get; set; }

        [Disabled]
        [MemberOrder(32)]
        [Mask("C")]
        public virtual decimal TaxAmt { get; set; }

        [Disabled]
        [MemberOrder(33)]
        [Mask("C")]
        public virtual decimal Freight { get; set; }

        [Disabled]
        [MemberOrder(34)]
        [Mask("C")]
        public virtual decimal TotalDue { get; set; }

        public void Recalculate()
        {
            SubTotal = Details.Sum(d => d.LineTotal);
            TotalDue = SubTotal;
            //TODO:  Calculate Tax, (Freight?)
        }

        #region CurrencyRate

        [Optionally]
        [MemberOrder(35)]
        public virtual CurrencyRate CurrencyRate { get; set; }

        #endregion

        #endregion

        #region OnlineOrder

        [Description("Order has been placed via the web")]
        [Disabled]
        [MemberOrder(41)]
        [DisplayName("Online Order")]
        public virtual bool OnlineOrder { get; set; }

        #endregion

        #region CreditCard

        [Optionally]
        [MemberOrder(42)]
        public virtual CreditCard CreditCard { get; set; }

        #endregion

        #region CreditCardApprovalCode

        [Disabled]
        [StringLength(15)]
        [MemberOrder(43)]
        public virtual string CreditCardApprovalCode { get; set; }

        #endregion

        #region RevisionNumber

        [Disabled]
        [MemberOrder(51)]
        public virtual byte RevisionNumber { get; set; }

        #endregion

        #region Comment

        [Optionally]
        [MultiLine(NumberOfLines = 3, Width = 50)]
        [MemberOrder(52)]
        public virtual string Comment { get; set; }

        #endregion

        #region SalesPerson

        [Optionally]
        [MemberOrder(61)]
        public virtual SalesPerson SalesPerson { get; set; }

        #endregion

        #region SalesTerritory

        [Optionally]
        [MemberOrder(62)]
        public virtual SalesTerritory SalesTerritory { get; set; }

        #endregion

        #region ModifiedDate and rowguid

        #region ModifiedDate

        [MemberOrder(99)]
        [Disabled]
        [ConcurrencyCheck]
        public override DateTime ModifiedDate { get; set; }

        #endregion

        #region rowguid

        [Hidden]
        public override Guid rowguid { get; set; }

        #endregion

        #endregion

        #endregion

        #region Collections

        #region Details

        private ICollection<SalesOrderDetail> details = new List<SalesOrderDetail>();

        [Disabled]
        public virtual ICollection<SalesOrderDetail> Details
        {
            get { return details; }
            set { details = value; }
        }

        #endregion

        #region Reasons

        private ICollection<SalesOrderHeaderSalesReason> salesOrderHeaderSalesReason = new List<SalesOrderHeaderSalesReason>();

        [Disabled]
        [DisplayName("Reasons")]
        public virtual ICollection<SalesOrderHeaderSalesReason> SalesOrderHeaderSalesReason
        {
            get { return salesOrderHeaderSalesReason; }
            set { salesOrderHeaderSalesReason = value; }
        }

        #endregion

        #endregion

        #region Actions

        #region CreateNewOrderDetail

        [Description("Add a new line item to the order")]
        [MemberOrder(1)]
        public SalesOrderDetail AddNewDetail(Product product,
                            [DefaultValue((short)1), Range(1, 999)] short quantity)
        {
            int stock = product.NumberInStock();
            if (stock < quantity)
            {
                var t = new TitleBuilder();
                t.Append("Current inventory of").Append(product).Append(" is").Append(stock);
                Container.WarnUser(t.ToString());
            }
            var sod = Container.NewTransientInstance<SalesOrderDetail>();
            sod.SalesOrderHeader = this;
            sod.SalesOrderID = SalesOrderID;
            sod.OrderQty = quantity;
            sod.SpecialOfferProduct = product.BestSpecialOfferProduct(quantity);
            sod.Recalculate();

            return sod;
        }

        public virtual string DisableAddNewDetail()
        {
            if (!IsInProcess())
            {
                return "Can only add to 'In Process' order";
            }
            return null;
        }

        #endregion

        #region CreateNewCreditCard

        [Hidden]
        public void CreatedCardHasBeenSaved(CreditCard card)
        {
            CreditCard = card;
        }

        public CreditCard CreateNewCreditCard()
        {
            var newCard = Container.NewTransientInstance<CreditCard>();
            newCard.ForContact = Contact;
            newCard.Creator = this;
            return newCard;
        }

        #endregion

        #region AddNewSalesReason

        [MemberOrder(1)]
        public void AddNewSalesReason(SalesReason reason)
        {
            if (SalesOrderHeaderSalesReason.Where(y => y.SalesReason == reason).Count() == 0)
            {
                var link = Container.NewTransientInstance<SalesOrderHeaderSalesReason>();
                link.SalesOrderHeader = this;
                link.SalesReason = reason;
                Container.Persist(ref link);
                SalesOrderHeaderSalesReason.Add(link);

            }
            else
            {
                Container.WarnUser(string.Format("{0} already exists in Sales Reasons", reason.Name));
            }
        }

        // Use 'hide', 'dis', 'val', 'actdef', 'actcho' shortcuts to add supporting methods here.

        #endregion

        #region ApproveOrder

        [MemberOrder(1)]
        public void ApproveOrder()
        {
            Status = (byte) OrderStatus.Approved;
        }

        public virtual bool HideApproveOrder()
        {
            return !IsInProcess();
        }

        public virtual string DisableApproveOrder()
        {
            var rb = new ReasonBuilder();
            if (Details.Count == 0)
            {
                rb.Append("Cannot approve orders with no Details");
            }
            return rb.Reason;
        }

        #endregion

        #region MarkAsShipped

        [Description("Indicate that the order has been shipped, specifying the date")]
        [Hidden]
        public void MarkAsShipped(DateTime shipDate)
        {
            Status = (byte) OrderStatus.Shipped;
            ShipDate = shipDate;
        }

        public virtual string ValidateMarkAsShipped(DateTime date)
        {
            var rb = new ReasonBuilder();
            if (date.Date > DateTime.Now.Date)
            {
                rb.Append("Ship Date cannot be after Today");
            }
            return rb.Reason;
        }

        public virtual string DisableMarkAsShipped()
        {
            if (!IsApproved())
            {
                return "Order must be Approved before shipping";
            }
            return null;
        }

        public virtual bool HideMarkAsShipped(DateTime shipDate)
        {
            return IsRejected() || IsShipped() || IsCancelled();
        }

        #endregion

        #region CancelOrder

        public void CancelOrder()
        {
            Status = (byte) OrderStatus.Cancelled;
        }

        public virtual bool HideCancelOrder()
        {
            return IsShipped() || IsCancelled();
        }

        #endregion

        #endregion

        [Disabled, NotPersisted, Hidden(WhenTo.OncePersisted)]
        public bool AddItemsFromCart { get; set; }
    }

    public enum OrderStatus : byte {InProcess=1, Approved=2, BackOrdered=3, Rejected=4, Shipped=5,  Cancelled=6 }
}