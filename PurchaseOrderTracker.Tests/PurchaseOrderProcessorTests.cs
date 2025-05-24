using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using PurchaseOrderTracker; // Assuming this is the namespace of the main project

// These are definitions that would normally be in the main project.
// Making them available here to make the file "compilable" in isolation for the tool.
namespace PurchaseOrderTracker
{
    // PurchaseOrder is already defined in DbServiceTests.cs context, but for full standalone, it's repeated.
    // public class PurchaseOrder { ... } 

    public class FileReadResult<T>
    {
        public List<T> SuccessfullRecords { get; set; } = new List<T>();
        public List<string> UnprocessableRecords { get; set; } = new List<string>();
    }

    // Making FileReader mockable for tests.
    // Original is static. This would require refactoring PurchaseOrderProcessor
    // to use IFileReader or a Func<string, Task<FileReadResult<PurchaseOrder>>>.
    // For this test, we'll assume PurchaseOrderProcessor can use a mocked version.
    public interface IFileReader
    {
        Task<FileReadResult<PurchaseOrder>> ReadFile(string path);
    }

    // Making ValidationService mockable.
    // Original is static. This would require refactoring PurchaseOrderProcessor.
    public interface IValidationService
    {
        Task<(bool IsBuyerIdValid, bool IsProductCodeValid, bool IsQuantityValid)> IsRecordValid(PurchaseOrder record);
    }
    
    public class PurchaseOrderProcessResult
    {
        public List<PurchaseOrder> ValidPurchaseOrders { get; set; } = new List<PurchaseOrder>();
        public List<PurchaseOrder> InValidPurchaseOrders { get; set; } = new List<PurchaseOrder>();
        public List<string> UnprocessablePurchaseOrders { get; set; } = new List<string>();
    }

    // Dummy PurchaseOrderProcessor structure for compilation context.
    // The actual PurchaseOrderProcessor is in the main project.
    // This version would be modified to accept IFileReader and IValidationService.
    public static class PurchaseOrderProcessor
    {
        // This is a conceptual refactor for testability.
        public static async Task<PurchaseOrderProcessResult> ProcessPurchaseOrders(
            IFileReader fileReader, // Injected dependency
            IValidationService validationService, // Injected dependency
            string filePath)
        {
            var records = await fileReader.ReadFile(filePath);
            var invalidRecords = new List<PurchaseOrder>();
            var validRecords = new List<PurchaseOrder>();
            var processedProductCodes = new HashSet<string>();

            foreach (var record in records.SuccessfullRecords)
            {
                if (processedProductCodes.Contains(record.ProductCode))
                {
                    invalidRecords.Add(record); 
                }
                else
                {
                    var validation = await validationService.IsRecordValid(record);
                    if (validation.IsBuyerIdValid && validation.IsProductCodeValid && validation.IsQuantityValid)
                    {
                        validRecords.Add(record);
                        processedProductCodes.Add(record.ProductCode);
                    }
                    else
                    {
                        invalidRecords.Add(record);
                    }
                }
            }
            return new PurchaseOrderProcessResult()
            {
                ValidPurchaseOrders = validRecords,
                InValidPurchaseOrders = invalidRecords,
                UnprocessablePurchaseOrders = records.UnprocessableRecords
            };
        }

        // Original static method, which would now call the testable one.
        // This part is trickier. For tests, we'd call the overload above directly.
        // The actual application might call this original signature.
        // One way is to have a static, settable field for IFileReader and IValidationService instances.
        public static async Task<PurchaseOrderProcessResult> ProcessPurchaseOrders()
        {
             // This would use default production services.
             // For testing, this method itself is hard to unit test without shims or further refactoring.
             // The tests below will target the refactored version.
             throw new NotImplementedException("This static method would need refactoring or use of a DI container to be testable for its own dependencies. Tests should target the overload with injected dependencies.");
        }
    }
}
// End of dummy definitions for self-contained compilation.


[TestClass]
public class PurchaseOrderProcessorTests
{
    private Mock<PurchaseOrderTracker.IFileReader> _mockFileReader;
    private Mock<PurchaseOrderTracker.IValidationService> _mockValidationService;

