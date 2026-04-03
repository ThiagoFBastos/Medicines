using Medicines.Enums;
using Medicines.Interfaces;
using Medicines.Models;
using Medicines.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace MedicinesTests
{
    public class MedicinesServiceTest
    {
        private readonly Mock<IRepositoryManager> _repositoryManager;
        private readonly MedicinesService _mediicinesService;

        public MedicinesServiceTest()
        {
            ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());

            var logger = factory.CreateLogger<MedicinesService>();

            _repositoryManager = new Mock<IRepositoryManager>();

            _mediicinesService = new MedicinesService(_repositoryManager.Object, logger);
        }

        [Fact]
        public async Task AddMedicineAsyncMedicineAlreadyExistsTest()
        {
            Guid id = Guid.NewGuid();
            const string medicineName = "dipirona";
            const long userId = 1;
            const int pillsQuantity = 30;
            DateTimeOffset scheduledTime = DateTimeOffset.UtcNow + new TimeSpan(5, 30, 0);

            var medicine = new Medicine
            {
                Id = id,
                Name = medicineName,
                UserId = userId,
                PillsQuantity = pillsQuantity,
                ScheduledTime = scheduledTime
            };

            var medicineRepository = new Mock<IMedicineRepository>();

            medicineRepository.Setup(m => m.GetMedicineByNameAsync(It.IsAny<string>(), It.IsAny<long>())).ReturnsAsync(medicine);

            _repositoryManager.SetupGet(r => r.MedicineRepository).Returns(medicineRepository.Object);

            var result = await _mediicinesService.AddMedicineAsync(medicineName, pillsQuantity, scheduledTime, userId);

            Assert.False(result.IsSuccess);
            Assert.Equal(EMedicinesStatusCode.MEDICINE_ALREADY_EXISTS, result.Error);
        }

        [Fact]
        public async Task AddMedicineAsyncMedicineNotValidTest()
        {
            const string medicineName = "dipirona $";
            const long userId = -1;
            const int pillsQuantity = -30;
            DateTimeOffset scheduledTime = DateTimeOffset.UtcNow + new TimeSpan(5, 30, 0);

            var medicineRepository = new Mock<IMedicineRepository>();

            medicineRepository.Setup(m => m.GetMedicineByNameAsync(It.IsAny<string>(), It.IsAny<long>())).ReturnsAsync((Medicine?)null);

            _repositoryManager.SetupGet(r => r.MedicineRepository).Returns(medicineRepository.Object);

            var result = await _mediicinesService.AddMedicineAsync(medicineName, pillsQuantity, scheduledTime, userId);

            Assert.False(result.IsSuccess);
            Assert.Equal(EMedicinesStatusCode.MEDICINE_DATA_INVALID, result.Error);
        }

        [Fact]
        public async Task AddMedicineAsyncSuccessTest()
        {
            const string medicineName = "dipirona";
            const long userId = 1;
            const int pillsQuantity = 30;
            DateTimeOffset scheduledTime = DateTimeOffset.UtcNow + new TimeSpan(5, 30, 0);

            var medicineRepository = new Mock<IMedicineRepository>();

            medicineRepository.Setup(m => m.GetMedicineByNameAsync(It.IsAny<string>(), It.IsAny<long>())).ReturnsAsync((Medicine?)null);
            medicineRepository.Setup(m => m.AddMedicine(It.IsAny<Medicine>())).Verifiable();

            _repositoryManager.SetupGet(r => r.MedicineRepository).Returns(medicineRepository.Object);
            _repositoryManager.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask).Verifiable();

            var result = await _mediicinesService.AddMedicineAsync(medicineName, pillsQuantity, scheduledTime, userId);

            Assert.True(result.IsSuccess);

            medicineRepository.VerifyAll();
            _repositoryManager.VerifyAll();
        }

        [Fact]
        public async Task AddMedicineAsyncMedicineAddErrorTest()
        {
            const string medicineName = "dipirona";
            const long userId = 1;
            const int pillsQuantity = 30;
            DateTimeOffset scheduledTime = DateTimeOffset.UtcNow + new TimeSpan(5, 30, 0);

            var medicineRepository = new Mock<IMedicineRepository>();

            medicineRepository.Setup(m => m.GetMedicineByNameAsync(It.IsAny<string>(), It.IsAny<long>())).ReturnsAsync((Medicine?)null);
            medicineRepository.Setup(m => m.AddMedicine(It.IsAny<Medicine>())).Verifiable();

            _repositoryManager.SetupGet(r => r.MedicineRepository).Returns(medicineRepository.Object);
            _repositoryManager.Setup(r => r.SaveAsync()).Throws(new Exception()).Verifiable();

            var result = await _mediicinesService.AddMedicineAsync(medicineName, pillsQuantity, scheduledTime, userId);

            Assert.False(result.IsSuccess);
            Assert.Equal(EMedicinesStatusCode.MEDICINE_ADD_ERROR, result.Error);

            medicineRepository.VerifyAll();
            _repositoryManager.VerifyAll();
        }

        [Fact]
        public async Task AddMedicinePillsAsyncMedicineNotFoundTest()
        {
            const string medicine = "dipirona";
            const long userId = 1;
            const int pillsToAdd = 10;

            var medicineRepository = new Mock<IMedicineRepository>();
            medicineRepository.Setup(m => m.GetMedicineByNameAsync(It.IsAny<string>(), It.IsAny<long>())).ReturnsAsync((Medicine?)null);

            _repositoryManager.SetupGet(r => r.MedicineRepository).Returns(medicineRepository.Object);

            var result = await _mediicinesService.AddMedicinePillsAsync(medicine, pillsToAdd, userId);

            Assert.False(result.IsSuccess);
            Assert.Equal(EMedicinesStatusCode.MEDICINE_NOT_FOUND, result.Error);
        }

        [Fact]
        public async Task AddMedicinePillsAsyncMedicineNotValidTest()
        {
            Guid id = Guid.NewGuid();
            const string medicineName = "dipirona$";
            const long userId = 1;
            const int pillsToAdd = -10;
            var scheduleTime = DateTimeOffset.UtcNow + new TimeSpan(5, 30, 0);

            var medicine = new Medicine
            {
                Id = id,
                Name = medicineName,
                UserId = userId,
                PillsQuantity = pillsToAdd,
                ScheduledTime = scheduleTime
            };

            var medicineRepository = new Mock<IMedicineRepository>();

            medicineRepository.Setup(m => m.GetMedicineByNameAsync(It.IsAny<string>(), It.IsAny<long>())).ReturnsAsync(medicine);

            _repositoryManager.SetupGet(r => r.MedicineRepository).Returns(medicineRepository.Object);

            var result = await _mediicinesService.AddMedicinePillsAsync(medicineName, pillsToAdd, userId);

            Assert.False(result.IsSuccess);
            Assert.Equal(EMedicinesStatusCode.MEDICINE_DATA_INVALID, result.Error);
        }

        [Fact]
        public async Task AddMedicinePillsAsyncSuccessTest()
        {
            Guid id = Guid.NewGuid();
            const string medicineName = "dipirona";
            const long userId = 5;
            const int pillsToAdd = 20;
            var scheduleTime = DateTimeOffset.UtcNow + new TimeSpan(5, 30, 0);

            var medicine = new Medicine
            {
                Id = id,
                Name = medicineName,
                UserId = userId,
                PillsQuantity = pillsToAdd,
                ScheduledTime = scheduleTime
            };

            var medicineRepository = new Mock<IMedicineRepository>();

            medicineRepository.Setup(m => m.GetMedicineByNameAsync(It.IsAny<string>(), It.IsAny<long>())).ReturnsAsync(medicine);
            medicineRepository.Setup(m => m.UpdateMedicine(It.IsAny<Medicine>())).Verifiable();

            _repositoryManager.SetupGet(r => r.MedicineRepository).Returns(medicineRepository.Object);
            _repositoryManager.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask).Verifiable();

            var result = await _mediicinesService.AddMedicinePillsAsync(medicineName, pillsToAdd, userId);

            Assert.True(result.IsSuccess);

            medicineRepository.VerifyAll();
            _repositoryManager.VerifyAll();
        }

        [Fact]
        public async Task AddMedicinePillsAsyncMedicineUpdateErrorTest()
        {
            Guid id = Guid.NewGuid();
            const string medicineName = "dipirona";
            const long userId = 5;
            const int pillsToAdd = 20;
            var scheduleTime = DateTimeOffset.UtcNow + new TimeSpan(5, 30, 0);

            var medicine = new Medicine
            {
                Id = id,
                Name = medicineName,
                UserId = userId,
                PillsQuantity = pillsToAdd,
                ScheduledTime = scheduleTime
            };

            var medicineRepository = new Mock<IMedicineRepository>();
            medicineRepository.Setup(m => m.GetMedicineByNameAsync(It.IsAny<string>(), It.IsAny<long>())).ReturnsAsync(medicine);
            medicineRepository.Setup(m => m.UpdateMedicine(It.IsAny<Medicine>())).Verifiable();

            _repositoryManager.SetupGet(r => r.MedicineRepository).Returns(medicineRepository.Object);
            _repositoryManager.Setup(r => r.SaveAsync()).Throws(new Exception()).Verifiable();

            var result = await _mediicinesService.AddMedicinePillsAsync(medicineName, pillsToAdd, userId);

            Assert.False(result.IsSuccess);
            Assert.Equal(EMedicinesStatusCode.MEDICINE_UPDATE_ERROR, result.Error);

            medicineRepository.VerifyAll();
            _repositoryManager.VerifyAll();
        }
    }
}
