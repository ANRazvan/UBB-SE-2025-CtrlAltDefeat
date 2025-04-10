﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ArtAttack.Domain;
using ArtAttack.Model;
using ArtAttack.Shared;
using QuestPDF.Fluent;
using QuestPDF.Helpers;

namespace ArtAttack.ViewModel
{
    public class ContractRenewViewModel : IContractRenewViewModel
    {
        private readonly IContractModel contractModel;
        private readonly IContractRenewalModel renewalModel;
        private readonly INotificationDataAdapter notificationAdapter;
        private readonly IDatabaseProvider databaseProvider;
        private readonly string connectionString;
        private readonly IFileSystem fileSystem;
        private readonly IDateTimeProvider dateTimeProvider;

        public List<IContract> BuyerContracts { get; private set; }
        public IContract SelectedContract { get; private set; } = null!;

        [ExcludeFromCodeCoverage]
        public ContractRenewViewModel(string connectionString)
            : this(
                  new ContractModel(connectionString),
                  new ContractRenewalModel(connectionString),
                  new NotificationDataAdapter(connectionString),
                  new SqlDatabaseProvider(),
                  connectionString,
                  new FileSystemWrapper(),
                  new DateTimeProvider())
        {
        }

        // Constructor with dependency injection for testing
        public ContractRenewViewModel(
            IContractModel contractModel,
            IContractRenewalModel renewalModel,
            INotificationDataAdapter notificationAdapter,
            IDatabaseProvider databaseProvider,
            string connectionString,
            IFileSystem fileSystem,
            IDateTimeProvider dateTimeProvider)
        {
            if (contractModel == null)
            {
                throw new ArgumentNullException(nameof(contractModel));
            }
            if (renewalModel == null)
            {
                throw new ArgumentNullException(nameof(renewalModel));
            }
            if (notificationAdapter == null)
            {
                throw new ArgumentNullException(nameof(notificationAdapter));
            }
            if (databaseProvider == null)
            {
                throw new ArgumentNullException(nameof(databaseProvider));
            }
            if (connectionString == null)
            {
                throw new ArgumentNullException(nameof(connectionString));
            }
            if (fileSystem == null)
            {
                throw new ArgumentNullException(nameof(fileSystem));
            }
            if (dateTimeProvider == null)
            {
                throw new ArgumentNullException(nameof(dateTimeProvider));
            }

            this.contractModel = contractModel;
            this.renewalModel = renewalModel;
            this.notificationAdapter = notificationAdapter;
            this.databaseProvider = databaseProvider;
            this.connectionString = connectionString;
            this.fileSystem = fileSystem;
            this.dateTimeProvider = dateTimeProvider;
            BuyerContracts = new List<IContract>();
        }

        /// <summary>
        /// Loads all contracts for the given buyer and filters them to include only those with status "ACTIVE" or "RENEWED".
        /// </summary>
        /// <param name="buyerID">The ID of the buyer to load contracts for.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task LoadContractsForBuyerAsync(int buyerID)
        {
            // Load all contracts for the buyer
            var allContracts = await contractModel.GetContractsByBuyerAsync(buyerID);

            // Filter the contracts to include only those with status "ACTIVE" or "RENEWED"
            BuyerContracts = allContracts.Where(c => c.ContractStatus == "ACTIVE" || c.ContractStatus == "RENEWED").ToList();
        }

        /// <summary>
        /// Retrieves and sets the selected contract by its ID.
        /// </summary>
        /// <param name="contractID">The ID of the contract to select.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task SelectContractAsync(long contractID)
        {
            SelectedContract = await contractModel.GetContractByIdAsync(contractID);
        }

        /// <summary>
        /// Retrieves the start and end dates of the product associated with a given contract.
        /// </summary>
        public async Task<(DateTime StartDate, DateTime EndDate, double price, string name)?> GetProductDetailsByContractIdAsync(long contractId)
        {
            return await contractModel.GetProductDetailsByContractIdAsync(contractId);
        }