    [TestInitialize]
    public void TestInitialize()
    {
        _mockFileReader = new Mock<PurchaseOrderTracker.IFileReader>();
        _mockValidationService = new Mock<PurchaseOrderTracker.IValidationService>();

        // Default setup for FilePaths.PurchaseOrdersPath if needed by the processor
        // However, the path is passed to ProcessPurchaseOrders in the refactored version.
        PurchaseOrderTracker.FilePaths.PurchaseOrdersPath = "dummy_po_path.csv";
    }

    private PurchaseOrderTracker.PurchaseOrder CreatePO(string productCode, string buyerId = "B1", int quantity = 1, string orderDate = "2023-01-01")
    {
        return new PurchaseOrderTracker.PurchaseOrder { ProductCode = productCode, BuyerId = buyerId, Quantity = quantity, OrderDate = orderDate };
    }

    [TestMethod]
    public async Task ProcessPurchaseOrders_WithDuplicateProductCodes_ShouldMarkDuplicatesAsInvalid()
    {
        // Arrange
        var po1 = CreatePO("P1"); // Valid
        var po2 = CreatePO("P2"); // Valid
        var po3 = CreatePO("P1", buyerId: "B2"); // Duplicate ProductCode P1

        var successfulRecords = new List<PurchaseOrderTracker.PurchaseOrder> { po1, po2, po3 };
        var fileReadResult = new PurchaseOrderTracker.FileReadResult<PurchaseOrderTracker.PurchaseOrder> { SuccessfullRecords = successfulRecords };

        _mockFileReader.Setup(fr => fr.ReadFile(It.IsAny<string>())).ReturnsAsync(fileReadResult);
        _mockValidationService.Setup(vs => vs.IsRecordValid(It.IsAny<PurchaseOrderTracker.PurchaseOrder>()))
                              .ReturnsAsync((true, true, true)); // All records are otherwise valid

        // Act
        // Calling the conceptual refactored method
        var result = await PurchaseOrderTracker.PurchaseOrderProcessor.ProcessPurchaseOrders(
            _mockFileReader.Object, _mockValidationService.Object, "any_path.csv");

        // Assert
        Assert.AreEqual(2, result.ValidPurchaseOrders.Count, "Should have 2 valid orders.");
        Assert.IsTrue(result.ValidPurchaseOrders.Contains(po1), "PO1 should be valid.");
        Assert.IsTrue(result.ValidPurchaseOrders.Contains(po2), "PO2 should be valid.");
        Assert.AreEqual(1, result.InValidPurchaseOrders.Count, "Should have 1 invalid order due to duplication.");
        Assert.IsTrue(result.InValidPurchaseOrders.Contains(po3), "PO3 should be invalid as duplicate.");
    }

    [TestMethod]
    public async Task ProcessPurchaseOrders_WithMixedValidInvalidAndDuplicateRecords_ShouldCategorizeCorrectly()
    {
        // Arrange
        var po1 = CreatePO("P10"); // Valid
        var po2 = CreatePO("P20", buyerId: "INVALID_BUYER"); // Invalid by ValidationService (BuyerId)
        var po3 = CreatePO("P10", buyerId: "B2"); // Duplicate of P10 (otherwise valid)
        var po4 = CreatePO("P30", quantity: 0); // Invalid by ValidationService (Quantity)
        var po5 = CreatePO("P40"); // Valid

        var successfulRecords = new List<PurchaseOrderTracker.PurchaseOrder> { po1, po2, po3, po4, po5 };
        var fileReadResult = new PurchaseOrderTracker.FileReadResult<PurchaseOrderTracker.PurchaseOrder> { SuccessfullRecords = successfulRecords };
        _mockFileReader.Setup(fr => fr.ReadFile(It.IsAny<string>())).ReturnsAsync(fileReadResult);

        // Setup ValidationService mock
        _mockValidationService.Setup(vs => vs.IsRecordValid(po1)).ReturnsAsync((true, true, true));
        _mockValidationService.Setup(vs => vs.IsRecordValid(po2)).ReturnsAsync((false, true, true)); // Invalid BuyerId
        _mockValidationService.Setup(vs => vs.IsRecordValid(po3)).ReturnsAsync((true, true, true)); // This one is a duplicate
        _mockValidationService.Setup(vs => vs.IsRecordValid(po4)).ReturnsAsync((true, true, false)); // Invalid Quantity
        _mockValidationService.Setup(vs => vs.IsRecordValid(po5)).ReturnsAsync((true, true, true));

        // Act
        var result = await PurchaseOrderTracker.PurchaseOrderProcessor.ProcessPurchaseOrders(
            _mockFileReader.Object, _mockValidationService.Object, "any_path.csv");

        // Assert
        Assert.AreEqual(2, result.ValidPurchaseOrders.Count, "Incorrect number of valid orders.");
        Assert.IsTrue(result.ValidPurchaseOrders.Contains(po1), "PO1 should be valid.");
        Assert.IsTrue(result.ValidPurchaseOrders.Contains(po5), "PO5 should be valid.");

        Assert.AreEqual(3, result.InValidPurchaseOrders.Count, "Incorrect number of invalid orders.");
        Assert.IsTrue(result.InValidPurchaseOrders.Contains(po2), "PO2 (invalid buyer) should be in invalid orders.");
        Assert.IsTrue(result.InValidPurchaseOrders.Contains(po3), "PO3 (duplicate) should be in invalid orders.");
        Assert.IsTrue(result.InValidPurchaseOrders.Contains(po4), "PO4 (invalid quantity) should be in invalid orders.");
    }

