// Copyright � Naked Objects Group Ltd ( http://www.nakedobjects.net). 
// All Rights Reserved. This code released under the terms of the 
// Microsoft Public License (MS-PL) ( http://opensource.org/licenses/ms-pl.html) 
using System;
using System.Collections.Generic;
using System.Linq;
using NakedObjects;
using NakedObjects.Value;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AdventureWorksModel {
    [IconName("carton.png")]
    public class Product : AWDomainObject {

        #region Injected Services

        #region Injected: SpecialOfferRepository

        public SpecialOfferRepository SpecialOfferRepository { set; protected get; }

        #endregion

        public ShoppingCartRepository ShoppingCartRepository { set; protected get; }


        #endregion

        #region Properties

        #region ProductID

        [Hidden]
        public virtual int ProductID { get; set; }

        #endregion

        #region Name

        [Title]
        [MemberOrder(1)]
        public virtual string Name { get; set; }

        #endregion

        #region ProductNumber

        [MemberOrder(2)]
        public virtual string ProductNumber { get; set; }

        #endregion

        #region Color

        [Optionally]
        [MemberOrder(3)]
        public virtual string Color { get; set; }

        #endregion

        #region Photo

        private Image cachedPhoto;

        [MemberOrder(4)]
        public virtual Image Photo {
            get {
                if (cachedPhoto == null) {
                    ProductPhoto p = (from obj in ProductProductPhoto
                                          select obj.ProductPhoto).FirstOrDefault();

                    if (p != null) {
                        cachedPhoto = new Image(p.LargePhoto, p.LargePhotoFileName);
                    }
                }
                return cachedPhoto;
            }
        }


        public void AddOrChangePhoto(Image newImage)
        {
            ProductPhoto p = (from obj in ProductProductPhoto
                              select obj.ProductPhoto).FirstOrDefault();

            p.LargePhoto = newImage.GetResourceAsByteArray();
            p.LargePhotoFileName = newImage.Name;

        }

        #endregion

        #region Make

        [MemberOrder(20)]
        public virtual bool Make { get; set; }

        #endregion

        #region FinishedGoods

        [MemberOrder(21)]
        public virtual bool FinishedGoods { get; set; }

        #endregion

        #region SafetyStockLevel

        [MemberOrder(22)]
        public virtual short SafetyStockLevel { get; set; }

        #endregion

        #region ReorderPoint

        [MemberOrder(23)]
        public virtual short ReorderPoint { get; set; }

        #endregion

        #region StandardCost

        [MemberOrder(90)]
        [Mask("C")]
        public virtual decimal StandardCost { get; set; }

        #endregion

        #region ListPrice

        [MemberOrder(11)]
        [Mask("C")]
        public virtual decimal ListPrice { get; set; }

        #endregion

        #region Size & Weight

        [Hidden]
        public virtual string Size { get; set; }

        [Hidden]
        public virtual UnitMeasure SizeUnit { get; set; }

        [DisplayName("Size")]
        [MemberOrder(16)]
        public virtual string SizeWithUnit {
            get {
                var t = new TitleBuilder();
                t.Append(Size).Append(SizeUnit);
                return t.ToString();
            }
        }

        [Hidden]
        public virtual decimal? Weight { get; set; }

        [Hidden]
        public virtual UnitMeasure WeightUnit { get; set; }

        [MemberOrder(17)]
        [DisplayName("Weight")]
        public virtual string WeightWithUnit {
            get {
                var t = new TitleBuilder();
                t.Append(Weight).Append(WeightUnit);
                return t.ToString();
            }
        }

        #endregion

        #region DaysToManufacture

        [MemberOrder(24), Range(1,90)]
        public virtual int DaysToManufacture { get; set; }

        #endregion

        #region ProductLine

        [Optionally]
        [MemberOrder(14)]
        public virtual string ProductLine { get; set; }

        public virtual string[] ChoicesProductLine() {
            return new[] { "R", "M", "T", "S" };
        }


        #endregion

        #region Class

        [Optionally]
        [MemberOrder(19)]
        public virtual string Class { get; set; }

        public virtual string[] ChoicesClass() {
            return new[] {"H", "M", "L"};
        }

        #endregion

        #region Style

        [Optionally]
        [MemberOrder(18)]
        public virtual string Style { get; set; }

        public virtual string[] ChoicesStyle() {
            return new[] { "U", "M", "W" };
        }


        #endregion

        #region SellStartDate

        [MemberOrder(81)]
        [Mask("d")]
        public virtual DateTime SellStartDate { get; set; }

        #endregion

        #region SellEndDate

        [MemberOrder(82)]
        [Optionally]
        [Mask("d")]
        public virtual DateTime? SellEndDate { get; set; }

        #endregion

        #region Discontinued

        [Optionally]
        [MemberOrder(83)]
        [Mask("d")]
        public virtual DateTime? DiscontinuedDate { get; set; }

        [Hidden]
        public virtual bool IsDiscontinued() {
            return DiscontinuedDate != null ? DiscontinuedDate.Value < DateTime.Now : false;
        }

        #endregion

        #region ProductModel

        [Optionally]
        [MemberOrder(10)]
        public virtual ProductModel ProductModel { get; set; }

        #endregion

        #region ProductSubcategory

        [Optionally]
        [MemberOrder(12)]
        public virtual ProductSubcategory ProductSubcategory { get; set; }

        #endregion

        #region ModifiedDate and rowguid

        #region ModifiedDate

        [MemberOrder(99)]
        [Disabled]
        public override DateTime ModifiedDate { get; set; }

        #endregion

        #region rowguid

        [Hidden]
        public override Guid rowguid { get; set; }

        #endregion

        #endregion

        #endregion

        #region Collections

        #region Photos

        private ICollection<ProductProductPhoto> _ProductProductPhoto = new List<ProductProductPhoto>();

        [Hidden]
        public virtual ICollection<ProductProductPhoto> ProductProductPhoto {
            get { return _ProductProductPhoto; }
            set { _ProductProductPhoto = value; }
        }

        #endregion

        #region Reviews

        private ICollection<ProductReview> _ProductReviews = new List<ProductReview>();

        public virtual ICollection<ProductReview> ProductReviews {
            get { return _ProductReviews; }
            set { _ProductReviews = value; }
        }

        #endregion

        #region Inventory

        private ICollection<ProductInventory> _ProductInventory = new List<ProductInventory>();

        public virtual ICollection<ProductInventory> ProductInventory {
            get { return _ProductInventory; }
            set { _ProductInventory = value; }
        }

        [Hidden]
        public virtual int NumberInStock() {
            return (from obj in ProductInventory
                    select obj).Sum(obj => obj.Quantity);
        }

        #endregion

        #region Special Offers

        private ICollection<SpecialOfferProduct> _SpecialOfferProduct = new List<SpecialOfferProduct>();

        [Hidden]
        public virtual ICollection<SpecialOfferProduct> SpecialOfferProduct {
            get { return _SpecialOfferProduct; }
            set { _SpecialOfferProduct = value; }
        }

        public IList<SpecialOffer> SpecialOffers {
            get { return SpecialOfferProduct.Select(n => n.SpecialOffer).Where(so => so != null).ToList(); }
        }

        #endregion

        #endregion

        #region BestSpecialOffer

        [Description("Determines the best discount offered by current special offers for a specified order quantity")]
        public virtual SpecialOffer BestSpecialOffer(short quantity) {
            return BestSpecialOfferProduct(quantity).SpecialOffer;
        }

        public virtual string ValidateBestSpecialOffer(short quantity) {
            return quantity <= 0 ? "Quantity must be > 0" : null;
        }

        public virtual string DisableBestSpecialOffer() {
            if (IsDiscontinued()) {
                return "Product is discontinued";
            }
            return null;
        }

        [Hidden]
        public virtual SpecialOfferProduct BestSpecialOfferProduct(short quantity) {
            //TODO:  reason for testing end date against 1/6/2004 is that in AW database, all offers terminate by 30/6/04
            var query = from obj in Container.Instances<SpecialOfferProduct>()
                                                           where obj.Product.ProductID == ProductID &&
                                                                 obj.SpecialOffer.StartDate <= DateTime.Now &&
                                                                 obj.SpecialOffer.EndDate >= new DateTime(2004, 6, 1) &&
                                                                 obj.SpecialOffer.MinQty < quantity
                                                           orderby obj.SpecialOffer.DiscountPct descending
                                                           select obj;

            SpecialOfferProduct best = query.FirstOrDefault();
            if (best != null) {
                return best;
            }
            else {
                 
                SpecialOffer none = SpecialOfferRepository.NoDiscount();
                return SpecialOfferRepository.AssociateSpecialOfferWithProduct(none, this);
        
            }
        }

        #endregion


    }
}