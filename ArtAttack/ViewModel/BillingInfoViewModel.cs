﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using ArtAttack.Domain;
using ArtAttack.Model;
using ArtAttack.Shared;
using ArtAttack.Utils;

namespace ArtAttack.ViewModel
{
    /// <summary>
    /// Represents the view model for billing information and processes order history and payment details.
    /// </summary>
    public class BillingInfoViewModel : IBillingInfoViewModel, INotifyPropertyChanged
    {
        private readonly IOrderHistoryModel orderHistoryModel;
        private readonly IOrderSummaryModel orderSummaryModel;
        private readonly IOrderModel orderModel;
        private readonly IDummyProductModel dummyProductModel;
        private readonly IDummyWalletModel dummyWalletModel;

        private int orderHistoryID;

        private bool isWalletEnabled;
        private bool isCashEnabled;
        private bool isCardEnabled;

        private string selectedPaymentMethod;

        private string fullName;
        private string email;
        private string phoneNumber;
        private string address;
        private string zipCode;
        private string additionalInfo;

        private DateTime startDate;
        private DateTime endDate;

        private float subtotal;
        private float deliveryFee;
        private float total;
        private float warrantyTax;

        public ObservableCollection<DummyProduct> ProductList { get; set; }
        public List<DummyProduct> DummyProducts;

        /// <summary>
        /// Initializes a new instance of the <see cref="BillingInfoViewModel"/> class and begins loading order history details.
        /// </summary>
        /// <param name="orderHistoryID">The unique identifier for the order history.</param>
        public BillingInfoViewModel(int orderHistoryID)
        {
            orderHistoryModel = new OrderHistoryModel(Configuration.CONNECTION_STRING);
            orderModel = new OrderModel(Configuration.CONNECTION_STRING);
            orderSummaryModel = new OrderSummaryModel(Configuration.CONNECTION_STRING);
            dummyWalletModel = new DummyWalletModel(Configuration.CONNECTION_STRING);
            dummyProductModel = new DummyProductModel(Configuration.CONNECTION_STRING);
            DummyProducts = new List<DummyProduct>();
            this.orderHistoryID = orderHistoryID;

            _ = InitializeViewModelAsync();

            warrantyTax = 0;
        }

        /// <summary>
        /// Asynchronously initializes the view model by loading dummy products, setting up the product list, and calculating order totals.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task InitializeViewModelAsync()
        {
            DummyProducts = await GetDummyProductsFromOrderHistoryAsync(orderHistoryID);
            ProductList = new ObservableCollection<DummyProduct>(DummyProducts);

            OnPropertyChanged(nameof(ProductList));

            SetVisibilityRadioButtons();

            CalculateOrderTotal(orderHistoryID);
        }

        /// <summary>
        /// Sets the visibility of payment method radio buttons based on the first product's type.
        /// </summary>
        public void SetVisibilityRadioButtons()
        {
            if (ProductList.Count > 0)
            {
                string firstProductType = ProductList[0].ProductType;

                if (firstProductType == "new" || firstProductType == "used" || firstProductType == "borrowed")
                {
                    IsCardEnabled = true;
                    IsCashEnabled = true;
                    IsWalletEnabled = false;
                }
                else if (firstProductType == "bid")
                {
                    IsCardEnabled = false;
                    IsCashEnabled = false;
                    IsWalletEnabled = true;
                }
                else if (firstProductType == "refill")
                {
                    IsCardEnabled = true;
                    IsCashEnabled = false;
                    IsWalletEnabled = false;
                }
            }
        }
        [ExcludeFromCodeCoverage]
        /// <summary>
        /// Handles the finalization button click event, updating orders and order summary, then opens the next window.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task OnFinalizeButtonClickedAsync()
        {
            string paymentMethod = SelectedPaymentMethod;

            // This is subject to change, as the orderModel is to be switched to asynchronous architecture
            List<Order> orderList = await orderModel.GetOrdersFromOrderHistoryAsync(orderHistoryID);

            foreach (var order in orderList)
            {
                await orderModel.UpdateOrderAsync(order.OrderID, order.ProductType, SelectedPaymentMethod, DateTime.Now);
            }

            // Currently, an order summary has the same ID as the order history for simplicity
            await orderSummaryModel.UpdateOrderSummaryAsync(orderHistoryID, Subtotal, warrantyTax, DeliveryFee, Total, FullName, Email, PhoneNumber, Address, ZipCode, AdditionalInfo, null);

            await OpenNextWindowAsync(SelectedPaymentMethod);
        }
        [ExcludeFromCodeCoverage]
        /// <summary>
        /// Opens the next window based on the selected payment method.
        /// </summary>
        /// <param name="selectedPaymentMethod">The selected payment method (e.g. "card", "wallet").</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task OpenNextWindowAsync(string selectedPaymentMethod)
        {
            if (selectedPaymentMethod == "card")
            {
                var billingInfoWindow = new BillingInfoWindow();
                var cardInfoPage = new CardInfo(orderHistoryID);
                billingInfoWindow.Content = cardInfoPage;

                billingInfoWindow.Activate();

                // This is just a workaround until I figure out how to switch between pages
            }
            else
            {
                if (selectedPaymentMethod == "wallet")
                {
                    await ProcessWalletRefillAsync();
                }
                var billingInfoWindow = new BillingInfoWindow();
                var finalisePurchasePage = new FinalisePurchase(orderHistoryID);
                billingInfoWindow.Content = finalisePurchasePage;

                billingInfoWindow.Activate();
            }
        }