    [TestMethod]
    public async Task ProcessPurchaseOrders_WithUnprocessableRecords_ShouldPassThemThrough()
    {
        // Arrange
        var po1 = CreatePO("P1"); // Valid
        var successfulRecords = new List<PurchaseOrderTracker.PurchaseOrder> { po1 };
        var unprocessableLines = new List<string> { "bad,csv,line,format", "another;bad;line" };
        
        var fileReadResult = new PurchaseOrderTracker.FileReadResult<PurchaseOrderTracker.PurchaseOrder> 
        { 
            SuccessfullRecords = successfulRecords,
            UnprocessableRecords = unprocessableLines
        };

        _mockFileReader.Setup(fr => fr.ReadFile(It.IsAny<string>())).ReturnsAsync(fileReadResult);
        _mockValidationService.Setup(vs => vs.IsRecordValid(po1)).ReturnsAsync((true, true, true));

        // Act
        var result = await PurchaseOrderTracker.PurchaseOrderProcessor.ProcessPurchaseOrders(
            _mockFileReader.Object, _mockValidationService.Object, "any_path.csv");

        // Assert
        Assert.AreEqual(1, result.ValidPurchaseOrders.Count, "Should have 1 valid order.");
        Assert.IsTrue(result.ValidPurchaseOrders.Contains(po1), "PO1 should be valid.");
        Assert.AreEqual(0, result.InValidPurchaseOrders.Count, "Should have 0 business-invalid orders.");
        Assert.AreEqual(2, result.UnprocessablePurchaseOrders.Count, "Should have 2 unprocessable orders.");
        Assert.IsTrue(result.UnprocessablePurchaseOrders.Contains("bad,csv,line,format"));
        Assert.IsTrue(result.UnprocessablePurchaseOrders.Contains("another;bad;line"));
    }

    [TestMethod]
    public async Task ProcessPurchaseOrders_EmptySuccessfulRecords_ShouldReturnEmptyValidAndInvalidLists()
    {
        // Arrange
        var successfulRecords = new List<PurchaseOrderTracker.PurchaseOrder>();
        var fileReadResult = new PurchaseOrderTracker.FileReadResult<PurchaseOrderTracker.PurchaseOrder> { SuccessfullRecords = successfulRecords };
        _mockFileReader.Setup(fr => fr.ReadFile(It.IsAny<string>())).ReturnsAsync(fileReadResult);

        // Act
        var result = await PurchaseOrderTracker.PurchaseOrderProcessor.ProcessPurchaseOrders(
            _mockFileReader.Object, _mockValidationService.Object, "any_path.csv");

        // Assert
        Assert.AreEqual(0, result.ValidPurchaseOrders.Count);
        Assert.AreEqual(0, result.InValidPurchaseOrders.Count);
        Assert.AreEqual(0, result.UnprocessablePurchaseOrders.Count);
    }
}