        /// <summary>
        /// Checks whether the current date is within the valid renewal period (between 2 and 7 days before contract end).
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public virtual async Task<bool> IsRenewalPeriodValidAsync()
        {
            if (SelectedContract == null)
            {
                return false;
            }

            var dates = await GetProductDetailsByContractIdAsync(SelectedContract.ContractID);
            if (dates == null)
            {
                return false;
            }

            DateTime oldEndDate = dates.Value.EndDate;
            DateTime currentDate = dateTimeProvider.Now.Date;
            int daysUntilEnd = (oldEndDate - currentDate).Days;

            return daysUntilEnd <= 7 && daysUntilEnd >= 2;
        }

        /// <summary>
        /// Simulates a check to determine if a product is available.
        /// </summary>
        /// <param name="productId">The ID of the product to check.</param>
        /// <returns>True if the product is available; false otherwise.</returns>
        public virtual bool IsProductAvailable(int productId)
        {
            return true;
        }

        /// <summary>
        /// Simulates a check to determine if the seller can approve a renewal based on the renewal count.
        /// </summary>
        /// <param name="renewalCount">The current renewal count of the contract.</param>
        /// <returns>True if the seller can approve the renewal; false otherwise.</returns>
        public virtual bool CanSellerApproveRenewal(int renewalCount)
        {
            return renewalCount < 1;
        }

        /// <summary>
        /// Inserts a PDF file into the database and returns the newly generated PDF ID.
        /// </summary>
        /// <param name="fileBytes">The byte array representing the PDF file.</param>
        /// <returns>The ID of the newly inserted PDF.</returns>
        public virtual async Task<int> InsertPdfAsync(byte[] fileBytes)
        {
            using (IDbConnection connection = databaseProvider.CreateConnection(connectionString))
            using (IDbCommand command = connection.CreateCommand())
            {
                command.CommandText = "INSERT INTO PDF ([file]) OUTPUT INSERTED.ID VALUES (@file)";
                var parameter = command.CreateParameter();
                parameter.ParameterName = "@file";
                parameter.Value = fileBytes;
                command.Parameters.Add(parameter);

                await connection.OpenAsync();
                var result = await command.ExecuteScalarAsync();
                return Convert.ToInt32(result);
            }
        }

        /// <summary>
        /// Checks whether the currently selected contract has already been renewed.
        /// </summary>
        /// <returns>True if the contract has been renewed; false otherwise.</returns>
        public async Task<bool> HasContractBeenRenewedAsync()
        {
            if (SelectedContract == null)
            {
                return false;
            }

            return await renewalModel.HasContractBeenRenewedAsync(SelectedContract.ContractID);
        }