        /// <summary>
        /// Processes the wallet refill by deducting the order total from the current wallet balance.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task ProcessWalletRefillAsync()
        {
            float walletBalance = await dummyWalletModel.GetWalletBalanceAsync(1);

            float newBalance = walletBalance - Total;

            await dummyWalletModel.UpdateWalletBalance(1, newBalance);
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event for the specified property.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Calculates the total order amount including applicable delivery fees.
        /// </summary>
        /// <param name="orderHistoryID">The order history identifier used for calculation.</param>
        public void CalculateOrderTotal(int orderHistoryID)
        {
            float subtotalProducts = 0;
            if (DummyProducts.Count == 0)
            {
                return;
            }
            foreach (var product in DummyProducts)
            {
                subtotalProducts += product.Price;
            }

            // For orders over 200 RON, a fixed delivery fee of 13.99 will be added
            // (this is only for orders of new, used or borrowed products)
            Subtotal = subtotalProducts;
            if (subtotalProducts >= 200 || DummyProducts[0].ProductType == "refill" || DummyProducts[0].ProductType == "bid")
            {
                Total = subtotalProducts;
            }
            else
            {
                Total = subtotalProducts + 13.99f;
                DeliveryFee = 13.99f;
            }
        }

        /// <summary>
        /// Asynchronously retrieves dummy products associated with the specified order history.
        /// </summary>
        /// <param name="orderHistoryID">The unique identifier for the order history.</param>
        /// <returns>A task representing the asynchronous operation that returns a list of <see cref="DummyProduct"/>.</returns>
        public async Task<List<DummyProduct>> GetDummyProductsFromOrderHistoryAsync(int orderHistoryID)
        {
            return await orderHistoryModel.GetDummyProductsFromOrderHistoryAsync(orderHistoryID);
        }

        /// <summary>
        /// Applies the borrowed tax to the specified dummy product if applicable.
        /// </summary>
        /// <param name="dummyProduct">The dummy product on which to apply the borrowed tax.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task ApplyBorrowedTax(DummyProduct dummyProduct)
        {
            if (dummyProduct == null || dummyProduct.ProductType != "borrowed")
            {
                return;
            }
            if (StartDate > EndDate)
            {
                return;
            }
            int monthsBorrowed = ((EndDate.Year - StartDate.Year) * 12) + EndDate.Month - StartDate.Month;
            if (monthsBorrowed <= 0)
            {
                monthsBorrowed = 1;
            }

            float warrantyTaxAmount = 0.2f;

            float finalPrice = dummyProduct.Price * monthsBorrowed;

            warrantyTax += finalPrice * warrantyTaxAmount;

            WarrantyTax = warrantyTax;

            dummyProduct.Price = finalPrice + WarrantyTax;

            CalculateOrderTotal(orderHistoryID);

            DateTime newStartDate = startDate.Date;
            DateTime newEndDate = endDate.Date;

            dummyProduct.StartDate = newStartDate;
            dummyProduct.EndDate = newEndDate;

            if (dummyProduct.SellerID == null)
            {
                dummyProduct.SellerID = 0;
            }

            await dummyProductModel.UpdateDummyProductAsync(dummyProduct.ID, dummyProduct.Name, dummyProduct.Price, (int)dummyProduct.SellerID, dummyProduct.ProductType, newStartDate, newEndDate);
        }
        [ExcludeFromCodeCoverage]
        /// <summary>
        /// Updates the start date for the product's rental period.
        /// </summary>
        /// <param name="date">The new start date as a <see cref="DateTimeOffset"/>.</param>
        public void UpdateStartDate(DateTimeOffset date)
        {
            startDate = date.DateTime;
            StartDate = date.DateTime;
        }
        [ExcludeFromCodeCoverage]

        /// <summary>
        /// Updates the end date for the product's rental period.
        /// </summary>
        /// <param name="date">The new end date as a <see cref="DateTimeOffset"/>.</param>
        public void UpdateEndDate(DateTimeOffset date)
        {
            endDate = date.DateTime;
            EndDate = date.DateTime;
        }

        [ExcludeFromCodeCoverage]
        public string SelectedPaymentMethod
        {
            get => selectedPaymentMethod;
            set
            {
                selectedPaymentMethod = value;
                OnPropertyChanged(nameof(SelectedPaymentMethod));
            }
        }

        [ExcludeFromCodeCoverage]
        public string FullName
        {
            get => fullName;
            set
            {
                fullName = value;
                OnPropertyChanged(nameof(FullName));
            }
        }

        [ExcludeFromCodeCoverage]
        public string Email
        {
            get => email;
            set
            {
                email = value;
                OnPropertyChanged(nameof(Email));
            }
        }

        [ExcludeFromCodeCoverage]
        public string PhoneNumber
        {
            get => phoneNumber;
            set
            {
                phoneNumber = value;
                OnPropertyChanged(nameof(PhoneNumber));
            }
        }

        [ExcludeFromCodeCoverage]
        public string Address
        {
            get => address;
            set
            {
                address = value;
                OnPropertyChanged(nameof(Address));
            }
        }
        [ExcludeFromCodeCoverage]
        public string ZipCode
        {
            get => zipCode;
            set
            {
                zipCode = value;
                OnPropertyChanged(nameof(ZipCode));
            }
        }

        [ExcludeFromCodeCoverage]
        public string AdditionalInfo
        {
            get => additionalInfo;
            set
            {
                additionalInfo = value;
                OnPropertyChanged(nameof(AdditionalInfo));
            }
        }
        [ExcludeFromCodeCoverage]
        public bool IsWalletEnabled
        {
            get => isWalletEnabled;
            set
            {
                isWalletEnabled = value;
                OnPropertyChanged(nameof(IsWalletEnabled));
            }
        }

        [ExcludeFromCodeCoverage]
        public bool IsCashEnabled
        {
            get => isCashEnabled;
            set
            {
                isCashEnabled = value;
                OnPropertyChanged(nameof(IsCashEnabled));
            }
        }

        [ExcludeFromCodeCoverage]
        public bool IsCardEnabled
        {
            get => isCardEnabled;
            set
            {
                isCardEnabled = value;
                OnPropertyChanged(nameof(IsCardEnabled));
            }
        }

        [ExcludeFromCodeCoverage]
        public float Subtotal
        {
            get => subtotal;
            set
            {
                subtotal = value;
                OnPropertyChanged(nameof(Subtotal));
            }
        }
        [ExcludeFromCodeCoverage]
        public float DeliveryFee
        {
            get => deliveryFee;
            set
            {
                deliveryFee = value;
                OnPropertyChanged(nameof(DeliveryFee));
            }
        }
        [ExcludeFromCodeCoverage]
        public float Total
        {
            get => total;
            set
            {
                total = value;
                OnPropertyChanged(nameof(Total));
            }
        }
        [ExcludeFromCodeCoverage]
        public float WarrantyTax
        {
            get => warrantyTax;
            set
            {
                warrantyTax = value;
                OnPropertyChanged(nameof(warrantyTax));
            }
        }

        [ExcludeFromCodeCoverage]
        public DateTime StartDate
        {
            get => startDate;
            set
            {
                startDate = value;
                OnPropertyChanged(nameof(StartDate));
            }
        }

        [ExcludeFromCodeCoverage]
        public DateTime EndDate
        {
            get => endDate;
            set
            {
                endDate = value;
                OnPropertyChanged(nameof(EndDate));
            }
        }
    }
}
