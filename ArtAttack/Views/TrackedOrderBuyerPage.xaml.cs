using System.Diagnostics.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArtAttack.Domain;
using ArtAttack.ViewModel;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
namespace ArtAttack.Views
{
    [ExcludeFromCodeCoverage]
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TrackedOrderBuyerPage : Page
    {
        internal ITrackedOrderViewModel ViewModel { get; set; }
        public int TrackedOrderID { get; set; }
        internal List<OrderCheckpoint> Checkpoints { get; set; }

        internal TrackedOrderBuyerPage(ITrackedOrderViewModel viewModel, int trackedOrderID)
        {
            this.InitializeComponent();
            ViewModel = viewModel;
            TrackedOrderID = trackedOrderID;
            Checkpoints = new List<OrderCheckpoint>();
            LoadOrderData();
        }

        /// <summary>
        /// Shows an error dialog with the specified error message
        /// </summary>
        /// <param name="errorMessage">The message of the error to be shown</param>
        /// <returns></returns>
        private async Task ShowErrorDialog(string errorMessage)
        {
            var dialog = new ContentDialog
            {
                Title = "Error",
                Content = errorMessage,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };

            await dialog.ShowAsync();
        }

        /// <summary>
        /// Loads the order data asynchronously
        /// </summary>
        private async void LoadOrderData()
        {
            try
            {
                var order = await ViewModel.GetTrackedOrderByIDAsync(TrackedOrderID);
                var checkpoints = await ViewModel.GetAllOrderCheckpointsAsync(TrackedOrderID);
                checkpoints.Reverse();

                DataContext = new
                {
                    CurrentStatus = order.CurrentStatus,
                    TrackedOrderID = order.TrackedOrderID,
                    DeliveryAddress = order.DeliveryAddress,
                    EstimatedDeliveryDate = order.EstimatedDeliveryDate,
                    OrderID = order.OrderID,
                    Checkpoints = checkpoints
                };
            }
            catch (Exception ex)
            {
                await ShowErrorDialog(ex.Message);
            }
        }
    }
}