        /// <summary>
        /// Generates a PDF document containing the contract content.
        /// </summary>
        /// <param name="contract">The contract object to include in the PDF.</param>
        /// <param name="content">The content of the contract to include in the PDF.</param>
        /// <returns>The byte array representing the generated PDF.</returns>
        public virtual byte[] GenerateContractPdf(IContract contract, string content)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(50);
                    page.Size(PageSizes.A4);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));
                    page.Content().Text(content);
                });
            });

            return document.GeneratePdf();
        }

        /// <summary>
        /// Submits a request to renew the selected contract if all business rules are satisfied.
        /// Also generates and saves a new PDF and sends notifications.
        /// </summary>
        /// <param name="newEndDate">The new end date for the contract.</param>
        /// <param name="buyerID">The ID of the buyer submitting the renewal request.</param>
        /// <param name="productID">The ID of the product associated with the contract.</param>
        /// <param name="sellerID">The ID of the seller associated with the contract.</param>
        /// <returns>A tuple containing a boolean indicating success and a message describing the result.</returns>
        public virtual async Task<(bool Success, string Message)> SubmitRenewalRequestAsync(DateTime newEndDate, int buyerID, int productID, int sellerID)
        {
            try
            {
                // Ensure a contract is selected before proceeding
                if (SelectedContract == null)
                {
                    return (false, "No contract selected.");
                }

                // Check if the contract was already renewed
                if (await HasContractBeenRenewedAsync())
                {
                    return (false, "This contract has already been renewed.");
                }

                // Validate the current date is within the renewal window (2 to 7 days before end)
                if (!await IsRenewalPeriodValidAsync())
                {
                    return (false, "Contract is not in a valid renewal period (between 2 and 7 days before end date).");
                }

                // Get the current contract's product dates
                var oldDates = await GetProductDetailsByContractIdAsync(SelectedContract.ContractID);
                if (!oldDates.HasValue)
                {
                    return (false, "Could not retrieve current contract dates.");
                }

                // Ensure the new end date is after the old one
                if (newEndDate <= oldDates.Value.EndDate)
                {
                    return (false, "New end date must be after the current end date.");
                }

                // Check if product is available for renewal
                if (!IsProductAvailable(productID))
                {
                    return (false, "Product is not available.");
                }

                // Check if seller allows renewal (based on renewal count)
                if (!CanSellerApproveRenewal(SelectedContract.RenewalCount))
                {
                    return (false, "Renewal not allowed: seller limit exceeded.");
                }

                // Build the updated contract content text
                string contractContent = $"Renewed Contract for Order {SelectedContract.OrderID}.\nOriginal Contract ID: {SelectedContract.ContractID}.\nNew End Date: {newEndDate:dd/MM/yyyy}";

                // Generate the contract PDF
                byte[] pdfBytes = GenerateContractPdf(SelectedContract, contractContent);

                // Insert the new PDF into the database and get its ID
                int newPdfId = await InsertPdfAsync(pdfBytes);

                // Save PDF locally in Downloads folder
                string downloadsPath = fileSystem.GetDownloadsPath();
                string fileName = $"RenewedContract_{SelectedContract.ContractID}_to_{newEndDate:yyyyMMdd}.pdf";
                string filePath = fileSystem.CombinePath(downloadsPath, fileName);
                await fileSystem.WriteAllBytesAsync(filePath, pdfBytes);

                // Prepare and insert the new renewed contract into the database
                var updatedContract = new Contract
                {
                    OrderID = SelectedContract.OrderID,
                    ContractStatus = "RENEWED",
                    ContractContent = contractContent,
                    RenewalCount = SelectedContract.RenewalCount + 1,
                    PredefinedContractID = SelectedContract.PredefinedContractID,
                    PDFID = newPdfId,
                    RenewedFromContractID = SelectedContract.ContractID
                };

                await renewalModel.AddRenewedContractAsync(updatedContract, pdfBytes);

                // Send notifications to seller, buyer, and waitlist
                var now = dateTimeProvider.Now;
                notificationAdapter.AddNotification(new ContractRenewalRequestNotification(sellerID, now, (int)SelectedContract.ContractID));
                notificationAdapter.AddNotification(new ContractRenewalAnswerNotification(buyerID, now, (int)SelectedContract.ContractID, true));
                notificationAdapter.AddNotification(new ContractRenewalWaitlistNotification(999, now, productID));

                return (true, "Contract renewed successfully!");
            }
            catch (Exception ex)
            {
                // Catch any unexpected errors and return an appropriate message
                return (false, $"Unexpected error: {ex.Message}");
            }
        }
    }

    // Interface for file system operations to enable testing
    public interface IFileSystem
    {
        string GetDownloadsPath();
        string CombinePath(string path1, string path2);
        Task WriteAllBytesAsync(string path, byte[] bytes);
    }

    // Concrete implementation of IFileSystem that uses actual file system
    public class FileSystemWrapper : IFileSystem
    {
        public string GetDownloadsPath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        }

        public string CombinePath(string path1, string path2)
        {
            return Path.Combine(path1, path2);
        }

        public async Task WriteAllBytesAsync(string path, byte[] bytes)
        {
            await File.WriteAllBytesAsync(path, bytes);
        }
    }

    // Interface for DateTime operations to enable testing
    public interface IDateTimeProvider
    {
        DateTime Now { get; }
    }

    // Concrete implementation of IDateTimeProvider that uses actual DateTime
    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTime Now => DateTime.Now;
    }
}
